using FlightLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly Func<string[], int, ITelemetryItem> factory = (a, i) => TelemetryItem.Create(a);
        private readonly List<TelemetryLogger> loggers;

        private readonly List<string> EVAIds = new() { "TIME0000UTC", "USLAB000011", "USLAB000012", "AIRLOCK000048", "AIRLOCK000054", "AIRLOCK000049", "AIRLOCK000001", "AIRLOCK000003", "AIRLOCK000007", "AIRLOCK000009", "S0000008", "S0000009", "USLAB000095" };
        private readonly List<string> VVOIds = new() { "TIME0000UTC", "USLAB000011", "USLAB000012", "USLAB000016", "USLAB000017", "USLAB000081", "USLAB000095", "USLAB000099", "USLAB000100", "USLAB000101", "S0000008", "S0000009", "S0000006", "S0000007" };

        //private readonly MapViewModel mvm = new();

        public ObservableCollection<string> Log { get; private set; }
        public ObservableCollection<ITelemetryItem> EVA { get; private set; }
        public ObservableCollection<ITelemetryItem> VVO { get; private set; }
        public ObservableCollection<ITelemetryItem> Status { get; private set; }
        public ObservableCollection<ITelemetryItem> LSup { get; private set; }
        public Uri IssCamUrl { get; private set; }
        public Uri EhdcCamUrl { get; private set; }

        internal Process unity;
        public Command launchIIS3D => new Command(StartIIS3D, IIS3DNotRunning);

        public ITelemetryItem this[string TelemetryId]
        {
            get { return this.Where(i => i.Id == TelemetryId).SingleOrDefault(); }
            set { throw new NotSupportedException(); }
        }

        public FlightViewModel()
        {
            Log = new();
            telemetry = new DataProvider(factory, converter: TelemetryItem.Convert);
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

            IssCamUrl = new($"https://www.youtube.com/embed/{Properties.Settings.Default.ISSCamUrl}?autoplay=true");
            EhdcCamUrl = new($"https://www.youtube.com/embed/{Properties.Settings.Default.EHDCCamUrl}?autoplay=true");
        }

        private void InitCustomTelemetry()
        {
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
        private void StartIIS3D(object obj)
        {
            unity = Process.Start(Properties.Settings.Default.ISS3DPath);
        }

        private bool IIS3DNotRunning(object none)
            => unity is null || unity.HasExited;

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
