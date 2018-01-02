using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaPlayer.Helper_Classes
{
    class PlayerAPI
    {
        private HttpListener HttpListener;
        private Player Player;

        static bool KeepGoing = true;
        static List<Task> OngoingTasks = new List<Task>();

        string InterfaceName;

        string WebFilesPath;

        public PlayerAPI(int port, Player player, string WebFilesPath, bool AllInterfaces = true)
        {
            this.HttpListener = new HttpListener();
            this.WebFilesPath = WebFilesPath;

            if (AllInterfaces && !General_Helper.IsAdministrator())
                throw new InvalidOperationException();
            else if (AllInterfaces && General_Helper.IsAdministrator())
                this.InterfaceName = "*";
            else if (!AllInterfaces)
                this.InterfaceName = "localhost";

            this.HttpListener.Prefixes.Add(String.Format("http://{0}:{1}/", InterfaceName, port));

            this.Player = player;
        }

        public void Start()
        {
            HttpListener.Start();
            ProcessAsync(HttpListener).ContinueWith(async task => {
                await Task.WhenAll(OngoingTasks.ToArray());
            });
        }

        public void ExceptionHandler(Task task)
        {
            throw new NotImplementedException();
        }

        async Task ProcessAsync(HttpListener listener)
        {
            while (KeepGoing)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await this.HandleRequestAsync(context);
            }
        }

        async Task HandleRequestAsync(HttpListenerContext context)
        {
            await Task.Delay(5);
            this.Perform(context);
        }

        string MainPage()
        {
            string MainFile = File.ReadAllText(this.WebFilesPath + "/Main.html");
            return MainFile;
        }

        void Perform(HttpListenerContext ctx)
        {
            HttpListenerResponse response = ctx.Response;
            HttpListenerRequest request = ctx.Request;

            string uri = request.Url.AbsolutePath.Substring(1);
            string responseString = "";

            if(uri.Contains("SetVolume"))
            {
                string[] volume_arr = uri.Split('/');
                if(volume_arr.Length >= 2)
                {
                    string vol_str = volume_arr[1];
                    int Volume;

                    if (!int.TryParse(vol_str, out Volume))
                    {
                        responseString = "Invalid volume " + vol_str;
                        response.StatusCode = 403;
                    }
                    else
                    {
                        this.Player.SetVolume((float)Volume / 100.0f);
                    }
                }
            }

            switch (uri)
            {
                case "":
                    responseString = this.MainPage();
                    break;
                case "GetVolume":
                    responseString = "" + Math.Floor(this.Player.GetVolume() * 100);
                    break;
                case "GetName":
                    if(this.Player.CurrentSong != null)
                        responseString = this.Player.CurrentSong.SongName;
                    break;
                case "GetTime":
                    responseString = this.Player.GetCurrentTime().ToString();
                    break;
                case "Pause":
                    Player.StopSong();
                    break;
                case "Play":
                    try
                    {
                        this.Player.PlaySong();
                        responseString = "Played";
                    }
                    catch (InvalidOperationException ex)
                    {
                        responseString = ex.Message;
                    }
                    break;
                case "Previous":
                    try
                    {
                        this.Player.PlayPreviousSong();
                    }
                    catch (InvalidOperationException)
                    {

                    }
                    catch (InvalidDataException)
                    {

                    }
                    break;
                case "Next":
                    try
                    {
                        this.Player.PlayNextSong();
                    }
                    catch (InvalidOperationException)
                    {
                        
                    }
                    catch (InvalidDataException)
                    {
                        
                    }
                    break;
                default:
                    responseString = "404";
                    response.StatusCode = 404;
                    break;
            }
            
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // You must close the output stream.
            output.Close();
        }

        public void Stop()
        {
            this.HttpListener.Stop();
        }
    }
}
