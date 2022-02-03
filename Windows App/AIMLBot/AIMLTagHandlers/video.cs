using System;
using System.Xml;
using System.Text;
using Max;


namespace AIMLBot.AIMLTagHandlers
{
    public class video : AIMLBot.Utils.AIMLTagHandler
    {
        public MaxEngine MaxEngine;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public video(AIMLBot.Bot bot,
                        AIMLBot.User user,
                        AIMLBot.Utils.SubQuery query,
                        AIMLBot.Request request,
                        AIMLBot.Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            MaxEngine = App.GetEngine();
        }

        protected override string ProcessChange()
        {
            if (this.templateNode.Name.ToLower() == "video")
            {
                string key = this.query.InputStar[0];
                MaxEngine.NetflixEngine.Play(MaxEngine.NetflixEngine.Search(key));
                return MaxEngine.MaxConfig.DefaultCommandMessages[new Random().Next(MaxEngine.MaxConfig.DefaultCommandMessages.Count)];
            }
            return string.Empty;
        }
    }
}
