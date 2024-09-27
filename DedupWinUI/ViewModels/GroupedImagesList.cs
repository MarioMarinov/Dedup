using Services.Models;
using System.Collections.Generic;

namespace DedupWinUI.ViewModels
{
    public class GroupedImagesList(IEnumerable<ImageModel> items) : List<ImageModel>(items)
    {
        public object Key { get; set; }

        public override string ToString()
        {
            return "Group by " + Key.ToString();
        }
    }

}
