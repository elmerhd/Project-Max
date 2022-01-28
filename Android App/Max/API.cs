using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Max
{
    public class API
    {
        private static readonly int PORT = 6969;
        public static void sendRequest(ServerRequest serverRequest)
        {
            UdpClient udpClient = new UdpClient();
            string contents = JsonConvert.SerializeObject(serverRequest);
            var data = Encoding.UTF8.GetBytes(contents);
            udpClient.Send(data, data.Length, "255.255.255.255", PORT);
        }
    }
}