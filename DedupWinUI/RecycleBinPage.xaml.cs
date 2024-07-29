using DedupWinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DedupWinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecycleBinPage : Page
    {
        public RecycleBinViewModel ViewModel { get; }
        private ListViewSelectionMode SelectionMode { get; set; }
        public RecycleBinPage()
        {
            this.InitializeComponent();
            var host = ((App)Application.Current).Host;
            SelectionMode = ListViewSelectionMode.Single;
            ViewModel = host.Services.GetRequiredService<RecycleBinViewModel>();
            Task.Run(async ()=> await ViewModel.RefreshRecycleBinAsync());
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void UndeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = RecycleBinItemsView.SelectedItems.Cast<ImageModel>().ToList();
            ViewModel.Undelete(selection);

        }

        private void SelectionModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionMode == ListViewSelectionMode.Single) {
                SelectionMode = ListViewSelectionMode.Multiple;
            }
            else if (SelectionMode == ListViewSelectionMode.Multiple) {
                SelectionMode = ListViewSelectionMode.Extended;
            }
            else {
                SelectionMode = ListViewSelectionMode.Single; 
            }
            RecycleBinItemsView.SelectionMode = SelectionMode;
        }
    }
}
