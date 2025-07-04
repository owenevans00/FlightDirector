using FlightLib;
using System;
using System.ComponentModel;

namespace FlightDirector_WPF
{
    internal abstract class TelemetryLogger
    {
        internal event EventHandler<LogEvent> Log;

        public TelemetryLogger(ITelemetryItem telemetryItem)
        {
            var descr = DependencyPropertyDescriptor.FromProperty(
                TelemetryItemBase.TranslatedValueProperty,
                typeof(TelemetryItemBase));
            descr.AddValueChanged(telemetryItem, ValueChanged);

        }

        internal abstract void ValueChanged(object sender, EventArgs e);
     

        internal void InvokeLog(string logText)
        {
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