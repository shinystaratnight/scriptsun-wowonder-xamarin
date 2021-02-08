using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ForgetPasswordActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                 
                // Create your application here
                SetContentView(Resource.Layout.ForgetPassword_Layout);

                EmailEditext = FindViewById<EditText>(Resource.Id.emailfield);
                BtnSend = FindViewById<Button>(Resource.Id.SendButton);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                MainLinearLayout = FindViewById<LinearLayout>(Resource.Id.mainLinearLayout);

                ProgressBar.Visibility = ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                //Add Event
                MainLinearLayout.Click += MainLinearLayoutOnClick;
                BtnSend.Click += BtnSendOnClick;
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

                //Close Event
                MainLinearLayout.Click -= MainLinearLayoutOnClick;
                BtnSend.Click -= BtnSendOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void BtnSendOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!string.IsNullOrEmpty(EmailEditext.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditext.Text);
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                GetText(Resource.String.Lbl_Security),
                                GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            HideKeyboard();
                            ProgressBar.Visibility = ViewStates.Visible;
                            var (apiStatus, respond) = await RequestsAsync.Global.Get_Reset_Password_Email(EmailEditext.Text);
                            switch (apiStatus)
                            {
                                case 200:
                                {
                                    if (respond is StatusObject result)
                                    {
                                        Console.WriteLine(result);
                                        ProgressBar.Visibility = ViewStates.Invisible;
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Email_Has_Been_Send), GetText(Resource.String.Lbl_Ok));
                                    }

                                    break;
                                }
                                case 400:
                                {
                                    if (respond is ErrorObject error)
                                    {
                                        var errorText = error.Error.ErrorText;
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    }

                                    ProgressBar.Visibility = ViewStates.Invisible;
                                    break;
                                }
                                case 404:
                                    ProgressBar.Visibility = ViewStates.Invisible;
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_Error_Login), GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Invisible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),
                            GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Invisible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed),
                    exception.ToString(), GetText(Resource.String.Lbl_Ok));
            }
        }

        private void MainLinearLayoutOnClick(object sender, EventArgs eventArgs)
        {
            HideKeyboard();
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        private void DestroyBasic()
        {
            try
            {
                BtnSend = null!;
                EmailEditext = null!;
                MainLinearLayout = null!; 
                ProgressBar = null!; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Variables Basic

        private Button BtnSend;
        private EditText EmailEditext;
        private LinearLayout MainLinearLayout;

        private ProgressBar ProgressBar;

        #endregion
    }
}