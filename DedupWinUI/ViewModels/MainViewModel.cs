using DedupWinUI.Converters;
using Microsoft.Extensions.Options;
using Services;
using Services.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DedupWinUI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        
        private IAppService _appService;
        private IImagingService _imgService;
        private IDataService _dataService;
        private AppSettings _settings;
        public int HashImageSize { get; set; }
        
        private ObservableCollection<ImageModel> _images;
        public ObservableCollection<ImageModel> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                RaisePropertyChanged(nameof(Images));
                StatusText = $"{_images.Count} images";
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
            set
            {
                if (_selectedViewModel != value)
                {
                    _selectedViewModel = value;
                    RaisePropertyChanged(nameof(SelectedViewModel));
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
                    SelectedViewModel.Thumbnail = _imgService.ResizeAndGrayout(SelectedViewModel.FullName, _settings.ThumbnailSize);
                }
            }
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
        private Single _sourceImageScale;
        public Single SourceImageScale
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
        
        public ICommand ScanFilesCommand { get; }
        public ICommand GetSimilarImagesCommand { get; }
        public ICommand RenameFileCommand { get; }


        public MainViewModel(IOptions<AppSettings> settings, IAppService appService, IDataService dataService, IImagingService imagingService)
        {
            _settings = settings.Value;
            _appService = appService;
            _dataService = dataService;
            _imgService = imagingService;
            HashImageSize = _settings.HashImageSize;
            Recurse = false;
            SourcePath = settings.Value.SourcePath;
            SourceImageScale = 80;
            ThumbnailSize = _settings.ThumbnailSize;
            GetSimilarImagesCommand = new CommandEventHandler<string>((path) => GetSimilarImages(path));
            RenameFileCommand = new CommandEventHandler<string>((path) => RenameFile(path));
            ScanFilesCommand = new CommandEventHandler<string>(async path => await ScanFiles());
        }

        public async Task<bool> DeleteModelAsync(ImageModel model)
        {
            try
            {
                var deleted = Images.Remove(model);
                if (deleted)
                {
                    var sourceFileName = Path.Combine(model.FilePath, model.FileName);
                    var thumbFileName = model.ThumbnailSource;
                    if (File.Exists(thumbFileName))
                    {
                        await Task.Run(()=>File.Delete(thumbFileName));
                    }
                    if (File.Exists(sourceFileName))
                    {
                        await Task.Run(() => File.Delete(sourceFileName));
                    }
                    _dataService.SaveImageData(Images, Path.Combine(_settings.ThumbnailDbDir, "thumbs.db"));
                    StatusText=$"{Images.Count} images";
                }
                return deleted;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetCachedModelsAsync()
        {
            try
            {
                var thumbnails = await _appService.GetCachedModelsAsync();
                Images = new ObservableCollection<ImageModel>(
                    thumbnails.OrderBy(o=>o.FilePath).ThenBy(o=>o.FileName)
                    );
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
                    foreach (var image in imagesList) { Images.Add(image); }
                    _dataService.SaveImageData(Images, Path.Combine(_settings.ThumbnailDbDir, "thumbs.db"));
                    RaisePropertyChanged(nameof(Images));
                }
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

    }
}
