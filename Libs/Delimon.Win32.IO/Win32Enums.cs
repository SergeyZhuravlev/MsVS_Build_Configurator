using System;

namespace Delimon.Win32.IO
{
    public class Win32Enums
    {
        [Flags]
        public enum EFileAccess : uint
        {
            GenericRead = 2147483648U,
            GenericWrite = 1073741824U,
            GenericExecute = 536870912U,
            GenericAll = 268435456U,
        }

        [Flags]
        public enum EFileShare : uint
        {
            None = 0U,
            Read = 1U,
            Write = 2U,
            Delete = 4U,
        }

        public enum ECreationDisposition : uint
        {
            New = 1U,
            CreateAlways = 2U,
            OpenExisting = 3U,
            OpenAlways = 4U,
            TruncateExisting = 5U,
        }

        [Flags]
        public enum FileAttributes : uint
        {
            Readonly = 1U,
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

        [Flags]
        public enum MoveFileFlags : uint
        {
            MOVEFILE_REPLACE_EXISTING = 1U,
            MOVEFILE_COPY_ALLOWED = 2U,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4U,
            MOVEFILE_WRITE_THROUGH = 8U,
            MOVEFILE_CREATE_HARDLINK = 16U,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 32U,
        }

        public enum SECURITY_IMPERSONATION_LEVEL : uint
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation,
        }
    }
}
