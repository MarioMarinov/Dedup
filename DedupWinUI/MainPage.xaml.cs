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
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await ViewModel.GetModelsAsync();
        }

        private void ItemsGridView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Delete && ViewModel.SelectedModels!=null)
            {
                var index = ViewModel.Images.IndexOf(ViewModel.SelectedModel);
                ViewModel.DeleteFilesCommand.Execute(null);
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

        //Converters
        public string GetLengthText(long length)
        {
            if (length <= 1024) { return string.Format($"{length} bytes"); }
            if (length <= 1_048_576) { return string.Format($"{length / 1024} KB"); }
            if (length <= 1_073_741_824) { return string.Format($"{length / 1_048_576} MB"); }
            if (length <= 1_099_511_627_776) { return string.Format($"{length / 1_073_741_824} GB"); }
            return string.Empty;
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

        private void ItemsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedModels = ItemsGridView.SelectedItems.Cast<ImageModel>().ToList();
        }

       
    }
}
