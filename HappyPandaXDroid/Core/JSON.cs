using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyPandaXDroid.Core
{
    class JSON
    {
        public  class Serializer
        {
            public static Serializer simpleSerializer = new Serializer();
            public string Serialize<T>( T obj )
            {
                return JsonConvert.SerializeObject(obj, obj.GetType(), null);
            }

            

            public T Deserialize<T>(string serialized_string)
            {
                
                return (T)JsonConvert.DeserializeObject<T>(serialized_string);
            }

            public List<T> DeserializeToList<T>(string serialized_string)
            {

                var array = JArray.Parse(serialized_string);

                List<T> objectsList = new List<T>();
                
                foreach (var item in array)

                {
                    try

                    {
                        // CorrectElements
                        objectsList.Add(item.ToObject<T>());
                    }

                    catch (Exception ex)
                    {
                    }
                }
                return objectsList;
            }
        }

        public class API
        {
            public static void PushKey(ref List<Tuple<string, string>> request, string name, string value)
            {
                request.Add(new Tuple<string, string>(name,value) );
            }

            public static string ParseToString(List<Tuple<string, string>> request)
            {
                string jsonstring = string.Empty;
                jsonstring += "{ " + System.Environment.NewLine;
                foreach (Tuple<string, string> s in request)
                {
                    jsonstring += '"' + s.Item1 + '"' + " : ";
                    if (!s.Item2.StartsWith("{") && !s.Item2.StartsWith("["))
                    {
                        if (s.Item2.Contains("<int>"))
                            jsonstring += s.Item2.Replace("<int>", "") + "," + System.Environment.NewLine;
                        else if (s.Item2.Contains("<bool>"))
                            jsonstring += s.Item2.Replace("<bool>", "") + "," + System.Environment.NewLine;
                        else
                            jsonstring += '"' + s.Item2 + '"' + "," + System.Environment.NewLine;
                    }
                    else
                        jsonstring += s.Item2 + "," + System.Environment.NewLine;

                }
               
                jsonstring=jsonstring.Remove(jsonstring.LastIndexOf(","),1);
                jsonstring += "}";

                return jsonstring;
            }

            public static string GetData(string jsonstring, int level = 1)
            {
                //recursive get data segment
                string data = jsonstring;
                string temp = data;
                for (int i = 0; i<level; i++)
                {
                    
                    int startindex = temp.IndexOf("data");
                    temp = temp.Substring(startindex);
                    startindex = temp.IndexOf(':')+1;
                    int length = temp.LastIndexOf("}") - (startindex);
                    temp = temp.Substring(startindex, length);
                }
                return temp;
                
            }
            
        }
    }
}