using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace VolcaSampleCmdLineTool.Decorators
{
    public abstract class WavProcessingComponent
    {
        public abstract FileInfo Process(FileInfo waveFile);
    }
}
