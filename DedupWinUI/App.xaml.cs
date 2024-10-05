using DedupWinUI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Services;
using System.IO;
using Serilog;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using CommunityToolkit.Mvvm.Messaging;

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

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((context,builder) =>
                {
                    builder.AddConfiguration(config);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                    services.AddTransient<IFileWrapper, FileWrapper>();
                    services.AddTransient<IDirectoryWrapper, DirectoryWrapper>();
                    services.AddTransient<IFileService, FileService>();
                    services.AddTransient<IImagingService, ImagingService>();
                    services.AddTransient<IDataService, DataService>();
                    services.AddTransient<IAppService, AppService>();
                    services.AddTransient<IMessenger, WeakReferenceMessenger>();
                    services.AddSingleton<MainViewModel>(); 
                    services.AddSingleton<RecycleBinViewModel>();
                })
                .Build();

            var serviceProvider = Host.Services;
            var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            config["Serilog:WriteTo:0:Args:path"] = appSettings.SerilogPath;
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            this.InitializeComponent();
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage));
            //rootFrame.Navigate(typeof(RecycleBinPage));
            _window.Content = rootFrame;
            _window.Activate();
        }
        
    }
}
