using DedupWinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Services.Models;
using System.Linq;
using System.Threading.Tasks;


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
            SelectionMode = ListViewSelectionMode.Extended;
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
        /*
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
        */
        private async void UndeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = RecycleBinItemsView.SelectedItems.Cast<ImageModel>().ToList();
            await ViewModel.UndeleteAsync(selection);

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = RecycleBinItemsView.SelectedItems.Cast<ImageModel>().ToList();
            ViewModel.Delete(selection);
        }

        private void EmptyRecycleBinButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = ViewModel.Images.ToList<ImageModel>();
            ViewModel.Delete(selection);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            RecycleBinItemsView.SelectAll();
        }

        private void UnselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            RecycleBinItemsView.SelectedItems.Clear();
        }
    }
}
