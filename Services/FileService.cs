﻿using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Shipwreck.Phash;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace Services
{
    public class FileService : IFileService
    {

        readonly AppSettings _settings;

        public FileService(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        
            CreateThumbnailDbRoot();
            CreateRecycleBinRoot();
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

        private void CreateThumbnailDbRoot()
        {
            if (!Directory.Exists(_settings.ThumbnailsPath))
            {
                var di = Directory.CreateDirectory(_settings.ThumbnailsPath);
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
        /*
        /// <summary>
        /// Save the image thumbnail into thumbnails folder
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filePath"></param>
        /// <returns>The thubnmail file location in the thumbnails db</returns>
        public async Task<string> SaveThumbnailImageAsync(Bitmap image, string filePath)
        {
            var destFilePath = ConvertSourcePathToThumbnailPath(filePath);
            var destFolder = Path.GetDirectoryName(destFilePath);
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            if (!File.Exists(destFilePath))
                await SaveImageFileAsync(image, destFilePath);
            return destFilePath;
        }
        */

        public string ConvertSourcePathToThumbnailPath(string fullFilePath)
        {
            var relPath = fullFilePath.Substring(_settings.SourcePath.Length);
            var destPath = Path.Combine(_settings.ThumbnailsPath, relPath);
            return destPath;
        }

        public string ConvertThumbnailPathToSourcePath(string fullThumbnailPath)
        {
            var relPath = fullThumbnailPath.Substring(_settings.ThumbnailsPath.Length);
            var sourcePath = Path.Combine(_settings.SourcePath, relPath);
            return sourcePath;
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

        public static void DeleteFile(string imagePath)
        {
            bool fileDeleted = false;
            int attempts = 0;
            while (!fileDeleted && attempts < 5)
            {
                try
                {
                    File.Delete(imagePath);
                    fileDeleted = true;
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(100); // Wait for 100 milliseconds before retrying
                    if (attempts == 5) throw;
                }
            }
        }

        public static void MoveFile(string sourcePath, string destinationPath)
        {
            bool fileDeleted = false;
            int attempts = 0;
            while (!fileDeleted && attempts < 5)
            {
                try
                {
                    File.Move(sourcePath, destinationPath);
                    fileDeleted = true;
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(100); // Wait for 100 milliseconds before retrying
                    if (attempts == 5) throw;
                }
            }

        }
    }
}