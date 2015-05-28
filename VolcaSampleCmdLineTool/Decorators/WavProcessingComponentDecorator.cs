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
    public class WavProcessingComponentDecorator : WavProcessingComponent
    {
        private readonly DirectoryInfo _workingDirectoryInfo;
        private readonly WavProcessingComponent _component;

        protected WavProcessingComponentDecorator(WavProcessingComponent wavProcessingComponent)
        {
            _component = wavProcessingComponent;
            _workingDirectoryInfo = new DirectoryInfo(Path.GetTempPath());
        }

        public DirectoryInfo WorkingDirectoryInfo
        {
            get { return _workingDirectoryInfo; }
        }

        public override FileInfo Process(FileInfo waveFile)
        {
            return _component.Process(waveFile);
        }
    }
}
