using FlightLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace FlightLib
{
    internal class TI : ITelemetryItem
    {
        NamedPipeServerStream pipe;

        public string Description { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string System { get; set; }
        public string TranslatedValue { get; set; }
        public string Units { get; set; }
        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; ForwardData(value); }
        }

        public Dictionary<string, string> StateNames { get; set; }
        public bool AlertOnChange { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<string, string> RawTelemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal TI(NamedPipeServerStream pipe)
        {
            this.pipe = pipe;
        }
        internal virtual void ForwardData(string value)
        {
            Console.WriteLine($"{Id} {value}");
            if (!pipe.IsConnected) return;
            var fval = float.Parse(value);
            var buf = new byte[18];
            Encoding.UTF8.GetBytes(Id, 0, Id.Length, buf, 0);
            BitConverter.GetBytes(fval).CopyTo(buf, 14);
            lock (pipe)
            {
                try
                {
                    pipe.Write(buf, 0, buf.Length);
                }
                catch (IOException)
                {
                    //Console.WriteLine("Disconnected: Waiting for new connection");
                    pipe.Disconnect();
                    pipe.WaitForConnectionAsync();
                    //Console.WriteLine("connection established");
                }
            }
        }
    }
}
