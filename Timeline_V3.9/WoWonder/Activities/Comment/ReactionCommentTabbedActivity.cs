using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.PostData.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Comment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ReactionCommentTabbedActivity : BaseActivity
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private ViewPager ViewPager;
        private TabLayout TabLayout;

        private AngryReactionFragment AngryTab;
        private HahaReactionFragment HahaTab;
        private LikeReactionFragment LikeTab;
        private LoveReactionFragment LoveTab;
        private SadReactionFragment SadTab;
        private WowReactionFragment WowTab;

        private string Id = "", TypeReaction = "Like", TypeClass;
        private GetCommentObject CommentObject;
         
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
                SetContentView(Resource.Layout.PostReactionsLayout);

                TypeClass = Intent?.GetStringExtra("TypeClass") ?? "comment";
                TypeReaction = "Like";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                StartApiService();  
                AdsGoogle.Ad_RewardedVideo(this);
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
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                ViewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                ViewPager.PageScrolled += ViewPager_PageScrolled;
                ViewPager.PageSelected += ViewPagerOnPageSelected;


                ViewPager.OffscreenPageLimit = 6;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);
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
                    toolbar.Title = GetString(Resource.String.Lbl_CommentReaction);
                    toolbar.SetTitleTextColor(Color.White);
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
        private void DestroyBasic()
        {
            try
            {
                

                Adapter = null!;
                ViewPager = null!;
                TabLayout = null!;
                AngryTab = null!;
                HahaTab = null!;
                LikeTab = null!;
                LoveTab = null!;
                SadTab = null!;
                WowTab = null!;
                Id = null!;
                TypeReaction = null!;
                CommentObject = null!;
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Set Tab

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            try
            {
                var position = e.Position;
                TypeReaction = position switch
                {
                    0 => "Like",
                    1 => "Love",
                    2 => "Haha",
                    3 => "Wow",
                    4 => "Sad",
                    5 => "Angry",
                    _ => "Like"
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewPager_PageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            try
            {
                var position = e.Position;
                TypeReaction = position switch
                {
                    0 => "Like",
                    1 => "Love",
                    2 => "Haha",
                    3 => "Wow",
                    4 => "Sad",
                    5 => "Angry",
                    _ => "Like"
                };
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                CommentObject = JsonConvert.DeserializeObject<GetCommentObject>(Intent?.GetStringExtra("CommentObject") ?? "");
                if (CommentObject != null)
                {
                    Id = CommentObject.Id;

                    Adapter = new MainTabAdapter(SupportFragmentManager);

                    if (CommentObject.Reaction.Count > 0)
                    {
                        LikeTab = new LikeReactionFragment();
                        LoveTab = new LoveReactionFragment();
                        HahaTab = new HahaReactionFragment();
                        WowTab = new WowReactionFragment();
                        SadTab = new SadReactionFragment();
                        AngryTab = new AngryReactionFragment();

                        Bundle args = new Bundle();
                        args.PutString("NamePage", TypeClass);

                        LikeTab = new LikeReactionFragment();
                        LoveTab = new LoveReactionFragment();
                        HahaTab = new HahaReactionFragment();
                        WowTab = new WowReactionFragment();
                        SadTab = new SadReactionFragment();
                        AngryTab = new AngryReactionFragment();

                        LikeTab.Arguments = args;
                        LoveTab.Arguments = args;
                        HahaTab.Arguments = args;
                        WowTab.Arguments = args;
                        SadTab.Arguments = args;
                        AngryTab.Arguments = args;

                        Adapter.AddFragment(LikeTab, GetText(Resource.String.Btn_Likes));
                        Adapter.AddFragment(LoveTab, GetText(Resource.String.Btn_Love));
                        Adapter.AddFragment(HahaTab, GetText(Resource.String.Btn_Haha));
                        Adapter.AddFragment(WowTab, GetText(Resource.String.Btn_Wow));
                        Adapter.AddFragment(SadTab, GetText(Resource.String.Btn_Sad));
                        Adapter.AddFragment(AngryTab, GetText(Resource.String.Btn_Angry));
                    }//wael
                    //else
                    //{
                    //    if (PostData.Reaction.Like > 0 || PostData.Reaction.Like1 > 0)
                    //    {
                    //        LikeTab = new LikeReactionFragment();
                    //        Adapter.AddFragment(LikeTab, GetText(Resource.String.Btn_Likes));
                    //    }

                    //    if (PostData.Reaction.Love > 0 || PostData.Reaction.Love2 > 0)
                    //    {
                    //        LoveTab = new LoveReactionFragment();
                    //        Adapter.AddFragment(LoveTab, GetText(Resource.String.Btn_Love));
                    //    }

                    //    if (PostData.Reaction.HaHa > 0 || PostData.Reaction.HaHa3 > 0)
                    //    {
                    //        HahaTab = new HahaReactionFragment();
                    //        Adapter.AddFragment(HahaTab, GetText(Resource.String.Btn_Haha));
                    //    }

                    //    if (PostData.Reaction.Wow > 0 || PostData.Reaction.Wow4 > 0)
                    //    {
                    //        WowTab = new WowReactionFragment();
                    //        Adapter.AddFragment(WowTab, GetText(Resource.String.Btn_Wow));
                    //    }

                    //    if (PostData.Reaction.Sad > 0 || PostData.Reaction.Sad5 > 0)
                    //    {
                    //        SadTab = new SadReactionFragment();
                    //        Adapter.AddFragment(SadTab, GetText(Resource.String.Btn_Sad));
                    //    }

                    //    if (PostData.Reaction.Angry > 0 || PostData.Reaction.Angry6 > 0)
                    //    {
                    //        AngryTab = new AngryReactionFragment();
                    //        Adapter.AddFragment(AngryTab, GetText(Resource.String.Btn_Angry));
                    //    }
                    //}

                    viewPager.CurrentItem = Adapter.Count;
                    viewPager.Adapter = Adapter;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion Set Tab

        #region Load data comment 
         
        public void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataPostAsync(offset) });
        }

        private async Task LoadDataPostAsync(string offset = "0")
        {
            if (LikeTab != null && LikeTab.MainScrollEvent.IsLoading)
                return;

            if (LoveTab != null && LoveTab.MainScrollEvent.IsLoading)
                return;

            if (WowTab != null && WowTab.MainScrollEvent.IsLoading)
                return;

            if (HahaTab != null && HahaTab.MainScrollEvent.IsLoading)
                return;

            if (SadTab != null && SadTab.MainScrollEvent.IsLoading)
                return;

            if (AngryTab != null && AngryTab.MainScrollEvent.IsLoading)
                return;

            if (LikeTab != null)
                LikeTab.MainScrollEvent.IsLoading = true;
            if (LoveTab != null)
                LoveTab.MainScrollEvent.IsLoading = true;
            if (WowTab != null)
                WowTab.MainScrollEvent.IsLoading = true;
            if (HahaTab != null)
                HahaTab.MainScrollEvent.IsLoading = true;
            if (SadTab != null)
                SadTab.MainScrollEvent.IsLoading = true;
            if (AngryTab != null)
                AngryTab.MainScrollEvent.IsLoading = true;
             
            var (apiStatus, respond) = await RequestsAsync.Comment.GetCommentReactionsAsync(Id, TypeClass.ToLower(), "10", TypeReaction, offset);
            if (apiStatus == 200)
            {
                if (respond is PostReactionsObject result)
                {
                    if (LikeTab != null)
                    {
                        int countLikeUserList = LikeTab?.MAdapter?.UserList?.Count ?? 0;

                        //Like
                        var respondListLike = result.Data.Like.Count;
                        if (respondListLike > 0)
                        {
                            var dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Likes)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(LikeTab, GetText(Resource.String.Btn_Likes));

                            if (countLikeUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Like let check = LikeTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    LikeTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { LikeTab.MAdapter.NotifyItemRangeInserted(countLikeUserList - 1, LikeTab.MAdapter.UserList.Count - countLikeUserList); });
                            }
                            else
                            {
                                LikeTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Like);
                                RunOnUiThread(() => { LikeTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (LikeTab.MAdapter.UserList.Count > 10 && !LikeTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }

                    if (LoveTab != null)
                    {
                        int countLoveUserList = LoveTab?.MAdapter?.UserList?.Count ?? 0;

                        //Love
                        var respondListLove = result.Data.Love.Count;
                        if (respondListLove > 0)
                        {
                            var dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Love)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(LoveTab, GetText(Resource.String.Btn_Love));

                            if (countLoveUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Love let check = LoveTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    LoveTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { LoveTab.MAdapter.NotifyItemRangeInserted(countLoveUserList - 1, LoveTab.MAdapter.UserList.Count - countLoveUserList); });
                            }
                            else
                            {
                                LoveTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Love);
                                RunOnUiThread(() => { LoveTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (LoveTab.MAdapter.UserList.Count > 10 && !LoveTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }

                    if (WowTab != null)
                    {
                        int countWowUserList = WowTab?.MAdapter?.UserList?.Count ?? 0;

                        //Wow
                        var respondListWow = result.Data.Wow.Count;
                        if (respondListWow > 0)
                        {
                            var dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Wow)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(WowTab, GetText(Resource.String.Btn_Wow));

                            if (countWowUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Wow let check = WowTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    WowTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { WowTab.MAdapter.NotifyItemRangeInserted(countWowUserList - 1, WowTab.MAdapter.UserList.Count - countWowUserList); });
                            }
                            else
                            {
                                WowTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Wow);
                                RunOnUiThread(() => { WowTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (WowTab.MAdapter.UserList.Count > 10 && !WowTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }

                    if (HahaTab != null)
                    {
                        int countHahaUserList = HahaTab?.MAdapter?.UserList?.Count ?? 0;

                        //Haha
                        var respondListHaha = result.Data.Haha.Count;
                        if (respondListHaha > 0)
                        {
                            var dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Haha)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(HahaTab, GetText(Resource.String.Btn_Haha));

                            if (countHahaUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Haha let check = HahaTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    HahaTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { HahaTab.MAdapter.NotifyItemRangeInserted(countHahaUserList - 1, HahaTab.MAdapter.UserList.Count - countHahaUserList); });
                            }
                            else
                            {
                                HahaTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Haha);
                                RunOnUiThread(() => { HahaTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (HahaTab.MAdapter.UserList.Count > 10 && !HahaTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }

                    if (SadTab != null)
                    {
                        int countSadUserList = SadTab?.MAdapter?.UserList?.Count ?? 0;

                        //Sad
                        var respondListSad = result.Data.Sad.Count;
                        if (respondListSad > 0)
                        {
                            var dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Sad)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(SadTab, GetText(Resource.String.Btn_Sad));

                            if (countSadUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Sad let check = SadTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    SadTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { SadTab.MAdapter.NotifyItemRangeInserted(countSadUserList - 1, SadTab.MAdapter.UserList.Count - countSadUserList); });
                            }
                            else
                            {
                                SadTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Sad);
                                RunOnUiThread(() => { SadTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (SadTab.MAdapter.UserList.Count > 10 && !SadTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }

                    if (AngryTab != null)
                    {
                        int countAngryUserList = AngryTab?.MAdapter?.UserList?.Count ?? 0;

                        //Angry
                        var respondListAngry = result.Data.Angry.Count;
                        if (respondListAngry > 0)
                        {
                            string dataTab = Adapter.FragmentNames.FirstOrDefault(a => a.Contains(GetText(Resource.String.Btn_Angry)));
                            if (string.IsNullOrEmpty(dataTab))
                                Adapter.AddFragment(AngryTab, GetText(Resource.String.Btn_Angry));

                            if (countAngryUserList > 0)
                            {
                                foreach (var item in from item in result.Data.Angry let check = AngryTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    AngryTab.MAdapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { AngryTab.MAdapter.NotifyItemRangeInserted(countAngryUserList - 1, AngryTab.MAdapter.UserList.Count - countAngryUserList); });
                            }
                            else
                            {
                                AngryTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Data.Angry);
                                RunOnUiThread(() => { AngryTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (AngryTab.MAdapter.UserList.Count > 10 && !AngryTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short)?.Show();
                        }
                    }
                }
            }
            else Methods.DisplayReportResult(this, respond);

            RunOnUiThread(ShowEmptyPage);

            if (LikeTab != null)
                LikeTab.MainScrollEvent.IsLoading = false;
            if (LoveTab != null)
                LoveTab.MainScrollEvent.IsLoading = false;
            if (WowTab != null)
                WowTab.MainScrollEvent.IsLoading = false;
            if (HahaTab != null)
                HahaTab.MainScrollEvent.IsLoading = false;
            if (SadTab != null)
                SadTab.MainScrollEvent.IsLoading = false;
            if (AngryTab != null)
                AngryTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (LikeTab != null)
                    LikeTab.MainScrollEvent.IsLoading = false;
                if (LoveTab != null)
                    LoveTab.MainScrollEvent.IsLoading = false;
                if (WowTab != null)
                    WowTab.MainScrollEvent.IsLoading = false;
                if (HahaTab != null)
                    HahaTab.MainScrollEvent.IsLoading = false;
                if (SadTab != null)
                    SadTab.MainScrollEvent.IsLoading = false;
                if (AngryTab != null)
                    AngryTab.MainScrollEvent.IsLoading = false;

                if (Adapter.Count != ViewPager.Adapter.Count)
                {
                    ViewPager.CurrentItem = Adapter.Count;
                    ViewPager.Adapter = Adapter;
                    ViewPager.Adapter.NotifyDataSetChanged();
                }

                if (LikeTab != null)
                {
                    if (LikeTab.SwipeRefreshLayout.Refreshing)
                        LikeTab.SwipeRefreshLayout.Refreshing = false;

                    if (LikeTab.MAdapter.UserList.Count > 0)
                    {
                        LikeTab.MRecycler.Visibility = ViewStates.Visible;
                        LikeTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        LikeTab.MRecycler.Visibility = ViewStates.Gone;

                        LikeTab.Inflated ??= LikeTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(LikeTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        LikeTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }

                if (LoveTab != null)
                {
                    if (LoveTab.SwipeRefreshLayout.Refreshing)
                        LoveTab.SwipeRefreshLayout.Refreshing = false;

                    if (LoveTab.MAdapter.UserList.Count > 0)
                    {
                        LoveTab.MRecycler.Visibility = ViewStates.Visible;
                        LoveTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        LoveTab.MRecycler.Visibility = ViewStates.Gone;

                        LoveTab.Inflated ??= LoveTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(LoveTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        LoveTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }

                if (WowTab != null)
                {
                    if (WowTab.SwipeRefreshLayout.Refreshing)
                        WowTab.SwipeRefreshLayout.Refreshing = false;

                    if (WowTab.MAdapter.UserList.Count > 0)
                    {
                        WowTab.MRecycler.Visibility = ViewStates.Visible;
                        WowTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        WowTab.MRecycler.Visibility = ViewStates.Gone;

                        WowTab.Inflated ??= WowTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(WowTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        WowTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }

                if (HahaTab != null)
                {
                    if (HahaTab.SwipeRefreshLayout.Refreshing)
                        HahaTab.SwipeRefreshLayout.Refreshing = false;

                    if (HahaTab.MAdapter.UserList.Count > 0)
                    {
                        HahaTab.MRecycler.Visibility = ViewStates.Visible;
                        HahaTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        HahaTab.MRecycler.Visibility = ViewStates.Gone;

                        HahaTab.Inflated ??= HahaTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(HahaTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        HahaTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }

                if (SadTab != null)
                {
                    if (SadTab.SwipeRefreshLayout.Refreshing)
                        SadTab.SwipeRefreshLayout.Refreshing = false;

                    if (SadTab.MAdapter.UserList.Count > 0)
                    {
                        SadTab.MRecycler.Visibility = ViewStates.Visible;
                        SadTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        SadTab.MRecycler.Visibility = ViewStates.Gone;

                        SadTab.Inflated ??= SadTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(SadTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        SadTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }

                if (AngryTab != null)
                {
                    if (AngryTab.SwipeRefreshLayout.Refreshing)
                        AngryTab.SwipeRefreshLayout.Refreshing = false;

                    if (AngryTab.MAdapter.UserList.Count > 0)
                    {
                        AngryTab.MRecycler.Visibility = ViewStates.Visible;
                        AngryTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        AngryTab.MRecycler.Visibility = ViewStates.Gone;

                        AngryTab.Inflated ??= AngryTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(AngryTab.Inflated, EmptyStateInflater.Type.NoUsersReaction);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        AngryTab.EmptyStateLayout.Visibility = ViewStates.Visible;
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