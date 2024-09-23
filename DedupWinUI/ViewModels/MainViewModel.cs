﻿using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Serilog;
using Services;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedupWinUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        
        private IAppService _appService;
        private IImagingService _imgService;
        private IDataService _dataService;
        private AppSettings _settings;
        private readonly ILogger<MainViewModel> _logger;
        private readonly IMessenger _messenger;

        public int HashImageSize { get; set; }

        private Visibility _detailsViewVisibility;
        public Visibility DetailsViewVisibility 
        { 
            get { return _detailsViewVisibility; }
            private set
            {
                _detailsViewVisibility = value;
                RaisePropertyChanged(nameof(DetailsViewVisibility));
            }
        }

        private ObservableCollection<ImageModel> _images;
        public ObservableCollection<ImageModel> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                RaisePropertyChanged(nameof(Images));
                StatusText = $"{_images.Count} files found";
            }
        }

        public string SourcePath { get; set; }

        private bool _recurse;
        public bool Recurse
        {
            get { return _recurse; }
            set
            {
                if (_recurse == value) return;
                _recurse = value;
                RaisePropertyChanged(nameof(Recurse));
            }
        }

        private ImageViewModel _selectedViewModel;
        public ImageViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            private set
            {
                if (_selectedViewModel != value)
                {
                    _selectedViewModel = value;
                    RaisePropertyChanged(nameof(SelectedViewModel));
                    DetailsViewVisibility = (_selectedViewModel == null) ?
                        Visibility.Collapsed :
                        Visibility.Visible;
                }
            }
        }

        private ImageModel _selectedModel;
        public ImageModel SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    RaisePropertyChanged(nameof(SelectedModel));
                    SelectedViewModel = (_selectedModel == null) ? null : new ImageViewModel(_selectedModel);
                }
            }
        }
        private async void GetImageSourceAsync(Bitmap thBitmap)
        {
            var sBmp = await ImagingService.BmpToSBmp(thBitmap);
            var src = new SoftwareBitmapSource();
            await src.SetBitmapAsync(sBmp);
            
            SelectedViewModel.Thumbnail = src;
            
        }
        /// <summary>
        /// Binds to the viewbox
        /// </summary>
        private Vector3 _sourceImageViewboxScale;
        public Vector3 SourceImageViewboxScale
        {
            get { return _sourceImageViewboxScale; }
            private set
            {
                if (_sourceImageViewboxScale == value) return;
                _sourceImageViewboxScale = value;
                RaisePropertyChanged(nameof(SourceImageViewboxScale));
            }
        }

        /// <summary>
        /// Binds to the scale slider
        /// </summary>
        private float _sourceImageScale;
        public float SourceImageScale
        {
            get { return _sourceImageScale; }
            set
            {
                if (_sourceImageScale == value) return;
                _sourceImageScale = value;
                RaisePropertyChanged(nameof(SourceImageScale));
                SourceImageViewboxScale = new Vector3(value / 100, value / 100, 1);
            }
        }

        private string _statusText;

        public string StatusText
        {
            get { return _statusText; }
            set 
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    RaisePropertyChanged(nameof(StatusText));
                }
            }
        }

        public int ThumbnailSize { get; set; }

        #region Commands
        public ICommand DeleteFilesCommand { get; }
        public ICommand ScanFilesCommand { get; }
        public ICommand GetSimilarImagesCommand { get; }
        public ICommand RenameFileCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        
        #endregion

        public MainViewModel(
            IOptions<AppSettings> settings, 
            IAppService appService, 
            IDataService dataService, 
            IImagingService imagingService,
            IMessenger messenger,
            ILogger<MainViewModel> logger)
        {
            _settings = settings.Value;
            _appService = appService;
            _dataService = dataService;
            _imgService = imagingService;
            _messenger = messenger;
            _logger= logger;
            HashImageSize = _settings.HashImageSize;
            Recurse = false;
            SourcePath = settings.Value.SourcePath;
            SourceImageScale = 100.0f;
            ThumbnailSize = _settings.ThumbnailSize;
            Images = new ObservableCollection<ImageModel>();
            DeleteFilesCommand = new CommandEventHandler<ImageModel>(async (model) => await DeleteFiles(model));
            GetSimilarImagesCommand = new CommandEventHandler<string>((path) => GetSimilarImages(path));
            RenameFileCommand = new CommandEventHandler<string>((path) => RenameFile(path));
            ScanFilesCommand = new CommandEventHandler<string>(async path => await ScanFiles());
            ZoomInCommand = new CommandEventHandler<object>((_) => ZoomInImage(_));
            ZoomOutCommand = new CommandEventHandler<object>((_) => ZoomOutImage(_));
            DetailsViewVisibility = Visibility.Collapsed;
        }

        private async Task DeleteFiles(ImageModel model)
        {
            await DeleteModelAsync(model);
        }



        public async Task<bool> DeleteModelAsync(ImageModel model)
        {
            var deleted = false;
            try
            {
                Images.Remove(model);
                deleted = await _appService.DeleteImageAsync(model);
                if (deleted)
                {
                    StatusText =$"{Images.Count} images";
                }
                return deleted;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete a model: {ex.Message}");
                throw;
            }
        }

        public async Task GetModelsAsync()
        {
            try
            {
                var models = await _appService.GetModelsAsync();
                _logger.LogInformation($"{models.Count} files loaded");
                Images = new ObservableCollection<ImageModel>(models);
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                throw;
            }
        }

        private void GetSimilarImages(string path)
        {
            throw new NotImplementedException();
        }


        private void RenameFile(string path)
        {
            throw new NotImplementedException();
        }

        public async Task ScanFiles()
        {
            try
            {
                await _dataService.CreateTablesAsync();//TODO: To be deleted
                var fileNames = await _appService.GetSourceFolderFilesAsync(Recurse);
                var imgCount = fileNames.Count;
                int partitionSize = 100;
                var partitionCount = (imgCount + partitionSize) / partitionSize;
                for (int i = 0; i < partitionCount; i++)
                {
                    var rangeStart = i * partitionSize;
                    var count = (i!=partitionCount-1)? partitionSize -1 : imgCount-(partitionCount-1)*partitionSize;
                    var chunk = fileNames.GetRange(rangeStart, count);
                    StatusText = $"Reading group {i + 1}/{partitionCount} | {rangeStart}-{rangeStart+count}/{imgCount}";
                    var imagesList = await _appService.ScanSourceFolderAsync(chunk);
                    await _dataService.InsertImageDataAsync(imagesList);
                    foreach (var image in imagesList) { Images.Add(image); }
                }
                //await _dataService.InsertImageDataAsync(Images.ToList());
                StatusText = $"{imgCount} images";
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                throw;
            }
            
        }

        public async Task<ObservableCollection<GroupedImagesList>> GetGroupedImagesAsync(List<ImageModel> imageList)
        {
            var query = from item in imageList
                        group item by item.FilePath into g orderby g.Key
            select new GroupedImagesList(g) { Key = g.Key };

            return new ObservableCollection<GroupedImagesList>(query);
        }

        private void ZoomInImage(object obj)
        {
            if (SourceImageScale < 200)
            {
                SourceImageScale += 10;
            }
        }
        private void ZoomOutImage(object obj)
        {
            if (SourceImageScale > 10)
            {
                SourceImageScale -= 10;
            }
        }

    }
}