using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IDataService
    {
        List<ImageModel> GetImageData(string dataFileName);
        void SaveImageData(IEnumerable<ImageModel> imageData, string dataFileName);
    }
}
