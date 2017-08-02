using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace HappyPandaXDroid
{
    [Activity(Label = "Settings")]
    public class SettingsActivity : AppCompatActivity
    {
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
                //listener = new changelistener(this);

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
                Core.Net.Connect();
                base.OnDestroy();
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                Preference pref = FindPreference(key);

                if (pref is EditTextPreference)
                {
                    EditTextPreference editp = (EditTextPreference)pref;
                    var edit = sharedPreferences.Edit();
                    edit.PutString(key, editp.Text);
                    editp.Summary = editp.Text;
                    edit.Commit();
                }
            }


        }

    }
}