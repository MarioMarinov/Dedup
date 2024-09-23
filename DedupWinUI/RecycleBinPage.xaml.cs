using DedupWinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Services.Models;
using System.Linq;


namespace DedupWinUI
{
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
            InitializeAsync();
        }
        
        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void InitializeAsync()
        {
            await ViewModel.RefreshRecycleBinAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        
        private void SelectionModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionMode == ListViewSelectionMode.Single)
            {
                SelectionMode = ListViewSelectionMode.Multiple;
            }
            else if (SelectionMode == ListViewSelectionMode.Multiple)
            {
                SelectionMode = ListViewSelectionMode.Extended;
            }
            else
            {
                SelectionMode = ListViewSelectionMode.Single;
            }
            RecycleBinItemsView.SelectionMode = SelectionMode;
        }
        
        private void UndeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = RecycleBinItemsView.SelectedItems.Cast<ImageModel>().ToList();
            ViewModel.Undelete(selection);

        }

        
    }
}
