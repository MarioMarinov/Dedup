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
        public string? ImageHashSource { get; set; }
        public string? ImageHash { get; set; }

        public ImageModel(string fullName)
        {
            var fi = new FileInfo(fullName);
            FileName = fi.Name;
            FilePath = fi.DirectoryName;
            Length = fi.Length;
        }

        [JsonConstructor]
        public ImageModel(string fileName, string? filePath, long length, string thumbnailSource, string imageHashSource, string imageHash)
        {
            FileName = fileName;
            FilePath = filePath;
            Length = length;
            ThumbnailSource = thumbnailSource;
            ImageHashSource = imageHashSource;
            ImageHash = imageHash;
        }
    }


}
