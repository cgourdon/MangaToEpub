using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EpubManga
{
    /// <summary>
    /// Treat given images, mainly grayscaling and trimming.
    /// </summary>
    public class ImageTreater
    {
        #region Singleton

        private static ImageTreater instance;

        /// <summary>
        /// Returns the single instance of the ImageTreater class.
        /// </summary>
        /// <returns>A single instance of the ImageTreater class.</returns>
        public static ImageTreater GetInstance()
        {
            lock (typeof(ImageTreater))
            {
                if (instance == null)
                {
                    instance = new ImageTreater();
                }
            }
            return instance;
        }

        private ImageTreater()
        {
            Initialize();
        }

        #endregion


        #region Data

        private ImageAttributes imageAttributes;
        
        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the color matrix needed to gray an image.
        /// </summary>
        private void Initialize()
        {
            imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(new ColorMatrix(new float[][] 
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                }));
        }

        #endregion


        #region Image Treatment

        /// <summary>
        /// Returns 1 or 2 images depending on the given DoublePage value.
        /// If DoublePage is RotateLeft or RotateRigh, it will perform the necessary rotation and returns a list containing only the rotated image.
        /// Otherwise, 2 images will be returned, Offset allowing to specify if one of them need to be larger than the other one.
        /// Of Offsset is positive, the left part of the image will be larger, the right part otherwise.
        /// </summary>
        /// <param name="image">The image that need to be treated.</param>
        /// <param name="doublePage">The type of DoublePage treatment requested.</param>
        /// <param name="offset">Specifies if the left page (positive value) or right page (negative value) must be larger than the other one.</param>
        /// <returns>A list of 1 or 2 images depending on the given DoublePage value.</returns>
        public List<Bitmap> HandleDoublePage(Bitmap image, DoublePage doublePage, int offset)
        {
            List<Bitmap> result = new List<Bitmap>();

            if (image.Width > image.Height)
            {
                if (doublePage == DoublePage.RotateLeft)
                {
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    result.Add(image);
                }
                else if (doublePage == DoublePage.RotateRight)
                {
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    result.Add(image);
                }
                else
                {
                    int firstStart;
                    int firstWidth;
                    int secondStart;
                    int secondWidth;

                    switch (doublePage)
                    {
                        case DoublePage.LeftPageFirst:
                            firstStart = 0;
                            firstWidth = image.Width / 2 + offset;
                            secondStart = image.Width / 2 + offset;
                            secondWidth = image.Width / 2 - offset;
                            break;
                        case DoublePage.RightPageFirst:
                            firstStart = image.Width / 2 + offset;
                            firstWidth = image.Width / 2 - offset;
                            secondStart = 0;
                            secondWidth = image.Width / 2 + offset;
                            break;
                        default:
                            firstStart = 0;
                            firstWidth = 0;
                            secondStart = 0;
                            secondWidth = 0;
                            break;
                    }

                    Bitmap image1 = image.Clone(new RectangleF(firstStart, 0, firstWidth, image.Height), image.PixelFormat);
                    result.Add(image1);

                    Bitmap image2 = image.Clone(new RectangleF(secondStart, 0, secondWidth, image.Height), image.PixelFormat);
                    result.Add(image2);
                }
            }
            else
            {
                result.Add(image);
            }

            return result;
        }

        /// <summary>
        /// Perform the full treatment on an image, that is graying, trimming and resizing, and returns the result.
        /// </summary>
        /// <param name="originalImage">The image that need to be treated.</param>
        /// <param name="height">The height of the treated image.</param>
        /// <param name="grayscale">Does the image need to be grayed.</param>
        /// <param name="trimming">Does the image need to be trimmed.</param>
        /// <param name="trimmingValue">Between 0 and 255, the closer to 0, the more will be trimmed.</param>
        /// <param name="leftMargin">Between 0 and 1, the width of the left margin, 0.5 in order to have the left and right margin equals.</param>
        /// <param name="trimmingMethod">The trimming method to be used.</param>
        /// <returns>An image grayed, trimmed and resized according to the given parameters.</returns>
        public Bitmap TreatImage(Bitmap originalImage, int height, bool grayscale, bool trimming, int trimmingValue, double leftMargin, TrimmingMethod trimmingMethod)
        {
            int theoreticalWidth = (Int32)Math.Round((double)(height * 0.75), 0, MidpointRounding.AwayFromZero);

            Bitmap treatedImage = new Bitmap(theoreticalWidth, height);

            using (Bitmap grayedImage = GrayImage(originalImage, grayscale))
            {
                using (Bitmap trimmedImage = TrimImage(grayedImage, trimming, grayscale, trimmingValue, trimmingMethod))
                {
                    using (Graphics g = Graphics.FromImage((System.Drawing.Image)treatedImage))
                    {
                        int width = (Int32)Math.Round((double)(height * trimmedImage.Width / trimmedImage.Height), 0, MidpointRounding.AwayFromZero);

                        g.FillRectangle(new SolidBrush(Color.White), 0, 0, theoreticalWidth, height);
                        g.DrawImage(trimmedImage, (float)((theoreticalWidth - width) * leftMargin), 0, width, height);
                    }
                }
            }

            return treatedImage;
        }

        /// <summary>
        /// Returns a grayed image if Grayscale is true, the original image otherwise.
        /// </summary>
        /// <param name="originalImage">The image that need to be grayed.</param>
        /// <param name="grayscale">Does the image need to be grayed.</param>
        /// <returns>A grayed image if Grayscale is true, the original image otherwise.</returns>
        private Bitmap GrayImage(Bitmap originalImage, bool grayscale)
        {
            if (grayscale)
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

        /// <summary>
        /// Returns a trimmed image according to the given parameters if Trimming is true, the original image otherwise.
        /// </summary>
        /// <param name="originalImage">The image that need to be trimmed.</param>
        /// <param name="trimming">Does the image need to be trimmed.</param>
        /// <param name="isGrayed">Is the image grayed ?</param>
        /// <param name="trimmingValue">Between 0 and 255, the closer to 0, the more will be trimmed.</param>
        /// <param name="trimmingMethod">The trimming method to be used.</param>
        /// <returns>A trimmed image according to the given parameters if Trimming is true, the original image otherwise.</returns>
        private Bitmap TrimImage(Bitmap originalImage, bool trimming, bool isGrayed, int trimmingValue, TrimmingMethod trimmingMethod)
        {
            if (trimming)
            {
                using (Bitmap grayedImage = GrayImage(originalImage, !isGrayed))
                {
                    int startX = -2;
                    int startY = -2;
                    int endX = -2;
                    int endY = -2;

                    switch (trimmingMethod)
                    {
                        case TrimmingMethod.Absolute:
                            AbsoluteTrimming(grayedImage, trimmingValue, ref startX, ref startY, ref endX, ref endY);
                            break;
                        case TrimmingMethod.Average:
                            AverageTrimming(grayedImage, trimmingValue, ref startX, ref startY, ref endX, ref endY);
                            break;
                    }

                    if (startX < 0) startX = 0;
                    if (startY < 0) startY = 0;
                    if (endX < 0) endX = 0;
                    if (endY < 0) endY = 0;
                    if (startX > endX) startX = endX;
                    if (startY > endY) startY = endY;

                    return originalImage.Clone(new RectangleF(startX, startY, endX - startX, endY - startY), originalImage.PixelFormat);
                }
            }
            else
            {
                return originalImage;
            }
        }

        /// <summary>
        /// Calculate the rectangle that must be kept on the given image in the case of an Absolute trimming.
        /// The Absolute trimming will stop looking at the first pixel whose color is below the trimming value.
        /// Thus making an isolated black pixel stoping the trimming but with no risk of erasing useful part of the image.
        /// </summary>
        /// <param name="grayedImage">The image that need to be trimmed according to the absolute method.</param>
        /// <param name="trimmingValue">Between 0 and 255, the closer to 0, the more will be trimmed.</param>
        private void AbsoluteTrimming(Bitmap grayedImage, int trimmingValue, ref int startX, ref int startY, ref int endX, ref int endY)
        {
            for (int i = 0; i < grayedImage.Width; i++)
            {
                for (int j = 0; j < grayedImage.Height; j++)
                {
                    Color orgColor = grayedImage.GetPixel(i, j);
                    if ((orgColor.R < trimmingValue) || (orgColor.G < trimmingValue) || (orgColor.B < trimmingValue))
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
                    if ((orgColor.R < trimmingValue) || (orgColor.G < trimmingValue) || (orgColor.B < trimmingValue))
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
                    if ((orgColor.R < trimmingValue) || (orgColor.G < trimmingValue) || (orgColor.B < trimmingValue))
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
                    if ((orgColor.R < trimmingValue) || (orgColor.G < trimmingValue) || (orgColor.B < trimmingValue))
                    {
                        endY = j + 1;
                        break;
                    }
                }

                if (endY != -2) break;
            }
        }

        /// <summary>
        /// Calculate the rectangle that must be kept on the given image in the case of an Average trimming.
        /// The Average trimming will perform an average of the color encountered on a given line or column of pixel.
        /// If this average is lower than TrimmingValue, the trimming will stop.
        /// This method allow to bypass any isolated black pixel that should not have been here but is very agressive and might cause the loss of useful part of the image.
        /// </summary>
        /// <param name="grayedImage">The image that need to be trimmed according to the Average method.</param>
        /// <param name="trimmingValue">Between 0 and 255, the closer to 0, the more will be trimmed.</param>
        private void AverageTrimming(Bitmap grayedImage, int trimmingValue, ref int startX, ref int startY, ref int endX, ref int endY)
        {
            int localTrimmingValue = trimmingValue + (Int32)Math.Round(((256 - trimmingValue) * 0.5), MidpointRounding.AwayFromZero);

            for (int i = 0; i < grayedImage.Width; i++)
            {
                int sum = 0;

                for (int j = 0; j < grayedImage.Height; j++)
                {
                    Color orgColor = grayedImage.GetPixel(i, j);
                    sum += orgColor.R;
                    sum += orgColor.G;
                    sum += orgColor.B;
                }

                if (sum / 3 / grayedImage.Height < localTrimmingValue)
                {
                    startX = i - 1;
                    break;
                }
            }

            for (int j = 0; j < grayedImage.Height; j++)
            {
                int sum = 0;

                for (int i = 0; i < grayedImage.Width; i++)
                {
                    Color orgColor = grayedImage.GetPixel(i, j);
                    sum += orgColor.R;
                    sum += orgColor.G;
                    sum += orgColor.B;
                }

                if (sum / 3 / grayedImage.Width < localTrimmingValue)
                {
                    startY = j - 1;
                    break;
                }
            }

            for (int i = grayedImage.Width - 1; i >= 0; i--)
            {
                int sum = 0;

                for (int j = 0; j < grayedImage.Height; j++)
                {
                    Color orgColor = grayedImage.GetPixel(i, j);
                    sum += orgColor.R;
                    sum += orgColor.G;
                    sum += orgColor.B;
                }

                if (sum / 3 / grayedImage.Height < localTrimmingValue)
                {
                    endX = i + 1;
                    break;
                }
            }

            for (int j = grayedImage.Height - 1; j >= 0; j--)
            {
                int sum = 0;

                for (int i = 0; i < grayedImage.Width; i++)
                {
                    Color orgColor = grayedImage.GetPixel(i, j);
                    sum += orgColor.R;
                    sum += orgColor.G;
                    sum += orgColor.B;
                }

                if (sum / 3 / grayedImage.Width < localTrimmingValue)
                {
                    endY = j + 1;
                    break;
                }
            }
        }

        #endregion
    }
}
