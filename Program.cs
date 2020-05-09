using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Crawl_videos_group_facebook {
    class Program {
        #region Properties
        private static string dataPath { get => Env.dataPath; }
        private static string cookie {
            get => Env.cookie;
            set => Env.cookie = value;
        }
        private static string group {
            get => Env.group;
            set => Env.group = value;
        }

        private static string groupPath { get => Env.groupPath; }
        private static string videosPath { get => Env.videosPath; }
        private static string userid { get => Env.userid; }
        private static string spin_r { get => Env.spin_r; }
        public static string spin_b { get => Env.spin_b; }
        public static string spin_t { get => Env.spin_t; }
        private static string ajaxpipe_token;
        private static string async_get_token;
        #endregion

        static void Main (string[] args) {
            if (!Directory.Exists (dataPath))
                Directory.CreateDirectory (dataPath);

            if (string.IsNullOrEmpty (cookie)) {
                Console.Write ("Cookie: ");
                cookie = Console.ReadLine ();
            }

            while (true) {
                Console.Write ("> ");
                var command = Console.ReadLine ().Split (' ');
                try {
                    switch (command[0]) {
                        case "download":
                            Console.WriteLine ($"Download videos group: {command[1]}...");
                            DownloadVideos (command[1]);
                            break;
                        case "update":
                            Directory.GetDirectories (dataPath).ToList ().ForEach (dir => {
                                var groupDir = dir.Substring (dir.LastIndexOf ("/") + 1);
                                Console.WriteLine ($"Update videos group: {groupDir}...");
                                DownloadVideos (group);
                            });
                            break;
                        case "open":
                            if (command.Length == 1) {
                                Directory.GetDirectories (dataPath).ToList ().ForEach (dir => {
                                    var groupDir = dir.Substring (dir.LastIndexOf ("/") + 1);
                                    Console.WriteLine (groupDir);
                                });
                                break;
                            }

                            if (!Directory.Exists ($"{dataPath}/{command[1]}"))
                                throw new Exception ("Directory not exist!");

                            OpenFolder ($"{dataPath}/{command[1]}/videos");
                            break;
                        default:
                            Console.WriteLine ("unknown!");
                            break;
                    }
                } catch (Exception e) {
                    Console.WriteLine ($"Error: {e.Message}");
                }
            }
        }

        private static void OpenFolder (string path) {
            Process.Start ("zsh", $"-c \"xdg-open {path}\"");
        }
        private static void DownloadVideos (string group) {
            Program.group = group;
            if (!Directory.Exists (groupPath)) Directory.CreateDirectory (groupPath);
            if (!Directory.Exists (videosPath)) Directory.CreateDirectory (videosPath);

            var url = $"https://www.facebook.com/groups/{group}/videos";
            var html = Helper.GetHTML (url);

            (ajaxpipe_token, async_get_token) = Helper.GetToken (html);

            Download: ;
            // Regex.Matches (html, "href=\"(?<urlPost>.*?)\" rel=\"async\"")
            //     .Select (i => $"https://fb.com{i.Groups["urlPost"].Value}").ToList ()
            //     .ForEach (urlPost => {
            //         Console.WriteLine ($"Enter post: {urlPost}");
            //         Helper.DownloadVideo (urlPost);
            //     });

            var urlPosts = Regex.Matches (html, "href=\"(?<urlPost>.*?)\" rel=\"async\"")
                .Select (i => $"https://fb.com{i.Groups["urlPost"].Value}").ToList ();

            Helper.DownloadVideos (urlPosts);

            var scroll_load = Helper.GetScroll_load (html);
            if (string.IsNullOrEmpty (scroll_load)) File.WriteAllText ("html.txt", html);
            else {
                url = $"https://www.facebook.com/ajax/pagelet/generic.php/GroupPhotosetPagelet?fb_dtsg_ag={async_get_token}&ajaxpipe=1&ajaxpipe_token={ajaxpipe_token}&no_script_path=1&data={WebUtility.UrlEncode(scroll_load)}&__user={userid}&__a=1&__csr=&__req=fetchstream_1&__beoa=0&__pc=PHASED:DEFAULT&dpr=1&__s=nvzl6z:zb6vnm:tueia0&__comet_req=0&jazoest=27866&__spin_r={spin_r}&__spin_b={spin_b}&__spin_t={spin_t}&__adt=1&ajaxpipe_fetch_stream=1";
                html = Helper.GetHTML (url);
                goto Download;
            }
        }
    }
}