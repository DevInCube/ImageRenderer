using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VitML.ImageRenderer.Storages
{
    public class FTPItem
    {

        public string Name;
        public bool IsDirectory;
        public string[] Permissions;
        public long Size;
        public DateTime LastModified;

        public FTPItem()
        {
            Name = null;
            IsDirectory = false;
            Permissions = new string[3];
            Size = 0;
            LastModified = DateTime.Now;
        }

        public static FTPItem Parse(string str)
        {
            Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            MatchCollection matches = regex.Matches(str);
            GroupCollection g = matches[0].Groups;
            FTPItem item = new FTPItem();
            item.IsDirectory = g[1].Value.Equals("d");
            item.LastModified = DateTime.Parse(g[4].Value + g[5].Value);
            item.Name = g[6].Value;
            return item;
        }
    }
}
