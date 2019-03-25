using System;
using System.IO;
using System.Runtime.Serialization;

namespace Delimon.Win32.IO
{
    public abstract class FileSystemInfo : MarshalByRefObject, ISerializable
    {
        protected string _sFullName;
        private FileAttributes _dwFileAttributes;
        private DateTime _datCreationTime;
        private DateTime _datLastWriteTime;
        private DateTime _datLastAccessTime;
        private long _lLength;

        protected string FullPath
        {
            get
            {
                return Helpers.GetNormalPath(_sFullName);
            }
        }

        protected string OriginalPath
        {
            get
            {
                return Path.GetDirectoryName(Helpers.GetNormalPath(_sFullName));
            }
        }

        public string FullName
        {
            get
            {
                return Helpers.GetNormalPath(_sFullName);
            }
        }

        public virtual string Name
        {
            get
            {
                return Path.GetFileName(_sFullName);
            }
        }

        public virtual bool Exists
        {
            get
            {
                return File.Exists(_sFullName);
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return _datCreationTime;
            }
            set
            {
                SetFileTime(value, new DateTime(), new DateTime());
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return _datCreationTime;
            }
            set
            {
                SetFileTime(value, new DateTime(), new DateTime());
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return _datLastWriteTime;
            }
            set
            {
                SetFileTime(new DateTime(), new DateTime(), value);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return _datLastWriteTime;
            }
            set
            {
                SetFileTime(new DateTime(), new DateTime(), value);
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return _datLastAccessTime;
            }
            set
            {
                SetFileTime(new DateTime(), value, new DateTime());
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return _datLastAccessTime;
            }
            set
            {
                SetFileTime(new DateTime(), value, new DateTime());
            }
        }

        public long Length
        {
            get
            {
                var fileInformation = Helpers.GetFileInformation(Helpers.GetNormalPath(_sFullName).TrimEnd('\\'));
                if ((fileInformation.dwFileAttributes & 16) != 0 || fileInformation.dwFileAttributes == 0)
                {
                    if (_sFullName.Length == 0)
                        throw new FileNotFoundException(Properties.Resources.IO_FileNotFound);
                    throw new FileNotFoundException(string.Format(Properties.Resources.IO_FileNotFound_FileName, _sFullName));
                }
                return Helpers.ConvertToLongFromHighLow(fileInformation.nFileSizeHigh, fileInformation.nFileSizeLow);
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                return _dwFileAttributes;
            }
            set
            {
                _dwFileAttributes = value;
                Helpers.SetFileAttributes(_sFullName, (uint)_dwFileAttributes);
            }
        }

        protected FileSystemInfo(string dir)
        {
            _sFullName = Helpers.GetLongWin32Path(dir);
            Refresh();
        }

        protected FileSystemInfo(string file, WIN32_FIND_DATAW structFindData)
        {
            _sFullName = Helpers.GetLongWin32Path(file);
            _datCreationTime = Helpers.ConvertFileTimeToDateTime(structFindData.ftCreationTime);
            _datLastWriteTime = Helpers.ConvertFileTimeToDateTime(structFindData.ftLastWriteTime);
            _datLastAccessTime = Helpers.ConvertFileTimeToDateTime(structFindData.ftLastAccessTime);
            _lLength = Helpers.ConvertToLongFromHighLow(structFindData.nFileSizeHigh, structFindData.nFileSizeLow);
            _dwFileAttributes = (FileAttributes)structFindData.dwFileAttributes;
        }

        public FileSystemInfo()
        {
        }

        public FileSystemInfo(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Helpers.GetNormalPath(_sFullName);
        }

        private void SetFileTime(DateTime create, DateTime access, DateTime write)
        {
            Helpers.SetFileTime(_sFullName, create, access, write);
            Refresh();
        }

        public virtual void Delete()
        {
            //if (!Helpers.FileExists(_sFullName))
            //    throw new DirectoryNotFoundException();
            Helpers.DeleteFile(_sFullName);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Refresh()
        {
            var fileInformation = Helpers.GetFileInformation(_sFullName);
            _datCreationTime = Helpers.ConvertFileTimeToDateTime(fileInformation.ftCreationTime);
            _datLastWriteTime = Helpers.ConvertFileTimeToDateTime(fileInformation.ftLastWriteTime);
            _datLastAccessTime = Helpers.ConvertFileTimeToDateTime(fileInformation.ftLastAccessTime);
            _lLength = Helpers.ConvertToLongFromHighLow(fileInformation.nFileSizeHigh, fileInformation.nFileSizeLow);
            _dwFileAttributes = (FileAttributes)fileInformation.dwFileAttributes;
        }
    }
}