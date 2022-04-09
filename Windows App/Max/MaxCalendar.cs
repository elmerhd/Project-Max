using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Max
{
    public class MaxCalendar : MaxService
    {
        public CalendarService service { get; set; }

        public DateTime DateTime { get; set; }

        public MaxCalendar(MaxEngine maxEngine, MaxUI maxUI, DateTime dateTime) : base(maxEngine, maxUI)
        {
            this.OnStart(true);
            this.DateTime = dateTime;
            this.Log($"Initializing Calendar ...");
            this.Log($"Getting events. . .");
        }

        private void GetEvents(DateTime dateTime)
        {
            string data = string.Empty;
            try
            {
                string[] Scopes = {
                   CalendarService.Scope.Calendar,
                   CalendarService.Scope.CalendarEvents,
                   CalendarService.Scope.CalendarEventsReadonly
                };
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = MaxEngine.MaxConfig.GoogleCalendarClientId,
                            ClientSecret = MaxEngine.MaxConfig.GoogleCalendarSecret
                        },
                        Scopes,
                        "user",
                        CancellationToken.None
                    ).Result;

                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Project Max",
                });

                string day = string.Empty;

                EventsResource.ListRequest request = service.Events.List("primary");
                request.TimeMin = dateTime;
                request.TimeMax = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                string eventsValue = string.Empty;

                Events events = request.Execute();

                if (dateTime.Date == DateTime.Now.Date)
                {
                    day = "Today";
                }
                else
                {
                    day = $"On {dateTime.DayOfWeek.ToString()}";
                }

                if (events.Items != null && events.Items.Count > 0)
                {
                    int count = 0;
                    foreach (var eventItem in events.Items)
                    {
                        string when = eventItem.Start.DateTime.ToString();
                        DateTime dt = DateTime.Parse(when);

                        if (String.IsNullOrEmpty(when))
                        {
                            when = eventItem.Start.Date;
                        }
                        eventsValue += $"{eventItem.Summary} at {dt.ToString("hh:mm tt")}. ";
                        count++;
                    }
                    data = $"you have {count} upcoming { (count > 1 ? "events" : "event") } {day}, {eventsValue}";
                }
                else
                {
                    data = $"No upcoming events {day}, {{!salutation}}.";
                }
            }
            catch (Exception ex)
            {
                data = $"I encountered an error while checking your calendar. It say's {ex.Message}";
                this.Log(data);
            } 
            finally
            {
                this.Log(data);
                this.OnFinished(data);
            }
        }

        public override void StartService()
        {
            GetEvents(this.DateTime);
        }
    }
}
