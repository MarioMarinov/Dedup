using Microsoft.Extensions.Options;
using Serilog;
using Services.Models;
using System.Collections.Concurrent;

namespace Services
{
    public class AppService(
        IFileService fileService,
        IImagingService imagingService,
        IDataService iDataService,
        IOptions<AppSettings> settings) : IAppService
    {
        private IFileService _fileService { get; } = fileService;
        private IImagingService _imagingService { get; } = imagingService;
        private IDataService _dataService { get; } = iDataService;
        private AppSettings _settings { get; } = settings.Value;

        public async Task<bool> DeleteImageAsync(ImageModel model)
        {
            var res = false;
            if (model == null) return res;

            var destPath = Path.Combine(_settings.RecycleBinPath, model.RelativePath);
            if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
            var destFilePath = Path.Combine(destPath, model.FileName);
            try
            {
                //Try to move to recycle bin first, failure to move effectively cancels the whole deletion
                if (await FileService.MoveFileAsync(model.FilePath, destFilePath))
                {
                    File.Delete(model.ThumbnailSource);
                    File.Delete(model.ImageHashSource);
                    var cnt = await _dataService.DeleteImageDataAsync([model]);
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            catch (Exception)
            {
                Log.Error($"AppService failed to delete {model.FilePath}");
                throw;
                
            }
            
            return res;
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
        public async Task<List<String>> GetRecycleBinFilesAsync()
        {
            var fileNames = await
               _fileService.EnumerateFilteredFilesAsync(
                   _settings.RecycleBinPath,
                   _settings.Extensions,
                   SearchOption.AllDirectories);
            return fileNames;
        }

        public async Task<List<ImageModel>> GenerateImageModelsAsync(IEnumerable<string> fileNames)
        {
            var res = new ConcurrentBag<ImageModel>();
            var failed = new ConcurrentBag<string>();
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            await Parallel.ForEachAsync(fileNames, parallelOptions, async (fname, token) =>
            {
                var imageModel = await GenerateImageModelAsync(fname, token);
                if (!string.IsNullOrEmpty(imageModel.ThumbnailSource))
                {
                    res.Add(imageModel);
                }
                else
                {
                    failed.Add(fname);
                }
            });
            return [.. res.OrderBy(f => f.RelativePath).ThenBy(f => f.FileName)];
        }
        
        public async Task<List<ImageModel>> GetRecycleBinImageModelsAsync()
        {
            var res = new ConcurrentBag<ImageModel>();
            var failed = new ConcurrentBag<string>();

            var fileNames = await GetRecycleBinFilesAsync();
           
            foreach (var fname in fileNames) {
                var imageModel = await GenerateRecycleBinImageModelAsync(fname, CancellationToken.None);
                res.Add(imageModel);
            }
            
            return [.. res.OrderBy(f => f.RelativePath).ThenBy(f => f.FileName)];

        }
        /// <summary>
        /// Based on the file information and settings, generate the thumnail and hash image
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ImageModel> GenerateImageModelAsync(string filePath, CancellationToken token)
        {
            var overwrite = true;//TODO: make that a setting
            var fi = new FileInfo(filePath);
            var fileName = fi.Name; 
            var directoryName = fi.DirectoryName ?? "";
            var relPath = _fileService.GetRelPath(directoryName);
            var model = new ImageModel(fileName, relPath, fi.Length, null);
            model.ThumbnailSource = Path.Combine(_settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.ThumbnailPrefix}{fileName}");
            model.ImageHashSource = Path.Combine(_settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.HashImagePrefix}{fileName}");
            model.FilePath = Path.Combine(_settings.SourcePath, model.RelativePath, fileName);
            //Create the destination folder
            Directory.CreateDirectory(Path.Combine(_settings.ThumbnailsPath, model.RelativePath));

            if (!File.Exists(model.ThumbnailSource) || overwrite)
            {
                //Compute the thumbnail to save
                var th = await ImagingService.ResizeBitmapAsync(filePath, _settings.ThumbnailSize);
                if (th != null)
                {
                    var opRes = await _fileService.SaveImageAsync(th, model.ThumbnailSource);
                    if (!opRes)
                    {
                        //TODO: Handle!
                        //throw new Exception("Cannot save the thumbnail image");
                    }

                    if (!File.Exists(model.ImageHashSource) || overwrite)
                    {
                        //Compute the hash image and save
                        var ih = await ImagingService.ResizeBitmapAsync(th, _settings.HashImageSize);
                        if (ih != null)
                        {
                            ih = ImagingService.MakeGrayscale(ih);
                            opRes = await _fileService.SaveImageAsync(ih, model.ImageHashSource);
                            if (!opRes)
                            {
                                //TODO: Handle!
                                //throw new Exception("Cannot save the hash image");
                            }
                        }
                    }
                }
            }
            return model;
        }
        
        /// <summary>
        /// Create a recycle bin item
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<ImageModel> GenerateRecycleBinImageModelAsync(string filePath, CancellationToken token)
        {
            await Task.Yield();
            var fi = new FileInfo(filePath);
            var fileName = fi.Name;
            var directoryName = fi.DirectoryName;
            var relPath = _fileService.GetRelPath(_settings.RecycleBinPath, directoryName ?? string.Empty);
            var model = new ImageModel(fileName, relPath, fi.Length, null)
            {
                FilePath = filePath
            };
            return model;
        }

        /// <summary>
        /// Creates a list of ImageModel
        /// </summary>
        /// <param name="fileNames">List of filenames</param>
        /// <returns>A list of ImageModel instances</returns>
        public async Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames)
        {
            var models = await GenerateImageModelsAsync(fileNames);
            return models;
        }

        public async Task<List<ImageModel>> GetModelsAsync()
        {
            var models = await _dataService.SelectImageDataAsync();
            Parallel.ForEach(models, model =>
            {
                model.ThumbnailSource = Path.Combine(_settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.ThumbnailPrefix}{model.FileName}");
                model.ImageHashSource = Path.Combine(_settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.HashImagePrefix}{model.FileName}");
                model.FilePath = Path.Combine(_settings.SourcePath, model.RelativePath, model.FileName);
                if (!File.Exists(model.ThumbnailSource) || !File.Exists(model.ImageHashSource))
                {
                    Console.WriteLine($"Target images not found for {model.FileName}");
                }
            });
            return [.. models];
        }

        public List<ImageModel> GetSimilarImages(ImageModel leadModel, List<ImageModel> comparedModels, float threshold)
        {
            var siblingFileNames = comparedModels
                .Where(o=>o.FileName!=leadModel.FileName)
                .Select(o=>o.ThumbnailSource).ToList();
            var correlated = ImagingService.GetSimilarImages(threshold, leadModel.ThumbnailSource, siblingFileNames);
            var similar = comparedModels.Where(o => correlated.ContainsKey(o.ThumbnailSource)).ToList();
            return similar;
        }

        
    }
}
