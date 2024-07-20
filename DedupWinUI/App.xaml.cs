using DedupWinUI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Services;
using System.IO;

namespace DedupWinUI
{
    public partial class App : Application
    {
        private Window _window;
        public IHost Host { get; private set; }
        

        public App()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var sourcePhotosPath = config["AppSettings:SourcePath"];
            var thumbnailsPath = config["AppSettings:ThumbnailDbDir"];

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context,builder) =>
                {
                    builder.AddConfiguration(config);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IFileService, IFileService>(provider =>
                    new FileService(sourcePhotosPath, thumbnailsPath));
                    services.AddTransient<IImagingService, ImagingService>();
                    services.AddTransient<IDataService, DataService>();
                    services.AddTransient<IAppService, AppService>();
                    services.AddSingleton<MainViewModel>();
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                })
                .Build();
            this.InitializeComponent();
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage));
            _window.Content = rootFrame;
            _window.Activate();
        }
    }
}
