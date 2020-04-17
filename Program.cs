using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawl_videos_group_facebook {
    class Program {
        #region Properties
        private static string dataPath { get => Environment.dataPath; }
        private static string cookie {
            get => Environment.cookie;
            set => Environment.cookie = value;
        }
        private static string group {
            get => Environment.group;
            set {
                Environment.group = value;
                if (!Directory.Exists (groupPath)) Directory.CreateDirectory (groupPath);
                if (!Directory.Exists (videosPath)) Directory.CreateDirectory (videosPath);
            }
        }

        private static string groupPath { get => Environment.groupPath; }
        private static string videosPath { get => Environment.videosPath; }
        private static string userid { get => Environment.userid; }
        private static string spin_r { get => Environment.spin_r; }
        public static string spin_b { get => Environment.spin_b; }
        public static string spin_t { get => Environment.spin_t; }
        private static string ajaxpipe_token;
        private static string async_get_token;
        #endregion

        static void Main (string[] args) {
            if (!Directory.Exists (dataPath))
                Directory.CreateDirectory (dataPath);

            //Console.Write ("Group: ");
            //group = Console.ReadLine ();
            group = "gaixinhchonloc";
            if (string.IsNullOrEmpty (cookie)) {
                Console.Write ("Cookie: ");
                cookie = Console.ReadLine ();
            }

            var url = $"https://www.facebook.com/groups/{group}/videos";
            var html = Helper.GetHTML (url);

            (ajaxpipe_token, async_get_token) = Helper.GetToken (html);

            Download: ;
            var downloadTasks = Regex.Matches (html, "href=\"(?<urlPost>.*?)\" rel=\"async\"")
                .Select (i => {
                    return Task.Run (() => {
                        var urlPost = $"https://fb.com{i.Groups["urlPost"].Value}";
                        Console.WriteLine ($"Enter post: {urlPost}");
                        Helper.DownloadVideo (urlPost);
                    });
                }).ToArray ();
            Task.WaitAll (downloadTasks);

            var scroll_load = Helper.GetScroll_load (html);
            url = $"https://www.facebook.com/ajax/pagelet/generic.php/GroupPhotosetPagelet?fb_dtsg_ag={async_get_token}&ajaxpipe=1&ajaxpipe_token={ajaxpipe_token}&no_script_path=1&data={WebUtility.UrlEncode(scroll_load)}&__user={userid}&__a=1&__csr=&__req=fetchstream_1&__beoa=0&__pc=PHASED:DEFAULT&dpr=1&__s=nvzl6z:zb6vnm:tueia0&__comet_req=0&jazoest=27866&__spin_r={spin_r}&__spin_b={spin_b}&__spin_t={spin_t}&__adt=1&ajaxpipe_fetch_stream=1";
            html = Helper.GetHTML (url);

            goto Download;
        }
    }
}