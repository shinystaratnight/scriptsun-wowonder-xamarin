using System.Collections.Generic;
using AndroidX.Fragment.App;
using Java.Lang;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using String = Java.Lang.String;
using SupportFragment = AndroidX.Fragment.App.Fragment;

namespace WoWonder.Adapters
{
    public class MainTabAdapter : FragmentStatePagerAdapter
    { 
#pragma warning disable 618
        public MainTabAdapter(FragmentManager fm) : base(fm)
#pragma warning restore 618
        {
            try
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public MainTabAdapter(FragmentManager fm, int behavior) : base(fm, behavior)
        {
            try
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public List<SupportFragment> Fragments { get; set; }
        public List<string> FragmentNames { get; set; }

        public override int Count => Fragments.Count;

        public void AddFragment(SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Add(fragment);
                FragmentNames.Add(name);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ClaerFragment()
        {
            try
            {
                Fragments.Clear();
                FragmentNames.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void RemoveFragment(SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Remove(fragment);
                FragmentNames.Remove(name);
                NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void InsertFragment(int index, SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Insert(index, fragment);
                FragmentNames.Insert(index, name);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override SupportFragment GetItem(int position)
        {
            try
            {
                if (Fragments[position] != null)
                    return Fragments[position];
                return Fragments[0];
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(FragmentNames[position]);
        }

       
    }
}