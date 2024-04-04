using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FlightLib
{
    internal class MemoryMappedTelemetryItem : ITelemetryItem
    {
        string _value;
        public string Value { get => _value; set { _value = value; ForwardData(value); } }
        public string Id { get; set; }

        readonly MemoryMappedViewAccessor accessor;

        #region unimplemented properties
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string System { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string TranslatedValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Units { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AlertOnChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dispatcher Dispatcher => throw new NotImplementedException();
        public Dictionary<string, string> StateNames { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> RawTelemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        internal MemoryMappedTelemetryItem(MemoryMappedViewAccessor accessor)
            => this.accessor = accessor;

        internal void ForwardData(string value)
        {
            if (!Id.StartsWith("TIME")) Console.WriteLine($"{Id}\t{value}");
            accessor.Write(0, float.Parse(value));
        }
    }
}
