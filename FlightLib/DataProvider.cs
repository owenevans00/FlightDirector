using com.lightstreamer.client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace FlightLib
{
    public class DataProvider : SubscriptionListener, ClientListener
    {
        private readonly Subscription subData, subStatus;
        private readonly LightstreamerClient client;
        private readonly Dictionary<string, ITelemetryItem> telemetry;
        private readonly List<ITelemetryItem> customTelemetry = new();

        public event EventHandler<UpdateEventArgs> ValueUpdated;

        public IEnumerable<ITelemetryItem> Items
        {
            get { return telemetry.Values.Select(t => t); }
        }

        public string Status { get; private set; }

        public ITelemetryItem this[string id] { get { return telemetry[id]; } }

        public DataProvider(Func<string[],int, ITelemetryItem> factory, string[] filters = null)
        {
            telemetry = GetTelemetryItems(factory).Where(t => filters == null || filters.Contains(t.System))
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
                ) { RequestedMaxFrequency = "5" };
            client.subscribe(subData);
            subData.addListener(this);

            InitCustomTelemetry();
        }

        private void InitCustomTelemetry()
        {
            customTelemetry.Add(new CustomTelemetry(
                ".STATUS", 
                "USLAB000ALT",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                (a, b) => (b == "USLAB000032")
                          ? ($"{MathF.Sqrt(a.Select(i => MathF.Pow(i, 2)).Sum()) - 6385}", true)
                          : ("", false),
                this));
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
