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
using AndroidX.SwipeRefreshLayout.Widget;
using WoWonder.Activities.Base;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Search.Adapters;
using WoWonder.Activities.Suggested.Adapters;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Suggested.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SuggestedGroupActivity : BaseActivity
    {
        #region Variables Basic

        private SuggestedGroupAdapter MAdapter;
        private SearchGroupAdapter RandomAdapter;
        private CategoriesImageAdapter CategoriesAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;

        private ViewStub EmptyStateLayout, SuggestedGroupViewStub, CatGroupViewStub, RandomGroupViewStub;
        private View Inflated, SuggestedGroupInflated, CatGroupInflated, RandomGroupInflated;
        private TemplateRecyclerInflater RecyclerInflaterSuggestedGroup,RecyclerInflaterCatGroup,RecyclerInflaterRandomGroup;
        private RecyclerViewOnScrollListener SuggestedGroupScrollEvent;

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
                SetContentView(Resource.Layout.SuggestedGroupLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                StartApiService();
                AdsGoogle.Ad_Interstitial(this);
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
                if (MAdapter.GroupList.Count > 0)
                    ListUtils.SuggestedGroupList = MAdapter.GroupList;

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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SuggestedGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubSuggestedGroup);
                CatGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubCatGroup);
                RandomGroupViewStub = FindViewById<ViewStub>(Resource.Id.viewStubRandomGroup);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                 
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
                    toolbar.Title = GetString(Resource.String.Lbl_Discover);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new SuggestedGroupAdapter(this) { GroupList = new ObservableCollection<GroupClass>()  };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.JoinButtonItemClick += MAdapterOnJoinButtonItemClick;

                RandomAdapter = new SearchGroupAdapter(this) { GroupList = new ObservableCollection<GroupClass>() };
                RandomAdapter.ItemClick += RandomAdapterOnItemClick;
                RandomAdapter.JoinButtonItemClick += MAdapterOnJoinButtonItemClick;

                if (CategoriesController.ListCategoriesGroup.Count > 0)
                {
                    CategoriesAdapter = new CategoriesImageAdapter(this) { CategoriesList = CategoriesController.ListCategoriesGroup };
                    CategoriesAdapter.ItemClick += CategoriesAdapterOnItemClick;
                      
                    if (CatGroupInflated == null)
                        CatGroupInflated = CatGroupViewStub.Inflate();

                    RecyclerInflaterCatGroup = new TemplateRecyclerInflater();
                    RecyclerInflaterCatGroup.InflateLayout<Classes.Categories>(this, CatGroupInflated, CategoriesAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_Categories), GetString(Resource.String.Lbl_FindGroupByCategories));

                    RecyclerInflaterCatGroup.Recyler.Visibility = ViewStates.Visible;

                    CategoriesAdapter.NotifyDataSetChanged();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Group");
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
                MAdapter = null!;
                RandomAdapter = null!;
                CategoriesAdapter = null!;
                SwipeRefreshLayout = null!;
                EmptyStateLayout = null!;
                SuggestedGroupViewStub = null!;
                CatGroupViewStub = null!;
                RandomGroupViewStub = null!;
                Inflated = null!;
                SuggestedGroupInflated = null!;
                CatGroupInflated = null!;
                RandomGroupInflated = null!;
                RecyclerInflaterSuggestedGroup = null!;
                RecyclerInflaterCatGroup = null!;
                RecyclerInflaterRandomGroup = null!;
                SuggestedGroupScrollEvent = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Get Group By Categories
        private void CategoriesAdapterOnItemClick(object sender, CategoriesImageAdapterClickEventArgs e)
        {
            try
            {
                var item = CategoriesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(GroupByCategoriesActivity));
                    intent.PutExtra("CategoryId", item.CategoriesId);
                    intent.PutExtra("CategoryName", Methods.FunString.DecodeString(item.CategoriesName));
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //See all SuggestedGroup
        private void MainLinearSuggestedGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AllSuggestedGroupActivity));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Offset Suggested Group
        private void SuggestedGroupScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.GroupList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.GroupId))
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroup(item.GroupId) });   
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile Suggested Group
        private void MAdapterOnItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterOnJoinButtonItemClick(object sender, SuggestedGroupAdapterClickEventArgs e)
        {
            try
            { 

                var item = MAdapter.GetItem(e.Position);
                if (item == null)
                    return;

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                var (apiStatus, respond) = await RequestsAsync.Group.Join_Group(item.GroupId);
                if (apiStatus == 200)
                {
                    if (respond is JoinGroupObject result)
                    {
                        if (result.JoinStatus == "requested")
                        {
                            e.JoinButton.SetTextColor(Color.White);
                            e.JoinButton.Text = Application.Context.GetText(Resource.String.Lbl_Request);
                            e.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                        }
                        else
                        {
                            var isJoined = result.JoinStatus == "left" ? "false" : "true";
                            e.JoinButton.Text = GetText(isJoined == "yes" || isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);

                            if (isJoined == "yes" || isJoined == "true")
                            {
                                e.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                e.JoinButton.SetTextColor(Color.White);
                            }
                            else
                            {
                                e.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                e.JoinButton.SetTextColor(Color.White);
                            }
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnJoinButtonItemClick(object sender, SearchGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetJoinGroup(this, item.GroupId, e.Button);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Open Profile Random Group
        private void RandomAdapterOnItemClick(object sender, SearchGroupAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroup("0") , () => LoadRandomGroup("0") });
        }

        private async Task LoadGroup(string offset)
        {
            if (SuggestedGroupScrollEvent != null && SuggestedGroupScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = true;
                var countList = MAdapter.GroupList.Count;

                var (respondCode, respondString) = await RequestsAsync.Group.GetRecommendedGroups("10", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is ListGroupsObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.GroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                                {
                                    MAdapter.GroupList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.GroupList.Count - countList); });
                            }
                            else
                            { 
                                MAdapter.GroupList = new ObservableCollection<GroupClass>(result.Data);

                                RunOnUiThread(() =>
                                { 
                                    SuggestedGroupInflated ??= SuggestedGroupViewStub.Inflate();

                                    RecyclerInflaterSuggestedGroup = new TemplateRecyclerInflater();
                                    RecyclerInflaterSuggestedGroup.InflateLayout<GroupClass>(this, SuggestedGroupInflated, MAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_SuggestedForYou), "", true);

                                    RecyclerInflaterSuggestedGroup.MainLinear.Click += MainLinearSuggestedGroupOnClick;

                                    if (SuggestedGroupScrollEvent == null)
                                    {
                                        RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterSuggestedGroup.LayoutManager);
                                        SuggestedGroupScrollEvent = playlistRecyclerViewOnScrollListener;
                                        SuggestedGroupScrollEvent.LoadMoreEvent += SuggestedGroupScrollEventOnLoadMoreEvent;
                                        RecyclerInflaterSuggestedGroup.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                        SuggestedGroupScrollEvent.IsLoading = false;
                                    } 
                                }); 
                            }
                        }
                        else
                        {
                            if (RecyclerInflaterSuggestedGroup?.Recyler != null && MAdapter.GroupList.Count > 10 && !RecyclerInflaterSuggestedGroup.Recyler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                     x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
            } 
        }

        private async Task LoadRandomGroup(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = RandomAdapter.GroupList.Count;

                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "30"},
                    {"group_offset", offset},
                    {"search_key", "a"},
                };

                var (respondCode, respondString) = await RequestsAsync.Global.Get_Search(dictionary);
                if (respondCode.Equals(200))
                {
                    if (respondString is GetSearchObject result)
                    {
                        var respondList = result.Groups.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Groups let check = RandomAdapter.GroupList.FirstOrDefault(a => a.GroupId == item.GroupId) where check == null select item)
                                {
                                    RandomAdapter.GroupList.Add(item);
                                }

                                RunOnUiThread(() => { RandomAdapter.NotifyItemRangeInserted(countList, RandomAdapter.GroupList.Count - countList); });
                            }
                            else
                            {
                                RandomAdapter.GroupList = new ObservableCollection<GroupClass>(result.Groups);

                                RunOnUiThread(() =>
                                {
                                    if (RandomGroupInflated == null)
                                        RandomGroupInflated = RandomGroupViewStub.Inflate();

                                    RecyclerInflaterRandomGroup = new TemplateRecyclerInflater();
                                    RecyclerInflaterRandomGroup.InflateLayout<GroupClass>(this, RandomGroupInflated, RandomAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, GetString(Resource.String.Lbl_RandomGroups));
                                });
                            }
                        }
                        else
                        {
                            if (RecyclerInflaterRandomGroup?.Recyler != null && RandomAdapter.GroupList.Count > 10 && !RecyclerInflaterRandomGroup.Recyler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                     x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                if (MAdapter.GroupList.Count > 0)
                {
                    if (RecyclerInflaterSuggestedGroup?.Recyler != null)
                        RecyclerInflaterSuggestedGroup.Recyler.Visibility = ViewStates.Visible;

                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (RandomAdapter.GroupList.Count > 0)
                {
                    if (RecyclerInflaterRandomGroup?.Recyler != null)
                        RecyclerInflaterRandomGroup.Recyler.Visibility = ViewStates.Visible;

                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                
                if (MAdapter.GroupList.Count == 0 && RandomAdapter.GroupList.Count == 0)
                {
                    if (RecyclerInflaterSuggestedGroup?.Recyler != null)
                        RecyclerInflaterSuggestedGroup.Recyler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                         x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                if (SuggestedGroupScrollEvent != null) SuggestedGroupScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
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