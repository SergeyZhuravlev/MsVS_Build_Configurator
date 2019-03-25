using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum FileOptions : uint
    {
        None = 0U,
        Encrypted = 16384U,
        DeleteOnClose = 67108864U,
        SequentialScan = 134217728U,
        RandomAccess = 268435456U,
        Asynchronous = 1073741824U,
        WriteThrough = 2147483648U,
    }
}