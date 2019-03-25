using System;
using System.Security.AccessControl;

namespace Delimon.Win32.IO
{
    public class DirectoryInfo : FileSystemInfo
    {
        public override bool Exists
        {
            get
            {
                return Directory.Exists(_sFullName);
            }
        }

        public override string Name
        {
            get
            {
                return Path.GetNormalPath(Path.GetFileName(_sFullName));
            }
        }

        public DirectoryInfo Parent
        {
            get
            {
                return new DirectoryInfo(Path.GetDirectoryName(_sFullName));
            }
        }

        public string Root
        {
            get
            {
                return Helpers.GetNormalPath(Path.GetPathRoot(_sFullName));
            }
        }

        public DirectoryInfo(string dir)
            : base(dir)
        {
        }

        public DirectoryInfo(string file, WIN32_FIND_DATAW structFindData)
            : base(file, structFindData)
        {
        }

        public void Create()
        {
            Directory.CreateDirectory(_sFullName);
        }

        public FileInfo[] GetFiles()
        {
            return Helpers.FindFilesInfos(_sFullName, "*.*");
        }

        public FileInfo[] GetFiles(string searchPattern)
        {
            return Helpers.FindFilesInfos(_sFullName, searchPattern);
        }

        public DirectoryInfo[] GetDirectories()
        {
            return Helpers.FindDirectoriesInfos(_sFullName, "*.*");
        }

        public DirectoryInfo[] GetDirectories(string searchPattern)
        {
            return Helpers.FindDirectoriesInfos(_sFullName, searchPattern);
        }

        public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            if (searchOption == SearchOption.AllDirectories)
                throw new Exception("The method or operation is not implemented.");

            return Helpers.FindDirectoriesInfos(_sFullName, searchPattern);
        }

        public FileSystemInfo[] GetFileSystemInfos()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetAccessControl(DirectorySecurity directorysecurity)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DirectorySecurity GetAccessControl()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Delete()
        {
            Helpers.DeleteDirectory(_sFullName, false);
        }

        public void Delete(bool recursive)
        {
            Helpers.DeleteDirectory(_sFullName, true);
        }

        public void MoveTo(string destination)
        {
            Helpers.MoveFile(_sFullName, destination);
        }
    }
}
