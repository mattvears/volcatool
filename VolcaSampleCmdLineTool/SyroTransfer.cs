using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gui;
using NAudio.Wave;

namespace VolcaSampleCmdLineTool
{
    public class SyroTransfer
    {
        private readonly IEnumerable<FileInfo> _wavs;
        private readonly FileInfo _targetFile;
        private readonly int _startingSampleNumber;

        public SyroTransfer(
            IEnumerable<FileInfo> wavs,
            FileInfo targetFile,
            int startingSampleNumber)
        {
            _wavs = wavs;
            _targetFile = targetFile;
            _startingSampleNumber = startingSampleNumber;
        }

        public void Transfer()
        {
            var bridgeAdapter = new SdkBridgeAdapter();
            bridgeAdapter.Prepare(_wavs, _startingSampleNumber);

            Console.WriteLine("Converting data to SyroStream...");
            bridgeAdapter.Convert(_targetFile);

            Console.WriteLine("Wrote file: {0}", _targetFile.Name);
        }
    }
}
