using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public List<PlayableAccount> PlayableAccounts =>
            new()
            {
                new(AccountType.Alice, _portraitAlice, false, "Alice"),
                new(AccountType.Bob, _portraitBob, false, "Bob"),
                new(AccountType.Charlie, _portraitCharlie, false, "Charlie"),
                new(AccountType.Dave, _portraitDave, false, "Dave"),
                new(AccountType.Custom, _portraitCustom, true, string.Empty),
            };

        public AccountManager()
        {
            //_portraitAlice = Resources.Load<Texture2D>($"Images/alice_portrait");
            //_portraitBob = Resources.Load<Texture2D>($"Images/bob_portrait");
            //_portraitCharlie = Resources.Load<Texture2D>($"Images/charlie_portrait");
            //_portraitDave = Resources.Load<Texture2D>($"Images/dave_portrait");
            //_portraitCustom = Resources.Load<Texture2D>($"Images/custom_account_portrait");
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

        public PlayableAccount findNext(AccountType current)
        {
            var idx = PlayableAccounts.FindIndex(x => x.accountType == current);

            if (idx + 1 >= PlayableAccounts.Count) return PlayableAccounts[idx];
            return PlayableAccounts[idx + 1];
        }

        public PlayableAccount findPrevious(AccountType current)
        {
            var idx = PlayableAccounts.FindIndex(x => x.accountType == current);

            if (idx - 1 < 0) return PlayableAccounts[idx];
            return PlayableAccounts[idx - 1];
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
