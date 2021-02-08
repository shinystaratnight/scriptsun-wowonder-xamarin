using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Media;


using Android.Views;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Tuyenmonkey.Textdecorator;
using Java.IO;
using Java.Util;
using WoWonder.Activities.Comment.Fragment;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient;
using WoWonderClient.Classes.Comments;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using Timer = System.Timers.Timer;

namespace WoWonder.Activities.Comment.Adapters
{
    public class CommentObjectExtra : GetCommentObject
    {
        public new MediaPlayer MediaPlayer { get; set; }
        public new Timer MediaTimer { get; set; } 
    }

    public class CommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        public readonly Activity ActivityContext;

        public ObservableCollection<CommentObjectExtra> CommentList = new ObservableCollection<CommentObjectExtra>();
        private readonly CommentClickListener PostEventListener;
        private readonly StReadMoreOption ReadMoreOption;

        public CommentAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                PostEventListener = new CommentClickListener(ActivityContext, "Comment");

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
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                    case 1:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment_Image, parent, false), this, PostEventListener);
                    case 2:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment_Voice, parent, false), this, PostEventListener);
                    case 666:
                        return new AdapterHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public void LoadCommentData(CommentObjectExtra item, CommentAdapterViewHolder holder)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Orginaltext) || !string.IsNullOrWhiteSpace(item.Orginaltext))
                {
                    var text = Methods.FunString.DecodeString(item.Orginaltext);
                    ReadMoreOption.AddReadMoreTo(holder.CommentText, new Java.Lang.String(text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }

                holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time) , false);
                holder.UserName.Text = item.Publisher.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircle;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircle);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                //Image
                if (holder.ItemViewType == 1 || holder.CommentImage != null)
                {
                    if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.CFile);
                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);

                        //GlideImageLoader.LoadImage(ActivityContext,item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        if (!item.CFile.Contains(Client.WebsiteUrl))
                            item.CFile = WoWonderTools.GetTheFinalLink(item.CFile);

                        GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                        item.CFile = WoWonderTools.GetFile("", Methods.Path.FolderDiskImage, item.CFile.Split('/').Last(), item.CFile);
                    } 
                }

                //Voice
                if (holder.VoiceLayout != null && !string.IsNullOrEmpty(item.Record))
                {
                    LoadAudioItem(holder, item);
                }

                var repliesCount = !string.IsNullOrEmpty(item.RepliesCount) ? item.RepliesCount : item.Replies ?? ""; 
                if (repliesCount != "0" && !string.IsNullOrEmpty(repliesCount))
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + repliesCount + ")";

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.ReactionDefault:
                    case PostButtonSystem.ReactionSubShine:
                    {
                        item.Reaction ??= new Reaction();
                   
                        if (item.Reaction.Count > 0)
                        {
                            holder.CountLikeSection.Visibility = ViewStates.Visible;
                            holder.CountLike.Text = Methods.FunString.FormatPriceValue(item.Reaction.Count);  
                        }
                        else
                        {
                            holder.CountLikeSection.Visibility = ViewStates.Gone;
                        }
                     
                        if (item.Reaction.IsReacted != null && item.Reaction.IsReacted.Value)
                        { 
                            if (!string.IsNullOrEmpty(item.Reaction.Type))
                            {
                                var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == item.Reaction.Type).Value?.Id ?? "";
                                switch (react)
                                {
                                    case "1":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.Like);
                                        holder.LikeTextView.Tag = "Liked"; 
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                    case "2":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.Love);
                                        holder.LikeTextView.Tag = "Liked";
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                        break;
                                    case "3":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.HaHa);
                                        holder.LikeTextView.Tag = "Liked";
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                        break;
                                    case "4":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.Wow);
                                        holder.LikeTextView.Tag = "Liked";
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                        break;
                                    case "5":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.Sad);
                                        holder.LikeTextView.Tag = "Liked";
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                        break;
                                    case "6":
                                        ReactionComment.SetReactionPack(holder, ReactConstants.Angry);
                                        holder.LikeTextView.Tag = "Liked";
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                        break;
                                    default:
                                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                        holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                        holder.LikeTextView.Tag = "Like";

                                        if (item.Reaction.Count > 0)
                                            holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);

                                        break;
                                }
                            }
                        }
                        else
                        {
                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                            holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                            holder.LikeTextView.Tag = "Like";
                            if (item.Reaction.Count > 0)
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                        }

                        break;
                    }
                    case PostButtonSystem.Wonder:
                    case PostButtonSystem.DisLike:
                    {
                        if (item.Reaction?.IsReacted != null && !item.Reaction.IsReacted.Value)
                            ReactionComment.SetReactionPack(holder, ReactConstants.Default);

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

                        break;
                    }
                    default:
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

                        break;
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

                    var itemEmpty = CommentList.FirstOrDefault(a => a.Id == EmptyState);
                    if (itemEmpty != null && !string.IsNullOrEmpty(itemEmpty.Orginaltext))
                    {
                        emptyHolder.EmptyText.Text = itemEmpty.Orginaltext;
                    }
                    else
                    {
                        emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);
                    }
                    return;
                }

                if (!(viewHolder is CommentAdapterViewHolder holder))
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

        public int PositionSound;

        private void LoadAudioItem(CommentAdapterViewHolder soundViewHolder, CommentObjectExtra item)
        {
            try
            { 
                soundViewHolder.VoiceLayout.Visibility = ViewStates.Visible;

                var fileName = item.Record.Split('/').Last();

                var mediaFile = WoWonderTools.GetFile(item.PostId, Methods.Path.FolderDcimSound, fileName, item.Record);

                if (string.IsNullOrEmpty(item.MediaDuration) || item.MediaDuration == "00:00")
                {
                    var duration = WoWonderTools.GetDuration(mediaFile);
                    soundViewHolder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(duration);
                }
                else
                    soundViewHolder.DurationVoice.Text = item.MediaDuration;

                soundViewHolder.PlayButton.Visibility = ViewStates.Visible;

                if (item.MediaIsPlaying)
                {
                    soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);
                }
                else
                {
                    soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public CommentObjectExtra GetItem(int position)
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

                if (string.IsNullOrEmpty(item.CFile) && string.IsNullOrEmpty(item.Record) && item.Text != EmptyState)
                    return 0;

                if (!string.IsNullOrEmpty(item.CFile) && !string.IsNullOrEmpty(item.Record) || !string.IsNullOrEmpty(item.CFile))
                    return 1;

                if (!string.IsNullOrEmpty(item.Record))
                    return 2;

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
                        if (!string.IsNullOrEmpty(item.CFile))
                            d.Add(item.CFile);

                        if (!string.IsNullOrEmpty(item.Publisher.Avatar))
                            d.Add(item.Publisher.Avatar);

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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }
}
