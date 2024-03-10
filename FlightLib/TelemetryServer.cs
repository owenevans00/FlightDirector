using FlightLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlightLib
{
    public class TelemetryServer
    {
        readonly ServerRunner sr;
        public TelemetryServer(ServerRunner serverRunner=null)
        {
            sr = serverRunner ?? new NamedPipeServerRunner();
        }
         
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(sr.Run)) { IsBackground = true };
            t.Start();
        }

        public void Stop() {
            sr.Cancel = true;
        }
    }
}
