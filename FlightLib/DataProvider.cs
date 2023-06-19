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
        private Dictionary<string, ITelemetryItem> telemetry;

        public event EventHandler<UpdateEventArgs> ValueUpdated;

        public IEnumerable<ITelemetryItem> Items
        {
            get { return telemetry.Values.Select(t => t); }
        }

        public string Status { get; private set; }

        public ITelemetryItem this[string id] { get { return telemetry[id]; } }

        public DataProvider(Func<string[], ITelemetryItem> factory, string[] filters = null)
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
                )
            { RequestedMaxFrequency = "5" };
            client.subscribe(subData);
            subData.addListener(this);

        }

        public void onItemUpdate(ItemUpdate itemUpdate)
            => ValueUpdated?.Invoke(this, new UpdateEventArgs(itemUpdate));

        public void onStatusChange(string status)
        {
            Debug.WriteLine(status);
        }

        private IEnumerable<ITelemetryItem> GetTelemetryItems(Func<string[], ITelemetryItem> factory)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();
            //ResourceReader r = new(asm.GetManifestResourceStream(names[0]));

            using (UnmanagedMemoryStream ms = asm.GetManifestResourceStream("FlightLib.ISS_Public_Telemetry.txt") as UnmanagedMemoryStream)
            {
                return new StreamReader(ms).ReadLines()
                            .Select(l => l.Split("\t"))
                            .Select(a => factory(a)).ToList();
            }
        }



        #region unused callbacks
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


        public void onListenEnd(Subscription subscription)
        {

        }

        public void onListenStart(Subscription subscription)
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

        public void onListenEnd(LightstreamerClient client)
        {
            throw new NotImplementedException();
        }

        public void onListenStart(LightstreamerClient client)
        {

        }

        public void onServerError(int errorCode, string errorMessage)
        {

        }



        public void onPropertyChange(string property)
        {

        }
        #endregion
    }

    public class UpdateEventArgs : EventArgs
    {
        public string Id { get; private set; }
        public string NewValue { get; private set; }

        public Dictionary<string, string> RawTelemetry { get; private set; }

        internal UpdateEventArgs(ItemUpdate update)
        {
            Id = update.ItemName;
            RawTelemetry = new Dictionary<string, string>(update.Fields);
            NewValue = update.getValue("Value");
            Debug.Assert(RawTelemetry != null);
        }

    }
}
