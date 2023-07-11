using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Foxconn.Licensing
{
    public static class ComputerID
    {
        private static RegistryKey OpenWPAKey() => Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("WPA");

        public static WpaID GetWpaID()
        {
            RegistryKey registryKey = OpenWPAKey();
            List<string> stringList = new List<string>();
            Regex regex = new Regex("^8DEC0AF1-0341-4b93-85CD-72606C2DF94C-[2|5]P-[0-9A-F]$", RegexOptions.IgnoreCase);
            foreach (string subKeyName in registryKey.GetSubKeyNames())
            {
                if (regex.IsMatch(subKeyName))
                    stringList.Add(subKeyName);
            }
            stringList.Sort();
            WpaID wpaId = new WpaID();
            wpaId.Components = new WpaID.Component[stringList.Count];
            for (int index = 0; index < stringList.Count; ++index)
            {
                string name = stringList[index];
                if (!(registryKey.OpenSubKey(name).GetValue(null) is byte[] numArray))
                    throw new InvalidOperationException();
                wpaId.Components[index] = new WpaID.Component()
                {
                    Key = name,
                    Value = numArray
                };
            }
            return wpaId;
        }
    }
}
