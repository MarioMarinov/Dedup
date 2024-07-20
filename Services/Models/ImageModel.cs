using System.IO;
using System.Text.Json.Serialization;

namespace Services.Models
{
    public record ImageModel
    {
        public string FileName { get; private set; }
        public string? FilePath { get; private set; }
        public long Length { get; private set; }
        public string? ThumbnailSource { get; set; }
        public ImageModel(string fullName)
        {
            var fi = new FileInfo(fullName);
            Length = fi.Length;
            FileName = fi.Name;
            FilePath = fi.DirectoryName;
        }

        [JsonConstructor]
        public ImageModel(string fileName, string? filePath, long length, string thumbnailSource)
        {
            FileName = fileName;
            FilePath = filePath;
            Length = length;
            ThumbnailSource = thumbnailSource;
        }
    }


}
