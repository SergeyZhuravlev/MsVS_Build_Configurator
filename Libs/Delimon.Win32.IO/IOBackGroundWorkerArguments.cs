using System.Collections.Generic;

namespace Delimon.Win32.IO
{
    public class IOBackGroundWorkerArguments
    {
        public bool ShowUI = true;
        public bool Recursive = true;
        public List<string> SourceFiles = new List<string>();
        public List<string> SourceFolders = new List<string>();
        public IOBackGroundWorkerType Type;
        public string Destination;
        public bool AllwaysOverwriteFiles;
        public bool AllwaysOverwirteReadOnlyFiles;
        public bool AllwaysOverwriteFolders;
        public bool AllwaysSkipOnErrorFiles;
        public bool AllwaysSkipOnErrorFolders;
        public bool NeverOverwriteFiles;
        public bool NeverOverwriteFolders;
        public bool NeverOverwriteReadOnlyFiles;
        public bool AllwaysDeleteReadOnlyFiles;
        public bool NeverDeleteReadOnlyFiles;
    }
}