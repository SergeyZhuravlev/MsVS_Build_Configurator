﻿namespace Delimon.Win32.IO
{
    public class Win32Consts
    {
        public static uint FullAccess = 2032127U;
        public static uint FILE_READ_DATA = 1U;
        public static uint FILE_LIST_DIRECTORY = 1U;
        public static uint FILE_WRITE_DATA = 2U;
        public static uint FILE_ADD_FILE = 2U;
        public static uint FILE_APPEND_DATA = 4U;
        public static uint FILE_ADD_SUBDIRECTORY = 4U;
        public static uint FILE_CREATE_PIPE_INSTANCE = 4U;
        public static uint FILE_READ_EA = 8U;
        public static uint FILE_WRITE_EA = 16U;
        public static uint FILE_EXECUTE = 32U;
        public static uint FILE_TRAVERSE = 32U;
        public static uint FILE_DELETE_CHILD = 64U;
        public static uint FILE_READ_ATTRIBUTES = 128U;
        public static uint FILE_WRITE_ATTRIBUTES = 256U;
        public static uint FILE_NOTIFY_CHANGE_FILE_NAME = 1U;
        public static uint FILE_NOTIFY_CHANGE_DIR_NAME = 2U;
        public static uint FILE_NOTIFY_CHANGE_ATTRIBUTES = 4U;
        public static uint FILE_NOTIFY_CHANGE_SIZE = 8U;
        public static uint FILE_NOTIFY_CHANGE_LAST_WRITE = 16U;
        public static uint FILE_NOTIFY_CHANGE_LAST_ACCESS = 32U;
        public static uint FILE_NOTIFY_CHANGE_CREATION = 64U;
        public static uint FILE_NOTIFY_CHANGE_SECURITY = 256U;
        public static uint FILE_ACTION_ADDED = 1U;
        public static uint FILE_ACTION_REMOVED = 2U;
        public static uint FILE_ACTION_MODIFIED = 3U;
        public static uint FILE_ACTION_RENAMED_OLD_NAME = 4U;
        public static uint FILE_ACTION_RENAMED_NEW_NAME = 5U;
        public static uint FILE_ACTION_ADDED_STREAM = 6U;
        public static uint FILE_ACTION_REMOVED_STREAM = 7U;
        public static uint FILE_ACTION_MODIFIED_STREAM = 8U;
        public static uint FO_COPY = 2U;
        public static uint FO_DELETE = 3U;
        public static uint FO_MOVE = 1U;
        public static uint RENAME = 4U;
        public static uint FOF_ALLOWUNDO = 64U;
        public static uint FOF_CONFIRMMOUSE = 2U;
        public static uint FOF_FILESONLY = 128U;
        public static uint FOF_MULTIDESTFILES = 1U;
        public static uint FOF_NO_CONNECTED_ELEMENTS = 8192U;
        public static uint FOF_NOCONFIRMATION = 16U;
        public static uint FOF_NOCONFIRMMKDIR = 512U;
        public static uint FOF_NOCOPYSECURITYATTRIBS = 2048U;
        public static uint FOF_NOERRORUI = 1024U;
        public static uint FOF_NORECURSION = 4096U;
        public static uint FOF_RENAMEONCOLLISION = 8U;
        public static uint FOF_SILENT = 4U;
        public static uint FOF_SIMPLEPROGRESS = 256U;
        public static uint FOF_WANTMAPPINGHANDLE = 32U;
        public static uint FOF_WANTNUKEWARNING = 16384U;
        public static uint FILE_ENCRYPTABLE = 0U;
        public static uint FILE_IS_ENCRYPTED = 1U;
        public static uint FILE_SYSTEM_ATTR = 2U;
        public static uint FILE_ROOT_DIR = 3U;
        public static uint FILE_SYSTEM_DIR = 4U;
        public static uint FILE_UNKNOWN = 5U;
        public static uint FILE_SYSTEM_NOT_SUPPORT = 6U;
        public static uint FILE_READ_ONLY = 8U;
        public static uint FILE_DIR_DISALLOWED = 9U;
        public static uint FILE_BEGIN = 0U;
        public static uint FILE_CURRENT = 1U;
        public static uint FILE_END = 2U;
        public const uint GENERIC_WRITE = 1073741824U;
        public const uint GENERIC_READ = 2147483648U;
        public const uint FILE_SHARE_READ = 1U;
        public const uint FILE_SHARE_WRITE = 2U;
        public const uint FILE_SHARE_DELETE = 4U;
        public const uint CREATE_NEW = 1U;
        public const uint CREATE_ALWAYS = 2U;
        public const uint OPEN_EXISTING = 3U;
        public const uint OPEN_ALWAYS = 4U;
        public const uint TRUNCATE_EXISTING = 5U;
        public const int INVALID_HANDLE_VALUE = -1;
        public const uint MAX_PATH = 260U;
        public const int SEM_FAILCRITICALERRORS = 1;
        public const int STD_INPUT_HANDLE = -10;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;
        public const int ENABLE_LINE_INPUT = 2;
        public const int ENABLE_ECHO_INPUT = 4;
        public const int FILE_TYPE_DISK = 1;
        public const int FILE_TYPE_CHAR = 2;
        public const int FILE_TYPE_PIPE = 3;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_NO_MORE_FILES = 18;
        public const int ERROR_NOT_READY = 21;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_FILE_EXISTS = 80;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_CALL_NOT_IMPLEMENTED = 120;
        public const int ERROR_FILENAME_EXCED_RANGE = 206;
        public const int ERROR_DLL_INIT_FAILED = 1114;
        public const int ERROR_MORE_DATA = 234;
        public const uint FILE_FLAG_WRITE_THROUGH = 2147483648U;
        public const uint FILE_FLAG_OVERLAPPED = 1073741824U;
        public const uint FILE_FLAG_NO_BUFFERING = 536870912U;
        public const uint FILE_FLAG_RANDOM_ACCESS = 268435456U;
        public const uint FILE_FLAG_SEQUENTIAL_SCAN = 134217728U;
        public const uint FILE_FLAG_DELETE_ON_CLOSE = 67108864U;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 33554432U;
        public const uint FILE_FLAG_POSIX_SEMANTICS = 16777216U;
        public const uint FILE_FLAG_OPEN_REPARSE_POINT = 2097152U;
        public const uint FILE_FLAG_OPEN_NO_RECALL = 1048576U;
        public const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 524288U;
        public const uint SECURITY_ANONYMOUS = 0U;
        public const uint SECURITY_IDENTIFICATION = 65536U;
        public const uint SECURITY_IMPERSONATION = 131072U;
        public const uint SECURITY_DELEGATION = 196608U;
        public const long STANDARD_RIGHTS_REQUIRED = 983040L;
        public const long READ_CONTROL = 131072L;
        public const long SYNCHRONIZE = 1048576L;
        public const long STANDARD_RIGHTS_READ = 131072L;
        public const long STANDARD_RIGHTS_WRITE = 131072L;
        public const long STANDARD_RIGHTS_EXECUTE = 131072L;
        public const long STANDARD_RIGHTS_ALL = 2031616L;
        public const long SPECIFIC_RIGHTS_ALL = 65535L;
        public const long FILE_ALL_ACCESS = 2032127L;
        public const int FILE_ATTRIBUTE_READONLY = 1;
        public const int FILE_ATTRIBUTE_HIDDEN = 2;
        public const int FILE_ATTRIBUTE_SYSTEM = 4;
        public const int FILE_ATTRIBUTE_DIRECTORY = 16;
        public const int FILE_ATTRIBUTE_ARCHIVE = 32;
        public const int FILE_ATTRIBUTE_DEVICE = 64;
        public const int FILE_ATTRIBUTE_NORMAL = 128;
        public const int FILE_ATTRIBUTE_TEMPORARY = 256;
        public const int FILE_ATTRIBUTE_SPARSE_FILE = 512;
        public const int FILE_ATTRIBUTE_REPARSE_POINT = 1024;
        public const int FILE_ATTRIBUTE_COMPRESSED = 2048;
        public const int FILE_ATTRIBUTE_OFFLINE = 4096;
        public const int FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192;
        public const int FILE_ATTRIBUTE_ENCRYPTED = 16384;
        public const int INVALID_FILE_ATTRIBUTES = -1;
        public const int MAILSLOT_NO_MESSAGE = -1;
        public const int MAILSLOT_WAIT_FOREVER = -1;
        public const int FILE_CASE_SENSITIVE_SEARCH = 1;
        public const int FILE_CASE_PRESERVED_NAMES = 2;
        public const int FILE_UNICODE_ON_DISK = 4;
        public const int FILE_PERSISTENT_ACLS = 8;
        public const int FILE_FILE_COMPRESSION = 16;
        public const int FILE_VOLUME_QUOTAS = 32;
        public const int FILE_SUPPORTS_SPARSE_FILES = 64;
        public const int FILE_SUPPORTS_REPARSE_POINTS = 128;
        public const int FILE_SUPPORTS_REMOTE_STORAGE = 256;
        public const int FILE_VOLUME_IS_COMPRESSED = 32768;
        public const int FILE_SUPPORTS_OBJECT_IDS = 65536;
        public const int FILE_SUPPORTS_ENCRYPTION = 131072;
        public const int FILE_NAMED_STREAMS = 262144;
        public const int FILE_READ_ONLY_VOLUME = 524288;
        public const int WAIT_TIMEOUT = 258;
        public const int WAIT_OBJECT_0 = 0;
        public const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        public const int REMOTE_NAME_INFO_LEVEL = 0x00000002;
        public const int ERROR_NOT_CONNECTED = 2250;
        public const int ERROR_NO_NETWORK = 1222;
        public const int NOERROR = 0;
        static Win32Consts()
        {
        }
    }
}