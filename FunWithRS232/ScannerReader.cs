using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        public char[] SuffixSeq { get; set; }
    }

    public class ScannerReader
    {
        private readonly ScannerOptions options;
        private BehaviorSubject<string> subject;

        public ScannerReader(ScannerOptions options)
        {
            this.options = options;
        }

        private readonly List<char> globalBuffer = new List<char>(65536);

        public IObservable<string> Barcodes => subject.Where(x => !string.IsNullOrEmpty(x)).AsObservable();

        public async Task ObserveScanner()
        {
            var port = new SerialPort(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits);
            port.Open();
            subject = new BehaviorSubject<string>(string.Empty);

            var stream = port.BaseStream;
            var sreader = new StreamReader(stream);
            try
            {
                while (port.IsOpen)
                {
                    var buf = new char[4096];

                    var readBytes = await sreader.ReadAsync(buf, 0, buf.Length);
                    globalBuffer.AddRange(buf.Take(readBytes));
                    ProcessBuffer();
                }
            }
            catch (OperationCanceledException) { }        
            subject.OnCompleted();
        }

        private void ProcessBuffer()
        {
            int chunkIdx = -1;
            do
            {
                chunkIdx = SearchSuffix(globalBuffer, options.SuffixSeq);
                if (chunkIdx > 0)
                {
                    var s = new string(globalBuffer.Take(chunkIdx - options.SuffixSeq.Length).ToArray());
                    globalBuffer.RemoveRange(0, chunkIdx);
                    subject.OnNext(s);
                }
            } while (chunkIdx > 0);
        }

        private static int SearchSuffix(IEnumerable<char> haystack, char[] needle)
        {
            var needleLen = needle.Length;
            var limit = haystack.Count() - needleLen;
            for (int i = 0; i <= limit; ++i)
            {
                if (haystack.Skip(i).Take(needleLen).SequenceEqual(needle))
                    return i + needleLen;
            }
            return -1;
        }
    }
}
