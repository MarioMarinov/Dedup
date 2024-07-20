using System.Collections.ObjectModel;

namespace Services.Models
{
    /*
    public class SearchModel
    {
        public string Directory { get; set; }
        public bool Recursive { get; set; }

        public ObservableCollection<FileInfoModel> Files { get; set; }
        public ObservableCollection<string> Extensions { get; set; }
        public ObservableCollection<FilterItemModel> FilterItems { get; set; }
        //public DelegateCommand<string> Fetch { get; private set; }
        //public DelegateCommand<string> Filter { get; private set; }

        //private IFileSysService fsSvc;
        private IImagingService imgSvc;

        public SearchModel()
        {
            Directory = @"Z:\Photos";
            Recursive = false;
            //Fetch = new DelegateCommand<string>(FetchExecute, FetchCanExecute);
            //Filter = new DelegateCommand<string>(FilterExecute);
            //fsSvc = searchService;
            //imgSvc = imagingService;
        }

        private void FilterExecute(string obj)
        {
            var s = obj;
        }

        private async void FetchExecute(string obj)
        {
            //var fi = fsSvc.EnumerateFiles(obj, Recursive);
            //Files = new ObservableCollection<FileInfoModel>(fi);
            //Extensions = new ObservableCollection<string>(Files.Select(o => o.FileItem.Extension).Distinct().OrderBy(o => o));
            //FilterItems = new ObservableCollection<FilterItemModel>(Extensions.Select(o => new FilterItemModel() { Name = o, Enabled = true }));
        }

        private bool FetchCanExecute(string arg)
        {
            return true;
        }
    }
    */
}
