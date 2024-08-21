namespace Maui_20311_Workaround;

public interface IImagePickerService
{
    Task<Stream> PickImageAsync();
}