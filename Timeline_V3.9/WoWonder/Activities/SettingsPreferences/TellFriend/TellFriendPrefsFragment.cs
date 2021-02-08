using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Activities.SettingsPreferences.InviteFriends;
using WoWonder.Activities.Wallet;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;

 

namespace WoWonder.Activities.SettingsPreferences.TellFriend
{
    public class TellFriendPrefsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region  Variables Basic

        private Preference SharePref, MyAffiliatesPref, InviteFriendsPref, MyPointsPref, MyBalancePref, WithdrawalsPref;
        private string InviteSmsText = ""; 
        private readonly Activity ActivityContext;

        #endregion

        #region General

        public TellFriendPrefsFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_TellFriend);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                SharePref = FindPreference("Share_key");
                MyAffiliatesPref = FindPreference("MyAffiliates_key");
                InviteFriendsPref = FindPreference("InviteFriends_key");
                MyPointsPref = FindPreference("MyPoints_key");
                MyBalancePref = FindPreference("MyBalance_key");
                WithdrawalsPref = FindPreference("Withdrawals_key");

                //Delete Preference
                var mCategoryEarnings = (PreferenceCategory)FindPreference("SectionEarnings_key");
                 
                if (!AppSettings.ShowSettingsMyAffiliates)
                    mCategoryEarnings.RemovePreference(MyAffiliatesPref);
                 
                if (!AppSettings.ShowUserPoint)
                    mCategoryEarnings.RemovePreference(MyPointsPref);
                 
                if (!AppSettings.ShowWallet)
                    mCategoryEarnings.RemovePreference(MyBalancePref);

                if (!AppSettings.ShowWithdrawals)
                    mCategoryEarnings.RemovePreference(WithdrawalsPref);


                var mCategoryInvite = (PreferenceCategory)FindPreference("SectionInvite_key"); 
                if (!AppSettings.ShowSettingsShare)
                    mCategoryInvite.RemovePreference(SharePref);

                if (!AppSettings.InvitationSystem)
                    mCategoryInvite.RemovePreference(InviteFriendsPref);

                InviteSmsText = GetText(Resource.String.Lbl_InviteSMSText_1) + " " + AppSettings.ApplicationName + " " +
                                GetText(Resource.String.Lbl_InviteSMSText_2); 
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
                    SharePref.PreferenceClick += SharePref_OnPreferenceClick;
                    MyAffiliatesPref.PreferenceClick += MyAffiliatesPref_OnPreferenceClick;
                    InviteFriendsPref.PreferenceClick += InviteFriendsPrefOnPreferenceClick;
                    MyPointsPref.PreferenceClick += MyPointsPrefOnPreferenceClick;
                    MyBalancePref.PreferenceClick += MyBalancePrefOnPreferenceClick;
                    WithdrawalsPref.PreferenceClick += WithdrawalsPrefOnPreferenceClick;
                }
                else
                {
                    SharePref.PreferenceClick -= SharePref_OnPreferenceClick;
                    MyAffiliatesPref.PreferenceClick -= MyAffiliatesPref_OnPreferenceClick;
                    InviteFriendsPref.PreferenceClick -= InviteFriendsPrefOnPreferenceClick;
                    MyPointsPref.PreferenceClick -= MyPointsPrefOnPreferenceClick;
                    MyBalancePref.PreferenceClick -= MyBalancePrefOnPreferenceClick;
                    WithdrawalsPref.PreferenceClick -= WithdrawalsPrefOnPreferenceClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Withdrawals
        private void WithdrawalsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                ActivityContext.StartActivity(new Intent(ActivityContext, typeof(WithdrawalsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //MyBalance
        private void MyBalancePrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(TabbedWalletActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //MyPoints
        private void MyPointsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(MyPointsActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Share App with your friends using Url This App in Market Google play 
        private async void SharePref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                //Share Plugin same as flame
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = AppSettings.ApplicationName,
                    Text = InviteSmsText,
                    Url = "http://play.google.com/store/apps/details?id=" + ActivityContext.PackageName
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        //My Affiliates
        private void MyAffiliatesPref_OnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(MyAffiliatesActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InviteFriendsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    var intent = new Intent(ActivityContext, typeof(InviteFriendsActivity));
                    ActivityContext.StartActivity(intent);
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (ActivityContext.CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                    {
                        var intent = new Intent(ActivityContext, typeof(InviteFriendsActivity));
                        ActivityContext.StartActivity(intent);
                    }
                    else
                    {
                        new PermissionsController(ActivityContext).RequestPermission(101);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        var intent = new Intent(ActivityContext, typeof(InviteFriendsActivity));
                        ActivityContext.StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
   