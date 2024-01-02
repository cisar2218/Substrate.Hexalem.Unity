using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class AccountManager : Singleton<AccountManager>
    {
        public bool IsAccountNameValid(string accountName)
        {
            if (string.IsNullOrEmpty(accountName)) return false;

            if (accountName.Length > 20) return false;

            if (!Regex.IsMatch(accountName, "^[a-zA-Z0-9]*$")) return false;

            return true;
        }
    }
}
