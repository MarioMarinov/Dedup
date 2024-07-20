using Services.Models;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;

namespace Services
{
    public interface IFileService
    {
        string ConvertSourcePathToThumbnailPath(string filePath);
        string ConvertThumbnailPathToSourcePath(string filePath);

        public Task<List<string>> EnumerateFilteredFilesAsync(string dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        public Task<string> InsertThumbnailToDbAsync(Image image, string fileName);

    }
}
