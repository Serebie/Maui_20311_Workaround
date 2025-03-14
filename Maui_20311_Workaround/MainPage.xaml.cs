namespace Maui_20311_Workaround;

public partial class MainPage
{
    private readonly IImagePickerService _imagePickerService;

    public MainPage(IImagePickerService imagePickerService)
    {
        _imagePickerService = imagePickerService;
        InitializeComponent();
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        var stream = await _imagePickerService.PickImageAsync();
        if (stream == Stream.Null)
        {
            return;
        }

        var imageSource = ImageSource.FromStream(() => stream);
        SelectedImage.Source = imageSource;
    }

    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        var stream = await _imagePickerService.PickPhotoAsync();
        if (stream == Stream.Null)
        {
            return;
        }

        var imageSource = ImageSource.FromStream(() => stream);
        SelectedImage.Source = imageSource;
    }

    private async void OnMediaPickerClicked(object sender, EventArgs e)
    {
        var fileResult = await MediaPicker.PickPhotoAsync();
        if (fileResult is null)
        {
            return;
        }

        var imageStream = await fileResult.OpenReadAsync();
        var imageSource = ImageSource.FromStream(() => imageStream);
        SelectedImage.Source = imageSource;
    }
}