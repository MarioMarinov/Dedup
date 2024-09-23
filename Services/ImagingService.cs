﻿
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Media.Imaging;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System.Drawing;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;


namespace Services
{
    public class ImagingService : IImagingService
    {

        //public ImagingService(IOptions<AppSettings> settings, IFileService fileService)
        //{
        //}

        public static async Task<SoftwareBitmap> BmpToSBmp(Bitmap bmp)
        {
            await Task.Yield();
            SoftwareBitmap sBmp;
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format8bppIndexed);
            var length = bmpData.Stride * bmpData.Height;
            var bytes = new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, length);
            sBmp = new SoftwareBitmap(BitmapPixelFormat.Bgra8, bmp.Width, bmp.Height);
            if (sBmp.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                sBmp.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                sBmp = SoftwareBitmap.Convert(sBmp, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            
            return sBmp;
        }
        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                // Create a new BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(memoryStream.AsRandomAccessStream());

                return bitmapImage;
            }
        }

        
        public static async Task<Bitmap?> ResizeBitmapAsync(string fileName, int maxSide)
        {
            return await Task.Run(() =>
            {
                var th = default(Bitmap);
                try
                {
                    using (var bmp = new Bitmap(fileName))
                    {
                        var thSize = GetTransformation(bmp.Width, bmp.Height, maxSide);
                        th = new Bitmap(bmp, thSize.Width, thSize.Height);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resizing bitmap: {ex.Message}");
                }
                return th;
            });
        }

        public static async Task<Bitmap?> ResizeBitmapAsync(Bitmap bmp, int maxSide)
        {
            return await Task.Run(() =>
            {
                var th = default(Bitmap);
                try
                {
                    var thSize = GetTransformation(bmp.Width, bmp.Height, maxSide);
                    th = new Bitmap(bmp, thSize.Width, thSize.Height);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resizing bitmap: {ex.Message}");
                }
                return th;
            });  
        }

        static (int Width, int Height, double Scale) GetTransformation(int originalWidth, int originalHeight, int thumbnailSide)
        {
            var scale = (double)thumbnailSide / Math.Max(originalWidth, originalHeight);
            var size = new Size((int)(originalWidth * scale), (int)(originalHeight * scale));
            return (size.Width, size.Height, scale);
        }

        private Digest GetImageDigest(string filePath)
        {
            var img = (Bitmap)Image.FromFile(filePath);
            var hash = ImagePhash.ComputeDigest(img.ToLuminanceImage());
            return hash;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
            new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 },
            new float[] { 0, 0, 0, 0, 1 }
                });

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return newBitmap;
        }
        /*
        public BitmapImage ResizeAndGrayout(string fileName, int maxSide)

        {
            ///// Create a BitmapImage and set it's DecodePixelWidth to maxSide. Use  /////
            ///// this BitmapImage as a source for other BitmapSource objects.    /////

            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapSource objects like BitmapImage can only have their properties
            // changed within a BeginInit/EndInit block.
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(fileName); //, UriKind.Relative);

            // To save significant application memory, set the DecodePixelWidth or
            // DecodePixelHeight of the BitmapImage value of the image source to the desired
            // height or width of the rendered image. If you don't do this, the application will
            // cache the image as though it were rendered as its normal size rather than just
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.
            myBitmapImage.DecodePixelWidth = maxSide;
            myBitmapImage.EndInit();

            ////////// Convert the BitmapSource to a new format ////////////
            // Use the BitmapImage created above as the source for a new BitmapSource object
            // which is set to a gray scale format using the FormatConvertedBitmap BitmapSource.
            // Note: New BitmapSource does not cache. It is always pulled when required.

            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            newFormatedBitmapSource.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            newFormatedBitmapSource.Source = myBitmapImage;

            // Set the new format to Gray32Float (grayscale).
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            newFormatedBitmapSource.EndInit();

            var bitmapImage = new BitmapImage();
            var encoder = new PngBitmapEncoder();
            using (var memoryStream = new MemoryStream())
            {
                
                encoder.Frames.Add(BitmapFrame.Create(newFormatedBitmapSource));
                encoder.Save(memoryStream);
                
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }

            //save to file
            //var enc = new PngBitmapEncoder();
            //using (var fileStream = new FileStream(@"D:\Temp\ThumbailTest.png", FileMode.Create))
            //{
            //    enc.Frames.Add(BitmapFrame.Create(newFormatedBitmapSource));
            //    enc.Save(fileStream);
            //}
            //-
            //System.Drawing.Bitmap bitmap;
            //using (var memStream = new MemoryStream())
            //{
            //    enc.Frames.Add(BitmapFrame.Create(newFormatedBitmapSource));
            //    enc.Save(memStream);
            //    bitmap = new System.Drawing.Bitmap(memStream);
            //    memStream.Close();
            //}
            //-
            //getting a bitmap
            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            //    enc.Save(outStream);
            //    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
            //    return new Bitmap(bitmap);
            //}
            //return bitmap;
            //-
            return bitmapImage;
        }
        */
    }

}