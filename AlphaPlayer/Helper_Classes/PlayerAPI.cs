﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AlphaPlayer.Helper_Classes
{
    class PlayerAPI
    {
        private HttpListener HttpListener;
        private Player Player;

        static bool KeepGoing = true;
        static List<Task> OngoingTasks = new List<Task>();

        public int CrashesHappend = 0;

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
            await Task.Delay(1);
            try
            {
                this.Perform(context);
            }
            catch (Exception)
            {
                // We don't really have a way to handle this crash, just ignore it and move to the next request.
                // This is only here to prevent the API from crashing.
                this.CrashesHappend += 1;
            }
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
            response.StatusCode = 200;

            switch (uri)
            {
                case "":
                    responseString = this.MainPage();
                    break;
                case "GetWebSocketPort":
                    responseString = "" + Config.WebSocketsPort;
                    break;
                default:
                    if (File.Exists(this.WebFilesPath + "\\" + uri))
                    {
                        string contents = File.ReadAllText(this.WebFilesPath + "\\" + uri);
                        responseString = contents;
                    }
                    else
                    {
                        responseString = "404";
                        response.StatusCode = 404;
                    }
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
            if(this.HttpListener.IsListening)
                this.HttpListener.Stop();
        }
    }
}
