using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EpubManga
{
    /// <summary>
    /// String comparer based on the windows file name comparer.
    /// </summary>
    public class FileNameComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }

    }
}
