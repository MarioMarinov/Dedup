using DedupWinUI.Converters;
using Microsoft.UI.Xaml.Media;
using Services;
using Services.Models;
using System;
using System.Drawing;
using Windows.Gaming.Input;

namespace DedupWinUI.ViewModels
{
    /*
    public class FileInfoViewModel: BaseViewModel
    {
        private ImageSourceConverter _imageSourceConverter;

        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    RaisePropertyChanged(nameof(FullName));
                }
            }
        }

        private IFileInfoModel _model;
        public IFileInfoModel Model
        {
            get { return _model; }
            set
            {
                if (_model == value) return;
                _model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }
        public string DirectoryName
        {
            get { return Model.Info.DirectoryName; }
        }



        public long Length
        {
            get { return Model.Info.Length; }
        }

        public string Name
        {
            get { return Model.Info.Name; }
        }

        //public Image Thumbnail
        //{
        //    get 
        //    { 
        //        if (Model.Thumbnail == null)
        //        {
        //            //Model.Thumbnail = _imagingService.CreateThumbnail(FullName).Result;
        //        }
        //        return Model.Thumbnail;
        //    }
        //    set 
        //    {
        //        Model.Thumbnail = value;
        //        RaisePropertyChanged(nameof(Thumbnail));
        //    }
        //}
        //private ImageSource _thumbnailSrc;
        //public ImageSource ThumbnailSrc
        //{
        //    get 
        //    {
        //        if (_thumbnailSrc == null)
        //        { 
        //            _thumbnailSrc = (ImageSource)_imageSourceConverter.Convert(Thumbnail, typeof(ImageSource), null, null);
        //        }
        //        return _thumbnailSrc; 
        //    }
        //    set
        //    {
        //        _thumbnailSrc = value;
        //        RaisePropertyChanged(nameof(ThumbnailSrc));
        //    }
        //}

        public FileInfoViewModel(string fullName)
        {
            FullName = fullName;
            Model = new FileInfoModel(FullName);
        }   
    }*/
}
//02-9420-338
//02-9420-413