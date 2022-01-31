using System;
using System.Xml;
using System.Text;
using System.IO;
using Max;
using System.Collections.Generic;
using OxfordDictionariesAPI.Models;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;
using HtmlAgilityPack;

namespace AIMLBot.AIMLTagHandlers
{
    /// <summary>
    /// The learn element instructs the AIML interpreter to retrieve a resource specified by a URI, 
    /// and to process its AIML object contents.
    /// </summary>
    public class ponder : AIMLBot.Utils.AIMLTagHandler
    {
        private Max.WebScraper.YourDictionary YourDictionary;
        private BrainEngine brainEngine;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public ponder(AIMLBot.Bot bot,
                        AIMLBot.User user,
                        AIMLBot.Utils.SubQuery query,
                        AIMLBot.Request request,
                        AIMLBot.Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            brainEngine =  App.GetEngine().BrainEngine;
        }

        protected override string ProcessChange()
        {
            if (this.templateNode.Name.ToLower() == "ponder")
            {
                if (this.query.InputStar.Count > 0)
                {
                    if (this.templateNode.Attributes.Count == 0)
                    {
                        string query = this.query.InputStar[0];
                        if (MaxUtils.HasEquation(query))
                        {

                        }
                        else
                        {



                            return string.Empty;

                        }

                    }
                }
            }
            return string.Empty;
        }
    }
}
