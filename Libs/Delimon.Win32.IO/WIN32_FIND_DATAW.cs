using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Delimon.Win32.IO
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATAW
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 520)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 28)]
        public string cAlternateFileName;
    }
}
