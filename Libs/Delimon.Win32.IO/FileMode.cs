using System;

namespace Delimon.Win32.IO
{
    [Flags]
    public enum FileMode : uint
    {
        CreateNew = 1U,
        Create = 2U,
        Open = Create | CreateNew,
        OpenOrCreate = 4U,
        Truncate = OpenOrCreate | CreateNew,
        Append = OpenOrCreate | Create,
    }
}