using Newtonsoft.Json.Linq;
using Pastel;
using System.Net.Http;

namespace WorkshopChecker
{
    internal class Program
    {
        static readonly string _path = $"{Directory.GetCurrentDirectory()}\\workshops.txt";
        private static readonly HttpClient client = new HttpClient();
        private static string _workshop = "";
        private static int _workshopCount = 0;
        private static int _vaildworkshopCount = 0;
        private static int _invailedworkshopCount = 0;

        static void Main(string[] args)
        {
            if(hasFile())
            {
                _workshop = File.ReadAllText(_path);

                new Program().MainAsync().GetAwaiter().GetResult();

            }
            else
            {
                Console.WriteLine("The workshops.txt file does not exist.\r\n\r\nCreate an example file.");
                sampleCreateFile();
                Main(args);
            }
        }

        private async Task MainAsync()
        {
            var tasks = new List<Task>();

            tasks.Add(Task.Run(async () =>
            {
                string[] ids = _workshop.Split(';');
                _workshopCount = ids.Count();
                foreach (string id in ids)
                {
                    await GetWorkshopInfo(id);
                }
                Console.WriteLine("END");
            }));

            await Task.WhenAll(tasks);

            await Task.Delay(-1);
        }
        private static async Task GetWorkshopInfo(string id)
        {
            var a = new Dictionary<string, string>
            {
                {
                    "itemcount", "1"
                },
                {
                    "publishedfileids[0]", $"{id}"
                }
            };

            var r = await PostHTTPRequestAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", a);
            var rr = await GetHTTPRequestAsync($"https://steamcommunity.com/sharedfiles/filedetails/?id={id}");
            var issue = "";
            int startIndex = rr.IndexOf("<title>") + "<title>".Length;
            int endIndex = rr.IndexOf("</title>");

            // 중간 내용 추출
            string middleContent = rr.Substring(startIndex, endIndex - startIndex);

            var json = JObject.Parse(r);
            if(json != null)
            {
                var details = json["response"]["publishedfiledetails"][0];

                var title = details?["title"]?.ToString();

                if (string.IsNullOrEmpty(title))
                    title = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";

                string code = "";
                if (details["result"].ToString() != "1")
                    code = "[ Invalid | Check URL ]".Pastel("FF2100");
                else
                    code = "[ Valid ]".Pastel("00FFFF");
                middleContent = middleContent.Replace("Steam Community :: ", "");
                if (middleContent.Contains("Error"))
                {
                    title = "Error";
                    issue = "There is no workshop page.";
                    Console.WriteLine($"{code} WorkshopID: {id} | Workshop Title: {title} | Issue: {issue}".PastelBg("FF7800"));
                    _invailedworkshopCount++;
                }
                else
                {
                    issue = "None";
                    Console.WriteLine($"{code} WorkshopID: {id} | Workshop Title: {title} | Issue: {issue}");
                    _vaildworkshopCount++;
                }

                Console.Title = $"Workshops {_workshopCount} | Vaild {_vaildworkshopCount} | Invaild {_invailedworkshopCount}";
            }
            else
            {
                Console.WriteLine("Steam API Not Response !");
            }
            
        }
        private static async Task<string> PostHTTPRequestAsync(string url, Dictionary<string, string> data)
        {
            using (HttpContent formContent = new FormUrlEncodedContent(data))
            {
                using (HttpResponseMessage response = await client.PostAsync(url, formContent).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        private static async Task<string> GetHTTPRequestAsync(string url)
        {
            using (HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
        static bool hasFile()
        {
            return File.Exists(_path);
        }

        static void sampleCreateFile()
        {
            File.WriteAllText(_path, "3044705007;2883755057;2899612344;2016648690");
        }
    }
}
