﻿using Services.Models;
namespace Services
{
    public interface IAppService
    {
        Task<List<ImageModel>> GetModelsAsync();
        Task<List<String>> GetSourceFolderFilesAsync(bool recurse);
        Task<List<String>> GetRecycleBinFilesAsync();
        Task<List<ImageModel>> GenerateImageModelsAsync(IEnumerable<string> fileNames);
        Task<List<ImageModel>> GetRecycleBinImageModelsAsync();
        Task<List<ImageModel>> ScanSourceFolderAsync(List<string> fileNames);
        Task<bool> DeleteImageAsync(ImageModel model);
    }
}