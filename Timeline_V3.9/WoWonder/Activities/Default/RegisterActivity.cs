using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using Java.Lang;
using WoWonder.Activities.General;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignal;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private Button RegisterButton;
        private EditText EmailEditText, UsernameEditText, PasswordEditText, PasswordRepeatEditText, FirstNameEditText, LastNameEditText, GenderEditText, PhoneEditText;
        private LinearLayout MainLinearLayout;

        private ProgressBar ProgressBar; 
        private CheckBox ChkAgree;
        private TextView SecPrivacyTextView;
        private TextView SecTermTextView;
        private string GenderStatus = "male";
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                //Set Full screen 
                Methods.App.FullScreenApp(this, true);

                SetContentView(Resource.Layout.Register_Layout);

                try
                {
                    Window?.SetBackgroundDrawableResource(Resource.Drawable.RegisterScreen);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

                EmailEditText = FindViewById<EditText>(Resource.Id.emailfield);
                UsernameEditText = FindViewById<EditText>(Resource.Id.usernamefield);
                PasswordEditText = FindViewById<EditText>(Resource.Id.passwordfield);
                PasswordRepeatEditText = FindViewById<EditText>(Resource.Id.ConfirmPasswordfield);
                FirstNameEditText = FindViewById<EditText>(Resource.Id.firstNamefield);
                LastNameEditText = FindViewById<EditText>(Resource.Id.lastNamefield);
                GenderEditText = FindViewById<EditText>(Resource.Id.Genderfield);
                PhoneEditText = FindViewById<EditText>(Resource.Id.Phonefield);

                GenderEditText.Visibility = AppSettings.ShowGenderOnRegister ? ViewStates.Visible : ViewStates.Gone;
                Methods.SetFocusable(GenderEditText);

                MainLinearLayout = FindViewById<LinearLayout>(Resource.Id.mainLinearLayout);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                RegisterButton = FindViewById<Button>(Resource.Id.signUpButton);

                ProgressBar.Visibility = ViewStates.Invisible;

                ChkAgree = FindViewById<CheckBox>(Resource.Id.termCheckBox);
                SecTermTextView = FindViewById<TextView>(Resource.Id.secTermTextView);
                SecPrivacyTextView = FindViewById<TextView>(Resource.Id.secPrivacyTextView);
                 
                var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                if (smsOrEmail == "sms")
                {
                    PhoneEditText.Visibility = ViewStates.Visible;
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    LoadConfigSettings();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        LoadConfigSettings();
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage
                        }, 101);
                    }
                }

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                {
                    OneSignalNotification.RegisterNotificationDevice();
                }
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
                RegisterButton.Click += RegisterButtonOnClick;
                SecTermTextView.Click += SecTermTextView_Click;
                SecPrivacyTextView.Click += SecPrivacyTextView_Click;
                GenderEditText.Touch += GenderEditTextOnTouch;
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
                RegisterButton.Click -= RegisterButtonOnClick;
                SecTermTextView.Click -= SecTermTextView_Click;
                SecPrivacyTextView.Click -= SecPrivacyTextView_Click;
                GenderEditText.Touch -= GenderEditTextOnTouch;
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

        private void DestroyBasic()
        {
            try
            {
                RegisterButton = null!;
                EmailEditText = null!;
                UsernameEditText = null!;
                PasswordEditText = null!;
                PasswordRepeatEditText = null!;
                FirstNameEditText = null!;
                LastNameEditText = null!;
                GenderEditText = null!;
                PhoneEditText = null!;
                MainLinearLayout = null!;
                ProgressBar = null!;
                SecPrivacyTextView = null!;
                ChkAgree = null!;
                SecTermTextView = null!;
                GenderStatus = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SecPrivacyTextView_Click(object sender, EventArgs e)
        {
            try
            {
                var url = Client.WebsiteUrl + "/terms/privacy-policy";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SecTermTextView_Click(object sender, EventArgs e)
        {
            try
            {
                var url = Client.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void GenderEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                }
                else
                {
                    arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                    arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                }
               
                dialogList.Title(GetText(Resource.String.Lbl_Gender));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private async void RegisterButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (ChkAgree.Checked)
                {
                    if (Methods.CheckConnectivity())
                    { 
                        if (!string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) ||!string.IsNullOrEmpty(FirstNameEditText.Text.Replace(" ", "")) ||!string.IsNullOrEmpty(LastNameEditText.Text.Replace(" ", "")) ||
                            !string.IsNullOrEmpty(PasswordEditText.Text) ||
                            !string.IsNullOrEmpty(PasswordRepeatEditText.Text) ||
                            !string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")))
                        {
                            if (AppSettings.ShowGenderOnRegister && string.IsNullOrEmpty(GenderStatus))
                            {
                                ProgressBar.Visibility = ViewStates.Invisible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            var smsOrEmail = ListUtils.SettingsSiteList?.SmsOrEmail;
                            if (smsOrEmail == "sms" && string.IsNullOrEmpty(PhoneEditText.Text))
                            {
                                ProgressBar.Visibility = ViewStates.Invisible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                                return;
                            }
                             
                            var check = Methods.FunString.IsEmailValid(EmailEditText.Text.Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this,
                                    GetText(Resource.String.Lbl_VerificationFailed),
                                    GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                if (PasswordRepeatEditText.Text == PasswordEditText.Text)
                                {
                                    HideKeyboard();

                                    ProgressBar.Visibility = ViewStates.Visible;

                                    var (apiStatus, respond) = await RequestsAsync.Global.Get_Create_Account(UsernameEditText.Text.Replace(" ", ""), PasswordEditText.Text,PasswordRepeatEditText.Text, EmailEditText.Text.Replace(" ", ""),
                                        GenderStatus, PhoneEditText.Text, UserDetails.DeviceId);
                                    switch (apiStatus)
                                    {
                                        case 200:
                                        {
                                            if (respond is CreatAccountObject result)
                                            {
                                                SetDataLogin(result);
                                              
                                                var dataPrivacy = new Dictionary<string, string>
                                                {
                                                    {"first_name", FirstNameEditText.Text},
                                                    {"last_name", LastNameEditText.Text},
                                                };
                                             
                                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Update_User_Data(dataPrivacy) });
                                             
                                                if (AppSettings.ShowWalkTroutPage)
                                                {
                                                    Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                                    newIntent?.PutExtra("class", "register");
                                                    StartActivity(newIntent);
                                                }
                                                else
                                                {     
                                                    if (ListUtils.SettingsSiteList?.MembershipSystem == "1")
                                                    {
                                                        var intent = new Intent(this, typeof(GoProActivity));
                                                        intent.PutExtra("class", "register");
                                                        StartActivity(intent); 
                                                    } 
                                                    else if (AppSettings.ShowSuggestedUsersOnRegister)
                                                    {
                                                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                                                        newIntent?.PutExtra("class", "register");
                                                        StartActivity(newIntent);
                                                    }
                                                    else
                                                    {
                                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                                    }
                                                }
                                            }
                                       
                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            Finish();
                                            break;
                                        }
                                        case 220:
                                        {
                                            if (respond is AuthMessageObject messageObject)
                                            {
                                                switch (smsOrEmail)
                                                {
                                                    case "sms":
                                                    {
                                                        UserDetails.Username = UsernameEditText.Text;
                                                        UserDetails.FullName = FirstNameEditText.Text + " " + LastNameEditText.Text;
                                                        UserDetails.Password = PasswordEditText.Text;
                                                        UserDetails.UserId = messageObject.UserId;
                                                        UserDetails.Status = "Pending";
                                                        UserDetails.Email = EmailEditText.Text;

                                                        //Insert user data to database
                                                        var user = new DataTables.LoginTb
                                                        {
                                                            UserId = UserDetails.UserId,
                                                            AccessToken = UserDetails.AccessToken,
                                                            Cookie = UserDetails.Cookie,
                                                            Username = UserDetails.Username,
                                                            Password = UserDetails.Password,
                                                            Status = "Pending",
                                                            Lang = "",
                                                            DeviceId = UserDetails.DeviceId,
                                                            Email = UserDetails.Email,
                                                        };

                                                        ListUtils.DataUserLoginList.Clear();
                                                        ListUtils.DataUserLoginList.Add(user);

                                                        var dbDatabase = new SqLiteDatabase();
                                                        dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                                

                                                        Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                                                        newIntent?.PutExtra("TypeCode", "AccountSms");
                                                        StartActivity(newIntent);
                                                        break;
                                                    }
                                                    case "mail":
                                                    {
                                                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                                                        dialog.Title(GetText(Resource.String.Lbl_ActivationSent));
                                                        dialog.Content(GetText(Resource.String.Lbl_ActivationDetails).Replace("@", EmailEditText.Text));
                                                        dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                                                        dialog.AlwaysCallSingleChoiceCallback();
                                                        dialog.Build().Show();
                                                        break;
                                                    }
                                                    default:
                                                        ProgressBar.Visibility = ViewStates.Invisible;
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), messageObject.Message, GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                }
                                            }

                                            break;
                                        }
                                        case 400:
                                        {
                                            if (respond is ErrorObject error)
                                            {
                                                var errorText = error.Error.ErrorText;

                                                var errorId = error.Error.ErrorId;
                                                switch (errorId)
                                                {
                                                    case "3":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_3), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "4":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_4), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "5":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_something_went_wrong), GetText(Resource.String.Lbl_Ok)); break;
                                                    case "6":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_6), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "7":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_7), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "8":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_8), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "9":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_9), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "10":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_10), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    case "11":
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorRegister_11), GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                    default:
                                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                                        break;
                                                }
                                            }

                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            break;
                                        }
                                        case 404:
                                            ProgressBar.Visibility = ViewStates.Invisible;
                                            Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_Security),respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                            break;
                                    }
                                }
                                else
                                {
                                    ProgressBar.Visibility = ViewStates.Invisible;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Register_password), GetText(Resource.String.Lbl_Ok));
                                }
                            }
                        }
                        else
                        {
                            ProgressBar.Visibility = ViewStates.Invisible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Invisible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressBar.Visibility = ViewStates.Invisible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
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


        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        LoadConfigSettings();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        Finish();
                    }
                }
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
         
        private void SetDataLogin(CreatAccountObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = UsernameEditText.Text;
                UserDetails.FullName = FirstNameEditText.Text + " " + LastNameEditText.Text;
                UserDetails.Password = PasswordEditText.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = EmailEditText.Text;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId, 
                    Email = UserDetails.Email,  
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    Finish();
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

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    GenderEditText.Text = itemString.ToString();

                    var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString.ToString()).Key;
                    GenderStatus = key ?? "male";
                }
                else
                {
                    if (itemString.ToString() == GetText(Resource.String.Radio_Male))
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
                    else if (itemString.ToString() == GetText(Resource.String.Radio_Female))
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Female);
                        GenderStatus = "female";
                    }
                    else
                    {
                        GenderEditText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion


        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}