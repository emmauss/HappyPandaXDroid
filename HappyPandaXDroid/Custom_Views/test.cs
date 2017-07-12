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
using HappyPandaXDroid;

namespace HappyPandaXDroid.Custom_Views
{
    public class test : RelativeLayout
    {
        TextView text;
        ImageView img;
        View view;
        public test(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public test(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            view = Inflate(this.Context, Resource.Layout.cview, this);
            text = FindViewById<TextView>(Resource.Id.valueLabel);
            img = FindViewById<ImageView>(Resource.Id.icon_image);

        }
    }
}