using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Support.V7.View;
using Com.Bumptech.Glide;


using System.Threading;
using System.Threading.Tasks;
using NLog;


namespace HappyPandaXDroid.Custom_Views
{
    public class GalleryCard : LinearLayout
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
            Label.Text = string.Empty;
            img = FindViewById<ImageView>(Resource.Id.imageView);
            //t.Text = "hey";
            Clickable = true;
            /*this.Click += GalleryCard_Click;
            this.LongClick += GalleryCard_LongClick;
            this.Touch += GalleryCard_Touch;*/
        }

        public async void Refresh()
        {
            logger.Info("Refreshing GalleryCard. GalleryId = {0}",Gallery.id);
            Name.Text = Gallery.titles[0].name;
            var h = new Handler(Looper.MainLooper);
            h.Post(() =>
            {
                try { 
                Glide.With(Context)
                    .Load(Resource.Drawable.loading2)
                    .Into(img);
            }catch (System.Exception ex)
            {
                if (ex.Message.Contains("destroyed"))
                    return;
            }
        });
            if (!IsCached)
            {
                await Task.Run(async () =>
               {
                   thumb_path = await Core.Gallery.GetImage(gallery,false);
               });
            }
            await Task.Run(async () =>
            {
                await Task.Delay(10);
                //Gallery = Core.App.Server.GetItem<Core.Gallery.GalleryItem>(Gallery.id, "Gallery");                
            });
            Label.Visibility = ViewStates.Gone;
            LoadThumb();
            logger.Info("Refresh {0} Successful",Gallery.id);
        }

        public void LoadThumb()
        {
            if (IsCached)
            {
                try
                {
                    var h = new Handler(Looper.MainLooper);
                    h.Post(() =>
                    {
                        try { 
                       Glide.With(Context)
                        .Load(thumb_path)
                        .Into(img);
                    }catch (Exception ex)
                {
                    if (ex.Message.Contains("destroyed"))
                        return;
                }
            });
                }catch(Exception ex)
                {
                    logger.Error(ex, "\n Exception Caught In GalleryCard.LoadThumb.");

                }

            }
            else Refresh();
        }

        bool IsCached
        {
            get
            {
                int item_id = gallery.id;
                try
                {
                    thumb_path = Core.App.Settings.cache + "thumbs/" + Core.App.Server.HashGenerator("medium", "thumb", item_id) + ".jpg";
                    bool check = Core.Media.Cache.IsCached(thumb_path);

                    return check;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "\n Exception Caught In GalleryCard.IsCached.");

                    return false;
                }

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
            Intent intent = new Intent(Context, typeof(GalleryActivity));
            string gallerystring = Core.JSON.Serializer.simpleSerializer.Serialize(gallery);
            intent.PutExtra("gallery", gallerystring);
            Context.StartActivity(intent);
        }
    }
}