namespace Maui_20311_Workaround;

public partial class MainPage : ContentPage
{
    private readonly IImagePickerService _imagePickerService;

    public MainPage(IImagePickerService imagePickerService)
    {
        _imagePickerService = imagePickerService;
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        var stream = await _imagePickerService.PickImageAsync();
        if (stream == Stream.Null)
        {
            return;
        }

        var imageSource = ImageSource.FromStream(() => stream);
        SelectedImage.Source = imageSource;
    }
}