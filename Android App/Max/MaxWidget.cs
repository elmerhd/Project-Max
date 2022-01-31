using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Widget;

namespace Max
{
    [BroadcastReceiver(Label = "Widget Button Click")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/max_widget_provider")]
    public class MaxWidget : AppWidgetProvider
    {
        public static String ACTION_WIDGET_TURNON = "Listen";
        private const int REQUEST_CODE_SPEECH_INPUT = 1000;
        public static SpeechRecognizer Recognizer { get; set; }
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            //Update Widget layout
            //Run when create widget or meet update time
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(MaxWidget)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {
            //Build widget layout
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.max_widget);

            //Handle click event of button on Widget
            RegisterClicks(context, appWidgetIds, widgetView);
            //VoiceRecognitionListener recListener = new VoiceRecognitionListener();
            //recListener.Recognized += RecListener_Recognized;
            //Recognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
            //Recognizer.SetRecognitionListener(recListener);
            return widgetView;
        }

        private void RecListener_Recognized(object sender, string recognized)
        {
            ServerRequest serverRequest = new ServerRequest();
            serverRequest.UUIDv4 = Guid.NewGuid().ToString();
            serverRequest.Message = recognized;
            API.sendRequest(serverRequest);
        }

        protected virtual Intent CreateSpeechIntent()
        {
            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, Java.Util.Locale.Default);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Android.App.Application.Context.PackageName);
            intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            intent.PutExtra("android.speech.extra.DICTATION_MODE", true);
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, false);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
            return intent;
        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            var intent = new Intent(context, typeof(MaxWidget));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            //Button 1
            widgetView.SetOnClickPendingIntent(Resource.Id.imageButtonMic, GetPendingSelfIntent(context, ACTION_WIDGET_TURNON));

        }

        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(MaxWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "ACTION_WIDGET_TURNOFF or ACTION_WIDGET_TURNON" button
            if (ACTION_WIDGET_TURNON.Equals(intent.Action))
            {
                Toast.MakeText(context, "Testing", ToastLength.Long);
                //Recognizer.StartListening(this.CreateSpeechIntent());
            }

        }
    }
}