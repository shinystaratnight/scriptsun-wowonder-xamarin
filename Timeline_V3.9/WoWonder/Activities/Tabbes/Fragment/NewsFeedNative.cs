using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;

using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class NewsFeedNative : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private FloatingActionButton PopupBubbleView;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        private Handler MainHandler = new Handler(Looper.MainLooper);
        private TabbedMainActivity GlobalContext; 

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        { 
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your fragment here 
                GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance();

              
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TestNewsFeed, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters();

                LoadPost(true);
                 
                GlobalContext.GetOneSignalNotification(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }
         
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MainRecyclerView = (WRecyclerView)view.FindViewById(Resource.Id.newsfeedRecyler);
                PopupBubbleView = (FloatingActionButton)view.FindViewById(Resource.Id.popup_bubble);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                if (SwipeRefreshLayout != null)
                {
                    SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                    SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                PostFeedAdapter = new NativePostAdapter(Activity, UserDetails.UserId, MainRecyclerView, NativeFeedType.Global);
                MainRecyclerView?.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                if (AppSettings.ShowNewPostOnNewsFeed)
                    MainRecyclerView?.SetXPopupBubble(PopupBubbleView); 
                else
                    PopupBubbleView.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Refresh

        //Refresh 
        public void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                PopupBubbleView.Visibility = ViewStates.Gone;

                PostFeedAdapter.ListDiffer.Clear();  
                PostFeedAdapter.NotifyDataSetChanged();
                 
                MainRecyclerView?.StopVideo();

                var combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, Activity);
                 
                if (AppSettings.ShowStory)
                {
                    combiner.AddStoryPostView();
                }

                combiner.AddPostBoxPostView("feed", -1);

                var checkSectionAlertBox = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                {
                    PostFeedAdapter.ListDiffer.Remove(checkSectionAlertBox);
                }

                var checkSectionAlertJoinBox = PostFeedAdapter.ListDiffer.Where(a => a.TypeView == PostModelType.AlertJoinBox).ToList();
                {
                    foreach (var adapterModelsClass in checkSectionAlertJoinBox)
                    {
                        PostFeedAdapter.ListDiffer.Remove(adapterModelsClass);
                    }
                }
                 
                PostFeedAdapter.NotifyDataSetChanged();
                 
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Post Feed

        public void LoadPost(bool local)
        {
            try
            {
                var combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, Activity);

                combiner.AddStoryPostView();
                combiner.AddPostBoxPostView("feed", -1);
                if (local)
                    combiner.AddGreetingAlertPostView();

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var json = dbDatabase.GetDataPost();

                if (!string.IsNullOrEmpty(json) && local)
                {
                    var postObject = JsonConvert.DeserializeObject<PostObject>(json);
                    if (postObject?.Data.Count > 0)
                    {
                        MainRecyclerView.ApiPostAsync.LoadDataApi(postObject.Status, postObject, "0"); 
                        MainRecyclerView.ScrollToPosition(0); 
                    }

                    //Start Updating the news feed every few minus 
                    StartHandler();
                    return;
                }

                if (PostFeedAdapter.ListDiffer.Count <= 5)
                {
                    StartApiService();
                }
                else
                {
                    var item = PostFeedAdapter.ListDiffer.LastOrDefault();

                    var lastItem = PostFeedAdapter.ListDiffer.IndexOf(item);

                    item = PostFeedAdapter.ListDiffer[lastItem];

                    string offset;
                    if (item.TypeView == PostModelType.Divider || item.TypeView == PostModelType.ViewProgress || item.TypeView == PostModelType.AdMob1 || item.TypeView == PostModelType.AdMob2 || item.TypeView == PostModelType.FbAdNative || item.TypeView == PostModelType.AdsPost || item.TypeView == PostModelType.SuggestedGroupsBox || item.TypeView == PostModelType.SuggestedUsersBox || item.TypeView == PostModelType.CommentSection || item.TypeView == PostModelType.AddCommentSection)
                    {
                        item = PostFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                        offset = item?.PostData?.Id ?? "0";
                        Console.WriteLine(offset);
                    }
                    else
                    {
                        offset = item.PostData?.Id ?? "0";
                    }

                    StartApiService(offset, "Insert");
                }

                //Start Updating the news feed every few minus
                StartHandler();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0" ,string typeRun = "Add")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory, () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts(offset , typeRun) }); 
        }

        public void StartHandler()
        {
            try
            {
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ApiPostUpdaterHelper(Activity, MainRecyclerView, new Handler(Looper.MainLooper)), 30000);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void RemoveHandler()
        {
            try
            {
                MainHandler?.RemoveCallbacks(new ApiPostUpdaterHelper(Activity, MainRecyclerView, new Handler(Looper.MainLooper)));
                MainHandler = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly WRecyclerView MainRecyclerView;
            private readonly Handler MainHandler;
            private readonly Activity Activity;

            public ApiPostUpdaterHelper(Activity activity, WRecyclerView mainRecyclerView, Handler mainHandler)
            {
                MainRecyclerView = mainRecyclerView;
                MainHandler = mainHandler;
                Activity = activity;
            }

            public async void Run()
            {
                try
                {
                    if (string.IsNullOrEmpty(Current.AccessToken))
                        return;

                    await MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts("0", "Insert");
                  
                    var instance = TabbedMainActivity.GetInstance();
                    if (instance != null)
                    {
                        await instance.NewsFeedTab.LoadStory(); 
                        await instance.Get_Notifications();
                    } 
                    //await ApiRequest.Get_MyProfileData_Api(Activity);
                    MainHandler?.PostDelayed(new ApiPostUpdaterHelper(Activity, MainRecyclerView, MainHandler), 30000);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #endregion

        #region Get Story

        private async Task LoadStory()
        {
            if (!AppSettings.ShowStory)
                return;
             
            if (Methods.CheckConnectivity())
            {
                var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    checkSection.StoryList ??= new ObservableCollection<GetUserStoriesObject.StoryObject>();

                    var (apiStatus, respond) = await RequestsAsync.Story.Get_UserStories();
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserStoriesObject result)
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                try
                                { 
                                    foreach (var item in result.Stories)
                                    {
                                        var check = checkSection.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                        if (check != null)
                                        {
                                            foreach (var item2 in item.Stories)
                                            {
                                                item.DurationsList ??= new List<long>();

                                                //image and video
                                                var mediaFile = !item2.Thumbnail.Contains("avatar") && item2.Videos.Count == 0 ? item2.Thumbnail : item2.Videos[0].Filename;

                                                var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                if (type != "Video")
                                                {
                                                    Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                    item.DurationsList.Add(10000L);
                                                }
                                                else
                                                {
                                                    var fileName = mediaFile.Split('/').Last();
                                                    mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                    var duration = WoWonderTools.GetDuration(mediaFile);
                                                    item.DurationsList.Add(Long.ParseLong(duration));
                                                }
                                            }

                                            check.Stories = item.Stories;
                                        }
                                        else
                                        {
                                            foreach (var item1 in item.Stories)
                                            {
                                                item.DurationsList ??= new List<long>();

                                                //image and video
                                                var mediaFile = !item1.Thumbnail.Contains("avatar") && item1.Videos.Count == 0 ? item1.Thumbnail : item1.Videos[0].Filename;

                                                var type1 = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                if (type1 != "Video")
                                                {
                                                    Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                    item.DurationsList.Add(10000L);
                                                }
                                                else
                                                {
                                                    var fileName = mediaFile.Split('/').Last();
                                                    WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                    var duration = WoWonderTools.GetDuration(mediaFile);
                                                    item.DurationsList.Add(Long.ParseLong(duration));
                                                }
                                            }

                                            checkSection.StoryList.Add(item);
                                        }
                                    }
                                    Activity?.RunOnUiThread(() => 
                                    {
                                        try
                                        {
                                            PostFeedAdapter.HolderStory.AboutMore.Visibility = checkSection.StoryList.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }).ConfigureAwait(false); 
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                    var d = new Runnable(() => { PostFeedAdapter.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkSection)); }); d.Run();
                }
            }
            else
            {
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        #endregion

        #region Permissions && Result

        //Result

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    //add post
                    case 2500 when resultCode == (int)Result.Ok:
                    {
                        if (!string.IsNullOrEmpty(data.GetStringExtra("itemObject")))
                        {
                            var postData = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("itemObject") ?? "");
                            if (postData != null)
                            {
                                var countList = PostFeedAdapter.ItemCount;

                                var combine = new FeedCombiner(postData, PostFeedAdapter.ListDiffer, Context);
                                combine.CombineDefaultPostSections("Top");

                                int countIndex = 1;
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
                    case 3950 when resultCode == (int)Result.Ok:
                    {
                        var postId = data.GetStringExtra("PostId") ?? "";
                        var postText = data.GetStringExtra("PostText") ?? "";
                        var diff = PostFeedAdapter.ListDiffer;
                        List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
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
                    case 3500 when resultCode == (int)Result.Ok:
                    {
                        if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                        var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData") ?? "");
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