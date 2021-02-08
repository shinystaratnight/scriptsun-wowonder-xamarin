using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.ObjectModel;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Tabbes.Adapters
{
    public class WeatherAdapter : RecyclerView.Adapter
    {
        private readonly Activity ActivityContext;

        public ObservableCollection<HourObject> WeatherHourList = new ObservableCollection<HourObject>();

        public WeatherAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int ItemCount => WeatherHourList?.Count ?? 0;

        public event EventHandler<WeatherAdapterClickEventArgs> ItemClick;
        public event EventHandler<WeatherAdapterClickEventArgs> ItemLongClick;


        // Create new views (invoked by the layout manager) 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_WeatherHourView
                var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_WeatherHourView, parent, false);
                var vh = new WeatherAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is WeatherAdapterViewHolder holder)
                {
                    var item = WeatherHourList[position];
                    if (item != null)
                    {
                        if (!item.Condition.Icon.Contains("http"))
                            item.Condition.Icon = "http://" + item.Condition.Icon;

                        Glide.With(ActivityContext).Load(item.Condition.Icon).Apply(new RequestOptions()).Into(holder.Icon);
                         
                        holder.Temp.Text = Methods.Time.TimeAgo(item.TimeEpoch);
                        holder.Time.Text = item.TempC + "°";
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                Console.WriteLine(ActivityContext);
            }
        }

        public HourObject GetItem(int position)
        {
            return WeatherHourList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void Click(WeatherAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(WeatherAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class WeatherAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; }

        public TextView Time { get; private set; }
        public ImageView Icon { get; private set; }
        public TextView Temp { get; private set; }

        #endregion

        public WeatherAdapterViewHolder(View itemView, Action<WeatherAdapterClickEventArgs> clickListener, Action<WeatherAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Time = MainView.FindViewById<TextView>(Resource.Id.time);
                Icon = MainView.FindViewById<ImageView>(Resource.Id.Icon);
                Temp = MainView.FindViewById<TextView>(Resource.Id.temp);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new WeatherAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new WeatherAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }

    public class WeatherAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}