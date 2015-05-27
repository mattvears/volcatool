using System;
using System.Collections.Generic;
using System.IO;
using VolcaSampleCmdLineTool.Decorators;

namespace VolcaSampleCmdLineTool
{
    public class WavLoader
    {
        private readonly DirectoryInfo _directoryInfo;
        private int _startingSampleNumber;

        public WavLoader(DirectoryInfo directoryInfo, int startingSampleNumber)
        {
            _directoryInfo = directoryInfo;
            _startingSampleNumber = startingSampleNumber;
        }

        public IEnumerable<FileInfo> LoadWavsFromDirectory(
            TimeSpan? maxSampleLength = null)
        {
            var fileInfoList = new List<FileInfo>();

            foreach (var file in _directoryInfo.EnumerateFiles("*.wav"))
            {
                Console.WriteLine("S{0,2}:{1}", _startingSampleNumber++, file.Name);
                WavProcessingComponent baseComponent = new WavProcessingBaseComponent();
                baseComponent = new WavFileConvertDecorator(baseComponent);
                if (maxSampleLength != null)
                {
                    baseComponent = new WavFileMaxLengthDecorator(
                        (TimeSpan) maxSampleLength, 
                        baseComponent);
                }

                var processed = baseComponent.Process(file);
                if (processed != null)
                {
                    fileInfoList.Add(processed);
                }
            }

            return fileInfoList;
        }
    }
}
