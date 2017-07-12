
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FFImageLoading;

using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Java.Lang;
using Emmaus.Widget;

namespace HappyPandaXDroid
{
    [Activity(Label = "GalleryViewer")]
    public class GalleryViewer : AppCompatActivity
    {
        Core.Media.ImageViewer imageHandler;
        Toolbar toolbar;
        RecyclerViewPager galleryPager;
        bool overlayVisible = true;
        List<FFImageLoading.Views.ImageViewAsync> ImageViewList =
            new List<FFImageLoading.Views.ImageViewAsync>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.GalleryLayout);
            string data = Intent.GetStringExtra("image");

            /*Core.Media.ImageViewer imageHandler = new Core.Media.ImageViewer();
            Core.JSON.Serializer.simpleSerializer
                .Deserialize(data, ref imageHandler);*/
           // toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);
            galleryPager = FindViewById<RecyclerViewPager>(Resource.Id.galleryViewPager);
            LinearLayoutManager layout = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
            galleryPager.SetLayoutManager(layout);
            ImageAdapter adapter = new ImageAdapter(ImageViewList,this);
            galleryPager.SetAdapter(new RecyclerViewPagerAdapter(galleryPager, adapter));
           // SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            imageHandler = new Core.Media.ImageViewer();
            var list = GetPictureList(data);
            imageHandler.SetList(list);
            int pos = list.IndexOf(data);
            if (pos < 0)
                pos = 0;
            imageHandler.StartReader(galleryPager, this, pos);
            
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

        void toggleOverlay()
        {
            if (overlayVisible)
            {
                overlayVisible = false;
                SupportActionBar.Hide();
            }
            else
            {
                overlayVisible = true;
                SupportActionBar.Show();
            }

            
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
                    case MotionEventActions.Down:
                        mparent.toggleOverlay();
                        break;
                }
                return false;
            }
        }

        public  class ImageAdapter : RecyclerView.Adapter , IOnItemChangedListener
        {
            public List<FFImageLoading.Views.ImageViewAsync> ImageViewList;
            Context context;
            IOnRecyclerViewItemClickListener mOnItemClickListener;
            public ImageAdapter(List<FFImageLoading.Views.ImageViewAsync> imagelist, Context context)
            {
                this.context = context;
                ImageViewList = imagelist;
            }

            public override int ItemCount
            {
                get
                {
                    return ImageViewList.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                ImageViewHolder vh = holder as ImageViewHolder;
                vh.imageView = ImageViewList[position];
                
            }

            public int indexOf(FFImageLoading.Views.ImageViewAsync item)
            {
                return ImageViewList.IndexOf(item);
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

            public ImageAdapter Add(FFImageLoading.Views.ImageViewAsync item)
            {
                ImageViewList.Add(item);

                return this;
            }

            public ImageAdapter Insert(int position,FFImageLoading.Views.ImageViewAsync item)
            {
                ImageViewList.Insert(position,item);
                NotifyItemInserted(position);
                return this;
            }

            public ImageAdapter Set(int index, FFImageLoading.Views.ImageViewAsync item)
            {
                if (index > -1)
                    ImageViewList[index] = item;
                NotifyItemChanged(index);
                return this;
            }

            public ImageAdapter Remove(int index)
            {
                ImageViewList.RemoveAt(index);
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

            public ImageAdapter Remove(FFImageLoading.Views.ImageViewAsync item)
            {
                int position = ImageViewList.IndexOf(item);
                ImageViewList.Remove(item);
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
                int size = ImageViewList.Count;
                ImageViewList.Clear();
                NotifyItemRangeRemoved(0, size);
                return this;
            }

            

            public interface IOnRecyclerViewItemClickListener {
                void onItemClick(View view, int position,RecyclerView.ViewHolder viewHolder);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var imageview = Android.Views.LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.ImageViewTemplate, parent,false);
               //var imageview = new FFImageLoading.Views.ImageViewAsync(context);
                return new ImageViewHolder(imageview);
            }

            public void Swap(List<FFImageLoading.Views.ImageViewAsync> list, int p1, int p2)
            {
                FFImageLoading.Views.ImageViewAsync temp = list[p1];
                list[p1] = list[p2];
                list[p2] = temp;
            }

            public bool OnItemMove(int fromPosition, int toPosition)
            {
                if (fromPosition < toPosition)
                {
                    for (int i = fromPosition; i < toPosition; i++)
                    {
                        
                        Swap(ImageViewList, i, i + 1);
                    }
                }
                else
                {
                    for (int i = fromPosition; i > toPosition; i--)
                    {
                        Swap(ImageViewList, i, i - 1);
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
            public FFImageLoading.Views.ImageViewAsync imageView;
            public ImageViewHolder(View itemView) : base(itemView)
            {
                imageView = itemView.FindViewById<FFImageLoading.Views.ImageViewAsync>(Resource.Id.ffimage);

            }
        }



        public interface IOnItemChangedListener
        {

            bool OnItemMove(int fromPosition, int toPosition);

            void OnItemDismiss(int position);
        }

        /*public class ImageAdapter : RecyclerViewPagerAdapter
        {
            
            public List<FFImageLoading.Views.ImageViewAsync> ImageViewList;
            Context context;
            public override int Count
            {
                get
                {
                    return ImageViewList.Count;
                }
            }

            /*public override int GetItemPosition(Java.Lang.Object objectValue)
            {
                /*var obj = (FFImageLoading.Views.ImageViewAsync)objectValue;
                if (ImageViewList.Contains(obj))
                {
                    return ImageViewList.IndexOf(obj);
                }
                return PositionNone;
            }
            

            public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
            {
                return view == objectValue;
            }

            public ImageAdapter(List<FFImageLoading.Views.ImageViewAsync> imageViewList, Context context)
            {
                this.context = context;
                ImageViewList = imageViewList;
            }
            public override void DestroyItem(View container, int position, Java.Lang.Object view)
            {
                var viewPager = container.JavaCast<ViewPager>();
                viewPager.RemoveView(view as View);
            }
            public override Java.Lang.Object InstantiateItem(View container, int position)
            {
                var imageView = new FFImageLoading.Views.ImageViewAsync(context);
                imageView = ImageViewList[position];
                var viewPager = container.JavaCast<ViewPager>();
                viewPager.AddView(imageView);
                return imageView;
            }
        }*/
    }
}