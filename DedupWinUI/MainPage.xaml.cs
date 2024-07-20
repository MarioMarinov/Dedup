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
using System.IO;
using System.Windows.Input;
using Windows.System;
using System.Linq;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DedupWinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            await ViewModel.GetCachedModelsAsync();
        }

        private void GridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (GridViewItem)sender;
            if (item != null)
            {
                //ViewModel.SelectedItem = item.DataContext as FileInfoViewModel;
            }
        }

        private async void DeleteSourceButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedModel == null) return;
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete file permanently?",
                Content = $"If you delete {ViewModel.SelectedViewModel.FileName}, you won't be able to recover it. Do you want to delete it?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            deleteFileDialog.XamlRoot = Content.XamlRoot;
            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var sel = ViewModel.SelectedModel;

                var deleted = false;
                try
                {
                    deleted = await ViewModel.DeleteModelAsync(sel);
                }
                catch { }
                if (!deleted)
                {
                    var msg = new MessageDialog("The file couldn't be deleted");
                }
            }
            else
            {
                // The user clicked the CloseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private void RenameSourceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FindSimilarToSourceButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void ItemsGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    if (args.InRecycleQueue)
        //    {
        //        var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
        //        var image = templateRoot.FindName("ItemImage") as Image;
        //        image.Source = null;
        //    }

        //    if (args.Phase == 0)
        //    {
        //        args.RegisterUpdateCallback(ShowImage);
        //        args.Handled = true;
        //    }
        //}
        //private ImagingService imgsvc = new ImagingService();

        //private async void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    if (args.Phase == 1)
        //    {
        //        // It's phase 1, so show this item's image.
        //        var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
        //        var image = templateRoot.FindName("ItemImage") as Image;
        //        var item = args.Item as ImageModel;
        //        image.Source = await imgsvc.GetImageThumbnailAsync();
        //    }

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

        private void ItemsGridView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Delete && ViewModel.SelectedModel!=null)
            {
                var index = ViewModel.Images.IndexOf(ViewModel.SelectedModel);
                ViewModel.DeleteModelAsync(ViewModel.SelectedModel);
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
    }
}
