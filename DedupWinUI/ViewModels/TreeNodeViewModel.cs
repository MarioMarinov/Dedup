using CommunityToolkit.Mvvm.Messaging;
using Services.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DedupWinUI.ViewModels
{
    public class TreeNodeViewModel : BaseViewModel
    {
        private ObservableCollection<TreeNodeViewModel> _children;
        public ObservableCollection<TreeNodeViewModel> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    RaisePropertyChanged(nameof(Children));
                }
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                    WeakReferenceMessenger.Default.Send(this);
                }
            }
        }

        public string Name { get; }
        public string RelativePath { get;}

        public TreeNodeViewModel(TreeNode model)
        {
            _children = new ObservableCollection<TreeNodeViewModel>();
            Name= model.Name;
            RelativePath = model.RelativePath;
            var list = new List<TreeNodeViewModel>();
            model.Children.ForEach(o=>list.Add(new TreeNodeViewModel(o)));
            _children = new ObservableCollection<TreeNodeViewModel>(list);
        }
    }
}
