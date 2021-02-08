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
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Tabs;
using WoWonder.Activities.Base;
using WoWonder.Activities.Fundings.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Funding;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FundingActivity : BaseActivity
    {
        #region Variables Basic

        private ViewPager ViewPager;
        public FundingFragment FundingTab;
        public MyFundingFragment MyFundingTab;
        private TabLayout TabLayout;
        private FloatingActionButton ActionButton; 
        private Toolbar ToolBar;
        private static FundingActivity Instance;

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
                SetContentView(Resource.Layout.EventMain_Layout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                StartApiService();
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
                AddOrRemoveFunding(true);
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
                AddOrRemoveFunding(false);
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

                ActionButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                ActionButton.Visibility = ViewStates.Visible;
                ActionButton.SetImageResource(Resource.Drawable.ic_add);
                 
                ViewPager.OffscreenPageLimit = 2;
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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Funding);
                    ToolBar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(ToolBar);
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
         
        public static FundingActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        private void AddOrRemoveFunding(bool addFunding)
        {
            try
            {
                // true +=  // false -=
                if (addFunding)
                {
                    ActionButton.Click += ActionButtonOnClick;
                }
                else
                {
                    ActionButton.Click -= ActionButtonOnClick;
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
                ViewPager = null!;
                TabLayout = null!;
                ActionButton = null!;
                ToolBar = null!;
                Instance = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                FundingTab = new FundingFragment();
                MyFundingTab = new MyFundingFragment();

                var adapter = new MainTabAdapter(SupportFragmentManager);
                adapter.AddFragment(FundingTab, GetText(Resource.String.Lbl_BrowseFunding));
                adapter.AddFragment(MyFundingTab, GetText(Resource.String.Lbl_MyFunding));

                viewPager.CurrentItem = 2;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
           
        #region Events
         
        //Add Funding
        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(CreateFundingActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Funding
        private void StartApiService(string offsetFunding = "0", string offsetMyFunding = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetFunding(offsetFunding), () => GetMyFunding(offsetMyFunding) });
            else
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
        }

        public async Task GetFunding(string offset = "0")
        {
            if (FundingTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                FundingTab.MainScrollEvent.IsLoading = true;
                var countList = FundingTab.MAdapter.FundingList.Count;

                var (respondCode, respondString) = await RequestsAsync.Funding.FetchFunding("10", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchFundingObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = FundingTab.MAdapter.FundingList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    FundingTab.MAdapter.FundingList.Add(item);
                                }

                                RunOnUiThread(() => { FundingTab.MAdapter.NotifyItemRangeInserted(countList, FundingTab.MAdapter.FundingList.Count - countList); });
                            }
                            else
                            {
                                FundingTab.MAdapter.FundingList = new ObservableCollection<FundingDataObject>(result.Data);
                                RunOnUiThread(() => { FundingTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (FundingTab.MAdapter.FundingList.Count > 10 && !FundingTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreFunding), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(() => ShowEmptyPage("GetFunding"));
            }
            else
            {
                FundingTab.Inflated = FundingTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(FundingTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                FundingTab.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task GetMyFunding(string offset = "0")
        {
            if (MyFundingTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MyFundingTab.MainScrollEvent.IsLoading = true;
                var countList = MyFundingTab.MAdapter.FundingList.Count;

                var (respondCode, respondString) = await RequestsAsync.Funding.FetchMyFunding(UserDetails.UserId , "10", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchFundingObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MyFundingTab.MAdapter.FundingList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MyFundingTab.MAdapter.FundingList.Add(item);
                                }

                                RunOnUiThread(() => { MyFundingTab.MAdapter.NotifyItemRangeInserted(countList, MyFundingTab.MAdapter.FundingList.Count - countList); });
                            }
                            else
                            {
                                MyFundingTab.MAdapter.FundingList = new ObservableCollection<FundingDataObject>(result.Data);
                                RunOnUiThread(() => { MyFundingTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MyFundingTab.MAdapter.FundingList.Count > 10 && !MyFundingTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreFunding), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(() => ShowEmptyPage("GetMyFunding"));
            }
            else
            {
                MyFundingTab.Inflated = MyFundingTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(MyFundingTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MyFundingTab.MainScrollEvent.IsLoading = false;
            }
        }


        private void ShowEmptyPage(string type)
        {
            try
            {
                switch (type)
                {
                    case "GetFunding":
                    {
                        FundingTab.MainScrollEvent.IsLoading = false;
                        FundingTab.SwipeRefreshLayout.Refreshing = false;

                        if (FundingTab.MAdapter.FundingList.Count > 0)
                        {
                            FundingTab.MRecycler.Visibility = ViewStates.Visible;
                            FundingTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            FundingTab.MRecycler.Visibility = ViewStates.Gone;

                            if (FundingTab.Inflated == null)
                                FundingTab.Inflated = FundingTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(FundingTab.Inflated, EmptyStateInflater.Type.NoFunding);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                            }
                            FundingTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                    case "GetMyFunding":
                    {
                        MyFundingTab.MainScrollEvent.IsLoading = false;
                        MyFundingTab.SwipeRefreshLayout.Refreshing = false;

                        if (MyFundingTab.MAdapter.FundingList.Count > 0)
                        {
                            MyFundingTab.MRecycler.Visibility = ViewStates.Visible;
                            MyFundingTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MyFundingTab.MRecycler.Visibility = ViewStates.Gone;

                            if (MyFundingTab.Inflated == null)
                                MyFundingTab.Inflated = MyFundingTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(MyFundingTab.Inflated, EmptyStateInflater.Type.NoFunding);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                            }
                            MyFundingTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                FundingTab.MainScrollEvent.IsLoading = false;
                FundingTab.SwipeRefreshLayout.Refreshing = false;
                MyFundingTab.MainScrollEvent.IsLoading = false;
                MyFundingTab.SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
    }
}