using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Management;


namespace Delimon.Win32.IO
{
    public class Helpers
    {
        public static string GetLongWin32Path(string dir)
        {
            dir = GetFullPathName(dir);
            string directory;
            if (dir.IndexOf(@"\\?\") == 0)
                directory = dir;
            else if (dir.IndexOf(@"\\") == 0 || dir.IndexOf(@"//") == 0)
                directory = @"\\?\UNC\" + dir.Substring(2);
            else
                directory = @"\\?\" + dir;
            return directory;
        }

        public static string GetNormalPath(string dir)
        {
            if (dir == null)
                return null;
            if (dir.IndexOf("\\\\?\\UNC\\") == 0)
                return "\\\\" + dir.Substring(8, dir.Length - 8);
            if (dir.IndexOf("\\\\?\\") == 0)
                return dir.Substring(4, dir.Length - 4);
            return dir;
        }

        public static bool IsDirectorySeparator(char c)
        {
            return c == System.IO.Path.DirectorySeparatorChar || c == System.IO.Path.AltDirectorySeparatorChar;
        }

        internal static readonly char[] TrimEndChars = new char[8]
        {
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            ' ',
            '\x0085',
            ' '
        };

        private static readonly char[] InvalidPathCharsWithAdditionalChecks = new char[38]
        {
            '"',
            '<',
            '>',
            '|',
            char.MinValue,
            '\x0001',
            '\x0002',
            '\x0003',
            '\x0004',
            '\x0005',
            '\x0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\x000E',
            '\x000F',
            '\x0010',
            '\x0011',
            '\x0012',
            '\x0013',
            '\x0014',
            '\x0015',
            '\x0016',
            '\x0017',
            '\x0018',
            '\x0019',
            '\x001A',
            '\x001B',
            '\x001C',
            '\x001D',
            '\x001E',
            '\x001F',
            '*',
            '?'
        };

        private static readonly char[] RealInvalidPathChars = new char[36]
        {
            '"',
            '<',
            '>',
            '|',
            char.MinValue,
            '\x0001',
            '\x0002',
            '\x0003',
            '\x0004',
            '\x0005',
            '\x0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\x000E',
            '\x000F',
            '\x0010',
            '\x0011',
            '\x0012',
            '\x0013',
            '\x0014',
            '\x0015',
            '\x0016',
            '\x0017',
            '\x0018',
            '\x0019',
            '\x001A',
            '\x001B',
            '\x001C',
            '\x001D',
            '\x001E',
            '\x001F'
        };

        internal static readonly int MaxPath = 32767;

        internal static void CheckInvalidPathChars(string path, bool checkAdditional = false)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (HasIllegalCharacters(path, checkAdditional))
            {
                throw new ArgumentException(Properties.Resources.Argument_InvalidPathChars);
            }
        }

        internal static bool HasIllegalCharacters(string path, bool checkAdditional)
        {
            if (checkAdditional)
                return path.IndexOfAny(InvalidPathCharsWithAdditionalChecks) >= 0;
            return path.IndexOfAny(RealInvalidPathChars) >= 0;
        }

        public static string NormalizePath(string path, bool fullCheck, int maxPathLength, bool expandShortPaths)
        {
            if (fullCheck)
            {
                path = path.TrimEnd(TrimEndChars);
                CheckInvalidPathChars(path, false);
            }
            int index1 = 0;
            PathHelper pathHelper = path.Length + 1 <= MaxPath
                ? new PathHelper(null, MaxPath)
                : new PathHelper(path.Length + MaxPath, maxPathLength);
            uint num1 = 0;
            uint num2 = 0;
            bool flag1 = false;
            uint num3 = 0;
            int num4 = -1;
            bool flag2 = false;
            bool flag3 = true;
            int num5 = 0;
            bool flag4 = false;
            if (path.Length > 0 && (path[0] == '\\' || path[0] == '/'))
            {
                pathHelper.Append('\\');
                ++index1;
                num4 = 0;
            }
            for (; index1 < path.Length; ++index1)
            {
                char ch1 = path[index1];
                if (ch1 == '\\' || ch1 == '/')
                {
                    if ((int)num3 == 0)
                    {
                        if (num2 > 0U)
                        {
                            int index2 = num4 + 1;
                            if ((int)path[index2] != 46)
                            {
                                throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                            }
                            if (num2 >= 2U)
                            {
                                if (flag2 && num2 > 2U)
                                {
                                    throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                                }
                                if ((int)path[index2 + 1] == 46)
                                {
                                    for (int index3 = index2 + 2; (long)index3 < (long)index2 + (long)num2; ++index3)
                                    {
                                        if ((int)path[index3] != 46)
                                        {
                                            throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                                        }
                                    }
                                    num2 = 2U;
                                }
                                else
                                {
                                    if (num2 > 1U)
                                    {
                                        throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                                    }
                                    num2 = 1U;
                                }
                            }
                            if ((int)num2 == 2)
                                pathHelper.Append('.');
                            pathHelper.Append('.');
                            flag1 = false;
                        }
                        if (num1 > 0U & flag3 && index1 + 1 < path.Length && (path[index1 + 1] == '\\' || path[index1 + 1] == '/'))
                            pathHelper.Append('\\');
                    }
                    num2 = 0U;
                    num1 = 0U;
                    if (!flag1)
                    {
                        flag1 = true;
                        pathHelper.Append('\\');
                    }
                    num3 = 0U;
                    num4 = index1;
                    flag2 = false;
                    flag3 = false;
                    if (flag4)
                    {
                        pathHelper.TryExpandShortFileName();
                        flag4 = false;
                    }
                    int num6 = pathHelper.Length - 1;
                    int num7 = num5;
                    if (num6 - num7 > byte.MaxValue)
                    {
                        throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                    }
                    num5 = num6;
                }
                else if ((int)ch1 == 46)
                    ++num2;
                else if ((int)ch1 == 32)
                {
                    ++num1;
                }
                else
                {
                    if ((int)ch1 == 126 & expandShortPaths)
                        flag4 = true;
                    flag1 = false;
                    if (flag3 && (int)ch1 == ':')
                    {
                        char ch2 = index1 > 0 ? path[index1 - 1] : ' ';
                        if (((int)num2 != 0 || num3 < 1U ? 0 : ((int)ch2 != 32 ? 1 : 0)) == 0)
                        {
                            throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                        }
                        flag2 = true;
                        if (num3 > 1U)
                        {
                            int index2 = 0;
                            while (index2 < pathHelper.Length && (int)pathHelper[index2] == 32)
                                ++index2;
                            if ((long)num3 - (long)index2 == 1L)
                            {
                                pathHelper.Length = 0;
                                pathHelper.Append(ch2);
                            }
                        }
                        num3 = 0U;
                    }
                    else
                        num3 += 1U + num2 + num1;
                    if (num2 > 0U || num1 > 0U)
                    {
                        int num6 = num4 >= 0 ? index1 - num4 - 1 : index1;
                        if (num6 > 0)
                        {
                            for (int index2 = 0; index2 < num6; ++index2)
                                pathHelper.Append(path[num4 + 1 + index2]);
                        }
                        num2 = 0U;
                        num1 = 0U;
                    }
                    pathHelper.Append(ch1);
                    num4 = index1;
                }
            }
            if (pathHelper.Length - 1 - num5 > byte.MaxValue)
            {
                throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
            }
            if ((int)num3 == 0 && num2 > 0U)
            {
                int index2 = num4 + 1;
                if ((int)path[index2] != 46)
                {
                    throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                }
                if (num2 >= 2U)
                {
                    if (flag2 && num2 > 2U)
                    {
                        throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                    }
                    if ((int)path[index2 + 1] == 46)
                    {
                        for (int index3 = index2 + 2; (long)index3 < (long)index2 + (long)num2; ++index3)
                        {
                            if ((int)path[index3] != 46)
                            {
                                throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                            }
                        }
                        num2 = 2U;
                    }
                    else
                    {
                        if (num2 > 1U)
                        {
                            throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
                        }
                        num2 = 1U;
                    }
                }
                if ((int)num2 == 2)
                    pathHelper.Append('.');
                pathHelper.Append('.');
            }
            if (pathHelper.Length == 0)
            {
                throw new ArgumentException(Properties.Resources.Arg_PathIllegal);
            }
            if (fullCheck && (pathHelper.OrdinalStartsWith("http:", false) || pathHelper.OrdinalStartsWith("file:", false)))
            {
                throw new ArgumentException(Properties.Resources.Argument_PathUriFormatNotSupported);
            }
            if (flag4)
                pathHelper.TryExpandShortFileName();
            int num8 = 1;
            if (fullCheck)
            {
                num8 = pathHelper.GetFullPathName();
                bool flag5 = false;
                for (int index2 = 0; index2 < pathHelper.Length && !flag5; ++index2)
                {
                    if ((int)pathHelper[index2] == 126 & expandShortPaths)
                        flag5 = true;
                }
                if (flag5 && !pathHelper.TryExpandShortFileName())
                {
                    int lastSlash = -1;
                    for (int index2 = pathHelper.Length - 1; index2 >= 0; --index2)
                    {
                        if (pathHelper[index2] == '\\')
                        {
                            lastSlash = index2;
                            break;
                        }
                    }
                    if (lastSlash >= 0)
                    {
                        if (pathHelper.Length >= maxPathLength)
                        {
                            throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                        }
                        int lenSavedName = pathHelper.Length - lastSlash - 1;
                        pathHelper.Fixup(lenSavedName, lastSlash);
                    }
                }
            }
            if (num8 != 0 && pathHelper.Length > 1 && ((int)pathHelper[0] == 92 && (int)pathHelper[1] == 92))
            {
                int index2;
                for (index2 = 2; index2 < num8; ++index2)
                {
                    if ((int)pathHelper[index2] == 92)
                    {
                        ++index2;
                        break;
                    }
                }
                if (index2 == num8)
                {
                    throw new ArgumentException(Properties.Resources.Arg_PathIllegalUNC);
                }
                if (pathHelper.OrdinalStartsWith("\\\\?\\globalroot", true))
                {
                    throw new ArgumentException(Properties.Resources.Arg_PathGlobalRoot);
                }
            }
            if (pathHelper.Length >= maxPathLength)
            {
                throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
            }
            if (num8 == 0)
            {
                GetLastErrorAndThrowIfFailed(path);
                return (string)null;
            }
            string a = pathHelper.ToString();
            if (string.Equals(a, path, StringComparison.Ordinal))
                a = path;
            return a;
        }

        internal static string InternalGetDirectoryName(string path)
        {
            if (path == null)
                return null;

            path = NormalizePath(path, false, 1024, false);

            var rootLength = GetRootLength(path);
            if (path.Length <= rootLength)
                return null;

            var length = path.Length;
            if (length == rootLength)
                return null;
            do
            {
            } while (length > rootLength && path[--length] != System.IO.Path.DirectorySeparatorChar && path[length] != System.IO.Path.AltDirectorySeparatorChar);
            return path.Substring(0, length);
        }

        private static int GetRootLength(string path)
        {
            int rootLength;
            int volumeEnd = 2;
            int rootStart = 2;
            var isLongPath = path.StartsWith(@"\\?\", StringComparison.Ordinal);
            var isUncPath = path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal);

            if (isLongPath)
            {
                if (isUncPath)
                    rootStart = @"\\?\UNC\".Length;
                else
                    volumeEnd += @"\\?\".Length;
            }

            var hasFirstSlash = path.Length > 0 && IsDirectorySeparator(path[0]);

            if (isLongPath && !isUncPath || !hasFirstSlash)
            {
                if (path.Length < volumeEnd || path[volumeEnd - 1] != System.IO.Path.VolumeSeparatorChar)
                    return 0;

                rootLength = volumeEnd;
                if (path.Length >= volumeEnd + 1 && IsDirectorySeparator(path[volumeEnd]))
                    ++rootLength;
                return rootLength;
            }

            // hasFirstSlash == true
            var isNetworkPath = path.Length > 1 && IsDirectorySeparator(path[1]);

            if (!isUncPath && !isNetworkPath)
                return 1;

            rootLength = rootStart;
            while (rootLength < path.Length && !IsDirectorySeparator(path[rootLength]))
                ++rootLength;
            return rootLength;
        }

        public static DirectoryInfo[] FindDirectoriesInfos(string source, string searchPattern)
        {
            var filesOrDirectories = FindFilesOrDirectories(source, searchPattern, false, true);
            if (filesOrDirectories == null || filesOrDirectories.Length <= 0)
                return new DirectoryInfo[0];
            var directoryInfoArray = new DirectoryInfo[filesOrDirectories.Length];
            int index = 0;
            foreach (var fileDirectoryInfo in filesOrDirectories)
            {
                directoryInfoArray[index] = new DirectoryInfo(fileDirectoryInfo.Path, fileDirectoryInfo.FindData);
                ++index;
            }
            return directoryInfoArray;
        }

        public static FileInfo[] FindFilesInfos(string source, string searchPattern)
        {
            var filesOrDirectories = FindFilesOrDirectories(source, searchPattern, true, false);
            if (filesOrDirectories == null || filesOrDirectories.Length <= 0)
                return new FileInfo[0];
            var fileInfoArray = new FileInfo[filesOrDirectories.Length];
            int index = 0;
            foreach (var fileDirectoryInfo in filesOrDirectories)
            {
                fileInfoArray[index] = new FileInfo(fileDirectoryInfo.Path, fileDirectoryInfo.FindData);
                ++index;
            }
            return fileInfoArray;
        }

        public static string[] FindDirectories(string source, string searchPattern)
        {
            var filesOrDirectories = FindFilesOrDirectories(source, searchPattern, false, true);
            if (filesOrDirectories == null || filesOrDirectories.Length <= 0)
                return new string[0];
            var strArray = new string[filesOrDirectories.Length];
            int index = 0;
            foreach (var fileDirectoryInfo in filesOrDirectories)
            {
                strArray[index] = fileDirectoryInfo.Path;
                ++index;
            }
            return strArray;
        }

        public static string[] FindFiles(string source, string searchPattern)
        {
            var filesOrDirectories = FindFilesOrDirectories(source, searchPattern, true, false);
            if (filesOrDirectories == null || filesOrDirectories.Length <= 0)
                return new string[0];
            var strArray = new string[filesOrDirectories.Length];
            int index = 0;
            foreach (var fileDirectoryInfo in filesOrDirectories)
            {
                strArray[index] = fileDirectoryInfo.Path;
                ++index;
            }
            return strArray;
        }

        private static FileDirectoryInfo[] FindFilesOrDirectories(string source, string searchPattern, bool bfiles, bool bfolders)
        {
            var initialSource = source;
            var arrayList = new ArrayList();
            source = GetLongWin32Path(source);
            var str = !source.Substring(source.Length - 1, 1).Equals("\\") ? source + "\\" + searchPattern : source + searchPattern;
            WIN32_FIND_DATAW lpFindFileData;
            IntPtr firstFileW = Win32Interop.FindFirstFileW(str, out lpFindFileData);
            if (firstFileW.ToInt32() == -1)
            {
                if (Marshal.GetLastWin32Error() != 2)
                    GetLastErrorAndThrowIfFailed(GetNormalPath(str));
            }
            else
            {
                var fileDirectoryInfo = ProcessDirectoryOrFile(source, lpFindFileData, bfiles, bfolders);
                if (fileDirectoryInfo.Path != null)
                    arrayList.Add(fileDirectoryInfo);
                while (Win32Interop.FindNextFileW(firstFileW, out lpFindFileData))
                {
                    fileDirectoryInfo = ProcessDirectoryOrFile(source, lpFindFileData, bfiles, bfolders);
                    if (fileDirectoryInfo.Path != null)
                        arrayList.Add(fileDirectoryInfo);
                }
            }
            Win32Interop.FindClose(firstFileW);
            if (arrayList.Count <= 0)
                return null;
            var fileDirectoryInfoArray = new FileDirectoryInfo[arrayList.Count];
            var index = 0;
            foreach (FileDirectoryInfo fileDirectoryInfo in arrayList)
            {
                fileDirectoryInfoArray[index].Path = (initialSource.Substring(initialSource.Length - 1) == "\\" || initialSource.Substring(initialSource.Length - 1) == "/")
                    ? initialSource + fileDirectoryInfo.FindData.cFileName
                    : initialSource + "\\" + fileDirectoryInfo.FindData.cFileName;
                //fileDirectoryInfoArray[index].Path = fileDirectoryInfo.Path;
                fileDirectoryInfoArray[index].FindData = fileDirectoryInfo.FindData;
                ++index;
            }
            return fileDirectoryInfoArray;
        }

        private static FileDirectoryInfo ProcessDirectoryOrFile(string source, WIN32_FIND_DATAW structFindData, bool bfiles, bool bfolders)
        {
            if (structFindData.cFileName.Equals(".") || structFindData.cFileName.Equals(".."))
                return new FileDirectoryInfo();
            if (bfiles)
            {
                var dir = !source.Substring(source.Length - 1, 1).Equals("\\") ? source + "\\" + structFindData.cFileName : source + structFindData.cFileName;
                if (((int)structFindData.dwFileAttributes & 16) == 16)
                    return new FileDirectoryInfo();
                return new FileDirectoryInfo
                    {
                    Path = GetNormalPath(dir),
                    FindData = structFindData,
                    IsFile = true,
                    IsFolder = false
                };
            }
            else
            {
                if (!bfolders)
                    return new FileDirectoryInfo();
                var dir = !source.Substring(source.Length - 1, 1).Equals("\\") ? source + "\\" + structFindData.cFileName : source + structFindData.cFileName;
                if (((int)structFindData.dwFileAttributes & 16) != 16)
                    return new FileDirectoryInfo();
                return new FileDirectoryInfo
                {
                    Path = GetNormalPath(dir),
                    FindData = structFindData,
                    IsFolder = true,
                    IsFile = false
                };
            }
        }

        public static bool FileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            WIN32_FIND_DATAW lpFindFileData;
            IntPtr firstFileW = Win32Interop.FindFirstFileW(GetLongWin32Path(path), out lpFindFileData);
            if (firstFileW.ToInt32() == -1 || (lpFindFileData.dwFileAttributes & 16) == 16)
            {
                Win32Interop.FindClose(firstFileW);
                return false;
            }
            Win32Interop.FindClose(firstFileW);
            return true;
        }

        public static WIN32_FIND_DATAW GetFileInformation(string path)
        {
            WIN32_FIND_DATAW lpFindFileData;
            IntPtr firstFileW = Win32Interop.FindFirstFileW(GetLongWin32Path(path), out lpFindFileData);
            if (firstFileW.ToInt32() == -1)
                return lpFindFileData;
            Win32Interop.FindClose(firstFileW);
            return lpFindFileData;
        }

        public static bool IsFile(string path)
        {
            WIN32_FIND_DATAW fileInformation = GetFileInformation(path);
            if (fileInformation.cFileName.Equals(""))
                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
            return ((int)fileInformation.dwFileAttributes & 16) != 16;
        }

        public static bool IsFolder(string path)
        {
            WIN32_FIND_DATAW fileInformation = GetFileInformation(path);
            if (fileInformation.cFileName.Equals(""))
                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
            return ((int)fileInformation.dwFileAttributes & 16) == 16;
        }

        public static DateTime ConvertToDateFromFileTime2(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            return DateTime.FromFileTime(ft.dwLowDateTime >= 0 ? ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime : ((long)ft.dwHighDateTime << 32) - ft.dwLowDateTime);
        }

        public static long ConvertToLongFromHighLow(long high, long low)
        {
            return low >= 0L ? (high << 32) + low : (high << 32) - low;
        }

        public static void GetLastErrorAndThrowIfFailed()
        {
            if (Marshal.GetLastWin32Error() != 0)
                throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message);
        }

        public static void GetLastErrorAndThrowIfFailed(string extra)
        {
            if (Marshal.GetLastWin32Error() != 0)
            {
                var errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case 206:
                        throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                    case 995:
                        throw new OperationCanceledException();
                    case 87:
                        throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message + ": " + extra);
                    case 183:
                        if (!string.IsNullOrEmpty(extra))
                            throw new IOException(string.Format(Properties.Resources.IO_IO_AlreadyExists_Name, extra));
                        break;
                    case 32:
                        if (string.IsNullOrEmpty(extra))
                            throw new IOException(Properties.Resources.IO_IO_SharingViolation_NoFileName);
                        throw new IOException(string.Format(Properties.Resources.IO_IO_SharingViolation_File, extra));
                    case 80:
                        if (!string.IsNullOrEmpty(extra))
                            throw new IOException(string.Format(Properties.Resources.IO_IO_FileExists_Name, extra));
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(extra))
                            throw new FileNotFoundException(Properties.Resources.IO_FileNotFound);
                        throw new FileNotFoundException(string.Format(Properties.Resources.IO_FileNotFound_FileName, extra));
                    case 3:
                        if (string.IsNullOrEmpty(extra))
                            throw new DirectoryNotFoundException(Properties.Resources.IO_PathNotFound_NoPathName);
                        throw new DirectoryNotFoundException(string.Format(Properties.Resources.IO_PathNotFound_Path, extra));
                    case 5:
                        if (string.IsNullOrEmpty(extra))
                            throw new UnauthorizedAccessException(Properties.Resources.UnauthorizedAccess_IODenied_NoPathName);
                        throw new UnauthorizedAccessException(string.Format(Properties.Resources.UnauthorizedAccess_IODenied_Path,
                                extra));
                    case 15:
                        throw new DriveNotFoundException(string.Format(Properties.Resources.IO_DriveNotFound_Drive,
                            extra));
                }
                throw new IOException(new Win32Exception(Marshal.GetLastWin32Error()).Message + ": " + extra);
            }
        }

        public static int GetLastWin32Error()
        {
            return Marshal.GetLastWin32Error();
        }

        private static Object _lockFullPathName = new Object();
        public static string GetFullPathName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            path = NormalizePath(path, true, MaxPath, true);
            lock (_lockFullPathName)
            {
                const int maxPath = 32767;
                var buffer = new StringBuilder(maxPath);
                var filePart = new StringBuilder(255);
                var result = Win32Interop.GetFullPathName(path, maxPath, buffer, filePart);
                if (result > 0 && result < buffer.Capacity)
                    //return GetLongWin32Path(buffer.ToString(0, result));
                    return buffer.ToString(0, result);

                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
                throw new Exception(string.Format("Не удалось получить полный путь для пути: {0}", path));
            }
        }

        public static void CreateDirectoryRecursively(string directory)
        {
            CreateDirRecursively(GetLongWin32Path(directory));
        }

        private static void CreateDirRecursively(string directory)
        {
            if (directory.Length < 255)
                directory = GetNormalPath(directory);
            var fileAttributes = Win32Interop.GetFileAttributes(directory);
            if (fileAttributes == Win32Consts.INVALID_FILE_ATTRIBUTES)
            {
                var slashIndex = directory.LastIndexOfAny(new[] { '\\', '/' }, directory.Length - 1);
                if(slashIndex != -1)
                    CreateDirRecursively(directory.Substring(0, slashIndex));
                CreateDirectory(directory);
            }
            else
            {
                var isDirectoryOrJunction =
                  ((fileAttributes & Win32Consts.FILE_ATTRIBUTE_DIRECTORY) != 0) ||
                  ((fileAttributes & Win32Consts.FILE_ATTRIBUTE_REPARSE_POINT) != 0) ||
                  ((fileAttributes & 0x16) != 0);
 
                if (!isDirectoryOrJunction)
                    throw new Exception("Не удалось создать папку, так как файл с таким именем существует: " + directory);
            }
        }

        public static bool DirectoryExists(string directory)
        {
            var dirAttributes = Win32Interop.GetFileAttributes(GetLongWin32Path(directory));
            if (dirAttributes == Win32Consts.INVALID_FILE_ATTRIBUTES)
                return false;

            if ((dirAttributes & Win32Consts.FILE_ATTRIBUTE_DIRECTORY) != 0)
                return true;

            return false;
        }

        public static void CreateDirectory(string directory)
        {
            if (Win32Interop.CreateDirectory(GetLongWin32Path(directory), IntPtr.Zero))
                return;
            if (DirectoryExists(directory))
                return;
            GetLastErrorAndThrowIfFailed(GetNormalPath(directory));
        }

        public static bool CopyFile(string source, string destination, bool failonexist)
        {
            bool flag = Win32Interop.CopyFile(GetLongWin32Path(source), GetLongWin32Path(destination), failonexist);
            if (!flag)
            {
                GetLastErrorAndThrowIfFailed(string.Format("while copying file '{0}' from '{1}'", GetNormalPath(destination), GetNormalPath(source)));
            }
            return flag;
        }

        public static void MoveFile(string source, string destination, bool isFile = true, bool overwrite = false )
        {
            var longWin32Path1 = GetLongWin32Path(source);
            var longWin32Path2 = GetLongWin32Path(destination);
            if (Path.GetPathRoot(longWin32Path1).ToLower().Equals(Path.GetPathRoot(longWin32Path2).ToLower()))
            {
                if (isFile && !FileExists(NormalizePath(source, true, Helpers.MaxPath, true).TrimEnd('\\')))
                {
                    throw new FileNotFoundException(string.Format(Properties.Resources.IO_FileNotFound_FileName, GetNormalPath(source)));
                }
                var flag = (overwrite && isFile)
                    ? Win32Interop.MoveFileEx(longWin32Path1, longWin32Path2, Win32Enums.MoveFileFlags.MOVEFILE_REPLACE_EXISTING) 
                    : Win32Interop.MoveFile(longWin32Path1, longWin32Path2);
                if (!flag)
                    GetLastErrorAndThrowIfFailed(
                        $"while moving file '{GetNormalPath(destination)}' from '{GetNormalPath(source)}'");
                return;
            }
            CopyFile(longWin32Path1, longWin32Path2, false);
            DeleteFile(longWin32Path1);
        }

        public static bool SetFileAttributes(string path, uint attributes)
        {
            var flag = Win32Interop.SetFileAttributes(GetLongWin32Path(path), attributes);
            if (!flag)
                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
            return flag;
        }

        public static void DeleteFile(string path)
        {
            bool flag = Win32Interop.DeleteFile(GetLongWin32Path(path));
            if (!flag)
            {
                if (Marshal.GetLastWin32Error() == 2)
                    return;
                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
            }
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (!DirectoryExists(path))
                throw new DirectoryNotFoundException();
            if (!recursive)
            {
                if (!Win32Interop.RemoveDirectory(GetLongWin32Path(path)))
                {
                    GetLastErrorAndThrowIfFailed(GetNormalPath(path));
                }
                return;
            }
            foreach (string path1 in Directory.GetDirectories(path))
                DeleteDirectory(path1, recursive);
            foreach (string dir in Directory.GetFiles(path))
                DeleteFile(dir);
            if (!Win32Interop.RemoveDirectory(GetLongWin32Path(path)))
            {
                GetLastErrorAndThrowIfFailed(GetNormalPath(path));
            }
        }

        public static string ReadFile(string path)
        {

            var file = Win32Interop.CreateFile(GetLongWin32Path(path), Convert.ToUInt32(int.MinValue), 1U, IntPtr.Zero, 3U, 0U, IntPtr.Zero);
            if (file.IsInvalid)
            {
                var fileSize = Win32Interop.GetFileSize(file, IntPtr.Zero);
                var numArray = new byte[(int)fileSize];
                uint bytesRead = 0U;
                Win32Interop.ReadFile(file, numArray, fileSize, out bytesRead, IntPtr.Zero);
                file.Close();
                file.Dispose();
                return Convert.ToBase64String(numArray);
            }
            GetLastErrorAndThrowIfFailed("\nFile : " + GetNormalPath(path));
            return null;
        }

        public static SafeFileHandle CreateFile(string path, FileAccess access, FileShare share, FileMode mode)
        {
            return CreateFile(path, access, share, mode, 0L);
        }

        public static SafeFileHandle CreateFile(string path, FileAccess access, FileShare share, FileMode mode, FileOptions options)
        {
            uint dwFlagsAndAttributes = (uint)options;
            return CreateFile(path, access, share, mode, dwFlagsAndAttributes);
        }

        public static SafeFileHandle CreateFile(string path, FileAccess access, FileShare share, FileMode mode, uint dwFlagsAndAttributes)
        {
            var fullPath = GetLongWin32Path(path);
            SafeFileHandle file;
            if (mode == FileMode.Append)
            {
                file = Win32Interop.CreateFile(fullPath, (uint)access, (uint)share, IntPtr.Zero, 4U, dwFlagsAndAttributes | 268435456U, IntPtr.Zero);
                if (file.IsInvalid)
                {
                    GetLastErrorAndThrowIfFailed(Path.GetNormalPath(fullPath));
                    return new SafeFileHandle(IntPtr.Zero, true);
                }
               
                Win32Interop.SetFilePointer(file, 0, IntPtr.Zero, Win32Consts.FILE_END);
            }
            else
                file = Win32Interop.CreateFile(fullPath, (uint)access, (uint)share, IntPtr.Zero, (uint)mode, dwFlagsAndAttributes, IntPtr.Zero);
            if (!file.IsInvalid)
                return file;
            GetLastErrorAndThrowIfFailed(Path.GetNormalPath(fullPath));
            return new SafeFileHandle(IntPtr.Zero, true);
        }

        public static SafeFileHandle CreateFile(string path)
        {
            SafeFileHandle file = Win32Interop.CreateFile(GetLongWin32Path(path), 1073741824U, 0U, IntPtr.Zero, 2U, 0U, IntPtr.Zero);
            if (!file.IsInvalid)
                return file;
            GetLastErrorAndThrowIfFailed();
            return new SafeFileHandle(IntPtr.Zero, true);
        }

        public static void CreateTempFileForType(string path)
        {
            var file = CreateFile(path);
            var fileStream = new FileStream(file, System.IO.FileAccess.ReadWrite);
            var bytes = Encoding.ASCII.GetBytes("Dummy");
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
            file.Close();
            file.Dispose();
        }

        public static string GetFirstFreeNewFolder(string path)
        {
            const string file1 = "New Folder";
            var num = 2;
            if (!Directory.Exists(Path.Combine(path, file1)))
                return file1;
            string file2;
            do
            {
                file2 = file1 + " (" + num++.ToString() + ")";
            }
            while (Directory.Exists(Path.Combine(path, file2)));
            return file2;
        }

        public static void CopyDirectory(string source, string destination, bool recursive, bool overwrite)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            foreach (var str in Directory.GetFiles(source))
                File.Copy(str, Path.Combine(destination, Path.GetFileName(str)), overwrite);
            if (!recursive)
                return;
            foreach (var str in Directory.GetDirectories(source))
                CopyDirectory(str, Path.Combine(destination, Path.GetFileName(str)), recursive, overwrite);
        }

        public static void TestDirChangeWA()
        {
            var file = Win32Interop.CreateFile(GetLongWin32Path("C:\\Temp"), Win32Consts.FILE_LIST_DIRECTORY, 7U, IntPtr.Zero, 3U, 1107296256U, IntPtr.Zero);
            if (file.IsInvalid)
                return;
            var num1 = IntPtr.Zero;
            try
            {
                var existingCompletionPort = IntPtr.Zero;
                Win32Interop.CreateIoCompletionPort(file.DangerousGetHandle(), existingCompletionPort, UIntPtr.Zero, 0U);
                num1 = Marshal.AllocHGlobal(4096);
                var lpOverlapped = new NativeOverlapped
                    {
                        EventHandle = Win32Interop.CreateEvent(IntPtr.Zero, true, false, null)
                    };
                uint lpBytesReturned;
                Win32Interop.ReadDirectoryChangesW(file, num1, 4096U, true, Win32Consts.FILE_NOTIFY_CHANGE_FILE_NAME | Win32Consts.FILE_NOTIFY_CHANGE_DIR_NAME | Win32Consts.FILE_NOTIFY_CHANGE_LAST_WRITE | Win32Consts.FILE_NOTIFY_CHANGE_CREATION, out lpBytesReturned, ref lpOverlapped, new Win32Interop.ReadDirectoryChangesDelegate(ReadDirectoryChangesWCallback));
                var flag = false;
                while (!flag)
                {
                    if (Win32Interop.WaitForSingleObject(lpOverlapped.EventHandle, 10000) != 258)
                        flag = true;
                }
            }
            catch
            {
            }
            finally
            {
                if (num1 != IntPtr.Zero)
                    Marshal.FreeHGlobal(num1);
                file.Close();
            }
        }

        private static void ReadDirectoryChangesWCallback(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped)
        {
        }

        public static string ToShortPathName(string longName)
        {
            var lpszShortPath = new StringBuilder(256);
            var cchBuffer = (uint)lpszShortPath.Capacity;
            if ((int)Win32Interop.GetShortPathName(GetLongWin32Path(longName), lpszShortPath, cchBuffer) == 0)
                GetLastErrorAndThrowIfFailed(longName);
            return (lpszShortPath).ToString();
        }

        public static string ToLongPathName(string shortName)
        {
            var lpszLongPath = new StringBuilder(short.MaxValue);
            var cchBuffer = (uint)lpszLongPath.Capacity;
            if ((int)Win32Interop.GetLongPathName(shortName, lpszLongPath, cchBuffer) == 0)
                GetLastErrorAndThrowIfFailed(shortName);
            return (lpszLongPath).ToString();
        }

        public static bool SetFileTime(string path, DateTime creationtime, DateTime accesstime, DateTime writetime)
        {
            var file = Win32Interop.CreateFile(GetLongWin32Path(path), 1073741824U, 1U, IntPtr.Zero, 3U, Win32Consts.FILE_WRITE_ATTRIBUTES | 33554432U, IntPtr.Zero);
            if (file.IsInvalid)
            {
                file.Close();
                file.Dispose();
                GetLastErrorAndThrowIfFailed(Path.GetNormalPath(path));
            }
            var lpCreationTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            var lpLastAccessTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            var lpLastWriteTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            if (!creationtime.Equals(new DateTime()))
                lpCreationTime = ConvertDateTimeToFileTime(creationtime);
            if (!accesstime.Equals(new DateTime()))
                lpLastAccessTime = ConvertDateTimeToFileTime(accesstime);
            if (!writetime.Equals(new DateTime()))
                lpLastWriteTime = ConvertDateTimeToFileTime(writetime);
            var flag = Win32Interop.SetFileTime(file, ref lpCreationTime, ref lpLastAccessTime, ref lpLastWriteTime);
            file.Close();
            file.Dispose();
            if (!flag)
                GetLastErrorAndThrowIfFailed(Path.GetNormalPath(path));
            return flag;
        }

        public static DateTime ConvertFileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            var fileTime = ((long)ft.dwHighDateTime << 32) + ft.dwLowDateTime;
            if (ft.dwLowDateTime < 0)
                fileTime += 4294967296L;
            return DateTime.FromFileTime(fileTime);
        }

        public static System.Runtime.InteropServices.ComTypes.FILETIME ConvertDateTimeToFileTime(DateTime dt)
        {
            var filetime = new System.Runtime.InteropServices.ComTypes.FILETIME();
            var num = dt.ToFileTime();
            filetime.dwLowDateTime = (int)(num & uint.MaxValue);
            filetime.dwHighDateTime = (int)(num >> 32);
            return filetime;
        }

        public static DateTime ConvertToDateTimeUtc(DateTime dt)
        {
            return Convert.ToDateTime(dt.ToFileTimeUtc());
        }

        public static bool EncryptFile(string path)
        {
            var flag = Win32Interop.EncryptFile(path);
            if (!flag)
                GetLastErrorAndThrowIfFailed(Path.GetNormalPath(path));
            return flag;
        }

        public static bool DecryptFile(string path)
        {
            var flag = Win32Interop.DecryptFile(path, 0);
            if (!flag)
                GetLastErrorAndThrowIfFailed(Path.GetNormalPath(path));
            return flag;
        }

        /// <summary>
        /// Возвращает сетевой путь к ресурсу (не работает с длинными путями)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetUniversalName(string path)
        {
            if (!path.StartsWith(@"\\?\") &&
                (path.StartsWith(@"\\") || path.StartsWith(@"//"))
                )
                return path;

            var buf = Marshal.AllocCoTaskMem(5);
            var size = 0;
           
            var ret = Win32Interop.WNetGetUniversalName(path, Win32Consts.UNIVERSAL_NAME_INFO_LEVEL, buf, ref size);
            Marshal.FreeCoTaskMem(buf);

            if (ret == Win32Consts.ERROR_NOT_CONNECTED || ret == Win32Consts.ERROR_NO_NETWORK)
                return GetLocalUniversalName(path);
            if (ret != Win32Consts.ERROR_MORE_DATA)
                return path;           
            buf = Marshal.AllocCoTaskMem(size);
            ret = Win32Interop.WNetGetUniversalName(path, Win32Consts.UNIVERSAL_NAME_INFO_LEVEL, buf, ref size);
            if (ret != Win32Consts.NOERROR)
            {
                Marshal.FreeCoTaskMem(buf);
                return path;
            }
            var uncPath = Marshal.PtrToStringAnsi(new IntPtr(buf.ToInt64() + IntPtr.Size), size);
            var actualLength = uncPath.IndexOf('\0');
            if (actualLength != -1)
                uncPath = uncPath.Substring(0, actualLength);
            Marshal.FreeCoTaskMem(buf);
            return uncPath;
        }
        
        public static string GetLocalUniversalName(string path)
        {
            if (!path.StartsWith(@"\\?\") && 
                (path.StartsWith(@"\\") || path.StartsWith(@"//"))
                )
                return path;

            var exportedShares = new ManagementClass("Win32_Share");
            var shares = exportedShares.GetInstances();
            foreach (var share in shares)
            {
                var name = share["Name"].ToString();
                var localPath = share["Path"].ToString();
                if (name.Contains("$") || !path.StartsWith(localPath)) continue;
                var rest = path.Substring(localPath.Length);
                return string.Format(@"\\{0}\{1}\{2}", Environment.MachineName, name, rest);
            }
            return "";
        }
    }
}