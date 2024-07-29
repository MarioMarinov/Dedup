using Services.Models;

namespace DedupWinUI.ViewModels
{
    public enum ImageModelChange { Created, Deleted, Restored, Edited }
    public class ImageModelMessage
    {
        public ImageModel Model { get; private set; }
        public ImageModelChange Change { get; private set; }

        public ImageModelMessage(ImageModel model, ImageModelChange change)
        {
            Model = model;
            Change = change;
        }
    }
}
