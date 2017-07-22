using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Support.V7.View;
using Com.Bumptech.Glide;


using System.Threading;
using System.Threading.Tasks;


namespace HappyPandaXDroid.Custom_Views
{
    public class GalleryCard : LinearLayout 
    {
        View galleryCard;
        ImageView img;
        TextView text;
        TextView text2;
        Core.Gallery.GalleryItem gallery;
        string thumb_path;

        public Core.Gallery.GalleryItem Gallery
        {
            get
            {
                return gallery;
            }
            set
            {
                gallery = value;
            }
        }
        public TextView Name
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        } 
        public ImageView Image
        {
            get
            {
                return img;
            }
            set
            {
                img = value;
            }
        }
        public TextView Label
        {
            get
            {
                return text2;
            }
            set
            {
                text2 = value;
            }
        }
        public GalleryCard(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
            
        }
        public GalleryCard(Context context) :
            base(context)
        {
            Initialize();
        }

        public GalleryCard(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            galleryCard = Inflate(this.Context,Resource.Layout.galleryCard, this);
            Name = FindViewById<TextView>(Resource.Id.textViewholder);
            Label = FindViewById<TextView>(Resource.Id.textViewholder2);
            img = FindViewById<ImageView>(Resource.Id.imageView);
            //t.Text = "hey";
            this.Clickable = true;
            this.Click += GalleryCard_Click;
            this.LongClick += GalleryCard_LongClick;
            this.Touch += GalleryCard_Touch;

        }

        public async void Refresh()
        {
            Name.Text = Gallery.titles[0].name;
            if (!IsCached)
            {
                await Task.Run(async () =>
               {
                   thumb_path = await Core.Gallery.GetImage(gallery);
               });
            }
            LoadThumb();
        }

        public void LoadThumb()
        {
            if (IsCached)
            {
                try
                {
                    Glide.With(Context)
                        .Load(thumb_path)
                        .Into(img);
                }catch(Exception ex)
                {

                }

            }
            else Refresh();
        }

        bool IsCached
        {
            get
            {
                int item_id = gallery.id;
                thumb_path = Core.App.Settings.cache + "thumbs/" +Core.App.Server.HashGenerator(gallery.profiles[0].size,item_id) + ".jpg";
                return File.Exists(thumb_path) ? true : false;
                
            }
        }

        private void GalleryCard_Touch(object sender, TouchEventArgs e)
        {
            
        }

        private void GalleryCard_LongClick(object sender, LongClickEventArgs e)
        {
            
        }

        private void GalleryCard_Click(object sender, EventArgs e)
        {
            
        }
    }
}