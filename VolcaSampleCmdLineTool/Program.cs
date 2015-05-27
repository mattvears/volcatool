using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

namespace VolcaSampleCmdLineTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = string.Empty;
            var fileName = "result.wav";
            var sampleNumber = 0;
            double? maxSeconds = null;

            var optionSet = new OptionSet
            {
                {"dir=", v => dir = v},
                {"result=", v => fileName = v},
                {"sns|sample_number_start=", v => Int32.TryParse(v, out sampleNumber)},
                {"max_seconds=", v =>
                {
                    double tmpMax;
                    if (Double.TryParse(v, out tmpMax))
                    {
                        maxSeconds = tmpMax;
                    }
                }}
            };

            optionSet.Parse(args);

            if (string.IsNullOrEmpty(dir))
            {
                Console.Error.WriteLine("provide 'dir' argument.");
                return;
            }

            var result = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileName));
            var wavLoader = new WavLoader(new DirectoryInfo(dir), sampleNumber);
            
            var wavs = maxSeconds != null 
                ? wavLoader.LoadWavsFromDirectory(TimeSpan.FromSeconds((double)maxSeconds)) 
                : wavLoader.LoadWavsFromDirectory();

            var wavsList = wavs.ToList();

            if (!wavsList.Any())
            {
                Console.Error.WriteLine("No wavs found in directory {0}.", dir);
                return;
            }

            var transferer = new SyroTransfer(wavsList, result, sampleNumber);
            transferer.Transfer();
        }
    }
}
