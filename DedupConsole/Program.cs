using Services;
using System.Diagnostics;

Console.WriteLine("Dedup");
Console.WriteLine("_____");
Console.WriteLine("Detects similar images within a folder and its subfolders");
Console.WriteLine();
Console.WriteLine(@"Image Folder: \\Rusi\Media2\Photos");
Console.WriteLine();
string[] allowedExtensions = { ".bmp", ".BMP", ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG" };

var imgFolder = @"\\Rusi\Media2\\Photos";
var thumFolder = @"D:\\Temp\\Dedup\\thumbnails\\";
var fsvc = new FileService(imgFolder, thumFolder);
Stopwatch stopwatch = new Stopwatch();

Console.WriteLine("Scanning");
stopwatch.Start();
var files = await fsvc.EnumerateFilteredFilesAsync(imgFolder, allowedExtensions, SearchOption.AllDirectories);
stopwatch.Stop();
Console.WriteLine($"Total: {files.Count()} files, took {stopwatch.Elapsed}");

stopwatch.Reset();
stopwatch.Start();
var files1 = files.Order();
stopwatch.Stop();
Console.WriteLine($"Total: Ordering {files.Count()} files, took {stopwatch.Elapsed}");

Console.WriteLine("Ready");

//foreach (var file in files1)
//{
//    Console.WriteLine($"{file.Info.Name}");
//}
