using DedupWinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.System;
using System.Linq;
using Serilog;
using Services.Models;
using System.Collections.Generic;


namespace DedupWinUI
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            var host = ((App)Application.Current).Host;
            ViewModel = host.Services.GetRequiredService<MainViewModel>();
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await ViewModel.GetModelsAsync();
        }

        private async void DeleteSourceButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (ItemsGridView.SelectedItems.Count > 0) {
                var models = ItemsGridView.SelectedItems.Cast<ImageModel>().ToList();
                DeleteSelectedItems(models);
            }
        }

        private void ItemsGridView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Delete && ViewModel.SelectedModel != null)
            {
                var index = ViewModel.Images.IndexOf(ViewModel.SelectedModel);
                var model = ViewModel.SelectedModel;
                ViewModel.DeleteFilesCommand.Execute(model);
                //DeleteSelectedItems([model]);

                if (index < ViewModel.Images.Count)
                {
                    ViewModel.SelectedModel = ViewModel.Images[index];
                }
                else
                {
                    ViewModel.SelectedModel = ViewModel.Images.Last();
                }
                e.Handled = true;
            }
        }

        private void DeleteSelectedItems(List<ImageModel> models)
        {
            SelectedImage.Source = null;
            SimilarImage.Source = null;
            foreach (var model in models)
            {
                try
                {
                    ViewModel.DeleteModelAsync(model);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Couldn't delete {model.FileName}");
                    //throw;
                }
            }
        }
        

        
        //Converters
        public string GetLengthText(long length)
        {
            if (length <= 1024) { return string.Format($"{length} bytes"); }
            if (length <= 1_048_576) { return string.Format($"{length / 1024} KB"); }
            if (length <= 1_073_741_824) { return string.Format($"{length / 1_048_576} MB"); }
            if (length <= 1_099_511_627_776) { return string.Format($"{length / 1_073_741_824} GB"); }
            return string.Empty;
        }

        public static async Task<Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource> ConvertToImageSource(System.Drawing.Image image)
        {
            if (image == null) return null;

            // Convert to bitmap
            var bitmap = new System.Drawing.Bitmap(image);

            // Lock the bitmap bits and copy them to a byte array
            var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                       System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                       bitmap.PixelFormat);
            var bytes = new byte[data.Stride * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);

            // Create a SoftwareBitmap from the byte array
            var softwareBitmap = new Windows.Graphics.Imaging.SoftwareBitmap(
                Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                bitmap.Width,
                bitmap.Height,
                Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
            softwareBitmap.CopyFromBuffer(bytes.AsBuffer());

            // Convert the SoftwareBitmap to a SoftwareBitmapSource
            var softwareBitmapSource = new Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource();
            await softwareBitmapSource.SetBitmapAsync(softwareBitmap);

            return softwareBitmapSource;
        }

        

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            var src = (AppBarButton)e.OriginalSource;
            var pageName = src.CommandParameter.ToString();
            var typeName = $"DedupWinUI.{pageName}, DedupWinUI";
            var type = Type.GetType(typeName);
            Frame.Navigate(type);
        }

        private void MenuFlyoutItem_ScanSimilar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                SelectedText.Text = menuItem.Text;
                if (menuItem.Icon is SymbolIcon symbolIcon)
                {
                    SelectedIcon.Content = new SymbolIcon { Symbol = symbolIcon.Symbol };
                }
            }
        }

        private void GetSimilarImages_Loaded(object sender, RoutedEventArgs e)
        {
            var defaultMenuItem = (MenuFlyoutItem)GetSimilarImagesFlyout.Items[0];
            SelectedText.Text = defaultMenuItem.Text;

            if (defaultMenuItem.Icon is SymbolIcon symbolIcon)
            {
                SelectedIcon.Content = new SymbolIcon { Symbol = symbolIcon.Symbol };
            }
        }
    }
}
