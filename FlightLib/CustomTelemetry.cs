using FlightLib;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightLib
{
    internal class CustomTelemetry : TI
    {
        DataProvider data;
        List<string> telemetryids;
        Func<IEnumerable<float>, string> Calculator;

        internal CustomTelemetry(string system, string id, IEnumerable<string> items, Func<IEnumerable<float>, string> Calculator, DataProvider data, NamedPipeServerStream pipe) : base(pipe)
        {
            this.System = system;
            this.Id = id;
            telemetryids = items.ToList();
            this.Calculator = Calculator;
            this.data = data;
            data.ValueUpdated += Data_ValueUpdated;
        }

        private void Data_ValueUpdated(object sender, UpdateEventArgs e)
        {
            if (!telemetryids.Contains(e.Id)) return;
            var datapoints = telemetryids.Select(i => data[i].Value ?? "0")
                                         .Select(d => float.Parse(d));
            ForwardData(Calculator(datapoints));
        }
    }
}
