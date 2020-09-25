using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
    public class Util
    {
        private static Dictionary<string, string?> StringTable { get; set; } = default!;

        public static void LoadStringTable(string path)
        {
            StringTable = File.ReadAllText(path)
                .Split('\n')
                .Select(x => x.Split(','))
                .ToDictionary(x => x[0], x => (x.Length == 2) ? x[1] : null);
        }

        public static string? GetString(string key)
        {
            if (StringTable.TryGetValue(key, out var str))
            {
                return str;
            }
            else
            {
                return default;
            }
        }
    }
}
