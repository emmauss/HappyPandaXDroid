using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Java.Lang;
using Android.Views;
using Android.Widget;
using ProgressView = XamarinBindings.MaterialProgressBar;
using PhotoView = Com.Github.Chrisbanes.Photoview;
using NLog;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request.Target;

using ThreadHandler = HappyPandaXDroid.Core.App.Threading;

namespace HappyPandaXDroid.Custom_Views
{
    public class ImageViewHolder : LinearLayout
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public PhotoView.PhotoView  img;
        ProgressView.MaterialProgressBar mProgressView;
        string page_path;
        Core.Gallery.Page Page { set; get; }
        public bool Loaded = false;
        View view;
        int tries = 0;
        public ImageViewHolder(Context context) : base(context)
        {
            Initialize();
        }

        public ImageViewHolder(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public ImageViewHolder(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            view = Inflate(this.Context, Resource.Layout.ImageLayout, this);
            img = FindViewById<PhotoView.PhotoView>(Resource.Id.image);
            mProgressView = FindViewById<ProgressView.MaterialProgressBar>(Resource.Id.progress_view);
            mProgressView.Visibility = ViewStates.Invisible;
            img.Visibility = ViewStates.Visible;
        }

        public void OnLoadStart(Core.Gallery.Page page)
        {
            if (!Loaded)
            {
                
                this.Page = page;
                var h = new Handler(Looper.MainLooper);
                h.Post(() =>
                {
                    mProgressView.Visibility = ViewStates.Visible;
                    img.Visibility = ViewStates.Invisible;
                });
                Load();
            }
            
        }

        async void Load()
        {
            
                try
                {
                    while (!IsCached())
                    {
                        page_path = await Core.Gallery.GetImage(Page, false, "original", false);

                        if (page_path.Contains("fail"))
                        {

                            if (page_path.Contains("misc"))
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


                var h = new Handler(Looper.MainLooper);
                h.Post(() =>
                    {
                        try
                        {
                            Glide.With(this.Context)
                            .Load(page_path)
                            .Override(Target.SizeOriginal, Target.SizeOriginal)
                            .Into(img)
                            ;
                            OnLoadEnd();
                        }
                        catch (IllegalArgumentException ex)
                        {
                            if (ex.Message.Contains("destroyed"))
                                return;
                        }

                    });
                    tries=0;

                    
                }
                catch (System.Exception ex)
                {

                }

            
        }

        public void OnLoadEnd()
        {
            var h = new Handler(Looper.MainLooper);

            Loaded = true;
            h.Post(() =>
            {
                mProgressView.Visibility = ViewStates.Gone;
                img.Visibility = ViewStates.Visible;
            });
        }

        bool IsCached()
        {
            int item_id = Page.id;
            try
            {

                page_path = Core.App.Settings.cache + "pages/" + Core.App.Server.HashGenerator("original", "page", item_id) + ".jpg";
                bool check = Core.Media.Cache.IsCached(page_path);

                return check;
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, "\n Exception Caught In GalleryCard.IsCached.");

                return false;
            }


        }
    }
}