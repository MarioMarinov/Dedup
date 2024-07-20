
using Microsoft.Extensions.Options;
using Services.Models;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Services
{
    public class ImagingService : IImagingService
    {
        private IFileService _fileService;
        private AppSettings _settings;

        public ImagingService(IOptions<AppSettings> settings, IFileService fileService)
        {
            _settings = settings.Value;
            _fileService = fileService;
        }

        public async Task<Image> CreateThumbnailAsync(string fileName, int maxSide)
        {
            try
            {
                using (var originalImage = Image.FromFile(fileName))
                {
                    var thumbnailSize = GetThumbnailSize(originalImage, maxSide);
                    var resizedImage = new Bitmap(thumbnailSize.Width, thumbnailSize.Height);

                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        await Task.Run(() => graphics.DrawImage(originalImage, 0, 0, thumbnailSize.Width, thumbnailSize.Height));
                    }
                    return resizedImage;
                }
            }
            catch { }
            return default(Bitmap);
        }
        
        static Size GetThumbnailSize(Image image, int maxSide)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            var factor = (double)maxSide / Math.Max(originalWidth, originalHeight);
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

        public async Task<List<ImageModel>> GetImageModelsAsync(IEnumerable<string> fileNames)
        {
            var res = new ConcurrentBag<ImageModel>();
            var failed = new ConcurrentBag<string>();
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            await Parallel.ForEachAsync(fileNames, parallelOptions, async (fname, token) =>
            {
                var imageModel = await HandleFile(fname, token);
                if (!string.IsNullOrEmpty(imageModel.ThumbnailSource))
                    res.Add(imageModel);
                else
                {
                    failed.Add(fname);
                }
            });
            return res.OrderBy(f => f.FilePath).ThenBy(f => f.FileName).ToList();
        }

        /// <summary>
        /// Traverse the thumbnails db path
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public async Task<List<ImageModel>> GetCachedModelsAsync(IEnumerable<string> fileNames)
        {
            var res = new ConcurrentBag<ImageModel>();
            var failed = new ConcurrentBag<string>();
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            await Parallel.ForEachAsync(fileNames, parallelOptions, async (fname, token) =>
            {
                var imageModel = await HandleCachedModel(fname, token);
                if (!string.IsNullOrEmpty(imageModel.ThumbnailSource))
                    res.Add(imageModel);
                else
                {
                    failed.Add(fname);
                }
            });
            return res.OrderBy(f => f.FilePath).ThenBy(f => f.FileName).ToList();
        }

        public async Task<ImageModel> HandleCachedModel(string filePath, CancellationToken token)
        {
            var sourcePath = _fileService.ConvertThumbnailPathToSourcePath(filePath);
            var model = new ImageModel(sourcePath);
            model.ThumbnailSource = filePath;
            return await Task.FromResult(model);
        }

        public async Task<ImageModel> HandleFile(string filePath, CancellationToken token)
        {
            var model = new ImageModel(filePath);
            var th = await CreateThumbnailAsync(filePath, _settings.ThumbnailSize);
            if (th != null)
            {
                var thSrc = await _fileService.InsertThumbnailToDbAsync(th, filePath);
                model.ThumbnailSource = thSrc;
            }
            return await Task.FromResult(model);
        }

        public void GetImageHash(string filePath)
        {
            var img = Image.FromFile(filePath);
            var width = img.Width;
            var height = img.Height;
            
        }

        private Image ToGrayScale(Image image)
        {
            // Make the ColorMatrix.
            ColorMatrix cm = new ColorMatrix(
            [
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] { 0, 0, 0, 1, 0},
                new float[] { 0, 0, 0, 0, 1}
            ]);
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            // Draw the image onto the new bitmap while
            // applying the new ColorMatrix.
            Point[] points =
            {
                new Point(0, 0),
                new Point(image.Width, 0),
                new Point(0, image.Height),
            };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            // Make the result bitmap.
            Bitmap bm = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return bm;
        }

        public BitmapImage ResizeAndGrayout(string fileName, int maxSide)
        {
            ///// Create a BitmapImage and set it's DecodePixelWidth to 200. Use  /////
            ///// this BitmapImage as a source for other BitmapSource objects.    /////

            BitmapImage myBitmapImage = new BitmapImage();//new Uri(fileName)

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

            //getting a bitmap
            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            //    enc.Save(outStream);
            //    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
            //    return new Bitmap(bitmap);
            //}
            return bitmapImage;
        }
    }

}
