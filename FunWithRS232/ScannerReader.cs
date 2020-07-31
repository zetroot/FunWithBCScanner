using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace FunWithRS232
{
    public class ScannerOptions
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; } 
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
    }

    public class ScannerReader
    {
        private readonly ScannerOptions options;

        public ScannerReader(ScannerOptions options)
        {
            this.options = options;
        }

        private readonly List<char> globalBuffer = new List<char>(65536);

        public IObservable<string> Barcodes { get; }

        public async Task ObserveScanner()
        {
            var port = new SerialPort(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits);
            port.Open();

            var stream = port.BaseStream;
            using var sreader = new StreamReader(stream);
            using var owner = MemoryPool<char>.Shared.Rent();
            while (true)
            {
                var buf = new char[4096];

                var readBytes = await sreader.ReadAsync(buf, 0, buf.Length);
                globalBuffer.AddRange(buf.Take(readBytes));
                Console.WriteLine($"Read {readBytes} bytes");
                for (int i = 0; i < readBytes; ++i)
                    Console.WriteLine($"{buf[i]}\t{((byte)buf[i]).ToString("X2")}");
                Console.WriteLine($"now global buffer is {globalBuffer.Count} elements");
            }
        }
    }
}
