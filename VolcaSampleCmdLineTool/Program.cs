using System;
using System.IO;
using System.Linq;
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
            var showHelp = false;

            var optionSet = new OptionSet
            {
                {"help", "Show this message and exit.", v => showHelp = v != null },
                {"dir=", "[REQUIRED] Directory containing samples.", v => dir = v},
                {"result=", "Provide a filename to create the resulting wav file (default is 'result.wav').", v => fileName = v},
                {"sns|sample_number_start=", "The sample slot number to start writing to. This is the starting sample slot number, samples are added incrementally after it.", v => Int32.TryParse(v, out sampleNumber)},
                {"max_seconds=", "The longest a sample can be (in seconds) before it is trimmed.", v =>
                {
                    double tmpMax;
                    if (Double.TryParse(v, out tmpMax))
                    {
                        maxSeconds = tmpMax;
                    }
                }}
            };

            optionSet.Parse(args);

            if (string.IsNullOrEmpty(dir) || showHelp)
            {
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            var sourceDirectory = new DirectoryInfo(dir);
            if (!sourceDirectory.Exists)
            {
                Console.Error.WriteLine("The directory '{0}' does not exist!", sourceDirectory);
                return; 
            }

            var result = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileName));
            var wavLoader = new WavLoader(sourceDirectory, sampleNumber);
            
            var wavs = maxSeconds != null 
                ? wavLoader.LoadWavsFromDirectory(TimeSpan.FromSeconds((double)maxSeconds)) 
                : wavLoader.LoadWavsFromDirectory();

            var wavsList = wavs.ToList();

            if (!wavsList.Any())
            {
                Console.Error.WriteLine("No wavs found in directory {0}.", dir);
                return;
            }

            var transferState = TransferState.Instance();
            var totalMegabytes = transferState.GetAllFilesSizeInMegabytes();

            Console.WriteLine("Total memory used by samples: " + totalMegabytes.ToString("0.0") + " MB.");
            if (totalMegabytes > 4)
            {
                Console.Error.WriteLine("The size of the files to be transfered exceedes maximum capacity of 4 MB.");
                return;
            }
            
            var transferer = new SyroTransfer(wavsList, result, sampleNumber);
            transferer.Transfer();
        }
    }
}
