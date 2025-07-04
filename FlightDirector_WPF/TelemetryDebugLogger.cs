using FlightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightDirector_WPF
{
    class TelemetryDebugLogger : TelemetryLogger
    {
        internal TelemetryDebugLogger(ITelemetryItem telemetryItem) : base(telemetryItem) { }

        internal override void ValueChanged(object sender, EventArgs e)
        {
            var ti = sender as TelemetryItemBase;
            var logText = $"{DateTime.UtcNow:HH:mm:ss.ff}\t{ti.Id}\t{ti.Value}\t{ ti.TranslatedValue}";
            InvokeLog(logText);
        }
    }
}
