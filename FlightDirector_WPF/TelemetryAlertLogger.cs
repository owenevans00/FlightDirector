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
    class TelemetryLogger
    {
        internal event EventHandler<LogEvent> Log;
        public TelemetryLogger(TelemetryItemBase telemetryItem)
        {
            var descr = DependencyPropertyDescriptor.FromProperty(
                TelemetryItemBase.TranslatedValueProperty,
                typeof(TelemetryItemBase));
            descr.AddValueChanged(telemetryItem, ValueChanged);
                
        }

        void ValueChanged(object sender, EventArgs e)
        {
            var ti = sender as TelemetryItemBase;
            var logText = $"{DateTime.UtcNow:HH:mm:ss.ff}\t{ti.Description}\t{ti.TranslatedValue}";
            Log?.Invoke(this, new LogEvent(logText));
        }
    }

    internal class LogEvent
    {
        public string LogText { get; private set; }
        internal LogEvent(string LogText)
        {
            this.LogText = LogText;
        }
    }
}
