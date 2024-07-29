using Microsoft.Extensions.Options;
using Services.Models;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System.Collections.Concurrent;
using System.Drawing;

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
        /// Creates a list of ImageModel
        /// </summary>
        /// <param name="fileNames">List of filenames</param>
        /// <returns>A list of ImageModel instances</returns>
        public async Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames)
        {
            var models = await _imagingService.GenerateImageModelsAsync(fileNames);
            return models;
        }

        public async Task<List<ImageModel>> GetCachedModelsAsync()
        {
            var models = await _dataService.GetImageDataAsync(Path.Combine(_settings.ThumbnailDbDir, "thumbs.db"));
            models.ForEach(model =>
            {
                var fn = Path.GetFileName(model.ThumbnailSource);
                var ihn = _settings.HashImagePrefix + fn;
                model.ImageHashSource = model.ThumbnailSource.Replace(fn, ihn);
            });
            var res = models.OrderBy(o => o.FilePath).ThenBy(o => o.FileName).ToList();
            return res;
        }


        public static 
            (ConcurrentDictionary<string, Digest> filePathsToHashes, ConcurrentDictionary<Digest, HashSet<string>> hashesToFiles) 
            GetHashes(List<string> files)
        {
            var filePathsToHashes = new ConcurrentDictionary<string, Digest>();
            var hashesToFiles = new ConcurrentDictionary<Digest, HashSet<string>>();

            Parallel.ForEach(files, (currentFile) =>
            {
                var bitmap = (Bitmap)Image.FromFile(currentFile);
                var hash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
                filePathsToHashes[currentFile] = hash;

                HashSet<string> currentFilesForHash;

                lock (hashesToFiles)
                {
                    if (!hashesToFiles.TryGetValue(hash, out currentFilesForHash))
                    {
                        currentFilesForHash = new HashSet<string>();
                        hashesToFiles[hash] = currentFilesForHash;
                    }
                }

                lock (currentFilesForHash)
                {
                    currentFilesForHash.Add(currentFile);
                }
            });

            return (filePathsToHashes, hashesToFiles);
        }

    }
}
