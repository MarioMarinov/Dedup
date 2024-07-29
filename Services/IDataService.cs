using Services.Models;

namespace Services
{
    public interface IDataService
    {
        Task<List<ImageModel>> GetImageDataAsync(string dataFileName);
        Task SaveImageDataAsync(IEnumerable<ImageModel> imageData, string dataFileName);
    }
}
