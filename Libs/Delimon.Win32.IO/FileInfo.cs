using System;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace Delimon.Win32.IO
{
    public class FileInfo : FileSystemInfo
    {
        public DirectoryInfo Directory
        {
            get
            {
                return new DirectoryInfo(Path.GetDirectoryName(_sFullName));
            }
        }

        public string DirectoryName
        {
            get
            {
                return Helpers.GetNormalPath(Path.GetDirectoryName(_sFullName));
            }
        }

        public string Extension
        {
            get
            {
                return Helpers.GetNormalPath(Path.GetExtension(_sFullName));
            }
        }

        public bool IsReadOnly
        {
            get
            {
                Refresh();
                return (Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            }
            set
            {
                if (value)
                {
                    Attributes = Attributes | FileAttributes.ReadOnly;
                }
                else
                {
                    if ((Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                        return;
                    Attributes = Attributes ^ FileAttributes.ReadOnly;
                }
            }
        }

        public FileInfo(string file)
            : base(file)
        {
        }

        public FileInfo(string file, WIN32_FIND_DATAW structFindData)
            : base(file, structFindData)
        {
        }

        public FileInfo CopyTo(string destinationfile)
        {
            return CopyTo(destinationfile, false);
        }

        public FileInfo CopyTo(string destinationfile, bool overwrite)
        {
            File.Copy(_sFullName, destinationfile, overwrite);
            return new FileInfo(destinationfile);
        }

        public void MoveTo(string destinationfile)
        {
            Helpers.MoveFile(_sFullName, destinationfile);
        }

        public StreamWriter AppendText()
        {
            return new StreamWriter(new FileStream(Helpers.CreateFile(_sFullName, FileAccess.Write, FileShare.Read, FileMode.Append), System.IO.FileAccess.Write), Encoding.UTF8);
        }

        public FileStream Create()
        {
            return new FileStream(Helpers.CreateFile(_sFullName, FileAccess.ReadWrite, FileShare.None, FileMode.OpenOrCreate), System.IO.FileAccess.ReadWrite);
        }

        public StreamWriter CreateText()
        {
            return new StreamWriter(new FileStream(Helpers.CreateFile(_sFullName, FileAccess.Write, FileShare.Read, FileMode.OpenOrCreate), System.IO.FileAccess.Write), Encoding.UTF8);
        }

        public FileStream Open(FileMode mode)
        {
            return new FileStream(Helpers.CreateFile(_sFullName, FileAccess.ReadWrite, FileShare.None, mode), System.IO.FileAccess.ReadWrite);
        }

        public FileStream Open(FileMode mode, FileAccess access)
        {
            return new FileStream(Helpers.CreateFile(_sFullName, access, FileShare.None, mode), System.IO.FileAccess.ReadWrite);
        }

        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(Helpers.CreateFile(_sFullName, access, share, mode), (System.IO.FileAccess)mode);
        }

        public FileStream OpenRead()
        {
            return new FileStream(Helpers.CreateFile(_sFullName, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read);
        }

        public StreamReader OpenText()
        {
            return new StreamReader(new FileStream(Helpers.CreateFile(_sFullName, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read), Encoding.UTF8);
        }

        public FileStream OpenWrite()
        {
            return new FileStream(Helpers.CreateFile(_sFullName, FileAccess.Write, FileShare.Read, FileMode.OpenOrCreate), System.IO.FileAccess.Write);
        }

        public FileInfo Replace(string destination, string backupfilename)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FileInfo Replace(string destination, string backupfilename, bool ignoremetadataerror)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetAccessControl(FileSecurity filesecurity)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FileSecurity GetAccessControl()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FileSecurity GetAccessControl(AccessControlSections sections)
        {
            throw new NotImplementedException();
        }

        public void Encrypt()
        {
            throw new NotImplementedException();
        }

        public void Decrypt()
        {
            throw new NotImplementedException();
        }
    }
}