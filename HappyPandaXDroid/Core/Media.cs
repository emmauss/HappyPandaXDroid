using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;

using Com.Bumptech.Glide;
using Emmaus.Widget;

namespace HappyPandaXDroid.Core
{
    class Media
    {
        
        
        public class ImageViewer
        {
            List<string> ImageList = new List<string>();
            int ImagePosition = 0, preloadPositionRight = 0, preloadPositionLeft;
            Context mcontext;
            GalleryViewer.ImageAdapter madapter;
           
            public ImageViewer()
            {
            }

            public void SetList(List<string> list)
            {
                ImageList = list;

            }

            public async void StartReader(RecyclerViewPager viewpager,Context context,int pos, RecyclerViewPager pager)
            {
                if(ImageList == null || ImageList.Count == 0)
                {
                    throw new ImageListEmptyException("Image List not initiallized or empty");
                }


                //start reader

                mcontext = context;
                this.madapter = (GalleryViewer.ImageAdapter)viewpager.GetAdapter();
                var ImageView = new Custom_Views.ImageViewHolder(mcontext);
                ImagePosition = pos;
                

                //madapter.Add(ImageView);
                viewpager.AddOnPageChangedListener(new OnPageChange(this));
                //madapter.ImageList = ImageList;
                
                madapter.NotifyDataSetChanged();
                pager.ScrollToPosition(ImagePosition);
                preloadPositionLeft = pos;
                /*PreloadImages();
                PreloadLeft();                
                madapter.NotifyDataSetChanged();*/

            }

            /// <summary>
            /// Preloads  a default of the next 2 images
            /// </summary>
            public void PreloadImages()
            {
                while(preloadPositionRight < ImagePosition + 3)
                {
                    preloadPositionRight++;
                    if (preloadPositionRight == ImagePosition || preloadPositionRight < ImagePosition)
                        preloadPositionRight = ImagePosition + 1;
                    if (preloadPositionRight >= ImageList.Count)
                        return;
                    var ImageView = new Custom_Views.ImageViewHolder(mcontext);                    
                    /*GalleryService
                        .LoadFile(ImageList[preloadPositionRight]).Retry(3, 1000)
                        .Into(ImageView);*/
                    //madapter.Add(ImageView);
                    //int posn = madapter.GetItemPosition(ImageView);
                    madapter.NotifyDataSetChanged();
                    
                }

            }

            public void PreloadLeft()
            {

                while ((preloadPositionLeft > ImagePosition - 3) && preloadPositionLeft > 0)
                {
                    preloadPositionLeft--;
                    if (preloadPositionLeft == ImagePosition || preloadPositionLeft > ImagePosition)
                        preloadPositionLeft = ImagePosition - 1;
                    if (preloadPositionLeft < 0)
                        return;
                    var ImageView = new Custom_Views.ImageViewHolder(mcontext);
                    /*GalleryService
                        .LoadFile(ImageList[preloadPositionLeft]).Retry(3, 1000)
                        .Into(ImageView);*/
                    //madapter.Insert(0,ImageView);
                    //int posn = madapter.GetItemPosition(ImageView);
                    madapter.NotifyDataSetChanged();
                }
            }

            /// <summary>
            /// Preloads the next 'number' of images
            /// </summary>
            /// <param name="number">Number of images to preload</param>
            public void PreloadImages(int number)
            {
                while (preloadPositionRight < ImagePosition + (number+1))
                {
                    preloadPositionRight++;
                    if (preloadPositionRight == ImagePosition || preloadPositionRight < ImagePosition)
                        preloadPositionRight = ImagePosition + 1;
                    if (preloadPositionRight >= ImageList.Count)
                        return;
                    var ImageView = new Custom_Views.ImageViewHolder(mcontext);
                    /*GalleryService
                        .LoadFile(ImageList[preloadPositionRight]).Retry(3, 1000)
                        .Into(ImageView);*/
                    //madapter.ImageViewList.Add(ImageView);

                    madapter.NotifyDataSetChanged();
                }
            }

            public class OnPageChange : Java.Lang.Object,RecyclerViewPager.OnPageChangedListener
            {
                ImageViewer mviewer;
                public OnPageChange(ImageViewer viewer)
                {
                    mviewer = viewer;
                }

                public void OnPageChanged(int oldPosition, int newPosition)
                {
                    bool dir = (newPosition < oldPosition);
                    if (!dir)
                    {
                        mviewer.ImagePosition++;
                        mviewer.PreloadImages();
                    }
                    else
                    {
                        mviewer.ImagePosition--;
                        mviewer.PreloadLeft();
                    }
                }
            }

            public void LoadNextImage()
            {

            }         
                       

            public void Dispose()
            {
                ImageList.Clear();
                
            }

            

            public class ImageListEmptyException : Exception
            {
                public ImageListEmptyException() : base()
                {

                }

                public ImageListEmptyException(string message) : base(message)
                {

                }

                public ImageListEmptyException(string message, Exception innerException ) 
                    : base(message,innerException)
                {

                }

                protected ImageListEmptyException(SerializationInfo info, StreamingContext context) 
                    : base(info, context)
                {

                }
            }
        }
    }
}