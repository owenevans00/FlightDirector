using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightLib
{
    public abstract class ServerRunner
    {
        internal bool Cancel;

        internal DataProvider data;
        internal void Data_ValueUpdated(object sender, UpdateEventArgs e)
        {
            data[e.Id].Value = e.NewValue;
        }

        internal ServerRunner()
        {
            // 
        }

        internal void Init(Func<string[], int, ITelemetryItem> factory, string[] filters =null) {
            data = new DataProvider(factory, filters);
            data.ValueUpdated += Data_ValueUpdated;
        }

        public abstract void Run();
    }
}
