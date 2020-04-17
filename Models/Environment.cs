using System.IO;
using System.Text.RegularExpressions;

namespace Crawl_videos_group_facebook.Models {
    public static class Environment {
        public static string dataPath { get => "./data"; }
        public static string cookie {
            get {
                try {
                    return File.ReadAllText ($"{dataPath}/cookie.txt");
                } catch { return null; }
            }
            set => File.WriteAllText ($"{dataPath}/cookie.txt", value);
        }
        public static string group { get; set; }

        public static string groupPath => $"{Models.Environment.dataPath}/{group}";
        public static string videosPath => $"{groupPath}/videos";
        public static string userid {
            get => Regex.Match (cookie, "c_user=(?<id>.*?);", RegexOptions.Singleline).Groups["id"].Value;
        }
        public static string spin_r {
            get => Regex.Match (cookie, @"spin=(.*?)r\.(?<r>.*?)\.", RegexOptions.Singleline).Groups["r"].Value;
        }
        public static string spin_b {
            get => Regex.Match (cookie, @"spin=(.*?)b\.(?<b>.*?)_", RegexOptions.Singleline).Groups["b"].Value;
        }
        public static string spin_t {
            get => Regex.Match (cookie, @"spin=(.*?)t\.(?<t>.*?)_", RegexOptions.Singleline).Groups["t"].Value;
        }
    }
}