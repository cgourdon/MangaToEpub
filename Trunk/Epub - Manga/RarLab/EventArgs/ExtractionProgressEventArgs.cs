
namespace RarLab
{
    public class ExtractionProgressEventArgs
    {
        public string FileName;
        public long FileSize;
        public long BytesExtracted;
        public double PercentComplete;
        public bool ContinueOperation = true;
    }
}
