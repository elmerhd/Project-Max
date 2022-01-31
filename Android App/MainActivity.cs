using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using Android.Speech;
using Java.Util;
using System.Collections.Generic;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using Android.Util;
using Encoding = System.Text.Encoding;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using System.Net.Sockets;
using Android.Media;
using Android.Media.Session;

namespace Max
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private const int REQUEST_CODE_SPEECH_INPUT = 1000;
		private const string APPID = "93e667aa-3452-43a9-a2d7-12ee078fead4";
		private static readonly string CHANNEL_ID = "maxai_notification";
		private static readonly int NOTIFICATION_ID = 1000;

		private MediaButtonBroadcastReceiver MediaButtonBroadcastReceiver;
		private ImageButton BtnMic;
		public static SpeechRecognizer Recognizer { get; set; }
		public static Intent SpeechIntent { get; set; }

		static readonly int REQUEST_AUDIO = 1001;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Console.WriteLine("Test");
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			InitComponents();
			RequestApplicationPermissions();
		}

		public void InitComponents()
		{
			CreateNotificationChannel();
			CreateBroadcastChannel();
			BtnMic = FindViewById<ImageButton>(id: Resource.Id.imageButtonMic);
			BtnMic.Click += BtnMic_Click;

		}

		public void RequestApplicationPermissions()
		{
			if (!AudioPermissionGranted())
			{
				ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.RecordAudio }, REQUEST_AUDIO);
			}
			else
			{
				InitializedSpeechRecognizer();
			}

		}

        protected override void OnResume()
        {
            base.OnResume();
			//RegisterReceiver(MediaButtonBroadcastReceiver, new IntentFilter("com.junk.application.max"));
		}

        protected override void OnPause()
        {
			//UnregisterReceiver(MediaButtonBroadcastReceiver);
			base.OnPause();
        }

        private void CreateBroadcastChannel()
		{
			//MediaButtonBroadcastReceiver = new MediaButtonBroadcastReceiver();
			//RegisterReceiver(MediaButtonBroadcastReceiver, new IntentFilter("com.junk.application.max"));
			MediaSession ms = new MediaSession(ApplicationContext, PackageName);
			ms.Active = true;
			ms.SetCallback(new myMediaSession());
			ms.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
			ms.Active = true;
		}


		private void CreateNotificationChannel()
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.O)
			{
				// Notification channels are new in API 26 (and not a part of the
				// support library). There is no need to create a notification 
				// channel on older versions of Android.
				return;
			}

			var channel = new NotificationChannel(CHANNEL_ID, "Max AI", NotificationImportance.Default)
			{
				Description = "Max AI Notification"
			};

			NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.CreateNotificationChannel(channel);
		}

		private void InitializedSpeechRecognizer()
		{
			VoiceRecognitionListener recListener = new VoiceRecognitionListener();
			recListener.BeginSpeech += RecListener_BeginSpeech;
			recListener.EndSpeech += RecListener_EndSpeech;
			recListener.Error += RecListener_Error;
			recListener.Ready += RecListener_Ready;
			recListener.Recognized += RecListener_Recognized;

			Recognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
			Recognizer.SetRecognitionListener(recListener);

			SpeechIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguage, Locale.Default);
		}

		public bool AudioPermissionGranted()
		{
			return ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) == (int)Permission.Granted;
		}

		private void BtnMic_Click(object sender, System.EventArgs e)
		{
			if (AudioPermissionGranted())
			{
				StartActivityForResult(SpeechIntent, REQUEST_CODE_SPEECH_INPUT);
			}
			else
			{
				Toast.MakeText(Application.Context, "Audio Recording Permission Denied", ToastLength.Long).Show();
			}

		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Result.Ok && data != null)
			{
				switch (requestCode)
				{
					case REQUEST_CODE_SPEECH_INPUT:
						IList<string> result = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
						string message = result[0];
						ServerRequest serverRequest = new ServerRequest();
						serverRequest.UUIDv4 = Guid.NewGuid().ToString();
						serverRequest.Message = message;
						API.sendRequest(serverRequest);
						break;
				}
			}
		}

		//public void notify(ServerRequest serverRequest)
		//{
		//          var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
		//                        .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
		//                        .SetContentTitle("API Request Success") // Set the title
		//                        .SetSmallIcon(Resource.Drawable.abc_ic_star_black_48dp) // This is the icon to display
		//                        .SetContentText($"{serverRequest.Message}"); // the message to display.

		//          // Finally, publish the notification:
		//          NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
		//          notificationManager.Notify(NOTIFICATION_ID, builder.Build());
		//      }

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			if (requestCode == REQUEST_AUDIO)
			{
				if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
				{
					Toast.MakeText(Application.Context, "Audio Recording Permission Granted", ToastLength.Long).Show();
					InitializedSpeechRecognizer();
				}
				else
				{
					Toast.MakeText(Application.Context, "Audio Recording Permission Denied", ToastLength.Long).Show();
				}
			}
			else
			{
				Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
				base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			}
		}

		private void RecListener_Ready(object sender, Bundle e)
		{
			BtnMic.SetBackgroundResource(Resource.Drawable.MaxButtonReady);
		}

		private void RecListener_BeginSpeech() => Log.Debug(nameof(MainActivity), nameof(RecListener_BeginSpeech));

		private void RecListener_EndSpeech() => Log.Debug(nameof(MainActivity), nameof(RecListener_EndSpeech));

		private void RecListener_Error(object sender, SpeechRecognizerError e) => Log.Debug(nameof(MainActivity), $"{nameof(RecListener_Error)}={e.ToString()}");

		private void RecListener_Recognized(object sender, string recognized)
		{
			BtnMic.SetBackgroundResource(Resource.Drawable.MaxButtonIdle);
			ServerRequest serverRequest = new ServerRequest();
			serverRequest.UUIDv4 = Guid.NewGuid().ToString();
			serverRequest.Message = recognized;
			API.sendRequest(serverRequest);
		}


	}
	class myMediaSession : MediaSession.Callback
	{

		public override bool OnMediaButtonEvent(Intent mediaButtonIntent)
		{

			MainActivity.Recognizer.StartListening(MainActivity.SpeechIntent);
			return base.OnMediaButtonEvent(mediaButtonIntent);
		}

	}
}