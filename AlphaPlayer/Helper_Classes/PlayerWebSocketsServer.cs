using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using Newtonsoft.Json;

namespace AlphaPlayer.Helper_Classes
{
    class PlayerWebSocketsServer
    {
        WebSocketServer Server;
        Player Player;

        Dictionary<string, Func<Dictionary<string, object>, string>> RequestHandler;
        List<IWebSocketConnection> Sockets;

        public PlayerWebSocketsServer(int port, Player player, bool IsAllInterfaces = true)
        {
            string Interface = IsAllInterfaces ? "0.0.0.0" : "127.0.0.1";

            if (IsAllInterfaces && !General_Helper.IsAdministrator())
                throw new InvalidOperationException();
            else if (IsAllInterfaces && General_Helper.IsAdministrator())
                Interface = "0.0.0.0";
            else if (!IsAllInterfaces)
                Interface = "127.0.0.1";

            this.Server = new WebSocketServer(String.Format("ws://{0}:{1}", Interface, port))
            {
                RestartAfterListenError = true
            };

            this.Player = player;
            this.Sockets = new List<IWebSocketConnection>();

            this.RequestHandler = new Dictionary<string, Func<Dictionary<string, object>, string>>() {
                { "get_data", this.HandleGetData },
                { "set_volume", this.HandleSetVolume },
                { "next", this.HandleNext },
                { "prev", this.HandlePrev },
                { "play", this.HandlePlay },
                { "pause", this.HandlePause },
                { "get_time", this.HandleGetTime }
            };
        }

        public void Start()
        {
            try
            {
                this.Server.Start(socket =>
                {
                    socket.OnMessage = message => HandleRequest(socket, message);
                    socket.OnOpen = () => this.Sockets.Add(socket);
                    socket.OnClose = () => this.Sockets.Remove(socket);
                });
            }
            catch (Exception)
            {
                // We don't really have a way to handle this crash, just ignore it and move to the next request.
                // This is only here to prevent the Server from crashing.
            }
        }

        public void HandleRequest(IWebSocketConnection socket, string data)
        {
            Dictionary<string, object> JsonData;
            string Action;
            try
            {
                JsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                Action = (string)JsonData["action"];
            } catch (Exception)
            {
                socket.Send(JsonConvert.SerializeObject(new Dictionary<string, string>() {
                    { "Error", "Invalid JSON request" }
                }));
                return;
            }

            if (JsonData != null)
            {
                try
                {
                    socket.Send(this.RequestHandler[Action].Invoke(JsonData));
                }
                catch (Exception ex)
                {
                    socket.Send(General_Helper.DictToJSON(new Dictionary<string, object>() {
                        { "Error", "Unkown Command"},
                        { "Message",  ex.Message}
                    }));
                }
            }
        }

        public void SendData()
        {
            string data = General_Helper.DictToJSON(new Dictionary<string, object>() {
                { "volume", Math.Floor(this.Player.GetVolume() * 100)},
                { "song_name", this.Player.GetCurrentSongName() },
            });

            this.Sockets.ToList().ForEach(s => s.Send(data));
        }

        public string HandleGetData(Dictionary<string, object> JsonData)
        {
            return General_Helper.DictToJSON(new Dictionary<string, object>() {
                { "volume", Math.Floor(this.Player.GetVolume() * 100)},
                { "song_name", this.Player.GetCurrentSongName() }
            });
        }

        public string HandleGetTime(Dictionary<string, object> JsonData)
        {
            return General_Helper.DictToJSON(new Dictionary<string, object>() {
                { "current_time",  this.Player.GetCurrentTime().ToString()}
            });
        }

        public string HandleSetVolume(Dictionary<string, object> JsonData)
        {
            if (JsonData.ContainsKey("volume"))
            {
                if (!int.TryParse(JsonData["volume"].ToString(), out int Volume))
                    throw new InvalidOperationException("Invalid 'volume' data");

                this.Player.SetVolume((float)Volume / 100.0f);

                return General_Helper.DictToJSON(new Dictionary<string, object>() { { "Success", "true" } });
            }
            else
                throw new InvalidOperationException("No 'volume' data");
        }

        private string HandlePlay(Dictionary<string, object> JsonData)
        {
            this.Player.PlaySong();
            return General_Helper.DictToJSON(new Dictionary<string, object>() { { "Success", "true" } });
        }

        private string HandleNext(Dictionary<string, object> JsonData)
        {
            this.Player.PlayNextSong();
            return General_Helper.DictToJSON(new Dictionary<string, object>() { { "Success", "true" } });
        }

        private string HandlePrev(Dictionary<string, object> JsonData)
        {
            this.Player.PlayPreviousSong();
            return General_Helper.DictToJSON(new Dictionary<string, object>() { { "Success", "true" } });
        }

        private string HandlePause(Dictionary<string, object> JsonData)
        {
            this.Player.StopSong();
            return General_Helper.DictToJSON(new Dictionary<string, object>() { { "Success", "true" } });
        }

        public void Stop()
        {
            this.Server.Dispose();
        }
    }
}
