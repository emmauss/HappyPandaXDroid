using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using System.Threading;
using Android.Support.V7.View;
using Toolbar = Android.Support.V7.Widget.Toolbar; 

namespace HappyPandaXDroid
{
    [Activity(Label = "HappyPandaXDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity , Android.Support.V7.Widget.SearchView.IOnQueryTextListener
    {
        
        public int count = 1,lastindex = 0;
        //public List<string> lists = new List<string>();
        public static string searchQuery = string.Empty;
        //ArrayAdapter<string> adapter;
        Toolbar toolbar;
        bool IsLoading = false;
        string current_query = "";
        RecyclerView mRecyclerView;
        SwipeRefreshLayout mSwipeLayout;
        ProgressBar mProgressCircle;
        ProgressBar mBottomProgressBar;
        int page = 0;
        int CurrentPage
        {
            get
            {
                return page;
            }
            set
            {
                page = value;
            }
        }
        RecyclerView.LayoutManager mLayoutManager;
        FloatingActionButton fab;
        ListViewAdapter adapter;
        DrawerLayout navDrawer;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Android.Support.V7.App.AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Library";
            

            string[] array = { "hey", "eh" };
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            adapter = new ListViewAdapter(this);


            //set default drawer
            /*mSwipeLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
            mProgressCircle = FindViewById<ProgressBar>(Resource.Id.progress_bar);
            */mBottomProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar_bottom);
            navDrawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //fab = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton1);
            var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navView.NavigationItemSelected += NavView_NavigationItemSelected;
            var navToggle = new ActionBarDrawerToggle
                (this, navDrawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            navDrawer.AddDrawerListener(navToggle);
            SetBottomLoading(false);
            navToggle.SyncState();
            mLayoutManager = new GridLayoutManager(this,2);
            mRecyclerView.SetAdapter(adapter);
            mRecyclerView.SetLayoutManager(mLayoutManager);
            mRecyclerView.AddOnScrollListener(new ScrollListener(this, toolbar));
            //mRecyclerView.SetOnScrollChangeListener(new ScrollChangeListener(this));
           // mSwipeLayout.Refresh += MSwipeLayout_Refresh;
            SetMainLoading(true);
            ThreadStart thrds = new ThreadStart(() =>
            {
                Core.Net.Connect();
                GetTotalCount();
                GetLib();
                RunOnUiThread(() =>
                {
                    SetMainLoading(false);
                    adapter.ResetList();
                    lastindex = Core.Gallery.CurrentList.Count - 1;
                    GetTotalCount();
                });
            });
            Thread thread = new Thread(thrds);
            thread.Start();



        }

        class FABClickListener : Java.Lang.Object, View.IOnClickListener
        {
            public void OnClick(View v)
            {
                
            }
        }

        public class ScrollChangeListener : Java.Lang.Object, RecyclerView.IOnScrollChangeListener
        {
            MainActivity activity;
            public ScrollChangeListener(MainActivity activity)
            {
                this.activity = activity;

            }
            public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
            {
                RecyclerView view = (RecyclerView)v;
                var mLayoutManager = view.GetLayoutManager();
                int totalItemCount = mLayoutManager.ItemCount;
                int visibleCount = mLayoutManager.ChildCount;
                var layoutmanager = (GridLayoutManager)mLayoutManager;
                int lastVisibleItem = layoutmanager.FindLastVisibleItemPosition();
                int firstVisibleItem = layoutmanager.FindFirstVisibleItemPosition();
                if (!activity.IsLoading && totalItemCount - visibleCount <= firstVisibleItem + 1)
                {
                    // End has been reached
                    // Do something
                    activity.SetBottomLoading(true);
                    ThreadStart load = new ThreadStart(activity.NextPage);
                    Thread thread = new Thread(load);
                    thread.Start();
                }

            }
        }

        private void MSwipeLayout_Refresh(object sender, EventArgs e)
        {
            RefreshLibrary();
            void RefreshLibrary()
            {
                SetBottomLoading(true);
                bool success = Core.Gallery.SearchGallery(current_query);
                if(!success)
                   //TODO : create error screen
                CurrentPage = 0;
                adapter.NotifyDataSetChanged();
                GetTotalCount();
                OnRefreshLoadComplete();
            }

            void OnRefreshLoadComplete()
            {
                SetBottomLoading(true);
            }
        }

        private void MRecyclerView_ScrollChange(object sender, View.ScrollChangeEventArgs e)
        {
            int totalItemCount = mLayoutManager.ItemCount;
            int visibleCount = mLayoutManager.ChildCount;
            var layoutmanager = (GridLayoutManager)mLayoutManager;
            int  lastVisibleItem = layoutmanager.FindLastVisibleItemPosition();
            int firstVisibleItem = layoutmanager.FindFirstVisibleItemPosition();
            if (!IsLoading && totalItemCount -visibleCount <= firstVisibleItem+5)
            {
                // End has been reached
                // Do something
                SetBottomLoading(true);
                ThreadStart load = new ThreadStart(NextPage);
                Thread thread = new Thread(load);
                thread.Start();
            }
        }

        public void SetBottomLoading(bool state)
        {
           switch (state)
            {
                case true:
                    mBottomProgressBar.Visibility = ViewStates.Visible;
                    IsLoading = true;
                    break;
                case false:
                    mBottomProgressBar.Visibility = ViewStates.Gone;
                    IsLoading = false;
                    break;
            }
        }

        public void SetMainLoading(bool state)
        {
            switch (state)
            {
                case true:
                   // mRecyclerView.Visibility = ViewStates.Invisible;
                   // mProgressCircle.Visibility = ViewStates.Visible;
                    IsLoading = true;
                    break;
                case false:
                    //mRecyclerView.Visibility = ViewStates.Visible;
                    //mProgressCircle.Visibility = ViewStates.Gone;
                    IsLoading = false;
                    break;
            }
        }

        public void NextPage()
        {
            GetTotalCount();
            if((CurrentPage+1) >=(count/25)){
                RunOnUiThread(() =>
                {
                    Toast to = Toast.MakeText(this, "Reached end of library", ToastLength.Short);
                to.SetGravity(GravityFlags.Bottom, 0, 10);
               
                to.Show();
                SetBottomLoading(false);
            });
            return;
            }
            int newitems = Core.Gallery.NextPage(CurrentPage + 1, current_query);
            if (newitems>0)
            {
                RunOnUiThread(() =>
                {
                    adapter.NotifyItemRangeInserted(lastindex, newitems);
                    lastindex = Core.Gallery.CurrentList.Count - 1;
                    GetTotalCount();
                    CurrentPage++;
                });
                
            }
            RunOnUiThread(() =>
            {
                SetBottomLoading(false);
            });
        }

        public void PreviousPage()
        {
            if (CurrentPage == 0)
            {
                RunOnUiThread(() =>
                {
                    Toast to = Toast.MakeText(this, "Reached beginning of library", ToastLength.Short);
                    to.SetGravity(GravityFlags.Bottom, 0, 10);
                    to.Show();
                    SetBottomLoading(false);
                });
                return;
            }
            int newitems = Core.Gallery.NextPage(CurrentPage + 1, current_query);
            if (newitems > 0)
            {
                RunOnUiThread(() =>
                {
                    adapter.NotifyItemRangeInserted(0, newitems);
                    lastindex = Core.Gallery.CurrentList.Count - 1;
                    GetTotalCount();
                    CurrentPage--;
                });
            }
            RunOnUiThread(() =>
            {
                SetBottomLoading(false);
            });
        }

        public class ScrollListener : RecyclerView.OnScrollListener
        {
            private int toolbarOffset = 0;
            private int toolbarHeight;
            private Toolbar toolbar;
            public MainActivity mactivity;

            public ScrollListener(Context context,Toolbar toolbar)
            {
                mactivity = (MainActivity)context;
                this.toolbar = toolbar;
                int[] actionbar_att = new int[] { Resource.Attribute.actionBarSize };
                Android.Content.Res.TypedArray a = context.ObtainStyledAttributes(actionbar_att);
                toolbarHeight = (int)a.GetDimension(0, 0) + 10;
                a.Recycle();
            }
            

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);
                ClipToolbarOffset();
                OnMoved(toolbarOffset);

                if ((toolbarOffset < toolbarHeight && dy > 0) || (toolbarOffset > 0 && dy < 0))
                {
                    toolbarOffset += dy;
                }

                RecyclerView view = mactivity.mRecyclerView;
                var mLayoutManager = view.GetLayoutManager();
                int totalItemCount = mLayoutManager.ItemCount;
                int visibleCount = mLayoutManager.ChildCount;
                var layoutmanager = (GridLayoutManager)mLayoutManager;
                int lastVisibleItem = layoutmanager.FindLastVisibleItemPosition();
                int firstVisibleItem = layoutmanager.FindFirstVisibleItemPosition();
                if (!mactivity.IsLoading && totalItemCount - visibleCount <= firstVisibleItem + 1)
                {
                    // End has been reached
                    // Do something
                    mactivity.SetBottomLoading(true);
                    ThreadStart load = new ThreadStart(mactivity.NextPage);
                    Thread thread = new Thread(load);
                    thread.Start();
                }
            }

            private void ClipToolbarOffset()
            {
                if (toolbarOffset > toolbarHeight)
                {
                    toolbarOffset = toolbarHeight;
                }
                else if (toolbarOffset < 0)
                {
                    toolbarOffset = 0;
                }
            }

            

            public void OnMoved(int distance)
            {
                toolbar.TranslationY =  -distance;
            }
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

        public  void GetLib()
        {
            Core.Gallery.GetLibrary();
        }

        public async void GetTotalCount()
        {
            count =  await Core.Gallery.GetCount(current_query);
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
            public List<Core.Gallery.GalleryItem> mdata = Core.Gallery.CurrentList;
            Android.Content.Context mcontext;
            public ListViewAdapter(Context context)
            {
                mcontext = context;
            }

            public override int ItemCount {
                get { return mdata.Count; }
            }

            public void ResetList()
            {
                mdata = Core.Gallery.CurrentList;
                this.NotifyDataSetChanged();
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ListViewHolder vh = holder as ListViewHolder;
                vh.gcard.Gallery = mdata[position];
                    try
                    {
                        vh.gcard.Refresh();
                    }catch(Exception ex)
                    {

                    }
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
            current_query = query;
            SetBottomLoading(true);
            bool success = Core.Gallery.SearchGallery(current_query);
            if (success)
            {
                adapter.NotifyDataSetChanged();
                GetTotalCount();
            }
            SetBottomLoading(false);
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
            
            return base.OnOptionsItemSelected(item);
        } 

        public void switchtotest()
        {
            Android.Content.Intent intent = new Android.Content.Intent(this, typeof(test));
            StartActivity(intent);
        }
        
    }

    
}

