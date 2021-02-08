using System.Collections.Generic;

namespace WoWonder.Activities.Live.Stats
{
    public class StatsManager
    {
        private List<int> MUidList = new List<int>();
        private Dictionary<int, StatsData> MDataMap = new Dictionary<int, StatsData>();
        private bool MEnable = false;

        public void AddUserStats(int uid, bool ifLocal)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                return;
            }

            var data = ifLocal
                ? (StatsData) new LocalStatsData()
                : new RemoteStatsData();
            // in case 32-bit unsigned integer uid is received
            data.SetUid(uid & 0xFFFFFFFFL);

            if (ifLocal) MUidList.Add(uid);
            else MUidList.Add(uid);

            MDataMap.Add(uid, data);
        }

        public void RemoveUserStats(int uid)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                MUidList.Remove(uid);
                MDataMap.Remove(uid);
            }
        }

        public StatsData GetStatsData(int uid)
        {
            if (MUidList.Contains(uid) && MDataMap.ContainsKey(uid))
            {
                return MDataMap[uid];
            }
            else
            {
                return null;
            }
        }

        public string QualityToString(int quality)
        {
            switch (quality)
            {
                case DT.Xamarin.Agora.Constants.QualityExcellent:
                    return "Exc";
                case DT.Xamarin.Agora.Constants.QualityGood:
                    return "Good";
                case DT.Xamarin.Agora.Constants.QualityPoor:
                    return "Poor";
                case DT.Xamarin.Agora.Constants.QualityBad:
                    return "Bad";
                case DT.Xamarin.Agora.Constants.QualityVbad:
                    return "VBad";
                case DT.Xamarin.Agora.Constants.QualityDown:
                    return "Down";
                default:
                    return "Unk";
            }
        }

        public void EnableStats(bool enabled)
        {
            MEnable = enabled;
        }

        public bool IsEnabled()
        {
            return MEnable;
        }

        public void ClearAllData()
        {
            MUidList.Clear();
            MDataMap.Clear();
        }
    }

}