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

            using (var wr = new AudioFileReader(waveFile.FullName))
            {
                var resampler = new WdlResamplingSampleProvider(wr, 16000);
                WaveFileWriter.CreateWaveFile16(newFileName, resampler);
            }

            return base.Process(new FileInfo(newFileName));
        }
    }
}
