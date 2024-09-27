using Services.Models;

namespace DedupWinUI.ViewModels
{
    public enum ImageModelChange { Created, Deleted, Restored, Edited }
    public class ImageModelMessage(ImageModel model, ImageModelChange change)
    {
        public ImageModel Model { get; private set; } = model;
        public ImageModelChange Change { get; private set; } = change;
    }
}
