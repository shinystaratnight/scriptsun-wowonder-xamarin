using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class DeleteCommunitiesActivity : BaseActivity
    {
        #region Variables Basic

        private TextView IconPassword;
        private EditText TxtPassword;
        private CheckBox ChkDelete;
        private Button BtnDelete;
        private string CommunitiesType, CommunitiesId;

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
                SetContentView(Resource.Layout.Settings_DeleteAccount_layout);

                CommunitiesType = Intent?.GetStringExtra("Type") ?? string.Empty;
                CommunitiesId = Intent?.GetStringExtra("Id") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_AdMobNative(this);
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
                IconPassword = FindViewById<TextView>(Resource.Id.IconPassword);
                TxtPassword = FindViewById<EditText>(Resource.Id.PasswordEditText);

                ChkDelete = FindViewById<CheckBox>(Resource.Id.DeleteCheckBox);
                BtnDelete = FindViewById<Button>(Resource.Id.DeleteButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPassword, FontAwesomeIcon.Key);

                Methods.SetColorEditText(TxtPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ChkDelete.Text = GetText(Resource.String.Lbl_AreYouSureToDelete);
                BtnDelete.Text = GetText(Resource.String.Lbl_Delete);
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
                    switch (CommunitiesType)
                    {
                        case "Page":
                            toolbar.Title = GetText(Resource.String.Lbl_DeletePage);
                            break;
                        case "Group":
                            toolbar.Title = GetText(Resource.String.Lbl_DeleteGroup);
                            break;
                        default:
                            toolbar.Title = GetText(Resource.String.Lbl_Delete);
                            break;
                    }

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
                    BtnDelete.Click += BtnDeleteOnClick;
                }
                else
                {
                    BtnDelete.Click -= BtnDeleteOnClick;
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
                IconPassword = null!;
                TxtPassword = null!;
                ChkDelete = null!;
                CommunitiesType = null!; CommunitiesId = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Event Delete
        private void BtnDeleteOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!ChkDelete.Checked)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning),GetText(Resource.String.Lbl_You_can_not_access_your_disapproval),GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short)?.Show();
                    return;
                }

                var data = ListUtils.DataUserLoginList.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                if (data != null)
                {
                    if (TxtPassword.Text == data.Password)
                    {  
                        switch (CommunitiesType)
                        {
                            case "Page":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.DeletePage(CommunitiesId, TxtPassword.Text) });
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PageSuccessfullyDeleted),ToastLength.Short)?.Show();
                                break;
                            case "Group":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.DeleteGroup(CommunitiesId, TxtPassword.Text) });
                                Toast.MakeText(this, GetText(Resource.String.Lbl_GroupSuccessfullyDeleted), ToastLength.Short)?.Show();
                                break;
                        }
                         
                        Intent returnIntent = new Intent(); 
                        SetResult(Result.Ok, returnIntent);
                        Finish(); 
                    }
                    else
                    {
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning),GetText(Resource.String.Lbl_Please_confirm_your_password),GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 

    }
} 