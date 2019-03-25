using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Delimon.Win32.IO
{
    public struct PathHelper
    {
        private int m_capacity;
        private int m_length;
        private int m_maxPath;
        private StringBuilder m_arrayPtr;
        private StringBuilder m_sb;
        private bool useStackAlloc;
        private bool doNotTryExpandShortFileName;

        internal PathHelper(string charArrayPtr, int length)
        {
            m_length = 0;
            m_sb = (StringBuilder)null;
            m_arrayPtr = new StringBuilder(Helpers.MaxPath);
            m_capacity = length;
            m_maxPath = Helpers.MaxPath;
            useStackAlloc = true;
            doNotTryExpandShortFileName = false;
        }

        internal PathHelper(int capacity, int maxPath)
        {
            m_length = 0;
            m_arrayPtr = null;
            useStackAlloc = false;
            m_sb = new StringBuilder(capacity);
            m_capacity = capacity;
            m_maxPath = maxPath;
            doNotTryExpandShortFileName = false;
        }

        internal int Length
        {
            get
            {
                if (useStackAlloc)
                    return m_length;
                return m_sb.Length;
            }
            set
            {
                if (useStackAlloc)
                    m_length = value;
                else
                    m_sb.Length = value;
            }
        }

        internal int Capacity
        {
            get
            {
                return m_capacity;
            }
        }

        internal char this[int index]
        {
            get
            {
                if (useStackAlloc)
                    return m_arrayPtr[index];
                return m_sb[index];
            }
            set
            {
                if (useStackAlloc)
                    if (m_arrayPtr.Length - 1 < Length)
                        m_arrayPtr.Append(value);
                    else
                        m_arrayPtr[Length] = value;
                else
                    if (m_sb.Length - 1 < Length)
                        m_sb.Append(value);
                    else
                        m_sb[Length] = value;
            }
        }

        internal void Append(char value)
        {
            if (Length + 1 >= m_capacity)
            {
                throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
            }
            if (useStackAlloc)
            {
                if (m_arrayPtr.Length - 1 < Length)
                    m_arrayPtr.Append(value);
                else
                    m_arrayPtr[Length] = value;
                m_length = m_length + 1;
            }
            else
                m_sb.Append(value);
        }

        internal int GetFullPathName()
        {
            if (useStackAlloc)
            {
                StringBuilder chPtr = new StringBuilder(Helpers.MaxPath);
                var filePart = new StringBuilder(255);
                int fullPathName = Win32Interop.GetFullPathName(m_arrayPtr.ToString(), (uint)Helpers.MaxPath + 1, chPtr, filePart);
                if (fullPathName > Helpers.MaxPath)
                {
                    filePart = new StringBuilder(255);
                    fullPathName = Win32Interop.GetFullPathName(m_arrayPtr.ToString(), (uint)fullPathName, chPtr, filePart);
                }
                if (fullPathName >= Helpers.MaxPath)
                {
                    throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                }
                if (fullPathName == 0 && m_arrayPtr != null)
                    Helpers.GetLastErrorAndThrowIfFailed();
                doNotTryExpandShortFileName = false;
                m_arrayPtr = chPtr;
                Length = fullPathName;
                return fullPathName;
            }
            StringBuilder buffer = new StringBuilder(m_capacity + 1);
            int fullPathName1 = Win32Interop.GetFullPathName(m_sb.ToString(), (uint)m_capacity + 1, buffer, null);
            if (fullPathName1 > m_maxPath)
            {
                buffer.Length = fullPathName1;
                fullPathName1 = Win32Interop.GetFullPathName(m_sb.ToString(), (uint)fullPathName1, buffer, null);
            }
            if (fullPathName1 >= m_maxPath)
            {
                throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
            }
            if (fullPathName1 == 0 && (int)m_sb[0] != 0)
            {
                if (Length >= m_maxPath)
                {
                    throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                }
                Helpers.GetLastErrorAndThrowIfFailed();
            }
            doNotTryExpandShortFileName = false;
            m_sb = buffer;
            return fullPathName1;
        }

        internal bool TryExpandShortFileName()
        {
            if (doNotTryExpandShortFileName)
                return false;
            if (useStackAlloc)
            {
                StringBuilder arrayPtr = UnsafeGetArrayPtr();
                StringBuilder chPtr = new StringBuilder(Helpers.MaxPath);
                uint longPathName = Win32Interop.GetLongPathName(arrayPtr.ToString(), chPtr, (uint)Helpers.MaxPath);
                if (longPathName >= Helpers.MaxPath)
                {
                    throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                }
                if (longPathName == 0)
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 2:
                        case 3:
                            doNotTryExpandShortFileName = true;
                            break;
                    }
                    return false;
                }
                Length = (int)longPathName;
                return true;
            }
            StringBuilder stringBuilder = GetStringBuilder();
            string str = stringBuilder.ToString();
            string path = str;
            //bool flag = false;
            //if (path.Length > Helpers.MaxPath)
            //{
            //    path = Path.AddLongPathPrefix(path);
            //    flag = true;
            //}
            stringBuilder.Capacity = m_capacity;
            stringBuilder.Length = 0;
            uint longPathName1 = Win32Interop.GetLongPathName(path, stringBuilder, (uint)m_capacity);
            if (longPathName1 == 0)
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (2 == lastWin32Error || 3 == lastWin32Error)
                    doNotTryExpandShortFileName = true;
                stringBuilder.Length = 0;
                stringBuilder.Append(str);
                return false;
            }
            //if (flag)
            //    longPathName1 -= 4;
            if (longPathName1 >= m_maxPath)
            {
                throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
            }
            //Length = Path.RemoveLongPathPrefix(stringBuilder).Length;
            return true;
        }

        internal void Fixup(int lenSavedName, int lastSlash)
        {
            if (useStackAlloc)
            {
                string chPtr = m_arrayPtr.ToString().Substring(lastSlash, lenSavedName);//string.wstrcpy(chPtr, (char*)((IntPtr)(m_arrayPtr + lastSlash) + 2), lenSavedName);
                Length = lastSlash;
                doNotTryExpandShortFileName = false;
                TryExpandShortFileName();
                Append('\\');
                if (Length + lenSavedName >= Helpers.MaxPath)
                {
                    throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                }
                m_arrayPtr.Append(chPtr);
                Length = Length + lenSavedName;
            }
            else
            {
                string str = m_sb.ToString(lastSlash + 1, lenSavedName);
                Length = lastSlash;
                doNotTryExpandShortFileName = false;
                TryExpandShortFileName();
                Append('\\');
                if (Length + lenSavedName >= m_maxPath)
                {
                    throw new PathTooLongException(Properties.Resources.IO_PathTooLong);
                }
                m_sb.Append(str);
            }
        }

        internal bool OrdinalStartsWith(string compareTo, bool ignoreCase)
        {
            if (Length < compareTo.Length)
                return false;
            if (useStackAlloc)
            {
                if (ignoreCase)
                {
                    string str = new string(m_arrayPtr.ToString().ToCharArray(), 0, compareTo.Length);
                    return compareTo.Equals(str, StringComparison.OrdinalIgnoreCase);
                }
                for (int index = 0; index < compareTo.Length; ++index)
                {
                    if (m_arrayPtr[index] != compareTo[index])
                        return false;
                }
                return true;
            }
            if (ignoreCase)
                return m_sb.ToString().StartsWith(compareTo, StringComparison.OrdinalIgnoreCase);
            return m_sb.ToString().StartsWith(compareTo, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            if (useStackAlloc)
                return new string(m_arrayPtr.ToString().ToCharArray(), 0, Length);
            return m_sb.ToString();
        }

        private StringBuilder UnsafeGetArrayPtr()
        {
            return m_arrayPtr;
        }

        private StringBuilder GetStringBuilder()
        {
            return m_sb;
        }
    }
}