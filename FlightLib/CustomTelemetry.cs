﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightLib
{
    internal class CustomTelemetry : ITelemetryItem
    {
        readonly DataProvider data;
        readonly List<string> telemetryids;
        readonly Func<IEnumerable<float>, string, (string, bool)> calculator;

        internal CustomTelemetry(string id, string system, string name, string description, string units, IEnumerable<string> items,
            Func<IEnumerable<float>, string, (string, bool)> calculator, DataProvider data)
        {
            this.System = system;
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Units = units;
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
        public string Description { get; set; }
        public bool AlertOnChange { get; set; }
        public Dictionary<string, string> StateNames { get; set; }
        public Dictionary<string, string> RawTelemetry { get; set; }

        public System.Windows.Threading.Dispatcher Dispatcher
            => null;

        public string TranslatedValue { get=>Value; set=>throw new NotImplementedException(); }
        #endregion

        private void Data_ValueUpdated(object sender, UpdateEventArgs e)
        {
            if (!telemetryids.Contains(e.Id)) return;
            var datapoints = telemetryids.Select(i => data[i].SafeValue() ?? "0")
                                         .Select(d => float.Parse(d)).ToArray();

            var (rval, send) = calculator(datapoints, e.Id);
            if (send) data.OnCustomItemUpdate(new CustomUpdateEventArgs(this.Id, rval));
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
