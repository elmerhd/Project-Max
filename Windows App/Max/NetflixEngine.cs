using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using HtmlAgilityPack;

namespace Max
{
    public class NetflixEngine
    {
        private MaxEngine MaxEngine;
        private Process Process;
        private NetflixSearch NetflixSearch;

        public NetflixEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            maxEngine.BrainEngine.Log($"Loading {nameof(NetflixEngine)}");
        }

        public void Play(string id)
        {
            System.Console.WriteLine(id);
            Process = new Process();
            Process.StartInfo.FileName = $"netflix:/app?playVideoId={id}";
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Process.Start();
        }

        public string Search(string title)
        {
            NetflixSearch = new NetflixSearch($"{title} netflix");
            return NetflixSearch.GetNetflixId(); ;
        }
    }

    public class NetflixSearch
    {
        private const string protocol = "https://";
        private string url = null;
        private string filename = null;
        public NetflixSearch(string text)
        {
            url = $"{protocol}www.google.com/search?q={text}";
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            filename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".html";
            doc.Save(filename);
        }
        public string GetNetflixId()
        {
            string id = null;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filename);
            HtmlNode[] nodes = doc.DocumentNode.SelectNodes("//a[@class=\"yuRUbf\"]").ToArray();
            foreach (HtmlNode item in nodes)
            {
                string dataUrl = item.Attributes["href"].Value;
                if (dataUrl.Contains("www.netflix.com"))
                {
                    id = dataUrl.Substring(dataUrl.LastIndexOf("/") + 1);
                }
            }
            return id;
        }
    }
}
