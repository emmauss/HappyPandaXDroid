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
using NLog;

namespace HappyPandaXDroid.Core
{
    class Net
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string session_id = string.Empty;

        static List<Client> ClientList = new List<Client>();

        public class Client
        {
            public bool InUse = false;
            public bool initialise = false;
            public TcpClient client;
            public Client()
            {
                client = new TcpClient(App.Settings.Server_IP, int.Parse(App.Settings.Server_Port));
                string response = Connect(this);
                ClientList.Add(this);
            }
        }

        public static bool Connect()
        {
            Client cli = null;
            try
            {
                logger.Info("Connecting to server ...");
                cli = new Client();
                return cli.client.Connected;
            }catch(SocketException ex)
            {
                logger.Error(ex, "\n Exception Caught In Net.Connect.");
                return false;

            }
        }

        static string Connect(Client cli)
        {
            var listener = cli.client;
            if (session_id == null || session_id == string.Empty)
            {
                try
                {
                    logger.Info("Connection Successful. Starting Handshake");
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
                        logger.Info("Handshake Failed");
                        success = true;
                        response = "fail";
                    }
                    else
                    {
                        logger.Info("Handshake Successful");
                        payload = payload.Replace("<EOF>", "");
                        Dictionary<string, string> reply =
                            JSON.Serializer.simpleSerializer.Deserialize<Dictionary<string, string>>(payload);
                        success = reply.TryGetValue("session", out session_id);
                    }
                    cli.initialise = true;
                    return response;
                }
                catch (SocketException ex)
                {
                    logger.Error(ex, "\n Exception Caught In Net.Connect.");

                    return "fail";
                }
            }
            else return null;
}


        static Client GetActiveConnection()
        {
            Client tcp = null;
            CleanClients();
            foreach(var c in ClientList)
            {
                if (!c.InUse & c.client.Connected)
                {
                    c.InUse = true;
                    tcp = c;
                    break;
                }
            }
            if(tcp == null)
            {
                tcp = new Client();
                tcp.InUse = true;
            }
            tcp.InUse = true;
            return tcp;
        }

        static void CleanClients()
        {
            var removelist = new List<Client>();
            foreach(var c in ClientList)
            {
                if (!c.client.Connected)
                    removelist.Add(c);
            }
            foreach(var v in removelist)
            {
                ClientList.Remove(v);
            }
        }

        public  static string SendPost(string payload)
        {
            logger.Info("Sending Request.\n Request : \n {0} \n", payload);
            string response = "fail";
            Client listener = GetActiveConnection();
            try
            {
                if (session_id == null || session_id == string.Empty)
                    Connect();
                if (session_id!= null && session_id!=string.Empty)
                {
                    var stream = listener.client.GetStream();
                    byte[] req = Encoding.UTF8.GetBytes(payload + "<EOF>");
                    byte[] res = new byte[1024*10];
                    if (!listener.initialise)
                    {
                        listener.initialise = true;
                        while (true)
                        {
                            stream.Read(res, 0, res.Length);
                            payload = Encoding.UTF8.GetString(res).TrimEnd('\0');
                            response += payload;
                            if (response.Contains("<EOF>"))
                                break;
                            Array.Clear(res, 0, res.Length);
                        }
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
                    logger.Info("Response from server: \n {0}", response);
                }

            }
            catch(System.Exception ex)
            {
                listener.initialise = true;
                listener.InUse = false;
                logger.Error(ex, "\n Exception Caught In Net.SendPost.");

            }
            listener.initialise = true;
            listener.InUse = false;
            return response;
        }
        
    }
}