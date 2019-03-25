using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum SearchOption : uint
    {
        AllDirectories = 0U,
        TopDirectoryOnly = 1U,
    }
}
