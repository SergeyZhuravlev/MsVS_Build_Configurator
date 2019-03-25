using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace Delimon.Win32.IO
{
    public class Directory
    {
        public static bool Exists(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return false;
            }
            return Helpers.DirectoryExists(dir);
        }

        public static void CreateDirectory(string dir)
        {
            Helpers.CreateDirectoryRecursively(dir);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            CopyDirectory(sourceDirName, destDirName, false, false);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            CopyDirectory(sourceDirName, destDirName, copySubDirs, false);
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new Exception(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Exists(destDirName))
                CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite);
            }

            if (!copySubDirs) return;
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectory(subdir.FullName, temppath, copySubDirs, overwrite);
            }
        }

        public static string[] GetFiles(string dir, string searchPattern)
        {
            return Helpers.FindFiles(dir, searchPattern);
        }

        public static string[] GetFiles(string dir, string searchPattern, SearchOption searchOption)
        {
            if (searchOption == SearchOption.AllDirectories)
                return GetFilesRecursively(dir, searchPattern, searchOption);
    
            return Helpers.FindFiles(dir, searchPattern);
        }

        public static string[] GetFiles(string dir)
        {
            return GetFiles(dir, "*.*");
        }

        private static string[] GetFilesRecursively(string dir, string searchPattern, SearchOption searchOption)
        {
            var arrayList = new ArrayList();
            arrayList.AddRange(Helpers.FindFiles(dir, searchPattern));
            foreach (string dir1 in Helpers.FindDirectories(dir, "*.*"))
                arrayList.AddRange(GetFilesRecursively(dir1, searchPattern, searchOption));
            return (string[])arrayList.ToArray(typeof(string));
        }

        public static string[] GetDirectories(string dir, string searchPattern, SearchOption searchOption)
        {
            return searchOption == SearchOption.AllDirectories ? GetDirectoriesRecursively(dir, searchPattern, searchOption) : Helpers.FindDirectories(dir, searchPattern);
        }

        public static string[] GetDirectories(string dir, string searchPattern)
        {
            return Helpers.FindDirectories(dir, searchPattern);
        }

        public static string[] GetDirectories(string dir)
        {
            return GetDirectories(dir, "*.*");
        }

        private static string[] GetDirectoriesRecursively(string dir, string searchPattern, SearchOption searchOption)
        {
            var arrayList = new ArrayList();
            foreach (var dir1 in Helpers.FindDirectories(dir, searchPattern))
            {
                arrayList.Add(dir1);
                arrayList.AddRange(GetDirectoriesRecursively(dir1, searchPattern, searchOption));
            }
            return (string[])arrayList.ToArray(typeof(string));
        }

        public static DateTime GetLastAccessTime(string dir)
        {
            return new DirectoryInfo(dir).LastAccessTime;
        }

        public static DateTime GetCreationTime(string dir)
        {
            return new DirectoryInfo(dir).CreationTime;
        }

        public static DateTime GetLastWriteTime(string dir)
        {
            return new DirectoryInfo(dir).LastWriteTime;
        }

        public static void Delete(string path)
        {
            Helpers.DeleteDirectory(path, false);
        }

        public static void Delete(string path, bool recursive)
        {
            Helpers.DeleteDirectory(path, recursive);
        }

        public static void Move(string source, string destination)
        {
            try
            {
                Helpers.MoveFile(source, destination, false);
            }
            catch (FileNotFoundException ex)
            {
                throw new DirectoryNotFoundException(ex.Message);
            }
        }

        public static DateTime GetCreationTimeUtc(string path)
        {
            return new DirectoryInfo(path).CreationTime;
        }

        public static DateTime GetLastAccessTimeUtc(string path)
        {
            return new DirectoryInfo(path).LastAccessTime;
        }

        public static DateTime GetLastWriteTimeUtc(string path)
        {
            return new DirectoryInfo(path).LastWriteTime;
        }

        public static void SetCreationTime(string path, DateTime time)
        {
            new DirectoryInfo(path).CreationTime = time;
        }

        public static void SetLastAccessTime(string path, DateTime time)
        {
            new DirectoryInfo(path).LastAccessTime = time;
        }

        public static void SetLastWriteTime(string path, DateTime time)
        {
            new DirectoryInfo(path).LastWriteTime = time;
        }

        public static void SetCreationTimeUtc(string path, DateTime time)
        {
            new DirectoryInfo(path).CreationTimeUtc = time;
        }

        public static void SetLastAccessTimeUtc(string path, DateTime time)
        {
            new DirectoryInfo(path).LastAccessTimeUtc = time;
        }

        public static void SetLastWriteTimeUtc(string path, DateTime time)
        {
            new DirectoryInfo(path).LastWriteTimeUtc = time;
        }

        public static string[] GetLogicalDrives()
        {
            var drives = System.IO.DriveInfo.GetDrives();
            return drives.Select(driveInfo => driveInfo.Name).ToArray();
        }

        public static DirectoryInfo GetParent(string path)
        {
            return new DirectoryInfo(Path.GetDirectoryName(path));
        }

        public static void Encrypt(string path)
        {
            Helpers.EncryptFile(path);
        }

        public static void Decrypt(string path)
        {
            Helpers.DecryptFile(path);
        }

        public static void SetAccessControl(string path, DirectorySecurity directorysecurity)
        {
            throw new NotImplementedException();
        }

        public static DirectorySecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        public static FileSecurity GetAccessControl(string path, AccessControlSections sections)
        {
            throw new NotImplementedException();
        }
    }
}