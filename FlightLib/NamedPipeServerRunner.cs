using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace FlightLib
{
    internal class NamedPipeServerRunner : ServerRunner
    {
        NamedPipeServerStream pipe;
        //List<CustomTelemetry> customTelemetries = new();

        

        internal NamedPipeServerRunner():base()
        {
            pipe = new NamedPipeServerStream("telemetry", PipeDirection.Out);
            Init(Factory, new[] { ".ANGLES", ".STATUS" });
        }        

        public override void Run()
        {
            //Console.WriteLine("Waiting for connection");
            pipe.WaitForConnection();
            //Console.WriteLine("connection established");


            //InitCustomTelemetry();

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

        ITelemetryItem Factory(string[] data, int i)
        {
            return new NamedPipeTelemetryItem(pipe) { Id = data[0], System = data[1] };
        }

        //private void InitCustomTelemetry()
        //{
        //    customTelemetries.Add(new CustomTelemetry(".STATUS", "USLAB000ALT",
        //        new[] { "USLAB000032", "USLAB000033", "USLAB000034" },
        //        a => $"{(MathF.Sqrt(a.Select(i => MathF.Pow(i, 2)).Sum()) - 6385)}",
        //        data, pipe));
        //}
    }


}
