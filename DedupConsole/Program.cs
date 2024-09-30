using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Services;
using System.Diagnostics;


Console.WriteLine("Dedup");
Console.WriteLine("_____");
Console.WriteLine("Detects similar images within a folder and its subfolders");


var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();


var host = Host.CreateDefaultBuilder()
    .UseSerilog()
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder.AddConfiguration(config);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IImagingService, ImagingService>();
        services.AddTransient<IDataService, DataService>();
        services.AddTransient<IAppService, AppService>();
        
    })
    .Build();

var serviceProvider = host.Services;
var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
config["Serilog:WriteTo:0:Args:path"] = appSettings.SerilogPath;
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

var cfg = host.Services.GetRequiredService<IOptions<AppSettings>>().Value;

Console.WriteLine("Settings");
Console.WriteLine($"Image Folder: {cfg.SourcePath}");
Console.WriteLine($"Thumbnails Folder: {cfg.ThumbnailDbDir}");
Console.WriteLine($"Scanned extensions: {String.Join(',', cfg.Extensions)}");
Console.WriteLine();

var fileSvc = host.Services.GetRequiredService<IFileService>();
var imgSvc = host.Services.GetRequiredService<IImagingService>();
var dataSvc = host.Services.GetRequiredService<IDataService>();
var appSvc = host.Services.GetRequiredService<IAppService>();

Stopwatch stopwatch = new Stopwatch();
//var data = dataSvc.SelectImageDataAsync();
await dataSvc.CreateTablesAsync();
Console.WriteLine("Scanning");
stopwatch.Start();
var files = await fileSvc.EnumerateFilteredFilesAsync(cfg.SourcePath, cfg.Extensions, SearchOption.TopDirectoryOnly);
stopwatch.Stop();
Console.WriteLine($"Total: {files.Count()} files, took {stopwatch.Elapsed}");

stopwatch.Reset();
stopwatch.Start();
var files1 = files.Order();
stopwatch.Stop();
Console.WriteLine($"Total: Ordering {files.Count()} files, took {stopwatch.Elapsed}");
Console.WriteLine();

Console.WriteLine("Thumbnail generation");
stopwatch.Reset();
stopwatch.Start();
var models = await appSvc.GenerateImageModelsAsync(files);
await dataSvc.CreateTablesAsync();

await dataSvc.InsertBulkImageDataAsync(models);
//var data = dataSvc.SelectImageDataAsync();
stopwatch.Stop();
Console.WriteLine($"Generated {files.Count()} files, took {stopwatch.Elapsed}");

Console.WriteLine();
Console.WriteLine("Ready");


