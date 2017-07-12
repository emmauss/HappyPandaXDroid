using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.Preferences;
using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Toolbar = Android.Support.V7.Widget.Toolbar;


namespace HappyPandaXDroid
{
    
    public class SettingsFragment : PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    { 
        ISharedPreferences sharedPreferences;
        ISharedPreferencesOnSharedPreferenceChangeListener listener;
        Preference pref;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            AddPreferencesFromResource(Resource.Xml.preferences);
            sharedPreferences = Application.Context.GetSharedPreferences
                ("settings", FileCreationMode.Private);
            //listener = new changelistener(this);

        }
        public override void OnResume()
        {

            base.OnResume();
            

            sharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
            
         }

        public void updateSummary(EditTextPreference pref)
        {

        }

        public override void OnPause()
        {
            sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
            base.OnPause();
        }



        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            Preference pref = FindPreference(key);

            if (pref is EditTextPreference)
            {
                EditTextPreference editp = (EditTextPreference)pref;
                editp.Summary = editp.Text;
            }
        }
        

    }
}