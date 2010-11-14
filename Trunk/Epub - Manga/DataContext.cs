using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Ionic.Zip;

namespace EpubManga
{
    public class DataContext : INotifyPropertyChanged
    {
        #region Data

        private List<string> allowedFileExtensions;

        private EncoderParameters parameters;
        private ImageCodecInfo codec;
        private ImageAttributes imageAttributes;

        private StringBuilder builderContent1;
        private StringBuilder builderContent2;
        private StringBuilder builderContent3;
        private StringBuilder builderToc1;
        private StringBuilder builderToc2;
        private StringBuilder builderChapter1;
        private StringBuilder builderChapter2;

        private string compilePath;
        private string oebpsFolderPath;
        private string imagesFolderPath;

        private BackgroundWorker worker;

        private List<string> errors;

        #endregion


        #region Ctor

        public DataContext()
        {
            Data = new UserInput() { Height = 744, OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\",
                DoublePage = DoublePage.RightPageFirst, Grayscale = true, Trimming = true, TrimmingValue = 220 };
            if (Directory.Exists(Data.OutputFolder + "My Books"))
            {
                Data.OutputFolder += "My Books\\";
            }

            IsBusy = false;
            InitializeCommands();

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 1);
            codec = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType.Contains("jpeg")).FirstOrDefault();
            imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(new ColorMatrix(new float[][] 
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                }));

            allowedFileExtensions = new List<string>();
            var decoders = ImageCodecInfo.GetImageDecoders();
            foreach (var decoder in decoders)
            {
                var extensions = decoder.FilenameExtension.Split(';');
                foreach (var extension in extensions)
                {
                    allowedFileExtensions.Add(extension.Trim('*').ToLower());
                }
            }
        }

        #endregion

        #region Properties

        public UserInput Data { get; private set; }

        #region IsBusy

        private bool isBusy;
        private static readonly PropertyChangedEventArgs isBusyChangedArgs = new PropertyChangedEventArgs("IsBusy");
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                NotifyPropertyChanged(isBusyChangedArgs);
            }
        }

        #endregion

        #region TotalImages

        private int totalImages;
        private static readonly PropertyChangedEventArgs totalImagesChangedArgs = new PropertyChangedEventArgs("TotalImages");
        public int TotalImages
        {
            get
            {
                return totalImages;
            }
            set
            {
                if (totalImages == value) return;
                totalImages = value;
                NotifyPropertyChanged(totalImagesChangedArgs);
            }
        }

        #endregion

        #region TreatedImages

        private int treatedImages;
        private static readonly PropertyChangedEventArgs treatedImagesChangedArgs = new PropertyChangedEventArgs("TreatedImages");
        public int TreatedImages
        {
            get
            {
                return treatedImages;
            }
            set
            {
                if (treatedImages == value) return;
                treatedImages = value;
                NotifyPropertyChanged(treatedImagesChangedArgs);
            }
        }

        #endregion

        #endregion

        #region Commands

        public Command GenerateCommand { get; private set; }
        public Command SelectFilesCommand { get; private set; }
        public Command SelectInputFolderCommand { get; private set; }
        public Command SelectOutputFolderCommand { get; private set; }

        private void InitializeCommands()
        {
            GenerateCommand = new Command()
            {
                CanExecuteDelegate = (obj) => GenerateCommandCanExecute(),
                ExecuteDelegate = (obj) => GenerateCommandExecute()
            };

            SelectFilesCommand = new Command()
            {
                CanExecuteDelegate = (obj) => SelectFilesCommandCanExecute(),
                ExecuteDelegate = (obj) => SelectFilesCommandExecute()
            };

            SelectInputFolderCommand = new Command()
            {
                CanExecuteDelegate = (obj) => SelectInputFolderCommandCanExecute(),
                ExecuteDelegate = (obj) => SelectInputFolderCommandExecute()
            };

            SelectOutputFolderCommand = new Command()
            {
                CanExecuteDelegate = (obj) => SelectOutputFolderCommandCanExecute(),
                ExecuteDelegate = (obj) => SelectOutputFolderCommandExecute()
            };
        }

        #region GenerateCommand

        private bool GenerateCommandCanExecute()
        {
            return ((!IsBusy)
                && (Data.Files != null)
                && (Data.Files.Count > 0)
                && (Data.Height > 0)
                && (!string.IsNullOrEmpty(Data.OutputFolder))
                && (Directory.Exists(Data.OutputFolder))
                && (!string.IsNullOrEmpty(Data.OutputFile)));
        }

        private void GenerateCommandExecute()
        {
            IsBusy = true;

            TotalImages = Data.Files.Count;
            TreatedImages = 0;

            if (!Data.OutputFile.EndsWith(".epub", StringComparison.InvariantCultureIgnoreCase))
            {
                Data.OutputFile += ".epub";
            }

            worker.RunWorkerAsync();
        }

        #endregion

        #region SelectFilesCommand

        private bool SelectFilesCommandCanExecute()
        {
            return !IsBusy;
        }

        private void SelectFilesCommandExecute()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Data.Files = ofd.FileNames.Where(f =>
                    {
                        if (string.IsNullOrEmpty(f)) return false;

                        FileInfo file = new FileInfo(f);
                        if (allowedFileExtensions.Contains(file.Extension.ToLower())) return true;

                        return false;
                    }).ToList();
            }
        }

        #endregion

        #region SelectInputFolderCommand

        private string lastSelectedFolder;

        private bool SelectInputFolderCommandCanExecute()
        {
            return !IsBusy;
        }

        private void SelectInputFolderCommandExecute()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = lastSelectedFolder;
                fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    lastSelectedFolder = fbd.SelectedPath;
                    DirectoryInfo dir = new DirectoryInfo(lastSelectedFolder);
                    List<FileInfo> files = dir.GetFiles().OrderBy(f => f.Name, new FileNameComparer()).ToList();

                    List<string> paths = new List<string>();
                    foreach (FileInfo file in files)
                    {
                        if (allowedFileExtensions.Contains(file.Extension.ToLower()))
                        {
                            paths.Add(file.FullName);
                        }
                    }
                    Data.Files = paths;
                }
            }
        }

        #endregion

        #region SelectOutputFolderCommand

        private bool SelectOutputFolderCommandCanExecute()
        {
            return !IsBusy;
        }

        private void SelectOutputFolderCommandExecute()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Data.OutputFolder;
                fbd.ShowNewFolderButton = true;

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Data.OutputFolder = fbd.SelectedPath.EndsWith("\\") ? fbd.SelectedPath : fbd.SelectedPath + "\\";
                }
            }
        }

        #endregion

        #endregion


        #region Generate

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitializeWorkspace();
            InitializeBuilders();



            int imageIndex = 1;

            foreach (string path in Data.Files)
            {
                using (Bitmap from = new Bitmap(path))
                {
                    if (from.Width > from.Height)
                    {
                        if (Data.DoublePage == DoublePage.RotateLeft)
                        {
                            from.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            SaveImage(from, ref imageIndex, path);
                        }
                        else if (Data.DoublePage == DoublePage.RotateRight)
                        {
                            from.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            SaveImage(from, ref imageIndex, path);
                        }
                        else
                        {
                            int firstStart;
                            int secondStart;

                            switch (Data.DoublePage)
                            {
                                case DoublePage.LeftPageFirst:
                                    firstStart = 0;
                                    secondStart = from.Width / 2;
                                    break;
                                case DoublePage.RightPageFirst:
                                    firstStart = from.Width / 2;
                                    secondStart = 0;
                                    break;
                                default:
                                    firstStart = 0;
                                    secondStart = 0;
                                    break;
                            }

                            using (Bitmap image = from.Clone(new RectangleF(firstStart, 0, from.Width / 2, from.Height), from.PixelFormat))
                            {
                                SaveImage(image, ref imageIndex, path);
                            }

                            using (Bitmap image = from.Clone(new RectangleF(secondStart, 0, from.Width / 2, from.Height), from.PixelFormat))
                            {
                                SaveImage(image, ref imageIndex, path);
                            }
                        }
                    }
                    else
                    {
                        SaveImage(from, ref imageIndex, path);
                    }
                }

                TreatedImages++;
            }



            string toWrite;
            StreamWriter writer;

            toWrite = builderContent1.ToString() + builderContent2.ToString() + builderContent3.ToString();
            writer = new StreamWriter(oebpsFolderPath + "content.opf", false);
            writer.Write(toWrite);
            writer.Close();

            toWrite = builderToc1.ToString() + builderToc2.ToString();
            writer = new StreamWriter(oebpsFolderPath + "toc.ncx", false);
            writer.Write(toWrite);
            writer.Close();



            Cleanup();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            if (errors.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("The following images have been ignored because of their width / height ratio above 0.75:");

                errors.ForEach(s => builder.AppendLine(s));

                MessageBox.Show(builder.ToString(), "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            TreatedImages = 0;
            CommandManager.InvalidateRequerySuggested();
        }

        private void InitializeWorkspace()
        {
            compilePath = Data.OutputFolder + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "") + "\\";
            oebpsFolderPath = compilePath + "OEBPS\\";
            imagesFolderPath = oebpsFolderPath + "Images\\";
            errors = new List<string>();

            Directory.CreateDirectory(compilePath + "META-INF");
            Directory.CreateDirectory(compilePath + "OEBPS");
            Directory.CreateDirectory(compilePath + "OEBPS\\Images");

            string mimetype = "application/epub+zip";
            StreamWriter writer = new StreamWriter(compilePath + "mimetype", false);
            writer.Write(mimetype);
            writer.Close();

            StringBuilder container = new StringBuilder();
            container.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            container.AppendLine("<container xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\" version=\"1.0\">");
            container.AppendLine("\t<rootfiles>");
            container.AppendLine("\t\t<rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/>");
            container.AppendLine("\t</rootfiles>");
            container.AppendLine("</container>");

            writer = new StreamWriter(compilePath + "META-INF\\container.xml", false);
            writer.Write(container.ToString());
            writer.Close();
        }

        private void InitializeBuilders()
        {
            builderContent1 = new StringBuilder();
            builderContent1.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builderContent1.AppendLine("<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"EPB-UUID\" version=\"2.0\">");
            builderContent1.AppendLine("\t<metadata xmlns:opf=\"http://www.idpf.org/2007/opf\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
            builderContent1.AppendLine("\t\t<dc:creator opf:role=\"aut\">" + Data.Author + "</dc:creator>");
            builderContent1.AppendLine("\t\t<dc:title>" + Data.Title + "</dc:title>");
            builderContent1.AppendLine("\t\t<dc:creator></dc:creator>");
            builderContent1.AppendLine("\t\t<dc:publisher></dc:publisher>");
            builderContent1.AppendLine("\t\t<dc:date></dc:date>");
            builderContent1.AppendLine("\t\t<dc:source></dc:source>");
            builderContent1.AppendLine("\t\t<dc:rights></dc:rights>");
            builderContent1.AppendLine("\t\t<dc:identifier id=\"EPB-UUID\">" + Guid.NewGuid().ToString().ToUpperInvariant().Replace("{", "").Replace("}", "") + "</dc:identifier>");
            builderContent1.AppendLine("\t\t<dc:language>en-gb</dc:language>");
            builderContent1.AppendLine("\t</metadata>");
            builderContent1.AppendLine("\t<manifest>");
            builderContent1.AppendLine("\t\t<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");

            builderContent2 = new StringBuilder();
            builderContent2.AppendLine("\t</manifest>");
            builderContent2.AppendLine("\t<spine toc=\"ncx\">");

            builderContent3 = new StringBuilder();
            builderContent3.AppendLine("\t</spine>");
            builderContent3.AppendLine("</package>");

            builderToc1 = new StringBuilder();
            builderToc1.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builderToc1.AppendLine("<?xml-buf ----------------------------------------------------------------------------------------?>");
            builderToc1.AppendLine("<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">");
            builderToc1.AppendLine("\t<head>");
            builderToc1.AppendLine("\t\t<meta name=\"dtb:uid\" content=\"0\"/>");
            builderToc1.AppendLine("\t\t<meta name=\"epub-creator\" content=\"0\"/>");
            builderToc1.AppendLine("\t\t<meta name=\"dtb:depth\" content=\"1\"/>");
            builderToc1.AppendLine("\t\t<meta name=\"dtb:totalPageCount\" content=\"0\"/>");
            builderToc1.AppendLine("\t\t<meta name=\"dtb:maxPageNumber\" content=\"0\"/>");
            builderToc1.AppendLine("\t</head>");
            builderToc1.AppendLine("\t<docTitle>");
            builderToc1.AppendLine("\t\t<text></text>");
            builderToc1.AppendLine("\t</docTitle>");
            builderToc1.AppendLine("\t<docAuthor>");
            builderToc1.AppendLine("\t\t<text></text>");
            builderToc1.AppendLine("\t</docAuthor>");
            builderToc1.AppendLine("\t<navMap>");

            builderToc2 = new StringBuilder();
            builderToc2.AppendLine("\t</navMap>");
            builderToc2.AppendLine("</ncx>");

            builderChapter1 = new StringBuilder();
            builderChapter1.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builderChapter1.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">");
            builderChapter1.AppendLine("\t<head>");
            builderChapter1.AppendLine("\t\t<title></title>");
            builderChapter1.AppendLine("\t\t<link rel=\"stylesheet\" href=\"C1.css\" type=\"text/css\"/>");
            builderChapter1.AppendLine("\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=utf-8\"/>");
            builderChapter1.AppendLine("\t\t<meta name=\"EPB-UUID\" content=\"\"/>");
            builderChapter1.AppendLine("\t</head>");
            builderChapter1.AppendLine("\t<body>");

            builderChapter2 = new StringBuilder();
            builderChapter2.AppendLine("\t</body>");
            builderChapter2.AppendLine("</html>");
        }

        private void Cleanup()
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(compilePath);
                zip.Save(Data.OutputFolder + Data.OutputFile);
            }

            Directory.Delete(compilePath, true);
        }

        #endregion

        #region Image Treatment

        private void SaveImage(Bitmap imageOriginal, ref int imageIndex, string imagePath)
        {
            int width = (Int32)Math.Round((double)(Data.Height * imageOriginal.Width / imageOriginal.Height), 0, MidpointRounding.AwayFromZero);
            int theoreticalWidth = (Int32)Math.Round((double)(Data.Height * 0.75), 0, MidpointRounding.AwayFromZero);

            if (width / Data.Height > 0.75)
            {
                errors.Add(imagePath);
                return;
            }

            using (Bitmap grayedImage = GrayImage(imageOriginal, Data.Grayscale))
            {
                using (Bitmap trimmedImage = TrimImage(grayedImage, Data.Trimming, Data.Grayscale))
                {
                    using (Bitmap completedImage = new Bitmap(theoreticalWidth, Data.Height))
                    {
                        using (Graphics g = Graphics.FromImage((System.Drawing.Image)completedImage))
                        {
                            width = (Int32)Math.Round((double)(Data.Height * trimmedImage.Width / trimmedImage.Height), 0, MidpointRounding.AwayFromZero);

                            g.FillRectangle(new SolidBrush(Color.White), 0, 0, theoreticalWidth, Data.Height);
                            g.DrawImage(trimmedImage, (float)((theoreticalWidth - width) * 0.65), 0, width, Data.Height);
                        }

                        completedImage.Save(imagesFolderPath + "I" + imageIndex.ToString() + ".jpg", codec, parameters);
                    }
                }
            }

            builderContent1.AppendLine("\t\t<item xmlns=\"\" id=\"P" + imageIndex.ToString() + "\" href=\"P" + imageIndex.ToString() + ".xml\" media-type=\"application/xhtml+xml\"/>");
            builderContent2.AppendLine("\t\t<itemref xmlns=\"\" idref=\"P" + imageIndex.ToString() + "\" linear=\"yes\"/>");
            builderToc1.AppendLine("\t\t<navPoint xmlns=\"\" id=\"N" + imageIndex.ToString() + "\" playOrder=\"" + imageIndex.ToString() + "\"><navLabel><text>P" + imageIndex.ToString() + "</text></navLabel><content src=\"P" + imageIndex.ToString() + ".xml\"/></navPoint>");

            StringBuilder chapter = new StringBuilder();
            chapter.Append(builderChapter1.ToString());
            chapter.AppendLine("\t\t<img style=\"margin:0\" src=\"Images/I" + imageIndex.ToString() + ".jpg\" />");
            chapter.Append(builderChapter2.ToString());

            StreamWriter chapterWriter = new StreamWriter(oebpsFolderPath + "P" + imageIndex.ToString() + ".xml", false);
            chapterWriter.Write(chapter.ToString());
            chapterWriter.Close();

            imageIndex++;
        }

        private Bitmap GrayImage(Bitmap originalImage, bool toGrayscale)
        {
            if (toGrayscale)
            {
                Bitmap grayedImage = new Bitmap(originalImage.Width, originalImage.Height);
                using (Graphics g = Graphics.FromImage(grayedImage))
                {
                    g.DrawImage(originalImage, new Rectangle(0, 0, originalImage.Width, originalImage.Height), 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                return grayedImage;
            }
            else
            {
                return originalImage;
            }
        }

        private Bitmap TrimImage(Bitmap originalImage, bool toTrim, bool isGrayed)
        {
            if (toTrim)
            {
                using (Bitmap grayedImage = GrayImage(originalImage, !isGrayed))
                {
                    int startX = -2;
                    int startY = -2;
                    int endX = -2;
                    int endY = -2;

                    for (int i = 0; i < grayedImage.Width; i++)
                    {
                        for (int j = 0; j < grayedImage.Height; j++)
                        {
                            Color orgColor = grayedImage.GetPixel(i, j);
                            if ((orgColor.R < Data.TrimmingValue) || (orgColor.G < Data.TrimmingValue) || (orgColor.B < Data.TrimmingValue))
                            {
                                startX = i - 1;
                                break;
                            }
                        }

                        if (startX != -2) break;
                    }

                    for (int j = 0; j < grayedImage.Height; j++)
                    {
                        for (int i = 0; i < grayedImage.Width; i++)
                        {
                            Color orgColor = grayedImage.GetPixel(i, j);
                            if ((orgColor.R < Data.TrimmingValue) || (orgColor.G < Data.TrimmingValue) || (orgColor.B < Data.TrimmingValue))
                            {
                                startY = j - 1;
                                break;
                            }
                        }

                        if (startY != -2) break;
                    }

                    for (int i = grayedImage.Width - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < grayedImage.Height; j++)
                        {
                            Color orgColor = grayedImage.GetPixel(i, j);
                            if ((orgColor.R < Data.TrimmingValue) || (orgColor.G < Data.TrimmingValue) || (orgColor.B < Data.TrimmingValue))
                            {
                                endX = i + 1;
                                break;
                            }
                        }

                        if (endX != -2) break;
                    }

                    for (int j = grayedImage.Height - 1; j >= 0; j--)
                    {
                        for (int i = 0; i < grayedImage.Width; i++)
                        {
                            Color orgColor = grayedImage.GetPixel(i, j);
                            if ((orgColor.R < Data.TrimmingValue) || (orgColor.G < Data.TrimmingValue) || (orgColor.B < Data.TrimmingValue))
                            {
                                endY = j + 1;
                                break;
                            }
                        }

                        if (endY != -2) break;
                    }

                    if (startX < 0) startX = 0;
                    if (startY < 0) startY = 0;
                    if (endX < 0) endX = 0;
                    if (endY < 0) endY = 0;

                    return originalImage.Clone(new RectangleF(startX, startY, endX - startX, endY - startY), originalImage.PixelFormat);
                }
            }
            else
            {
                return originalImage;
            }
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
    }
}
