using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AlphaPlayer.Helper_Classes
{
    class PlayerAPI
    {
        private HttpListener HttpListener;
        private Player Player;

        private Dictionary<string, Delegate> Routes;

        public PlayerAPI(int port, Player player)
        {
            this.HttpListener = new HttpListener();
            this.HttpListener.Prefixes.Add("http://localhost:" + port + "/");
            this.Player = player;
        }

        public void Start()
        {
            HttpListener.Start();
            this.MainLoop();
        }

        public void MainLoop()
        {
            while (true)
            {
                var context = HttpListener.GetContext();
                var response = context.Response;
                const string responseString = "<html><body>Hello world</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                this.Player.SetVolume(1);
            }
        }

        public void Stop()
        {
            this.HttpListener.Stop();
        }
    }
}
