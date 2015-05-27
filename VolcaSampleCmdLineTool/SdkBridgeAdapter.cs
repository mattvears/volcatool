using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gui
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct StrStruct
    {
        public string Data;
        public int Size;
    }
    
    public class SdkBridgeAdapter
    {
        public void Convert(FileInfo newFile)
        {
            var newFileNameString = StringToStrStruct(newFile.FullName);
            var res = _Convert(ref newFileNameString);

            if (res != 0)
            {
                throw new Exception("Expected SyroStatus during Convert: " + res);
            }
        }

        public void Prepare(IEnumerable<FileInfo> wavs, int startingSampleNumber)
        {
            var fileInfos = wavs as FileInfo[] ?? wavs.ToArray();
            foreach (var wav in fileInfos)
            {
                var fileNameString = StringToStrStruct(wav.FullName);
                const uint dt = (uint) SyroDataType.DataType_Sample_Compress;
                var res = _Prepare(dt, (uint)fileInfos.Count(), (uint)startingSampleNumber, ref fileNameString);

                if (res != 0)
                {
                    throw new Exception("Expected SyroStatus during Prepare: " + res);
                }
            }
        }

        [DllImport(
            "SdkBridge.dll",
            EntryPoint = "Convert", 
            CallingConvention = CallingConvention.Cdecl)]
        protected static extern uint _Convert(ref StrStruct newFileName);

        [DllImport(
            "SdkBridge.dll",
            EntryPoint = "Prepare",
            CallingConvention = CallingConvention.Cdecl)]
        protected static extern uint _Prepare(uint dataType, uint totalSamples, uint startingSampleNumber, ref StrStruct fileName);

        private static StrStruct StringToStrStruct(string fileName)
        {
            StringBuilder fnBuffer = new StringBuilder(fileName, 100);
            fnBuffer.Append((char)0);
            fnBuffer.Append('*', fnBuffer.Capacity - 8);

            StrStruct fileNameString;
            fileNameString.Data = fnBuffer.ToString();
            fileNameString.Size = fileNameString.Data.Length;
            return fileNameString;
        }
    }

    public enum SyroStatus
    {
        Status_Success,

        //------ Convert -------
        Status_IllegalDataType,
        Status_IllegalData,
        Status_IllegalParameter,
        Status_OutOfRange_Number,
        Status_OutOfRange_Quality,
        Status_NotEnoughMemory,

        //------ GetSample/End  -------
        Status_InvalidHandle,
        Status_NoData
    }
    
    public enum SyroDataType
    {
        DataType_Sample_Liner,
        DataType_Sample_Compress,
        DataType_Sample_Erase,
        DataType_Sample_All,
        DataType_Sample_AllCompress,
        DataType_Pattern
    }
}
