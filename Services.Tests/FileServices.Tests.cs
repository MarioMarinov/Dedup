using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Services.Tests
{
    public class FileServiceTests
    {
        private readonly FileService _fileService;
        private readonly Mock<IOptions<AppSettings>> _appSettingsMock;
        private readonly Mock<IFileWrapper> _fileWrapperMock;
        private readonly Mock<IDirectoryWrapper> _directoryWrapperMock;

        public FileServiceTests()
        {
            // Mocking the configuration
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x["AppSettings:SourcePath"]).Returns(@"C:\Source");
            configurationMock.Setup(x => x["AppSettings:ThumbnailDbDir"]).Returns(@"C:\Thumbnails");
            configurationMock.Setup(x => x["AppSettings:RecycleBinPath"]).Returns(@"C:\RecycleBin");
            configurationMock.Setup(x => x["AppSettings:Extensions"]).Returns(".jpg,.png");
            configurationMock.Setup(x => x["AppSettings:HashImageSize"]).Returns("64");
            configurationMock.Setup(x => x["AppSettings:ThumbnailSize"]).Returns("192");

            // Using the AppSettings constructor that takes an IConfiguration object
            var appSettings = new AppSettings(configurationMock.Object);

            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(x => x.Value).Returns(appSettings);

            // Mocking file and directory wrappers
            _fileWrapperMock = new Mock<IFileWrapper>();
            _directoryWrapperMock = new Mock<IDirectoryWrapper>();

            // Injecting mocks into FileService
            _fileService = new FileService(_appSettingsMock.Object, _directoryWrapperMock.Object, _fileWrapperMock.Object);
        }

        [Fact]
        public async Task EnumerateFilteredFilesAsync_ShouldReturnCorrectFiles()
        {
            // Arrange
            var directory = @"C:\TestDirectory";
            var extensions = new[] { ".jpg", ".png" };
            var expectedFiles = new List<string> { "file1.jpg", "file2.png" };

            // Mocking the directory wrapper to return a list of files
            _directoryWrapperMock.Setup(d => d.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
                                 .Returns(expectedFiles);

            // Act
            var result = await _fileService.EnumerateFilteredFilesAsync(directory, extensions);

            // Assert
            Assert.Equal(expectedFiles, result);
        }

        [Fact]
        public async Task MoveFileAsync_ShouldMoveFileSuccessfully()
        {
            // Arrange
            var sourcePath = @"C:\Source\image1.jpg";
            var destinationPath = @"C:\Dest\image1.jpg";

            // Mock the file wrapper's Move method
            _fileWrapperMock.Setup(f => f.Move(sourcePath, destinationPath)).Verifiable();

            // Act
            var result = await _fileService.MoveFileAsync(sourcePath, destinationPath);

            // Assert
            Assert.True(result);
            _fileWrapperMock.Verify(f => f.Move(sourcePath, destinationPath), Times.Once);
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldDeleteFileSuccessfully()
        {
            // Arrange
            var filePath = @"C:\Source\image1.jpg";

            // Mock the file wrapper's Delete method
            _fileWrapperMock.Setup(f => f.Delete(filePath)).Verifiable();

            // Act
            var result = await _fileService.DeleteFileAsync(filePath);

            // Assert
            Assert.True(result);
            _fileWrapperMock.Verify(f => f.Delete(filePath), Times.Once);
        }

    }
}