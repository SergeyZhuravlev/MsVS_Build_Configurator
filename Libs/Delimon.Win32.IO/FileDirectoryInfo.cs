namespace Delimon.Win32.IO
{
    internal struct FileDirectoryInfo
    {
        public string Path;
        public WIN32_FIND_DATAW FindData;
        public bool IsFile;
        public bool IsFolder;
    }
}