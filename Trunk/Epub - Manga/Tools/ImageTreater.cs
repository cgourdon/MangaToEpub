﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EpubManga
{
    public class ImageTreater
    {
        #region Singleton

        private static ImageTreater instance;
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

        public Bitmap TreatImage(Bitmap originalImage, int height, bool grayscale, bool trimming, int trimmingValue, double leftMargin)
        {
            int theoreticalWidth = (Int32)Math.Round((double)(height * 0.75), 0, MidpointRounding.AwayFromZero);

            Bitmap treatedImage = new Bitmap(theoreticalWidth, height);

            using (Bitmap grayedImage = GrayImage(originalImage, grayscale))
            {
                using (Bitmap trimmedImage = TrimImage(grayedImage, trimming, grayscale, trimmingValue))
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

        private Bitmap TrimImage(Bitmap originalImage, bool trimming, bool isGrayed, int trimmingValue)
        {
            if (trimming)
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
    }
}
