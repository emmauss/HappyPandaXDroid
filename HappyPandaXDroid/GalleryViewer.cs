
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;

using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using PhotoView = Com.Github.Chrisbanes.Photoview;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Java.Lang;
using Emmaus.Widget;
using Com.Bumptech.Glide.Request.Target;

using ThreadHandler = HappyPandaXDroid.Core.App.Threading;

namespace HappyPandaXDroid
{
    [Activity(Label = "GalleryViewer")]
    public class GalleryViewer : AppCompatActivity
    {
        
        Toolbar toolbar;
        public TextView page_number;
        public RecyclerViewPager galleryPager;
        bool overlayVisible = true;
        public  ImageAdapter adapter;
        FrameLayout lay;
        /*List<string> ImageList =
            new List<string>();*/
        List<Core.Gallery.Page> PageList =
            new List<Core.Gallery.Page>();
        SeekBar seekbar;
        bool doubl_click = false;
        public int activityID;
        int touch_count = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.GalleryLayout);
            
                string data = Intent.GetStringExtra("page");
                PageList = Core.JSON.Serializer.simpleSerializer.Deserialize<List<Core.Gallery.Page>>(data);
            InitPageGen();
            activityID = ThreadHandler.Thread.IdGen.Next();
            
            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            lay = FindViewById<FrameLayout>(Resource.Id.frame);
            galleryPager = FindViewById<RecyclerViewPager>(Resource.Id.galleryViewPager);
            var layout = new ExtraLayoutManager(this, LinearLayoutManager.Horizontal, false);            
            galleryPager.SetLayoutManager(layout);
            adapter = new ImageAdapter(PageList,this);
            galleryPager.SetAdapter(new RecyclerViewPagerAdapter(galleryPager, adapter));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            seekbar = FindViewById<SeekBar>(Resource.Id.progress_seekbar);
            seekbar.Max = PageList.Count;
            seekbar.Progress = galleryPager.CurrentPosition + 1;
            galleryPager.GetAdapter().NotifyDataSetChanged();
            page_number = FindViewById<TextView>(Resource.Id.page_number);
            page_number.Text = seekbar.Progress.ToString();
            galleryPager.AddOnPageChangedListener(new PageChangeListener(this));
            seekbar.SetOnSeekBarChangeListener(new SeekBarChangeListener(this));
            /*int pos = list.IndexOf(data);
            if (pos < 0)
                pos = 0;
            imageHandler.StartReader(galleryPager, this, pos, galleryPager);*/
             
         }

        protected override void OnResume()
        {
            ThreadHandler.ElevateActivityPriority(activityID);
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            
            ThreadHandler.AbortActivityThreads(activityID);
            base.OnDestroy();
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            touch_count++;
            bool res =  base.DispatchTouchEvent(ev);

            Task.Run(async () =>
            {
                await Task.Delay(500);
                if (touch_count == 2)
                {
                    touch_count = 0;
                    toggleOverlay();
                }
                else if (touch_count > 2)
                {
                    touch_count = 0;
                }
            });
            return res;
        }

       

        public class PhotoImageVIew: PhotoView.PhotoView
        {
            GalleryViewer mactivity;
            public PhotoImageVIew(Context context) : base(context)
            {
                if (context is GalleryViewer)
                    mactivity = (GalleryViewer)context;
            }
            
        }
        

        public class SeekBarChangeListener : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
        {
            GalleryViewer activity;
            public SeekBarChangeListener(GalleryViewer activity)
            {
                this.activity = activity;
            }   
            public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
            {
                //throw new NotImplementedException();
            }

            public void OnStartTrackingTouch(SeekBar seekBar)
            {
                //throw new NotImplementedException();
            }

            public void OnStopTrackingTouch(SeekBar seekBar)
            {
                int new_position = seekBar.Progress;
                if (new_position < 1)
                    new_position = 1;
                seekBar.Progress = new_position;
                activity.galleryPager.ScrollToPosition(new_position - 1);
                activity.page_number.Text = seekBar.Progress.ToString();

            }
        }

        public class PageChangeListener : RecyclerViewPager.OnPageChangedListener
        {
            GalleryViewer mactivity;
            public PageChangeListener(GalleryViewer activity)
            {
                mactivity = activity;
            }
            public void OnPageChanged(int oldPosition, int newPosition)
            {
                mactivity.seekbar.Progress = newPosition + 1;
                mactivity.page_number.Text = (newPosition + 1).ToString() ;
                mactivity.toolbar.Title = mactivity.adapter.PageList[newPosition].name;
            }
        }

        public class ExtraLayoutManager : LinearLayoutManager
        {
            private static readonly int DEFAULT_EXTRA_LAYOUT_SPACE = 600;
            private int extraLayoutSpace = -1;
            private Context context;

            public ExtraLayoutManager(Context context) : base(context)
            {
                this.context = context;
            }

            public ExtraLayoutManager(Context context, int extraLayoutSpace) : base(context)
            {
                this.context = context;
                this.extraLayoutSpace = extraLayoutSpace;
            }

            public ExtraLayoutManager(Context context, int orientation, bool reverseLayout) 
                : base(context,orientation,reverseLayout)
            {
                this.context = context;
            }

            public void setExtraLayoutSpace(int extraLayoutSpace)
            {
                this.extraLayoutSpace = extraLayoutSpace;
            }

            protected override int GetExtraLayoutSpace(RecyclerView.State state)
            {
                if (extraLayoutSpace > 0)
                {
                    return extraLayoutSpace;
                }
                else
                return DEFAULT_EXTRA_LAYOUT_SPACE;
            }
        }

        public List<string> GetPictureList(string imagefile)
        {
            List<string> imgList = new List<string>();
            string folder = Directory.GetParent(imagefile).FullName;
            var list = Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly);
            foreach(string file in list)
            {
                if(file.Contains(".jpg")|| file.Contains(".png")|| file.Contains(".gif"))
                {
                    imgList.Add(file);
                }
            }
            return imgList;
        }

        public void InitPageGen()
        {

            Core.Gallery.Page[] pages = PageList.ToArray();
            int[] ids = new int[pages.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = pages[i].id;
            }
            Core.Gallery.InitiateImageGeneration(ids, "page","original");
            
        }

        public async void toggleOverlay()
        {
            RunOnUiThread(() =>
            {
                if (overlayVisible)
                {
                    overlayVisible = false;
                    toolbar.Visibility = ViewStates.Gone;
                    seekbar.Visibility = ViewStates.Gone;
                }
                else
                {
                    overlayVisible = true;
                    toolbar.Visibility = ViewStates.Visible;
                    seekbar.Visibility = ViewStates.Visible;
                }
            });

            
        }

        public void NextPage()
        {
            int pos = galleryPager.CurrentPosition;
            if (pos < galleryPager.ItemCount)
                galleryPager.SmoothScrollToPosition(pos + 1);

        }

        public void PreviousPage()
        {
            int pos = galleryPager.CurrentPosition;
            if (pos > -1)
                galleryPager.SmoothScrollToPosition(pos - 1);
        }
        
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                int pos = galleryPager.CurrentPosition;
                switch (keyCode)
                {
                    case Keycode.VolumeUp:
                        PreviousPage();
                        return true;
                    case Keycode.DpadLeft:
                        PreviousPage();
                        return true;
                    //break;
                    case Keycode.VolumeDown:
                        NextPage();
                        return true;
                    case Keycode.DpadRight:
                        NextPage();
                        return true;
                        //break;
                }
            }
            return base.OnKeyDown(keyCode, e);
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

        public class OnTouchListener : Java.Lang.Object,View.IOnTouchListener
        {
            GalleryViewer mparent;
            public OnTouchListener(GalleryViewer g)
            {
                mparent = g;
            }
            public bool OnTouch(View v, MotionEvent e)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Up:
                        mparent.toggleOverlay();
                        break;
                }
                return false;
            }
        }

        public  class ImageAdapter : RecyclerView.Adapter , IOnItemChangedListener
        {
            public List<Core.Gallery.Page> PageList;
            Context context;
            IOnRecyclerViewItemClickListener mOnItemClickListener;
            public ImageAdapter(List<Core.Gallery.Page> imagelist, Context context)
            {
                this.context = context;
                PageList = imagelist;
            }

            public override int ItemCount
            {
                get
                {
                    return PageList.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ImageViewHolder vh = holder as ImageViewHolder;
                var activity = (GalleryViewer)context;

                Glide.With(context)
                    .Load(Resource.Drawable.Loading)                    
                    .Into(vh.imageView);
                int tries = 0;
                var thread = ThreadHandler.CreateThread(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(10);
                        if (PageList[position].image_url == string.Empty || PageList[position].image_url == null 
                            || PageList[position].image_url.Contains("fail:"))
                        {
                            PageList[position].image_url = await Core.Gallery.GetImage(PageList[position], true, "original");
                        }
                        if (PageList[position].image_url.Contains("fail"))
                        {

                            if (PageList[position].image_url.Contains("misc"))
                            {
                                tries++;
                                if (tries < 4)
                                {
                                    continue;
                                }

                                return;

                            }
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }

                string url = PageList[position].image_url;
                    
                    var h = new Handler(Looper.MainLooper);
                    h.Post(() =>
                    {
                        Glide.With(context)
                        .Load(url)
                        .DiskCacheStrategy(Com.Bumptech.Glide.Load.Engine.DiskCacheStrategy.All)
                        .Override(Target.SizeOriginal,Target.SizeOriginal)
                        .Into(vh.imageView)
                        ;
                    });
                    tries = 0;
                    //vh.imageView = ImageList[position];
                },activity.activityID);
                ThreadHandler.Schedule(thread);


            }

            public int indexOf(Core.Gallery.Page item)
            {
                return PageList.IndexOf(item);
            }

            public class Onclick : Java.Lang.Object,View.IOnClickListener
            {
                IOnRecyclerViewItemClickListener ionclick;
                int position;
                RecyclerView.ViewHolder holder;
                public Onclick(ImageAdapter.IOnRecyclerViewItemClickListener onclick,int position,
                    RecyclerView.ViewHolder holder)
                {
                    ionclick = onclick;
                    this.position = position;
                    this.holder = holder;

                }
                public void OnClick(View v)
                {
                    if ( ionclick!= null)
                    {
                        ionclick.onItemClick(v,position,holder);
                    }
                }
            }

            public ImageAdapter Add(Core.Gallery.Page item)
            {
                PageList.Add(item);

                return this;
            }

            public ImageAdapter Insert(int position, Core.Gallery.Page item)
            {
                PageList.Insert(position,item);
                NotifyItemInserted(position);
                return this;
            }

            public ImageAdapter Set(int index, Core.Gallery.Page item)
            {
                if (index > -1)
                    PageList[index] = item;
                NotifyItemChanged(index);
                return this;
            }

            public ImageAdapter Remove(int index)
            {
                PageList.RemoveAt(index);
                if (index == 0)
                {
                    NotifyDataSetChanged();
                }
                else if(index > 0)
                {
                    NotifyItemRemoved(index);
                }
                return this;
            }

            public ImageAdapter Remove(Core.Gallery.Page item)
            {
                int position = PageList.IndexOf(item);
                PageList.Remove(item);
                if (position == 0)
                {
                    NotifyDataSetChanged();
                }
                else if (position > 0)
                {
                    NotifyItemRemoved(position);
                }
                return this;
            }

            public ImageAdapter Clear()
            {
                int size = PageList.Count;
                PageList.Clear();
                NotifyItemRangeRemoved(0, size);
                return this;
            }

            

            public interface IOnRecyclerViewItemClickListener {
                void onItemClick(View view, int position,RecyclerView.ViewHolder viewHolder);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                /*var imageview = Android.Views.LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.ImageViewTemplate, parent,false);*/
                PhotoImageVIew img = new PhotoImageVIew(context);
                return new ImageViewHolder(img);
            }

            public void Swap(List<Core.Gallery.Page> list, int p1, int p2)
            {
                Core.Gallery.Page temp = list[p1];
                list[p1] = list[p2];
                list[p2] = temp;
            }

            public bool OnItemMove(int fromPosition, int toPosition)
            {
                if (fromPosition < toPosition)
                {
                    for (int i = fromPosition; i < toPosition; i++)
                    {
                        
                        Swap(PageList, i, i + 1);
                    }
                }
                else
                {
                    for (int i = fromPosition; i > toPosition; i--)
                    {
                        Swap(PageList, i, i - 1);
                    }
                }
                NotifyItemMoved(fromPosition, toPosition);
                return true;
            }

            public void OnItemDismiss(int position)
            {
                Remove(position);
            }

            public IOnRecyclerViewItemClickListener getOnItemClickListener()
            {
                return mOnItemClickListener;
            }

            public ImageAdapter SetOnItemClickListener(
      IOnRecyclerViewItemClickListener itemClickListener)
            {
                mOnItemClickListener = itemClickListener;
                return this;
            }
        }

        public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public PhotoImageVIew imageView;
            public ImageViewHolder(View itemView) : base(itemView)
            {
                imageView = (PhotoImageVIew)itemView;
                

            }
            
        }



        public interface IOnItemChangedListener
        {

            bool OnItemMove(int fromPosition, int toPosition);

            void OnItemDismiss(int position);
        }

        
    }
}