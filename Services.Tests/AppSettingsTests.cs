using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void AppSettings_ShouldThrowException_WhenMissingRequiredSettings()
        {
            // Arrange
            var configValues = new Dictionary<string, string>(); // Empty configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AppSettings(configuration));
        }

        [Fact]
        public void AppSettings_ShouldAssignDefaults_WhenValuesAreMissingInConfiguration()
        {
            // Arrange
            var configValues = new Dictionary<string, string>
        {
            {"AppSettings:SourcePath", "\\\\Server\\Media\\Photos"}
            // Missing other values, should fall back to defaults or empty
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Act
            var appSettings = new AppSettings(configuration);

            // Assert
            Assert.Equal("\\\\Server\\Media\\Photos", appSettings.SourcePath);
            Assert.Empty(appSettings.Extensions);  // Default should be an empty array
            Assert.Equal(0, appSettings.ThumbnailSize); // Default integer value
            Assert.Equal(0, appSettings.HashImageSize);
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
