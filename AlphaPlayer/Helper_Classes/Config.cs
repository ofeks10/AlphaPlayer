﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaPlayer.Helper_Classes
{
    static class Config
    { 
        public static int WebPort = 8080;
        public static string WebFilesRelativePath;
        public static bool IsAPI = false;

        public static int WebSocketsPort = 8081;

        public static void Parse()
        {
            string configFileName = "config.ini";
            string[] configLines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + configFileName);

            foreach(string configLine in configLines)
            {
                string trimmedConfigLine = configLine.Trim().Replace(" ", String.Empty);
                string[] configLineParts = trimmedConfigLine.Split('=');

                if (configLineParts.Length != 2)
                    continue;

                switch(configLineParts[0])
                {
                    case "api":
                        string option = configLineParts[1];

                        if (!bool.TryParse(option, out bool IsAPI))
                            continue;

                        Config.IsAPI = IsAPI;
                        break;
                    case "port":
                        string strPort = configLineParts[1];

                        if (!int.TryParse(strPort, out int port))
                            continue;

                        Config.WebPort = port;
                        break;
                    case "web_sock_port":
                        string strWebSockPort = configLineParts[1];

                        if (!int.TryParse(strWebSockPort, out int WebSockPort))
                            continue;

                        Config.WebSocketsPort = WebSockPort;
                        break;
                    case "web_files_relative_path":
                        if (!Config.IsAPI)
                            continue;

                        string path = configLineParts[1];

                        if (!File.Exists(Directory.GetCurrentDirectory() + "\\" + path + "\\Main.html"))
                            throw new InvalidDataException("Invalid API path");

                        Config.WebFilesRelativePath = path;
                        break;
                    default:
                        continue;
                }
            }

            if (Config.WebFilesRelativePath == null && Config.IsAPI)
                throw new InvalidDataException("No API path specified");
        }
    }
}
