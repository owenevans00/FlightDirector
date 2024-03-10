using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;

namespace FlightLib
{
    public class MemoryMappedServerRunner : ServerRunner
    {
        const string maybeAppSID = "S-1-15-2-1162135864-640799773-311411905-3086399623-335766872-3597601787-2471990241";
        MemoryMappedFile mfile;
        public MemoryMappedServerRunner() : base()
        {
            try
            {
                mfile = MemoryMappedFile.CreateOrOpen($"AppContainerNamedObjects\\{maybeAppSID}\\telemetry", 350 * 4);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Running in standalone mode");
                mfile = MemoryMappedFile.CreateOrOpen("telemetry", 350 * 4);
            }
            Init(Factory);
        }

        public override void Run()
        {
            while (!Cancel) { Thread.Sleep(500); }
        }

        ITelemetryItem Factory(string[] data, int offset)
            => new MemoryMappedTelemetryItem(mfile.CreateViewAccessor(offset * 4, 4)) { Id = data[0] };
    }
}
