using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace VolcaSampleCmdLineTool.Decorators
{

    public class WavFileConvertDecorator : WavProcessingComponentDecorator
    {
        private static string _outputPath;

        public WavFileConvertDecorator(WavProcessingComponent wavProcessingComponent)
            : base(wavProcessingComponent)
        {
            _outputPath = _outputPath ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "\\");
        }

        public override FileInfo Process(FileInfo waveFile)
        {
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }

            var newFileName = Path.Combine(_outputPath, waveFile.Name);
            try
            {
                using (var wr = new AudioFileReader(waveFile.FullName))
                {
                    var resampler = new WdlResamplingSampleProvider(wr, 16000);
                    WaveFileWriter.CreateWaveFile16(newFileName, resampler);
                    var newFileInfo = new FileInfo(newFileName);

                    var transferState = TransferState.Instance();
                    transferState.FileSizes.Add(waveFile.Name, newFileInfo.Length);
                    transferState.AllFilesSize += newFileInfo.Length;
                    return base.Process(newFileInfo);
                }
            }
            catch (IOException ioException)
            {
                Console.Error.WriteLine("Could not convert file '{0}'", waveFile.FullName);
                Console.Error.WriteLine("Reason: {0}", ioException.Message);
                return null;
            }
        }
    }
}
