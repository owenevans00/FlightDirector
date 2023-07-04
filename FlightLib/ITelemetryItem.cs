using com.lightstreamer.client.events;
using System.Collections.Generic;

namespace FlightLib
{
    public interface ITelemetryItem
    {
        string Description { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        string System { get; set; }
        string TranslatedValue { get; set; }
        string Units { get; set; }
        string Value { get; set; }
        bool AlertOnChange { get; set; }
        System.Windows.Threading.Dispatcher Dispatcher { get; }
        Dictionary<string, string> StateNames { get; set; }
        Dictionary<string, string> RawTelemetry { get; set; }
    }
}