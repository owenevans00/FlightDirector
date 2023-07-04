using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace PipeReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var p = new PipeReader();
            var t = new Thread(new ThreadStart(p.Start));
            t.Start();
            Console.ReadKey();
        }
    }

    internal class PipeReader
    {
        public Dictionary<string, float> Data = new();
        //public int requestsPerFixedUpdate = 1;
        private NamedPipeClientStream? pipeclient;
        IAsyncResult? requestHandle;
        private byte[] buffer = new byte[18];
        private Timer t;

        internal PipeReader()
        {
            TryConnectPipe();
            Data.Add("TEST", 0F);
            t = new Timer(FixedUpdate, null, Timeout.Infinite, 20);

        }

        private bool TryConnectPipe()
        {
            if (pipeclient is not null && pipeclient.IsConnected) { return true; }
            try
            {
                pipeclient = new NamedPipeClientStream(".", "telemetry", PipeDirection.In);
                pipeclient.Connect();
                Console.WriteLine("Connection Established");
                return true;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                //Console.WriteLineError(e.Message);
                Thread.Sleep(100);
            }
            return false;
        }

        private void FixedUpdate(object? state)
        {
            if (!TryConnectPipe()) return;
            if (requestHandle == null || requestHandle.IsCompleted)
                requestHandle = pipeclient?.BeginRead(buffer, 0, 18, GetMessage, null);
        }

        private void GetMessage(IAsyncResult ar)
        {
            pipeclient?.EndRead(ar);
            string id = Encoding.UTF8.GetString(buffer, 0, 14).Replace("\0", "");
            float val = BitConverter.ToSingle(buffer, 14);

            if (Data.ContainsKey(id))
                Data[id] = val;
            else
            {
                Data.Add(id, val);
                Console.WriteLine($"Added {id}");
                //Console.WriteLine($"Monitoring {Data.Count} telemetry ids");
            }
            Data["TEST"] = Data["TEST"] + 1 % 360;
        }

        internal void Start()
        {
            t.Change(0, 20);
        }
    }
}