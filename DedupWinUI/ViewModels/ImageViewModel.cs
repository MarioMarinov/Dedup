using Services.Models;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DedupWinUI.ViewModels
{
    public class ImageViewModel : BaseViewModel
    {
        public DateTime CreationTime { get; private set; }
        public string FileName { get { return Model.FileName; } }
        public string FileExt { get { return Path.GetExtension(Model.FileName); } }
        public string FullName { get { return Path.Combine(FilePath, FileName); } }
        public string FilePath { get { return Model.FilePath; } }
        public long Length { get { return Model.Length; } }
        public Size Dimensions { get; private set; }
        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                RaisePropertyChanged(nameof(Thumbnail));
                //var enc = new PngBitmapEncoder();
                //enc.Frames.Add(BitmapFrame.Create(value));
                //var fileStream = new FileStream(@"D:\Temp\ThumbailTest.png", FileMode.Create);
                //enc.Save(fileStream);
                //fileStream.Close();
            }
        }


        public BitmapSource HashImage { get; set; }

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
            var img = Image.FromFile(fi.FullName);
            Dimensions = new Size(img.Width, img.Height);
        }

    }
}
