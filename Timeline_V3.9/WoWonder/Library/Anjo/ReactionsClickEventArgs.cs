using System;
using Android.Widget;

namespace WoWonder.Library.Anjo
{
    public class ReactionsClickEventArgs : EventArgs
    {
        public ImageView ImgButton { get; set; }
        public int Position { get; set; } 
        public string React { get; set; } 
    }
}