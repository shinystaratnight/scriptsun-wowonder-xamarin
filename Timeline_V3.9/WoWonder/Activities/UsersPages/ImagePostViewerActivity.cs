﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using Sephiroth.ImageZoom;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.PostData;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests; 
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.UsersPages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ImagePostViewerActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageViewTouch PageImage;
        private ImageView ImgLike, ImgWoWonder, ImgWonder; 
        private SuperTextView TxtDescription; 
        private TextView TxtCountLike, TxtCountWoWonder, TxtWonder, ShareText; 
        private LinearLayout MainSectionButton,BtnCountLike, BtnCountWoWonder, BtnLike, BtnComment, BtnShare, BtnWonder, InfoImageLiner;
        private RelativeLayout MainLayout;
        private PostDataObject PostData;
        private ReactButton LikeButton;
        private string ImageUrl;
        private PostClickListener ClickListener;
        
       
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
                SetContentView(Resource.Layout.ImagePostViewer_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                Get_DataImage();

                ClickListener = new PostClickListener(this , NativeFeedType.Global);

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ImagePost, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.download:
                    Download_OnClick();
                    break;

                case Resource.Id.ic_action_comment:
                    Copy_OnClick();
                    break;

                case Resource.Id.action_More:
                    More_OnClick();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Event Download Image  
        private void Download_OnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    Methods.MultiMedia.DownloadMediaTo_GalleryAsync(Methods.Path.FolderDcimImage, ImageUrl);
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ImageSaved), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Copy link image 
        private void Copy_OnClick()
        {
            try
            {
                Methods.CopyToClipboard(this, PostData.Url); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event More 
        private void More_OnClick()
        {
            try
            {
                ClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = PostData });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                PageImage = FindViewById<ImageViewTouch>(Resource.Id.imageview); 
                TxtDescription = FindViewById<SuperTextView>(Resource.Id.tv_description);
                TxtDescription?.SetTextInfo(TxtDescription); 
                
                ImgLike = FindViewById<ImageView>(Resource.Id.image_like1);
                ImgWoWonder = FindViewById<ImageView>(Resource.Id.image_wowonder);
                 
                TxtCountLike = FindViewById<TextView>(Resource.Id.LikeText1);
                TxtCountWoWonder = FindViewById<TextView>(Resource.Id.WoWonderTextCount);

                MainLayout = FindViewById<RelativeLayout>(Resource.Id.main);
                InfoImageLiner = FindViewById<LinearLayout>(Resource.Id.infoImageLiner);
                InfoImageLiner.Visibility = ViewStates.Visible;
                 
                BtnCountLike = FindViewById<LinearLayout>(Resource.Id.linerlikeCount);
                BtnCountWoWonder = FindViewById<LinearLayout>(Resource.Id.linerwowonderCount);

                BtnLike = FindViewById<LinearLayout>(Resource.Id.linerlike);
                BtnComment = FindViewById<LinearLayout>(Resource.Id.linercomment);
                BtnShare = FindViewById<LinearLayout>(Resource.Id.linershare);

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.mainsection);
                BtnWonder = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                ImgWonder = FindViewById<ImageView>(Resource.Id.image_SecondReaction);
                TxtWonder = FindViewById<TextView>(Resource.Id.SecondReactionText);

                // set the default image display type
               // PageImage.SetDisplayType(ImageViewTouchBase.DisplayType.FitIfBigger);

                LikeButton = FindViewById<ReactButton>(Resource.Id.ReactButton);
                LikeButton.SetTextColor(Color.White);

                ShareText = FindViewById<TextView>(Resource.Id.ShareText); 
                if (!AppSettings.ShowTextShareButton && ShareText != null)
                    ShareText.Visibility = ViewStates.Gone;

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                    case PostButtonSystem.Like:
                        MainSectionButton.WeightSum = 3;
                        BtnWonder.Visibility = ViewStates.Gone;

                        TxtCountWoWonder.Visibility = ViewStates.Gone;
                        BtnCountWoWonder.Visibility = ViewStates.Gone;
                        ImgWoWonder.Visibility = ViewStates.Gone;
                        break;
                    case PostButtonSystem.Wonder:
                        MainSectionButton.WeightSum = 4;
                        BtnWonder.Visibility = ViewStates.Visible;

                        TxtCountWoWonder.Visibility = ViewStates.Visible;
                        BtnCountWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.Visibility = ViewStates.Visible;

                        ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                        ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                        TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                        break;
                    case PostButtonSystem.DisLike:
                        MainSectionButton.WeightSum = 4;
                        BtnWonder.Visibility = ViewStates.Visible;

                        TxtCountWoWonder.Visibility = ViewStates.Visible;
                        BtnCountWoWonder.Visibility = ViewStates.Visible;
                        ImgWoWonder.Visibility = ViewStates.Visible;

                        ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                        ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                        TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                        break;
                }

                if (!AppSettings.ShowShareButton && BtnShare != null)
                    BtnShare.Visibility = ViewStates.Gone;
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
                    BtnComment.Click += BtnCommentOnClick;
                    BtnShare.Click += BtnShareOnClick;
                    BtnCountLike.Click += BtnCountLikeOnClick;
                    BtnCountWoWonder.Click += BtnCountWoWonderOnClick;
                    InfoImageLiner.Click += MainLayoutOnClick;
                    MainLayout.Click += MainLayoutOnClick;
                     
                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click += BtnWonderOnClick;

                    LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs
                    {
                        NewsFeedClass = PostData,
                        View = TxtCountLike,
                    },null, "ImagePostViewerActivity");

                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                        LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs
                        {
                            NewsFeedClass = PostData,
                            View = TxtCountLike,
                        }, null, "ImagePostViewerActivity"); 
                }
                else
                {
                    BtnComment.Click -= BtnCommentOnClick;
                    BtnShare.Click -= BtnShareOnClick;
                    BtnCountLike.Click -= BtnCountLikeOnClick;
                    BtnCountWoWonder.Click -= BtnCountWoWonderOnClick;
                    InfoImageLiner.Click -= MainLayoutOnClick;
                    MainLayout.Click -= MainLayoutOnClick;

                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click -= BtnWonderOnClick;

                    LikeButton.Click += null!;
                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                        LikeButton.LongClick -= null!; 
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
                

                PageImage = null!;
                TxtDescription = null!;
                ImgLike = null!;
                ImgWoWonder = null!;
                TxtCountLike = null!;
                TxtCountWoWonder = null!;
                MainLayout = null!;
                InfoImageLiner = null!;
                BtnCountLike = null!;
                BtnCountWoWonder = null!;
                BtnLike = null!;
                BtnComment = null!;
                BtnShare = null!;
                MainSectionButton = null!;
                BtnWonder = null!;
                ImgWonder = null!;
                TxtWonder = null!;
                LikeButton = null!;
                ShareText = null!;
                ClickListener = null!;
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void MainLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoImageLiner.Visibility = InfoImageLiner.Visibility != ViewStates.Visible ? ViewStates.Visible : ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Add Wonder
        private void BtnWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (PostData.IsWondered != null && PostData.IsWondered.Value)
                {
                    var x = Convert.ToInt32(PostData.PostWonders);
                    if (x > 0)
                        x--;
                    else
                        x = 0;
                             
                    ImgWonder.SetColorFilter(Color.White);
                    ImgWoWonder.SetColorFilter(Color.White);
                    
                    PostData.IsWondered = false;
                    PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(x);
                    
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Wonder:
                            TxtWonder.Text = GetText(Resource.String.Btn_Wonder);
                            break;
                        case PostButtonSystem.DisLike:
                            TxtWonder.Text = GetText(Resource.String.Btn_Dislike);
                            break;
                    }

                    BtnWonder.Tag = "false";
                }
                else
                {
                    var x = Convert.ToInt32(PostData.PostWonders);
                    x++;

                    PostData.IsWondered = true;
                    PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    ImgWonder.SetColorFilter(Color.ParseColor("#f89823"));
                    ImgWoWonder.SetColorFilter(Color.ParseColor("#f89823"));

                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(x);

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Wonder:
                            TxtWonder.Text = GetText(Resource.String.Lbl_wondered);
                            break;
                        case PostButtonSystem.DisLike:
                            TxtWonder.Text = GetText(Resource.String.Lbl_disliked);
                            break;
                    }

                    BtnWonder.Tag = "true";
                }
                 
                TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostData.PostWonders));
                       
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(PostData.PostId, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(PostData.PostId, "dislike") });
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Event Show all users Wowonder >> Open Post PostData_Activity
        private void BtnCountWoWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PostDataActivity));
                intent.PutExtra("PostId", PostData.PostId);
                intent.PutExtra("PostType", "post_wonders");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Show all users liked >> Open Post PostData_Activity
        private void BtnCountLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    if (PostData.Reaction.Count > 0)
                    {
                        var intent = new Intent(this, typeof(ReactionPostTabbedActivity));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostData));
                        StartActivity(intent);
                    }
                }
                else
                {
                    var intent = new Intent(this, typeof(PostDataActivity));
                    intent.PutExtra("PostId", PostData.PostId);
                    intent.PutExtra("PostType", "post_likes");
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs {NewsFeedClass = PostData,}, PostModelType.ImagePost);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Add Comment
        private void BtnCommentOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = PostData, 
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion
         
        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("AlbumObject") ?? "");
                if (PostData != null)
                {
                    string index = Intent?.GetStringExtra("itemIndex") ?? "0";
                    if (index == "00")
                    { 
                        if (PostData.PhotoAlbum?.Count > 0)
                        {
                            var imagesList = PostData.PhotoAlbum;
                            ImageUrl = imagesList[0].Image;
                        }
                        else
                        {
                            ImageUrl = !string.IsNullOrEmpty(PostData.PostSticker) ? PostData.PostSticker : PostData.PostFileFull;
                        }
                        
                        //var bit = drawable_from_url(new Uri(ImageUrl));
                        //PageImage.SetImageBitmap(bit);
                        if (ImageUrl.Contains(".gif"))
                            Glide.With(this).Load(ImageUrl).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(PageImage);
                        else
                            Glide.With(this).Load(ImageUrl).Apply(new RequestOptions()).Into(PageImage); 
                    } 
                    else
                    {
                        var imageData = PostData.PhotoAlbum[Convert.ToInt32(index)];
                        if (imageData != null)
                        {
                            ImageUrl = imageData.Image;
                            //var bit = drawable_from_url(new Uri(imageData.Image));
                            //PageImage.SetImageBitmap(bit);

                            if (ImageUrl.Contains(".gif"))
                                Glide.With(this).Load(ImageUrl).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(PageImage);
                            else
                                Glide.With(this).Load(ImageUrl).Apply(new RequestOptions()).Into(PageImage);
                        }
                    }

                    if (string.IsNullOrEmpty(PostData.Orginaltext) || string.IsNullOrWhiteSpace(PostData.Orginaltext))
                    {
                        TxtDescription.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        var description = Methods.FunString.DecodeString(PostData.Orginaltext);
                        var readMoreOption = new StReadMoreOption.Builder()
                            .TextLength(250, StReadMoreOption.TypeCharacter)
                            .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                            .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                            .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LabelUnderLine(true)
                            .Build();
                        readMoreOption.AddReadMoreTo(TxtDescription, new Java.Lang.String(description)); 
                    }

                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                    {
                        PostData.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                        TxtCountLike.Text = Methods.FunString.FormatPriceValue(PostData.Reaction.Count);

                        if (PostData.Reaction.IsReacted != null && PostData.Reaction.IsReacted.Value)
                        {
                            if (!string.IsNullOrEmpty(PostData.Reaction.Type))
                            {
                                var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == PostData.Reaction.Type).Value?.Id ?? "";
                                switch (react)
                                {
                                    case "1":
                                        LikeButton.SetReactionPack(ReactConstants.Like);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                    case "2":
                                        LikeButton.SetReactionPack(ReactConstants.Love);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_love);
                                        break;
                                    case "3":
                                        LikeButton.SetReactionPack(ReactConstants.HaHa);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_haha);
                                        break;
                                    case "4":
                                        LikeButton.SetReactionPack(ReactConstants.Wow);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_wow);
                                        break;
                                    case "5":
                                        LikeButton.SetReactionPack(ReactConstants.Sad);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_sad);
                                        break;
                                    case "6":
                                        LikeButton.SetReactionPack(ReactConstants.Angry);
                                        ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                                        break;
                                    default:
                                        LikeButton.SetReactionPack(ReactConstants.Default);
                                        ImgLike.SetImageResource(PostData.Reaction.Count > 0 ? Resource.Drawable.emoji_like : Resource.Drawable.icon_post_like_vector);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            LikeButton.SetReactionPack(ReactConstants.Default);
                            LikeButton.SetTextColor(Color.White);

                            ImgLike.SetImageResource(PostData.Reaction.Count > 0 ? Resource.Drawable.emoji_like : Resource.Drawable.icon_post_like_vector);
                        }
                    }
                    else
                    {
                        ImgLike.SetImageResource(Resource.Drawable.icon_post_like_vector);

                        TxtCountLike.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostData.PostLikes));

                        if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                            TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(PostData.PostWonders));

                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder when PostData.IsWondered != null && PostData.IsWondered.Value:

                                BtnWonder.Tag = "true";
                                ImgWoWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                                ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                TxtWonder.Text = GetString(Resource.String.Lbl_wondered);
                                TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                break;
                            case PostButtonSystem.Wonder:

                                BtnWonder.Tag = "false";
                                ImgWoWonder.SetColorFilter(Color.White);

                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                                ImgWonder.SetColorFilter(Color.White);

                                TxtWonder.Text = GetString(Resource.String.Btn_Wonder);
                                TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                                break;
                            case PostButtonSystem.DisLike when PostData.IsWondered != null && PostData.IsWondered.Value:

                                BtnWonder.Tag = "true";
                                ImgWoWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                                ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                TxtWonder.Text = GetString(Resource.String.Lbl_disliked);
                                TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                                break;
                            case PostButtonSystem.DisLike:

                                BtnWonder.Tag = "false";
                                ImgWoWonder.SetColorFilter(Color.White);

                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                                ImgWonder.SetColorFilter(Color.White);

                                TxtWonder.Text = GetString(Resource.String.Btn_Dislike);
                                TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                                break;
                            case PostButtonSystem.Like when PostData.IsLiked != null && PostData.IsLiked.Value:

                                BtnLike.Tag = "true";
                                ImgLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                break;
                            case PostButtonSystem.Like:

                                BtnLike.Tag = "false";
                                ImgLike.SetColorFilter(Color.White);

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
 
    }
}