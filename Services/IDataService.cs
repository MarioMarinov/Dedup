﻿using Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface IDataService
    {
        Task CreateTablesAsync();
        Task<List<ImageModel>> SelectImageDataAsync();
        Task InitTablesAsync(string[] tableNames);
        Task InsertBulkImageDataAsync(List<ImageModel> imageData);
        Task<int> DeleteImageDataAsync(List<ImageModel> imageData);
        Task<List<string>> GetRelativePathsAsync(string rootFolder);
    }
}
