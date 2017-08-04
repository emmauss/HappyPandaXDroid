using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using SocketIO.Client;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HappyPandaXDroid.Core
{
    class Net
    {
        
        public static string session_id = string.Empty;

        public  static string Connect()
        {
            try
            {
                TcpClient listener = new TcpClient(App.Settings.Server_IP, int.Parse(App.Settings.Server_Port));
                string response = string.Empty;
                string payload;
                var stream = listener.GetStream();
                byte[] res = new byte[1024];
                while (true)
                {
                    stream.Read(res, 0, res.Length);
                    payload = Encoding.UTF8.GetString(res).TrimEnd('\0');
                    response += payload;
                    if (response.Contains("<EOF>"))
                        break;
                    Array.Clear(res, 0, res.Length);
                }
                payload = payload.Replace("<EOF>", "");
                App.Server.info = JSON.Serializer.simpleSerializer.Deserialize<App.Server.ServerInfo>(payload);
                List<Tuple<string, string>> main = new List<Tuple<string, string>>();
                JSON.API.PushKey(ref main, "name", "test");
                JSON.API.PushKey(ref main, "session", "");
                JSON.API.PushKey(ref main, "data", "{}");
                string request = JSON.API.ParseToString(main);
                byte[] req = Encoding.UTF8.GetBytes(request + "<EOF>");
                stream.Write(req, 0, req.Length);
                Array.Clear(res, 0, res.Length);
                stream.Read(res, 0, res.Length);
                payload = Encoding.UTF8.GetString(res).TrimEnd('\0');
                bool success = false;
                if (!payload.Contains("Authenticated"))
                {
                    success = true;
                    response = "fail";
                }
                else
                {
                    payload = payload.Replace("<EOF>", "");
                    Dictionary<string, string> reply =
                        JSON.Serializer.simpleSerializer.Deserialize<Dictionary<string, string>>(payload);
                    success = reply.TryGetValue("session", out session_id);
                }
                return response;
            }catch(SocketException ex)
            {
                return "fail";
            }
        }
        public  static string SendPost(string payload)
        {
            
            string response = "fail";
            TcpClient listener = new TcpClient(App.Settings.Server_IP, int.Parse(App.Settings.Server_Port));
            try
            {
                if(session_id == null || session_id == string.Empty)
                    response = Connect();
                if (session_id!= null && session_id!=string.Empty)
                {
                    var stream = listener.GetStream();
                    byte[] req = Encoding.UTF8.GetBytes(payload + "<EOF>");
                    byte[] res = new byte[1024*10];
                    while (true)
                    {
                        stream.Read(res, 0, res.Length);
                        payload = Encoding.UTF8.GetString(res).TrimEnd('\0');
                        response += payload;
                        if (response.Contains("<EOF>"))
                            break;
                        Array.Clear(res, 0, res.Length);
                    }
                    stream.Write(req, 0, req.Length);
                    Array.Clear(res, 0, res.Length);
                    response = string.Empty;
                    while (true)
                    {
                        stream.Read(res, 0, res.Length);
                        string reply= Encoding.UTF8.GetString(res).TrimEnd('\0');
                        response += reply;
                        if (response.Contains("<EOF>"))
                            break;
                        Array.Clear(res, 0, res.Length);
                    }
                }

            }
            catch(System.Exception ex)
            {

            }
            return response;
        }
        
    }
}