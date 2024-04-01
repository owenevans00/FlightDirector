using FlightLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;

namespace FlightDirector_WPF
{
    public class FlightViewModel : ObservableCollection<ITelemetryItem>, INotifyPropertyChanged, IValueConverter
    {
        private readonly DataProvider telemetry;
        private readonly Func<string[],int, ITelemetryItem> factory = (a,i) => TelemetryItem.Create(a);
        private readonly List<TelemetryLogger> loggers;

        private readonly List<string> EVAIds = new() { "TIME0000UTC", "USLAB000011", "USLAB000012", "AIRLOCK000048", "AIRLOCK000054", "AIRLOCK000049", "AIRLOCK000001", "AIRLOCK000003", "AIRLOCK000007", "AIRLOCK000009", "S0000008", "S0000009", "USLAB000095" };
        private readonly List<string> VVOIds = new() { "TIME0000UTC", "USLAB000011", "USLAB000012", "USLAB000016", "USLAB000017", "USLAB000081", "USLAB000095", "USLAB000099", "USLAB000100", "USLAB000101", "S0000008", "S0000009", "S0000006", "S0000007" };

        private readonly MapViewModel mvm = new();

        public ObservableCollection<string> Log { get; private set; }
        public ObservableCollection<ITelemetryItem> EVA { get; private set; }
        public ObservableCollection<ITelemetryItem> VVO { get; private set; }
        public ObservableCollection<ITelemetryItem> Status { get; private set; }
        public ObservableCollection<ITelemetryItem> LSup { get; private set; }
        public ITelemetryItem this[string TelemetryId]
        {
            get { return this.Where(i => i.Id == TelemetryId).SingleOrDefault(); }
            set { throw new NotSupportedException(); }
        }

        public string IssCamUrl = Properties.Settings.Default.ISSCamUrl + "?autoplay=true";// "https://www.youtube.com/embed/jPTD2gnZFUw";
        public string EhdcCamUrl = Properties.Settings.Default.EHDCCamUrl + "?autoplay=true";

        public FlightViewModel()
        {
            Log = new();
            telemetry = new DataProvider(factory);
            telemetry.ValueUpdated += Telemetry_ValueUpdated;
            foreach (var i in telemetry.Items) Add(i);
            InitCustomTelemetry();

            loggers = new(this.Where(ti => ti.AlertOnChange)
                .Select(ti => new TelemetryLogger(ti as TelemetryItemBase)));
            foreach (var l in loggers)
                l.Log += L_Log;

            EVA = new(this.Where(ti => EVAIds.Contains(ti.Id)));
            Status = new(this.Where(ti => ti.System == ".STATUS"));
            VVO = new(this.Where(ti => VVOIds.Contains(ti.Id)));
            LSup = new(this.Where(ti => ti.System == "ETHOS"));
        }

        private void InitCustomTelemetry()
        {
            // Calculated values for custom overrides.
            Add(new TelemetryCalculator("USLAB000VEL", ".STATUS", "Velocity", "m/s",
                new[] { telemetry["USLAB000035"], telemetry["USLAB000036"], telemetry["USLAB000037"] },
                a => MathF.Sqrt(a.Select(i => MathF.Pow(i.AsFloat(), 2)).Sum()).ToString("0.00")));

            Add(new TelemetryCalculator("TIME0000UTC", ".STATUS", "Station Time", "UTC",
                new[] { telemetry["TIME_000001"], telemetry["TIME_000002"] },
                a => string.Format(
                    "{0}-{1:MM-dd HH:mm:ss}",
                    a.SecondOrDefault().AsInt(),
                    new DateTime(1980, 1, 1).AddMilliseconds(a.First().AsDouble())
                )));

             Add(new TelemetryCalculator("USLAB00ULAT", ".ANGLES", "Unformatted Station Latitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => mvm.Latitude.ToString("N2")));

            Add(new TelemetryCalculator("USLAB00ULON", ".ANGLES", "Unformatted Station Longitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => mvm.Longitude.ToString("N2")));

            Add(new TelemetryCalculator("USLAB000LAT", ".STATUS", "Latitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => mvm.FormattedLatitude));

            Add(new TelemetryCalculator("USLAB000LON", ".STATUS", "Longitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => mvm.FormattedLongitude));

            Add(new TelemetryCalculator("USLAB000HDG", ".STATUS", "Heading", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => mvm.Heading.ToString("000")));


            Add(new TelemetryCalculator("SIG00000001", ".STATUS", "Telemetry", "", new[] { telemetry["TIME_000001"] },
            a => AOS.SignalState(a), false));
        }

        private void L_Log(object sender, LogEvent e) => Log.Add(e.LogText);

        private void Telemetry_ValueUpdated(object sender, UpdateEventArgs e)
            => Application.Current?.Dispatcher.BeginInvoke(DoUpdate, e.Id, e.NewValue, e.RawTelemetry);

        private void DoUpdate(string id, string newValue, Dictionary<string, string> RawTelemetry)
        {
            if (this.SingleOrDefault(i => i.Id == id) is ITelemetryItem item)
            {
                item.RawTelemetry = RawTelemetry;
                item.Value = newValue;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Where(i => i.Id == parameter.ToString()).FirstOrDefault();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class NullTelemetry : TelemetryItemBase { }

}
