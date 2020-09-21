using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public class Util
    {
        private static Dictionary<string, string>? StringTable { get; set; }

        public static void LoadStringTable(string path)
        {
            StringTable = File.ReadAllText(path)
                .Split('\n')
                .Select(x => x.Split(','))
                .ToDictionary(x => x[0], x => x[1]);
        }

        public static string? GetString(string key)
        {
            return StringTable?[key];
        }
    }
}
