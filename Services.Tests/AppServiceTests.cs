using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Services;
using Services.Models;

namespace Services.Tests
{
    public class AppServiceTests
    {

        private readonly AppService _appService;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IDataService> _dataServiceMock;
        private readonly Mock<IImagingService> _imagingServiceMock;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IOptions<AppSettings> _appSettingsOptions;

        public AppServiceTests()
        {
            // Mock IConfiguration
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup mocked configuration values
            _mockConfiguration.SetupGet(x => x["AppSettings:SourcePath"]).Returns("\\\\Rusi\\Media2\\Photos\\");
            _mockConfiguration.SetupGet(x => x["AppSettings:ThumbnailDbDir"]).Returns("D:\\Temp\\Dedup\\");
            _mockConfiguration.SetupGet(x => x["AppSettings:Extensions"]).Returns(".bmp,.BMP,.jpg,.JPG,.jpeg,.JPEG,.png,.PNG");
            _mockConfiguration.SetupGet(x => x["AppSettings:ThumbnailSize"]).Returns("192");
            _mockConfiguration.SetupGet(x => x["AppSettings:HashImageSize"]).Returns("64");

            // Create AppSettings from the mocked configuration
            var appSettings = new AppSettings(_mockConfiguration.Object);

            _appSettingsOptions = Options.Create(appSettings);

            // Mock the services that AppService depends on
            _fileServiceMock = new Mock<IFileService>();
            _dataServiceMock = new Mock<IDataService>();
            _imagingServiceMock = new Mock<IImagingService>();

            // Initialize AppService with mocked dependencies
            _appService = new AppService(
                _fileServiceMock.Object,
                _imagingServiceMock.Object,
                _dataServiceMock.Object,
                _appSettingsOptions);
        }

        //[Fact]
        //public async Task DeleteImageAsync_DeletesImage_WhenFileMovedSuccessfully()
        //{
        //    // Arrange
        //    var model = new ImageModel("image.jpg", "", 1000)
        //    {
        //        FilePath = "C:\\Source\\image.jpg",
        //        ThumbnailSource = "thumb.jpg",
        //        ImageHashSource = "hash.jpg"
        //    };

        //    _fileServiceMock.Setup(fs => fs.MoveFileAsync(It.IsAny<string>(), It.IsAny<string>()))
        //                    .ReturnsAsync(true);  // Simulate successful move


        //    // Act
        //    var result = await _appService.DeleteImageAsync(model);

        //    // Assert
        //    Assert.True(result);
        //    _fileServiceMock.Verify(fs => fs.MoveFileAsync(model.FilePath, It.IsAny<string>()), Times.Once);
        //}
    }
}