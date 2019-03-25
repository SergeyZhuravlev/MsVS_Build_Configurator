using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum FileAttributes : uint
    {
        ReadOnly = 1U,
        Hidden = 2U,
        System = 4U,
        Directory = 16U,
        Archive = 32U,
        Device = 64U,
        Normal = 128U,
        Temporary = 256U,
        SparseFile = 512U,
        ReparsePoint = 1024U,
        Compressed = 2048U,
        Offline = 4096U,
        NotContentIndexed = 8192U,
        Encrypted = 16384U,
        Write_Through = 2147483648U,
        Overlapped = 1073741824U,
        NoBuffering = 536870912U,
        RandomAccess = 268435456U,
        SequentialScan = 134217728U,
        DeleteOnClose = 67108864U,
        BackupSemantics = 33554432U,
        PosixSemantics = 16777216U,
        OpenReparsePoint = 2097152U,
        OpenNoRecall = 1048576U,
        FirstPipeInstance = 524288U,
    }
}