using Microsoft.Extensions.Configuration;

namespace Services
{
    public class AppSettings
    {
        #region Configured in appsettings.json
        public required string[] Extensions { get; set; }
        public required int HashImageSize { get; set; }
        public required string SourcePath { get; set; }
        public required string ThumbnailDbDir { get; set; }
        public required int ThumbnailSize { get; set; }
        #endregion

        /// <summary>
        /// Full path to the database file
        /// </summary>
        public string DbFilePath { get; private set; }


        /// <summary>
        /// Full path to the log files
        /// </summary>
        public string LogPath { get; private set; }

        /// <summary>
        /// Full path to the root of the recycle bin
        /// </summary>
        public string RecycleBinPath {  get; private set; }

        /// <summary>
        /// Full path to the root of the thumbnails and hash images
        /// </summary>
        public string ThumbnailsPath { get; private set; }
        
        /// <summary>
        /// Serilog log file path
        /// </summary>
        public string SerilogPath {  get; private set; }

        #region ReadOnly
        public const string ThumbnailPrefix = "th_";
        public const string HashImagePrefix = "ih_";
        
        const string DbFileName = "thumbs1.db";
        const string DefaultLogFileName = "log.txt";
        const string LogFolder = "Logs";
        const string RecycleBinFolder = "Deleted";
        const string ThumbnailsFolder = "thumbnails-test";
        
        #endregion

        public AppSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddEnvironmentVariables()
                .Build();
            var thumbnailDbDir = config.GetValue<string>("AppSettings:ThumbnailDbDir") ?? String.Empty; 
            ThumbnailsPath = Path.Combine(thumbnailDbDir, ThumbnailsFolder);
            RecycleBinPath = Path.Combine(thumbnailDbDir, RecycleBinFolder);
            LogPath = Path.Combine(thumbnailDbDir, LogFolder);
            var serilogWriteTo = config["Serilog:WriteTo:0:Args:path"];
            SerilogPath = Path.Combine(LogPath, serilogWriteTo ?? DefaultLogFileName);
            DbFilePath = Path.Combine(thumbnailDbDir, DbFileName);
        }
    }
}
