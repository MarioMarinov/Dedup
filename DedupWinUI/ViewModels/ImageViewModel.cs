﻿using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Services;
using Services.Models;
using Shipwreck.Phash.Bitmaps;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Windows.Graphics.Imaging;


namespace DedupWinUI.ViewModels
{
    public class ImageViewModel : BaseViewModel
    {
        public DateTime CreationTime { get; private set; }
        public string FileName { get { return Model.FileName; } }
        public string FileExt { get { return Path.GetExtension(Model.FileName); } }
        public string FullName { get { return Model.FilePath; } }
        public string FilePath { get { return Model.FilePath; } }
        public long Length { get { return Model.Length; } }
        public Size Dimensions { get; private set; }

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                RaisePropertyChanged(nameof(Thumbnail));
            }
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        public Bitmap HashImage { get; set; }

        private ImageModel _model;
        public ImageModel Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    _model = value;
                    RaisePropertyChanged(nameof(Model));
                }
            }
        }

        public ImageViewModel(ImageModel model)
        {
            Model = model;
            var fi = new FileInfo(FullName);
            CreationTime = fi.CreationTime;
            using (var bmp = new Bitmap(model.ThumbnailSource))
            {
                Thumbnail = ConvertBitmapToBitmapSource(bmp);
            }
            using (var bmp = new Bitmap(model.FilePath)) {
                Image = ConvertBitmapToBitmapSource(bmp);
                Dimensions = new Size(bmp.Width, bmp.Height);
            }
            
        }

        public BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            var exifOrientation = ImagingService.GetExifOrientation(bitmap);
            bitmap = ImagingService.AdjustBitmapOrientation(bitmap, exifOrientation);
            using MemoryStream memoryStream = new();
            // Save the bitmap to the memory stream
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            // Create a new BitmapImage
            BitmapImage bitmapImage = new();
            bitmapImage.SetSource(memoryStream.AsRandomAccessStream());

            return bitmapImage;
        }
    }
}
