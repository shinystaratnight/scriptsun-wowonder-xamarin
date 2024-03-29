﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using Org.Json;
using WoWonder.Activities.General;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.SocialLogins;
using WoWonder.Helpers.Utils;
using WoWonder.Library.OneSignal;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Console = System.Console;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback 
    {
        #region Variables Basic

        private TextView MTextViewForgotPwd, MTextViewCreateAccount, MTextViewSignUp;
        private EditText MEditTextEmail, MEditTextPassword;
        private Button MButtonViewSignIn;
        private ProgressBar ProgressBar;
        private LinearLayout LayoutCreateAccount;
        private LoginButton BtnFbLogin;
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker ProfileTracker;

        public static GoogleSignInClient MGoogleSignInClient;
        private SignInButton MGoogleSignIn;
        private string TimeZone = "";
        private bool IsActiveUser = true;

        #endregion
     
        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                //Set Full screen 
                Methods.App.FullScreenApp(this, true);

                // Create your application here
                SetContentView(Resource.Layout.Login_Layout);

                Client a = new Client(AppSettings.TripleDesAppServiceProvider);
                Console.WriteLine(a);

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();
                GetTimezone();

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
                ProfileTracker?.StopTracking();
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //protected override void AttachBaseContext(Context @base)
        //{
        //    try
        //    {
        //        base.AttachBaseContext(@base);
        //        if (AppSettings.Lang != "")
        //            LangController.SetApplicationLang(@base, AppSettings.Lang);
        //        else
        //        {
        //            UserDetails.LangName = Resources?.Configuration?.Locale?.DisplayLanguage.ToLower();
        //            LangController.SetAppLanguage(@base);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                //declare layouts and editText
                MEditTextEmail = (EditText)FindViewById(Resource.Id.editTxtEmail);
                MEditTextPassword = (EditText)FindViewById(Resource.Id.editTxtPassword);

                MTextViewSignUp = (TextView)FindViewById(Resource.Id.tvSignUp); // Register
                MButtonViewSignIn = (Button)FindViewById(Resource.Id.SignInButton); // Login
                 
                MTextViewForgotPwd = (TextView)FindViewById(Resource.Id.tvForgotPwd); // Forget password 

                LayoutCreateAccount = (LinearLayout)FindViewById(Resource.Id.layout_create_account);  
                MTextViewCreateAccount = (TextView)FindViewById(Resource.Id.tvCreateAccount);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                
                if (!AppSettings.EnableRegisterSystem)
                    LayoutCreateAccount.Visibility = ViewStates.Gone;
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
                    MButtonViewSignIn.Click += BtnLoginOnClick;
                    MTextViewCreateAccount.Click += RegisterButton_Click;
                    MTextViewSignUp.Click += RegisterButton_Click;
                    MTextViewForgotPwd.Click += TxtForgetPassOnClick;
                }
                else
                {
                    MButtonViewSignIn.Click -= BtnLoginOnClick;
                    MTextViewCreateAccount.Click -= RegisterButton_Click;
                    MTextViewSignUp.Click -= RegisterButton_Click;
                    MTextViewForgotPwd.Click -= TxtForgetPassOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    ProfileTracker = new FbMyProfileTracker();
                    ProfileTracker.StartTracking();
                    ProfileTracker.MOnProfileChanged += ProfileTrackerOnMOnProfileChanged;

                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Visible;
                    BtnFbLogin.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    BtnFbLogin.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                { 
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    MGoogleSignIn = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    MGoogleSignIn = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Login With Facebook
        private void ProfileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                   //var FbFirstName = e.MProfile.FirstName;
                   //var FbLastName = e.MProfile.LastName;
                   //var FbName = e.MProfile.Name;
                   //var FbProfileId = e.MProfile.Id;
                    
                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                MTextViewForgotPwd = null!;
                MTextViewCreateAccount = null!;
                MTextViewSignUp = null!;
                MEditTextEmail = null!;
                MEditTextPassword = null!;
                MButtonViewSignIn = null!;
                LayoutCreateAccount = null!;
                LayoutCreateAccount = null!;
                ProgressBar = null!;
                BtnFbLogin = null!;
                MFbCallManager = null!;
                MGoogleSignIn = null!;
                TimeZone = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Click Button Login
        private async void BtnLoginOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
                else
                {
                    if (!string.IsNullOrEmpty(MEditTextEmail.Text.Replace(" ", "")) || !string.IsNullOrEmpty(MEditTextPassword.Text))
                    {
                        HideKeyboard();

                        ProgressBar.Visibility = ViewStates.Visible;
                        MButtonViewSignIn.Visibility = ViewStates.Gone;

                        if (string.IsNullOrEmpty(TimeZone))
                            GetTimezone();

                        var (apiStatus, respond) = await RequestsAsync.Global.Get_Auth(MEditTextEmail.Text.Replace(" ", ""), MEditTextPassword.Text, TimeZone, UserDetails.DeviceId);
                        switch (apiStatus)
                        {
                            case 200 when respond is AuthObject auth:
                            {
                                var emailValidation = ListUtils.SettingsSiteList?.EmailValidation ?? "0";
                                if (emailValidation == "1")
                                {
                                    IsActiveUser = await CheckIsActiveUser(auth.UserId); 
                                }

                                if (IsActiveUser)
                                {
                                    SetDataLogin(auth);

                                    if (auth.Membership != null && auth.Membership.Value)
                                    {
                                        var intent = new Intent(this, typeof(GoProActivity));
                                        intent.PutExtra("class", "login");
                                        StartActivity(intent);
                                    }
                                    else
                                    { 
                                        if (AppSettings.ShowWalkTroutPage)
                                        {
                                            Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                            newIntent?.PutExtra("class", "login");
                                            StartActivity(newIntent);
                                        }
                                        else
                                        {
                                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                        }
                                    }
                                   
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                                    Finish();
                                }
                                else
                                {
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                                }

                                break;
                            }
                            case 200:
                            {
                                if (respond is AuthMessageObject messageObject)
                                {
                                    UserDetails.Username = MEditTextEmail.Text;
                                    UserDetails.FullName = MEditTextEmail.Text;
                                    UserDetails.Password = MEditTextPassword.Text;
                                    UserDetails.UserId = messageObject.UserId;
                                    UserDetails.Status = "Pending";
                                    UserDetails.Email = MEditTextEmail.Text;

                                    //Insert user data to database
                                    var user = new DataTables.LoginTb
                                    {
                                        UserId = UserDetails.UserId,
                                        AccessToken = "",
                                        Cookie = "",
                                        Username = MEditTextEmail.Text,
                                        Password = MEditTextPassword.Text,
                                        Status = "Pending",
                                        Lang = "",
                                        DeviceId = UserDetails.DeviceId,
                                    };
                                    ListUtils.DataUserLoginList.Add(user);

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                

                                    Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                                    newIntent?.PutExtra("TypeCode", "TwoFactor");
                                    StartActivity(newIntent);
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
                                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                                GetText(Resource.String.Lbl_Security),
                                                GetText(Resource.String.Lbl_ErrorLogin_3), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        case "4":
                                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                                GetText(Resource.String.Lbl_Security),
                                                GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        case "5":
                                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                                GetText(Resource.String.Lbl_Security),
                                                GetText(Resource.String.Lbl_ErrorLogin_5), GetText(Resource.String.Lbl_Ok));
                                            break;
                                        default:
                                            Methods.DialogPopup.InvokeAndShowDialog(this,
                                                GetText(Resource.String.Lbl_Security), errorText,
                                                GetText(Resource.String.Lbl_Ok));
                                            break;
                                    }
                                }

                                ProgressBar.Visibility = ViewStates.Gone;
                                MButtonViewSignIn.Visibility = ViewStates.Visible;
                                break;
                            }
                            case 404:
                                ProgressBar.Visibility = ViewStates.Gone;
                                MButtonViewSignIn.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                break;
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message,GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Button Register
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Click Forget Password
        private void TxtForgetPassOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result
         
        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data); 
                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int) resultCode, data);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

        #endregion

        #region Social Logins

        private string FbAccessToken,GAccessToken,GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                MButtonViewSignIn.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                //var data = json.ToString();
                //var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                //var FbEmail = result.Email;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    var (apiStatus, respond) = await RequestsAsync.Global.Get_SocialLogin(FbAccessToken, "facebook", UserDetails.DeviceId);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            if (respond is AuthObject auth)
                            {
                                SetDataLogin(auth);

                                if (AppSettings.ShowWalkTroutPage)
                                {
                                    Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                    newIntent?.PutExtra("class", "login");
                                    StartActivity(newIntent);
                                }
                                else
                                {
                                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                }
                                ProgressBar.Visibility = ViewStates.Gone;
                                MButtonViewSignIn.Visibility = ViewStates.Visible; 
                            }
                            Finish();
                            break;
                        }
                        case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;
                             
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }

                            break;
                        }
                        case 404:
                        {
                            var error = respond.ToString();
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                            break;
                        }
                    }

                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        //======================================================

        #region Google
            
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    MButtonViewSignIn.Visibility = ViewStates.Gone;

                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Global.Get_SocialLogin(GAccessToken, "google", UserDetails.DeviceId);
                        switch (apiStatus)
                        {
                            case 200:
                            {
                                if (respond is AuthObject auth)
                                {
                                    SetDataLogin(auth);

                                    if (AppSettings.ShowWalkTroutPage)
                                    {
                                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                        newIntent?.PutExtra("class", "login");
                                        StartActivity(newIntent);
                                    }
                                    else
                                    {
                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                    }
                                    ProgressBar.Visibility = ViewStates.Gone;
                                    MButtonViewSignIn.Visibility = ViewStates.Visible; 
                                }
                                Finish();
                                break;
                            }
                            case 400:
                            {
                                if (respond is ErrorObject error)
                                {
                                    var errorText = error.Error.ErrorText;

                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                }

                                break;
                            }
                            case 404:
                            {
                                var error = respond.ToString();
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                                break;
                            }
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message,GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

      
        #endregion

        //======================================================

        #endregion

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

        private async void GetTimezone()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    TimeZone = await ApiRequest.GetTimeZoneAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private async Task<bool> CheckIsActiveUser(string userId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Global.IsActiveUser(userId);
                switch (apiStatus)
                {
                    case 200 when respond is MessageObject auth:
                        Console.WriteLine(auth);
                        return true;
                    case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                            var errorId = error.Error.ErrorId;
                            switch (errorId)
                            {
                                case "5":
                                    Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_Security),GetText(Resource.String.Lbl_ThisUserNotActive), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case "4":
                                    Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_Security),GetText(Resource.String.Lbl_UserNotFound), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this,GetText(Resource.String.Lbl_Security), errorText,GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        break;
                    }
                    case 404:
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        break;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = MEditTextEmail.Text;
                UserDetails.FullName = MEditTextEmail.Text;
                UserDetails.Password = MEditTextPassword.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = MEditTextEmail.Text;
                
                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = MEditTextEmail.Text,
                    Password = MEditTextPassword.Text,
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