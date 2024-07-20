using Services.Models;
using System.Collections.Generic;

namespace DedupWinUI.ViewModels
{
    public class GroupedImagesList : List<ImageModel>
    {
        public GroupedImagesList(IEnumerable<ImageModel> items) : base(items)
        {
        }
        public object Key { get; set; }

        public override string ToString()
        {
            return "Group by " + Key.ToString();
        }
    }

}
