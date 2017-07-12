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
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Support.V7.View;


namespace HappyPandaXDroid.Custom_Views
{
    public class GalleryCard : LinearLayout
    {
        View galleryCard;
        public TextView text
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
        public ImageView img
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
        public TextView text2
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
            text = FindViewById<TextView>(Resource.Id.textViewholder);
            text2 = FindViewById<TextView>(Resource.Id.textViewholder2);
            text.Text = "hey";
            this.Clickable = true;
            this.Click += GalleryCard_Click;
            this.LongClick += GalleryCard_LongClick;
            this.Touch += GalleryCard_Touch;

        }

        private void GalleryCard_Touch(object sender, TouchEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GalleryCard_LongClick(object sender, LongClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GalleryCard_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}