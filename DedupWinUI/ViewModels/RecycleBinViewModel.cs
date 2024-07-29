using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using Serilog;
using Services;
using Services.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Geolocation;
using Windows.Media.Protection.PlayReady;

namespace DedupWinUI.ViewModels
{
    
    public class RecycleBinViewModel : BaseViewModel
    {
        private readonly AppSettings _appSettings;
        private readonly IAppService _appService;
        private readonly IDataService _dataService;

        private ObservableCollection<ImageModel> _images;
        public ObservableCollection<ImageModel> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                RaisePropertyChanged(nameof(Images));
            }
        }

        public RecycleBinViewModel(
            IAppService appService,
            IDataService dataService,
            IOptions<AppSettings> appSettings)
        {
            _appService = appService;
            _dataService = dataService;
            _appSettings = appSettings.Value;
        }

        public async Task RefreshRecycleBinAsync()
        {
            var images = await _appService.GetRecycleBinImageModelsAsync();
            Images = new ObservableCollection<ImageModel>(images);
        }

        public async Task Undelete(List<ImageModel> selection)
        {
            if (!selection.Any()) return;
            foreach (var model in selection)
            {
                var destPath = Path.Combine(_appSettings.SourcePath, model.RelativePath, model.FileName);
                if (!File.Exists(destPath))
                {
                    File.Move(model.FilePath, destPath);
                }
                else
                {
                    File.Delete(model.FilePath);
                    Log.Warning($"Restoring {model.FileName}: Destination file exists already - {destPath}");
                }
                model.FilePath = destPath;
                var m = await _appService.GenerateImageModelsAsync([model.FilePath]);
                if (m != null)
                {
                    Images.Remove(model);
                    await _dataService.InsertImageDataAsync([model]);
                }
                Log.Information($"Restored {model.FilePath}");
            }
        }
    }
}
