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
using ProgressView = XamarinBindings.MaterialProgressBar;
using ThreadHandler = HappyPandaXDroid.Core.App.Threading;

namespace HappyPandaXDroid
{
    [Activity(Label = "HPXDroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity , Android.Support.V7.Widget.SearchView.IOnQueryTextListener
    {
        
        public int count = 0,lastindex = 0;
        //public List<string> lists = new List<string>();
        public static string searchQuery = string.Empty;
        //ArrayAdapter<string> adapter;
        Toolbar toolbar;
        bool RootActivity = true;
        bool IsLoading = false;
        Custom_Views.PageSelector mpageSelector;
        string current_query = "";
        RecyclerView mRecyclerView;
        
        ProgressView.MaterialProgressBar mProgressView;
       
        RefreshLayout.RefreshLayout mRefreshLayout;
        //ProgressBar mBottomProgressBar;
        int page = 0;
        public int CurrentPage
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
        Clans.Fab.FloatingActionMenu fam;
        Clans.Fab.FloatingActionButton mRefreshFab;
        Clans.Fab.FloatingActionButton mJumpFab;
        ListViewAdapter adapter;
        DrawerLayout navDrawer;
        public DialogEventListener dialogeventlistener;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Android.Support.V7.App.AppCompatDelegate.CompatVectorFromResourcesEnabled = true;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            ThreadHandler.InitScheduler();
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Library";


            string[] array = { "hey", "eh" };
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            adapter = new ListViewAdapter(this);

            mRefreshLayout = FindViewById<RefreshLayout.RefreshLayout>(Resource.Id.refresh_layout);
            mProgressView = FindViewById<ProgressView.MaterialProgressBar>(Resource.Id.progress_view);
            mProgressView.Visibility = ViewStates.Gone;
            navDrawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            mRefreshFab = FindViewById<Clans.Fab.FloatingActionButton>(Resource.Id.fabRefresh);
            mJumpFab = FindViewById<Clans.Fab.FloatingActionButton>(Resource.Id.fabJumpTo);
            mJumpFab.SetImageResource(Resource.Drawable.v_go_to_dark_x24);
            mRefreshFab.SetImageResource(Resource.Drawable.v_refresh_dark_x24);
            fam = FindViewById<Clans.Fab.FloatingActionMenu>(Resource.Id.fam);
            var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navView.NavigationItemSelected += NavView_NavigationItemSelected;
            FABClickListener fabclick = new FABClickListener(this);
            mJumpFab.SetOnClickListener(fabclick);
            mRefreshFab.SetOnClickListener(fabclick);
            var navToggle = new ActionBarDrawerToggle
               (this, navDrawer, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);
            navDrawer.AddDrawerListener(navToggle);
            SetBottomLoading(false);
            navToggle.SyncState();
            mLayoutManager = new GridLayoutManager(this, 2);
            mRecyclerView.SetAdapter(adapter);
            mRefreshLayout.OnHeaderRefresh += MRefreshLayout_OnHeaderRefresh;
            mRefreshLayout.OnFooterRefresh += MRefreshLayout_OnFooterRefresh;
            mRecyclerView.SetLayoutManager(mLayoutManager);
            mRecyclerView.AddOnScrollListener(new ScrollListener(this, toolbar));
            if (!Core.Net.Connect().Contains("fail"))
            {
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
                        SetMainLoading(false);
                        lastindex = Core.Gallery.CurrentList.Count - 1;
                        GetTotalCount();
                    });
                });
                Thread thread = new Thread(thrds);
                thread.Start();
            }

            mpageSelector = new Custom_Views.PageSelector();
            dialogeventlistener = new DialogEventListener();

            // adapter.ItemClick += OnItemClick;
            
        }

        private void MRefreshLayout_OnFooterRefresh(object sender, EventArgs e)
        {
            
            SetBottomLoading(true);
            ThreadStart load = new ThreadStart(NextPage);
            Thread thread = new Thread(load);
            thread.Start();
        }

        protected override void OnDestroy()
        {
           
            base.OnDestroy();
            Android.Support.V4.App.ActivityCompat.FinishAffinity(this);
            
        }
        private void MRefreshLayout_OnHeaderRefresh(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Refresh();
                mRefreshLayout.HeaderRefreshing = false;
                mRefreshLayout.FooterRefreshing = false;
            });
        }

        public class DialogEventListener : Custom_Views.PageSelector.NoticeDialogListener
        {
            public void OnDialogNegativeClick(DialogFragment dialog)
            {
                //close dialog
            }

            public void OnDialogPositiveClick(DialogFragment dialog)
            {
                //jump to page
            }
        }

        class FABClickListener : Java.Lang.Object, View.IOnClickListener
        {
            MainActivity main;
            public FABClickListener(MainActivity main)
            {
                this.main = main;
            }
            public void OnClick(View v)
            {
                var fab = (Clans.Fab.FloatingActionButton)v;
                switch (fab.Id)
                {
                    case Resource.Id.fabJumpTo:
                        main.mpageSelector.Show(main.FragmentManager,"PageSelecter");
                        break;
                    case Resource.Id.fabRefresh:
                        main.Refresh();
                        break;
                }
            }
        }

        public async void Refresh()
        {
            RunOnUiThread(() =>
            {
                SetMainLoading(true);
            });
            bool success = await Core.Gallery.SearchGallery(current_query);
            if (!success)
                //TODO : create error screen
                CurrentPage = 0;
            RunOnUiThread(() =>
            {
                adapter.NotifyDataSetChanged();
                adapter.ResetList();
                SetMainLoading(false);
                if (Core.Gallery.CurrentList.Count>0)
                mRecyclerView.ScrollToPosition(0);
                GetTotalCount();
                
            });
        }

        public class ScrollChangeListener : Java.Lang.Object ,View.IOnScrollChangeListener
        {
            Clans.Fab.FloatingActionMenu fam;
            public ScrollChangeListener(Clans.Fab.FloatingActionMenu fam)
            {
                this.fam = fam;
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
                /*if (!activity.IsLoading && totalItemCount - visibleCount <= firstVisibleItem + 1)
                {
                    // End has been reached
                    // Do something
                    activity.SetBottomLoading(true);
                    ThreadStart load = new ThreadStart(activity.NextPage);
                    Thread thread = new Thread(load);
                    thread.Start();
                }*/

                if (oldScrollY > scrollY + 10)
                {
                    fam.Visibility = ViewStates.Invisible;
                }
                else if (oldScrollY < scrollY - 10)
                {
                    fam.Visibility = ViewStates.Visible;
                }
            }
        }
        private void MSwipeLayout_Refresh(object sender, EventArgs e)
        {
            RefreshLibrary();
            void RefreshLibrary()
            {
                SetBottomLoading(true);
                Refresh();
                GetTotalCount();
                OnRefreshLoadComplete();
            }

            void OnRefreshLoadComplete()
            {
                SetBottomLoading(true);
            }
        }

        

        public void SetBottomLoading(bool state)
        {
           switch (state)
            {
                case true:
                   // mBottomProgressBar.Visibility = ViewStates.Visible;
                    IsLoading = true;
                    break;
                case false:
                   // mBottomProgressBar.Visibility = ViewStates.Gone;
                    IsLoading = false;
                    break;
            }
        }

        public void SetMainLoading(bool state)
        {
            switch (state)
            {
                case true:
                    mProgressView.Visibility = ViewStates.Visible;
                    mRefreshLayout.Visibility = ViewStates.Gone;
                    IsLoading = true;
                    break;
                case false:
                    mProgressView.Visibility = ViewStates.Gone;
                    mRefreshLayout.Visibility = ViewStates.Visible;
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
                    mRefreshLayout.HeaderRefreshing = false;
                    mRefreshLayout.FooterRefreshing = false;
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
                mRefreshLayout.HeaderRefreshing = false;
                mRefreshLayout.FooterRefreshing = false;
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

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                base.OnScrollStateChanged(recyclerView, newState);
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);
               /* try
                {
                    ClipToolbarOffset();
                    OnMoved(toolbarOffset);

                    if ((toolbarOffset < toolbarHeight && dy > 0) || (toolbarOffset > 0 && dy < 0))
                    {
                        toolbarOffset += dy;
                    }

                    RecyclerView view = mactivity.mRecyclerView;
                    if (!view.CanScrollVertically(1))
                    {
                        // End has been reached
                        // Do something
                         mactivity.SetBottomLoading(true);
                         ThreadStart load = new ThreadStart(mactivity.NextPage);
                         Thread thread = new Thread(load);
                         thread.Start();
                        //Toast.MakeText(mactivity, "You have reached to the bottom!", ToastLength.Short).Show();
                    }


                    if (dy > 0)
                    {
                        mactivity.fam.Visibility = ViewStates.Invisible;
                    }
                    else if (dy < 0)
                    {
                        mactivity.fam.Visibility = ViewStates.Visible;
                    }
                }
                catch(Exception ex)
                {

                }*/
            }

            async Task<bool> HasReachedBottom()
            {
                RecyclerView view = mactivity.mRecyclerView;
                var mLayoutManager = view.GetLayoutManager();
                int totalItemCount = mLayoutManager.ItemCount;
                int visibleCount = mLayoutManager.ChildCount;
                var layoutmanager = (GridLayoutManager)mLayoutManager;
                int lastVisibleItem = layoutmanager.FindLastVisibleItemPosition();
                int firstVisibleItem = layoutmanager.FindFirstVisibleItemPosition();
                
                if (!mactivity.IsLoading && (totalItemCount - visibleCount) <= firstVisibleItem)
                {
                    if (!view.CanScrollVertically(1))
                        // End has been reached
                        // Do something
                        /* mactivity.SetBottomLoading(true);
                         ThreadStart load = new ThreadStart(mactivity.NextPage);
                         Thread thread = new Thread(load);
                         thread.Start();*/
                        Toast.MakeText(mactivity, "You have reached to the bottom!", ToastLength.Short).Show();
                    return true;
                }

                return false;
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
                // gcard.Click += (s, e) => clicklistener(base.AdapterPosition);
               gcard.SetOnClickListener(new GalleryCardClickListener());
            }

            class GalleryCardClickListener : Java.Lang.Object, View.IOnClickListener
            {
                Custom_Views.GalleryCard card;
                public void OnClick(View v)
                {
                    card = (Custom_Views.GalleryCard)v;
                    Intent intent = new Intent(card.Context, typeof(GalleryActivity));
                    string gallerystring = Core.JSON.Serializer.simpleSerializer.Serialize(card.Gallery);
                    intent.PutExtra("gallery", gallerystring);
                    card.Context.StartActivity(intent);
                }
            }


        }

        public class ListViewAdapter : RecyclerView.Adapter
        {

            public EventHandler<int> ItemClick;

            void OnClick(int position)
            {
                if (ItemClick != null)
                    ItemClick(this, position);
            }

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
                //vh.gcard.SetOnClickListener(new GalleryCardClickListener());
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
            searchView.ClearFocus();
            current_query = query;
            Refresh();
            return true;
        }

        Android.Support.V7.Widget.SearchView searchView;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.gallerySearch, menu);
            var search = toolbar.Menu.FindItem(Resource.Id.search);
            searchView = (Android.Support.V7.Widget.SearchView)search.ActionView;
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

