using System;

namespace RarLab
{
    public class RarFileInfo
    {
        public string FileName;
        public bool ContinuedFromPrevious = false;
        public bool ContinuedOnNext = false;
        public bool IsDirectory = false;
        public long PackedSize = 0;
        public long UnpackedSize = 0;
        public int HostOS = 0;
        public long FileCRC = 0;
        public DateTime FileTime;
        public int VersionToUnpack = 0;
        public int Method = 0;
        public int FileAttributes = 0;
        public long BytesExtracted = 0;

        public double PercentComplete
        {
            get
            {
                if (this.UnpackedSize != 0)
                    return (((double)this.BytesExtracted / (double)this.UnpackedSize) * (double)100.0);
                else
                    return (double)0;
            }
        }
    }
}
