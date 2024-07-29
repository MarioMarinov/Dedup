using Services.Models;
using System.IO;
using System.Text.Json;

namespace Services
{
    public class DataService : IDataService
    {
        public async Task<List<ImageModel>> GetImageDataAsync(string dataFileName)
        {
            await Task.Yield();
            var res = new List<ImageModel>();
            if (File.Exists(dataFileName))
            {
                var json = File.ReadAllText(dataFileName);
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                var imageData = JsonSerializer.Deserialize<List<ImageModel>>(json, options);
                res.AddRange(imageData);
            }
            return res;
        }

        public async Task SaveImageDataAsync(IEnumerable<ImageModel> imageData, string dataFileName)
        {
            await Task.Yield();
            if (File.Exists(dataFileName)) 
            { 
                var backupFileName = Path.Combine(Path.GetDirectoryName(dataFileName),"thumbs.txt");
                File.Move(dataFileName, backupFileName, true);
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(imageData, options);
            File.WriteAllText(dataFileName, json);
        }
    }
}
