using Newtonsoft.Json.Linq;

namespace WorkshopChecker
{
    internal class Program
    {
        static readonly string _path = $"{Directory.GetCurrentDirectory()}\\workshops.txt";
        private static readonly HttpClient client = new HttpClient();
        private static string _workshop = "";
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
                foreach (string id in ids)
                {
                    await GetWorkshopInfo(id);
                }
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

            var json = JObject.Parse(r);
            if(json != null)
            {
                var details = json["response"]["publishedfiledetails"][0];

                Console.WriteLine($"{details["result"]} | {id} | {details["title"]}");
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
        static bool hasFile()
        {
            return File.Exists(_path);
        }

        static void sampleCreateFile()
        {
            File.WriteAllText(_path, "3044705007;2883755057;2899612344");
        }
    }
}
