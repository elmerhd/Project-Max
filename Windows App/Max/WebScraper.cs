using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Max
{
    public class WebScraper
    {

        public class YourDictionary
        {

            public YourDictionary()
            {

            }

            public IList<HtmlNode> getResults(string query)
            {
                var url = "https://www.yourdictionary.com/" + query;
                var web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                IList<HtmlNode> nodes = doc.QuerySelectorAll("div.single-definition-box > div.definition-wrapper > div.relative  > div.definition > div");
                return nodes;
            }
        }
    }
}
