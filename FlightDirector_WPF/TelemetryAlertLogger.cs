using FlightLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlightDirector_WPF
{
    /// <summary>
    /// Logger for changes to imporetant telemetry values like station mode
    /// or attitude control system state
    /// </summary>
    class TelemetryAlertLogger : TelemetryLogger
    {
        public TelemetryAlertLogger(TelemetryItemBase telemetryItem) : base(telemetryItem) { }

        internal override void ValueChanged(object sender, EventArgs e)
        {
            var ti = sender as TelemetryItemBase;
            var logText = $"{DateTime.UtcNow:HH:mm:ss.ff}\t{ti.Description}\t{ti.TranslatedValue}";
            InvokeLog(logText);
        }
    }

}
