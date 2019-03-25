using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Delimon.Win32.IO
{
    public class Win32Interop
    {
        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        public static extern int WNetGetUniversalName(string lpLocalPath, [MarshalAs(UnmanagedType.U4)] int dwInfoLevel, 
            IntPtr lpBuffer, [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetFileTime(SafeFileHandle hFile, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpCreationTime, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastAccessTime, [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpLastWriteTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RemoveDirectory(string lpPathName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string filename, uint desiredAccess, uint shareMode, IntPtr attributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WriteFile(SafeFileHandle handle, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle handle, byte[] buffer, uint byteToRead, out uint bytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hFile);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetFileSize(SafeFileHandle handle, IntPtr size);

        [DllImport("KERNEL32.DLL", EntryPoint = "MoveFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool MoveFile(string src, string dst);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, Win32Enums.MoveFileFlags dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ReadDirectoryChangesW(SafeFileHandle hDirectory, IntPtr lpBuffer, uint nBufferLength, bool bWatchSubtree, uint dwNotifyFilter, out uint lpBytesReturned, [In] ref NativeOverlapped lpOverlapped, ReadDirectoryChangesDelegate lpCompletionRoutine);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetShortPathName(string lpszLongPath, [Out] StringBuilder lpszShortPath, uint cchBuffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetLongPathName(string lpszShortPath, [Out] StringBuilder lpszLongPath, uint cchBuffer);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EncryptFile(string lpFilename);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DecryptFile(string lpFilename, int dwReserved);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool FileEncryptionStatus(string lpFilename, out int lpStatus);

        [DllImport("kernel32.dll")]
        public static extern uint SetFilePointer(SafeFileHandle hFile, int lDistanceToMove, IntPtr lpDistanceToMoveHigh, uint dwMoveMethod);

        [DllImport("kernel32", SetLastError = true)]
        public static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateIoCompletionPort(IntPtr fileHandle, IntPtr existingCompletionPort, UIntPtr completionKey, uint numberOfConcurrentThreads);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetFullPathName(string lpFileName, uint nBufferLength, [Out]StringBuilder lpBuffer, [Out]StringBuilder lpFilePart);

        public delegate void ReadDirectoryChangesDelegate(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);
    }
}
