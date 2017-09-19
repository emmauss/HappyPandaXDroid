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
using NLog;

namespace HappyPandaXDroid.Custom_Views
{
    public class HPContent : FrameLayout
    {
        View ContentView;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public Custom_Views.PageSelector mpageSelector;
        RecyclerView mRecyclerView;
        bool IsRefreshing = false;
        public string activityName;
        public int activityId;
        bool initialized = false;
        ProgressView.MaterialProgressBar mProgressView;
        public int count = 0, lastindex = 0;
        RefreshLayout.RefreshLayout mRefreshLayout;
        RecyclerView.LayoutManager mLayoutManager;
        FrameLayout mErrorFrame;
        ImageView mErrorImage;
        TextView mErrorText;
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

        int columns = 0;
        private void Initialize()
        {
            logger.Info("Initializing HPContent");
            ContentView = Inflate(Context, Resource.Layout.HPContent, this);
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            adapter = new ListViewAdapter(this.Context);

            mRefreshLayout = FindViewById<RefreshLayout.RefreshLayout>(Resource.Id.refresh_layout);
            mProgressView = FindViewById<ProgressView.MaterialProgressBar>(Resource.Id.progress_view);
            mProgressView.Visibility = ViewStates.Gone;
            mErrorFrame = FindViewById<FrameLayout>(Resource.Id.error_frame);
            mErrorFrame.Visibility = ViewStates.Gone;
            mErrorImage = FindViewById<ImageView>(Resource.Id.error_image);
            mErrorImage.SetImageResource(Resource.Drawable.big_weird_face);
            mErrorText = FindViewById<TextView>(Resource.Id.error_text);
            mErrorText.Text = "Error";
            mErrorImage.Click += MErrorFrame_Click;
            SetBottomLoading(false);

            if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
                columns = 2;
            else
                columns = 1;
            mLayoutManager = new GridLayoutManager(this.Context,columns);
            mRecyclerView.SetAdapter(adapter);
            /*mRefreshLayout.OnHeaderRefresh += MRefreshLayout_OnHeaderRefresh;
            mRefreshLayout.OnFooterRefresh += MRefreshLayout_OnFooterRefresh;*/
            mRefreshLayout.SetOnRefreshListener(new OnRefreshListener(this));
            mRecyclerView.SetLayoutManager(mLayoutManager);
            SetMainLoading(true);
            mpageSelector = new Custom_Views.PageSelector();
            dialogeventlistener = new DialogEventListener();
            initialized = true;
            logger.Info("HPContent Initialized");
        }

        public void OrientationChanged(Android.Content.Res.Orientation orientation)
        {
            switch (orientation)
            {
                case Android.Content.Res.Orientation.Landscape:
                    columns = 2;
                    break;
                default:
                    columns = 1;
                    break;
            }
            mLayoutManager = new GridLayoutManager(this.Context, columns);
            mRecyclerView.SetLayoutManager(mLayoutManager);
        }

        public class AutoFitGridLayout : GridLayoutManager
        {
            private int mColumnWidth;
            private bool mColumnWidthChanged = true;

            public AutoFitGridLayout(Context context, int columnWidth) : base(context,1)
            {
                /* Initially set spanCount to 1, will be changed automatically later. */
                setColumnWidth(checkedColumnWidth(context, columnWidth));
            }

            public AutoFitGridLayout(Context context, int columnWidth, int orientation, bool reverseLayout) : base (context, 1, orientation, reverseLayout)
            {
                /* Initially set spanCount to 1, will be changed automatically later. */

                setColumnWidth(checkedColumnWidth(context, columnWidth));
            }

            private int checkedColumnWidth(Context context, int columnWidth)
            {
                if (columnWidth <= 0)
                {
                    /* Set default columnWidth value (48dp here). It is better to move this constant
                    to static constant on top, but we need context to convert it to dp, so can't really
                    do so. */
                    columnWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 48,
                            context.Resources.DisplayMetrics);
                }
                return columnWidth;
            }

            public void setColumnWidth(int newColumnWidth)
            {
                if (newColumnWidth > 0 && newColumnWidth != mColumnWidth)
                {
                    mColumnWidth = newColumnWidth;
                    mColumnWidthChanged = true;
                }
            }

            public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
            {
                int width = Width;
                int height = Height;
                if (mColumnWidthChanged && mColumnWidth > 0 && width > 0 && height > 0)
                {
                    int totalSpace;
                    if (Orientation == Vertical)
                    {
                        totalSpace = width - PaddingRight - PaddingLeft;
                    }
                    else
                    {
                        totalSpace = height - PaddingTop - PaddingBottom;
                    }
                    int spanCount = Math.Max(1, totalSpace / mColumnWidth);
                    SpanCount = spanCount;
                    mColumnWidthChanged = false;
                }
                base.OnLayoutChildren(recycler, state);
            }
        }


        public void InitLibrary()
        {
            
            ThreadHandler.Thread thread = ThreadHandler.CreateThread(() =>
             {
                 while (!initialized)
                 {
                     Thread.Sleep(100);
                 }
                 if (Core.Net.Connect())
                 {
                     {
                         logger.Info("Getting Library");
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
                         SetError(true);
                     });
                 }
             }, activityId, activityName);
            ThreadHandler.StartThread(thread);
            ThreadHandler.Schedule(thread);
        }

        private void MErrorFrame_Click(object sender, EventArgs e)
        {
            SetMainLoading(true);
            Refresh();
        }

        private void MRefreshLayout_OnFooterRefresh(object sender, EventArgs e)
        {

            SetBottomLoading(true);
            ThreadHandler.Thread thread = ThreadHandler.CreateThread(NextPage, activityId, activityName);
            ThreadHandler.StartThread(thread);
            ThreadHandler.Schedule(thread);
        }

        private void MRefreshLayout_OnHeaderRefresh(object sender, EventArgs e)
        {
            if(!IsRefreshing)
            Task.Run(async () =>
            {
                logger.Info("Swipe Header Refreshing");
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
                logger.Info("Swipe Footer Refreshing");
                content.SetBottomLoading(true);
                ThreadHandler.Thread thread = ThreadHandler.CreateThread(content.NextPage,content.activityId,content.activityName);
                ThreadHandler.StartThread(thread);
                ThreadHandler.Schedule(thread);

            }

            public void OnHeaderRefresh()
            {
                if (!content.IsRefreshing)
                    Task.Run(async () =>
                    {
                        logger.Info("Swipe Header Refreshing");
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
            SetMainLoading(true);
            ThreadHandler.Thread thread = ThreadHandler.CreateThread(async () =>
            {
                logger.Info("Refreshing HPContent");
                var h = new Handler(Looper.MainLooper);
                bool success = await Core.Gallery.SearchGallery(Current_Query);
                if (!success)
                {
                    h.Post(() =>
                    {
                        SetMainLoading(false);
                        SetError(true);
                    });
                    return;
                }
                CurrentPage = 0;
                h.Post(() =>
                {
                    adapter.NotifyDataSetChanged();
                    adapter.ResetList();
                    SetMainLoading(false);
                    if (Core.Gallery.CurrentList.Count > 0)
                        mRecyclerView.ScrollToPosition(0);
                    GetTotalCount();

                });
                logger.Info("HPContent Refresh Successful");
            }, activityId, activityName);
            ThreadHandler.StartThread(thread);
            ThreadHandler.Schedule(thread);
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

        public void SetError(bool show)
        {
            switch (show)
            {
                case true:
                    mRefreshLayout.Visibility = ViewStates.Gone;
                    mProgressView.Visibility = ViewStates.Gone;
                    mErrorFrame.Visibility = ViewStates.Visible;
                    break;
                case false:
                    mErrorFrame.Visibility = ViewStates.Gone;
                    break;
            }
            

        }

        public void SetMainLoading(bool state)
        {
            switch (state)
            {
                case true:
                    mProgressView.Visibility = ViewStates.Visible;
                    SetError(false);
                    mRefreshLayout.Visibility = ViewStates.Gone;
                    mRecyclerView.Visibility = ViewStates.Invisible;
                    IsLoading = true;
                    break;
                case false:
                    mProgressView.Visibility = ViewStates.Gone;
                    mRefreshLayout.Visibility = ViewStates.Visible;
                    mRecyclerView.Visibility = ViewStates.Visible;
                    IsLoading = false;
                    break;
            }
        }

        public void NextPage()
        {
            logger.Info("Loading Next Page");
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
            logger.Info("Loading Next Page Successful");
        }

        public void PreviousPage()
        {
            logger.Info("Loading Previous Page");
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
            logger.Info("Loading Previous Page Successful");
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
            private static Logger logger = LogManager.GetCurrentClassLogger();
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
                GalleryCardHolder vh = holder as GalleryCardHolder;
                vh.gcard.Gallery = mdata[position];
                try
                {
                    vh.gcard.Refresh();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "\n Exception Caught In HPContent.ListViewAdapter.OnBindViewHolder.");

                }
                //vh.gcard.SetOnClickListener(new GalleryCardClickListener());
            }




            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                /*View itemview = Android.Views.LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.galleryCard, parent, false);*/
                View itemview = new Custom_Views.GalleryCard(mcontext);
                GalleryCardHolder vh = new GalleryCardHolder(itemview);
                return vh;
            }
        }

        public class GalleryCardHolder : RecyclerView.ViewHolder
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();
            public Custom_Views.GalleryCard gcard;
            public GalleryCardHolder(View itemView) : base(itemView)
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

        /*public class ScrollListener : RecyclerView.OnScrollListener
        {
            private int toolbarOffset = 0;
            private int toolbarHeight;
            private Toolbar toolbar;
            public HPContent mactivity;

            /*public ScrollListener(HPContent context, Toolbar toolbar)
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
        }*

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
        }*/


    }
}