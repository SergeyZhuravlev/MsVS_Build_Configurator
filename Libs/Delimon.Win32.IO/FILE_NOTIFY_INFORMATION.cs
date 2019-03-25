using System.Runtime.InteropServices;

namespace Delimon.Win32.IO
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FILE_NOTIFY_INFORMATION
    {
        public uint NextEntryOffset;
        public uint Action;
        public uint FileNameLength;
        public char[] FileName;
    }
}