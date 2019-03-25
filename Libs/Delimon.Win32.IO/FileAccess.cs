using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum FileAccess : uint
    {
        Read = 1U,
        Write = 2U,
        ReadWrite = Write | Read,
    }
}