using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace Max
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "com.junk.application.max" })]
    public class MediaButtonBroadcastReceiver : BroadcastReceiver
    {

        public MediaButtonBroadcastReceiver()
        {

        }
        public string ComponentName { get { return Class.Name; } }
        public override void OnReceive(Context context, Intent intent)
        {
            //Toast.MakeText(context, "Test", ToastLength.Short);
            //if (intent.Action != Intent.ActionMediaButton)
            //    return;

            var keyEvent = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            Toast.MakeText(context, ""+keyEvent.KeyCode, ToastLength.Short);
            Console.WriteLine("KeyEvent : "+keyEvent.KeyCode);
            switch (keyEvent.KeyCode)
            {
                case Keycode.MediaPlay:
                    Console.WriteLine("MediaPlay");
                    //MainActivity.Recognizer.StartListening(MainActivity.SpeechIntent);
                    break;
                case Keycode.MediaPlayPause:
                    Console.WriteLine("PlayPause");
                    MainActivity.Recognizer.StartListening(MainActivity.SpeechIntent);
                    break;
                case Keycode.MediaNext:
                    Console.WriteLine("Next");
                    break;
                case Keycode.MediaPrevious:
                    Console.WriteLine("Prev");
                    break;
                case Keycode.Call:
                    Console.WriteLine("Call");
                    //MainActivity.Recognizer.StartListening(MainActivity.SpeechIntent);
                    break;
            }
        }
    }
}