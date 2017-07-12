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

namespace HappyPandaXDroid.Core
{
    class JSON
    {
        public  class Serializer
        {
            public static Serializer simpleSerializer = new Serializer();
            public void Serialize<T>(ref string serialized_text, T obj )
            {
                serialized_text = JsonConvert.SerializeObject(obj, obj.GetType(), null);
            }

            public void Deserialize<T>(string serialized_string,ref T obj)
            {
                
                obj = (T)JsonConvert.DeserializeObject(serialized_string);
            }
        }
    }
}