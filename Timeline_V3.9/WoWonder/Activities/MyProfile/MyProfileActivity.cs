using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.SwipeRefreshLayout.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using TheArtOfDev.Edmodo.Cropper;
using Com.Tuyenmonkey.Textdecorator;
using Google.Android.Material.AppBar;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using Refractored.Controls;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.General;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.Privacy;
using WoWonder.Activities.SettingsPreferences.TellFriend;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyProfileActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private CollapsingToolbarLayout CollapsingToolbar;
        private AppBarLayout AppBarLayout;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private CircleButton BtnEditDataUser, BtnEditImage, BtnMore;
        private TextView TxtUsername, TxtFollowers, TxtFollowing, TxtPoints;
        private TextView TxtCountFollowers, TxtCountLikes, TxtCountFollowing, TxtCountPoints, TxtWalletNumber;
        private ImageView UserProfileImage, CoverImage, IconBack;
        private LinearLayout LayoutCountFollowers, LayoutCountFollowing, LayoutCountLikes, CountPointsLayout, HeaderSection, WalletSection;
        private CircleImageView OnlineView;
        private View ViewPoints, ViewLikes, ViewFollowers;
        private string SUrlUser, SProType, ImageType;
        private FeedCombiner Combiner;
        private AdView MAdView;
        private static MyProfileActivity Instance;
        private UserDataObject UserData;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                 
                Methods.App.FullScreenApp(this);
                 
                // Create your application here
                SetContentView(Resource.Layout.MyProfile_Layout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetMyInfoData();
                PostClickListener.OpenMyProfile = true;

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                MAdView?.Resume();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                MAdView?.Pause();
                MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            { 
                MainRecyclerView.ReleasePlayer();
                PostClickListener.OpenMyProfile = false;
                MAdView?.Destroy(); 
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);

                BtnEditDataUser = (CircleButton)FindViewById(Resource.Id.AddUserbutton);
                BtnEditImage = (CircleButton)FindViewById(Resource.Id.message_button);
                BtnMore = (CircleButton)FindViewById(Resource.Id.morebutton);
                IconBack = (ImageView)FindViewById(Resource.Id.back);
                TxtUsername = (TextView)FindViewById(Resource.Id.username_profile);
                TxtCountFollowers = (TextView)FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)FindViewById(Resource.Id.CountFollowing);
                TxtCountLikes = (TextView)FindViewById(Resource.Id.CountLikes);
                TxtFollowers = FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = FindViewById<TextView>(Resource.Id.txtFollowing);
                UserProfileImage = (ImageView)FindViewById(Resource.Id.profileimage_head);
                CoverImage = (ImageView)FindViewById(Resource.Id.cover_image);
                OnlineView = FindViewById<CircleImageView>(Resource.Id.online_view);
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                HeaderSection = FindViewById<LinearLayout>(Resource.Id.headerSection);
                LayoutCountFollowers = FindViewById<LinearLayout>(Resource.Id.CountFollowersLayout);
                LayoutCountFollowing = FindViewById<LinearLayout>(Resource.Id.CountFollowingLayout);
                LayoutCountLikes = FindViewById<LinearLayout>(Resource.Id.CountLikesLayout);
                WalletSection = FindViewById<LinearLayout>(Resource.Id.walletSection);

                TxtWalletNumber = FindViewById<TextView>(Resource.Id.walletnumber);

                TxtCountPoints = (TextView)FindViewById(Resource.Id.CountPoints);
                TxtPoints = FindViewById<TextView>(Resource.Id.txtPoints);
                CountPointsLayout = FindViewById<LinearLayout>(Resource.Id.CountPointsLayout);

                ViewPoints = FindViewById<View>(Resource.Id.ViewPoints);
                ViewLikes = FindViewById<View>(Resource.Id.ViewLikes);
                ViewFollowers = FindViewById<View>(Resource.Id.ViewFollowers);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                if (AppSettings.FlowDirectionRightToLeft)
                    IconBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);

                if (AppSettings.ConnectivitySystem == 1) // Following
                {
                    TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                    TxtFollowing.Text = GetText(Resource.String.Lbl_Following);
                }
                else // Friend
                {
                    TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                    TxtFollowing.Text = GetText(Resource.String.Lbl_Post);
                }

                BtnEditDataUser.Visibility = ViewStates.Visible;
                BtnMore.Visibility = ViewStates.Visible;
                BtnEditImage.Visibility = ViewStates.Visible;

                if (!AppSettings.ShowUserPoint)
                {
                    ViewPoints.Visibility = ViewStates.Gone;
                    CountPointsLayout.Visibility = ViewStates.Gone;

                    HeaderSection.WeightSum = 3;
                }

                if (!AppSettings.ShowWallet)
                    WalletSection.Visibility = ViewStates.Gone;

                OnlineView.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MainRecyclerView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            { 
                PostFeedAdapter = new NativePostAdapter(this, UserDetails.UserId, MainRecyclerView, NativeFeedType.User);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                Combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, this); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    BtnEditDataUser.Click += BtnEditDataUserOnClick;
                    BtnEditImage.Click += BtnEditImageOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    IconBack.Click += IconBackOnClick;
                    LayoutCountFollowers.Click += LayoutCountFollowersOnClick;
                    LayoutCountFollowing.Click += LayoutCountFollowingOnClick;
                    LayoutCountLikes.Click += LayoutCountLikesOnClick;
                    CountPointsLayout.Click += CountPointsLayoutOnClick;
                    WalletSection.Click += WalletSectionOnClick;
                    UserProfileImage.Click += UserProfileImageOnClick;
                    CoverImage.Click += CoverImageOnClick;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    BtnEditDataUser.Click -= BtnEditDataUserOnClick;
                    BtnEditImage.Click -= BtnEditImageOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    IconBack.Click -= IconBackOnClick;
                    LayoutCountFollowers.Click -= LayoutCountFollowersOnClick;
                    LayoutCountFollowing.Click -= LayoutCountFollowingOnClick;
                    LayoutCountLikes.Click -= LayoutCountLikesOnClick;
                    CountPointsLayout.Click -= CountPointsLayoutOnClick;
                    WalletSection.Click -= WalletSectionOnClick;
                    UserProfileImage.Click -= UserProfileImageOnClick;
                    CoverImage.Click -= CoverImageOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static MyProfileActivity GetInstance()
        {
            try
            {
                return  Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                CollapsingToolbar = null!;
                AppBarLayout = null!;
                BtnEditDataUser = null!;
                BtnEditImage = null!;
                BtnMore  = null!;
                IconBack = null!;
                TxtUsername = null!;
                TxtCountFollowers  = null!;
                TxtCountFollowing = null!;
                TxtCountLikes = null!;
                TxtFollowers  = null!;
                TxtFollowing = null!;
                UserProfileImage = null!;
                CoverImage = null!;
                OnlineView = null!;
                PostFeedAdapter = null!;
                MainRecyclerView = null!;
                HeaderSection = null!;
                LayoutCountFollowers  = null!;
                LayoutCountFollowing = null!;
                LayoutCountLikes = null!;
                WalletSection = null!;
                TxtWalletNumber = null!;
                TxtCountPoints = null!;
                TxtPoints = null!;
                CountPointsLayout = null!;
                ViewPoints = null!;
                ViewLikes = null!;
                ViewFollowers = null!;
                SUrlUser = null!;
                ImageType = null!;
                Combiner = null!;
                SwipeRefreshLayout = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                PostFeedAdapter.ListDiffer.Clear();
                PostFeedAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }
 
        //Open image Cover
        private void CoverImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Cover.Split('/').Last(), UserData.Cover);
                if (media.Contains("http"))
                {
                    var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    var file2 = new File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    var intent = new Intent(Intent.ActionPick);
                    intent.SetAction(Intent.ActionView);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    intent.SetDataAndType(photoUri, "image/*");
                    StartActivity(intent);
                }

                //var intent = new Intent(this, typeof(ViewFullPostActivity));
                //intent.PutExtra("Id", UserData.CoverPostId);
                ////intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                //StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open image Avatar
        private void UserProfileImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var media = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, UserData.Avatar.Split('/').Last(), UserData.Avatar);
                if (media.Contains("http"))
                {
                    var intent = new Intent(Intent.ActionView, Uri.Parse(media));
                    StartActivity(intent);
                }
                else
                {
                    var file2 = new File(media);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                    var intent = new Intent(Intent.ActionPick);
                    intent.SetAction(Intent.ActionView);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                    intent.SetDataAndType(photoUri, "image/*");
                    StartActivity(intent);
                }

                //var intent = new Intent(this, typeof(ViewFullPostActivity));
                //intent.PutExtra("Id", UserData.AvatarPostId);
                ////intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                //StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Wallet 
        private void WalletSectionOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(TabbedWalletActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Show Point
        private void CountPointsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyPointsActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Event Show All Page Likes
        private void LayoutCountLikesOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PagesActivity));
                intent.PutExtra("UserID", UserDetails.UserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show All Users Following  
        private void LayoutCountFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                if (LayoutCountFollowing?.Tag?.ToString() == "Following")
                {
                    var intent = new Intent(this, typeof(MyContactsActivity));
                    intent.PutExtra("ContactsType", "Following");
                    intent.PutExtra("UserId", UserDetails.UserId);
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show All Users Followers 
        private void LayoutCountFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyContactsActivity));
                intent.PutExtra("ContactsType", "Followers");
                intent.PutExtra("UserId", UserDetails.UserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                arrayAdapter.Add(GetText(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Activities));
                arrayAdapter.Add(GetText(Resource.String.Lbl_ViewPrivacy));
                arrayAdapter.Add(GetText(Resource.String.Lbl_SettingsAccount));

                if (ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro && SProType != "4")
                    arrayAdapter.Add(GetText(Resource.String.Lbl_upgrade_now));
                 
                dialogList.Title(Resource.String.Lbl_More);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnEditImageOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                arrayAdapter.Add(GetText(Resource.String.Lbl_Avatar));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Cover));
                dialogList.Title(Resource.String.Lbl_changeImage);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnEditDataUserOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(EditMyProfileActivity));
                StartActivityForResult(intent , 5124);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Profile

        private void GetMyInfoData()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    LoadPassedDate(dataUser);
                     
                    if (ListUtils.MyFollowersList.Count > 0 && dataUser.Details.DetailsClass != null)
                        LoadFriendsLayout(new List<UserDataObject>(ListUtils.MyFollowersList), Methods.FunString.FormatPriceValue(Convert.ToInt32(dataUser.Details.DetailsClass.FollowingCount)));
                     
                    PostFeedAdapter.NotifyDataSetChanged();
                }

                PostFeedAdapter.SetLoading();

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi});
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.Get_User_Data(UserDetails.UserId, "user_data,followers,following");

            if (apiStatus != 200 || !(respond is GetUserDataObject result) || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            { 
                LoadPassedDate(result.UserData);

                //if (result.likedPages.Length > 0)
                //    RunOnUiThread(() => { LoadPagesLayout(result.likedPages); });

                //if (result.joinedGroups.Length > 0)
                //    RunOnUiThread(() => { LoadGroupsLayout(result.joinedGroups, IMethods.FunString.FormatPriceValue(Convert.ToInt32(result.UserProfileObject.Details.GroupsCount))); });

                //if (SPrivacyFriend == "0" || result.UserProfileObject?.IsFollowing == "1" && SPrivacyFriend == "1" || SPrivacyFriend == "2")
                    if (result.Following.Count > 0 && result.UserData.Details.DetailsClass != null)
                        RunOnUiThread(() => { LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(Convert.ToInt32(result.UserData.Details.DetailsClass.FollowingCount))); });
                     
                //##Set the AddBox place on Main RecyclerView
                //------------------------------------------------------------------------
                //var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                //var check2 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.GroupsBox);
                var check3 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                //var check4 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.ImagesBox);
                var check5 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                var check6 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);

                if (check3 != null)
                {
                    Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check3) + 1);
                }
                else if (check5 != null)
                {
                    Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check5) + 1);
                }
                else if (check6 != null)
                {
                    Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check6) + 1);
                }
                 
                if (AppSettings.ShowSearchForPosts)
                    Combiner.SearchForPostsView("user");

                //------------------------------------------------------------------------ 
                RunOnUiThread(() =>
                {
                    try
                    {
                        PostFeedAdapter.NotifyDataSetChanged();
                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.Insert_Or_Update_To_MyProfileTable(result.UserData);

                        if (result.Following?.Count > 0)
                            sqlEntity.Insert_Or_Replace_MyContactTable(new ObservableCollection<UserDataObject>(result.Following));

                        if (result.Followers?.Count > 0)
                            ListUtils.MyFollowersList = new ObservableCollection<UserDataObject>(result.Followers);

                        
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
            }
        }

        private void LoadPassedDate(UserDataObject result)
        {
            try
            {
                UserData = result;
                  
                //TxtUsername.Text = result.Name; 
                SUrlUser = result.Url;

                var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                TxtUsername.SetTypeface(font, TypefaceStyle.Normal);

                var textHighLighter = result.Name;
                var textIsPro = string.Empty;
                
                if (result.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (result.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }
                
                var decorator = TextDecorator.Decorate(TxtUsername, textHighLighter);

                if (result.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (result.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.gnt_white, textIsPro);

                SProType = result.ProType;

                decorator.Build();

                if (AppSettings.ShowWallet)
                    TxtWalletNumber.Text = result.Wallet;

                RunOnUiThread(() =>
                {
                    try
                    {
                        if (result.LastseenStatus == "on")
                            OnlineView.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });

                
                var checkAboutBox = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                if (checkAboutBox == null)
                { 
                    Combiner.AboutBoxPostView(WoWonderTools.GetAboutFinal(result), 0);
                    //PostFeedAdapter.ListDiffer.Insert(0, aboutBox);
                    //PostFeedAdapter.NotifyItemInserted(0);
                }
                else
                {
                    checkAboutBox.AboutModel.Description = WoWonderTools.GetAboutFinal(result);
                    //PostFeedAdapter.NotifyItemChanged(0);
                }

                GlideImageLoader.LoadImage(this, result.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                //GlideImageLoader.LoadImage(this, result.Cover, CoverImage, ImageStyle.FitCenter, ImagePlaceholders.Color, false);
                Glide.With(this).Load(result.Cover).Apply(new RequestOptions().Placeholder(Resource.Drawable.Cover_image).Error(Resource.Drawable.Cover_image)).Into(CoverImage);

                //Set Privacy User
                //==================================
                //SPrivacyBirth = result.BirthPrivacy;
                //SPrivacyFollow = result.FollowPrivacy;
                //SPrivacyFriend = result.FriendPrivacy;
                //SPrivacyMessage = result.MessagePrivacy;

                if (result.Details.DetailsClass != null)
                    TxtCountLikes.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.LikesCount));

                //SetProfilePrivacy(result);

                if (AppSettings.ShowUserPoint)
                    TxtCountPoints.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Points));

                if (result.Details.DetailsClass != null)
                {
                    // Following
                    if (AppSettings.ConnectivitySystem == 1)
                    {
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Following);

                        if (result.Details.DetailsClass != null)
                            TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.FollowersCount));

                        if (result.Details.DetailsClass != null)
                            TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.FollowingCount));

                        LayoutCountFollowing.Tag = "Following";
                    }
                    else // Friend
                    {
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Post);

                        if (result.Details.DetailsClass != null) TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.FollowersCount));
                        if (result.Details.DetailsClass != null) TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(result.Details.DetailsClass.PostCount));

                        LayoutCountFollowing.Tag = "Post";
                    }
                }

                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Cover.Split('/').Last(), result.Cover);
                WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, result.Avatar.Split('/').Last(), result.Avatar);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFriendsLayout(List<UserDataObject> followers, string friendsCounter)
        {
            try
            {
                BtnMore.Visibility = ViewStates.Visible;

                var followersClass = new FollowersModelClass
                {
                    TitleHead = GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends),
                    FollowersList = new List<UserDataObject>(followers.Take(12)),
                    More = friendsCounter
                };
                 
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                if (check != null)
                {
                    check.FollowersModel = followersClass;
                }
                else
                {
                    Combiner.FollowersBoxPostView(followersClass, 2);
                    //PostFeedAdapter.ListDiffer.Insert(1, followersBox);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var text = itemString.ToString();
                if (text == GetText(Resource.String.Lbl_Avatar))
                {
                    OpenDialogGallery("Avatar");
                }
                else if (text == GetText(Resource.String.Lbl_Cover))
                {
                    OpenDialogGallery("Cover");
                }
                else if (text == GetText(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLinkToProfile_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_ViewPrivacy))
                {
                    OnViewPrivacy_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Activities))
                {
                    OnMyActivities_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_SettingsAccount))
                {
                    OnSettingsAccount_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_upgrade_now))
                {
                    UpgradeNow_Click();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Cope Link To Profile
        private void OnCopeLinkToProfile_Button_Click()
        {
            try
            {
                Methods.CopyToClipboard(this, SUrlUser); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = UserDetails.Username,
                    Text = "",
                    Url = SUrlUser
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> View Privacy Shortcuts
        private void OnViewPrivacy_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(PrivacyActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> View Privacy Shortcuts
        private void OnMyActivities_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(MyActivitiesActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> General Account 
        private void OnSettingsAccount_Button_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GeneralAccountActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpgradeNow_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(GoProActivity));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Update Image Avatar && Cover

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                ImageType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task Update_Image_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    switch (type)
                    {
                        case "Avatar":
                        {
                            var (apiStatus, respond) = await RequestsAsync.Global.Update_User_Avatar(path);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short)?.Show();

                                    var file2 = new File(path);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(UserProfileImage);


                                    //Set image  
                                    //GlideImageLoader.LoadImage(this, path, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                }
                            }
                            else Methods.DisplayReportResult(this,respond);

                            break;
                        }
                        case "Cover":
                        {
                            var (apiStatus, respond) = await RequestsAsync.Global.Update_User_Cover(path);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short)?.Show();

                                    //Set image 
                                    var file2 = new File(path);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(CoverImage);
                                         
                                    //GlideImageLoader.LoadImage(this, path, CoverImage, ImageStyle.FitCenter, ImagePlaceholders.Drawable);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    //If its from Camera or Gallery
                    case CropImage.CropImageActivityRequestCode:
                    {
                        var result = CropImage.GetActivityResult(data);

                        if (resultCode == Result.Ok)
                        {
                            if (result.IsSuccessful)
                            {
                                var resultUri = result.Uri;

                                if (!string.IsNullOrEmpty(resultUri.Path))
                                {
                                    string pathImg;
                                    switch (ImageType)
                                    {
                                        case "Cover":
                                            pathImg = resultUri.Path;
                                            UserDetails.Cover = pathImg;
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, pathImg) });
                                            break;
                                        case "Avatar":
                                            pathImg = resultUri.Path;
                                            UserDetails.Avatar = pathImg;
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => Update_Image_Api(ImageType, pathImg) });
                                            break;
                                    }
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong),ToastLength.Long)?.Show();
                                }
                            }
                        }

                        break;
                    }
                    //add post
                    case 2500 when resultCode == Result.Ok:
                    {
                        if (!string.IsNullOrEmpty(data.GetStringExtra("itemObject")))
                        {
                            var postData = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("itemObject"));
                            if (postData != null)
                            {
                                var countList = PostFeedAdapter.ItemCount;

                                var combine = new FeedCombiner(postData, PostFeedAdapter.ListDiffer, this);
                                combine.CombineDefaultPostSections("Top");

                                var countIndex = 1;
                                var model1 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                var model2 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                                var model3 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                                var model4 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                                if (model4 != null)
                                    countIndex += PostFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                                else if (model3 != null)
                                    countIndex += PostFeedAdapter.ListDiffer.IndexOf(model3) + 1;
                                else if (model2 != null)
                                    countIndex += PostFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                                else if (model1 != null)
                                    countIndex += PostFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                                else
                                    countIndex = 0;

                                PostFeedAdapter.NotifyItemRangeInserted(countIndex, PostFeedAdapter.ListDiffer.Count - countList);
                            }
                        }
                        else
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts() });
                        }

                        break;
                    }
                    //Edit post
                    case 3950 when resultCode == Result.Ok:
                    {
                        var postId = data.GetStringExtra("PostId") ?? "";
                        var postText = data.GetStringExtra("PostText") ?? "";
                        var diff = PostFeedAdapter.ListDiffer;
                        var dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                        if (dataGlobal.Count > 0)
                        {
                            foreach (var postData in dataGlobal)
                            {
                                postData.PostData.Orginaltext = postText;
                                var index = diff.IndexOf(postData);
                                if (index > -1)
                                {
                                    PostFeedAdapter.NotifyItemChanged(index);
                                }
                            }

                            var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                            if (checkTextSection == null)
                            {
                                var collection = dataGlobal.FirstOrDefault()?.PostData;
                                var item = new AdapterModelsClass
                                {
                                    TypeView = PostModelType.TextSectionPostPart,
                                    Id = Convert.ToInt32((int)PostModelType.TextSectionPostPart + collection?.Id),
                                    PostData = collection,
                                    IsDefaultFeedPost = true
                                };

                                var headerPostIndex = diff.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                if (headerPostIndex > -1)
                                {
                                    diff.Insert(headerPostIndex + 1, item);
                                    PostFeedAdapter.NotifyItemInserted(headerPostIndex + 1);
                                }
                            }
                        }

                        break;
                    }
                    //Edit post product 
                    case 3500 when resultCode == Result.Ok:
                    {
                        if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                        var item = JsonConvert.DeserializeObject<ProductDataObject>(data?.GetStringExtra("itemData") ?? "");
                        if (item != null)
                        {
                            var diff = PostFeedAdapter.ListDiffer;
                            var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                            if (dataGlobal.Count > 0)
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    var index = diff.IndexOf(postData);
                                    if (index > -1)
                                    {
                                        var productUnion = postData.PostData.Product?.ProductClass;
                                        if (productUnion != null) productUnion.Id = item.Id;
                                        productUnion = item;
                                        Console.WriteLine(productUnion);

                                        PostFeedAdapter.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                    }
                                }
                            }
                        }

                        break;
                    }
                    //Edit profile 
                    case 5124 when resultCode == Result.Ok:
                    {
                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                        if (dataUser != null)
                            LoadPassedDate(dataUser);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery(ImageType);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

    }
}