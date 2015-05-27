using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gui;
using NDesk.Options;

namespace VolcaSampleCmdLineTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            var dir = string.Empty;
            var resultWav = "result.wav";
            var sampleNumber = 0;
            double? maxSeconds = null;
            var showHelp = false;
            var eraseSlots = string.Empty;
            var mode = string.Empty;

            var optionSet = new OptionSet
            {
                {"help",    "Show this message and exit.", v => showHelp = v != null },
                {"mode=",   "[REQUIRED] can be either 'erase' or 'transfer'.", v => mode = v },
                {"erase=",  "[REQUIRED for 'erase' mode.] Slots to erase in format of: A range (ex: 0-99), or a comma seperated list (ex: 2,6,25,42), or a single slot number.", s => eraseSlots = s},
                {"dir=",    "[REQUIRED for 'transfer' mode.] Directory containing samples to transfer. REQUIRED if transferring.", v => dir = v},
                {"result=", "Provide a filename to create the resulting wav file (default is 'result.wav').", v => resultWav = v},
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

            // for help commands or invalid input data:
            if (string.IsNullOrEmpty(mode)
                || (mode == "transfer" && string.IsNullOrEmpty(dir)) 
                || (mode == "erase" && string.IsNullOrEmpty(eraseSlots))
                || showHelp)
            {
                optionSet.WriteOptionDescriptions(Console.Out);
                
                Console.WriteLine(
                    string.Join(
                        Environment.NewLine,
                        "Examples:", Environment.NewLine,
                        "\t" + "Transfer a directory of files:",
                        "\t\t" + "volcatool -mode=transfer -dir=.\\samples" + Environment.NewLine,
                        "\t" + "Erase all sample slots:",
                        "\t\t" + "volcatool -mode=erase -erase=0-99" + Environment.NewLine,
                        "\t" + "Erase a range of sample slots:",
                        "\t\t" + "volcatool -mode=erase -erase=25-30" + Environment.NewLine,
                        "\t" + "Erase a list of sample slots:",
                        "\t\t" + "volcatool -mode=erase -erase=0,1,2,3,4" + Environment.NewLine,
                        "\t" + "Erase a single sample slot:",
                        "\t\t" + "volcatool -mode=erase -erase=42"));
                
                return;
            }

            var outputFileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), resultWav));

            if (mode == "erase")
            {
                DoErase(eraseSlots, outputFileInfo, resultWav);
            }
            else if (mode == "transfer")
            {
                DoTransferProcess(dir, outputFileInfo, sampleNumber, maxSeconds);
            }
            else
            {
                Console.Error.WriteLine("Unknown mode '{0}'", mode);
            }
        }

        private static void DoErase(string eraseSlots, FileInfo outputFileInfo, string resultWav)
        {
            if (!string.IsNullOrEmpty(eraseSlots))
            {
                if (eraseSlots.Contains("-") && eraseSlots.Contains(","))
                {
                    Console.Error.WriteLine("You can provide a range, or a comma seperated list, but not both.");
                    return;
                }

                var slotsToErase = ParseSlots(eraseSlots);
                var sdkBridgeAdapter = new SdkBridgeAdapter();
                sdkBridgeAdapter.Prepare(slotsToErase);
                sdkBridgeAdapter.Convert(outputFileInfo);
                Console.WriteLine("Wrote file: {0}", resultWav);
            }
        }

        public static List<int> ParseSlots(string eraseSlots)
        {
            var slotsToErase = new List<int>();

            if (eraseSlots.Contains("-"))
            {
                var data = eraseSlots.Split(new[] {'-'}, 2);
                for (var i = int.Parse(data[0]); i <= int.Parse(data[1]); i++)
                {
                    slotsToErase.Add(i);
                }
            }
            else if (eraseSlots.Contains(","))
            {
                var slots = eraseSlots.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var slot in slots)
                {
                    var trimmedSlotNumber = slot.Trim();
                    int slotNumber;
                    if (!int.TryParse(trimmedSlotNumber, out slotNumber))
                    {
                        Console.Error.WriteLine("Erroring converting input to a list of slot numbers...");
                        Console.Error.WriteLine("Couldn't parse '{0}' into a number.", trimmedSlotNumber);
                    }

                    if (slotNumber < 0 || slotNumber > 99)
                    {
                        Console.Error.WriteLine("Slot Number '{0}' out of range (0 - 99)", slotNumber);
                    }
                    else
                    {
                        slotsToErase.Add(slotNumber);
                    }
                }
            }
            else
            {
                var trimmedSlotNumber = eraseSlots;
                int slotNumber;
                if (!int.TryParse(trimmedSlotNumber, out slotNumber))
                {
                    Console.Error.WriteLine("Erroring converting input to a list of slot numbers...");
                    Console.Error.WriteLine("Couldn't parse '{0}' into a number.", trimmedSlotNumber);
                }
                else
                {
                    slotsToErase.Add(slotNumber);
                }
            }

            return slotsToErase;
        }

        private static void DoTransferProcess(string dir, FileInfo outputFile, int sampleNumber, double? maxSeconds)
        {
            var sourceDirectory = new DirectoryInfo(dir);
            if (!sourceDirectory.Exists)
            {
                Console.Error.WriteLine("The directory '{0}' does not exist!", sourceDirectory);
                return;
            }

            
            var wavLoader = new WavLoader(sourceDirectory, sampleNumber);

            var wavs = maxSeconds != null
                ? wavLoader.LoadWavsFromDirectory(TimeSpan.FromSeconds((double) maxSeconds))
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

            var transferer = new SyroTransfer(wavsList, outputFile, sampleNumber);
            transferer.Transfer();
        }
    }
}
