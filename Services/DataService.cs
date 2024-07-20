using Services.Models;
using System.IO;
using System.Text.Json;

namespace Services
{
    public class DataService : IDataService
    {
        public List<ImageModel> GetImageData(string dataFileName)
        {
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

        public void SaveImageData(IEnumerable<ImageModel> imageData, string dataFileName)
        {
            if (File.Exists(dataFileName)) 
            { 
                var backupFileName = Path.Combine(Path.GetDirectoryName(dataFileName),"thumbs.txt");
                File.Copy(dataFileName, backupFileName, true);
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(imageData, options);
            File.WriteAllText(dataFileName, json);
        }
    }
}
