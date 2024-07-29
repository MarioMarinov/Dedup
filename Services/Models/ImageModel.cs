using System.Text.Json.Serialization;

namespace Services.Models
{
    public record ImageModel
    {
        /// <summary>
        /// Source Image file name
        /// </summary>
        public string FileName { get; private set; }
        
        /// <summary>
        /// Source Image full path
        /// </summary>
        [JsonIgnore]
        public string FilePath { get; set; }
        
        /// <summary>
        /// Hash image full path
        /// </summary>
        [JsonIgnore]
        public string ImageHashSource { get; set; }
        public string? ImageHash { get; set; }
        public long Length { get; private set; }
        
        /// <summary>
        /// Thumbnail full path
        /// </summary>
        [JsonIgnore]
        public string ThumbnailSource { get; set; }
        
        /// <summary>
        /// Image's relative path from source folder
        /// </summary>
        public string RelativePath { get; private set; }

        public ImageModel(string fileName, string relativePath, long length, string? imageHash)
        {
            FileName = fileName;
            RelativePath = relativePath;
            Length = length;
            ImageHash = imageHash;
        }
    }


}
