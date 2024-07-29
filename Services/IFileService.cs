using System.Drawing;
using Windows.Graphics.Imaging;

namespace Services
{
    public interface IFileService
    {
        string ConvertSourcePathToThumbnailPath(string filePath);
        string ConvertThumbnailPathToSourcePath(string filePath);

        public Task<List<string>> EnumerateFilteredFilesAsync(string dir, string[] extensions, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        public Task<string> InsertThumbnailToDbAsync(Bitmap image, string filePath);

    }
}
