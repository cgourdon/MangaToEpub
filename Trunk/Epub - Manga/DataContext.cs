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

        private List<string> allowedExtensions;
        private List<string> allowedImageExtensions;
        private List<string> allowedZipExtensions = new List<string>() { ".zip", ".cbz" };

        private string fileDialogFilter;

        private EncoderParameters parameters;
        private ImageCodecInfo codec;

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
        private List<string> tempDirectories = new List<string>();

        private BackgroundWorker worker;

        private List<string> pathErrors;
        private List<string> zipErrors;
        private List<string> imageErrors;
        private List<string> ratioErrors;

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


            DisplayPreviewButton = true;
            IsBusy = false;
            InitializeCommands();


            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;


            parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 1);
            codec = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType.Contains("jpeg")).FirstOrDefault();


            allowedImageExtensions = new List<string>();
            var decoders = ImageCodecInfo.GetImageDecoders();
            foreach (var decoder in decoders)
            {
                var extensions = decoder.FilenameExtension.Split(';');
                foreach (var extension in extensions)
                {
                    allowedImageExtensions.Add(extension.Trim('*').ToLower());
                }
            }

            allowedExtensions = new List<string>();
            allowedExtensions.AddRange(allowedImageExtensions);
            allowedExtensions.AddRange(allowedZipExtensions);
            allowedExtensions = allowedExtensions.OrderBy(e => e).ToList();
            
            fileDialogFilter = "All Allowed Files|";
            bool first = true;
            foreach (string extension in allowedExtensions)
            {
                if (first) first = false;
                else fileDialogFilter += ";";
                fileDialogFilter += String.Format("*{0}", extension);
            }

            fileDialogFilter += "|Images Only|";
            first = true;
            foreach (string extension in allowedImageExtensions)
            {
                if (first) first = false;
                else fileDialogFilter += ";";
                fileDialogFilter += String.Format("*{0}", extension);
            }

            fileDialogFilter += "|Zip Archives Only|";
            first = true;
            foreach (string extension in allowedZipExtensions)
            {
                if (first) first = false;
                else fileDialogFilter += ";";
                fileDialogFilter += String.Format("*{0}", extension);
            }
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
        public Command ShowPreviewCommand { get; private set; }

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

            ShowPreviewCommand = new Command()
            {
                CanExecuteDelegate = (obj) => ShowPreviewCommandCanExecute(),
                ExecuteDelegate = (obj) => ShowPreviewCommandExecute()
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
            ofd.Filter = fileDialogFilter;
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Data.Files = ofd.FileNames.Where(f =>
                    {
                        if (string.IsNullOrEmpty(f)) return false;

                        FileInfo file = new FileInfo(f);
                        if (allowedExtensions.Contains(file.Extension.ToLower())) return true;

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
                    List<FileInfo> files = new List<FileInfo>();
                    GetDirectoryFiles(files, new DirectoryInfo(lastSelectedFolder));
                    files = files.OrderBy(f => f.FullName, new FileNameComparer()).ToList();

                    List<string> paths = new List<string>();
                    foreach (FileInfo file in files)
                    {
                        if (allowedExtensions.Contains(file.Extension.ToLower()))
                        {
                            paths.Add(file.FullName);
                        }
                    }
                    Data.Files = paths;
                }
            }
        }

        private void GetDirectoryFiles(List<FileInfo> files, DirectoryInfo directory)
        {
            files.AddRange(directory.GetFiles());

            foreach (DirectoryInfo subDirecdtory in directory.GetDirectories())
            {
                GetDirectoryFiles(files, subDirecdtory);
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

        #region ShowPreviewCommand

        private bool ShowPreviewCommandCanExecute()
        {
            return ((!IsBusy)
                && (Data.Files != null)
                && (Data.Files.Count > 0));
        }

        private void ShowPreviewCommandExecute()
        {
            new PreviewWindow(Data).ShowDialog();
        }

        #endregion

        #endregion


        #region Generate

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitializeWorkspace();
            InitializeBuilders();

            CheckSelectedFiles();
            ProcessImages();

            Cleanup();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            if (pathErrors.Count + zipErrors.Count + imageErrors.Count + ratioErrors.Count > 0)
            {
                StringBuilder builder = new StringBuilder();

                if (pathErrors.Count > 0)
                {
                    builder.AppendLine("The following files have been ignored because they do not exist:");
                    pathErrors.ForEach(s => builder.AppendLine(s));
                    builder.AppendLine();
                }

                if (zipErrors.Count > 0)
                {
                    builder.AppendLine("The following archives have been ignored because they could not be opened as zip archives:");
                    zipErrors.ForEach(s => builder.AppendLine(s));
                    builder.AppendLine();
                }

                if (imageErrors.Count > 0)
                {
                    builder.AppendLine("The following files have been ignored because they could not be opened as images:");
                    imageErrors.ForEach(s => builder.AppendLine(s));
                    builder.AppendLine();
                }

                if (ratioErrors.Count > 0)
                {
                    builder.AppendLine("The following images have been ignored because of their width / height ratio above 0.75:");
                    ratioErrors.ForEach(s => builder.AppendLine(s));
                    builder.AppendLine();
                }

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

            pathErrors = new List<string>();
            zipErrors = new List<string>();
            imageErrors = new List<string>();
            ratioErrors = new List<string>();

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

        private void CheckSelectedFiles()
        {
            List<string> paths = new List<string>(Data.Files);

            foreach (string path in paths)
            {
                FileInfo file = new FileInfo(path);
                if (!file.Exists)
                {
                    pathErrors.Add(path);
                    Data.Files.Remove(path);
                    continue;
                }

                if (!allowedZipExtensions.Contains(file.Extension.ToLower())) continue;

                try
                {
                    using (ZipFile zipFile = new ZipFile(path))
                    {
                        int pathIndex = Data.Files.IndexOf(path);
                        string zipPath = compilePath + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "") + "\\";
                        Directory.CreateDirectory(zipPath);
                        tempDirectories.Add(zipPath);
                        Data.Files.Remove(path);

                        List<string> newFiles = new List<string>();
                        foreach (ZipEntry entry in zipFile)
                        {
                            bool fileAllowed = false;
                            foreach (string extension in allowedImageExtensions)
                            {
                                if (entry.FileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    fileAllowed = true;
                                    break;
                                }
                            }

                            if (fileAllowed)
                            {
                                entry.Extract(zipPath, ExtractExistingFileAction.OverwriteSilently);
                                newFiles.Add(zipPath + entry.FileName.Replace("/", "\\"));
                            }
                        }

                        newFiles = newFiles.OrderBy(f => f, new FileNameComparer()).ToList();
                        foreach (string newFile in newFiles)
                        {
                            Data.Files.Insert(pathIndex, newFile);
                            pathIndex++;
                        }
                    }
                }
                catch
                {
                    zipErrors.Add(path);
                    Data.Files.Remove(path);
                }
            }

            TotalImages = Data.Files.Count;
        }

        private void ProcessImages()
        {
            int imageIndex = 1;
            foreach (string path in Data.Files)
            {
                Bitmap from = null;
                try
                {
                    from = new Bitmap(path);
                }
                catch
                {
                    imageErrors.Add(path);
                }
                if (from == null) continue;

                using (from)
                {
                    List<Bitmap> images = ImageTreater.GetInstance().HandleDoublePage(from, Data.DoublePage);
                    foreach (Bitmap image in images)
                    {
                        SaveImage(image, ref imageIndex, path);
                        image.Dispose();
                    }
                }

                TreatedImages++;
            }
        }

        private void SaveImage(Bitmap imageOriginal, ref int imageIndex, string imagePath)
        {
            if (imageOriginal.Width / imageOriginal.Height > 0.75)
            {
                ratioErrors.Add(imagePath);
                return;
            }

            using (Bitmap treatedImage = ImageTreater.GetInstance().TreatImage(imageOriginal, Data.Height, Data.Grayscale, Data.Trimming, Data.TrimmingValue))
            {
                treatedImage.Save(imagesFolderPath + "I" + imageIndex.ToString() + ".jpg", codec, parameters);
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

        private void Cleanup()
        {
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



            foreach (string path in tempDirectories)
            {
                Directory.Delete(path, true);
            }

            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(compilePath);
                zip.Save(Data.OutputFolder + Data.OutputFile);
            }

            Directory.Delete(compilePath, true);
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
