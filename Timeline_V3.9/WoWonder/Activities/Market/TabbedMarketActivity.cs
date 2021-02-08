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
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Market.Fragment;
using WoWonder.Activities.NearbyShops;
using WoWonder.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedMarketActivity : BaseActivity
    { 
        #region Variables Basic

        private ViewPager ViewPager;
        public MarketFragment MarketTab;
        public MyProductsFragment MyProductsTab;
        private TabLayout TabLayout;
        private FloatingActionButton FloatingActionButtonView;
        private RecyclerView CatRecyclerView;
        private CategoriesAdapter CategoriesAdapter;
        private SearchView SearchBox;
        private TextView FilterButton;
        private ImageView DiscoverButton;
        private string KeySearch = "";
        private static TabbedMarketActivity Instance;

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
                SetContentView(Resource.Layout.MarketMain_Layout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadDataApi();
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
                if (MarketTab.MAdapter.MarketList.Count > 0)
                    ListUtils.ListCachedDataProduct = MarketTab.MAdapter.MarketList;

                if (MyProductsTab.MAdapter.MarketList.Count > 0)
                    ListUtils.ListCachedDataMyProduct = MyProductsTab.MAdapter.MarketList;

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
         
        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                KeySearch = e.NewText;

                MarketTab.MAdapter.MarketList.Clear();
                MarketTab.MAdapter.NotifyDataSetChanged();

                MarketTab.SwipeRefreshLayout.Refreshing = true;
                MarketTab.SwipeRefreshLayout.Enabled = true;
                 
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarketByKey(KeySearch) });
                else
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                KeySearch = e.NewText;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);
                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);

                CatRecyclerView = FindViewById<RecyclerView>(Resource.Id.catRecyler);

                DiscoverButton = (ImageView)FindViewById(Resource.Id.discoverButton);
                if (!AppSettings.ShowNearbyShops)
                {
                    DiscoverButton.Visibility = ViewStates.Gone;
                }

                FilterButton = (TextView)FindViewById(Resource.Id.filter_icon);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FilterButton, IonIconsFonts.Options);
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
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    
                }
                
                SearchBox = FindViewById<SearchView>(Resource.Id.searchBox);
                SearchBox.SetQuery("", false);
                SearchBox.SetIconifiedByDefault(false);
                SearchBox.OnActionViewExpanded();
                SearchBox.Iconified = false;
                SearchBox.QueryTextChange += SearchViewOnQueryTextChange;
                SearchBox.QueryTextSubmit += SearchViewOnQueryTextSubmit;
                SearchBox.ClearFocus();

                //Change text colors
                var editText = (EditText)SearchBox.FindViewById(Resource.Id.search_src_text);
                editText.SetHintTextColor(Color.White);
                editText.SetTextColor(Color.White);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchBox.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon.Parent;
                linearLayoutSearchView.RemoveView(searchViewIcon);
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
                if (CategoriesController.ListCategoriesProducts.Count > 0)
                {
                    var check = CategoriesController.ListCategoriesProducts.Where(a => a.CategoriesColor == AppSettings.MainColor).ToList();
                    if (check.Count > 0)
                        foreach (var all in check)
                            all.CategoriesColor = "#ffffff";
                     
                    CatRecyclerView.HasFixedSize = true;
                    CatRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                    CategoriesAdapter = new CategoriesAdapter(this)
                    {
                        MCategoriesList = CategoriesController.ListCategoriesProducts,
                    };
                    CatRecyclerView.SetAdapter(CategoriesAdapter);
                    CatRecyclerView.NestedScrollingEnabled = false;
                    CategoriesAdapter.NotifyDataSetChanged();
                    CatRecyclerView.Visibility = ViewStates.Visible;
                    CategoriesAdapter.ItemClick += CategoriesAdapterOnItemClick;
                }
                else
                {
                    CatRecyclerView.Visibility = ViewStates.Gone;
                }
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
                    FloatingActionButtonView.Click += CreateProductOnClick;
                    FilterButton.Click += FilterButtonOnClick;
                    DiscoverButton.Click += DiscoverButtonOnClick;
                }
                else
                {
                    FloatingActionButtonView.Click -= CreateProductOnClick;
                    FilterButton.Click -= FilterButtonOnClick;
                    DiscoverButton.Click -= DiscoverButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static TabbedMarketActivity GetInstance()
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
        private void DestroyBasic()
        {
            try
            {
                ViewPager= null!;
                TabLayout = null!;
                MarketTab = null!;
                MyProductsTab = null!;
                FloatingActionButtonView = null!;
                CatRecyclerView = null!;
                CategoriesAdapter = null!;
                DiscoverButton = null!;
                FilterButton = null!;
                KeySearch = null!;
                Instance = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            { 
                FilterMarketDialogFragment mFragment = new FilterMarketDialogFragment();

                Bundle bundle = new Bundle();
                bundle.PutString("TypeFilter", "Market");

                mFragment.Arguments = bundle;

                mFragment.Show(SupportFragmentManager, mFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CreateProductOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(CreateProductActivity)), 200);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CategoriesAdapterOnItemClick(object sender, CategoriesAdapterClickEventArgs e)
        {
            try
            {
                KeySearch = "";

                MarketTab.MAdapter.MarketList.Clear();
                MarketTab.MAdapter.NotifyDataSetChanged();
                 
                var item = CategoriesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var check = CategoriesAdapter.MCategoriesList.Where(a => a.CategoriesColor == AppSettings.MainColor).ToList();
                    if (check.Count > 0)
                        foreach (var all in check)
                            all.CategoriesColor = "#ffffff";

                    var click = CategoriesAdapter.MCategoriesList.FirstOrDefault(a => a.CategoriesId == item.CategoriesId);
                    if (click != null) click.CategoriesColor = AppSettings.MainColor;

                    CategoriesAdapter.NotifyDataSetChanged();

                    MarketTab.SwipeRefreshLayout.Refreshing = true;
                    MarketTab.SwipeRefreshLayout.Enabled = true;
                     
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarketByKey(KeySearch, item.CategoriesId) });
                    else
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Nearby Shops
        private void DiscoverButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(NearbyShopsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                if (requestCode == 200)
                {
                    if (resultCode == Result.Ok)
                    {
                        if (MarketTab != null)
                        {
                            var result = data.GetStringExtra("product");

                            var item = JsonConvert.DeserializeObject<ProductDataObject>(result);

                            MarketTab.MAdapter.MarketList.Insert(0, item);
                            MarketTab.MAdapter.NotifyItemInserted(0);
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
         
        #region Set Tab

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                MyProductsTab = new MyProductsFragment();
                MarketTab = new MarketFragment();
              
                var adapter = new MainTabAdapter(SupportFragmentManager);
                adapter.AddFragment(MarketTab, GetText(Resource.String.Lbl_Market));
                adapter.AddFragment(MyProductsTab, GetText(Resource.String.Lbl_MyProducts));

                viewPager.CurrentItem = 2;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion Set Tab

        #region Get Market Api 

        private void LoadDataApi()
        {
            try
            {
                string offsetMarket = "0", offsetMyProducts = "0";

                if (MarketTab.MAdapter != null && ListUtils.ListCachedDataProduct.Count > 0)
                {
                    MarketTab.MAdapter.MarketList = ListUtils.ListCachedDataProduct;
                    MarketTab.MAdapter.NotifyDataSetChanged();

                    var item = MarketTab.MAdapter.MarketList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        offsetMarket = item.Id;
                }

                if (MyProductsTab.MAdapter != null && ListUtils.ListCachedDataMyProduct.Count > 0)
                {
                    MyProductsTab.MAdapter.MarketList = ListUtils.ListCachedDataMyProduct;
                    MyProductsTab.MAdapter.NotifyDataSetChanged();

                    var item = MyProductsTab.MAdapter.MarketList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        offsetMyProducts = item.Id;
                }
                  
                StartApiService(offsetMarket, offsetMyProducts);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void StartApiService(string offsetMarket = "0" , string offsetMyProducts = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetMarket(offsetMarket), () => GetMyProducts(offsetMyProducts) });
            else
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
        }

        public async Task GetMarket(string offset = "0")
        {
            if (MarketTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MarketTab.MainScrollEvent.IsLoading = true;

                var countList = MarketTab.MAdapter.MarketList.Count;
                var (apiStatus, respond) = await RequestsAsync.Market.Get_Products("", "10", offset,"","", UserDetails.MarketDistanceCount);
                if (apiStatus.Equals(200))
                {
                    if (respond is GetProductsObject result)
                    {
                        var respondList = result.Products.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Products let check = MarketTab.MAdapter.MarketList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MarketTab.MAdapter.MarketList.Add(item);
                                }

                                RunOnUiThread(() => { MarketTab.MAdapter.NotifyItemRangeInserted(countList , MarketTab.MAdapter.MarketList.Count - countList); });
                            }
                            else
                            {
                                MarketTab.MAdapter.MarketList = new ObservableCollection<ProductDataObject>(result.Products);
                                RunOnUiThread(() => { MarketTab.MAdapter.NotifyDataSetChanged(); }); 
                            }
                        }
                        else
                        {
                            if (MarketTab.MAdapter.MarketList.Count > 10 && !MarketTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);

                RunOnUiThread(() => { ShowEmptyPage("GetMarket");}); 
            }
            else
            {
                MarketTab.Inflated = MarketTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(MarketTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                     x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MarketTab.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task GetMyProducts(string offset = "0")
        {
            if (MyProductsTab.MainScrollEvent.IsLoading)
                return;

            MyProductsTab.MainScrollEvent.IsLoading = true;
            var countList = MyProductsTab.MAdapter.MarketList.Count;
            var (apiStatus, respond) = await RequestsAsync.Market.Get_Products(UserDetails.UserId, "10", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is GetProductsObject result)
                {
                    var respondList = result.Products.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Products let check = MyProductsTab.MAdapter.MarketList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MyProductsTab.MAdapter.MarketList.Add(item);
                            }

                            RunOnUiThread(() => { MyProductsTab.MAdapter.NotifyItemRangeInserted(countList, MyProductsTab.MAdapter.MarketList.Count - countList); });
                        }
                        else
                        {
                            MyProductsTab.MAdapter.MarketList = new ObservableCollection<ProductDataObject>(result.Products);
                            RunOnUiThread(() => { MyProductsTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MyProductsTab.MAdapter.MarketList.Count > 10 && !MyProductsTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                    }
                }
            }
            else Methods.DisplayReportResult(this, respond);

            RunOnUiThread(() => { ShowEmptyPage("GetMyProducts"); }); 
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                switch (type)
                {
                    case "GetMarket":
                    {
                        MarketTab.MainScrollEvent.IsLoading = false;
                        MarketTab.SwipeRefreshLayout.Refreshing = false;

                        if (MarketTab.MAdapter.MarketList.Count > 0)
                        {
                            MarketTab.MRecycler.Visibility = ViewStates.Visible;
                            MarketTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MarketTab.MRecycler.Visibility = ViewStates.Gone;

                            if (MarketTab.Inflated == null)
                                MarketTab.Inflated = MarketTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(MarketTab.Inflated, EmptyStateInflater.Type.NoProduct);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                                x.EmptyStateButton.Click += BtnCreateProductsOnClick;
                            }
                            MarketTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                    case "GetMyProducts":
                    {
                        MyProductsTab.MainScrollEvent.IsLoading = false;
                        MyProductsTab.SwipeRefreshLayout.Refreshing = false;

                        if (MyProductsTab.MAdapter.MarketList.Count > 0)
                        {
                            MyProductsTab.MRecycler.Visibility = ViewStates.Visible;
                            MyProductsTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            MyProductsTab.MRecycler.Visibility = ViewStates.Gone;

                            if (MyProductsTab.Inflated == null)
                                MyProductsTab.Inflated = MyProductsTab.EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(MyProductsTab.Inflated, EmptyStateInflater.Type.NoProduct);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null!;
                                x.EmptyStateButton.Click += BtnCreateProductsOnClick;
                            }
                            MyProductsTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MarketTab.MainScrollEvent.IsLoading = false;
                MarketTab.SwipeRefreshLayout.Refreshing = false;
                MyProductsTab.MainScrollEvent.IsLoading = false;
                MyProductsTab.SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Add New Product  >> CreateProduct_Activity
        private void BtnCreateProductsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(CreateProductActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        private async Task GetMarketByKey(string key = "", string categoriesId = "")
        {
            if (MarketTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MarketTab.MainScrollEvent.IsLoading = true;
                var countList = MarketTab.MAdapter.MarketList.Count;
                var (apiStatus, respond) = await RequestsAsync.Market.Get_Products("", "10", "0", categoriesId, key, UserDetails.MarketDistanceCount);
                if (apiStatus.Equals(200))
                {
                    if (respond is GetProductsObject result)
                    {
                        var respondList = result.Products.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Products let check = MarketTab.MAdapter.MarketList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MarketTab.MAdapter.MarketList.Add(item);
                                }

                                RunOnUiThread(() => { MarketTab.MAdapter.NotifyItemRangeInserted(countList, MarketTab.MAdapter.MarketList.Count - countList); });
                            }
                            else
                            {
                                MarketTab.MAdapter.MarketList = new ObservableCollection<ProductDataObject>(result.Products);
                                RunOnUiThread(() => { MarketTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MarketTab.MAdapter.MarketList.Count > 10 && !MarketTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreProducts), ToastLength.Short)?.Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);

                RunOnUiThread(() => { ShowEmptyPage("GetMarket"); });
            }
            else
            {
                MarketTab.Inflated = MarketTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(MarketTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                     x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MarketTab.MainScrollEvent.IsLoading = false;
            }
        }

        #endregion
         
    }
}