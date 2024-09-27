using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Language.Intellisense;
using Serilog;
using Services;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DedupWinUI.ViewModels
{

    public class RecycleBinViewModel : BaseViewModel
    {
        private readonly AppSettings _appSettings;
        private readonly IAppService _appService;
        private readonly IDataService _dataService;

        private BulkObservableCollection<ImageModel> _images;
        public BulkObservableCollection<ImageModel> Images
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
            Images = new BulkObservableCollection<ImageModel>();
            Images.BeginBulkOperation();
            Images.AddRange(images);
            Images.EndBulkOperation();
        }

        public async Task UndeleteAsync(List<ImageModel> selection)
        {
            if (!selection.Any()) return;
            try
            {
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
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(List<ImageModel> selection)
        {
            if (!selection.Any()) return;
            try
            {
                foreach (var model in selection)
                {
                    Images.Remove(model);
                    if (File.Exists(model.FilePath))
                    {
                        File.Delete(model.FilePath);
                        Log.Information($"Deleted {model.FilePath}");
                    }
                }
                Log.Information($"Deleted {selection.Count} images");
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
