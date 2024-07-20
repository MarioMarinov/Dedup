using Microsoft.Extensions.Options;
using Services.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace Services
{
    public class AppService: IAppService
    {
        private IFileService _fileService { get; set; }
        private IImagingService _imagingService { get; set; }
        private IDataService _dataService { get; set; }
        private AppSettings _settings { get; set; }

        public AppService(
            IFileService fileService, 
            IImagingService imagingService, 
            IDataService iDataService,
            IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            _fileService = fileService;
            _imagingService = imagingService;
            _dataService = iDataService;
        }

        public async Task<List<String>> GetSourceFolderFilesAsync(bool recurse)
        {
            var fileNames = await
               _fileService.EnumerateFilteredFilesAsync(
                   _settings.SourcePath,
                   _settings.Extensions,
                   (recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return fileNames;
        }

        /// <summary>
        /// Scan the media source folder
        /// </summary>
        /// <param name="recurse">Set to true to traverse the folder hierarchy</param>
        /// <returns>A list of ImageModel instances</returns>
        public async Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames)
        {
            var models = await _imagingService.GetImageModelsAsync(fileNames);
            _dataService.SaveImageData(models, Path.Combine(_settings.ThumbnailDbDir,"thumbs.db"));
            return models;
        }

        public async Task<List<ImageModel>> GetCachedModelsAsync()
        {
            var models = _dataService.GetImageData(Path.Combine(_settings.ThumbnailDbDir, "thumbs.db"));
            return models;
        }


    }
}
