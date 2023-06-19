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
        private readonly Func<string[], ITelemetryItem> factory = a => TelemetryItem.Create(a);
        private readonly List<TelemetryLogger> loggers;

        public ObservableCollection<string> Log { get; private set; }

        public FlightViewModel()
        {
            Log = new ObservableCollection<string>();
            telemetry = new DataProvider(factory);
            telemetry.ValueUpdated += Telemetry_ValueUpdated;
            foreach (var i in telemetry.Items) Add(i);

            // Calculated values for custom overrides.
            Add(new TelemetryCalculator("USLAB000VEL", ".STATUS", "Velocity", "m/s",
                new[] { telemetry["USLAB000035"], telemetry["USLAB000036"], telemetry["USLAB000037"] },
                a => MathF.Sqrt(a.Select(i => MathF.Pow(i.AsFloat(), 2)).Sum()).ToString("0.00")));

            Add(new TelemetryCalculator("USLAB000ALT", ".STATUS", "Altitude", "km",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => (MathF.Sqrt(a.Select(i => MathF.Pow(i.AsFloat(), 2)).Sum()) - 6385).ToString("0.00")));

            Add(new TelemetryCalculator("TIME0000UTC", ".STATUS", "Station Time", "UTC",
                new[] { telemetry["TIME_000001"], telemetry["TIME_000002"] },
                a => string.Format(
                    "{0}-{1:MM-dd HH:mm:ss}",
                    a.SecondOrDefault().AsInt(),
                    new DateTime(1980, 1, 1).AddMilliseconds(a.First().AsDouble())
                )));

            Add(new TelemetryCalculator("USLAB00ULAT", ".ANGLES", "Unformatted Station Latitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => GeoCoords.Latitude(a).ToString("N2")));

            Add(new TelemetryCalculator("USLAB00ULON", ".ANGLES", "Unformatted Station Longitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => GeoCoords.Longitude(a).ToString("N2")));

            Add(new TelemetryCalculator("USLAB000LAT", ".STATUS", "Latitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => GeoCoords.FormattedLatitude(a)));

            Add(new TelemetryCalculator("USLAB000LON", ".STATUS", "Longitude", "",
                new[] { telemetry["USLAB000032"], telemetry["USLAB000033"], telemetry["USLAB000034"] },
                a => GeoCoords.FormattedLongitude(a)));

            Add(new TelemetryCalculator("SIG00000001", ".STATUS", "Telemetry", "", new[] { telemetry["TIME_000001"] },
            a => AOS.SignalState(a), false));

            loggers = this.Where(ti => ti.AlertOnChange)
                .Select(ti => new TelemetryLogger(ti as TelemetryItemBase))
                .ToList();
            foreach (var l in loggers)
                l.Log += L_Log;

            //var opts = new JsonSerializerOptions() { WriteIndented = true };
            //using (var strm = new StreamWriter(@"..\..\..\fvm.json", append: false))
            //    strm.Write( JsonSerializer.Serialize(this.Items.ToList(), opts));
        }

        public ITelemetryItem this[string TelemetryId]
            {
            get { return this.Where(i => i.Id == TelemetryId).SingleOrDefault(); }
            set { throw new NotSupportedException(); }
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
