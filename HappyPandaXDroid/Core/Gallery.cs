using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HappyPandaXDroid.Core
{
    public class Gallery
    {
        public static List<GalleryItem> CurrentList = new List<GalleryItem>();
        public enum ImageSize
        {
            Big = 400,
            Medium = 250,
            Original = 0,
            Small = 100
        }

        public enum ItemType
        {
            Collection = 1,
            Gallery = 0,
            GalleryFilter = 2,
            Grouping = 4,
            Page =3
        }

        public enum ViewType
        {
            Favorite = 1,
            Inbox = 2,
            Library = 0
        }

        public class GalleryItem
        {
            public int taggable_id;
            public bool complete_pages;
            public string category_id;
            public string last_read;
            public string last_updated;
            public string language_id;
            public List<Artist> artists;
            public int timestamp;
            public string info;
            public bool fetched;
            public int grouping_id;
            public int number;
            public bool fav;
            public int id;
            public int rating;
            public List<Profile> profiles;
            public int times_read;
            public List<Title> titles;
            public int pub_date;
            public List<URL> urls;
            public bool inbox;
        }

        public class Artist
        {
            public int id;
            public string name;
        }
        public class Profile
        {
            public int id;
            public int timestamp;
            public string ext;
            public string size;
            public string data;
            public byte[] Data
            {
                get
                {
                    string temp = data.Substring(data.IndexOf(",")+1);
                    return Convert.FromBase64String(temp);
                }
            }
        }
        public class Title
        {
            public int id;
            public string name;
            public int language_id;
            public int gallery_id;
        }
        public class URL
        {
            public int id;
            public string url;
            public int gallery_id;
        }


        public static void GetLibrary()
        {
            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "library_view");
            JSON.API.PushKey(ref funct, "limit", "<int>25");
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string data = JSON.API.GetData(countstring, 2);
            if(data.LastIndexOf("]") != data.Length-1)
            data = data.Remove(data.LastIndexOf("]")+1);
            CurrentList = JSON.Serializer.simpleSerializer.DeserializeToList<GalleryItem>(data);            

        }

        public static async Task<string> GetImage(GalleryItem gallery)
        {
            try
            {
                int gallery_item_id = gallery.id;
                List<Tuple<string, string>> main = new List<Tuple<string, string>>();
                List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
                JSON.API.PushKey(ref main, "name", "test");
                JSON.API.PushKey(ref main, "session", Net.session_id);
                JSON.API.PushKey(ref funct, "fname", "get_image");
                JSON.API.PushKey(ref funct, "item_ids", "[" + gallery_item_id + "]");
                JSON.API.PushKey(ref funct, "uri", "<bool>true");
                string response = JSON.API.ParseToString(funct);
                JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
                response = JSON.API.ParseToString(main);
                string reply = Net.SendPost(response);
                int command_id = App.Server.GetCommandId(gallery_item_id, reply);
                if (App.Server.StartCommand(command_id) == "started")
                {
                    string state = App.Server.GetCommandState(command_id);
                    if (state.Contains("error"))
                        return "fail: server error";
                    while (!App.Server.GetCommandState(command_id).Contains("finished"))
                    {
                        Thread.Sleep(1000);
                    }
                    //get value
                    string name = App.Server.HashGenerator(gallery.profiles[0].size, gallery_item_id);
                    string path = App.Server.GetCommandValue(command_id, gallery_item_id,name, "thumb");
                    return path;
                }
                else return "fail: server error";
            }
            catch (Exception ex)
            {
                return "fail: server error";
            }
        }

        public static bool SearchGallery(string query)
        {
            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "library_view");
            JSON.API.PushKey(ref funct, "limit", "<int>25");
            JSON.API.PushKey(ref funct, "search_query", query);
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string data = JSON.API.GetData(countstring, 2);
            if (data.LastIndexOf("]") != data.Length - 1)
                data = data.Remove(data.LastIndexOf("]") + 1);
            CurrentList = JSON.Serializer.simpleSerializer.DeserializeToList<GalleryItem>(data);
            if (CurrentList.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// jump to a page in the current library
        /// </summary>
        /// <param name="page">zero based page number</param>        
        public void JumpToPage(int page,string search_query)
        {
            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "library_view");
            JSON.API.PushKey(ref funct, "limit", "<int>25");
            JSON.API.PushKey(ref funct, "search_query", search_query);
            JSON.API.PushKey(ref funct, "page","<int>" + page);
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string data = JSON.API.GetData(countstring, 2);
            if (data.LastIndexOf("]") != data.Length - 1)
                data = data.Remove(data.LastIndexOf("]") + 1);
            CurrentList = JSON.Serializer.simpleSerializer.DeserializeToList<GalleryItem>(data);
        }

        public static int NextPage(int page, string search_query)
        {
            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "library_view");
            JSON.API.PushKey(ref funct, "limit", "<int>25");
            JSON.API.PushKey(ref funct, "search_query", search_query);
            JSON.API.PushKey(ref funct, "page", "<int>" + page);
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string data = JSON.API.GetData(countstring, 2);
            if (data.LastIndexOf("]") != data.Length - 1)
                data = data.Remove(data.LastIndexOf("]") + 1);
            var newpagelist = JSON.Serializer.simpleSerializer.DeserializeToList<GalleryItem>(data);
            int itemaddedcount = newpagelist.Count;
            if (itemaddedcount < 1)
                return 0;
            CurrentList.AddRange(newpagelist);
            return itemaddedcount;
        }

        public static int PreviousPage(int page, string search_query)
        {
            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();
            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "library_view");
            JSON.API.PushKey(ref funct, "limit", "<int>25");
            JSON.API.PushKey(ref funct, "search_query", search_query);
            JSON.API.PushKey(ref funct, "page", "<int>" + page);
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n" + response + "\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string data = JSON.API.GetData(countstring, 2);
            if (data.LastIndexOf("]") != data.Length - 1)
                data = data.Remove(data.LastIndexOf("]") + 1);
            var newpagelist = JSON.Serializer.simpleSerializer.DeserializeToList<GalleryItem>(data);
            int itemaddedcount = newpagelist.Count;
            if (itemaddedcount < 1)
                return 0;
            newpagelist.AddRange(CurrentList);
            CurrentList.Clear();
            CurrentList.AddRange(newpagelist);
            return itemaddedcount;
        }

        public static async Task<int> GetCount(string query)
        {
            int count = 0;

            List<Tuple<string, string>> main = new List<Tuple<string, string>>();
            List<Tuple<string, string>> funct = new List<Tuple<string, string>>();

            JSON.API.PushKey(ref main, "name", "test");
            JSON.API.PushKey(ref main, "session", Net.session_id);
            JSON.API.PushKey(ref funct, "fname", "get_view_count");
            JSON.API.PushKey(ref funct, "item_type", "Gallery");
            JSON.API.PushKey(ref funct, "search_query", query);
            string response = JSON.API.ParseToString(funct);
            JSON.API.PushKey(ref main, "data", "[\n"+response+"\n]");
            response = JSON.API.ParseToString(main);
            string countstring = Net.SendPost(response);
            string countdata = JSON.API.GetData(countstring, 2);
            countdata = countdata.Substring(countdata.IndexOf(":") + 1, countdata.IndexOf("}") - countdata.IndexOf(":") - 1);
            int.TryParse(countdata, out count);
            return count;
        }
    }
}