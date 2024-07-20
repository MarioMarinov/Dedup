using Services.Models;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Services
{
    public interface IImagingService
    {
        Task<Image> CreateThumbnailAsync(string fileName, int maxSide);
        Task<List<ImageModel>> GetCachedModelsAsync(IEnumerable<string> fileNames);
        Task<List<ImageModel>> GetImageModelsAsync(IEnumerable<string> fileNames);
        BitmapImage ResizeAndGrayout(string fileName, int maxSide);
    }
}
