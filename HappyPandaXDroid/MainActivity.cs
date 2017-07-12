using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Toolbar = Android.Support.V7.Widget.Toolbar; 

namespace HappyPandaXDroid
{
    [Activity(Label = "HappyPandaXDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity , Android.Support.V7.Widget.SearchView.IOnQueryTextListener
    {
        
        public int count = 1;
        public List<string> lists = new List<string>();
        public static string searchQuery = string.Empty;
        //ArrayAdapter<string> adapter;
        Toolbar toolbar;
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        ListViewAdapter adapter;
        DrawerLayout navDrawer;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "hey listen";
            Button button = FindViewById<Button>(Resource.Id.addbutton);

            string[] array = { "hey", "eh" };
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            adapter = new ListViewAdapter(lists,this);
            mRecyclerView.SetAdapter(adapter);

            //set default drawer
            navDrawer = FindViewById<DrawerLayout>(Resource.Id.nav_drawer);
            var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navView.NavigationItemSelected += NavView_NavigationItemSelected;
            var navToggle = new ActionBarDrawerToggle
                (this, navDrawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            navDrawer.AddDrawerListener(navToggle);
            navToggle.SyncState();
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);


            lists.Add("hey");

            button.Click += (o, e) =>
            {
                addText();
            };
        }

        private void NavView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {

            var item = e.MenuItem;
            Android.Content.Intent intent;
            switch (item.ItemId)
            {
                case Resource.Id.action_setting:
                    intent = new Android.Content.Intent(this, typeof(SettingsActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.action_filemanager:
                    intent = new Android.Content.Intent(this, typeof(FileBrowser));
                    StartActivity(intent);
                    break;
                case Resource.Id.action_home:
                    navDrawer.CloseDrawers();
                    break;
            }

        }

        public class ListViewHolder : RecyclerView.ViewHolder
        {
            public Custom_Views.GalleryCard gcard;
            public ListViewHolder(View itemView) : base(itemView)
            {
                gcard = (Custom_Views.GalleryCard)itemView;
                
            }
        }

        public class ListViewAdapter : RecyclerView.Adapter
        {
            public List<string> mdata;
            Android.Content.Context mcontext;
            public ListViewAdapter(List<string> data,Android.Content.Context context)
            {
                mdata = data;
            }

            public override int ItemCount {
                get { return mdata.Count; }
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ListViewHolder vh = holder as ListViewHolder;
                vh.gcard.text.Text = mdata[position];
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                /*View itemview = Android.Views.LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.galleryCard, parent, false);*/
                View itemview = new Custom_Views.GalleryCard(mcontext);
                ListViewHolder vh = new ListViewHolder(itemview);
                return vh;
            }
        }
        


            public bool OnQueryTextChange(string newText)
            {
                //throw new NotImplementedException();
                return true;
            }

        public bool OnQueryTextSubmit(string query)
        {
            searchQuery = query;
            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.gallerySearch, menu);
            var search = toolbar.Menu.FindItem(Resource.Id.search);
            var searchView = (Android.Support.V7.Widget.SearchView)search.ActionView;
            searchView.SetOnQueryTextListener(this);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            /* Toast.MakeText(this, "Action selected: " + item.TitleFormatted,
                 ToastLength.Short).Show();*/
            return base.OnOptionsItemSelected(item);
        } 

        public void switchtotest()
        {
            Android.Content.Intent intent = new Android.Content.Intent(this, typeof(test));
            StartActivity(intent);
        }
        public void addText()
        {
            try
            {
                
                TextView text = new TextView(this);
                text.Text = "hello " + count.ToString();
                count++;
                lists.Add(text.Text);
                RunOnUiThread(adapter.NotifyDataSetChanged);
               
            }
            catch(Exception ex)
            {

            }
            
        }
    }

    
}

