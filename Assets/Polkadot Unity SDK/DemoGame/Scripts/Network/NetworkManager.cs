using Substrate.Integration;
using Substrate.Integration.Client;
using Substrate.Integration.Helper;
using Substrate.NET.Schnorrkel.Keys;
using Substrate.NET.Wallet;
using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public enum AccountType
    {
        Alice,
        Bob,
        Charlie,
        Dave,
        Custom
    }

    public enum NodeType
    {
        Local,
        Solo,
        Tanssi
    }

    public delegate void ExtrinsicStateUpdate(string subscriptionId, ExtrinsicStatus extrinsicUpdate);

    public class NetworkManager : Singleton<NetworkManager>
    {
        public delegate void ConnectionStateChangedHandler(bool IsConnected);

        public delegate void ExtrinsicCheckHandler();

        public event ConnectionStateChangedHandler ConnectionStateChanged;

        public event ExtrinsicCheckHandler ExtrinsicCheck;

        public MiniSecret MiniSecretAlice => new MiniSecret(Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"), ExpandMode.Ed25519);
        public Account SudoAlice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToEd25519Bytes(), MiniSecretAlice.GetPair().Public.Key);

        public MiniSecret MiniSecretSudo => new MiniSecret(Utils.HexToByteArray(""), ExpandMode.Ed25519);
        public Account SudoHexalem => Account.Build(KeyType.Sr25519, MiniSecretSudo.ExpandToSecret().ToEd25519Bytes(), MiniSecretSudo.GetPair().Public.Key);

        // Sudo account if needed
        public Account Sudo { get; private set; }

        private string _nodeUrl;
        public string NodeUrl => _nodeUrl;

        private readonly NetworkType _networkType = NetworkType.Live;

        public Keyring Keyring { get; private set; }

        public string CurrentAccountName => Wallet != null ? Wallet.Meta.Name : "Unknown";

        public NodeType CurrentNodeType { get; private set; }

        private SubstrateNetwork _client;
        public SubstrateNetwork Client => _client;

        private bool? _lastConnectionState = null;

        public Wallet Wallet { get; private set; }

        private int _walletIndex;

        protected override void Awake()
        {
            base.Awake();
            //Your code goes here
            CurrentNodeType = NodeType.Local;
            Sudo = SudoAlice;
            _nodeUrl = "ws://127.0.0.1:9944";

            InitializeClient();

            Keyring = new Keyring();

            var wallets = StoredWallets();
            wallets.ForEach(p => Keyring.AddWallet(p));

            Keyring.AddFromUri("//Alice", new Meta() { Name = "Alice" }, KeyType.Sr25519);
            Keyring.AddFromUri("//Bob", new Meta() { Name = "Bob" }, KeyType.Sr25519);
            Keyring.AddFromUri("//Charlie", new Meta() { Name = "Charlie" }, KeyType.Sr25519);
            Keyring.AddFromUri("//Dave", new Meta() { Name = "Dave" }, KeyType.Sr25519);

            _walletIndex = 0;
            if (PlayerPrefs.HasKey("NetworkManager.WalletRef"))
            {
                var playerPrefsWallet = PlayerPrefs.GetString("NetworkManager.WalletRef");
                _walletIndex = wallets.IndexOf(wallets.First(p => p.Meta.Name == playerPrefsWallet));
            }

            SetWallet(Keyring.Wallets[_walletIndex]);
        }

        public void Start()
        {
            InvokeRepeating(nameof(UpdateNetworkState), 0.0f, 2.0f);
            InvokeRepeating(nameof(UpdatedExtrinsic), 0.0f, 3.0f);
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(UpdateNetworkState));
            CancelInvoke(nameof(UpdatedExtrinsic));
        }

        private void UpdateNetworkState()
        {
            if (_client == null)
            {
                return;
            }

            var connectionState = _client.IsConnected;
            if (_lastConnectionState == null || _lastConnectionState != connectionState)
            {
                ConnectionStateChanged?.Invoke(connectionState);
                _lastConnectionState = connectionState;
            }
        }

        private void UpdatedExtrinsic()
        {
            ExtrinsicCheck?.Invoke();
        }

        public bool NextWallet()
        {
            if (Keyring.Wallets.Count <= 0 || Keyring.Wallets.Count - 1 <= _walletIndex)
            {
                return false;
            }

            SetWallet(Keyring.Wallets[++_walletIndex]);
            return true;
        }

        public void PrevWallet()
        {
            if (_walletIndex == 0)
            {
                return;
            }

            SetWallet(Keyring.Wallets[--_walletIndex]);
        }

        public (Account, string) GetAccount(AccountType accountType, string custom = null)
        {
            Account result;
            string name;
            switch (accountType)
            {
                case AccountType.Alice:
                case AccountType.Bob:
                case AccountType.Charlie:
                case AccountType.Dave:
                    name = accountType.ToString();
                    result = BaseClient.RandomAccount(GameConstant.AccountSeed, accountType.ToString(), KeyType.Sr25519);
                    break;

                case AccountType.Custom:
                    name = Wallet.Meta.Name.ToUpper();
                    result = Wallet.Account;
                    break;

                default:
                    name = AccountType.Alice.ToString();
                    result = BaseClient.RandomAccount(GameConstant.AccountSeed, AccountType.Alice.ToString(), KeyType.Sr25519);
                    break;
            }

            return (result, name);
        }

        public void SetWallet(Wallet wallet)
        {
            Client.Account = wallet.Account;
            Wallet = wallet;
        }

        #region Wallet

        public bool ChangeWallet(Wallet wallet)
        {
            Debug.Log($"Loading {wallet.FileName} wallet with account {wallet.Account}");

            SetWallet(wallet);

            // save if we change wallet to a new one
            if (PlayerPrefs.GetString("NetworkManager.WalletRef") != wallet.FileName)
            {
                PlayerPrefs.SetString("NetworkManager.WalletRef", Wallet.FileName);
                PlayerPrefs.Save();
            }

            return true;
        }

        public List<Wallet> StoredWallets()
        {
            var result = new List<Wallet>();
            foreach (var w in WalletFiles())
            {
                if (!Wallet.TryLoad(w, out Wallet wallet))
                {
                    Debug.Log($"Failed to load wallet {w}");
                }

                result.Add(wallet);
            }
            return result;
        }

        private IEnumerable<string> WalletFiles()
        {
            var d = new DirectoryInfo(CachingManager.GetInstance().PersistentPath);
            return d.GetFiles(Wallet.ConcatWalletFileType("*")).Select(p => Path.GetFileNameWithoutExtension(p.Name));
        }

        #endregion Wallet

        public bool ToggleNodeType()
        {
            switch (CurrentNodeType)
            {
                case NodeType.Local:
                    CurrentNodeType = NodeType.Solo;
                    _nodeUrl = "wss://hexalem-rpc.substrategaming.org";
                    Sudo = SudoAlice;
                    break;

                case NodeType.Solo:
                    CurrentNodeType = NodeType.Tanssi;
                    _nodeUrl = "wss://fraa-dancebox-3023-rpc.a.dancebox.tanssi.network";
                    Sudo = SudoHexalem;
                    break;

                case NodeType.Tanssi:
                    CurrentNodeType = NodeType.Local;
                    _nodeUrl = "ws://127.0.0.1:9944";
                    Sudo = SudoAlice;
                    break;
            }

            InitializeClient();
            return true;
        }

        // Start is called before the first frame update
        public void InitializeClient()
        {
            _client = new SubstrateNetwork(null, _networkType, _nodeUrl);
        }
    }
}