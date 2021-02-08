using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;  
using Android.Views;
using AndroidX.Core.Content;
using MeoNavLib.Com;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;

namespace WoWonder.Helpers.Utils
{
    public class CustomNavigationController : Java.Lang.Object , MeowBottomNavigation.IClickListener, MeowBottomNavigation.IReselectListener
    {
        private readonly Activity MainContext;
        public int PageNumber;
        private static int OpenNewsFeedTab = 1;

        private readonly TabbedMainActivity Context;
        private readonly MeowBottomNavigation NavigationTabBar;
        private List<MeowBottomNavigation.Model> Models;

        public CustomNavigationController(Activity activity , MeowBottomNavigation bottomNavigation)
        {
            MainContext = activity;
            NavigationTabBar = bottomNavigation;

            if (activity is TabbedMainActivity cont)
                Context = cont;
            
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Models = new List<MeowBottomNavigation.Model>
                {
                    new MeowBottomNavigation.Model(0, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_home_vector)),
                    new MeowBottomNavigation.Model(1, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_notification_vector)),
                };

                if (AppSettings.ShowTrendingPage)
                    Models.Add(new MeowBottomNavigation.Model(2, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_fire_vector)));

                Models.Add(new MeowBottomNavigation.Model(3, ContextCompat.GetDrawable(MainContext, Resource.Drawable.ic_menu)));
                 
                NavigationTabBar.AddModel(Models);

                NavigationTabBar.SetDefaultIconColor(Color.ParseColor("#bfbfbf"));
                NavigationTabBar.SetSelectedIconColor(Color.ParseColor(AppSettings.MainColor));

                NavigationTabBar.SetBackgroundBottomColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);
                NavigationTabBar.SetCircleColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);

                NavigationTabBar.SetCountTextColor(Color.White);
                NavigationTabBar.SetCountBackgroundColor(Color.ParseColor(AppSettings.MainColor));

                NavigationTabBar.SetOnClickMenuListener(this);
                NavigationTabBar.SetOnReselectListener(this); 
            } 
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClickItem(MeowBottomNavigation.Model item)
        {
            try
            {
                if (!item.GetCount().Equals("0") || !item.GetCount().Equals("empty"))
                {
                    NavigationTabBar.SetCount(item.GetId(), "empty"); 
                }

                PageNumber = item.GetId();
                
                if (PageNumber >= 0)
                {
                    switch (PageNumber)
                    {
                        // News_Feed_Tab
                        case 0:
                        {
                            if (AppSettings.ShowAddPostOnNewsFeed && Context.FloatingActionButton.Visibility == ViewStates.Invisible)
                                Context.FloatingActionButton.Visibility = ViewStates.Visible;

                            AdsGoogle.Ad_AppOpenManager(MainContext);
                            break;
                        }
                        // Notifications_Tab
                        case 1:
                        {
                            if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                                Context.FloatingActionButton.Visibility = ViewStates.Gone;

                            AdsGoogle.Ad_RewardedVideo(MainContext);
                            break;
                        }
                        // Trending_Tab
                        case 2 when AppSettings.ShowTrendingPage:
                        {
                            if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                                Context.FloatingActionButton.Visibility = ViewStates.Gone;

                            AdsGoogle.Ad_Interstitial(MainContext);

                            if (AppSettings.ShowLastActivities)
                                Task.Factory.StartNew(() => { Context.TrendingTab.StartApiService(); });

                            Context.InAppReview();
                            break;
                        }
                        // More_Tab
                        case 3:
                        {
                            if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                                Context.FloatingActionButton.Visibility = ViewStates.Gone;

                            AdsGoogle.Ad_RewardedVideo(MainContext);
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(MainContext) });
                            break;
                        }
                    }
                }

                if (Context.ViewPager.CurrentItem != PageNumber)
                    Context.ViewPager.SetCurrentItem(PageNumber, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnReselectItem(MeowBottomNavigation.Model item)
        {
            try
            {
                var p = item.GetId();

                switch (p)
                {
                    case < 0:
                        return;
                    // News_Feed_Tab
                    case 0 when OpenNewsFeedTab == 2:
                        OpenNewsFeedTab = 1;
                        Context.NewsFeedTab.MainRecyclerView.ScrollToPosition(0);
                        break;
                    case 0:
                        OpenNewsFeedTab++;
                        break;
                    // Notifications_Tab
                    case 1:
                        Context.NewsFeedTab?.MainRecyclerView?.StopVideo();
                        OpenNewsFeedTab = 1;
                        break;
                    // Trending_Tab
                    case 2 when AppSettings.ShowTrendingPage:
                        Context.NewsFeedTab?.MainRecyclerView?.StopVideo();
                        OpenNewsFeedTab = 1;
                        break;
                    // More_Tab
                    case 3:
                        Context.NewsFeedTab?.MainRecyclerView?.StopVideo();
                        OpenNewsFeedTab = 1;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowBadge(int id , string count, bool showBadge)
        {
            try
            {
                if (id < 0) return;

                if (showBadge)
                {
                    switch (id)
                    {
                        // News_Feed_Tab
                        case 0:
                            NavigationTabBar.SetCount(0, count);
                            break;
                        // Notifications_Tab
                        case 1:
                            NavigationTabBar.SetCount(1, count);
                            break;
                        // Trending_Tab
                        case 2:
                            NavigationTabBar.SetCount(2, count);
                            break;
                        // More_Tab
                        case 3:
                            NavigationTabBar.SetCount(3, count);
                            break;
                    }
                }
                else
                {
                    switch (id)
                    {
                        // News_Feed_Tab
                        case 0:
                            NavigationTabBar.SetCount(0, "empty");
                            break;
                        // Notifications_Tab
                        case 1:
                            NavigationTabBar.SetCount(1, "empty");
                            break;
                        // Trending_Tab
                        case 2:
                            NavigationTabBar.SetCount(2, "empty");
                            break;
                        // More_Tab
                        case 3:
                            NavigationTabBar.SetCount(3, "empty");
                            break;
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