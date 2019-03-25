using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum FileShare : uint
    {
        None = 0U,
        Read = 1U,
        Write = 2U,
        ReadWrite = Write | Read,
        Delete = 4U,
        Inheritable = 16U,
    }
}