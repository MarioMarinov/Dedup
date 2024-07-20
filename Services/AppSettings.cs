namespace Services
{
    public class AppSettings
    {
        public required string[] Extensions { get; set; }
        public int HashImageSize { get; set; }
        public required string SourcePath { get; set; }
        public string ThumbnailDbDir { get; set; }
        public int ThumbnailSize { get; set; }
    }
}
