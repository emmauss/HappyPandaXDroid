using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace HappyPandaXDroid.Core
{
    class App
    {
        

        /// <summary>
        /// This is the Settings static class that can be used in your Core solution or in any
        /// of your client applications. All settings are laid out the same exact way with getters
        /// and setters. 
        /// </summary>
        public static class Settings
        {
            public static string username = "test";
            public static string cache = Android.OS.Environment.ExternalStorageDirectory.Path+"/HPX/cache/";
            private static ISettings AppSettings
            {
                get
                {
                    return CrossSettings.Current;
                }
            }

            #region Setting Constants

            private const string SettingsKey = "settings_key";
            private static readonly string SettingsDefault = string.Empty;

            #endregion

            public static string Server_IP
            {
                get
                {
                    return AppSettings.GetValueOrDefault("server_ip", string.Empty);
                }
                set
                {
                    AppSettings.AddOrUpdateValue("server_ip", value);
                }
            }

            

            public static string GeneralSettings
            {
                get
                {
                    return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
                }
                set
                {
                    AppSettings.AddOrUpdateValue(SettingsKey, value);
                }
            }

        }

        public class Server
        {
            public static ServerInfo info = new ServerInfo();
            public class ServerInfo
            {
                public Data data = new Data();
                public string name = string.Empty;
                public string session = string.Empty;
            }

            public class Data
            {
                public bool guest_allowed;
                public Version version = new Version();

            }

            public class Version
            {
                public int[] torrent = new int[3];
                public int[] db = new int[3];
                public int[] core = new int[3];
            }

            public static void GetServerInfo()
            {
                string fname = "get_version";
                List<Tuple<string,string>> main = new List<Tuple<string, string>>();
                List<Tuple<string, string>> data = new List<Tuple<string, string>>();
                List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
                JSON.API.PushKey(ref main, "id", "1");
                JSON.API.PushKey(ref funct, "fname", fname);
                string response = JSON.API.ParseToString(funct);
                JSON.API.PushKey(ref data, "function", response);
                response = JSON.API.ParseToString(data);
                JSON.API.PushKey(ref main, "msg", response);
                response = JSON.API.ParseToString(main);


                string res = Net.Connect();

            }

            public static string StartCommand(int command_id)
            {
                string response = CreateCommand("start_command", command_id);
                response = Net.SendPost(response);
                string state = string.Empty;
                if (GetError(response) == "none")
                {
                    state = JSON.API.GetData(response, 2);
                    if (state.Contains("started"))
                        return "started";
                    else
                        return "failed";
                }
                else return (GetError(response));
            }

            public static string StopCommand(int command_id)
            {
                string response = CreateCommand("stop_command", command_id);
                response = Net.SendPost(response);
                string state = string.Empty;
                if (GetError(response) == "none")
                {
                    state = JSON.API.GetData(response, 2);
                    if (state.Contains("stopped"))
                        return "stopped";
                    else
                        return "failed";
                }
                else return (GetError(response));
            }

            public static string UndoCommand(int command_id)
            {
                
                string response = CreateCommand("undo_command",command_id);
                response = Net.SendPost(response);
                string state = string.Empty;
                if (GetError(response) == "none")
                {
                    state = JSON.API.GetData(response, 2);
                    if(state.Contains("s"))
                    return state;
                    else
                        return "failed";
                }
                else return (GetError(response));
            }

            static string CreateCommand(string key, int command_id)
            {
                List<Tuple<string, string>> main = new List<Tuple<string, string>>();
                List<Tuple<string, string>> funct = new List<Tuple<string, string>>();

                JSON.API.PushKey(ref main, "name", Settings.username);
                JSON.API.PushKey(ref main, "session", Net.session_id);
                JSON.API.PushKey(ref funct, "fname", key);
                JSON.API.PushKey(ref funct, "command_ids", "[" + command_id + "]");
                string response = JSON.API.ParseToString(funct);
                JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
                response = JSON.API.ParseToString(main);
                return response;
            }

            public static int GetCommandId(int item_id, string command_response)
            {
                if (command_response.Contains("\"" + item_id + "\""))
                {
                    string command = JSON.API.GetData(command_response, 2);
                    string id = command.Substring(command.IndexOf(":"));
                    id = id.Substring(id.IndexOf(":") + 1, id.IndexOf("}")-1);
                    id = id.Trim('\"');
                    id = id.Trim(' ');
                    return int.Parse(id);
                }
                else return -1;
            }

            public static string GetCommandValue(int command_id, int item_id, string name,string type)
            {
                string response = CreateCommand("get_command_value", command_id);
                response = Net.SendPost(response);
                string data = string.Empty;
                if (GetError(response) == "none")
                {
                    data = JSON.API.GetData(response, 2);
                    string filename = string.Empty;
                    data = data.Substring(data.IndexOf(":") + 1);
                    data = data.Remove(data.LastIndexOf("}"));
                    string dir = Settings.cache;
                    switch (type)
                    {
                        case "thumb":
                            dir += "thumbs/";
                            break;
                    }
                    Directory.CreateDirectory(dir);
                    var profiledata = JSON.Serializer.simpleSerializer.Deserialize<Gallery.Profile>(data);
                    filename = dir + name + ".jpg";
                    FileStream thumb = File.OpenWrite(filename);
                    thumb.Write(profiledata.Data, 0, profiledata.Data.Length);
                    thumb.Close();
                    return filename;
                }
                else return "fail";
            }

            public static string GetCommandState(int command_id)
            {
                string command = CreateCommand("get_command_state", command_id);
                string response = Net.SendPost(command);
                string state = string.Empty;
                if (GetError(response) == "none")
                {
                    state = JSON.API.GetData(response, 2);
                    return state;
                    
                }
                else return (GetError(response));

            }

            public static string GetError(string datajson)
            {
                if (datajson.Contains("\"error\""))
                {
                    string error = string.Empty;
                    string temp = datajson.Substring(datajson.IndexOf("error"));
                    temp = temp.Substring(temp.IndexOf("code"));
                    error = temp.Substring(temp.IndexOf(":") +1, temp.IndexOf("}") - temp.IndexOf(":"));
                    error = error.Trim(' ');
                    error += ":";
                    temp = datajson.Substring(datajson.IndexOf("msg"));
                    error += temp.Substring(temp.IndexOf(":") + 1, temp.IndexOf("}") - temp.IndexOf(":"));
                    error = error.Trim('\n');
                    return "error "+error;
                }
                else return "none";
            }

            public static string HashGenerator(string size , int item_id = 0)
            {
                string feed = info.name;
                feed += "-" + size;
                feed += item_id == 0 ? "" : item_id.ToString();
                byte[] feedbyte = Encoding.Unicode.GetBytes(feed);
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(feedbyte);
                    StringBuilder builder = new StringBuilder();

                    foreach (byte b in hash)
                        builder.Append(b.ToString("x2").ToLower());

                    return builder.ToString();
                }
            }


            public struct KeyPair
            {
                public string Key;
                public string Value;
            }
        }
    }
}