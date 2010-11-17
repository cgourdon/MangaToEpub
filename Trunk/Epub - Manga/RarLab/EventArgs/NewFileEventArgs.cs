
namespace RarLab
{
    public class NewFileEventArgs
    {
        public RarFileInfo fileInfo;
        public NewFileEventArgs(RarFileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }
    }
}
