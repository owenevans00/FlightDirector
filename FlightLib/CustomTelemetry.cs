using FlightLib;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightLib
{
    internal class CustomTelemetry : ITelemetryItem
    {
        readonly DataProvider data;
        readonly List<string> telemetryids;
        readonly Func<IEnumerable<float>, string> calculator;

        internal CustomTelemetry(string system, string id, IEnumerable<string> items,
            Func<IEnumerable<float>, string> calculator, DataProvider data)
        {
            this.System = system;
            this.Id = id;
            telemetryids = items.ToList();
            this.calculator = calculator;
            this.data = data;
            data.ValueUpdated += Data_ValueUpdated;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string System { get; set; }
        public string Units { get; set; }
        public string Value { get; set; }

        #region unused properties
        public string Description
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public bool AlertOnChange
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public Dictionary<string, string> StateNames
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public Dictionary<string, string> RawTelemetry
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public System.Windows.Threading.Dispatcher Dispatcher
            => throw new NotImplementedException();
        public string TranslatedValue
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        #endregion

        private void Data_ValueUpdated(object sender, UpdateEventArgs e)
        {
            if (!telemetryids.Contains(e.Id)) return;
            var datapoints = telemetryids.Select(i => data[i].SafeValue() ?? "0")
                                         .Select(d => float.Parse(d));
            data.OnCustomItemUpdate(new CustomUpdateEventArgs(this.Id, calculator(datapoints)));
        }
    }

    public class CustomUpdateEventArgs : UpdateEventArgs
    {
        public CustomUpdateEventArgs(string Id, string NewValue) : base()
        {
            this.Id = Id;
            this.NewValue = NewValue;
        }
    }
}
