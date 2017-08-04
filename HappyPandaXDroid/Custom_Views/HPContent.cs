using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using System.Threading;
using Android.Support.V7.View;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ProgressView = XamarinBindings.MaterialProgressBar;
using ThreadHandler = HappyPandaXDroid.Core.App.Threading;

namespace HappyPandaXDroid.Custom_Views
{
    public class HPContent : FrameLayout
    {
        View ContentView;
        public Custom_Views.PageSelector mpageSelector;
        RecyclerView mRecyclerView;
        bool IsRefreshing = false;
        ProgressView.MaterialProgressBar mProgressView;
        public int count = 0, lastindex = 0;
        RefreshLayout.RefreshLayout mRefreshLayout;
        RecyclerView.LayoutManager mLayoutManager;
        ListViewAdapter adapter;
        bool IsLoading = false;
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

       string current_query = string.Empty;
        public String Current_Query
        {
            get
            {
                return current_query;
            }
            set
            {
                current_query = value;
                SetMainLoading(true);
                Refresh();
                SetMainLoading(false);
            }
        }
        DrawerLayout navDrawer;
        public DialogEventListener dialogeventlistener;
        public HPContent(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public HPContent(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {

            ContentView = Inflate(Context, Resource.Layout.HPContent, this);
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            adapter = new ListViewAdapter(this.Context);

            mRefreshLayout = FindViewById<RefreshLayout.RefreshLayout>(Resource.Id.refresh_layout);
            mProgressView = FindViewById<ProgressView.MaterialProgressBar>(Resource.Id.progress_view);
            mProgressView.Visibility = ViewStates.Gone;
            
           
            SetBottomLoading(false);
            mLayoutManager = new GridLayoutManager(this.Context, 2);
            mRecyclerView.SetAdapter(adapter);
            /*mRefreshLayout.OnHeaderRefresh += MRefreshLayout_OnHeaderRefresh;
            mRefreshLayout.OnFooterRefresh += MRefreshLayout_OnFooterRefresh;*/
            mRefreshLayout.SetOnRefreshListener(new OnRefreshListener(this));
            mRecyclerView.SetLayoutManager(mLayoutManager);
            SetMainLoading(true);
            ThreadStart thrds = new ThreadStart(() =>
            {

                if (!Core.Net.Connect().Contains("fail"))
                {
                    {
                        GetTotalCount();
                        GetLib();
                        var h = new Handler(Looper.MainLooper);
                        h.Post(() =>
                        {
                            SetMainLoading(false);
                            adapter.ResetList();
                            SetMainLoading(false);
                            lastindex = Core.Gallery.CurrentList.Count - 1;
                            GetTotalCount();
                        });

                    }
                }
                else
                {
                    var h = new Handler(Looper.MainLooper);
                    h.Post(() =>
                    {
                        SetMainLoading(false);
                    });
                }
            });
            Thread thread = new Thread(thrds);
            thread.Start();
            mpageSelector = new Custom_Views.PageSelector();
            dialogeventlistener = new DialogEventListener();
        }

        private void MRefreshLayout_OnFooterRefresh(object sender, EventArgs e)
        {

            SetBottomLoading(true);
            ThreadStart load = new ThreadStart(NextPage);
            Thread thread = new Thread(load);
            thread.Start();
        }

        private void MRefreshLayout_OnHeaderRefresh(object sender, EventArgs e)
        {
            if(!IsRefreshing)
            Task.Run(async () =>
            {
                IsRefreshing = true;
                await Task.Delay(10);
                Refresh();
                IsRefreshing = false;
                mRefreshLayout.HeaderRefreshing = false;
                mRefreshLayout.FooterRefreshing = false;
            });
        }

        class OnRefreshListener : RefreshLayout.RefreshLayout.IOnRefreshListener
        {
            HPContent content;
            public OnRefreshListener(HPContent content)
            {
                this.content = content;
            }
            public void OnFooterRefresh()
            {
                content.SetBottomLoading(true);
                ThreadStart load = new ThreadStart(content.NextPage);
                Thread thread = new Thread(load);
                thread.Start();
            }

            public void OnHeaderRefresh()
            {
                if (!content.IsRefreshing)
                    Task.Run(async () =>
                    {
                        content.IsRefreshing = true;
                        await Task.Delay(10);
                        content.Refresh();
                        content.IsRefreshing = false;
                        content.mRefreshLayout.HeaderRefreshing = false;
                        content.mRefreshLayout.FooterRefreshing = false;
                    });
            }
        }

        public void GetLib()
        {
            Core.Gallery.GetLibrary();
        }

        public async void GetTotalCount()
        {
            count = await Core.Gallery.GetCount(Current_Query);
        }

        public async void Refresh()
        {
            var h = new Handler(Looper.MainLooper);
            bool success = await Core.Gallery.SearchGallery(Current_Query);
            if (!success)
                //TODO : create error screen
                CurrentPage = 0;
            h.Post(() =>
            {
                adapter.NotifyDataSetChanged();
                adapter.ResetList();
                if (Core.Gallery.CurrentList.Count > 0)
                    mRecyclerView.ScrollToPosition(0);
                GetTotalCount();

            });
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
            var h = new Handler(Looper.MainLooper);
            if ((CurrentPage + 1) >= (count / 25))
            {
                h.Post(() =>
                {
                    Toast to = Toast.MakeText(this.Context, "Reached end of library", ToastLength.Short);
                    to.SetGravity(GravityFlags.Bottom, 0, 10);

                    to.Show();
                    SetBottomLoading(false);
                    mRefreshLayout.HeaderRefreshing = false;
                    mRefreshLayout.FooterRefreshing = false;
                });
                return;
            }
            int newitems = Core.Gallery.NextPage(CurrentPage + 1, Current_Query);
            if (newitems > 0)
            {
                h.Post(() =>
                {
                    adapter.NotifyItemRangeInserted(lastindex, newitems);
                    lastindex = Core.Gallery.CurrentList.Count - 1;
                    GetTotalCount();
                    CurrentPage++;
                });

            }
            h.Post(() =>
            {
                mRefreshLayout.HeaderRefreshing = false;
                mRefreshLayout.FooterRefreshing = false;
                SetBottomLoading(false);
            });
        }

        public void PreviousPage()
        {
            var h = new Handler(Looper.MainLooper);
            if (CurrentPage == 0)
            {
                h.Post(() =>
                {
                    Toast to = Toast.MakeText(this.Context, "Reached beginning of library", ToastLength.Short);
                    to.SetGravity(GravityFlags.Bottom, 0, 10);
                    to.Show();
                    SetBottomLoading(false);
                });
                return;
            }
            int newitems = Core.Gallery.NextPage(CurrentPage + 1, Current_Query);
            if (newitems > 0)
            {
                h.Post(() =>
                {
                    adapter.NotifyItemRangeInserted(0, newitems);
                    lastindex = Core.Gallery.CurrentList.Count - 1;
                    GetTotalCount();
                    CurrentPage--;
                });
            }
            h.Post(() =>
            {
                SetBottomLoading(false);
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

            public override int ItemCount
            {
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
                }
                catch (Exception ex)
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
        public class ScrollChangeListener : Java.Lang.Object, View.IOnScrollChangeListener
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

        public class ScrollListener : RecyclerView.OnScrollListener
        {
            private int toolbarOffset = 0;
            private int toolbarHeight;
            private Toolbar toolbar;
            public HPContent mactivity;

            public ScrollListener(HPContent context, Toolbar toolbar)
            {
                mactivity = context;
                this.toolbar = toolbar;
                int[] actionbar_att = new int[] { Resource.Attribute.actionBarSize };
                Android.Content.Res.TypedArray a = context.Context.ObtainStyledAttributes(actionbar_att);
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

                /*if (!mactivity.IsLoading && (totalItemCount - visibleCount) <= firstVisibleItem)
                {
                    if (!view.CanScrollVertically(1))
                        // End has been reached
                        // Do something
                        /* mactivity.SetBottomLoading(true);
                         ThreadStart load = new ThreadStart(mactivity.NextPage);
                         Thread thread = new Thread(load);
                         thread.Start();*/
                /*Toast.MakeText(mactivity, "You have reached to the bottom!", ToastLength.Short).Show();
            return true;
        }*/

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
                toolbar.TranslationY = -distance;
            }
        }


    }
}