using Services.Models;
using System.Collections.ObjectModel;

namespace Services
{
    public interface IAppService
    {
        Task<List<ImageModel>> GetCachedModelsAsync();
        Task<List<String>> GetSourceFolderFilesAsync(bool recurse);
        Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames);
    }
}
