using System.Collections.Generic;

namespace VolcaSampleCmdLineTool
{
    internal class TransferState
    {
        private static TransferState _instance;

        protected TransferState()
        {
            FileSizes = new Dictionary<string, long>();
        }

        public static TransferState Instance()
        {
            return _instance ?? (_instance = new TransferState());
        }

        public Dictionary<string, long> FileSizes { get; private set; }

        public long AllFilesSize { get; set; }

        public float GetAllFilesSizeInMegabytes()
        {
            return (AllFilesSize/1024f)/1024f;
        }
    }
}
