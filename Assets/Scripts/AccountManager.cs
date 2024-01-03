using Substrate.Integration.Helper;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class AccountManager : Singleton<AccountManager>
    {
        private Texture2D _portraitAlice => Resources.Load<Texture2D>($"Images/alice_portrait");
        private Texture2D _portraitBob => Resources.Load<Texture2D>($"Images/bob_portrait");
        private Texture2D _portraitCharlie => Resources.Load<Texture2D>($"Images/charlie_portrait");
        private Texture2D _portraitDave => Resources.Load<Texture2D>($"Images/dave_portrait");
        private Texture2D _portraitCustom => Resources.Load<Texture2D>($"Images/custom_account_portrait");

        public static AccountType DefaultAccountType = AccountType.Alice;

        private List<PlayableAccount> _availableAccounts = null;

        public List<PlayableAccount> PlayableAccounts =>
            new()
            {
                new(AccountType.Alice, _portraitAlice, false, "Alice"),
                new(AccountType.Bob, _portraitBob, false, "Bob"),
                new(AccountType.Charlie, _portraitCharlie, false, "Charlie"),
                new(AccountType.Dave, _portraitDave, false, "Dave"),
                new(AccountType.Custom, _portraitCustom, true, string.Empty),
            };

        public void ClearAvailableAccounts()
        {
            _availableAccounts = null;
        }

        public async Task<List<PlayableAccount>> GetAvailableFriendAccountsAsync(CancellationToken token)
        {
            Debug.Log($"[{nameof(AccountManager)} | {nameof(GetAvailableFriendAccountsAsync)}] called");

            if (_availableAccounts != null) return _availableAccounts;

            Debug.Log($"[{nameof(AccountManager)} | {nameof(GetAvailableFriendAccountsAsync)}] Need to update available player list");

            _availableAccounts = new List<PlayableAccount>();

            foreach(var account in PlayableAccounts.Where(x => !x.isCustom))
            {
                var accountInstance = NetworkManager.GetInstance().GetAccount(account.accountType);
                var gameInstance = await NetworkManager.GetInstance().Client.GetBoardAsync(accountInstance.ToAccountId32().ToAddress(), token);
                
                if(NetworkManager.GetInstance().CurrentAccountType != account.accountType && gameInstance == null)
                {
                    _availableAccounts.Add(account);
                }
            }

            _availableAccounts.AddRange(PlayableAccounts.Where(x => x.isCustom));

            Debug.Log($"[{nameof(AccountManager)} | {nameof(GetAvailableFriendAccountsAsync)}] Available accounts : {string.Join(",", _availableAccounts.Select(x => x.name))}");

            return _availableAccounts;
        }

        public AccountManager()
        {
        }

        public Texture2D GetPortrait(AccountType accountType)
        {
            switch(accountType)
            {
                case AccountType.Alice: return _portraitAlice;
                case AccountType.Bob: return _portraitBob;
                case AccountType.Charlie: return _portraitCharlie;
                case AccountType.Dave: return _portraitDave;
                case AccountType.Custom: return _portraitCustom;
            }

            throw new InvalidOperationException($"AccountType {accountType} does not have portrait");
        }

        public PlayableAccount findNextPlayableAccount(AccountType current) => findNext(current, PlayableAccounts);

        public PlayableAccount findPreviousPlayableAccount(AccountType current) => findPrevious(current, PlayableAccounts);

        public async Task<PlayableAccount> findNextAvailableAccountAsync(AccountType current, CancellationToken token) 
            => findNext(current, await GetAvailableFriendAccountsAsync(token));

        public async Task<PlayableAccount> findPreviousAvailableAccountAsync(AccountType current, CancellationToken token) 
            => findPrevious(current, await GetAvailableFriendAccountsAsync(token));

        private PlayableAccount findNext(AccountType current, List<PlayableAccount> accounts)
        {
            var idx = accounts.FindIndex(x => x.accountType == current);

            if (idx + 1 >= accounts.Count) return accounts[idx];
            return accounts[idx + 1];
        }

        private PlayableAccount findPrevious(AccountType current, List<PlayableAccount> accounts)
        {
            var idx = accounts.FindIndex(x => x.accountType == current);
            Debug.Log($"[XXXX] current = {current} | idx = {idx} | accounts = {string.Join(",", accounts.Select(x => x.accountType))}");

            if (idx - 1 < 0) return accounts[idx];
            return accounts[idx - 1];
        }

        public bool IsAccountNameValid(string accountName)
        {
            if (string.IsNullOrEmpty(accountName)) return false;

            if (accountName.Length > 20) return false;

            if (!Regex.IsMatch(accountName, "^[a-zA-Z0-9]*$")) return false;

            return true;
        }
    }

    public struct PlayableAccount
    {
        public AccountType accountType;
        public string name;
        public Texture2D portrait;
        public bool isCustom;

        public PlayableAccount(AccountType accountType, Texture2D portrait, bool isCustom, string name)
        {
            this.accountType = accountType;
            this.portrait = portrait;
            this.isCustom = isCustom;
            this.name = name;
        }
    }
}
