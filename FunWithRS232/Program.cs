using System;
using System.Buffers;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace FunWithRS232
{
    class Program
    {

        private static SerialPort GetPort()
        {
            var ports = SerialPort.GetPortNames().Select((x, i) => (i, x)).ToDictionary(x => x.i, x => x.x);
            Console.WriteLine("Found ports:");
            foreach (var p in ports)
                Console.WriteLine($"{p.Key}\t{p.Value}");
            Console.Write("What port do u wanna to use: ");

            var portKey = Console.ReadKey() switch
            {
                ConsoleKeyInfo ki when char.IsDigit(ki.KeyChar) => int.Parse(ki.KeyChar.ToString()),
                _ => -1
            };
            Console.WriteLine();
            if (portKey == -1) return null;
            return new SerialPort(ports[portKey], 115200, Parity.None, 8, StopBits.One);
        }

        static async Task Main(string[] args)
        {
            var reader = new ScannerReader(new ScannerOptions { PortName = "COM3", BaudRate = 115200, DataBits = 8, Parity = Parity.None, StopBits = StopBits.One });

            await reader.ObserveScanner();
        }
    }
}
