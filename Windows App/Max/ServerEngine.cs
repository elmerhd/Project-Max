using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Max 
{
    public class ServerEngine
    {

        private MaxEngine MaxEngine { get; set; }

        private Timer Timer { get; set; }

        private ServerResponse ServerResponse = new ServerResponse();

        private const int PORT = 6969;
        public UdpClient UdpClient = new UdpClient();

        public ServerEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            this.ServerResponse = new ServerResponse();
            Timer = new Timer();
            UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
            Timer.Interval = 500;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            maxEngine.BrainEngine.Log($"Loading {nameof(ServerEngine)}");
        }

        
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
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
                    MaxEngine.BrainEngine.analyze(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error : " + ex.Message);
            }
            
        }
    }

    public class ServerResponse
    {
        public string UUIDv4 { get; set; }
        public string Message { get; set; }
        public int Port { get; set; }

    }
}
