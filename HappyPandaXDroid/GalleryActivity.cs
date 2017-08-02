using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.View;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using ThreadHandler = HappyPandaXDroid.Core.App.Threading;


namespace HappyPandaXDroid
{
    [Activity(Label = "GalleryActivity")]
    public class GalleryActivity : AppCompatActivity
    {
        public TextView title, category, read_action,
            language, pages, time_posted, no_tags;
        public LinearLayout TagLayout, InfoLayout;
        CardView ActionCard;
        public ImageView ThumbView;
        Core.Gallery.GalleryItem gallery;
        RecyclerView grid_layout;
        PreviewAdapter adapter;
        public bool IsRunning = true;
        public int activityId;
        List<Core.Gallery.Page> pagelist;
        //public List<Tuple<Task,CancellationTokenSource>> tasklist = new List<Tuple<Task, CancellationTokenSource>>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Gallery_Details_Layout);
            string data = Intent.GetStringExtra("gallery");
            InitializeViews();
            gallery = Core.JSON.Serializer.simpleSerializer.Deserialize<Core.Gallery.GalleryItem>(data);


            activityId = ThreadHandler.Thread.IdGen.Next();
            pagelist = Core.App.Server.GetRelatedItems<Core.Gallery.Page>(gallery.id);
            ParseData();

            string path = string.Empty ;
            Task.Run(async () =>
            {
                path = await Core.Gallery.GetThumb(gallery);
                RunOnUiThread(() =>
                {
                    try
                    {
                        Glide.With(this)
                            .Load(path)
                            .Into(ThumbView);
                    }
                    catch (Exception ex)
                    {

                    }
                });
            });
        }

        protected override void OnDestroy()
        {
            IsRunning = false;
            ThreadHandler.AbortActivityThreads(activityId);
            base.OnDestroy();
        }

        protected override void OnResume()
        {
            ThreadHandler.ElevateActivityPriority(activityId);
            base.OnResume();
        }

        void InitializeViews()
        {
            title = FindViewById<TextView>(Resource.Id.title);
            category = FindViewById<TextView>(Resource.Id.category);
            read_action = FindViewById<TextView>(Resource.Id.read);
            language = FindViewById<TextView>(Resource.Id.language);
            pages = FindViewById<TextView>(Resource.Id.pages);
            time_posted = FindViewById<TextView>(Resource.Id.posted);
            no_tags = FindViewById<TextView>(Resource.Id.no_tags);

           grid_layout = FindViewById<RecyclerView>(Resource.Id.grid_layout);
            TagLayout = FindViewById<LinearLayout>(Resource.Id.tags);
            InfoLayout = FindViewById<LinearLayout>(Resource.Id.info);
            ActionCard = FindViewById<CardView>(Resource.Id.action_card);

            ThumbView = FindViewById<ImageView>(Resource.Id.thumb);

            ActionCard.Clickable = true;
            ActionCard.Click += ActionCard_Click;
            adapter = new PreviewAdapter(this);

            grid_layout.SetAdapter(adapter);
            var layout = new GridLayoutManager(this, 5);
            grid_layout.SetLayoutManager(layout);
        }

        private void ActionCard_Click(object sender, EventArgs e)
        {
            List<int> pages_ids = new List<int>();
            if (pagelist.Count < 1)
                return;

            Intent intent = new Android.Content.Intent(this, typeof(GalleryViewer));
            intent.PutExtra("page", Core.JSON.Serializer.simpleSerializer.Serialize(pagelist));
            StartActivity(intent);

        }

        void ParseData()
        {
            title.Text = gallery.titles[0].name;
            gallery.tags = Core.Gallery.GetTags(gallery.id, "Gallery");
            category.Text = "place_holder";
            if (gallery.tags.Language.Count > 0)
            {
                string lan = gallery.tags.Language[0].name;
                language.Text = System.Globalization.CultureInfo.CurrentCulture
                    .TextInfo.ToTitleCase(lan.ToLower());
            }
            else
            language.Text = "eng";
            pages.Text = pagelist.Count.ToString() + " Pages" ;
            adapter.SetList(pagelist);
            ParseTags();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }


#pragma warning disable 618
        public void SetTagLayout()
        {

            var inflater = LayoutInflater;
            //reclass


            int color_header = Resources.GetColor(Resource.Color.colorPrimary);
            int color_tag = Resources.GetColor(Resource.Color.purple_a700);
            Type t = gallery.tags.GetType();
            PropertyInfo[] namespaces = t.GetProperties();
            foreach(var _namespace in namespaces)
            {
                object value = _namespace.GetValue(gallery.tags);
                string name = _namespace.Name;
                if (name.Contains("__"))
                    name = "misc";
                if (value != null)
                {
                    var tags = (List<Core.Gallery.TagItem>)value;
                    if (tags.Count > 0)
                    {
                        
                        var rtg = (LinearLayout)inflater.Inflate(Resource.Layout.tag_group_template, TagLayout, false);
                        rtg.Orientation = Orientation.Horizontal;
                        TagLayout.AddView(rtg);
                        TextView tag_header = (TextView)inflater.Inflate(Resource.Layout.tag_template, rtg, false);
                        tag_header.Text = name.ToLower();
                        tag_header.SetBackgroundDrawable(new Custom_Views.RoundSideRectDrawable(color_header));
                        rtg.AddView(tag_header);
                        Custom_Views.AutoWrapLayout awl = new Custom_Views.AutoWrapLayout(this);
                        rtg.AddView(awl, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                        tags.Sort((a, b) => a.name.CompareTo(b.name));
                        foreach (var tag in tags)
                        {
                            TextView tag_item = (TextView)inflater.Inflate(Resource.Layout.tag_template, awl, false);
                            tag_item.Text = tag.name;                            
                            tag_item.SetBackgroundDrawable(new Custom_Views.RoundSideRectDrawable(color_tag));

                            awl.AddView(tag_item);
                        }
                    }
                }
            }
        }

        void ParseTags()
        {
            if (!IsTagAvailable())
                return;
            TagLayout.RemoveAllViews();

            
            
            SetTagLayout();
            TagLayout.Visibility = ViewStates.Visible;
            
        }

        bool IsTagAvailable()
        {
            int count = 0;
            var taglist = gallery.tags;
            if (taglist.Artist != null)
                count += taglist.Artist.Count;
            if (taglist.Female != null)
                count += taglist.Female.Count;
            if (taglist.Language != null)
                count += taglist.Language.Count;
            if (taglist.Male != null)
                count += taglist.Male.Count;
            if (taglist.Reclass != null)
                count += taglist.Reclass.Count;
            if (taglist.__namespace__ != null)
                count += taglist.__namespace__.Count;
            if (taglist.Parody != null)
                count += taglist.Parody.Count;

            if (count == 0)
                return false;
            else
                return true;
        }
    }

    public class PreviewAdapter : RecyclerView.Adapter
    {
        public int preview_count = 10;

        public List<Core.Gallery.Page> mdata;
        Android.Content.Context mcontext;
        public PreviewAdapter(Context context)
        {
            mcontext = context;
        }

        public override int ItemCount
        {
            get { return mdata.Count; }
        }

        public void SetList(List<Core.Gallery.Page> UrlList) 
        {
            int number = preview_count;
            if (UrlList.Count < 5)
                number = UrlList.Count;
            mdata = new List<Core.Gallery.Page>();
            for(int i = 0; i < number; i++)
            {
                mdata.Add(UrlList[i]);
            }
            //mdata.Capacity = 20;
            mdata.TrimExcess();
            NotifyDataSetChanged();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            PreviewViewHolder vh = holder as PreviewViewHolder;
            var page = mdata[position];
            var activity = (GalleryActivity)mcontext;
            var cts = new CancellationTokenSource();

            var thread = ThreadHandler.CreateThread( async () =>
           {
               try
               {
                    await vh.LoadPreview(page);
               }
               catch(Exception ex)
               {

               }

           }, activity.activityId);
            ThreadHandler.Schedule(thread);
            
            vh.txt.Text = mdata[position].number.ToString();
        }




        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemview = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.preview_template, parent, false);
            PreviewViewHolder vh = new PreviewViewHolder(itemview);
            return vh;
        }
    }

    

    public class PreviewViewHolder : RecyclerView.ViewHolder
    {
        public View preview;
        public ImageView img;
        public TextView txt;
        public int position;
        public bool loaded = false;
        public PreviewViewHolder(View itemView) : base(itemView)
        {
            
            preview = itemView;
            img = preview.FindViewById<ImageView>(Resource.Id.preview);
            txt = preview.FindViewById<TextView>(Resource.Id.title);
            preview.SetOnClickListener(new PreviewClickListener());
        }
        public async Task<bool> LoadPreview(Core.Gallery.Page page)
        {
            int tries = 0;
            try
            {

                var h = new Handler(Looper.MainLooper);
                h.Post(() =>
                {
                    if (((GalleryActivity)(preview.Context)).IsRunning)
                        Glide.With(preview.Context)
                 .Load(Resource.Drawable.loading2)
                 .Into(img);
                });
                while (true)
                {
                    page.thumb_url = await Core.Gallery.GetImage(page, true);
                    if(page.thumb_url.Contains("fail"))
                    {
                        if (page.thumb_url.Contains("misc"))
                        {
                            tries++;
                            if (tries < 4)
                            {
                                continue;
                            }

                            return false;

                        }
                        return false;
                    }
                    else
                    {
                        break;
                    }
                }
                IFutureTarget future = Glide.With(preview.Context)
                    .Load(page.thumb_url)
                    .DownloadOnly(500, 500);
                var cache = future.Get();
                h = new Handler(Looper.MainLooper);
                h.Post(() =>
                {
                    if(((GalleryActivity)(preview.Context)).IsRunning)
                    Glide.With(preview.Context)
                         .Load(page.thumb_url)
                         .DiskCacheStrategy(Com.Bumptech.Glide.Load.Engine.DiskCacheStrategy.All)
                         .Into(img);
                    loaded = true;
                });
                tries = 0;
                return true;
            }
            catch(Exception ex)
            {
                tries = 0;
                return false;
            }
            
        }

        class PreviewClickListener : Java.Lang.Object, View.IOnClickListener
        {
            View preview;
            public void OnClick(View v)
            {
                preview = v;
                Intent intent = new Intent(preview.Context, typeof(GalleryActivity));
                /*string gallerystring = Core.JSON.Serializer.simpleSerializer.Serialize(preview.Gallery);
                intent.PutExtra("gallery", gallerystring);
                card.Context.StartActivity(intent);*/
            }
        }


    }
}