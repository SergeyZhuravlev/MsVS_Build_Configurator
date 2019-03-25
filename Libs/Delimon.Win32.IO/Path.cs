using System;
using System.Linq;

namespace Delimon.Win32.IO
{
    public class Path
    {
        public static string GetUniversalName(string path)
        {
            return Helpers.GetUniversalName(path);
        }

        public static string GetLocalUniversalName(string path)
        {
            return Helpers.GetLocalUniversalName(path);
        }

        private static string Combine_(string dir, string file)
        {
            if (dir == null || file == null)
                throw new ArgumentNullException(dir == null ? "dir" : "file");
            if (file.Length == 0)
                return dir;
            if (dir.Length == 0 || Path.IsPathRooted(file))
                return file;
            string str = dir;
            int index = str.Length - 1;
            char ch = str[index];
            if (ch != '\\' && ch != '/' && ch != ':')
                return dir + "\\" + file;
            return dir + file;
        }

        public static string Combine(string dir, string file)
        {
            return Combine_(dir, file);
        }

        public static string Combine(params string[] dir)
        {
            return dir.Aggregate(Combine_);
        }

        private static string GetParentDirectory(string path)
        {
            if (path == null)
                return null;
            int num = path.LastIndexOf(GetFileName(path));
            if (num == 0)
                return null;
            if (num != -1)
                return path.Substring(0, num - 1);
            return null;
        }

        public static string GetFileName(string path)
        {
            return GetLastPathEntry(path);
        }

        public static string GetExtension(string path)
        {
            if (path == null)
                return path;
            string lastPathEntry = GetLastPathEntry(path);
            int startIndex = lastPathEntry.LastIndexOf(".");
            if (startIndex == -1)
                return "";
            
            return lastPathEntry.Substring(startIndex, lastPathEntry.Length - startIndex);
        }

        public static string GetDirectoryName(string path)
        {
            return Helpers.InternalGetDirectoryName(path);
            /*
            if (path == null)
                return null;
            if (path == string.Empty)
                throw new ArgumentException("The path is not of a legal form.");
            path = Helpers.NormalizePath(path, false, Helpers.MaxPath, true);
            if (path.LastIndexOf('\\') != path.Length - 1)
                return GetParentDirectory(path);
            //if (path.LastIndexOf('\\') == path.IndexOf('\\'))
            //    return path;
            return path.Substring(0, path.Length - 1);
            */
        }

        public static string GetLongPath(string path)
        {
            return Helpers.GetLongWin32Path(path);
        }

        public static string GetNormalPath(string path)
        {
            return Helpers.GetNormalPath(path);
        }

        private static string GetLastPathEntry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            path = Helpers.NormalizePath(path, false, Helpers.MaxPath, true);
            if (path.IndexOf('\\') == -1)
                return path;
            var str = path.LastIndexOf('\\') != path.Length - 1 ? path : path.Substring(path.LastIndexOf('\\'));
            int num = str.LastIndexOf('\\');
            if (num != -1)
                return str.Substring(num + 1, str.Length - 1 - num);
            
            return str;
        }

        public static string GetPathRoot(string path)
        {
            if (path == null)
                return path;
            path = Helpers.NormalizePath(path, false, Helpers.MaxPath, false);
            return path.Substring(0, GetRootLength(path));
        }

        internal static int GetRootLength(string path)
        {
            int index = 0;
            int length = path.Length;
            if (length >= 1 && (path[0] == '\\' || path[0] == '/'))
            {
                index = 1;
                if (length >= 2 && (path[1] == '\\' || path[1] == '/'))
                {
                    index = 2;
                    int num = 2;
                    while (index < length && (path[index] != '\\' && path[index] != '/' || --num > 0))
                        ++index;
                }
            }
            else if (length >= 2 && (int) path[1] == ':')
            {
                index = 2;
                if (length >= 3 && (path[2] == '\\' || path[2] == '/'))
                    ++index;
            }
            return index;
        }

        public static string ChangeExtension(string path, string extension)
        {
            var result = path;
            var verifiedExtension = extension.StartsWith(".") ? extension : "." + extension;
            var str = GetExtension(path);
            if (!string.IsNullOrEmpty(str))
                result = path.Remove(path.Length - str.Length);
            return result + verifiedExtension;
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            path = Helpers.NormalizePath(path, false, Helpers.MaxPath, true);
            var str = GetExtension(path);
            if (string.IsNullOrEmpty(str)) return GetFileName(path);
            path = GetFileName(path);
            return path.Remove(path.Length - str.Length);
        }

        public static string GetFullPath(string path)
        {
            return Helpers.GetFullPathName(path);
        }

        public static char[] GetInvalidFileNameChars()
        {
            return System.IO.Path.GetInvalidFileNameChars();
        }

        public static char[] GetInvalidPathChars()
        {
            return System.IO.Path.GetInvalidPathChars();
        }

        public static string GetRandomFileName()
        {
            return System.IO.Path.GetRandomFileName();
        }

        public static string GetTempFileName()
        {
            return System.IO.Path.GetTempFileName();
        }

        public static string GetTempPath()
        {
            return System.IO.Path.GetTempPath();
        }

        public static bool HasExtension(string path)
        {
            throw new NotImplementedException();
        }

        public static bool IsPathRooted(string path)
        {
            if (path != null)
            {
                int length = path.Length;
                if (length >= 1 && (path[0] == '\\' || path[0] == '/') || length >= 2 && path[1] == ':')
                    return true;
            }
            return false;
        }
    }
}
