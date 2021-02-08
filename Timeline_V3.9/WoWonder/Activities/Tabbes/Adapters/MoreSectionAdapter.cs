using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;

using Android.Views;
using Android.Widget;
using Q.Rorbin.Badgeview;
using WoWonder.Helpers.Fonts;
using AmulyaKhare.TextDrawableLib;
using AndroidX.RecyclerView.Widget;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tabbes.Adapters
{
    public class SectionItem
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string Icon { get; set; }

        public int IconAsImage { get; set; }
        public int StyleRow { get; set; }
        public Color IconColor { get; set; }
        public int BadgeCount { get; set; }
        public bool Badgevisibilty { get; set; }
    }

    public class MoreSectionAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<SectionItem> SectionList = new ObservableCollection<SectionItem>();
        private readonly Activity ActivityContext;
        public MoreSectionAdapter(Activity activityContext)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = activityContext; 
                SectionList.Add(new SectionItem
                {
                    Id = 1,
                    SectionName = activityContext.GetText(Resource.String.Lbl_MyProfile),
                    BadgeCount = 0,
                    Badgevisibilty = false,
                    StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                    IconAsImage = Resource.Drawable.icon_more_my_profile,
                    Icon = IonIconsFonts.Happy,
                    IconColor = Color.ParseColor("#047cac")
                });
                if (AppSettings.MessengerIntegration)
                    SectionList.Add(new SectionItem
                    {
                        Id = 2,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Messages),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_chat,
                        Icon = IonIconsFonts.Chatbubbles,
                        IconColor = Color.ParseColor("#03a9f4")
                    });
                if (AppSettings.ShowUserContacts)
                {
                    string name = activityContext.GetText(AppSettings.ConnectivitySystem ==1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends);
                    SectionList.Add(new SectionItem
                    {
                        Id = 3,
                        SectionName = name,
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_following,
                        Icon = IonIconsFonts.People,
                        IconColor = Color.ParseColor("#d80073")
                    }); 
                }
                if (AppSettings.ShowPokes)
                    SectionList.Add(new SectionItem
                    {
                        Id = 4,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Pokes),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_pokes,
                        Icon = IonIconsFonts.Aperture,
                        IconColor = Color.ParseColor("#009688")
                    });
                if (AppSettings.ShowAlbum)
                    SectionList.Add(new SectionItem
                    {
                        Id = 5,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Albums),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Images,
                        IconAsImage = Resource.Drawable.icon_more_albums,
                        IconColor = Color.ParseColor("#8bc34a")
                    });
                if (AppSettings.ShowMyPhoto)
                    SectionList.Add(new SectionItem
                    {
                        Id = 6,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyImages),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_images,
                        Icon = IonIconsFonts.Camera,
                        IconColor = Color.ParseColor("#006064")
                    });
                if (AppSettings.ShowMyVideo)
                    SectionList.Add(new SectionItem
                    {
                        Id = 7,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyVideos),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_video,
                        Icon = IonIconsFonts.Film,  
                        IconColor = Color.ParseColor("#8e44ad")
                    });
                if (AppSettings.ShowSavedPost)
                    SectionList.Add(new SectionItem
                    {
                        Id = 8,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Saved_Posts),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_save,
                        Icon = IonIconsFonts.Bookmark,
                        IconColor = Color.ParseColor("#673ab7")
                    });
                if (AppSettings.ShowCommunitiesGroups)
                    SectionList.Add(new SectionItem
                    {
                        Id = 9,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Groups),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        IconAsImage = Resource.Drawable.icon_more_groups,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Apps,
                        IconColor = Color.ParseColor("#03A9F4")
                    });
                if (AppSettings.ShowCommunitiesPages)
                    SectionList.Add(new SectionItem
                    {
                        Id = 10,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Pages),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_page,
                        Icon = IonIconsFonts.Flag,
                        IconColor = Color.ParseColor("#f79f58")
                    });
                if (AppSettings.ShowArticles)
                    SectionList.Add(new SectionItem
                    {
                        Id = 11,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Blogs),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_blog,
                        Icon = IonIconsFonts.IosBook,
                        IconColor = Color.ParseColor("#f35d4d")
                    });
                if (AppSettings.ShowMarket)
                    SectionList.Add(new SectionItem
                    {
                        Id = 12,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Marketplace),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_bags_cat_vector,
                        Icon = IonIconsFonts.IosBriefcase,
                        IconColor = Color.ParseColor("#7d8250")
                    });
                if (AppSettings.ShowPopularPosts)
                    SectionList.Add(new SectionItem
                    {
                        Id = 13,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Popular_Posts),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_popular,
                        Icon = IonIconsFonts.Clipboard,
                        IconColor = Color.ParseColor("#8d73cc")
                    });
                if (AppSettings.ShowEvents)
                    SectionList.Add(new SectionItem
                    {
                        Id = 14,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Events),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_event,
                        Icon = IonIconsFonts.Calendar,
                        IconColor = Color.ParseColor("#f25e4e")
                    });
                if (AppSettings.ShowNearBy)
                    SectionList.Add(new SectionItem
                    {
                        Id = 15,
                        SectionName = activityContext.GetText(Resource.String.Lbl_FindFriends),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_nearby,
                        Icon = IonIconsFonts.Pin,
                        IconColor = Color.ParseColor("#b2c17c")
                    });
                if (AppSettings.ShowOffers)
                    SectionList.Add(new SectionItem
                    {
                        Id = 82,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Offers),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_offer_cat_vector,
                        Icon = IonIconsFonts.Pricetag,
                        IconColor = Color.ParseColor("#673AB7")
                    });
                if (AppSettings.ShowMovies)
                    SectionList.Add(new SectionItem
                    {
                        Id = 16,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Movies),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_movies,
                        Icon = IonIconsFonts.Film,
                        IconColor = Color.ParseColor("#8d73cc")
                    });

                if (AppSettings.ShowJobs)
                    SectionList.Add(new SectionItem
                    {
                        Id = 17,
                        SectionName = activityContext.GetText(Resource.String.Lbl_jobs),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_jobs ,
                        Icon = IonIconsFonts.IosBriefcase,
                        IconColor = Color.ParseColor("#4caf50")
                    });

                if (AppSettings.ShowCommonThings)
                    SectionList.Add(new SectionItem
                    {
                        Id = 18,
                        SectionName = activityContext.GetText(Resource.String.Lbl_common_things),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_common_things,
                        Icon = IonIconsFonts.CheckmarkCircle,
                        IconColor = Color.ParseColor("#ff5991")
                    });
                if (AppSettings.ShowMemories)
                    SectionList.Add(new SectionItem
                    {
                        Id = 80,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Memories),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_cat_history_vector,
                        Icon = IonIconsFonts.Timer,
                        IconColor = Color.ParseColor("#673AB7")
                    });
                if (AppSettings.ShowFundings)
                    SectionList.Add(new SectionItem
                    {
                        Id = 19,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Funding),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_more_funding,
                        Icon = IonIconsFonts.LogoUsd,
                        IconColor = Color.ParseColor("#673AB7")
                    });
                if (AppSettings.ShowGames)
                    SectionList.Add(new SectionItem
                    {
                        Id = 20,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Games),
                        BadgeCount = 0,
                        StyleRow = AppSettings.MoreTheme == MoreTheme.BeautyTheme ? 0 : 1,
                        Badgevisibilty = false,
                        IconAsImage = Resource.Drawable.icon_cat_gamepad_vector,
                        Icon = IonIconsFonts.LogoGameControllerB,
                        IconColor = Color.ParseColor("#03A9F4")
                    });
                //Settings Page
                if (AppSettings.ShowSettingsGeneralAccount)
                    SectionList.Add(new SectionItem
                    {
                        Id = 21,
                        SectionName = activityContext.GetText(Resource.String.Lbl_GeneralAccount),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Settings,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsPrivacy)
                    SectionList.Add(new SectionItem
                    {
                        Id = 22,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Privacy),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Eye,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsNotification)
                    SectionList.Add(new SectionItem
                    {
                        Id = 23,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Notifications),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Notifications,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsInvitationLinks)
                    SectionList.Add(new SectionItem
                    {
                        Id = 24,
                        SectionName = activityContext.GetText(Resource.String.Lbl_InvitationLinks),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Link,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsMyInformation)
                    SectionList.Add(new SectionItem
                    {
                        Id = 25,
                        SectionName = activityContext.GetText(Resource.String.Lbl_MyInformation),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosPaper,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsInviteFriends)
                    SectionList.Add(new SectionItem
                    {
                        Id = 26,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Earnings),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.IosHome,
                        IconColor = Color.ParseColor("#757575")
                    });
                if (AppSettings.ShowSettingsHelpSupport)
                    SectionList.Add(new SectionItem
                    {
                        Id = 27,
                        SectionName = activityContext.GetText(Resource.String.Lbl_Help_Support),
                        BadgeCount = 0,
                        StyleRow = 1,
                        Badgevisibilty = false,
                        Icon = IonIconsFonts.Help,
                        IconColor = Color.ParseColor("#757575")
                    });
                SectionList.Add(new SectionItem
                {
                    Id = 28,
                    SectionName = activityContext.GetText(Resource.String.Lbl_Logout),
                    BadgeCount = 0,
                    StyleRow = 1,
                    Badgevisibilty = false,
                    Icon = IonIconsFonts.LogOut,
                    IconColor = Color.ParseColor("#d50000")
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowOrHideBadgeViewIcon(MoreSectionAdapterViewHolderTheme2 viewHolderTheme2 , int countMessages = 0, bool show = false)
        {
            try
            {
                ActivityContext?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (viewHolderTheme2?.LinearLayoutImage != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                QBadgeView badge = new QBadgeView(ActivityContext);
                                badge.BindTarget(viewHolderTheme2.LinearLayoutImage);
                                badge.SetBadgeNumber(countMessages);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            new QBadgeView(ActivityContext).BindTarget(viewHolderTheme2?.LinearLayoutImage).Hide(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => SectionList?.Count ?? 0;
 
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemClick;
        public event EventHandler<MoreSectionAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView;

                switch (viewType)
                {
                    case 1:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection2_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolderTheme2(itemView, Click, LongClick);
                        return vh;
                    }
                    case 2:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolder(itemView, Click, LongClick);
                        return vh;
                    } 
                    default:
                    {
                        itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_MoreSection_view, parent, false);
                        var vh = new MoreSectionAdapterViewHolder(itemView, Click, LongClick);
                        return vh;
                    }
                }
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
                switch (viewHolder)
                {
                    case MoreSectionAdapterViewHolderTheme2 holder:
                    {
                        if (AppSettings.FlowDirectionRightToLeft)
                        {
                            holder.LinearLayoutImage.LayoutDirection = LayoutDirection.Rtl;
                            holder.LinearLayoutMain.LayoutDirection = LayoutDirection.Rtl;
                            holder.Name.LayoutDirection = LayoutDirection.Rtl;
                        }


                        var item = SectionList[position];
                        if (item != null)
                        { 
                            //FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.Icon, item.Icon);
                            //holder.Icon.SetTextColor(item.IconColor);
                            holder.Name.Text = item.SectionName;

                            if (item.IconAsImage != 0)
                                holder.Icon.SetImageResource(item.IconAsImage);
                       
                            if (item.BadgeCount != 0 && item.Id == 2 && item.Badgevisibilty) 
                            {
                                ShowOrHideBadgeViewIcon(holder, item.BadgeCount, true);
                                holder.Name.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            }
                            else if (item.Id == 2)
                            {
                                ShowOrHideBadgeViewIcon(holder);
                            }
                        }

                        break;
                    }
                    case MoreSectionAdapterViewHolder holder2:
                    {
                        if (AppSettings.FlowDirectionRightToLeft)
                        {
                            holder2.LinearLayoutImage.LayoutDirection = LayoutDirection.Rtl;
                            holder2.LinearLayoutMain.LayoutDirection = LayoutDirection.Rtl;
                            holder2.Name.LayoutDirection = LayoutDirection.Rtl;
                        }
                         
                        var item = SectionList[position];
                        if (item != null)
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder2.Icon, item.Icon);
                            holder2.Icon.SetTextColor(item.IconColor);
                            holder2.Name.Text = item.SectionName;

                            if (item.BadgeCount != 0 && item.Id == 2)
                            {
                                var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound(item.BadgeCount.ToString(), Color.ParseColor(AppSettings.MainColor));
                                holder2.Badge.SetImageDrawable(drawable);
                            }

                            holder2.Badge.Visibility = item.Badgevisibilty ? ViewStates.Visible : ViewStates.Invisible;
                        }

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public SectionItem GetItem(int position)
        {
            return SectionList[position];
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
                return SectionList[position].StyleRow switch
                {
                    0 => 1,
                    1 => 2,
                    2 => 3,
                    _ => 2
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(MoreSectionAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MoreSectionAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class MoreSectionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MoreSectionAdapterViewHolder(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener, Action<MoreSectionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = MainView.FindViewById<LinearLayout>(Resource.Id.main);
                LinearLayoutImage = MainView.FindViewById<RelativeLayout>(Resource.Id.imagecontainer);

                Icon = MainView.FindViewById<TextView>(Resource.Id.Icon);
                Name = MainView.FindViewById<TextView>(Resource.Id.section_name);
                Badge = MainView.FindViewById<ImageView>(Resource.Id.badge);

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                Console.WriteLine(longClickListener);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public RelativeLayout LinearLayoutImage { get; private set; }
        public TextView Icon { get; private set; }
        public TextView Name { get; private set; }
        public ImageView Badge { get; private set; }


    }

    public class MoreSectionAdapterViewHolderTheme2 : RecyclerView.ViewHolder
    {
        public MoreSectionAdapterViewHolderTheme2(View itemView, Action<MoreSectionAdapterClickEventArgs> clickListener,Action<MoreSectionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LinearLayoutMain = MainView.FindViewById<LinearLayout>(Resource.Id.main);
                LinearLayoutImage = MainView.FindViewById<RelativeLayout>(Resource.Id.imagecontainer);

                BackIcon = MainView.FindViewById<View>(Resource.Id.backIcon);
                Name = MainView.FindViewById<TextView>(Resource.Id.section_name);
                Icon = MainView.FindViewById<ImageView>(Resource.Id.Icon);

                itemView.Click += (sender, e) => clickListener(new MoreSectionAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                Console.WriteLine(longClickListener);

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public View MainView { get; }

        public LinearLayout LinearLayoutMain { get; private set; }
        public RelativeLayout LinearLayoutImage { get; private set; }
        public View BackIcon { get; private set; }
        public TextView Name { get; private set; }
        public ImageView Icon { get; private set; }

        
    }
    
    public class MoreSectionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}