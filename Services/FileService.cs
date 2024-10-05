using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace Services
{
    public class FileService : IFileService
    {
        readonly IDirectoryWrapper _directoryWrapper;
        readonly IFileWrapper _fileWrapper;
        readonly AppSettings _settings;

        public FileService(IOptions<AppSettings> settings, IDirectoryWrapper directoryWrapper, IFileWrapper fileWrapper)
        {
            _settings = settings.Value;
            _directoryWrapper = directoryWrapper;
            _fileWrapper = fileWrapper;
            
            CreateThumbnailDbRoot();
            CreateRecycleBinRoot();
        }

        public async Task<List<string>> EnumerateFilteredFilesAsync(string dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var allFiles = new ConcurrentBag<string>();
            await Task.Run(() =>
            {
                var files = _directoryWrapper.EnumerateFiles(dir, "*.*", searchOption);
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    if (extensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
                    {
                        allFiles.Add(filePath);
                    }
                });
            });
            var res = allFiles.ToList();
            res.Sort();
            return res;
        }

        private void CreateThumbnailDbRoot()
        {
            if (!Directory.Exists(_settings.ThumbnailsPath))
            {
                _ = Directory.CreateDirectory(_settings.ThumbnailsPath);
            }
        }

        private void CreateRecycleBinRoot()
        {
            if (!Directory.Exists(_settings.RecycleBinPath))
            {
                Directory.CreateDirectory(_settings.RecycleBinPath);
            }
        }

        public async Task<bool> SaveImageAsync(Bitmap image, string filePath)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                image.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                await memoryStream.CopyToAsync(fileStream);
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return false;
        }
        
        public string GetRelPath(string fullFilePath)
        {
            var relPath = GetRelPath(_settings.SourcePath, fullFilePath);
            return relPath;
        }

        public string GetRelPath(string rootPath, string fullFilePath)
        {
            var relPath = Path.GetRelativePath(rootPath, fullFilePath);
            return (relPath == ".") ? string.Empty : relPath;
        }

        public async Task<bool> DeleteFileAsync(string imagePath)
        {
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    await Task.Run(() => _fileWrapper.Delete(imagePath));
                    return true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(100);
                    if (attempts == 5) throw;
                }
            }
            return false;
        }

        public async Task<bool> MoveFileAsync(string sourcePath, string destinationPath)
        {
            int attempts = 0;
            while (attempts < 5)
            {
                try
                {
                    await Task.Run(() => _fileWrapper.Move(sourcePath, destinationPath));
                    return true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(100);
                    if (attempts == 5) throw;
                }
            }
            return false;
        }
    }
}
