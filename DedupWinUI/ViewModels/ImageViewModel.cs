using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Services.Models;
using System;
using System.Drawing;
using System.IO;
using Windows.Graphics.Imaging;


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
            var img = Image.FromFile(fi.FullName);
            Dimensions = new Size(img.Width, img.Height);
        }

    }
}
