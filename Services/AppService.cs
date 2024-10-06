using Microsoft.Extensions.Options;
using Serilog;
using Services.Models;
using System.Collections.Concurrent;

namespace Services
{
    public class AppService(
        IFileService fileService,
        IDataService dataService,
        IOptions<AppSettings> settings) : IAppService
    {
        private AppSettings settings { get; } = settings.Value;

        public async Task<bool> DeleteImageAsync(ImageModel model)
        {
            var res = false;
            if (model == null) return res;

            var destPath = Path.Combine(settings.RecycleBinPath, model.RelativePath);
            if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
            var destFilePath = Path.Combine(destPath, model.FileName);
            try
            {
                //Try to move to recycle bin first, failure to move effectively cancels the whole deletion
                if (await fileService.MoveFileAsync(model.FilePath, destFilePath))
                {
                    File.Delete(model.ThumbnailSource);
                    File.Delete(model.ImageHashSource);
                    var cnt = await dataService.DeleteImageDataAsync([model]);
                    res = cnt > 0;
                    if (res)
                    {
                        var srcPath = Path.Combine(settings.SourcePath, model.RelativePath);
                        if (!Directory.EnumerateFileSystemEntries(srcPath).Any())
                        {
                            Directory.Delete(srcPath);
                        }
                    }
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
               fileService.EnumerateFilteredFilesAsync(
                   settings.SourcePath,
                   settings.Extensions,
                   (recurse) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return fileNames;
        }
        public async Task<List<String>> GetRecycleBinFilesAsync()
        {
            var fileNames = await
               fileService.EnumerateFilteredFilesAsync(
                   settings.RecycleBinPath,
                   settings.Extensions,
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
                var imageModel = await GenerateImageModelAsync(fname);
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

            foreach (var fname in fileNames)
            {
                var imageModel = await GenerateRecycleBinImageModelAsync(fname);
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
        private async Task<ImageModel> GenerateImageModelAsync(string filePath)
        {
            var overwrite = true;//TODO: make that a setting
            var fi = new FileInfo(filePath);
            var fileName = fi.Name;
            var directoryName = fi.DirectoryName ?? "";
            var relPath = fileService.GetRelPath(directoryName);
            var model = new ImageModel(fileName, relPath, fi.Length, null);
            model.ThumbnailSource = Path.Combine(settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.ThumbnailPrefix}{fileName}");
            model.ImageHashSource = Path.Combine(settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.HashImagePrefix}{fileName}");
            model.FilePath = Path.Combine(settings.SourcePath, model.RelativePath, fileName);
            //Create the destination folder
            Directory.CreateDirectory(Path.Combine(settings.ThumbnailsPath, model.RelativePath));

            if (!File.Exists(model.ThumbnailSource) || overwrite)
            {
                //Compute the thumbnail to save
                var th = await ImagingService.ResizeBitmapAsync(filePath, settings.ThumbnailSize);
                if (th != null)
                {
                    var opRes = await fileService.SaveImageAsync(th, model.ThumbnailSource);
                    if (!opRes)
                    {
                        //TODO: Handle!
                        //throw new Exception("Cannot save the thumbnail image");
                    }

                    if (!File.Exists(model.ImageHashSource) || overwrite)
                    {
                        //Compute the hash image and save
                        var ih = await ImagingService.ResizeBitmapAsync(th, settings.HashImageSize);
                        if (ih != null)
                        {
                            ih = ImagingService.MakeGrayscale(ih);
                            opRes = await fileService.SaveImageAsync(ih, model.ImageHashSource);
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
        private async Task<ImageModel> GenerateRecycleBinImageModelAsync(string filePath)
        {
            await Task.Yield();
            var fi = new FileInfo(filePath);
            var fileName = fi.Name;
            var directoryName = fi.DirectoryName;
            var relPath = fileService.GetRelPath(settings.RecycleBinPath, directoryName ?? string.Empty);
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
            var models = await dataService.SelectImageDataAsync();
            Parallel.ForEach(models, model =>
            {
                model.ThumbnailSource = Path.Combine(settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.ThumbnailPrefix}{model.FileName}");
                model.ImageHashSource = Path.Combine(settings.ThumbnailsPath, model.RelativePath,
                $"{AppSettings.HashImagePrefix}{model.FileName}");
                model.FilePath = Path.Combine(settings.SourcePath, model.RelativePath, model.FileName);
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
                .Where(o => o.FileName != leadModel.FileName)
                .Select(o => o.ThumbnailSource).ToList();
            var correlated = ImagingService.GetSimilarImages(threshold, leadModel.ThumbnailSource, siblingFileNames);
            var similar = comparedModels.Where(o => correlated.ContainsKey(o.ThumbnailSource)).ToList();
            return similar;
        }

        public async Task<TreeNode> GetRelativePathsTreeAsync(string rootFolder)
        {
            var paths = await dataService.GetRelativePathsAsync(rootFolder);
            if (paths.Count == 0) return new TreeNode() { Name = rootFolder, RelativePath = string.Empty };
            var root = new TreeNode { Name = paths[0], RelativePath = paths[0] };
            for (int i = 1; i < paths.Count - 1; i++)
            {
                var parts = paths[i].Split(Path.DirectorySeparatorChar);
                AddFolderNode(root, parts, 0);
            }
            return root;
        }

        private void AddFolderNode(TreeNode current, string[] parts, int index)
        {
            if (index >= parts.Length) return;

            var child = current.Children.FirstOrDefault(c => c.Name == parts[index]);
            if (child == null)
            {
                child = new TreeNode { Name = parts[index], RelativePath = Path.Combine(current.RelativePath, parts[index]) };
                current.Children.Add(child);
            }
            AddFolderNode(child, parts, index + 1);
        }
    }
}
