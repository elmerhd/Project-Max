using System;
using System.Xml;
using System.Text;
using Max;
using System.Collections.Generic;

namespace AIMLBot.AIMLTagHandlers
{
    /// <summary>
    /// An element called bot, which may be considered a restricted version of get, is used to 
    /// tell the AIML interpreter that it should substitute the contents of a "bot predicate". The 
    /// value of a bot predicate is set at load-time, and cannot be changed at run-time. The AIML 
    /// interpreter may decide how to set the values of bot predicate at load-time. If the bot 
    /// predicate has no value defined, the AIML interpreter should substitute an empty string.
    /// 
    /// The bot element has a required name attribute that identifies the bot predicate. 
    /// 
    /// The bot element does not have any content. 
    /// </summary>
    public class greetings : AIMLBot.Utils.AIMLTagHandler
    {
        public MaxEngine MaxEngine;

        private List<string> Responses { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public greetings(AIMLBot.Bot bot,
                        AIMLBot.User user,
                        AIMLBot.Utils.SubQuery query,
                        AIMLBot.Request request,
                        AIMLBot.Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            MaxEngine = App.GetEngine();
            Responses = new List<string>();
        }
        protected override string ProcessChange()
        {
            if (this.templateNode.Name.ToLower() == "greetings")
            {
                string timesOfTheDay = this.query.InputStar[0];
                string actualTimesOfTheDay = MaxUtils.GetTimesOfTheDay();
                Responses.Add($"It's quite late {{!salutation}}, Its already in the {actualTimesOfTheDay}.");
                Responses.Add($"The time is {DateTime.Now.ToString("hh:mm tt")}, and i think it is {actualTimesOfTheDay} already.");
                if (timesOfTheDay.Equals(actualTimesOfTheDay))
                {
                    if (timesOfTheDay.Equals("night"))
                    {
                        return $"good {actualTimesOfTheDay} {{!salutation}}, have a good sleep.";
                    }
                    return timesOfTheDay;
                }
                else
                {
                    if (timesOfTheDay.Equals("morning") && ( actualTimesOfTheDay.Equals("afternoon")  || actualTimesOfTheDay.Equals("evening") || actualTimesOfTheDay.Equals("night")))
                    {
                        return Responses[new Random().Next(Responses.Count)];
                    } 
                    else if (timesOfTheDay.Equals("night") && (actualTimesOfTheDay.Equals("morning") || actualTimesOfTheDay.Equals("afternoon") || actualTimesOfTheDay.Equals("evening")))
                    {
                        return Responses[new Random().Next(Responses.Count)];
                    }
                }
            }
            return string.Empty;
        }
    }
}
