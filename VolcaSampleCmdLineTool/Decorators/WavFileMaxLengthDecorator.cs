using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;

namespace VolcaSampleCmdLineTool.Decorators
{
    public class WavFileMaxLengthDecorator : WavProcessingComponentDecorator
    {
        private TimeSpan _cutFromEnd;

        public WavFileMaxLengthDecorator(TimeSpan length, WavProcessingComponent wavProcessingComponent) : base(wavProcessingComponent)
        {
            _cutFromEnd = length;
            
        }

        public override FileInfo Process(FileInfo waveFile)
        {
            Debug.Assert(waveFile.Directory != null, "waveFile.Directory != null");
            var newName = Path.Combine(WorkingDirectoryInfo.FullName, waveFile.Name + "." + Guid.NewGuid().ToString("D") + "-snipped.wav");
            using (WaveFileReader reader = new WaveFileReader(waveFile.FullName))
            {
                var bps = reader.WaveFormat.AverageBytesPerSecond;
                if ((double)reader.Length/bps <= _cutFromEnd.TotalSeconds)
                {
                    return base.Process(waveFile);
                }

                using (WaveFileWriter writer = new WaveFileWriter(newName, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
                    var endBytes = (int)_cutFromEnd.TotalMilliseconds*bytesPerMillisecond;
                    endBytes = endBytes - endBytes%reader.WaveFormat.BlockAlign;

                    TrimWavFile(reader, writer, 0, endBytes);
                }
            }

            return base.Process(new FileInfo(newName));
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[reader.WaveFormat.BlockAlign];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
