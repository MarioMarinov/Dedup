using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Services
{
    public class AppSettings
    {
        #region Configured in appsettings.json
        public string[] Extensions { get; set; }
        public int HashImageSize { get; set; }
        public string SourcePath { get; set; }
        public string ThumbnailDbDir { get; set; }
        public int ThumbnailSize { get; set; }
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
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables()
                .Build();

            InitializeSettings(config);
        }

        [SetsRequiredMembers]
        public AppSettings(IConfiguration configuration)
        {
            InitializeSettings(configuration);
        }

        // Private method to initialize common settings
        private void InitializeSettings(IConfiguration config)
        {
            // Required values are read from the configuration or fallback to default values
            SourcePath = config["AppSettings:SourcePath"] ?? "DefaultSourcePath";  // Set a default or throw exception if required
            ThumbnailDbDir = config["AppSettings:ThumbnailDbDir"] ?? "DefaultThumbnailDbDir";
            Extensions = config["AppSettings:Extensions"]?.Split(',') ?? Array.Empty<string>();
            HashImageSize = int.Parse(config["AppSettings:HashImageSize"] ?? "64");  // Example default
            ThumbnailSize = int.Parse(config["AppSettings:ThumbnailSize"] ?? "192"); // Example default

            // Initialize the paths using shared logic
            ThumbnailsPath = Path.Combine(ThumbnailDbDir, ThumbnailsFolder);
            RecycleBinPath = Path.Combine(ThumbnailDbDir, RecycleBinFolder);
            LogPath = Path.Combine(ThumbnailDbDir, LogFolder);
            SerilogPath = Path.Combine(LogPath, config["Serilog:WriteTo:0:Args:path"] ?? DefaultLogFileName);
            DbFilePath = Path.Combine(ThumbnailDbDir, DbFileName);
        }
    }
}
