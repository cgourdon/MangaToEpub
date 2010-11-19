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
                NotifyPropertyChanged(isOffsetVisibleChangedArgs);
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

        #region Is Offset Visible

        private static readonly PropertyChangedEventArgs isOffsetVisibleChangedArgs = new PropertyChangedEventArgs("IsOffsetVisible");
        public bool IsOffsetVisible
        {
            get
            {
                return (DoublePage == DoublePage.LeftPageFirst)
                    || (DoublePage == DoublePage.RightPageFirst);
            }
        }

        #endregion

        #region Left Margin

        private double leftMargin;
        private static readonly PropertyChangedEventArgs leftMarginChangedArgs = new PropertyChangedEventArgs("LeftMargin");
        public double LeftMargin
        {
            get
            {
                return leftMargin;
            }
            set
            {
                if (leftMargin == value) return;
                leftMargin = value;
                NotifyPropertyChanged(leftMarginChangedArgs);
            }
        }

        #endregion

        #region Main Window Height

        private static readonly PropertyChangedEventArgs mainWindowHeightChangedArgs = new PropertyChangedEventArgs("MainWindowHeight");
        public int MainWindowHeight
        {
            get
            {
                if (Trimming) return 270;
                else return 245;
            }
        }

        #endregion

        #region Offset

        private int offset;
        private static readonly PropertyChangedEventArgs offsetChangedArgs = new PropertyChangedEventArgs("Offset");
        public int Offset
        {
            get
            {
                return offset;
            }
            set
            {
                if (offset == value) return;
                offset = value;
                NotifyPropertyChanged(offsetChangedArgs);
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
                NotifyPropertyChanged(mainWindowHeightChangedArgs);
            }
        }

        #endregion

        #region Trimming Level

        private TrimmingLevel trimmingLevel;
        private static readonly PropertyChangedEventArgs trimmingLevelChangedArgs = new PropertyChangedEventArgs("TrimmingLevel");
        public TrimmingLevel TrimmingLevel
        {
            get
            {
                return trimmingLevel;
            }
            set
            {
                if (trimmingLevel == value) return;
                trimmingLevel = value;
                NotifyPropertyChanged(trimmingLevelChangedArgs);
                NotifyPropertyChanged(trimmingValueChangedArgs);
            }
        }

        #endregion

        #region Trimming Method

        private TrimmingMethod trimmingMethod;
        private static readonly PropertyChangedEventArgs trimmingMethodChangedArgs = new PropertyChangedEventArgs("TrimmingMethod");
        public TrimmingMethod TrimmingMethod
        {
            get
            {
                return trimmingMethod;
            }
            set
            {
                if (trimmingMethod == value) return;
                trimmingMethod = value;
                NotifyPropertyChanged(trimmingMethodChangedArgs);
            }
        }

        #endregion

        #region Trimming Value

        private static readonly PropertyChangedEventArgs trimmingValueChangedArgs = new PropertyChangedEventArgs("TrimmingValue");
        public int TrimmingValue
        {
            get
            {
                int result;

                switch (TrimmingLevel)
                {
                    case TrimmingLevel.High:
                        result = 200;
                        break;
                    case TrimmingLevel.Medium:
                        result = 220;
                        break;
                    case TrimmingLevel.Low:
                        result = 240;
                        break;
                    default:
                        result = 256;
                        break;
                }

                return result;
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
