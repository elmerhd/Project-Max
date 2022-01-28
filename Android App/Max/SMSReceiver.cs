using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Telephony;
using Android.Provider;
using Xamarin.Essentials;
using System.Collections.ObjectModel;

namespace Max
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class SMSReceiver : BroadcastReceiver
    {
        public static readonly string INTENT_ACTION = "android.provider.Telephony.SMS_RECEIVED";

        [Obsolete]
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(INTENT_ACTION))
            {
                Bundle bundle = intent.Extras;
                if (bundle != null)
                {
                    try
                    {
                        var smsArray = (Java.Lang.Object[])bundle.Get("pdus");

                        foreach (var item in smsArray)
                        {
                            var sms = Android.Telephony.SmsMessage.CreateFromPdu((byte[])item);

                            ServerRequest serverRequest = new ServerRequest
                            {
                                UUIDv4 = Guid.NewGuid().ToString(),
                                Message = $"MaxSMSNotification"
                            };
                            API.sendRequest(serverRequest);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Error : " + e.Message);
                    }
                }
            }
        }
    }
}