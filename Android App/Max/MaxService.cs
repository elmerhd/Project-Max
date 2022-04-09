using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Max
{
    [Service(Exported = true, Name = "com.junk.application.max.MaxService")]
    public class MaxService : Service
    {
        private static readonly string TAG = typeof(MainActivity).FullName;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Timer timer = new Timer(_ => OnCallBack(), null, 1000 * 10, Timeout.Infinite);
            return base.OnStartCommand(intent, flags, startId); 
        }
        private void OnCallBack()
        {
            Log.Info(TAG, "Starting.");
        }

    }
}