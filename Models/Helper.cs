using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;

namespace Crawl_videos_group_facebook.Models {
    public static class Helper {
        public static string GetHTML (string url) {
            var client = new RestClient (url);
            var request = new RestRequest (Method.GET);
            request.AddHeader ("cookie", Environment.cookie);
            IRestResponse response = client.Execute (request);

            return response.Content.Replace ("\\", "");
        }

        public static (string ajaxpipe_token, string async_get_token) GetToken (string html) {
            var pattern = "ajaxpipe_token\":\"(?<ajaxpipe_token>.*?)\"|async_get_token\":\"(?<async_get_token>.*?)\"";
            var regex = Regex.Matches (html, pattern, RegexOptions.Singleline);

            return (regex[0].Groups["ajaxpipe_token"].Value, regex[1].Groups["async_get_token"].Value);
        }

        public static string GetScroll_load (string html) {
            var pattern = "GroupPhotosetPagelet\",({\"scroll_load\"|{scroll_load:)(.*?),{";
            var groupPhotosetPagelet = Regex.Match (html, pattern, RegexOptions.Singleline).Value;
            var scroll_load = Regex.Match (groupPhotosetPagelet, ",(?<scroll_load>.*?),{", RegexOptions.Singleline).Groups["scroll_load"].Value;
            
            return scroll_load;
        }

        public static void DownloadVideo (string urlPost) {
            try {
                var html = GetHTML (urlPost);
                var downloadTasks = Regex.Matches (html, "hd_src:\"(?<src>.*?)\"", RegexOptions.Singleline)
                    .Select (i => Task.Run (() => {
                        var src = i.Groups["src"].Value;
                        if (string.IsNullOrEmpty (src)) src = Regex.Match (html, "sd_src:\"(?<src>.*?)\"", RegexOptions.Singleline).Groups["src"].Value;
                        var filename = GetFilename (src);
                        var filePath = $"{Environment.videosPath}/{filename}";
                        if (File.Exists (filePath)) Console.WriteLine ($"Skip: {urlPost}");
                        else {
                            Console.WriteLine ($"Download: {filename}");
                            new WebClient ().DownloadFile (src, filePath);
                        }
                    })).ToArray ();
                Task.WaitAll (downloadTasks);
            } catch {
                Console.WriteLine ($"Error: {urlPost}");
            }
        }

        public static string GetFilename (string src) {
            var start = src.LastIndexOf ("/") + 1;
            var end = src.IndexOf ("?");

            return src.Substring (start, end - start);
        }
    }
}