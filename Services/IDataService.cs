﻿using Services.Models;

namespace Services
{
    public interface IDataService
    {
        Task CreateTablesAsync();
        Task<List<ImageModel>> SelectImageDataAsync();
        Task InitTablesAsync(string[] tableNames);
        Task InsertImageDataAsync(List<ImageModel> imageData);
        Task<int> DeleteImageDataAsync(List<ImageModel> imageData);
    }
}