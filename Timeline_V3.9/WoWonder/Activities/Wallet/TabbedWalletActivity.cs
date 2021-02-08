using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Razorpay;
using IAmMert.ReadableBottomBarLib;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Wallet.Fragment;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.PayPal.Android;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Wallet
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedWalletActivity : BaseActivity, ReadableBottomBar.IItemSelectListener, IPaymentResultWithDataListener
    { 
        #region Variables Basic

        private ReadableBottomBar BottomBar;
        private FragmentBottomNavigationView FragmentBottomNavigator;
        public SendMoneyFragment SendMoneyFragment;
        public AddFundsFragment AddFundsFragment;
        public string TypeOpenPayment = "";
        private static TabbedWalletActivity Instance;

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
                SetContentView(Resource.Layout.TabbedWalletLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                SetupBottomNavigationView();
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
                if (AppSettings.ShowPaypal)
                    AddFundsFragment?.InitPayPalPayment?.StopPayPalService();

                if (AppSettings.ShowRazorPay)
                    AddFundsFragment?.InitRazorPay?.StopRazorPay();

                if (AppSettings.ShowPayStack)
                    AddFundsFragment?.PayStackPayment?.StopPayStack();
                
                if (AppSettings.ShowCashFree)
                    AddFundsFragment?.CashFreePayment?.StopCashFree();

                if (AppSettings.ShowPaySera)
                    AddFundsFragment?.PaySeraPayment?.StopPaySera();

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
                BottomBar = FindViewById<ReadableBottomBar>(Resource.Id.readableBottomBar);
                BottomBar.SetOnItemSelectListener(this); 
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
                    toolbar.Title = GetString(Resource.String.Lbl_WalletCredits);
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

        public static TabbedWalletActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                BottomBar = null!;
                FragmentBottomNavigator = null!;
                SendMoneyFragment = null!;
                AddFundsFragment = null!;
                TypeOpenPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Set Navigation And Show Fragment

        private void SetupBottomNavigationView()
        {
            try
            {
                FragmentBottomNavigator = new FragmentBottomNavigationView(this);

                SendMoneyFragment = new SendMoneyFragment();
                AddFundsFragment = new AddFundsFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(SendMoneyFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(AddFundsFragment);

                FragmentBottomNavigator.NavigationTabBarOnStartTabSelected(0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                if (TypeOpenPayment == "AddFundsFragment")
                {
                    if (requestCode == AddFundsFragment?.InitPayPalPayment?.PayPalDataRequestCode)
                    {
                        switch (resultCode)
                        {
                            case Result.Ok:
                                var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                                PaymentConfirmation configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                                if (configuration != null)
                                {
                                    //string createTime = configuration.ProofOfPayment.CreateTime;
                                    //string intent = configuration.ProofOfPayment.Intent;
                                    //string paymentId = configuration.ProofOfPayment.PaymentId;
                                    //string state = configuration.ProofOfPayment.State;
                                    //string transactionId = configuration.ProofOfPayment.TransactionId;
                                    if (Methods.CheckConnectivity())
                                    {
                                        var (apiStatus, respond) = await RequestsAsync.Global.TopUpWalletAsync(UserDetails.UserId, AddFundsFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    AddFundsFragment.TxtAmount.Text = string.Empty;

                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Methods.DisplayReportResultTrack(e);
                                                }
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                    }
                                }

                                break;
                            case (int)Result.Canceled:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                                break;
                        }
                    }
                    else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long)?.Show();
                    }
                }
                else if (requestCode == 1202 && resultCode ==  Result.Ok)
                {
                    var userObject = data.GetStringExtra("DataUser");
                    if (!string.IsNullOrEmpty(userObject))
                    {
                        var userData = JsonConvert.DeserializeObject<UserDataObject>(userObject);
                        if (userData != null)
                        {
                           SendMoneyFragment.TxtEmail.Text = WoWonderTools.GetNameFinal(userData);
                           SendMoneyFragment.UserId = userData.UserId;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion 

        public void OnItemSelected(int index)
        {
            try
            {
                FragmentBottomNavigator.NavigationTabBarOnStartTabSelected(index);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
 
        public void OnPaymentError(int code, string response, PaymentData p2)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                Toast.MakeText(this, "Payment failed: " + response, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId, PaymentData p1)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                if (!string.IsNullOrEmpty(p1?.PaymentId))
                {
                    if (Methods.CheckConnectivity())
                    {
                        var priceInt = Convert.ToInt32(AddFundsFragment?.Price) * 100;
                        var keyValues = new Dictionary<string, string>
                        {
                            {"merchant_amount", priceInt.ToString()},
                        };

                        var (apiStatus, respond) = await RequestsAsync.Global.RazorPay(p1.PaymentId, "wallet", keyValues).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    AddFundsFragment.TxtAmount.Text = string.Empty;
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
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