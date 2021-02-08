﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Com.Razorpay;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Activities.Base;
using WoWonder.Activities.Fundings.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonder.PaymentGoogle;
using WoWonderClient;
using WoWonderClient.Classes.Funding;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Fundings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class FundingViewActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, IPaymentResultWithDataListener
    {
        #region Variables Basic

        private ImageView ImageUser, ImageFunding, IconBack;
        private TextView TxtMore, TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtFundRaise, TxtFundAmount, TxtDonation;
        private ProgressBar ProgressBar;
        private Button BtnDonate, BtnShare, BtnContact;
        private LinearLayout RecentDonationsLayout;
        private RecyclerView MRecycler;
        private RecentDonationAdapter MAdapter;
        private LinearLayoutManager LayoutManager;

        private FundingDataObject DataObject;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;
        private InitRazorPayPayment InitRazorPay;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitPaySeraPayment PaySeraPayment;
        private string DialogType = "";
        private string CodeName;
        private static FundingViewActivity Instance;

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
                SetContentView(Resource.Layout.FundingViewLayout);
                 
                Instance = this;
                
                //Get Value And Set Toolbar
                InitBuy();
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                LoadData();
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
                    Finish();
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
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageFunding = FindViewById<ImageView>(Resource.Id.imageFunding);
                IconBack = FindViewById<ImageView>(Resource.Id.iv_back);

                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtFundRaise = FindViewById<TextView>(Resource.Id.raised);
                TxtFundAmount = FindViewById<TextView>(Resource.Id.TottalAmount);
                TxtDonation = FindViewById<TextView>(Resource.Id.timedonation);
                BtnDonate = FindViewById<Button>(Resource.Id.DonateButton);
                BtnShare = FindViewById<Button>(Resource.Id.share);
                BtnContact = FindViewById<Button>(Resource.Id.cont);

                RecentDonationsLayout = FindViewById<LinearLayout>(Resource.Id.layout_recent_donations);
                RecentDonationsLayout.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);

                TxtMore = FindViewById<TextView>(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMore, IonIconsFonts.More);
                if (TxtMore != null)
                {
                    TxtMore.SetTextSize(ComplexUnitType.Sp, 20f);
                    TxtMore.Visibility = ViewStates.Gone;
                }
                 
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

                var font = Typeface.CreateFromAsset(Application.Context.Resources?.Assets, "ionicons.ttf");
                TxtDonation.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    IconBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);


                if (!AppSettings.MessengerIntegration)
                    BtnContact.Visibility = ViewStates.Gone;

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
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new RecentDonationAdapter(this)
                {
                    UserList = new ObservableCollection<RecentDonation>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter); 
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
                    TxtMore.Click += TxtMoreOnClick;
                    BtnDonate.Click += BtnDonateOnClick;
                    IconBack.Click += IconBackOnClick;
                    BtnShare.Click += BtnShareOnClick;
                    BtnContact.Click += BtnContactOnClick;
                    TxtTime.Click += UserImageAvatarOnClick;
                    TxtUsername.Click += UserImageAvatarOnClick;
                    ImageUser.Click += UserImageAvatarOnClick;
                }
                else
                {
                    TxtMore.Click -= TxtMoreOnClick;
                    BtnDonate.Click -= BtnDonateOnClick;
                    IconBack.Click -= IconBackOnClick;
                    BtnShare.Click -= BtnShareOnClick;
                    BtnContact.Click -= BtnContactOnClick;
                    TxtTime.Click -= UserImageAvatarOnClick;
                    TxtUsername.Click -= UserImageAvatarOnClick;
                    ImageUser.Click -= UserImageAvatarOnClick;
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
                ImageUser = null!;
                ImageFunding = null!;
                IconBack = null!;
                TxtUsername = null!;
                TxtTime = null!;
                TxtTitle = null!;
                TxtDescription = null!;
                TxtFundRaise = null!;
                TxtFundAmount = null!;
                TxtDonation = null!;
                BtnDonate = null!;
                BtnShare = null!;
                BtnContact = null!;
                ProgressBar = null!;
                TxtMore = null!;
                InitRazorPay = null!;
                PayStackPayment = null!;
                CashFreePayment = null!;
                PaySeraPayment = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static FundingViewActivity GetInstance()
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
        #endregion

        #region Events

        private void UserImageAvatarOnClick(object sender, EventArgs e)
        {
            try
            {
                WoWonderTools.OpenProfile(this, DataObject.UserData.UserId, DataObject.UserData);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Contact User
        private void BtnContactOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.MessengerIntegration)
                {
                    if (AppSettings.ShowDialogAskOpenMessenger)
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        dialog.Title(Resource.String.Lbl_Warning);
                        dialog.Content(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                    else
                    {
                        Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject { UserId = DataObject.UserData.UserId, Name = DataObject.UserData.Name, Avatar = DataObject.UserData.Avatar });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ShareEvent();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //BAck
        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "More";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Copy));

                bool owner = DataObject.UserId == UserDetails.UserId;
                if (owner)
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Delete));
                }
                
                dialogList.Title(GetText(Resource.String.Lbl_More));
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

        //Event Menu >> Edit
        private void EditEvent()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditFundingActivity));
                intent.PutExtra("FundingObject", JsonConvert.SerializeObject(DataObject));
                StartActivityForResult(intent, 253);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                Methods.CopyToClipboard(this, Client.WebsiteUrl + "/show_fund/" + DataObject.HashedId);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = DataObject.Title,
                    Text = DataObject.Description,
                    Url = Client.WebsiteUrl + "/show_fund/" + DataObject.HashedId
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //open Payment
        private void BtnDonateOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "Donate";

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                dialog.Title(Resource.String.Lbl_Donate);
                dialog.Input(Resource.String.Lbl_DonateCode, 0, false, (materialDialog, s) =>
                {
                    try
                    {
                        if (s.Length <= 0) return;
                        CodeName = s.ToString();

                        if (Convert.ToDouble(CodeName) > Convert.ToDouble(DataObject.Amount))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CantDonate) + " " + TxtFundAmount.Text, ToastLength.Long)?.Show();
                            return;
                        }
                         
                        DialogType = "Payment";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (AppSettings.ShowInAppBilling && Client.IsExtended && Convert.ToInt64(CodeName) <= 100)
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
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                dialog.InputType(InputTypes.ClassText);
                dialog.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show(); 
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

                if (requestCode == 253 && resultCode == Result.Ok)
                {
                    if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                    var item = JsonConvert.DeserializeObject<FundingDataObject>(data.GetStringExtra("itemData") ?? "");
                    if (item != null)
                    {
                        DataObject = item;

                        TxtUsername.Text = Methods.FunString.DecodeString(item.UserData.Name);

                        TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(Convert.ToInt32(item.Time), true);

                        TxtTitle.Text = Methods.FunString.DecodeString(item.Title);
                        TxtDescription.Text = Methods.FunString.DecodeString(item.Description);

                        ProgressBar.Progress = Convert.ToInt32(item.Bar);

                        //$0 Raised of $1000000
                        TxtFundRaise.Text = "$" + item.Raised.ToString(CultureInfo.InvariantCulture) + " " + GetString(Resource.String.Lbl_RaisedOf) + " " + "$" + item.Amount;
                    }
                }
                else if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
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

                                await FundingPay();
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
                        await FundingPay();
                        break;
                    case 2654 when resultCode == Result.Ok:
                        StartApiService();
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
                    InitPayPalPayment.BtnPaypalOnClick(CodeName, "Funding");
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    BillingPayment.SetConnInAppBilling();
                    BillingPayment.InitInAppBilling(CodeName, "Funding", "");
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
                    InitRazorPay?.BtnRazorPayOnClick(CodeName, "Funding" , "");
                }
                else if (text == GetString(Resource.String.Lbl_PayStack)) 
                {
                    DialogType = "PayStack";
                     
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
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Edit))
                {
                    EditEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Copy))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(GetText(Resource.String.Lbl_DeleteFunding));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            // Send Api delete  
                            if (Methods.CheckConnectivity())
                            {
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Funding.DeleteFunding(DataObject.Id) });

                                var instance = FundingActivity.GetInstance();
                                var dataFunding = instance?.FundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataFunding != null)
                                {
                                    instance?.FundingTab?.MAdapter?.FundingList.Remove(dataFunding);
                                    instance.FundingTab?.MAdapter?.NotifyItemRemoved(instance.FundingTab.MAdapter.FundingList.IndexOf(dataFunding));
                                }

                                var dataMyFunding = instance?.MyFundingTab?.MAdapter?.FundingList?.FirstOrDefault(a => a.Id == DataObject.Id);
                                if (dataMyFunding != null)
                                {
                                    instance?.MyFundingTab?.MAdapter?.FundingList.Remove(dataMyFunding);
                                    instance.MyFundingTab?.MAdapter?.NotifyItemRemoved(instance.MyFundingTab.MAdapter.FundingList.IndexOf(dataMyFunding));
                                }
                                 
                                var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                                var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal2 != null)
                                {
                                    foreach (var postData in dataGlobal2)
                                    {
                                        recycler.RemoveByRowIndex(postData);
                                    }
                                }

                                var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var diff = adapterGlobal?.ListDiffer;
                                var dataGlobal = diff?.Where(a => a.PostData?.FundId == DataObject.Id).ToList();
                                if (dataGlobal != null)
                                {
                                    foreach (var postData in dataGlobal)
                                    {
                                        WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                    }
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short)?.Show();
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
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
                if (DialogType != "Delete")
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
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
                intent.PutExtra("Id", DataObject.Id);
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
                StartActivityForResult(intent, 2654);
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
                intent.PutExtra("Id", DataObject.Id);
                intent.PutExtra("Price", CodeName);
                intent.PutExtra("payType", "Funding");
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
                        var keyValues = new Dictionary<string, string>
                        {
                            {"merchant_amount", CodeName},
                            {"fund_id", DataObject.Id}
                        };

                        var (apiStatus, respond) = await RequestsAsync.Global.RazorPay(p1.PaymentId, "fund", keyValues);
                        if (apiStatus == 200)
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long)?.Show();
                            StartApiService();
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
                    var priceInt = Convert.ToInt32(CodeName) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"amount", priceInt.ToString()},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.InitializePayStack("fund", keyValues);
                    if (apiStatus == 200)
                    {
                        if (respond is InitializePaymentObject result)
                        {
                            PayStackPayment ??= new InitPayStackPayment(this);
                            PayStackPayment.DisplayPayStackPayment(result.Url, "Funding", priceInt.ToString(), DataObject.Id);
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
                    var keyValues = new Dictionary<string, string>
                    {
                        {"amount", CodeName},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.InitializePaySera("fund", keyValues);
                    if (apiStatus == 200)
                    {
                        if (respond is InitializePaymentObject result)
                        {
                            PaySeraPayment ??= new InitPaySeraPayment(this);
                            PaySeraPayment.DisplayPaySeraPayment(result.Url, "Funding", CodeName, DataObject.Id);
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

                            await CashFree(TxtName.Text , TxtEmail.Text , TxtPhone.Text);
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

        private async Task CashFree(string name, string email , string phone)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                        {"amount", CodeName},
                        {"fund_id", DataObject.Id},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.InitializeCashFree("fund", AppSettings.CashFreeCurrency, ListUtils.SettingsSiteList?.CashfreeSecretKey ?? "" , ListUtils.SettingsSiteList?.CashfreeMode, keyValues);
                    if (apiStatus == 200)
                    {
                        if (respond is CashFreeObject result)
                        {
                            CashFreePayment ??= new InitCashFreePayment(this);
                            CashFreePayment.DisplayCashFreePayment(result, "Funding", CodeName, DataObject.Id);
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

        private void GetDataFunding(FundingDataObject dataObject)
        {
            try
            { 
                if (dataObject != null)
                { 
                    GlideImageLoader.LoadImage(this, dataObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable); 
                    GlideImageLoader.LoadImage(this, dataObject.Image, ImageFunding, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtUsername.Text = WoWonderTools.GetNameFinal(dataObject.UserData);

                    bool success = int.TryParse(dataObject.Time, out var number);
                    if (success)
                    {
                        Console.WriteLine("Converted '{0}' to {1}.", dataObject.Time, number);
                        TxtTime.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(number, true);
                        TxtDonation.Text = IonIconsFonts.Time + "  " + Methods.Time.TimeAgo(number, false);
                    }
                    else
                    {
                        Console.WriteLine("Attempted conversion of '{0}' failed.", dataObject.Time ?? "<null>");
                        TxtTime.Text = Methods.Time.ReplaceTime(dataObject.Time);
                        TxtDonation.Text = IonIconsFonts.Time + "  " + dataObject.Time;
                    }
                     
                    TxtTitle.Text = Methods.FunString.DecodeString(dataObject.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(dataObject.Description);
                    
                    TxtMore.Visibility = ViewStates.Visible;

                    try
                    {
                        dataObject.Raised = dataObject.Raised.Replace(AppSettings.CurrencyFundingPriceStatic, "");
                        dataObject.Amount = dataObject.Amount.Replace(AppSettings.CurrencyFundingPriceStatic, "");

                        decimal d = decimal.Parse(dataObject.Raised, CultureInfo.InvariantCulture);
                        TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + d.ToString("0.00");

                        decimal amount = decimal.Parse(dataObject.Amount, CultureInfo.InvariantCulture);
                        TxtFundAmount.Text = AppSettings.CurrencyFundingPriceStatic + amount.ToString("0.00");
                    }
                    catch (Exception exception)
                    {
                        TxtFundRaise.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Raised;
                        TxtFundAmount.Text = AppSettings.CurrencyFundingPriceStatic + dataObject.Amount;
                        Methods.DisplayReportResultTrack(exception);
                    }
                      
                    BtnContact.Visibility = dataObject.UserData.UserId == UserDetails.UserId ? ViewStates.Gone  : ViewStates.Visible;

                    ProgressBar.Progress = Convert.ToInt32(dataObject.Bar);

                    if (dataObject.IsDonate != null && dataObject.IsDonate.Value == 1)
                    {
                        BtnDonate.Visibility = ViewStates.Gone;
                    }

                    if (dataObject.RecentDonations?.Count > 0)
                    {
                        MAdapter.UserList = new ObservableCollection<RecentDonation>(dataObject.RecentDonations);
                        MAdapter.NotifyDataSetChanged();

                        RecentDonationsLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        RecentDonationsLayout.Visibility = ViewStates.Gone; 
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private async Task FundingPay()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Funding.FundingPay(DataObject.Id, CodeName);
                    if (apiStatus == 200)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long)?.Show(); 
                        StartApiService();
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

        private void LoadData()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent?.GetStringExtra("ItemObject") ?? "");
                if (DataObject != null)
                { 
                    GetDataFunding(DataObject);
                }
                 
                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
         
        public void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFundingById });
        }
         
        private async Task GetFundingById()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Funding.GetFundingById(DataObject.Id);
                    if (apiStatus == 200)
                    { 
                        if (respond is GetFundingByIdObject result)
                        {
                            GetDataFunding(result.Data);
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
    }
}