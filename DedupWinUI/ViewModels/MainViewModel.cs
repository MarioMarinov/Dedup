using CommunityToolkit.Mvvm.Messaging;
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

        private ObservableCollection<ImageModel> _similarImages;
        public ObservableCollection<ImageModel> SimilarImages
        {
            get { return _similarImages; }
            set
            {
                _similarImages = value;
                RaisePropertyChanged(nameof(SimilarImages));
            }
        }

        private ImageModel _selectedSimilarModel;
        public ImageModel SelectedSimilarModel
        {
            get { return _selectedSimilarModel; }
            set
            {
                if (_selectedSimilarModel != value)
                {
                    _selectedSimilarModel = value;
                    RaisePropertyChanged(nameof(SelectedSimilarModel));
                    SelectedSimilarViewModel = (_selectedSimilarModel == null) ? null : new ImageViewModel(_selectedSimilarModel);
                }
            }
        }

        private ImageViewModel _selectedSimilarViewModel;
        public ImageViewModel SelectedSimilarViewModel
        {
            get { return _selectedSimilarViewModel; }
            private set
            {
                if (_selectedSimilarViewModel != value)
                {
                    _selectedSimilarViewModel = value;
                    RaisePropertyChanged(nameof(SelectedSimilarViewModel));
                    SelectedSimilarViewVisibility = (_selectedSimilarViewModel == null) ?
                        Visibility.Collapsed :
                        Visibility.Visible;
                }
            }
        }

        private Visibility _selectedSimilarViewVisibility;
        public Visibility SelectedSimilarViewVisibility
        {
            get { return _selectedSimilarViewVisibility; }
            private set
            {
                if (_selectedSimilarViewVisibility != value)
                {
                    _selectedSimilarViewVisibility = value;
                    RaisePropertyChanged(nameof(SelectedSimilarViewVisibility));
                }
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

        private List<ImageModel> _selectedModels;
        public List<ImageModel> SelectedModels
        {
            get { return _selectedModels; }
            set
            {
                if (_selectedModels != value)
                {
                    _selectedModels = value;
                    RaisePropertyChanged(nameof(SelectedModels));
                }
            }
        }



        private string _lastSimilarScanOption;
        public string LastSimilarScanOption
        {
            get { return _lastSimilarScanOption; }
            set 
            { 
                _lastSimilarScanOption = value; 
                RaisePropertyChanged(nameof(LastSimilarScanOption));
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

        private float _threshold;
        public float Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    RaisePropertyChanged(nameof(Threshold));
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
            IMessenger messenger,
            ILogger<MainViewModel> logger)
        {
            _settings = settings.Value;
            _appService = appService;
            _dataService = dataService;
            _messenger = messenger;
            _logger= logger;
            HashImageSize = _settings.HashImageSize;
            Recurse = false;
            SourcePath = settings.Value.SourcePath;
            SourceImageScale = 100.0f;
            ThumbnailSize = _settings.ThumbnailSize;
            Threshold = 0.85f;
            LastSimilarScanOption = "Folder";
            Images = new ObservableCollection<ImageModel>();
            SimilarImages = new ObservableCollection<ImageModel>();
            DeleteFilesCommand = new CommandEventHandler<object>(async (_) => await DeleteFilesAsync());
            GetSimilarImagesCommand = new CommandEventHandler<string>((option) => GetSimilarImages(option));
            RenameFileCommand = new CommandEventHandler<string>((path) => RenameFile(path));
            ScanFilesCommand = new CommandEventHandler<object>(async (_) => await ScanFilesAsync());
            ZoomInCommand = new CommandEventHandler<object>(ZoomInImage);
            ZoomOutCommand = new CommandEventHandler<object>(ZoomOutImage);
            DetailsViewVisibility = Visibility.Collapsed;
            SelectedSimilarViewVisibility = Visibility.Collapsed;
        }

        private async Task DeleteFilesAsync()
        {
            foreach (var item in SelectedModels)
            {
                await DeleteModelAsync(item);
            }
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

        private async void GetImageSourceAsync(Bitmap thBitmap)
        {
            var sBmp = await ImagingService.BmpToSBmp(thBitmap);
            var src = new SoftwareBitmapSource();
            await src.SetBitmapAsync(sBmp);

            SelectedViewModel.Thumbnail = src;

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

        /// <summary>
        /// Scans the specified source for similar images
        /// </summary>
        /// <param name="path">"Repo" to search globally, 
        /// "Folder" to search in the current folder</param>
        private void GetSimilarImages(string option)
        {
            
            if (string.IsNullOrEmpty(option))
            {
                if (string.IsNullOrEmpty(LastSimilarScanOption))
                {
                    throw new Exception("Cannot resolve the type of scan to perform for similar images");
                }
                option = LastSimilarScanOption;
            }
            switch (option)
            {
                case "Repo":
                    Log.Information("Get Similar images from Repo");
                    LastSimilarScanOption = option;
                    var similar = new List<ImageModel>();
                    SimilarImages = new ObservableCollection<ImageModel>(similar);
                    break;

                case "Folder":
                    Log.Information("Get Similar images from Folder");
                    LastSimilarScanOption = option;
                    var comparedModels = Images.Where(o=>o.RelativePath==SelectedModel.RelativePath).ToList();
                    var similarModels = _appService.GetSimilarImages(SelectedModel, comparedModels, Threshold);
                    Log.Information($"Found {similarModels.Count} similar images for {SelectedModel.FileName} from a total of {comparedModels.Count-1} siblings");
                    SimilarImages = new ObservableCollection<ImageModel>(similarModels);
                    SelectedSimilarModel = default(ImageModel);
                    break;
                default:
                    throw new Exception($"Undefined option ('{option}') for use in getting similar images");
            }
        }

        public async Task ScanFilesAsync()
        {
            try
            {
                //await _dataService.CreateTablesAsync();//TODO: To be deleted
                Recurse = false;//TODO: delete and put as an option in the UI
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
                await _dataService.InsertImageDataAsync(Images.ToList());
                StatusText = $"{imgCount} images";
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                throw;
            }
            
        }
        
        private void RenameFile(string path)
        {
            throw new NotImplementedException();
        }

        private void ZoomInImage(object _)
        {
            if (SourceImageScale < 200)
            {
                SourceImageScale += 10;
            }
        }
        
        private void ZoomOutImage(object _)
        {
            if (SourceImageScale > 10)
            {
                SourceImageScale -= 10;
            }
        }

    }
}
