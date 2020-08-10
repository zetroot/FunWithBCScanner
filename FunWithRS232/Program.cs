using System;
using System.Buffers;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace FunWithRS232
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var reader = new ScannerReader(new ScannerOptions
            {
                PortName = "COM3",
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                SuffixSeq = new char[] { (char)0x0d, (char)0x0a }
            });

            reader.Barcodes.Subscribe(onNext: s => Console.WriteLine($"new barcode: {s}"), onCompleted: () => Console.WriteLine("That's all folks!"));

            await reader.ObserveScanner();
        }
    }
}
