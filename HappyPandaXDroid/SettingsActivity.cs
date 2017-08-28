using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Preferences;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Plugin.Settings;
using System.IO;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using NLog.Config;

using NLog;

namespace HappyPandaXDroid
{
    [Activity(Label = "Settings", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class SettingsActivity : AppCompatActivity
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings);
            Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            var newFragment = new SettingsFragments();
            var ft = FragmentManager.BeginTransaction();
            ft.Add(Resource.Id.fragment_container, newFragment);
            ft.Commit();

            logger.Info("Settings Loaded");
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public  class SettingsFragments : PreferenceFragment, 
            ISharedPreferencesOnSharedPreferenceChangeListener
        {

            Custom_Views.OptionDialogPreference cachedialog;
            private static Logger logger = LogManager.GetCurrentClassLogger();
            //Core.App.Settings set = new Core.App.Settings();
            ISharedPreferences sharedPreferences;
            ISharedPreferencesOnSharedPreferenceChangeListener listener;
            Preference pref;
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                // Create your application here
                AddPreferencesFromResource(Resource.Xml.preferences);

                sharedPreferences = PreferenceScreen.SharedPreferences;
                for (int i = 0; i < PreferenceScreen.PreferenceCount; i++)
                {
                    setSummary(PreferenceScreen.GetPreference(i));
                }
                sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                cachedialog = (Custom_Views.OptionDialogPreference)FindPreference("cachedialog");
                Task.Run(() =>
                {
                    var h = new Handler(Looper.MainLooper);
                    h.Post(() =>
                    {
                        
                        cachedialog.Title = "Local Cache";
                        cachedialog.Summary = Math.Round((double)(Core.Media.Cache.GetCacheSize()) / (1024 * 1024), 2).ToString() + " MB";
                    });
                });
                cachedialog.OnPositiveClick += Cachedialog_OnPositiveClick;
               
                
                //cachedialog.Dialog.DismissEvent += Dialog_DismissEvent;
                //listener = new changelistener(this);

            }

            private async void Cachedialog_OnPositiveClick(object sender, EventArgs e)
            {
               bool result = await Core.Media.Cache.ClearCache();
                Task.Run(() =>
                {
                    var h = new Handler(Looper.MainLooper);
                    h.Post(() =>
                    {
                        cachedialog.Summary = Math.Round((double)(Core.Media.Cache.GetCacheSize()) / (1024 * 1024), 2).ToString() + " MB";
                    });
                });
            }

            public override void OnResume()
            {

                base.OnResume();
                for(int i = 0; i < PreferenceScreen.PreferenceCount; i++)
                {
                    setSummary(PreferenceScreen.GetPreference(i));
                }
                sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

            }

            

            private void setSummary(Preference pref)
            {
                if (pref is EditTextPreference) {
                    updateSummary((EditTextPreference)pref);
                } else if (pref is ListPreference) {
                    updateSummary((ListPreference)pref);
                } else if (pref is MultiSelectListPreference) {
                    updateSummary((MultiSelectListPreference)pref);
                }
            }

            private void updateSummary(MultiSelectListPreference pref)
            {
                //pref.setSummary(Arrays.toString(pref.getValues().toArray()));
            }

            private void updateSummary(ListPreference pref)
            {
                pref.Summary=pref.Value;
            }

            private void updateSummary(EditTextPreference preference)
            {
                preference.Summary = preference.Text;
            }
            public override void OnPause()
            {
                sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                base.OnPause();
            }

            public override void OnDestroy()
            {
                
                base.OnDestroy();
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                Preference pref = FindPreference(key);
                logger.Info("Change Preference : {0}", key);
                if (pref is EditTextPreference)
                {

                    EditTextPreference editp = (EditTextPreference)pref;
                    var edit = sharedPreferences.Edit();
                    edit.PutString(key, editp.Text);
                    editp.Summary = editp.Text;
                    edit.Commit();
                    if (key == "server_ip" || key == "server_port" || key == "webclient_port")
                    {
                        Core.App.Settings.Refresh = true;
                    }
                }
                else if (pref is TwoStatePreference)
                {
                    var check = (TwoStatePreference)pref;
                    var edit = sharedPreferences.Edit();
                    edit.PutBoolean(key, check.Checked);
                    
                    edit.Commit();
                    
                    if (key == "enable_debugging")
                    {
                        switch (check.Checked)
                        {
                            case true:
                                NLog.Targets.FileTarget target = new NLog.Targets.FileTarget("log");
                                if (!Directory.Exists(Core.App.Settings.Log))
                                {
                                    Directory.CreateDirectory(Core.App.Settings.Log);
                                }
                                string logfile = Core.App.Settings.Log + DateTime.Now.ToShortDateString().Replace("/", "-") + " - "
                                    + DateTime.Now.ToShortTimeString().Replace(":", ".") + " - log.txt";
                                target.FileName = logfile;
                                target.FileNameKind = NLog.Targets.FilePathKind.Absolute;
                                LogManager.Configuration = new XmlLoggingConfiguration("assets/NLog.config");
                                LogManager.Configuration.AddTarget(target);

                                LogManager.Configuration.AddRuleForAllLevels(target, "*");
                                //LogManager.ReconfigExistingLoggers();
                                break;
                            case false:
                                LogManager.Configuration = null;
                                break;
                        }
                    }
                }
            }


        }

    }
}