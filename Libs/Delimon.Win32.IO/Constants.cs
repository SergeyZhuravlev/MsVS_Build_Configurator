using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Delimon.Win32.IO
{
    /// <summary>
    /// Константы Delimon
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Максимальная длинна пути с учетом имени файла.
        /// Для старой версии System.IO которая работает с короткими путями
        /// </summary>
        public const int MaxFilePathLenght = 260;

        /// <summary>
        /// Максимальная длинна пути папки.
        /// Для старой версии System.IO которая работает с короткими путями
        /// </summary>
        public const int MaxFolderPathLenght = 248;
    }
}