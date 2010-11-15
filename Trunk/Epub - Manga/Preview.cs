using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace EpubManga
{
    public class Preview : INotifyPropertyChanged, IDisposable
    {
        #region Data

        private EncoderParameters parameters;
        private ImageCodecInfo codec;

        private int currentImageIndex;

        #endregion


        #region Ctor

        public Preview(UserInput userInput)
        {
            Data = userInput;
            Data.PropertyChanged += Data_PropertyChanged;

            DisplayPreviewButton = false;


            currentImageIndex = 0;

            parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 1);
            codec = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType.Contains("bmp")).FirstOrDefault();


            InitializeCommands();

            TreatImage();
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TreatImage();
        }

        #endregion

        #region Properties

        public UserInput Data { get; private set; }

        #region DisplayPreviewButton

        private bool displayPreviewButton;
        private static PropertyChangedEventArgs displayPreviewButtonChangedArgs = new PropertyChangedEventArgs("DisplayPreviewButton");
        public bool DisplayPreviewButton
        {
            get
            {
                return displayPreviewButton;
            }
            set
            {
                if (displayPreviewButton == value) return;
                displayPreviewButton = value;
                NotifyPropertyChanged(displayPreviewButtonChangedArgs);
            }
        }

        #endregion

        #region Error

        private string error;
        private static readonly PropertyChangedEventArgs errorChangedArgs = new PropertyChangedEventArgs("Error");
        public string Error
        {
            get
            {
                return error;
            }
            set
            {
                if (object.ReferenceEquals(error, value)) return;
                error = String.Format("Unable to display: {0}", value);
                NotifyPropertyChanged(errorChangedArgs);
            }
        }

        #endregion

        #region Image1

        private BitmapFrame image1;
        private static readonly PropertyChangedEventArgs image1ChangedArgs = new PropertyChangedEventArgs("Image1");
        public BitmapFrame Image1
        {
            get
            {
                return image1;
            }
            set
            {
                if (object.ReferenceEquals(image1, value)) return;
                image1 = value;
                NotifyPropertyChanged(image1ChangedArgs);
            }
        }

        #endregion

        #region Image2

        private BitmapFrame image2;
        private static readonly PropertyChangedEventArgs image2ChangedArgs = new PropertyChangedEventArgs("Image2");
        public BitmapFrame Image2
        {
            get
            {
                return image2;
            }
            set
            {
                if (object.ReferenceEquals(image2, value)) return;
                image2 = value;
                NotifyPropertyChanged(image2ChangedArgs);
            }
        }

        #endregion

        #region ImagePath

        private string imagePath;
        private static readonly PropertyChangedEventArgs imagePathChangedArgs = new PropertyChangedEventArgs("ImagePath");
        public string ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                if (object.ReferenceEquals(imagePath, value)) return;
                imagePath = String.Format("Displaying: {0}", value);
                NotifyPropertyChanged(imagePathChangedArgs);
            }
        }

        #endregion

        #region ShowError

        private bool showError;
        private static readonly PropertyChangedEventArgs showErrorChangedArgs = new PropertyChangedEventArgs("ShowError");
        public bool ShowError
        {
            get
            {
                return showError;
            }
            set
            {
                if (showError == value) return;
                showError = value;
                NotifyPropertyChanged(showErrorChangedArgs);
            }
        }

        #endregion

        #endregion

        #region Commands

        public Command FirstCommand { get; private set; }
        public Command PreviousCommand { get; private set; }
        public Command NextCommand { get; private set; }
        public Command LastCommand { get; private set; }

        private void InitializeCommands()
        {
            FirstCommand = new Command()
            {
                CanExecuteDelegate = (obj) => FirstCommandCanExecute(),
                ExecuteDelegate = (obj) => FirstCommandExecute()
            };

            PreviousCommand = new Command()
            {
                CanExecuteDelegate = (obj) => PreviousCommandCanExecute(),
                ExecuteDelegate = (obj) => PreviousCommandExecute()
            };

            NextCommand = new Command()
            {
                CanExecuteDelegate = (obj) => NextCommandCanExecute(),
                ExecuteDelegate = (obj) => NextCommandExecute()
            };

            LastCommand = new Command()
            {
                CanExecuteDelegate = (obj) => LastCommandCanExecute(),
                ExecuteDelegate = (obj) => LastCommandExecute()
            };
        }

        #region FirstCommand

        private bool FirstCommandCanExecute()
        {
            return currentImageIndex > 0;
        }

        private void FirstCommandExecute()
        {
            currentImageIndex = 0;
            TreatImage();
        }

        #endregion

        #region PreviousCommand

        private bool PreviousCommandCanExecute()
        {
            return currentImageIndex > 0;
        }

        private void PreviousCommandExecute()
        {
            currentImageIndex--;
            TreatImage();
        }

        #endregion

        #region NextCommand

        private bool NextCommandCanExecute()
        {
            return currentImageIndex + 1 < Data.Files.Count;
        }

        private void NextCommandExecute()
        {
            currentImageIndex++;
            TreatImage();
        }

        #endregion

        #region LastCommand

        private bool LastCommandCanExecute()
        {
            return currentImageIndex + 1 < Data.Files.Count;
        }

        private void LastCommandExecute()
        {
            currentImageIndex = Data.Files.Count - 1;
            TreatImage();
        }

        #endregion

        #endregion


        #region Functions

        private void TreatImage()
        {
            ImagePath = Data.Files[currentImageIndex];

            using (MemoryStream stream = new MemoryStream())
            {
                Bitmap imageToTreat = null;
                try
                {
                    imageToTreat = new Bitmap(Data.Files[currentImageIndex]);
                }
                catch
                {
                    Image1 = null;
                    Image2 = null;
                    Error = Data.Files[currentImageIndex];
                    ShowError = true;
                }

                if (imageToTreat != null)
                {
                    ShowError = false;

                    using (imageToTreat)
                    {
                        if (imageToTreat.Width > imageToTreat.Height)
                        {
                            if (Data.DoublePage == DoublePage.RotateLeft)
                            {
                                imageToTreat.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                SetImage1(imageToTreat, stream);
                                Image2 = null;
                            }
                            else if (Data.DoublePage == DoublePage.RotateRight)
                            {
                                imageToTreat.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                SetImage1(imageToTreat, stream);
                                Image2 = null;
                            }
                            else
                            {
                                int firstStart;
                                int secondStart;

                                switch (Data.DoublePage)
                                {
                                    case DoublePage.LeftPageFirst:
                                        firstStart = 0;
                                        secondStart = imageToTreat.Width / 2;
                                        break;
                                    case DoublePage.RightPageFirst:
                                        firstStart = imageToTreat.Width / 2;
                                        secondStart = 0;
                                        break;
                                    default:
                                        firstStart = 0;
                                        secondStart = 0;
                                        break;
                                }

                                using (Bitmap image1 = imageToTreat.Clone(new RectangleF(firstStart, 0, imageToTreat.Width / 2, imageToTreat.Height), imageToTreat.PixelFormat))
                                {
                                    SetImage1(image1, stream);
                                }

                                stream.Flush();

                                using (Bitmap image2 = imageToTreat.Clone(new RectangleF(secondStart, 0, imageToTreat.Width / 2, imageToTreat.Height), imageToTreat.PixelFormat))
                                {
                                    SetImage2(image2, stream);
                                }
                            }
                        }
                        else
                        {
                            SetImage1(imageToTreat, stream);
                            Image2 = null;
                        }
                    }
                }
            }
        }

        private void SetImage1(Bitmap bitmap, MemoryStream stream)
        {
            using (Bitmap treatedImage = ImageTreater.GetInstance().TreatImage(bitmap, Data.Height, Data.Grayscale, Data.Trimming, Data.TrimmingValue))
            {
                treatedImage.Save(stream, codec, parameters);
            }

            Image1 = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

        private void SetImage2(Bitmap bitmap, MemoryStream stream)
        {
            using (Bitmap treatedImage = ImageTreater.GetInstance().TreatImage(bitmap, Data.Height, Data.Grayscale, Data.Trimming, Data.TrimmingValue))
            {
                treatedImage.Save(stream, codec, parameters);
            }

            Image2 = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }

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

        #region  IDisposable

        public void Dispose()
        {
            Data.PropertyChanged -= Data_PropertyChanged;
        }

        #endregion
    }
}
