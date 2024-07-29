using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace Services
{
    public class FileService : IFileService
    {
        private string _sourceDir;
        private string _thumbDbDir;

        public FileService(string sourceDir, string thumbDbDir)
        {
            _sourceDir = sourceDir;
            _thumbDbDir = thumbDbDir;
            CreateThumbnailDbRoot();
        }

        public async Task<List<string>> EnumerateFilteredFilesAsync(string dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var allFiles = new ConcurrentBag<string>();
            var filderedOutFiles = new ConcurrentBag<string>();
            await Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(dir, "*.*", searchOption);
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > 0 && extensions.Contains(fileInfo.Extension))
                    {
                        allFiles.Add(filePath);
                    }
                    else
                    {
                        filderedOutFiles.Add(filePath);
                    }
                });
        }).ConfigureAwait(false);

            return allFiles.ToList();
        }

        private bool CreateThumbnailDbRoot()
        {
            if (!Directory.Exists(_thumbDbDir))
            {
                var di = Directory.CreateDirectory(_thumbDbDir);
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> SaveImageFileAsync(Bitmap image, string filePath)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                    {
                        await memoryStream.CopyToAsync(fileStream);
                    }
                }
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

        /// <summary>
        /// Copy the thumbnail calculated for the image found at filePath to the thumbnails db
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <returns>The thubnmail file location in the thumbnails db</returns>
        public async Task<string> InsertThumbnailToDbAsync(Bitmap image, string filePath)
        {
            var destFilePath = ConvertSourcePathToThumbnailPath(filePath);
            var destFolder = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            if (!File.Exists(destFilePath))
                await SaveImageFileAsync(image, destFilePath);
            return destFilePath;
        }

        public string ConvertSourcePathToThumbnailPath(string fullFilePath)
        {
            var relPath = fullFilePath.Substring(_sourceDir.Length);
            var destPath = Path.Combine(_thumbDbDir, relPath);
            return destPath;
        }

        public string ConvertThumbnailPathToSourcePath(string fullThumbnailPath)
        {
            var relPath = fullThumbnailPath.Substring(_thumbDbDir.Length);
            var sourcePath = Path.Combine(_sourceDir, relPath);
            return sourcePath;
        }
    }
}
