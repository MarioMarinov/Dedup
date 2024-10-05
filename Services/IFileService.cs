using System.Drawing;

namespace Services
{
    public interface IFileService
    {
        public Task<List<string>> EnumerateFilteredFilesAsync(string dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        public string GetRelPath(string fullFilePath);
        public string GetRelPath(string rootPath, string fullFilePath);
        public Task<bool> SaveImageAsync(Bitmap image, string filePath);
        public Task<bool> MoveFileAsync(string sourcePath, string destinationPath);
        public Task<bool> DeleteFileAsync(string imagePath);
    }
}
