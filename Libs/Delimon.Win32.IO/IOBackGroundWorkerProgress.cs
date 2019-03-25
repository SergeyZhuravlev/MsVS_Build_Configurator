using System;
using System.Collections.Generic;

namespace Delimon.Win32.IO
{
    public class IOBackGroundWorkerProgress
    {
        public List<Exception> Errors = new List<Exception>();
        public bool Calculating;
        public int ProgressPercentage;
        public string CurrentOperation;
        public string CurrentFolder;
        public string CurrentFile;
        public string CurrentFolderDisplay;
        public long FoldersFound;
        public long FilesFound;
        public long TotalSize;
        public long FoldersProcessed;
        public long FilesProcessed;
        public long SizeProcessed;
        public long FoldersCopied;
        public long FilesCopied;
        public long FoldersMoved;
        public long FilesMoved;
        public long FoldersDeleted;
        public long FilesDeleted;
        public long FilesSkipped;
        public long FoldersSkipped;
        public long FilesError;
        public long FoldersError;
    }
}