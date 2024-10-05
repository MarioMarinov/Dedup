using Microsoft.Extensions.Configuration;

namespace Services.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void AppSettings_ShouldInitialize_WithValidConfiguration()
        {
            // Arrange
            var configValues = new Dictionary<string, string>
            {
                {"AppSettings:SourcePath", "\\\\Server\\Media\\Photos"},
                {"AppSettings:ThumbnailDbDir", "D:\\Temp\\Dedup"},
                {"AppSettings:Extensions", ".bmp,.jpg,.png"},
                {"AppSettings:ThumbnailSize", "192"},
                {"AppSettings:HashImageSize", "64"},
                {"Serilog:WriteTo:0:Args:path", "log.txt"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Act
            var appSettings = new AppSettings(configuration);

            // Assert
            Assert.Equal("\\\\Server\\Media\\Photos", appSettings.SourcePath);
            Assert.Equal("D:\\Temp\\Dedup", appSettings.ThumbnailDbDir);
            Assert.Equal(new string[] { ".bmp", ".jpg", ".png" }, appSettings.Extensions);
            Assert.Equal(192, appSettings.ThumbnailSize);
            Assert.Equal(64, appSettings.HashImageSize);
            Assert.Equal("D:\\Temp\\Dedup\\thumbnails-test", appSettings.ThumbnailsPath);
            Assert.Equal("D:\\Temp\\Dedup\\Deleted", appSettings.RecycleBinPath);
            Assert.Equal("D:\\Temp\\Dedup\\Logs", appSettings.LogPath);
            Assert.Equal("D:\\Temp\\Dedup\\Logs\\log.txt", appSettings.SerilogPath);
            Assert.Equal("D:\\Temp\\Dedup\\thumbs1.db", appSettings.DbFilePath);
        }

        [Fact]
        public void AppSettings_ShouldInitializeWithDefaults_WhenParameterlessConstructorIsUsed()
        {
            // Act
            var appSettings = new AppSettings();  // Uses parameterless constructor

            // Assert
            Assert.NotNull(appSettings);  // Check the instance is created
            Assert.False(string.IsNullOrEmpty(appSettings.ThumbnailsPath));
            Assert.False(string.IsNullOrEmpty(appSettings.LogPath));
            // Additional assertions to verify default values, if applicable.
        }

        [Fact]
        public void AppSettings_ShouldAssignDefaults_WhenValuesAreMissingInConfiguration()
        {
            //-- Arrange --
            var configValues = new Dictionary<string, string>
            {
                {"AppSettings:SourcePath", "D:\\Users\\Mario\\Pictures"}
                // Missing other values, should fall back to defaults or empty
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            //Source path default to "My Pictures". As it is user specific the defaults have to be adapted
            //for a different user/machine
            string picturesFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            
            //default to local user app data folder
            var localAppDataPath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            
            //-- Act --
            var appSettings = new AppSettings(configuration);

            //-- Assert --
            Assert.Equal("D:\\Users\\Mario\\Pictures", appSettings.SourcePath);
            Assert.Equal([".bmp", ".BMP", ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG"], appSettings.Extensions);  // Default should be an empty array
            Assert.Equal(192, appSettings.ThumbnailSize); 
            Assert.Equal(64, appSettings.HashImageSize);
        }

        [Fact]
        public void AppSettings_ShouldConstructPathsCorrectly()
        {
            // Arrange
            var configValues = new Dictionary<string, string>
        {
            {"AppSettings:ThumbnailDbDir", "D:\\Temp\\Dedup"}
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Act
            var appSettings = new AppSettings(configuration);

            // Assert
            Assert.Equal("D:\\Temp\\Dedup\\thumbnails-test", appSettings.ThumbnailsPath);
            Assert.Equal("D:\\Temp\\Dedup\\Deleted", appSettings.RecycleBinPath);
            Assert.Equal("D:\\Temp\\Dedup\\Logs", appSettings.LogPath);
        }
    }
}
