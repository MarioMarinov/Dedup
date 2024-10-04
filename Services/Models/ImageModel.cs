using System.Text.Json.Serialization;

namespace Services.Models
{
    public class ImageModel(string fileName, string relativePath, long length, string? imageHash = null)
    {
        /// <summary>
        /// Source Image file name
        /// </summary>
        public string FileName { get; private set; } = fileName;

        /// <summary>
        /// Source Image full path
        /// </summary>
        [JsonIgnore]
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Hash image full path
        /// </summary>
        [JsonIgnore]
        public string ImageHashSource { get; set; } = string.Empty;
        public string? ImageHash { get; set; } = imageHash;
        public long Length { get; private set; } = length;

        /// <summary>
        /// Thumbnail full path
        /// </summary>
        [JsonIgnore]
        public string ThumbnailSource { get; set; } = string.Empty;

        /// <summary>
        /// Image's relative path from source folder
        /// </summary>
        public string RelativePath { get; private set; } = relativePath;
    }


}
