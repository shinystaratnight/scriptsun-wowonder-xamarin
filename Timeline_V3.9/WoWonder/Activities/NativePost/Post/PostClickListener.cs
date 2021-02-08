﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Com.Google.Android.Exoplayer2;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Articles;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.EditPost;
using WoWonder.Activities.Events;
using WoWonder.Activities.Fundings;
using WoWonder.Activities.General;
using WoWonder.Activities.Jobs;
using WoWonder.Activities.Market;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Share;
using WoWonder.Activities.Offers;
using WoWonder.Activities.PostData;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UsersPages;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Timer = System.Timers.Timer;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Post
{
    public interface IOnPostItemClickListener
    {
        void ProfilePostClick(ProfileClickEventArgs e, string type, string typeEvent);
        void CommentPostClick(GlobalClickEventArgs e ,string type = "Normal");
        void SharePostClick(GlobalClickEventArgs e, PostModelType clickType);
        void CopyLinkEvent(string text);
        void MorePostIconClick(GlobalClickEventArgs item);
        void ImagePostClick(GlobalClickEventArgs item);
        void YoutubePostClick(GlobalClickEventArgs item);
        void LinkPostClick(GlobalClickEventArgs item, string type);
        void ProductPostClick(GlobalClickEventArgs item);
        void FileDownloadPostClick(GlobalClickEventArgs item);
        void OpenFilePostClick(GlobalClickEventArgs item);
        void OpenFundingPostClick(GlobalClickEventArgs item);
        void VoicePostClick(GlobalClickEventArgs item);
        void EventItemPostClick(GlobalClickEventArgs item);
        void ArticleItemPostClick(ArticleDataObject item);
        void DataItemPostClick(GlobalClickEventArgs item);
        void SecondReactionButtonClick(GlobalClickEventArgs item);
        void SingleImagePostClick(GlobalClickEventArgs item);
        void MapPostClick(GlobalClickEventArgs item);
        void OffersPostClick(GlobalClickEventArgs item);
        void JobPostClick(GlobalClickEventArgs item);
        void InitFullscreenDialog(Uri videoUrl, SimpleExoPlayer videoPlayer);
        void OpenAllViewer(string type, string passedId, AdapterModelsClass item);
    }

    public interface IOnLoadMoreListener
    {
        void OnLoadMore(int currentPage);
    }

    public class PostClickListener : Java.Lang.Object, IOnPostItemClickListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private readonly NativeFeedType NativeFeedType;

        private PostDataObject DataObject;
        private string TypeDialog;
        public static bool OpenMyProfile;
        public static readonly int MaxProgress = 10000;
        
        public PostClickListener(Activity context , NativeFeedType nativeFeedType)
        {
            MainContext = context;
            NativeFeedType = nativeFeedType;
            OpenMyProfile = false;
        }

        public void ProfilePostClick(ProfileClickEventArgs e, string type, string typeEvent)
        {
            try
            {
                var username = e.View.FindViewById<TextView>(Resource.Id.username);
                if (username != null && username.Text.Contains(MainContext.GetText(Resource.String.Lbl_SharedPost)) && typeEvent == "Username")
                {
                    var intent = new Intent(MainContext, typeof(ViewFullPostActivity));
                    intent.PutExtra("Id", e.NewsFeedClass.ParentId);
                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(e.NewsFeedClass));
                    MainContext.StartActivity(intent);
                }
                else if (username != null && username.Text == MainContext.GetText(Resource.String.Lbl_Anonymous))
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_SorryUserIsAnonymous), ToastLength.Short)?.Show();
                }
                else if (e.NewsFeedClass.PageId != null && e.NewsFeedClass.PageId != "0" && NativeFeedType != NativeFeedType.Page)
                {
                    var intent = new Intent(MainContext, typeof(PageProfileActivity));
                    intent.PutExtra("PageObject", JsonConvert.SerializeObject(e.NewsFeedClass.Publisher));
                    intent.PutExtra("PageId", e.NewsFeedClass.PageId);
                    MainContext.StartActivity(intent);
                }
                else if (e.NewsFeedClass.GroupId != null && e.NewsFeedClass.GroupId != "0" && NativeFeedType != NativeFeedType.Group)
                {
                    var intent = new Intent(MainContext, typeof(GroupProfileActivity));
                    intent.PutExtra("GroupObject", JsonConvert.SerializeObject(e.NewsFeedClass.GroupRecipient));
                    intent.PutExtra("GroupId", e.NewsFeedClass.GroupId);
                    MainContext.StartActivity(intent);
                }
                else
                {
                    if (type == "CommentClass")
                    {
                        WoWonderTools.OpenProfile(MainContext, e.CommentClass.UserId, e.CommentClass.Publisher);
                    }
                    else
                    {
                        WoWonderTools.OpenProfile(MainContext, e.NewsFeedClass.UserId, e.NewsFeedClass.Publisher);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public void CommentPostClick(GlobalClickEventArgs e , string type = "Normal")
        {
            try
            {
                var intent = new Intent(MainContext, typeof(CommentActivity));
                intent.PutExtra("PostId", e.NewsFeedClass.Id);
                intent.PutExtra("Type", type); 
                intent.PutExtra("PostObject", JsonConvert.SerializeObject(e.NewsFeedClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public void SharePostClick(GlobalClickEventArgs e, PostModelType clickType)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("ItemData", JsonConvert.SerializeObject(e.NewsFeedClass));
                bundle.PutString("TypePost", JsonConvert.SerializeObject(clickType));
                var activity = (AppCompatActivity)MainContext;
                var searchFilter = new ShareBottomDialogFragment
                {
                    Arguments = bundle
                };
                searchFilter.Show(activity.SupportFragmentManager, "ShareFilter");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        //Event Menu >> Copy Link
        public void CopyLinkEvent(string text)
        {
            try
            { 
                Methods.CopyToClipboard(MainContext, text); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Delete post
        private void DeletePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeletePost";
                    DataObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //ReportPost
        private void ReportPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.IsPostReported = true;
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short)?.Show();
                    //Sent Api >>
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.Id, "report") });
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //SavePost 
        private async void SavePostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.IsPostSaved = true;
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullySaved), ToastLength.Short)?.Show();
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "save").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            item.IsPostSaved = actionsObject.Code.ToString() != "0";
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //BoostPost 
        private async void BoostPostEvent(PostDataObject item)
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(MainContext, typeof(GoProActivity));
                    MainContext.StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    item.Boosted = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "boost");
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            MainContext?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    item.Boosted = actionsObject.Code.ToString();
                                    item.IsPostBoosted = actionsObject.Code.ToString();

                                    var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                    var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (dataGlobal?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                            dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                            adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass) , "BoostedPost");
                                        }

                                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                        if (checkTextSection == null && item.Boosted == "1")
                                        {
                                            var collection = dataGlobal.FirstOrDefault()?.PostData;
                                            var adapterModels = new AdapterModelsClass
                                            {
                                                TypeView = PostModelType.PromotePost,
                                                Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                PostData = collection,
                                                IsDefaultFeedPost = true
                                            };

                                            var headerPostIndex = adapterGlobal.ListDiffer.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                            if (headerPostIndex > -1)
                                            {
                                                adapterGlobal.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                adapterGlobal.NotifyItemInserted(headerPostIndex);
                                            }
                                        }
                                        else
                                        {
                                            WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                        }
                                    }

                                    var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                    var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.Boosted = actionsObject.Code.ToString();
                                            dataClass.PostData.IsPostBoosted = actionsObject.Code.ToString();
                                            adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass), "BoostedPost");
                                        }

                                        var checkTextSection = data.FirstOrDefault(w => w.TypeView == PostModelType.PromotePost);
                                        if (checkTextSection == null && item.Boosted == "1")
                                        {
                                            var collection = data.FirstOrDefault()?.PostData;
                                            var adapterModels = new AdapterModelsClass
                                            {
                                                TypeView = PostModelType.PromotePost,
                                                Id = Convert.ToInt32((int)PostModelType.PromotePost + collection?.Id),
                                                PostData = collection,
                                                IsDefaultFeedPost = true
                                            };

                                            var headerPostIndex = adapter.ListDiffer.IndexOf(data.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                                            if (headerPostIndex > -1)
                                            {
                                                adapter.ListDiffer.Insert(headerPostIndex, adapterModels);
                                                adapter.NotifyItemInserted(headerPostIndex);
                                            }
                                        }
                                        else
                                        {
                                            WRecyclerView.GetInstance().RemoveByRowIndex(checkTextSection);
                                        }
                                    }

                                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyBoosted), ToastLength.Short)?.Show();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e); 
                                }
                            });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Status Comments Post 
        private async void StatusCommentsPostEvent(PostDataObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.CommentsStatus = "1";
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Global.Post_Actions(item.Id, "disable_comments");
                    if (apiStatus == 200)
                    {
                        if (respond is PostActionsObject actionsObject)
                        {
                            MainContext?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    item.CommentsStatus = actionsObject.Code.ToString();

                                    var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                    var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (dataGlobal?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                            adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass));
                                        }
                                    }

                                    var adapter = TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter;
                                    var data = adapter?.ListDiffer?.Where(a => a.PostData?.Id == item.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var dataClass in from dataClass in data let index = adapter.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                                        {
                                            dataClass.PostData.CommentsStatus = actionsObject.Code.ToString();

                                            adapter.NotifyItemChanged(adapter.ListDiffer.IndexOf(dataClass));
                                        }
                                    }

                                    if (actionsObject.Code == 0)
                                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsDisabled), ToastLength.Short)?.Show();
                                    else
                                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_PostCommentsEnabled), ToastLength.Short)?.Show();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e); 
                                }
                            });
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void MorePostIconClick(GlobalClickEventArgs item)
        {
            try
            {
                DataObject = item.NewsFeedClass;

                var postType = PostFunctions.GetAdapterType(DataObject);

                TypeDialog = "MorePost";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                arrayAdapter.Add(!Convert.ToBoolean(item.NewsFeedClass.IsPostSaved) ? MainContext.GetString(Resource.String.Lbl_SavePost) : MainContext.GetString(Resource.String.Lbl_UnSavePost));

                if (!string.IsNullOrEmpty(item.NewsFeedClass.Orginaltext))
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeText));

                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeLink));

                if (!Convert.ToBoolean(item.NewsFeedClass.IsPostReported))
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_ReportPost));

                if ((item.NewsFeedClass.UserId != "0" || item.NewsFeedClass.PageId != "0" || item.NewsFeedClass.GroupId != "0") && item.NewsFeedClass.Publisher.UserId == UserDetails.UserId)
                {
                    switch (postType)
                    {
                        case PostModelType.ProductPost:
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditProduct));
                            break;
                        case PostModelType.OfferPost:
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditOffers));
                            break;
                        default:
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EditPost));
                            break;
                    }

                    if (AppSettings.ShowAdvertisingPost)
                    {
                        switch (item.NewsFeedClass?.Boosted)
                        {
                            case "0":
                                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_BoostPost));
                                break;
                            case "1":
                                arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_UnBoostPost));
                                break;
                        }
                    }
                     
                    switch (item.NewsFeedClass?.CommentsStatus)
                    {
                        case "0":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_EnableComments));
                            break;
                        case "1":
                            arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_DisableComments));
                            break;
                    }

                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_DeletePost));
                }

                dialogList.Title(MainContext.GetString(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void JobPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(JobsViewActivity));
                if (item.NewsFeedClass != null)
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job?.JobInfoClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void JobButtonPostClick(GlobalClickEventArgs item)
        {
            try
            {
                using var jobButton = item.View.FindViewById<Button>(Resource.Id.JobButton);
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext?.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                switch (jobButton?.Tag?.ToString())
                {
                    // Open Apply Job Activity 
                    case "ShowApply":
                    {
                        if (item.NewsFeedClass.Job != null && item.NewsFeedClass.Job.Value.JobInfoClass.ApplyCount == "0")
                        {
                            Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short)?.Show();
                            return;
                        }

                        var intent = new Intent(MainContext, typeof(ShowApplyJobActivity));
                        if (item.NewsFeedClass.Job != null)
                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                        MainContext.StartActivity(intent);
                        break;
                    }
                    case "Apply":
                    {
                        var intent = new Intent(MainContext, typeof(ApplyJobActivity));
                        if (item.NewsFeedClass.Job != null)
                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.NewsFeedClass.Job.Value.JobInfoClass));
                        MainContext.StartActivity(intent);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void ImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(MultiImagesPostViewerActivity));
                    intent.PutExtra("indexImage", item.Position.ToString()); // Index Image Show
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject
                    MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void SecondReactionButtonClick(GlobalClickEventArgs item)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (UserDetails.SoundControl)
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                var secondReactionText = item.View.FindViewById<TextView>(Resource.Id.SecondReactionText);

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder when item.NewsFeedClass.IsWondered != null && (bool)item.NewsFeedClass.IsWondered:
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        if (x > 0)
                            x--;
                        else
                            x = 0;

                        item.NewsFeedClass.IsWondered = false;
                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_wowonder);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Wonder);
                        secondReactionText.SetTextColor(Color.ParseColor("#666666"));
                        break;
                    }
                    case PostButtonSystem.Wonder:
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        x++;

                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                        item.NewsFeedClass.IsWondered = true;

                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_wowonder);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_wondered);
                        secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                        item.NewsFeedClass.Reaction ??= new Reaction();
                        if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                        {
                            item.NewsFeedClass.Reaction.IsReacted = false;
                        }

                        break;
                    }
                    case PostButtonSystem.DisLike when item.NewsFeedClass.IsWondered != null && item.NewsFeedClass.IsWondered.Value:
                    {
                        var unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_dislike);
                        var wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Btn_Dislike);
                        secondReactionText.SetTextColor(Color.ParseColor("#666666"));

                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        if (x > 0)
                            x--;
                        else
                            x = 0;

                        item.NewsFeedClass.IsWondered = false;
                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                        break;
                    }
                    case PostButtonSystem.DisLike:
                    {
                        var x = Convert.ToInt32(item.NewsFeedClass.PostWonders);
                        x++;

                        item.NewsFeedClass.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);
                        item.NewsFeedClass.IsWondered = true;

                        Drawable unwrappedDrawable = AppCompatResources.GetDrawable(MainContext, Resource.Drawable.ic_action_dislike);
                        Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                        if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                        {
                            DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                        }
                        else
                        {
                            wrappedDrawable = wrappedDrawable.Mutate();
                            wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                        }

                        secondReactionText.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                        secondReactionText.Text = MainContext.GetString(Resource.String.Lbl_disliked);
                        secondReactionText.SetTextColor(Color.ParseColor("#f89823"));

                        item.NewsFeedClass.Reaction ??= new Reaction();
                        if (item.NewsFeedClass.Reaction.IsReacted != null && item.NewsFeedClass.Reaction.IsReacted.Value)
                        {
                            item.NewsFeedClass.Reaction.IsReacted = false;
                        }

                        break;
                    }
                }

                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;

                var dataGlobal = adapterGlobal?.ListDiffer?.Where(a => a.PostData?.Id == item.NewsFeedClass.Id).ToList();
                if (dataGlobal?.Count > 0)
                {
                    foreach (var dataClass in from dataClass in dataGlobal let index = adapterGlobal.ListDiffer.IndexOf(dataClass) where index > -1 select dataClass)
                    {
                        dataClass.PostData = item.NewsFeedClass; 
                        adapterGlobal.NotifyItemChanged(adapterGlobal.ListDiffer.IndexOf(dataClass), "reaction");
                    }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.NewsFeedClass.Id, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(item.NewsFeedClass.Id, "dislike") });
                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void SingleImagePostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                    intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item.NewsFeedClass)); // PostDataObject
                    MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void MapPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    // Create a Uri from an intent string. Use the result to create an Intent?. 
                    var uri = Uri.Parse("geo:" + item.NewsFeedClass.CurrentLatitude + "," + item.NewsFeedClass.CurrentLongitude);
                    var intent = new Intent(Intent.ActionView, uri);
                    intent.SetPackage("com.google.android.apps.maps");
                    intent.AddFlags(ActivityFlags.NewTask);
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OffersPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (item.NewsFeedClass != null)
                {
                    var intent = new Intent(MainContext, typeof(OffersViewActivity));
                    intent.PutExtra("OffersObject", JsonConvert.SerializeObject(item.NewsFeedClass.Offer?.OfferClass));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenAllViewer(string type, string passedId, AdapterModelsClass item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(AllViewerActivity));
                intent.PutExtra("Type", type); //StoryModel , FollowersModel , GroupsModel , PagesModel , ImagesModel
                intent.PutExtra("PassedId", passedId);

                switch (type)
                {
                    case "StoryModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item));
                        break;
                    case "FollowersModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.FollowersModel));
                        break;
                    case "GroupsModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.GroupsModel));
                        break;
                    case "PagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PagesModel));
                        break;
                    case "ImagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.ImagesModel));
                        break;
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void YoutubePostClick(GlobalClickEventArgs item)
        {
            MainApplication.GetInstance()?.NavigateTo(MainContext, typeof(YoutubePlayerActivity), item.NewsFeedClass);
        }

        public void LinkPostClick(GlobalClickEventArgs item, string type)
        {
            try
            {
                if (type == "LinkPost")
                {
                    if (item.NewsFeedClass.PostLink.Contains(Client.WebsiteUrl) && item.NewsFeedClass.PostLink.Contains("movies/watch/"))
                    {
                        var videoId = item.NewsFeedClass.PostLink.Split("movies/watch/").Last().Replace("/", "");
                        var intent = new Intent(MainContext, typeof(VideoViewerActivity));
                        //intent.PutExtra("Viewer_Video", JsonConvert.SerializeObject(item));
                        intent.PutExtra("VideoId", videoId);
                        MainContext.StartActivity(intent);
                    }
                    else
                    {
                        if (!item.NewsFeedClass.PostLink.Contains("http"))
                            item.NewsFeedClass.PostLink = "http://" + item.NewsFeedClass.PostLink;

                        if (item.NewsFeedClass.PostLink.Contains("tiktok"))
                            new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostTikTok);
                        else
                            new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.PostLink);
                    }
                }
                else
                {
                    if (!item.NewsFeedClass.Url.Contains("http"))
                        item.NewsFeedClass.Url = "http://" + item.NewsFeedClass.Url;

                    new IntentController(MainContext).OpenBrowserFromApp(item.NewsFeedClass.Url); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void ProductPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ProductViewActivity));
                intent.PutExtra("Id", item.NewsFeedClass?.PostId);
                if (item?.NewsFeedClass?.Product != null)
                {
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.NewsFeedClass?.Product.Value.ProductClass));
                }
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenFundingPostClick(GlobalClickEventArgs item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(FundingViewActivity));
                var postType = PostFunctions.GetAdapterType(item.NewsFeedClass);
                switch (postType)
                {
                    case PostModelType.FundingPost:
                    {
                        if (item.NewsFeedClass?.FundData != null)
                        {
                            if (item.NewsFeedClass?.FundData.Value.FundDataClass.UserData == null)
                                item.NewsFeedClass.FundData.Value.FundDataClass.UserData = item.NewsFeedClass.Publisher;

                            intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.FundData.Value.FundDataClass));
                        }

                        break;
                    }
                    case PostModelType.PurpleFundPost:
                    {
                        if (item.NewsFeedClass?.Fund != null)
                        {
                            if (item.NewsFeedClass?.Fund.Value.PurpleFund.Fund.UserData == null)
                                item.NewsFeedClass.Fund.Value.PurpleFund.Fund.UserData = item.NewsFeedClass.Publisher;

                            intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item.NewsFeedClass?.Fund.Value.PurpleFund.Fund));
                        }

                        break;
                    }
                }

                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenFilePostClick(GlobalClickEventArgs item)
        {
            try
            {
                var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                if (getFile != "File Dont Exists")
                {
                    File file2 = new File(getFile);
                    var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                    Intent openFile = new Intent(Intent.ActionView, photoUri);
                    openFile.SetFlags(ActivityFlags.NewTask);
                    openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                    MainContext.StartActivity(openFile);
                }
                else
                {
                    Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.NewsFeedClass.PostFileFull));
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_FileNotDeviceMemory), ToastLength.Long)?.Show();
            }
        }

        //Download
        public void FileDownloadPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.NewsFeedClass.PostFileFull))
                {
                    Methods.Path.Chack_MyFolder();

                    var fileSplit = item.NewsFeedClass.PostFileFull.Split('/').Last();
                    string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                    if (getFile != "File Dont Exists")
                    {
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_FileExists), ToastLength.Long)?.Show();
                    }
                    else
                    {
                        Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDcimFile, item.NewsFeedClass.PostFileFull);
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourFileIsDownloaded), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        public void EventItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(EventViewActivity));
                if (item.NewsFeedClass.Event != null)
                    intent.PutExtra("EventView", JsonConvert.SerializeObject(item.NewsFeedClass.Event.Value.EventClass));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void ArticleItemPostClick(ArticleDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ArticlesViewActivity));
                intent.PutExtra("Id", item.Id);
                intent.PutExtra("ArticleObject", JsonConvert.SerializeObject(item));
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void DataItemPostClick(GlobalClickEventArgs item)
        {
            try
            {
                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    if (item.NewsFeedClass.Reaction.Count > 0)
                    {
                        var intent = new Intent(MainContext, typeof(ReactionPostTabbedActivity));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(item.NewsFeedClass));
                        MainContext.StartActivity(intent);
                    }
                }
                else
                {
                    var intent = new Intent(MainContext, typeof(PostDataActivity));
                    intent.PutExtra("PostId", item.NewsFeedClass.Id);
                    intent.PutExtra("PostType", "post_likes");
                    MainContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        private GlobalClickEventArgs ItemVoicePost; 
        public void VoicePostClick(GlobalClickEventArgs item)
        {
            try
            {
                var instance = WRecyclerView.GetInstance();
                if (instance != null)
                {
                    instance?.StopVideo();
                    instance.ViewHolderVoicePlayer = item.HolderSound.ItemView;
                    instance.IsVoicePlayed = true;
                }
               
                ItemVoicePost = item;

                if (item.HolderSound.PostAdapter.PositionSound != item.Position)
                { 
                    var list = item.HolderSound.PostAdapter.ListDiffer.Where(a => a.TypeView == PostModelType.VoicePost && a.VoicePlayer != null).ToList();
                    if (list.Count > 0)
                    {
                        foreach (var modelsClass in list)
                        {
                            modelsClass.MediaIsPlaying = false;

                            if (modelsClass.VoicePlayer != null)
                            {
                                modelsClass.VoicePlayer.Stop();
                                modelsClass.VoicePlayer.Reset();
                            }
                            modelsClass.VoicePlayer = null;
                            modelsClass.Timer = null;

                            modelsClass.VoicePlayer?.Release();
                            modelsClass.VoicePlayer = null;
                        }

                        item.HolderSound.PostAdapter.NotifyItemChanged(item.HolderSound.PostAdapter.PositionSound, "WithoutBlobAudio");
                    }
                }
                 
                if (item.AdapterModelsClass.VoicePlayer == null) 
                {
                    item.HolderSound.PostAdapter.PositionSound = item.Position;

                    //item.HolderSound.SeekBar.Max = 10000;
                    item.AdapterModelsClass.VoicePlayer = new MediaPlayer();
                    item.AdapterModelsClass.VoicePlayer.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Music)?.Build());
                    item.AdapterModelsClass.VoicePlayer.Completion += (sender, args) =>
                    {
                        try
                        {
                            item.HolderSound.LoadingProgressView.Visibility = ViewStates.Gone;
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;

                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_play);
                            item.HolderSound.PlayButton.Tag = "Play";
                            item.AdapterModelsClass.VoicePlayer.Stop();

                            item.AdapterModelsClass.MediaIsPlaying = false;

                            item.AdapterModelsClass.VoicePlayer.Stop();
                            item.AdapterModelsClass.VoicePlayer.Reset();
                            item.AdapterModelsClass.VoicePlayer = null;
                             
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                                item.HolderSound.SeekBar.SetProgress(0, true);
                            else // For API < 24 
                                item.HolderSound.SeekBar.Progress = 0;

                            if (item.AdapterModelsClass.Timer == null) return;
                            item.AdapterModelsClass.Timer.Enabled = false;
                            item.AdapterModelsClass.Timer.Stop();
                            item.AdapterModelsClass.Timer = null;
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.AdapterModelsClass.VoicePlayer.Prepared += (o, eventArgs) =>
                    {
                        try
                        {
                            item.AdapterModelsClass.MediaIsPlaying = true;
                            
                            item.AdapterModelsClass.VoicePlayer.Start();
                            item.HolderSound.PlayButton.Tag = "Pause";
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_pause);
                            item.HolderSound.LoadingProgressView.Visibility = ViewStates.Gone;
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;

                            if (item.AdapterModelsClass.Timer == null)
                            {
                                item.AdapterModelsClass.Timer = new Timer { Interval = 1000, Enabled = true };
                                item.AdapterModelsClass.Timer.Elapsed += TimerOnElapsed;
                                item.AdapterModelsClass.Timer.Start();
                            }
                            else
                            {
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.HolderSound.PlayButton.Visibility = ViewStates.Gone;
                    item.HolderSound.LoadingProgressView.Visibility = ViewStates.Visible;

                    var url = !string.IsNullOrEmpty(item.NewsFeedClass.PostFileFull) ? item.NewsFeedClass.PostFileFull : item.NewsFeedClass.PostRecord;

                    if (!string.IsNullOrEmpty(url) && (url.Contains("file://") || url.Contains("content://") || url.Contains("storage") || url.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.NewsFeedClass.PostFileFull);
                        var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                        item.AdapterModelsClass.VoicePlayer.SetDataSource(MainContext, photoUri);
                        item.AdapterModelsClass.VoicePlayer.Prepare();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.NewsFeedClass.PostRecord) && !item.NewsFeedClass.PostRecord.Contains(Client.WebsiteUrl))
                            url = WoWonderTools.GetTheFinalLink(url);

                        item.AdapterModelsClass.VoicePlayer.SetDataSource(MainContext, Uri.Parse(url));
                        item.AdapterModelsClass.VoicePlayer.PrepareAsync();

                    }

                    item.HolderSound.SeekBar.Max = 10000;

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        item.HolderSound.SeekBar.SetProgress(0, true);
                    else  // For API < 24 
                        item.HolderSound.SeekBar.Progress = 0;

                    item.HolderSound.SeekBar.StartTrackingTouch += (sender, args) =>
                    {
                        try
                        {
                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                item.AdapterModelsClass.Timer.Enabled = false;
                                item.AdapterModelsClass.Timer.Stop();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };

                    item.HolderSound.SeekBar.StopTrackingTouch += (sender, args) =>
                    {
                        try
                        {
                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                item.AdapterModelsClass.Timer.Enabled = false;
                                item.AdapterModelsClass.Timer.Stop();
                            }

                            int seek = args.SeekBar.Progress;

                            int totalDuration = item.AdapterModelsClass.VoicePlayer.Duration;
                            var currentPosition = ProgressToTimer(seek, totalDuration);

                            // forward or backward to certain seconds
                            item.AdapterModelsClass.VoicePlayer.SeekTo((int)currentPosition);

                            if (item.AdapterModelsClass.Timer != null)
                            {   
                                // update timer progress again
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e); 
                        }
                    };
                }
                else
                {
                    switch (item.HolderSound.PlayButton?.Tag?.ToString())
                    {
                        case "Play":
                        {
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_pause);
                            item.HolderSound.PlayButton.Tag = "Pause";
                            item.AdapterModelsClass.VoicePlayer?.Start();
                         
                            item.AdapterModelsClass.MediaIsPlaying = true;
                         
                            if (item.AdapterModelsClass.Timer != null)
                            {
                                item.AdapterModelsClass.Timer.Enabled = true;
                                item.AdapterModelsClass.Timer.Start();
                            }

                            break;
                        }
                        case "Pause":
                        {
                            item.HolderSound.PlayButton.Visibility = ViewStates.Visible;
                            item.HolderSound.PlayButton.SetImageResource(Resource.Drawable.icon_player_play);
                            item.HolderSound.PlayButton.Tag = "Play";
                            item.AdapterModelsClass.VoicePlayer?.Pause();

                            item.AdapterModelsClass.MediaIsPlaying = false; 

                            if (item.AdapterModelsClass.Timer == null) return;
                            item.AdapterModelsClass.Timer.Enabled = false;
                            item.AdapterModelsClass.Timer.Stop();
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

        private long ProgressToTimer(int progress, int totalDuration)
        {
            try
            {
                totalDuration /= 1000;
                var currentDuration = (int)((double)progress / MaxProgress * totalDuration);

                // return current duration in milliseconds
                return currentDuration * 1000;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                return 0;
            }
        }

        private int GetProgressSeekBar(int currentDuration, int totalDuration)
        {
            try
            {
                // calculating percentage
                double progress = (double)currentDuration / totalDuration * MaxProgress;
                if (progress >= 0)
                {
                    // return percentage
                    return Convert.ToInt32(progress);
                }
                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
                return 0;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            MainContext?.RunOnUiThread(() =>
            {
                try
                {
                    if (ItemVoicePost.AdapterModelsClass.VoicePlayer != null)
                    {
                        int totalDuration = ItemVoicePost.AdapterModelsClass.VoicePlayer.Duration;
                        int currentDuration = ItemVoicePost.AdapterModelsClass.VoicePlayer.CurrentPosition;

                        // Updating progress bar
                        int progress = GetProgressSeekBar(currentDuration, totalDuration);

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        {
                            ItemVoicePost.HolderSound.SeekBar.SetProgress(progress, true);
                        }
                        else
                        {
                            // For API < 24 
                            ItemVoicePost.HolderSound.SeekBar.Progress = progress;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception); 
                }
            });
        }

        //Event Menu >> Edit Info Post if user == is_owner  
        private void EditInfoPost_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditPostActivity));
                intent.PutExtra("PostId", item.Id);
                intent.PutExtra("PostItem", JsonConvert.SerializeObject(item));
                MainContext.StartActivityForResult(intent, 3950);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Edit Info Product if user == is_owner  
        private void EditInfoProduct_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditProductActivity));
                if (item.Product != null)
                    intent.PutExtra("ProductView", JsonConvert.SerializeObject(item.Product.Value.ProductClass));
                MainContext.StartActivityForResult(intent, 3500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        //Event Menu >> Edit Info Offers if user == is_owner  
        private void EditInfoOffers_OnClick(PostDataObject item)
        {
            try
            {
                Intent intent = new Intent(MainContext, typeof(EditOffersActivity));
                intent.PutExtra("OfferId", item.OfferId);
                if (item.Offer != null)
                    intent.PutExtra("OfferItem", JsonConvert.SerializeObject(item.Offer.Value.OfferClass));
                MainContext.StartActivityForResult(intent, 4000); //wael
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void OpenImageLightBox(PostDataObject item)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ImagePostViewerActivity));
                intent.PutExtra("itemIndex", "00"); //PhotoAlbumObject
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item)); // PostDataObject
                MainContext.OverridePendingTransition(Resource.Animation.abc_popup_enter, Resource.Animation.popup_exit);
                MainContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }

        public void InitFullscreenDialog(Uri videoUrl, SimpleExoPlayer videoPlayer)
        {
            try
            {
                // videoPlayer?.PlayWhenReady = false;

                Intent intent = new Intent(MainContext, typeof(VideoFullScreenActivity));
                intent.PutExtra("videoUrl", videoUrl.ToString());
                //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent(DataObject.Url);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_CopeText))
                {
                    CopyLinkEvent(Methods.FunString.DecodeString(DataObject.Orginaltext));
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditPost))
                {
                    EditInfoPost_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditProduct))
                {
                    EditInfoProduct_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EditOffers))
                {
                    EditInfoOffers_OnClick(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_ReportPost))
                {
                    ReportPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_DeletePost))
                {
                    DeletePostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_BoostPost) || text == MainContext.GetString(Resource.String.Lbl_UnBoostPost))
                {
                    BoostPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_EnableComments) || text == MainContext.GetString(Resource.String.Lbl_DisableComments))
                {
                    StatusCommentsPostEvent(DataObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_SavePost) || text == MainContext.GetString(Resource.String.Lbl_UnSavePost))
                {
                    SavePostEvent(DataObject);
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
                    if (TypeDialog == "DeletePost")
                    {
                        MainContext?.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (!Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                                    return;
                                }
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(DataObject.Id, "delete") });
                                 
                                var feedTab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = feedTab?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            feedTab.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                feedTab?.MainRecyclerView?.StopVideo();

                                var profileActivity = MyProfileActivity.GetInstance();
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    { 
                                        foreach (var post in data)
                                        {
                                            profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = profileActivity?.PostFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            profileActivity.MainRecyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                var recyclerView = WRecyclerView.GetInstance();
                                if (DataObject.SharedInfo.SharedInfoClass != null)
                                {
                                    var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id || a.PostData?.Id == DataObject?.SharedInfo.SharedInfoClass?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            recyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }
                                else
                                {
                                    var data = recyclerView?.NativeFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == DataObject?.Id).ToList();
                                    if (data?.Count > 0)
                                    {
                                        foreach (var post in data)
                                        {
                                            recyclerView?.RemoveByRowIndex(post);
                                        }
                                    }
                                }

                                recyclerView?.StopVideo();

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show(); 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e); 
                            }
                        });
                    }
                    else
                    {
                        if (p1 == DialogAction.Positive)
                        {
                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }
                    }
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

        #endregion MaterialDialog

    }
}