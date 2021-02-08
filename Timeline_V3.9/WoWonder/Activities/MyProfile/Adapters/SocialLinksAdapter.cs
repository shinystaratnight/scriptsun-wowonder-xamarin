using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.MyProfile.Adapters
{
    public class SocialItem
    {
        public int Id { get; set; }
        public string SocialName { get; set; }
        public string SocialIcon { get; set; }
        public Color IconColor { get; set; }
        public string SocialLinkName { get; set; }
        public bool Checkvisibilty { get; set; }
    }

    public class SocialLinksAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<SocialItem> SocialList = new ObservableCollection<SocialItem>();
         
        public SocialLinksAdapter(Activity context)
        {
            try
            {
                var activityContext = context;

                if (AppSettings.ShowSettingsSocialLinksFacebook)
                    SocialList.Add(new SocialItem
                    {
                        Id = 1,
                        SocialName = activityContext.GetText(Resource.String.Lbl_Facebook),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.LogoFacebook,
                        IconColor = Color.ParseColor("#3b5999")
                    });

                if (AppSettings.ShowSettingsSocialLinksTwitter)
                    SocialList.Add(new SocialItem
                    {
                        Id = 2,
                        SocialName = activityContext.GetText(Resource.String.Lbl_Twitter),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.LogoTwitter,
                        IconColor = Color.ParseColor("#55acee")
                    });

                if (AppSettings.ShowSettingsSocialLinksGoogle)
                    SocialList.Add(new SocialItem
                    {
                        Id = 3,
                        SocialName = activityContext.GetText(Resource.String.Lbl_GooglePlus) + "+",
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.LogoGoogle,
                        IconColor = Color.ParseColor("#dd4b39")
                    });

                if (AppSettings.ShowSettingsSocialLinksVkontakte)
                    SocialList.Add(new SocialItem
                    {
                        Id = 4,
                        SocialName = activityContext.GetText(Resource.String.Lbl_Vk),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = FontAwesomeIcon.Vk,
                        IconColor = Color.ParseColor("#4c75a3")
                    });

                if (AppSettings.ShowSettingsSocialLinksLinkedin)
                    SocialList.Add(new SocialItem
                    {
                        Id = 5,
                        SocialName = activityContext.GetText(Resource.String.Lbl_Linkedin),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.LogoLinkedin,
                        IconColor = Color.ParseColor("#0077B5")
                    });

                if (AppSettings.ShowSettingsSocialLinksInstagram)
                    SocialList.Add(new SocialItem
                    {
                        Id = 6,
                        SocialName = activityContext.GetText(Resource.String.Lbl_Instagram),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.Happy,
                        IconColor = Color.ParseColor("#e4405f")
                    });

                if (AppSettings.ShowSettingsSocialLinksYouTube)
                    SocialList.Add(new SocialItem
                    {
                        Id = 7,
                        SocialName = activityContext.GetText(Resource.String.Lbl_YouTube),
                        SocialLinkName = "",
                        Checkvisibilty = false,
                        SocialIcon = IonIconsFonts.LogoYoutube,
                        IconColor = Color.ParseColor("#cd201f")
                    });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SocialList?.Count ?? 0;
 
        public event EventHandler<SocialLinksAdapterClickEventArgs> ItemClick;
        public event EventHandler<SocialLinksAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> ChannelSubscribed_View
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_SocialLinks_View, parent, false);
                var vh = new SocialLinksAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is SocialLinksAdapterViewHolder holder)
                {
                    var item = SocialList[position];
                    if (item != null)
                    {
                        string name = Methods.FunString.DecodeString(item.SocialName);
                        holder.NameSocial.Text = Methods.FunString.SubStringCutOf(name, 20);

                        FontUtils.SetTextViewIcon(item.Id == 4 ? FontsIconFrameWork.FontAwesomeBrands : FontsIconFrameWork.IonIcons,holder.IconSocial, item.SocialIcon);

                        holder.IconSocial.SetTextColor(item.IconColor);

                        if (item.Checkvisibilty)
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconCheck, IonIconsFonts.Checkmark);
                            holder.IconCheck.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                            holder.NameLink.Text = item.SocialLinkName;
                            holder.NameLink.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                            holder.LayoutCheckvisibilty.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            holder.LayoutCheckvisibilty.Visibility = ViewStates.Invisible;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        public void Update(SocialItem item, string linkName)
        {
            try
            {
                var data = SocialList.FirstOrDefault(a => a.Id == item.Id);
                if (data != null)
                {
                    if (!string.IsNullOrEmpty(linkName))
                    {
                        data.SocialLinkName = linkName;
                        data.Checkvisibilty = true;
                    }
                    else
                    {
                        data.Checkvisibilty = false;
                    }

                    NotifyItemChanged(SocialList.IndexOf(data));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public SocialItem GetItem(int position)
        {
            return SocialList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(SocialLinksAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(SocialLinksAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class SocialLinksAdapterViewHolder : RecyclerView.ViewHolder
    {
        public SocialLinksAdapterViewHolder(View itemView, Action<SocialLinksAdapterClickEventArgs> clickListener,Action<SocialLinksAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                IconSocial = MainView.FindViewById<TextView>(Resource.Id.Social_Icon);
                NameSocial = MainView.FindViewById<TextView>(Resource.Id.Social_name);
                IconCheck = MainView.FindViewById<TextView>(Resource.Id.Icon_Check);
                NameLink = MainView.FindViewById<TextView>(Resource.Id.Link_name);

                LayoutCheckvisibilty = MainView.FindViewById<RelativeLayout>(Resource.Id.icon_container);

                itemView.Click += (sender, e) => clickListener(new SocialLinksAdapterClickEventArgs{ View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SocialLinksAdapterClickEventArgs{ View = itemView, Position = AdapterPosition });

                

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }


        public TextView IconSocial { get; private set; }
        public TextView IconCheck { get; private set; }
        public TextView NameSocial { get; private set; }
        public TextView NameLink { get; private set; }
        public RelativeLayout LayoutCheckvisibilty { get; private set; }

        #endregion
    }

    public class SocialLinksAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}