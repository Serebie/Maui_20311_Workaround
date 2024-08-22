namespace Maui_20311_Workaround;

public interface IImagePickerService
{
    /// <summary>
    /// Uses the UIImagePickerController for iOS to pick an image.
    /// </summary>
    /// <returns>The stream of a single image.</returns>
    Task<Stream> PickImageAsync();
    
    /// <summary>
    /// Uses the PHPickerViewController for iOS to pick a photo.
    /// </summary>
    /// <returns>The stream of a single photo.</returns>
    Task<Stream> PickPhotoAsync();
}