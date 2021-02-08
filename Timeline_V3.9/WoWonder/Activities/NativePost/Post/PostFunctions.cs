using System;
using Android.Content;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Activities.NativePost.Post
{
    public static class PostFunctions
    {
        public static PostModelType GetAdapterType(PostDataObject item)
        {
            try
            {
                if (item == null)
                    return PostModelType.NormalPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "ad")
                    return PostModelType.AdsPost;
                if (item.SharedInfo.SharedInfoClass != null)
                    return PostModelType.SharedPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "profile_cover_picture" || item.PostType == "profile_picture")
                    return PostModelType.ImagePost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "live" && !string.IsNullOrEmpty(item.StreamName))
                {
                    if (ListUtils.SettingsSiteList?.AgoraLiveVideo == 1 && !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AgoraAppId))
                    {
                        return PostModelType.AgoraLivePost; 
                    }
                    else 
                        return PostModelType.LivePost;
                }

                if (item.PostFileFull != null && (GetImagesExtensions(item.PostFileFull) || item.PhotoMulti?.Count > 0 || item.PhotoAlbum?.Count > 0 || !string.IsNullOrEmpty(item.AlbumName)))
                {
                    if (item.PhotoMulti?.Count > 0)
                    {
                        switch (item.PhotoMulti?.Count)
                        {
                            case 2:
                                return PostModelType.MultiImage2;
                            case 3:
                                return PostModelType.MultiImage3;
                            case 4:
                                return PostModelType.MultiImage4;
                            default:
                                {
                                    if (item.PhotoMulti?.Count >= 5)
                                        return PostModelType.MultiImages;
                                    break;
                                }
                        }
                    }

                    if (item.PhotoAlbum?.Count > 0)
                    {
                        switch (item.PhotoAlbum?.Count)
                        {
                            case 1:
                                return PostModelType.ImagePost;
                            case 2:
                                return PostModelType.MultiImage2;
                            case 3:
                                return PostModelType.MultiImage3;
                            case 4:
                                return PostModelType.MultiImage4;
                            default:
                                {
                                    if (item.PhotoAlbum?.Count >= 5)
                                        return PostModelType.MultiImages;
                                    break;
                                }
                        }
                    }

                    return PostModelType.ImagePost;
                }
                if (!string.IsNullOrEmpty(item.PostFileFull) && (item.PostFileFull.Contains(".MP3") || item.PostFileFull.Contains(".mp3") || item.PostFileFull.Contains(".wav")))
                    return PostModelType.VoicePost;
                if (!string.IsNullOrEmpty(item.PostRecord))
                    return PostModelType.VoicePost;
                if (!string.IsNullOrEmpty(item.PostFileFull) && GetVideosExtensions(item.PostFileFull ))
                    return PostModelType.VideoPost;
                if (!string.IsNullOrEmpty(item.PostSticker))
                    return PostModelType.StickerPost;
                if (!string.IsNullOrEmpty(item.PostFacebook))
                    return PostModelType.FacebookPost;
                if (!string.IsNullOrEmpty(item.PostVimeo))
                    return PostModelType.VimeoPost;
                if (!string.IsNullOrEmpty(item.PostYoutube))
                    return PostModelType.YoutubePost;
                if (!string.IsNullOrEmpty(item.PostDeepsound))
                    return PostModelType.DeepSoundPost;
                if (!string.IsNullOrEmpty(item.PostPlaytube))
                    return PostModelType.PlayTubePost;
                if (!string.IsNullOrEmpty(item.PostLink) && item.PostLink.Contains("tiktok"))
                    return PostModelType.TikTokPost;
                if (!string.IsNullOrEmpty(item.PostLink))
                    return PostModelType.LinkPost;
                if (item.Product?.ProductClass != null)
                    return PostModelType.ProductPost;
                if (item.Job != null && (item.PostType == "job" || item.Job.Value.JobInfoClass != null))
                    return PostModelType.JobPost;
                if (item.Offer?.OfferClass != null && item.PostType == "offer")
                    return PostModelType.OfferPost;
                if (item.Blog != null)
                    return PostModelType.BlogPost;
                if (item.Event?.EventClass != null)
                    return PostModelType.EventPost;
                if (item.PostFileFull != null && (item.PostFileFull.Contains(".rar") || item.PostFileFull.Contains(".zip") || item.PostFileFull.Contains(".pdf")))
                    return PostModelType.FilePost;
                if (item.ColorId != "0")
                    return PostModelType.ColorPost;
                if (item.PollId != "0")
                    return PostModelType.PollPost;
                if (item.FundData?.FundDataClass != null)
                    return PostModelType.FundingPost;
                if (item.Fund?.PurpleFund != null)
                    return PostModelType.PurpleFundPost;
                if (!string.IsNullOrEmpty(item.PostMap))
                    return PostModelType.MapPost;

                return PostModelType.NormalPost;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);  
                return PostModelType.NormalPost;
            }
        }
        public static string GetFeelingTypeIcon(string feeling)
        {
            try
            {
                if (string.IsNullOrEmpty(feeling))
                    return string.Empty;

                return feeling switch
                {
                    "sad" => "☹️",
                    "happy" => "😄",
                    "angry" => "😠",
                    "funny" => "😂",
                    "loved" => "😍",
                    "cool" => "🕶️",
                    "tired" => "😩",
                    "sleepy" => "😴",
                    "expressionless" => "😑",
                    "confused" => "😕",
                    "shocked" => "😱",
                    "so_sad" => "😭",
                    "blessed" => "😇",
                    "bored" => "😒",
                    "broken" => "💔",
                    "broke" => "💔",
                    "lovely" => "❤️",
                    "hot" => "😏",
                    _ => string.Empty
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return string.Empty;
            }
        }

        public static string GetFeelingTypeTextString(string feeling, Context activityContext)
        {
            try
            {
                if (string.IsNullOrEmpty(feeling))
                    return string.Empty;

                return feeling switch
                {
                    "sad" => activityContext.GetText(Resource.String.Lbl_Sad),
                    "happy" => activityContext.GetText(Resource.String.Lbl_Happy),
                    "angry" => activityContext.GetText(Resource.String.Lbl_Angry),
                    "funny" => activityContext.GetText(Resource.String.Lbl_Funny),
                    "loved" => activityContext.GetText(Resource.String.Lbl_Loved),
                    "cool" => activityContext.GetText(Resource.String.Lbl_Cool),
                    "tired" => activityContext.GetText(Resource.String.Lbl_Tired),
                    "sleepy" => activityContext.GetText(Resource.String.Lbl_Sleepy),
                    "expressionless" => activityContext.GetText(Resource.String.Lbl_Expressionless),
                    "confused" => activityContext.GetText(Resource.String.Lbl_Confused),
                    "shocked" => activityContext.GetText(Resource.String.Lbl_Shocked),
                    "so_sad" => activityContext.GetText(Resource.String.Lbl_VerySad),
                    "blessed" => activityContext.GetText(Resource.String.Lbl_Blessed),
                    "bored" => activityContext.GetText(Resource.String.Lbl_Bored),
                    "broken" => activityContext.GetText(Resource.String.Lbl_Broken),
                    "lovely" => activityContext.GetText(Resource.String.Lbl_Lovely),
                    "hot" => activityContext.GetText(Resource.String.Lbl_Hot),
                    _ => string.Empty
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return string.Empty;
            }
        }

        public static bool GetVideosExtensions(string extenstion)
        {
            try
            {
                if (string.IsNullOrEmpty(extenstion))
                    return false;

                if (extenstion.Contains(".MP4") || extenstion.Contains(".mp4"))
                    return true;
                if (extenstion.Contains(".WMV") || extenstion.Contains(".wmv"))
                    return true;
                if (extenstion.Contains(".3GP") || extenstion.Contains(".3gp"))
                    return true;
                if (extenstion.Contains(".WEBM") || extenstion.Contains(".webm"))
                    return true;
                if (extenstion.Contains(".FLV") || extenstion.Contains(".flv"))
                    return true;
                if (extenstion.Contains(".AVI") || extenstion.Contains(".avi"))
                    return true;
                if (extenstion.Contains(".HDV") || extenstion.Contains(".hdv"))
                    return true;
                if (extenstion.Contains(".MPEG") || extenstion.Contains(".mpeg"))
                    return true;
                if (extenstion.Contains(".MXF") || extenstion.Contains(".mxf"))
                    return true;
                if (extenstion.Contains(".mov") || extenstion.Contains(".MOV"))
                    return true; 
                else
                    return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool GetImagesExtensions(string extenstion)
        {
            try
            {
                if (string.IsNullOrEmpty(extenstion))
                    return false;

                if (extenstion.Contains(".PNG") || extenstion.Contains(".png"))
                    return true;
                if (extenstion.Contains(".JPG") || extenstion.Contains(".jpg"))
                    return true;
                if (extenstion.Contains(".GIF") || extenstion.Contains(".gif"))
                    return true;
                if (extenstion.Contains(".JPEG") || extenstion.Contains(".jpeg"))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        } 
    }
}