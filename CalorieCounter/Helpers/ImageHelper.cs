﻿using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System.Drawing;
namespace CalorieCounter.Helpers
{
    public class ImageHelper
    {
        public static void CropBoundingBox(string inputPath, BoundingBox boundingBox, string outputPath)
        {
            using (var stream = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Bitmap src = System.Drawing.Image.FromStream(stream) as Bitmap;

                Rectangle cropRectangle = GetRectangle(boundingBox, src.Width, src.Height);
                EnsureWidthAndHeight(src, ref cropRectangle);

                Bitmap croppedBitmap = src.Clone(cropRectangle, src.PixelFormat);
                croppedBitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static Rectangle GetRectangle(BoundingBox boundingBox, int width, int height)
        {
            return new Rectangle(
                (int)Math.Round(boundingBox.Left * width),
                (int)Math.Round(boundingBox.Top * height),
                (int)Math.Round(boundingBox.Width * width),
                (int)Math.Round(boundingBox.Height * height));
        }

        private const int MIN_WIDTH = 50;
        private const int MIN_HEIGHT = 50;

        private static void EnsureWidthAndHeight(Bitmap src, ref Rectangle rectangle)
        {
            if (src.Width < MIN_WIDTH || src.Height < MIN_HEIGHT)
            {
                throw new Exception("Image does not meet the minimum width and height requirements");
            }

            if (rectangle.Width < MIN_WIDTH)
            {
                var missingWidth = MIN_WIDTH - rectangle.Width;
                rectangle.X = Math.Max(0, rectangle.X - missingWidth);
                rectangle.Width = MIN_WIDTH;
            }

            if (rectangle.Height < MIN_HEIGHT)
            {
                var missingHeight = MIN_HEIGHT - rectangle.Height;
                rectangle.Y = Math.Max(0, rectangle.Y - missingHeight);
                rectangle.Height = MIN_HEIGHT;
            }
        }
    }
}
