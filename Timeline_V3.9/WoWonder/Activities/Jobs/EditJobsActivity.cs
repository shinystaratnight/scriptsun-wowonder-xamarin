using System;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Jobs;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Jobs
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditJobsActivity : BaseActivity, View.IOnFocusChangeListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic
      
        private TextView TxtSave;
        private TextView IconTitle, IconLocation, IconSalary, IconSalaryDate, IconJobType, IconDescription, IconCategory;
        private EditText TxtTitle, TxtLocation, TxtMinimum, TxtMaximum, TxtSalaryDate, TxtJobType, TxtDescription, TxtCategory;
        private string TypeDialog, CategoryId, JobTypeId, SalaryDateId;
        private JobInfoObject DataInfoObject;
         
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
                SetContentView(Resource.Layout.EditJobsLayout);

                var dataObject = Intent?.GetStringExtra("JobsObject");
                if (!string.IsNullOrEmpty(dataObject))
                    DataInfoObject = JsonConvert.DeserializeObject<JobInfoObject>(dataObject);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                BindJobPost();
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
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);

                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                
                IconSalary = FindViewById<TextView>(Resource.Id.IconSalary);
                TxtMinimum = FindViewById<EditText>(Resource.Id.MinimumEditText);
                TxtMaximum = FindViewById<EditText>(Resource.Id.MaximumEditText);

                IconSalaryDate = FindViewById<TextView>(Resource.Id.IconSalaryDate);
                TxtSalaryDate = FindViewById<EditText>(Resource.Id.SalaryDateEditText);

                IconJobType = FindViewById<TextView>(Resource.Id.IconJobType);
                TxtJobType = FindViewById<EditText>(Resource.Id.JobTypeEditText);

                IconCategory = FindViewById<TextView>(Resource.Id.IconCategory);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLocation, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconSalary, FontAwesomeIcon.MoneyBillAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconSalaryDate, FontAwesomeIcon.Times);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconJobType, FontAwesomeIcon.Briefcase); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconCategory, FontAwesomeIcon.Buromobelexperte);

                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMinimum, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtMaximum, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtSalaryDate, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtJobType, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtSalaryDate);
                Methods.SetFocusable(TxtJobType);
                Methods.SetFocusable(TxtCategory);
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
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    TxtSave.Click += TxtSaveOnClick;
                    TxtLocation.OnFocusChangeListener = this; 
                    TxtSalaryDate.Touch += TxtSalaryDateOnTouch;
                    TxtJobType.Touch += TxtJobTypeOnTouch;
                    TxtCategory.Touch += TxtCategoryOnTouch;
                }
                else
                {
                    TxtSave.Click -= TxtSaveOnClick;
                    TxtLocation.OnFocusChangeListener = null!; 
                    TxtSalaryDate.Touch -= TxtSalaryDateOnTouch;
                    TxtJobType.Touch -= TxtJobTypeOnTouch;
                    TxtCategory.Touch -= TxtCategoryOnTouch;
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
                TxtSave = null!;
                IconTitle = null!;
                TxtTitle = null!;
                IconLocation = null!;
                TxtLocation = null!;
                IconSalary = null!;
                TxtMinimum = null!;
                TxtMaximum = null!;
                IconSalaryDate = null!;
                TxtSalaryDate = null!;
                IconJobType = null!;
                TxtJobType = null!;
                IconCategory = null!;
                TxtCategory = null!;
                IconDescription = null!;
                TxtDescription = null!;
                TypeDialog = null!;
                CategoryId = null!;
                JobTypeId = null!;
                SalaryDateId = null!;
                DataInfoObject = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                if (CategoriesController.ListCategoriesJob.Count > 0)
                {
                    TypeDialog = "Categories";

                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    var arrayAdapter = CategoriesController.ListCategoriesJob.Select(item => item.CategoriesName).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_SelectCategories));
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Job");
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtJobTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                TypeDialog = "JobType";
                var arrayAdapter = WoWonderTools.GetJobTypeList(this).Select(item => item.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_JobType));
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

        private void TxtSalaryDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                TypeDialog = "SalaryDate";
                 
                var arrayAdapter = WoWonderTools.GetSalaryDateList(this).Select(item => item.Value).ToList(); 

                dialogList.Title(GetText(Resource.String.Lbl_SalaryDate));
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
         
        private void TxtLocationOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    if (string.IsNullOrEmpty(TxtTitle.Text) || string.IsNullOrEmpty(TxtLocation.Text) || string.IsNullOrEmpty(TxtMinimum.Text)
                        || string.IsNullOrEmpty(TxtMaximum.Text) || string.IsNullOrEmpty(TxtSalaryDate.Text) || string.IsNullOrEmpty(TxtJobType.Text)
                        || string.IsNullOrEmpty(TxtDescription.Text) || string.IsNullOrEmpty(TxtCategory.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                        return;
                    }
                      
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                     
                    var (apiStatus, respond) = await RequestsAsync.Jobs.EditJob(DataInfoObject.Id, TxtTitle.Text, TxtDescription.Text, TxtLocation.Text,
                        TxtMinimum.Text, TxtMaximum.Text, SalaryDateId, JobTypeId, CategoryId);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageJobObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            Toast.MakeText(this, GetString(Resource.String.Lbl_jobSuccessfullyEdited), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            JobInfoObject newInfoObject = new JobInfoObject
                            {
                                Title = TxtTitle.Text,
                                Location = TxtLocation.Text,
                                Minimum = TxtMinimum.Text,
                                Maximum = TxtMaximum.Text,
                                SalaryDate = TxtSalaryDate.Text,
                                JobType = TxtJobType.Text,
                                Description = TxtDescription.Text,
                                Category = CategoryId,
                            };
                             
                            JobInfoObject data = JobsActivity.GetInstance()?.MAdapter?.JobList?.FirstOrDefault(a => a.Id == DataInfoObject.Id);
                            if (data != null)
                            {
                                data.Title = TxtTitle.Text;
                                data.Location = TxtLocation.Text;
                                data.Minimum = TxtMinimum.Text;
                                data.Maximum = TxtMaximum.Text; 
                                data.SalaryDate = TxtSalaryDate.Text; 
                                data.JobType = TxtJobType.Text; 
                                data.Description = TxtDescription.Text; 
                                data.Category = CategoryId; 
                                JobsActivity.GetInstance().MAdapter.NotifyItemChanged(JobsActivity.GetInstance().MAdapter.JobList.IndexOf(data));
                            }

                            Intent intent = new Intent();
                            intent.PutExtra("JobsItem", JsonConvert.SerializeObject(newInfoObject));
                            SetResult(Result.Ok, intent);
                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
         
        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 502 && resultCode == Result.Ok)
                    GetPlaceFromPicker(data);
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

                if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Camera when the request code of result is 503
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

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

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Categories":
                        CategoryId = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesName == itemString.ToString())?.CategoriesId;
                        TxtCategory.Text = itemString.ToString();
                        break;
                    case "JobType":
                        JobTypeId = WoWonderTools.GetJobTypeList(this)?.FirstOrDefault(a => a.Value == itemString.ToString()).Key.ToString();
                        TxtJobType.Text = itemString.ToString();
                        break;
                    case "SalaryDate":
                        SalaryDateId = WoWonderTools.GetSalaryDateList(this)?.FirstOrDefault(a => a.Value == itemString.ToString()).Key.ToString();
                        TxtSalaryDate.Text = itemString.ToString();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                if (!string.IsNullOrEmpty(placeAddress))
                    TxtLocation.Text = placeAddress;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BindJobPost()
        {
            try
            {
                if (DataInfoObject != null)
                {
                    DataInfoObject = WoWonderTools.ListFilterJobs(DataInfoObject);

                    TxtTitle.Text = Methods.FunString.DecodeString(DataInfoObject.Title);
                    TxtLocation.Text = DataInfoObject.Location;

                    //Set Description
                    TxtDescription.Text = Methods.FunString.DecodeString(DataInfoObject.Description);

                    TxtMinimum.Text = DataInfoObject.Minimum;
                    TxtMaximum.Text = DataInfoObject.Maximum;

                    //Set Salary Date
                    SalaryDateId = DataInfoObject.SalaryDate; 
                    switch (DataInfoObject.SalaryDate)
                    {
                        case "per_hour":
                            TxtSalaryDate.Text = GetString(Resource.String.Lbl_per_hour);
                            break;
                        case "per_day":
                            TxtSalaryDate.Text = GetString(Resource.String.Lbl_per_day);
                            break;
                        case "per_week":
                            TxtSalaryDate.Text = GetString(Resource.String.Lbl_per_week);
                            break;
                        case "per_month":
                            TxtSalaryDate.Text = GetString(Resource.String.Lbl_per_month);
                            break;
                        case "per_year":
                            TxtSalaryDate.Text = GetString(Resource.String.Lbl_per_year);
                            break; 
                    }
                     
                    //Set job type
                    JobTypeId = DataInfoObject.JobType; 
                    switch (DataInfoObject.JobType)
                    {
                        case "full_time":
                            TxtJobType.Text = GetString(Resource.String.Lbl_full_time);
                            break;
                        case "part_time":
                            TxtJobType.Text = GetString(Resource.String.Lbl_part_time);
                            break;
                        case "internship":
                            TxtJobType.Text = GetString(Resource.String.Lbl_internship);
                            break;
                        case "volunteer":
                            TxtJobType.Text = GetString(Resource.String.Lbl_volunteer);
                            break;
                        case "contract":
                            TxtJobType.Text = GetString(Resource.String.Lbl_contract);
                            break;
                    }

                    CategoryId = DataInfoObject.Category;
                    TxtCategory.Text = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == DataInfoObject.Category)?.CategoriesName;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnFocusChange(View v, bool hasFocus)
        {
            if (v?.Id == TxtLocation.Id && hasFocus)
            {
                TxtLocationOnClick();
            }
        }
    }
}