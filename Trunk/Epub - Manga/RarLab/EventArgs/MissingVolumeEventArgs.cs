
namespace RarLab
{
    public class MissingVolumeEventArgs
    {
        public string VolumeName;
        public bool ContinueOperation = false;

        public MissingVolumeEventArgs(string volumeName)
        {
            this.VolumeName = volumeName;
        }
    }
}
