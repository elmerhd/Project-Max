using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using Android.Speech;
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
using System.Net;
using System.Threading;
using System.Timers;

namespace Max
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private static readonly string TAG = typeof(MainActivity).FullName;
		private const int REQUEST_CODE_SPEECH_INPUT = 1000;
		private const string APPID = "93e667aa-3452-43a9-a2d7-12ee078fead4";
		private static readonly string CHANNEL_ID = "maxai_notification";
		private static readonly int NOTIFICATION_ID = 1000;
		private const int PORT = 6669;
		public static UdpClient UdpClient = new UdpClient();
		private static ServerResponse ServerResponse = new ServerResponse();

		private ImageButton BtnMic;
		public static SpeechRecognizer Recognizer { get; set; }
		public static Intent SpeechIntent { get; set; }

		static readonly int REQUEST_AUDIO = 1001;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			InitComponents();
			RequestApplicationPermissions();
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Interval = 500;
			timer.Elapsed += t_Tick;
			timer.Start();
		}

        private void t_Tick(object sender, ElapsedEventArgs e)
        {
			try
			{
				var from = new IPEndPoint(0, 0);
				var recvBuffer = UdpClient.Receive(ref from);
				string dataBuff = (Encoding.UTF8.GetString(recvBuffer));
				ServerResponse content = JsonConvert.DeserializeObject<ServerResponse>(dataBuff);

				if (content.UUIDv4 != ServerResponse.UUIDv4)
				{
					ServerResponse = content;
					if (content.Message == "hey max")
					{
						this.RunOnUiThread(() =>
						{
							Recognizer.StartListening(SpeechIntent);
						});
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("error : " + ex.Message);
			}
		}

		public void InitComponents()
		{
			CreateNotificationChannel();
			UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
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
		}

        protected override void OnPause()
        {
			base.OnPause();
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
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguagePreference, Java.Util.Locale.Default);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
			SpeechIntent.PutExtra("android.speech.extra.DICTATION_MODE", true);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraPartialResults, false);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 15000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 15000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
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
						notify(serverRequest);
						API.sendRequest(serverRequest);
						break;
				}
			}
		}

        public void notify(ServerRequest serverRequest)
        {
            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentTitle("API Request Success") // Set the title
                          .SetSmallIcon(Resource.Drawable.abc_ic_star_black_48dp) // This is the icon to display
                          .SetContentText($"{serverRequest.Message}"); // the message to display.

            // Finally, publish the notification:
            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());
        }

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
			notify(serverRequest);
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