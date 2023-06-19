using FlightLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatLon
{
    internal class TI : ITelemetryItem
    {
        internal TI(string[] data)
        {
            Id = data[0];
            System = data[1];
        }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Id { get; set; }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string System { get; set; }
        public string TranslatedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Units { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AlertOnChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> StateNames { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> RawTelemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
