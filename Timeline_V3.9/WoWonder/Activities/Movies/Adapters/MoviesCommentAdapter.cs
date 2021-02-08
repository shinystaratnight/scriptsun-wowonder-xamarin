using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics; 
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Com.Tuyenmonkey.Textdecorator;
using Java.Util;
using Refractored.Controls;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Movies;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Movies.Adapters
{
    public class MoviesCommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        public readonly Activity ActivityContext;
        public readonly string Type;

        public ObservableCollection<CommentsMoviesObject> CommentList = new ObservableCollection<CommentsMoviesObject>();
        private readonly MoviesCommentClickListener PostEventListener;
        private readonly StReadMoreOption ReadMoreOption;

        public MoviesCommentAdapter(Activity context, string type)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                Type = type;
                PostEventListener = new MoviesCommentClickListener(ActivityContext, type);

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(250, StReadMoreOption.TypeCharacter)
                    .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => CommentList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case 0:
                        return new MoviesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                    case 666:
                        return new AdapterHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new MoviesCommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public void LoadCommentData(CommentsMoviesObject item, MoviesCommentAdapterViewHolder holder)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var text = Methods.FunString.DecodeString(item.Text);
                    ReadMoreOption.AddReadMoreTo(holder.CommentText, new Java.Lang.String(text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }

                holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Posted), false);
                holder.UserName.Text = item.UserData.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = item.UserData.Name;
                var textIsPro = string.Empty;

                if (item.UserData.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (item.UserData.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, item.UserData.Name.Length);

                if (item.UserData.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (item.UserData.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (item.Replies?.Count > 0)
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies.Count + ")";

                if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                {
                    if (item.IsCommentLiked)
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                        holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.LikeTextView.Tag = "Liked";
                    }

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Wonder when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_wondered);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                        case PostButtonSystem.Wonder:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Wonder);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                        case PostButtonSystem.DisLike when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_disliked);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor("#f89823"));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                        case PostButtonSystem.DisLike:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Dislike);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                    }
                }
                else
                {
                    if (item.IsCommentLiked)
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                        holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.LikeTextView.Tag = "Liked";
                    }
                    else
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                        holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                        holder.LikeTextView.Tag = "Like";
                    }
                }

                holder.TimeTextView.Tag = "true";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder.ItemViewType == 666)
                {
                    if (!(viewHolder is AdapterHolders.EmptyStateAdapterViewHolder emptyHolder))
                        return;

                    emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);

                    return;
                }

                if (!(viewHolder is MoviesCommentAdapterViewHolder holder))
                    return;

                var item = CommentList[position];
                if (item == null)
                    return;

                LoadCommentData(item, holder);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public CommentsMoviesObject GetItem(int position)
        {
            return CommentList[position];
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
                var item = CommentList[position];

                if (item.Text != EmptyState)
                    return 0;

                if (item.Text == EmptyState)
                    return 666;

                return 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (item.Text != EmptyState)
                    {
                        if (!string.IsNullOrEmpty(item.UserData.Avatar))
                            d.Add(item.UserData.Avatar);

                        return d;
                    }

                    return Collections.SingletonList(p0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class MoviesCommentAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
    {

        #region Variables Basic

        public View MainView { get; private set; }
        public MoviesCommentAdapter CommentAdapter;
        private readonly MoviesCommentClickListener PostClickListener;
        private readonly string TypeClass;

        public RelativeLayout MainCommentLayout { get; private set; }
        public LinearLayout BubbleLayout { get; private set; }
        public CircleImageView Image { get; private set; }
        public SuperTextView CommentText { get; private set; }
        public TextView TimeTextView { get; private set; }
        public TextView UserName { get; private set; }
        public TextView ReplyTextView { get; private set; }
        public TextView LikeTextView { get; private set; }
        public TextView DislikeTextView { get; private set; }

        public LinearLayout CountLikeSection { get; private set; }
        public TextView CountLike { get; private set; }
        public ImageView ImageCountLike { get; private set; }

        #endregion

        //Comment Article
        public MoviesCommentAdapterViewHolder(View itemView, MoviesCommentAdapter commentAdapter, MoviesCommentClickListener postClickListener, string typeClass = "Comment") : base(itemView)
        {
            try
            {
                MainView = itemView;

                CommentAdapter = commentAdapter;
                PostClickListener = postClickListener;
                TypeClass = typeClass;

                MainCommentLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.mainComment);
                BubbleLayout = MainView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<SuperTextView>(Resource.Id.active);
                CommentText?.SetTextInfo(CommentText);

                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);
                DislikeTextView = MainView.FindViewById<TextView>(Resource.Id.dislike);
                CountLikeSection = MainView.FindViewById<LinearLayout>(Resource.Id.countLikeSection);
                CountLike = MainView.FindViewById<TextView>(Resource.Id.countLike);
                ImageCountLike = MainView.FindViewById<ImageView>(Resource.Id.ImagecountLike);
                CountLikeSection.Visibility = ViewStates.Gone;

                var font = Typeface.CreateFromAsset(MainView.Context.Resources?.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);

                if (AppSettings.PostButton == PostButtonSystem.DisLike || AppSettings.PostButton == PostButtonSystem.Wonder)
                    DislikeTextView.Visibility = ViewStates.Visible;

                ReplyTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MainView.SetOnLongClickListener(this);
                Image.SetOnClickListener(this);
                LikeTextView.SetOnClickListener(this);
                DislikeTextView.SetOnClickListener(this);
                ReplyTextView.SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (AdapterPosition != RecyclerView.NoPosition)
                {
                    CommentsMoviesObject item = null!;
                    switch (TypeClass)
                    {
                        case "Comment":
                            item = CommentAdapter.CommentList[AdapterPosition];
                            break;
                        case "Reply":
                            item = CommentAdapter.CommentList[AdapterPosition];
                            break;
                    }

                    if (v.Id == Image.Id)
                        PostClickListener.ProfileClick(new CommentReplyMoviesClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == LikeTextView.Id)
                        PostClickListener.LikeCommentReplyPostClick(new CommentReplyMoviesClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == DislikeTextView.Id)
                        PostClickListener.DislikeCommentReplyPostClick(new CommentReplyMoviesClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == ReplyTextView.Id)
                        PostClickListener.CommentReplyClick(new CommentReplyMoviesClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnLongClick(View v)
        {
            //add event if System = ReactButton 
            if (AdapterPosition != RecyclerView.NoPosition)
            {
                CommentsMoviesObject item = null!;
                switch (TypeClass)
                {
                    case "Comment":
                        item = CommentAdapter.CommentList[AdapterPosition];
                        break;
                    case "Reply":
                        item = CommentAdapter.CommentList[AdapterPosition];
                        break;
                }

                if (v.Id == MainView.Id)
                    PostClickListener.MoreCommentReplyPostClick(new CommentReplyMoviesClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
            }

            return true;
        }
    }

}