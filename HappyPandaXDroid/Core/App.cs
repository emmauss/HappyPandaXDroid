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

namespace HappyPandaXDroid.Core
{
    class App
    {
        public class Settings
        {
            public static prefs appSettings = new prefs();
            public class prefs
            {
                public string serverIP;
            }
            public void InitSettings()
            {
                var settings = Application.Context.GetSharedPreferences("settings", FileCreationMode.Private);
                
                SaveSettings(settings);
            }

            public void RetrieveSettings(ISharedPreferences sharedPref)
            {
                var settings = sharedPref;
                if (settings.All.Count < 1)
                {
                    InitSettings();
                }
                appSettings.serverIP = settings.GetString("server_ip", string.Empty);
            }

            public void SaveSettings(ISharedPreferences sharedPref)
            {
                //var settings = Application.Context.GetSharedPreferences("settings", FileCreationMode.Private);
                var editor = sharedPref.Edit();
                editor.PutString("server_ip", appSettings.serverIP);
                editor.Commit();
                editor.Apply();
            }
        }
    }
}