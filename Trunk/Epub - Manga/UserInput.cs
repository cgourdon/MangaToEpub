using System.Collections.Generic;
using System.ComponentModel;

namespace EpubManga
{
    public class UserInput : INotifyPropertyChanged
    {
        #region Properties

        #region Author

        private string author;
        private static readonly PropertyChangedEventArgs authorChangedArgs = new PropertyChangedEventArgs("Author");
        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                if (object.ReferenceEquals(author, value)) return;
                author = value;
                NotifyPropertyChanged(authorChangedArgs);
            }
        }

        #endregion

        #region Double Page

        private DoublePage doublePage;
        private static readonly PropertyChangedEventArgs doublePageChangedArgs = new PropertyChangedEventArgs("DoublePage");
        public DoublePage DoublePage
        {
            get
            {
                return doublePage;
            }
            set
            {
                if (doublePage == value) return;
                doublePage = value;
                NotifyPropertyChanged(doublePageChangedArgs);
            }
        }

        #endregion

        #region Files

        private List<string> files;
        private static readonly PropertyChangedEventArgs filesChangedArgs = new PropertyChangedEventArgs("Files");
        public List<string> Files
        {
            get
            {
                return files;
            }
            set
            {
                if (object.ReferenceEquals(files, value)) return;
                files = value;
                NotifyPropertyChanged(filesChangedArgs);
            }
        }

        #endregion

        #region Grayscale

        private bool grayscale;
        private static readonly PropertyChangedEventArgs grayscaleChangedArgs = new PropertyChangedEventArgs("Grayscale");
        public bool Grayscale
        {
            get
            {
                return grayscale;
            }
            set
            {
                if (grayscale == value) return;
                grayscale = value;
                NotifyPropertyChanged(grayscaleChangedArgs);
            }
        }

        #endregion

        #region Height

        private int height;
        private static readonly PropertyChangedEventArgs heightChangedArgs = new PropertyChangedEventArgs("Height");
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height == value) return;
                height = value;
                NotifyPropertyChanged(heightChangedArgs);
            }
        }

        #endregion

        #region Output Folder

        private string outputFolder;
        private static readonly PropertyChangedEventArgs outputFolderChangedArgs = new PropertyChangedEventArgs("OutputFolder");
        public string OutputFolder
        {
            get
            {
                return outputFolder;
            }
            set
            {
                if (object.ReferenceEquals(outputFolder, value)) return;
                outputFolder = value;
                NotifyPropertyChanged(outputFolderChangedArgs);
            }
        }

        #endregion

        #region Output File

        private string outputFile;
        private static readonly PropertyChangedEventArgs outputFileChangedArgs = new PropertyChangedEventArgs("OutputFile");
        public string OutputFile
        {
            get
            {
                return outputFile;
            }
            set
            {
                if (object.ReferenceEquals(outputFile, value)) return;
                outputFile = value;
                NotifyPropertyChanged(outputFileChangedArgs);
            }
        }

        #endregion

        #region Title

        private string title;
        private static readonly PropertyChangedEventArgs titleChangedArgs = new PropertyChangedEventArgs("Title");
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (object.ReferenceEquals(title, value)) return;
                title = value;
                NotifyPropertyChanged(titleChangedArgs);
            }
        }

        #endregion

        #region Trimming

        private bool trimming;
        private static readonly PropertyChangedEventArgs trimmingChangedArgs = new PropertyChangedEventArgs("Trimming");
        public bool Trimming
        {
            get
            {
                return trimming;
            }
            set
            {
                if (trimming == value) return;
                trimming = value;
                NotifyPropertyChanged(trimmingChangedArgs);
            }
        }

        #endregion

        #region Trimming Value

        private int trimmingValue;
        private static readonly PropertyChangedEventArgs trimmingValueChangedArgs = new PropertyChangedEventArgs("TrimmingValue");
        public int TrimmingValue
        {
            get
            {
                return trimmingValue;
            }
            set
            {
                if (trimmingValue == value) return;
                trimmingValue = value;
                NotifyPropertyChanged(trimmingValueChangedArgs);
            }
        }

        #endregion

        #endregion


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}
