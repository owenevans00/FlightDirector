using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlightLib
{
    public class TelemetryServer
    {
        readonly ServerRunner sr = new();
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(sr.Run)) { IsBackground = true };
            t.Start();
        }

        public void Stop() {
            sr.Cancel = true;
        }
    }

    internal class ServerRunner
    {
        DataProvider data;
        NamedPipeServerStream pipe;
        List<CustomTelemetry> customTelemetries = new();

        internal bool Cancel;

        internal ServerRunner()
        {
            pipe = new NamedPipeServerStream("telemetry", PipeDirection.Out);

        }

        private void Data_ValueUpdated(object sender, UpdateEventArgs e)
        {
            data[e.Id].Value = e.NewValue;
        }

        internal void Run()
        {
            //Console.WriteLine("Waiting for connection");
            pipe.WaitForConnection();
            //Console.WriteLine("connection established");

            data = new DataProvider(Factory, new[] { ".ANGLES", ".STATUS" });
            data.ValueUpdated += Data_ValueUpdated;
            InitCustomTelemetry();

            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (Cancel) break;
                }
            }
            catch (IOException) { }
            pipe.Dispose();
        }

        ITelemetryItem Factory(string[] data)
        {
            return new TI(pipe) { Id = data[0], System = data[1] };
        }

        private void InitCustomTelemetry()
        {
            customTelemetries.Add(new CustomTelemetry(".STATUS", "USLAB000ALT",
                new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
                a => $"{(MathF.Sqrt(a.Select(i => MathF.Pow(i, 2)).Sum()) - 6385)}",
                data, pipe));
        }
    }
}
