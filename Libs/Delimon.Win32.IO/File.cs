using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace Delimon.Win32.IO
{
    public class File
    {
        public static bool Exists(string path)
        {
            return Helpers.FileExists(path);
        }

        public static void Copy(string source, string destination)
        {
            Copy(source, destination, false);
        }

        public static void Copy(string source, string destination, bool overwrite)
        {
            try
            {
                Helpers.CopyFile(source, destination, !overwrite);
            }
            catch (UnauthorizedAccessException)
            {
                if (Helpers.DirectoryExists(destination))
                {
                    throw new IOException(string.Format(Properties.Resources.Arg_FileIsDirectory_Name, Helpers.GetNormalPath(source) + " , " + Helpers.GetNormalPath(destination)));
                }
                throw;
            }
        }

        public static DateTime GetCreationTime(string path)
        {
            return new FileInfo(path).CreationTime;
        }

        public static DateTime GetLastAccessTime(string path)
        {
            return new FileInfo(path).LastAccessTime;
        }

        public static DateTime GetLastWriteTime(string path)
        {
            return new FileInfo(path).LastWriteTime;
        }

        public static DateTime GetCreationTimeUtc(string path)
        {
            return new FileInfo(path).CreationTime;
        }

        public static DateTime GetLastAccessTimeUtc(string path)
        {
            return new FileInfo(path).LastAccessTime;
        }

        public static DateTime GetLastWriteTimeUtc(string path)
        {
            return new FileInfo(path).LastWriteTime;
        }

        public static void SetCreationTime(string path, DateTime time)
        {
            new FileInfo(path).CreationTime = time;
        }

        public static void SetLastAccessTime(string path, DateTime time)
        {
            new FileInfo(path).LastAccessTime = time;
        }

        public static void SetLastWriteTime(string path, DateTime time)
        {
            new FileInfo(path).LastWriteTime = time;
        }

        public static void SetCreationTimeUtc(string path, DateTime time)
        {
            new FileInfo(path).CreationTimeUtc = time;
        }

        public static void SetLastAccessTimeUtc(string path, DateTime time)
        {
            new FileInfo(path).LastAccessTimeUtc = time;
        }

        public static void SetLastWriteTimeUtc(string path, DateTime time)
        {
            new FileInfo(path).LastWriteTimeUtc = time;
        }

        public static void Delete(string path)
        {
            new FileInfo(path).Delete();
        }

        public static void Move(string source, string destination, bool overwrite = false)
        {
            Helpers.MoveFile(source, destination, overwrite);
        }

        public static FileAttributes GetAttributes(string path)
        {
            return new FileInfo(path).Attributes;
        }

        public static void SetAttributes(string path, FileAttributes fileattributes)
        {
            new FileInfo(path).Attributes = fileattributes;
        }

        public static void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, Encoding.UTF8);
        }

        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.Read, FileMode.Append), System.IO.FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, encoding))
                {
                    streamWriter.Write(contents);
                }
            }
        }

        public static StreamWriter AppendText(string path)
        {
            return new StreamWriter(new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.Read, FileMode.Append), System.IO.FileAccess.ReadWrite), Encoding.UTF8);
        }

        public static StreamWriter CreateText(string path)
        {
            return new StreamWriter(new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.Read, FileMode.OpenOrCreate), System.IO.FileAccess.Write), Encoding.UTF8);
        }

        public static FileStream Create(string path)
        {
            return new FileStream(Helpers.CreateFile(path, FileAccess.ReadWrite, FileShare.None, FileMode.OpenOrCreate), System.IO.FileAccess.ReadWrite);
        }

        public static FileStream Create(string path, int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Properties.Resources.ArgumentOutOfRange_NeedPosNum);
            }
            return new FileStream(Helpers.CreateFile(path, FileAccess.ReadWrite, FileShare.None, FileMode.OpenOrCreate), System.IO.FileAccess.ReadWrite, bufferSize);
        }

        public static FileStream Create(string path, int buffersize, FileOptions options)
        {
            if (buffersize <= 0)
            {
                throw new Exception();
            }
            return new FileStream(Helpers.CreateFile(path, FileAccess.ReadWrite, FileShare.None, FileMode.OpenOrCreate, (uint)options), System.IO.FileAccess.ReadWrite, buffersize);
        }

        public static FileStream Open(string path, FileMode mode)
        {
            return new FileStream(Helpers.CreateFile(path, FileAccess.ReadWrite, FileShare.Read, mode), System.IO.FileAccess.ReadWrite);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(Helpers.CreateFile(path, access, FileShare.Read, mode), (System.IO.FileAccess)access);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(Helpers.CreateFile(path, access, share, mode), (System.IO.FileAccess)access);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileOptions options)
        {
            return new FileStream(Helpers.CreateFile(path, access, FileShare.Read, mode, options), (System.IO.FileAccess)access);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileOptions options, FileShare share)
        {
            return new FileStream(Helpers.CreateFile(path, access, share, mode, options), (System.IO.FileAccess)access);
        }

        public static FileStream OpenRead(string path)
        {
            return new FileStream(Helpers.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read);
        }

        public static StreamReader OpenText(string path)
        {
            return new StreamReader(new FileStream(Helpers.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read), Encoding.UTF8);
        }

        public static long Length(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public static FileStream OpenWrite(string path)
        {
            return new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.None, FileMode.OpenOrCreate), System.IO.FileAccess.Write);
        }

        public static byte[] ReadAllBytes(string path)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read))
            {
                var list = new List<byte[]>();
                var buffer = new byte[1024];
                int num;
                for (num = fileStream.Read(buffer, 0, buffer.Length); num == buffer.Length; num = fileStream.Read(buffer, 0, buffer.Length))
                {
                    list.Add(buffer);
                    buffer = new byte[1024];
                }
                var total = new byte[list.Count * 1024 + num];
                for (int i = 0; i < list.Count; i++)
                {
                    Array.Copy(list[i], 0, total, i * 1024, 1024);
                }
                if (num > 0)
                {
                    Array.Copy(buffer, 0, total, list.Count * 1024, num);
                }
                return total;
            }
        }

        public static string[] ReadAllLines(string path)
        {
            return ReadAllLines(path, Encoding.UTF8);
        }

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read))
            {
                var list = new List<string>();
                using (var streamReader = new StreamReader(fileStream, encoding))
                {
                    if (streamReader.EndOfStream)
                        return null;
                    do
                    {
                        list.Add(streamReader.ReadLine());
                    }
                    while (!streamReader.EndOfStream);
                }
                return list.ToArray();
            }
        }

        public static string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.UTF8);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Read, FileShare.ReadWrite, FileMode.Open), System.IO.FileAccess.Read))
            {
                string str;
                using (var streamReader = new StreamReader(fileStream))
                {
                    str = streamReader.ReadToEnd();
                }
                return str;
            }
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.None, FileMode.Create), System.IO.FileAccess.Write))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                    binaryWriter.Flush();
                    fileStream.Flush(true);
                    binaryWriter.Close();
                }
            }
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            WriteAllLines(path, contents, Encoding.UTF8);
        }

        public static void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            using (var fileStream =
                new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.None, FileMode.Create),
                    System.IO.FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, encoding))
                {
                    foreach (var str in contents)
                        streamWriter.WriteLine(str);
                    streamWriter.Flush();
                    fileStream.Flush(true);
                }
            }
        }

        public static void WriteAllText(string path, string contents)
        {
            WriteAllText(path, contents, Encoding.UTF8);
        }

        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            using (var fileStream = new FileStream(Helpers.CreateFile(path, FileAccess.Write, FileShare.None, FileMode.Create), System.IO.FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, encoding))
                {
                    streamWriter.Write(contents);
                    streamWriter.Flush();
                    fileStream.Flush(true);
                }
            }
        }

        public void Encrypt(string path)
        {
            Helpers.EncryptFile(path);
        }

        public void Decrypt(string path)
        {
            Helpers.DecryptFile(path);
        }

        public void SetAccessControl(string path, FileSecurity filesecurity)
        {
            throw new NotImplementedException();
        }

        public FileSecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        public FileSecurity GetAccessControl(string path, AccessControlSections sections)
        {
            throw new NotImplementedException();
        }
    }
}