using Substrate.Integration;
using Substrate.Integration.Helper;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class FaucetManager : Singleton<FaucetManager>
    {
        internal NetworkManager Network => NetworkManager.GetInstance();

        public async Task GetTokensAsync(CancellationToken token)
        {
            var accountInfo = await Network.Client.GetAccountAsync(CancellationToken.None);

            if (accountInfo != null && accountInfo.Data.Free != BigInteger.Zero)
            {
                Debug.Log($"[{nameof(NetworkManager)})] Current account has {accountInfo.Data.Free} tokens");
            }
            else
            {
                Debug.Log($"[{nameof(NetworkManager)})] Current account has no token");
                var targetAccount = Network.Client.Account;

                // Ugly hack sorry, assume that Sudo account has money...
                Network.Client.Account = Network.Sudo;
                var amountToTransfer = new BigInteger(1000 * SubstrateNetwork.DECIMALS);

                Debug.Log($"[{nameof(NetworkManager)})] Send {amountToTransfer} from {Network.Client.Account.Bytes.ToAccountId32().ToAddress()} to {targetAccount.ToAccountId32().ToAddress()}");
                await Network.Client.TransferKeepAliveAsync(targetAccount.ToAccountId32(), amountToTransfer, 5, token);

                Network.Client.Account = targetAccount;
            }
        }
    }
}
