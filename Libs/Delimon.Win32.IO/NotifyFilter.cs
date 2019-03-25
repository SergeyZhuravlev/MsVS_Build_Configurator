using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum NotifyFilter : uint
    {
        FileName = 1U,
        DirName = 2U,
        Attributes = 4U,
        Size = 8U,
        LastWrite = 16U,
        LastAccess = 32U,
        Creation = 64U,
        Security = 256U,
    }
}
