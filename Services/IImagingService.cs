using Microsoft.UI.Xaml.Media.Imaging;
using Services.Models;
using System.Drawing;
using Windows.Graphics.Imaging;

namespace Services
{
    public interface IImagingService
    {
        Task<SoftwareBitmap> BmpToSBmp(Bitmap bmp);
        Task<List<ImageModel>> GetCachedModelsAsync(IEnumerable<string> fileNames);
        Task<List<ImageModel>> GenerateImageModelsAsync(IEnumerable<string> fileNames);
    }
}
