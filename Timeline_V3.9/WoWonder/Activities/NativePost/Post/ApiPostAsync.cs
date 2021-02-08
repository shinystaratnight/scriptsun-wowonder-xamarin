using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.NativePost.Post
{
    public class ApiPostAsync
    {
        private readonly Activity ActivityContext;
        private readonly NativePostAdapter NativeFeedAdapter;
        private readonly WRecyclerView WRecyclerView;
        private static bool ShowFindMoreAlert;
        private static PostModelType LastAdsType = PostModelType.AdMob3;
        public static List<PostDataObject> PostCacheList { private set; get; } 
         
        public ApiPostAsync(WRecyclerView recyclerView, NativePostAdapter adapter)
        {
            try
            {
                ActivityContext = adapter.ActivityContext;
                NativeFeedAdapter = adapter;
                WRecyclerView = recyclerView;
                PostCacheList = new List<PostDataObject>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Api

        public async Task FetchNewsFeedApiPosts(string offset = "0", string typeRun = "Add", string hash = "")
        {
            if (WRecyclerView.MainScrollEvent.IsLoading)
                return;

            if (!Methods.CheckConnectivity())
                return;

            WRecyclerView.Hash = hash;
            int apiStatus;
            dynamic respond;

            WRecyclerView.MainScrollEvent.IsLoading = true; 

            switch (NativeFeedAdapter.NativePostType)
            {
                case NativeFeedType.Global:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter, "", WRecyclerView.Filter);
                    break;
                case NativeFeedType.User:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Group:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Page:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Event:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Saved:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved");
                    break;
                case NativeFeedType.HashTag:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash);
                    break;
                case NativeFeedType.Popular:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                default:
                    return;
            }

            if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false; 
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else
                LoadDataApi(apiStatus, respond, offset, typeRun);
        }

        public async Task FetchSearchForPosts(string offset, string id, string searchQuery, string type)
        {
            if (!Methods.CheckConnectivity())
                return;

            var (apiStatus, respond) = await RequestsAsync.Posts.SearchForPosts(AppSettings.PostApiLimitOnScroll, offset, id, searchQuery, type);
            if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
            {
                WRecyclerView.MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult(ActivityContext, respond);
            }
            else LoadDataApi(apiStatus, respond, offset);
        }

        public void LoadDataApi(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            try
            {
                if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                { 
                    if (WRecyclerView.SwipeRefreshLayoutView != null && WRecyclerView.SwipeRefreshLayoutView.Refreshing)
                        WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Count > 0)
                    {
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);

                        if (offset == "0" && countList > 10 && typeRun == "Insert" && NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            result.Data.Reverse();
                            bool add = false;

                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                NativeFeedAdapter.NewPostList.Add(post);
                            }

                            ActivityContext?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    if (add && WRecyclerView.PopupBubbleView != null && WRecyclerView.PopupBubbleView.Visibility != ViewStates.Visible && AppSettings.ShowNewPostOnNewsFeed)
                                    {
                                        WRecyclerView.PopupBubbleView.Visibility = ViewStates.Visible;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else
                        {
                            bool add = false;
                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                add = true;
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);

                                if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                                {
                                    if (result.Data.Count < 6 && NativeFeedAdapter.ListDiffer.Count < 6)
                                    {
                                        if (!ShowFindMoreAlert)
                                        {
                                            ShowFindMoreAlert = true;

                                            combiner.AddFindMoreAlertPostView("Pages");
                                            combiner.AddFindMoreAlertPostView("Groups");
                                        }
                                    }

                                    var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                    if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                    {
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);
                                    }

                                    var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                    if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                    {
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);
                                    }
                                }

                                if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdMobNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                                {
                                    switch (LastAdsType)
                                    {
                                        case PostModelType.AdMob1:
                                            LastAdsType = PostModelType.AdMob2;
                                            combiner.AddAdsPostView(PostModelType.AdMob1);
                                            break;
                                        case PostModelType.AdMob2:
                                            LastAdsType = PostModelType.AdMob3;
                                            combiner.AddAdsPostView(PostModelType.AdMob2);
                                            break;
                                        case PostModelType.AdMob3:
                                            LastAdsType = PostModelType.AdMob1;
                                            combiner.AddAdsPostView(PostModelType.AdMob3);
                                            break;
                                    }
                                }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                                if (post.PostType == "ad")
                                {
                                    combine.AddAdsPost();
                                }
                                else
                                {
                                    bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                    if (isPromoted) //Promoted
                                    {
                                        combine.CombineDefaultPostSections("Top");
                                    }
                                    else
                                    {
                                        combine.CombineDefaultPostSections();
                                    }
                                }

                                if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowFbNativeAdsCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                                {
                                    combiner.AddAdsPostView(PostModelType.FbAdNative);
                                }
                            }

                            if (add)
                            {
                                ActivityContext?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                        GC.Collect();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }

                            //else
                            //{
                            //    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short)?.Show(); 
                            //}
                        }
                    }

                    ActivityContext?.RunOnUiThread(ShowEmptyPage);

                    if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        WRecyclerView.DataPostJson = JsonConvert.SerializeObject(result); 
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }

        public void LoadTopDataApi(List<PostDataObject> list)
        {
            try
            {
                NativeFeedAdapter.ListDiffer.Clear();
                NativeFeedAdapter.NotifyDataSetChanged();

                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);
                combiner.AddStoryPostView();
                combiner.AddPostBoxPostView("feed", -1);
                 
                if (list.Count > 0)
                {
                    bool add = false;
                    foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                    {
                        add = true;
                        var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                        if (post.PostType == "ad")
                        {
                            combine.AddAdsPost();
                        }
                        else
                        {
                            combine.CombineDefaultPostSections();
                        }
                    }
                     
                    if (PostCacheList?.Count > 0)
                    {
                        LoadBottomDataApi(PostCacheList.Take(30).ToList());
                    }
                     
                    if (add)
                    {
                        ActivityContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                NativeFeedAdapter.NotifyDataSetChanged();
                                NativeFeedAdapter.NewPostList.Clear();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadMemoriesDataApi(int apiStatus, dynamic respond, List<AdapterModelsClass> diffList)
        {
            try
            {
                if (WRecyclerView.MainScrollEvent.IsLoading)
                    return;

                WRecyclerView.MainScrollEvent.IsLoading = true;

                if (apiStatus != 200 || !(respond is FetchMemoriesObject result) || result.Data == null)
                {
                    WRecyclerView.MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    if (WRecyclerView.SwipeRefreshLayoutView != null && WRecyclerView.SwipeRefreshLayoutView.Refreshing)
                        WRecyclerView.SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Posts.Count > 0)
                    {
                        result.Data.Posts.RemoveAll(a => a.Publisher == null && a.UserData == null);
                        result.Data.Posts.Reverse();

                        foreach (var post in from post in result.Data.Posts let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                        {
                            if (post.Publisher == null && post.UserData == null)
                                continue;

                            var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                            combine.CombineDefaultPostSections();
                        }

                        ActivityContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run(); 
                                GC.Collect();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    } 
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
        }
         
        public async Task FetchLoadMoreNewsFeedApiPosts()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                if (NativeFeedAdapter.NativePostType != NativeFeedType.Global)
                    return;
                 
                if (PostCacheList?.Count > 40)
                    return;
                  
                var diff = NativeFeedAdapter.ListDiffer;
                var list = new List<AdapterModelsClass>(diff);
                if (list.Count <= 20)
                    return;

                var item = list.LastOrDefault();
                 
                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                if (item.TypeView == PostModelType.Divider || item.TypeView == PostModelType.ViewProgress || item.TypeView == PostModelType.AdMob1 || item.TypeView == PostModelType.AdMob2 || item.TypeView == PostModelType.AdMob3 || item.TypeView == PostModelType.FbAdNative || item.TypeView == PostModelType.AdsPost || item.TypeView == PostModelType.SuggestedGroupsBox || item.TypeView == PostModelType.SuggestedUsersBox || item.TypeView == PostModelType.CommentSection || item.TypeView == PostModelType.AddCommentSection)
                {
                    item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                    offset = item?.PostData?.PostId ?? "0";
                    Console.WriteLine(offset);
                }
                else
                {
                    offset = item.PostData?.PostId ?? "0";
                }

                Console.WriteLine(offset);
              
                int apiStatus;
                dynamic respond;

                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Global:
                        (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter, "", WRecyclerView.Filter);
                        break; 
                    default:
                        return;
                }

                if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
                { 
                    Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    PostCacheList ??= new List<PostDataObject>();

                    var countList = PostCacheList?.Count ?? 0;
                    if (result?.Data?.Count > 0)
                    {
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                         
                        if (countList > 0)
                        {
                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                            {
                                PostCacheList.Add(post);
                            }
                        }
                        else
                        {
                            PostCacheList = new List<PostDataObject>(result.Data);
                        } 
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        public bool LoadBottomDataApi(List<PostDataObject> list)
        {
            try
            {
                var countList = NativeFeedAdapter.ItemCount;
                if (list?.Count > 0)
                {
                    bool add = false;
                    foreach (var post in from post in list let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.PostId == post.PostId && a?.TypeView == PostFunctions.GetAdapterType(post)) where check == null select post)
                    {
                        add = true;
                        var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, ActivityContext);

                        if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                        {
                            var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                            if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                            {
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);
                            }

                            var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                            if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                            {
                                combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);
                            }
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdMobNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                        {
                            switch (LastAdsType)
                            {
                                case PostModelType.AdMob1:
                                    LastAdsType = PostModelType.AdMob2;
                                    combiner.AddAdsPostView(PostModelType.AdMob1);
                                    break;
                                case PostModelType.AdMob2:
                                    LastAdsType = PostModelType.AdMob3;
                                    combiner.AddAdsPostView(PostModelType.AdMob2);
                                    break;
                                case PostModelType.AdMob3:
                                    LastAdsType = PostModelType.AdMob1;
                                    combiner.AddAdsPostView(PostModelType.AdMob3);
                                    break;
                            }
                        }

                        var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, ActivityContext);
                        if (post.PostType == "ad")
                        {
                            combine.AddAdsPost();
                        }
                        else
                        {
                            bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                            if (isPromoted) //Promoted
                            {
                                combine.CombineDefaultPostSections("Top");
                            }
                            else
                            {
                                combine.CombineDefaultPostSections();
                            }
                        }

                        if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowFbNativeAdsCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                        {
                            combiner.AddAdsPostView(PostModelType.FbAdNative);
                        }
                    }

                    if (add)
                    {
                        ActivityContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var d = new Runnable(() => { NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList); }); d.Run();
                                GC.Collect(); 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }

                    PostCacheList.RemoveRange(0, list.Count - 1); 
                    ActivityContext?.RunOnUiThread(ShowEmptyPage);

                    return add;
                }

                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                NativeFeedAdapter.SetLoaded();
                var viewProgress = NativeFeedAdapter.ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                if (viewProgress != null)
                    WRecyclerView.RemoveByRowIndex(viewProgress);

                var emptyStateCheck = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox && a.TypeView != PostModelType.FilterSection && a.TypeView != PostModelType.SearchForPosts);
                if (emptyStateCheck != null)
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker != null && NativeFeedAdapter.ListDiffer.Count > 1)
                        WRecyclerView.RemoveByRowIndex(emptyStateChecker);
                }
                else
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker == null)
                    {
                        var data = new AdapterModelsClass
                        {
                            TypeView = PostModelType.EmptyState,
                            Id = 744747447,
                        };
                        NativeFeedAdapter.ListDiffer.Add(data);
                        NativeFeedAdapter.NotifyItemInserted(NativeFeedAdapter.ListDiffer.IndexOf(data));
                    }
                }

                WRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void InsertTheLatestPosts()
        {
            try
            {
                if (!string.IsNullOrEmpty(WRecyclerView.DataPostJson))
                {
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdatePost(WRecyclerView.DataPostJson);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static PostDataObject RegexFilterText(PostDataObject item)
        {
            try
            {
                Dictionary<string, string> dataUser = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(item.PostText))
                    return item;

                if (item.PostText.Contains("data-id="))
                {
                    try
                    {
                        //string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""']|'>(.*?)a>)";

                        string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""'])";
                        var aa = Regex.Matches(item.PostText, pattern);
                        if (aa?.Count > 0)
                        {
                            for (int i = 0; i < aa.Count; i++)
                            { 
                                string userid = ""; 
                                if (aa.Count > i)
                                    userid = aa[i]?.Value?.Replace("data-id=", "").Replace('"', ' ').Replace(" ", "");
                                
                                string username = ""; 
                                if (aa.Count > i + 1)
                                    username = aa[i + 1]?.Value?.Replace("href=", "").Replace('"', ' ').Replace(" ", "").Replace(Client.WebsiteUrl, "").Replace("\n", "");

                                if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(username))
                                    continue;

                                var data = dataUser.FirstOrDefault(a => a.Key?.ToString() == userid && a.Value?.ToString() == username);
                                if (data.Key != null) 
                                    continue;

                                i++;

                                if (!string.IsNullOrWhiteSpace(userid) && !string.IsNullOrWhiteSpace(username) && !dataUser.ContainsKey(userid))
                                    dataUser.Add(userid, username);
                            }

                            item.RegexFilterList = new Dictionary<string, string>(dataUser);
                            return item;
                        }
                        else
                        {
                            return item;
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }

                return item;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return item;
            }
        }
         
        public static async Task FetchFirstNewsFeedApiPosts()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;
                 
                var (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnBackground); 
                if (apiStatus != 200 || !(respond is PostObject result))
                {
                    //Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                { 
                    if (result?.Data?.Count > 0)
                    {
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        //Insert All data to database
                        dbDatabase.InsertOrUpdatePost(JsonConvert.SerializeObject(result)); 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}