using com.lightstreamer.client;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FlightLib
{
    public class DataProvider : SubscriptionListener, ClientListener
    {
        private readonly Subscription subData, subStatus;
        private readonly LightstreamerClient client;
        private readonly Dictionary<string, ITelemetryItem> telemetry;
        private readonly List<ITelemetryItem> customTelemetry = new();
        private readonly ECIToGeoConverter e2g = new();

        public event EventHandler<UpdateEventArgs> ValueUpdated;

        public IEnumerable<ITelemetryItem> Items
        {
            get { return telemetry.Values.Select(t => t); }
        }

        public string Status { get; private set; }

        public ITelemetryItem this[string id] { get { return telemetry[id]; } }

        public DataProvider(Func<string[], int, ITelemetryItem> factory, string[] filters = null, Func<ITelemetryItem, ITelemetryItem> converter = null)
        {
            InitCustomTelemetry();

            converter ??= new Func<ITelemetryItem, ITelemetryItem>(i => i);
            telemetry = GetTelemetryItems(factory)
                       .Concat(customTelemetry.ConvertAll(i => converter(i)))
                       .Where(t => filters == null || filters.Contains(t.System) || t.Id.StartsWith("TIME"))
                       .ToDictionary(t => t.Id, t => t);

            client = new LightstreamerClient("https://push.lightstreamer.com", "ISSLIVE");
            client.addListener(this);
            client.connect();

            subStatus = new Subscription("MERGE",
                telemetry.Keys.Where(k => k.StartsWith("TIME")).ToArray(),
                new string[] { "Value", "TimeStamp", "Status.Class" }
                );
            client.subscribe(subStatus);
            subStatus.addListener(this);

            subData = new Subscription("MERGE",
                telemetry.Keys.Where(k => !k.StartsWith("TIME")).ToArray(),
                new string[] { "Value", "TimeStamp" }
                )
            { RequestedMaxFrequency = "5" };
            client.subscribe(subData);
            subData.addListener(this);
        }

        // Calculated values for custom overrides.
        private void InitCustomTelemetry()
        {
            customTelemetry.Add(new CustomTelemetry("TIME0000UTC", ".STATUS", "Station Time", "Station Time (UTC)", "UTC",
                new[] { "TIME_000001", "TIME_000002" },
                (a, b) =>
                {
                    var year = (int)a.SecondOrDefault();
                    if (year == 1980 || year == 0) year = DateTime.Now.Year;
                    DateTime d = new DateTime(year, 1, 1);
                    d = d.Add(TimeSpan.FromMilliseconds(a.First()));
                    return ($"{d:yyyy-MM-dd HH:mm:ss}", true);
                },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB000VEL", ".ANGLES", "Velocity", "Velocity", "m/s",
                new[] { "USLAB000035", "USLAB000036", "USLAB000037" },
                (a, b) => (MathF.Sqrt(a.Select(i => MathF.Pow(i, 2)).Sum()).ToString("0.00"), true),
                this));

            customTelemetry.Add(new CustomTelemetry(
                "USLAB000ALT", ".ANGLES", "Station Altitude", "Station Altitude", "km",
                e2g.telemetryIds,
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.Altitude}", true); },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB00ULAT", ".ANGLES", "Unformatted Station Latitude", "Unformatted Station Latitude", "",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.Latitude}", true); },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB00ULON", ".ANGLES", "Unformatted Station Longitude", "Unformatted Station Longitude", "",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.Longitude}", true); },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB000LAT", ".ANGLES", "Latitude", "Latitude", "",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.FormattedLatitude}", true); },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB000LON", ".ANGLES", "Longitude", "Longitude", "",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.FormattedLongitude}", true); },
                this));

            customTelemetry.Add(new CustomTelemetry("USLAB000HDG", ".ANGLES", "Heading", "", "",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => { e2g.TryUpdate(a, b); return ($"{e2g.Heading}", true); },
                this));

            //customTelemetry.Add(new CustomTelemetry("SIG00000001", ".STATUS", "Telemetry", "", 
            //    new[] { "TIME_000001" },
            //    (a,b) => (AOS.SignalState(a), true),
            //    this));
        }

        public void OnCustomItemUpdate(UpdateEventArgs args)
            => ValueUpdated?.Invoke(this, args);

        public void onItemUpdate(ItemUpdate itemUpdate)
            => ValueUpdated?.Invoke(this, new UpdateEventArgs(itemUpdate));

        public void onStatusChange(string status) => Debug.WriteLine(status);

        private static IEnumerable<ITelemetryItem> GetTelemetryItems(Func<string[], int, ITelemetryItem> factory)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();

            using UnmanagedMemoryStream ms = asm.GetManifestResourceStream("FlightLib.ISS_Public_Telemetry.txt") as UnmanagedMemoryStream;
            return new StreamReader(ms).ReadLines()
                        .Select(l => l.Split("\t"))
                        .Select((a, i) => factory(a, i)).ToList();
        }
        #region unused callbacks
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static
        public void onClearSnapshot(string itemName, int itemPos)
        {

        }

        public void onCommandSecondLevelItemLostUpdates(int lostUpdates, string key)
        {

        }

        public void onCommandSecondLevelSubscriptionError(int code, string message, string key)
        {

        }

        public void onEndOfSnapshot(string itemName, int itemPos)
        {

        }

        public void onItemLostUpdates(string itemName, int itemPos, int lostUpdates)
        {

        }

        public void onListenEnd(Subscription _)
        {

        }

        public void onListenStart(Subscription _)
        {

        }

        public void onSubscription()
        {

        }

        public void onSubscriptionError(int code, string message)
        {

        }

        public void onUnsubscription()
        {

        }

        public void onRealMaxFrequency(string frequency)
        {

        }

        public void onListenEnd(LightstreamerClient _)
        {

        }

        public void onListenStart(LightstreamerClient _)
        {

        }

        public void onServerError(int errorCode, string errorMessage)
        {

        }

        public void onPropertyChange(string property)
        {

        }

        public void onListenEnd()
        {

        }

        public void onListenStart()
        {

        }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1822 // Mark members as static
        #endregion
    }

    public class UpdateEventArgs : EventArgs
    {
        public string Id { get; protected set; }
        public string NewValue { get; protected set; }

        public Dictionary<string, string> RawTelemetry { get; protected set; }

        protected UpdateEventArgs()
        {
            RawTelemetry = new();
        }

        internal UpdateEventArgs(ItemUpdate update)
        {
            Id = update.ItemName;
            RawTelemetry = new Dictionary<string, string>(update.Fields);
            NewValue = update.getValue("Value");
            Debug.Assert(RawTelemetry != null);
        }
    }
}
