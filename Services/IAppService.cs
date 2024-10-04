using Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Services
{
    public interface IAppService
    {
        Task<List<ImageModel>> GetModelsAsync();
        Task<List<String>> GetSourceFolderFilesAsync(bool recurse);
        Task<List<ImageModel>> GenerateImageModelsAsync(IEnumerable<string> fileNames);
        List<ImageModel> GetSimilarImages(ImageModel leadModel, List<ImageModel> comparedModels, float threshold);
        Task<List<String>> GetRecycleBinFilesAsync(); 
        Task<List<ImageModel>> GetRecycleBinImageModelsAsync();
        Task<TreeNode> GetRelativePathsTreeAsync(string rootFolder);
        Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames);
        Task<bool> DeleteImageAsync(ImageModel model);

        
    }
}
