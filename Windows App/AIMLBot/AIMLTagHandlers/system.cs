using System;
using System.Xml;
using System.Text;
using Max;
using System.Threading;
using System.Diagnostics;

namespace AIMLBot.AIMLTagHandlers
{
    /// <summary>
    /// System Actions
    /// </summary>
    public class system : AIMLBot.Utils.AIMLTagHandler
    {
        public MaxEngine MaxEngine;
        public MaxUI MaxUI;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public system(AIMLBot.Bot bot,
                        AIMLBot.User user,
                        AIMLBot.Utils.SubQuery query,
                        AIMLBot.Request request,
                        AIMLBot.Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            MaxEngine = App.GetEngine();
            MaxUI = App.GetUI();
        }

        protected override string ProcessChange()
        {
            if (this.templateNode.Name.ToLower() == "system")
            {
                string uri = this.templateNode.Attributes["action"].Value;
                if (uri.Equals("checkInternet"))
                {
                    new Thread(new MaxInternetSpeedTest(MaxEngine, MaxUI).StartService).Start();
                    return MaxEngine.MaxConfig.DefaultWaitingMessages[new Random().Next(MaxEngine.MaxConfig.DefaultWaitingMessages.Count)];
                }
                else if (uri.Equals("checkWeather"))
                {
                    string location = this.query.InputStar[0];
                    new Thread(new MaxWeather(MaxEngine, MaxUI, location).StartService).Start();
                    return MaxEngine.MaxConfig.DefaultWaitingMessages[new Random().Next(MaxEngine.MaxConfig.DefaultWaitingMessages.Count)];
                }
                else if (uri.Equals("shutdown"))
                {
                    new Thread(MaxUtils.Shutdown).Start();
                }
                else if (uri.Equals("restart"))
                {
                    Process.Start("restart", "/s /t 0");
                }
                else if (uri.Equals("showUI"))
                {
                    App.GetUI().ShowUI();
                }
                else if (uri.Equals("hideUI"))
                {
                    App.GetUI().HideUI();
                }
                else if (uri.Equals("unlockWindows"))
                {
                    MaxUtils.UnlockWin(MaxEngine);
                } 
                else if (uri.Equals("setAlarm"))
                {
                    new Thread(new MaxAlarm(MaxEngine, MaxUI, MaxUtils.DecodedDateTime, true).StartService).Start();
                    return MaxEngine.MaxConfig.DefaultAlarmMessages[new Random().Next(MaxEngine.MaxConfig.DefaultAlarmMessages.Count)];
                }
                else if (uri.Equals("checkCalendar"))
                {
                    new Thread(new MaxCalendar(MaxEngine, MaxUI, DateTime.Now).StartService).Start();
                    return MaxEngine.MaxConfig.DefaultWaitingMessages[new Random().Next(MaxEngine.MaxConfig.DefaultWaitingMessages.Count)];
                }
                return MaxEngine.MaxConfig.DefaultCommandMessages[new Random().Next(MaxEngine.MaxConfig.DefaultCommandMessages.Count)];
            }
            return string.Empty;
        }
    }
}
