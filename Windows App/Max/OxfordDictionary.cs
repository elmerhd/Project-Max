using Newtonsoft.Json;
using OxfordDictionariesAPI;
using OxfordDictionariesAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Max
{
    public class OxfordDictionary
    {
        private OxfordDictionaryClient _client = new OxfordDictionaryClient("8c7ac9ce", "7864f3b678f0c09324782e2010233846");

        public SearchResult GetResult(string word)
        {
            SearchResult searchResult = _client.SearchEntries(word, CancellationToken.None).Result;
            return searchResult;
        }
    }
}
