using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;

using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Razorpay;
using Java.Lang;
using WoWonder.Activities.Base;
using WoWonder.Activities.General.Adapters;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonder.PaymentGoogle;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback , IPaymentResultWithDataListener
    {
        #region Variables Basic

        private RecyclerView MainRecyclerView, MainPlansRecyclerView;
        private GridLayoutManager LayoutManagerView;
        private LinearLayoutManager PlansLayoutManagerView;
        private GoProFeaturesAdapter FeaturesAdapter;
        private UpgradeGoProAdapter PlansAdapter;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;
        private InitRazorPayPayment InitRazorPay;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitPaySeraPayment PaySeraPayment;
        private ImageView IconClose;
        private string Caller, PayId, Price, PayType;
        private UpgradeGoProClass ItemUpgrade;

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
                SetContentView(Resource.Layout.Go_Pro_Layout);

                Caller = Intent?.GetStringExtra("class") ?? "";

                //Get Value And Set Toolbar
                InitBuy();
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
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
                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment?.DisconnectInAppBilling();
               
                if (AppSettings.ShowPaypal)
                    InitPayPalPayment?.StopPayPalService();

                if (AppSettings.ShowRazorPay)
                    InitRazorPay?.StopRazorPay();

                if (AppSettings.ShowPayStack)
                    PayStackPayment?.StopPayStack();

                if (AppSettings.ShowCashFree)
                    CashFreePayment?.StopCashFree();

                if (AppSettings.ShowPaySera)
                    PaySeraPayment?.StopPaySera();

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
                    FinishPage();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions
       
        private void InitBuy()
        {
            try
            { 
                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment = new InitInAppBillingPayment(this);

                if (AppSettings.ShowPaypal)
                    InitPayPalPayment = new InitPayPalPayment(this);

                if (AppSettings.ShowRazorPay)
                    InitRazorPay = new InitRazorPayPayment(this);

                if (AppSettings.ShowPayStack)
                    PayStackPayment = new InitPayStackPayment(this);

                if (AppSettings.ShowCashFree)
                    CashFreePayment = new InitCashFreePayment(this);

                if (AppSettings.ShowPaySera)
                    PaySeraPayment = new InitPaySeraPayment(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);
                MainPlansRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler2);
                IconClose = FindViewById<ImageView>(Resource.Id.iv1);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                FeaturesAdapter = new GoProFeaturesAdapter(this);
                LayoutManagerView = new GridLayoutManager(this, 3);
                MainRecyclerView.SetLayoutManager(LayoutManagerView);
                MainRecyclerView.HasFixedSize = true;
                MainRecyclerView.SetAdapter(FeaturesAdapter);

                PlansAdapter = new UpgradeGoProAdapter(this);
                PlansLayoutManagerView = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MainPlansRecyclerView.SetLayoutManager(PlansLayoutManagerView);
                MainPlansRecyclerView.HasFixedSize = true;
                MainPlansRecyclerView.SetAdapter(PlansAdapter);
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
                    PlansAdapter.UpgradeButtonItemClick += PlansAdapterOnItemClick;
                    IconClose.Click += IconCloseOnClick;
                }
                else
                {
                    PlansAdapter.UpgradeButtonItemClick -= PlansAdapterOnItemClick;
                    IconClose.Click -= IconCloseOnClick;
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
                MainRecyclerView = null!;
                MainPlansRecyclerView = null!;
                LayoutManagerView = null!;
                PlansLayoutManagerView = null!;
                FeaturesAdapter = null!;
                PlansAdapter = null!;
                InitPayPalPayment = null!;
                InitRazorPay = null!;
                PayStackPayment = null!;
                PayStackPayment = null!;
                BillingPayment = null!;
                IconClose = null!;
                PayId = null!;
                Price = null!;
                PayType = null!;
                ItemUpgrade = null!; 
                PaySeraPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void PlansAdapterOnItemClick(object sender, UpgradeGoProAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    ItemUpgrade = PlansAdapter.GetItem(e.Position);
                    if (ItemUpgrade != null)
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (AppSettings.ShowInAppBilling && Client.IsExtended)
                            arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));

                        if (AppSettings.ShowPaypal)
                            arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                        if (AppSettings.ShowCreditCard)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                        if (AppSettings.ShowBankTransfer)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                        if (AppSettings.ShowRazorPay)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));

                        if (AppSettings.ShowPayStack)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));

                        if (AppSettings.ShowCashFree)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));

                        if (AppSettings.ShowPaySera)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show(); 
                    } 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishPage();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);
                 
                if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
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

                                if (PayType == "membership")
                                {
                                    await SetProAsync();
                                }
                            }

                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                            break;
                    }
                }
                else switch (requestCode)
                {
                    case PaymentActivity.ResultExtrasInvalid:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long)?.Show();
                        break;
                    case 1001 when resultCode == Result.Ok && AppSettings.ShowInAppBilling && Client.IsExtended:
                        await SetProAsync();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }



        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayType = "membership";
                    PayId = ItemUpgrade.Id.ToString();
                    InitPayPalPayment.BtnPaypalOnClick(Price, "membership");
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id.ToString();

                    BillingPayment.SetConnInAppBilling();
                    BillingPayment.InitInAppBilling(Price, "membership", PayId);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
                } 
                else if (text == GetString(Resource.String.Lbl_RazorPay))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id.ToString();

                    InitRazorPay?.BtnRazorPayOnClick(Price, "membership", PayId);
                }
                else if (text == GetString(Resource.String.Lbl_PayStack))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id.ToString();

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(Resource.String.Lbl_PayStack);
                    dialog.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                    {
                        try
                        {
                            if (s.Length <= 0) return;
                            var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await PayStack(s.ToString());
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.InputType(InputTypes.TextVariationEmailAddress);
                    dialog.PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
                else if (text == GetString(Resource.String.Lbl_CashFree))
                {
                    OpenCashFreeDialog();
                }
                else if (text == GetString(Resource.String.Lbl_PaySera))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                    await PaySera();
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
  
        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", ItemUpgrade.Id.ToString());
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", ItemUpgrade.Id.ToString());
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
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
                        var type = PayId switch
                        {
                            "1" => "week",
                            "2" => "month",
                            "3" => "year",
                            "4" => "life-time",
                            _ => ""
                        };
                        var keyValues = new Dictionary<string, string>
                        {
                            {"type", type}, //week,year,month,life-time 
                        };

                        var (apiStatus, respond) = await RequestsAsync.Global.RazorPay(p1.PaymentId, "upgrade", keyValues).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.IsPro = "1";

                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                        
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long)?.Show();
                                    FinishPage();
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
         
        private async Task PayStack(string email)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {  
                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                    };

                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };
                     
                    var (apiStatus, respond) = await RequestsAsync.Global.InitializePayStack(type, keyValues);
                    if (apiStatus == 200)
                    {
                        if (respond is InitializePaymentObject result)
                        {
                            var priceInt = Convert.ToInt32(Price) * 100;

                            PayStackPayment ??= new InitPayStackPayment(this);
                            PayStackPayment.DisplayPayStackPayment(result.Url, "membership", priceInt.ToString(), PayId);
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light)
                    .Title(GetText(Resource.String.Lbl_CashFree))
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                                return;
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    })
                    .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog())
                    .Build();

                var iconName = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = dialog.CustomView.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = dialog.CustomView.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = dialog.CustomView.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var local = ListUtils.MyProfileList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = WoWonderTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(string name, string email, string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };
                      
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email}, 
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.InitializeCashFree(type, AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "", ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    if (apiStatus == 200)
                    {
                        if (respond is CashFreeObject result)
                        {
                            CashFreePayment ??= new InitCashFreePayment(this);
                            CashFreePayment.DisplayCashFreePayment(result, "membership", Price, PayId);
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PaySera()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var type = PayId switch
                    {
                        "1" => "week",
                        "2" => "month",
                        "3" => "year",
                        "4" => "life-time",
                        _ => ""
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.InitializePaySera(type, new Dictionary<string, string>());
                    if (apiStatus == 200)
                    {
                        if (respond is InitializePaymentObject result)
                        {
                            PaySeraPayment ??= new InitPaySeraPayment(this);
                            PaySeraPayment.DisplayPaySeraPayment(result.Url, "membership", Price, PayId);
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        private void FinishPage()
        {
            try
            {
                switch (Caller)
                {
                    case "register" when AppSettings.ShowSuggestedUsersOnRegister:
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent?.PutExtra("class", "register");
                        StartActivity(newIntent);
                        break;
                    }
                    case "register":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                    case "login" when AppSettings.ShowWalkTroutPage:
                    {
                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                        newIntent?.PutExtra("class", "login");
                        StartActivity(newIntent);
                        break;
                    }
                    case "login":
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                        break;
                }

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private async Task SetProAsync()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.SetProAsync(PayId).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.IsPro = "1";

                                    var sqlEntity = new SqLiteDatabase();
                                    sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Upgraded), ToastLength.Long)?.Show();
                                FinishPage();
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}