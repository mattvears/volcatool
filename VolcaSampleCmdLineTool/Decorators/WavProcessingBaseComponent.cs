using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcaSampleCmdLineTool.Decorators
{
    public class WavProcessingBaseComponent : WavProcessingComponent
    {
        public override FileInfo Process(FileInfo waveFile)
        {
            return waveFile;
        }
    }
}
