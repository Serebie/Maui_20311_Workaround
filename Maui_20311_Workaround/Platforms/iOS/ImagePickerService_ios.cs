using Foundation;
using Photos;
using PhotosUI;
using UIKit;

namespace Maui_20311_Workaround;

public class ImagePickerService : NSObject, IImagePickerService, IPHPickerViewControllerDelegate
{
    private TaskCompletionSource<Stream>? _tcs;

    // Pre iOS 14
    private UIImagePickerController? _imagePickerController;

    public async Task<Stream> PickImageAsync()
    {
        _tcs = new TaskCompletionSource<Stream>();
        
        var viewController = UIApplication.SharedApplication.GetCurrentViewController();
        if (viewController is null)
        {
            _tcs.SetResult(Stream.Null);
            return await _tcs.Task;
        }
        
        _imagePickerController = new UIImagePickerController
        {
            AllowsEditing = false,
#pragma warning disable CA1422
            SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
            MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
#pragma warning restore CA1422
        };

        _imagePickerController.FinishedPickingMedia += ImagePickerController_FinishedPickingMedia;
        
        viewController.PresentViewController(_imagePickerController, true, null);

        return await _tcs.Task;
    }

    public async Task<Stream> PickPhotoAsync()
    {
        _tcs = new TaskCompletionSource<Stream>();
        
        var viewController = UIApplication.SharedApplication.GetCurrentViewController();
        if (viewController is null)
        {
            _tcs.SetResult(Stream.Null);
            return await _tcs.Task;
        }
        
        OpenPhotoPicker(viewController);
        return await _tcs.Task;
    }

    private void OpenPhotoPicker(UIViewController attachTo)
    {
        if (!OperatingSystem.IsIOSVersionAtLeast(14))
        {
            return;
        }
        
        var filter = PHPickerFilter.ImagesFilter;
        var picker = new PHPickerViewController(new PHPickerConfiguration(PHPhotoLibrary.SharedPhotoLibrary)
        {
            Filter = filter
        });
        picker.Delegate = this;
        attachTo.PresentViewController(picker, true, null);
    }

    private void ImagePickerController_FinishedPickingMedia(object? sender, UIImagePickerMediaPickedEventArgs eventArgs)
    {
        var image = eventArgs.OriginalImage;

        if (image is null)
        {
            UnsubscribeFromEvents();
            _tcs?.SetResult(Stream.Null);
            return;
        }

        if (string.IsNullOrWhiteSpace(eventArgs.ReferenceUrl.PathExtension))
        {
            UnsubscribeFromEvents();
            _tcs?.SetResult(Stream.Null);
            return;
        }

        NSData? data;

        if (eventArgs.ReferenceUrl.PathExtension.Equals("PNG") || eventArgs.ReferenceUrl.PathExtension.Equals("png"))
        {
            data = image.AsPNG();
        }
        else
        {
            data = image.AsJPEG();
        }

        if (data is null)
        {
            UnsubscribeFromEvents();
            _tcs?.SetResult(Stream.Null);
            return;
        }

        var stream = new MemoryStream(data.ToArray());

        UnsubscribeFromEvents();

        _tcs?.SetResult(stream);

        _imagePickerController?.DismissViewController(true, null);
    }

    private void UnsubscribeFromEvents()
    {
        if (_imagePickerController is null)
        {
            return;
        }

        _imagePickerController.FinishedPickingMedia -= ImagePickerController_FinishedPickingMedia;
    }

    public void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
    {
        if (!OperatingSystem.IsIOSVersionAtLeast(14))
        {
            return;
        }
        
        var viewController = UIApplication.SharedApplication.GetCurrentViewController();
        if (viewController is null)
        {
            return;
        }
        
        viewController.DismissViewController(true, null);
        
        Console.WriteLine($"Finished picking with {results.Length} results");

        foreach (var result in results)
        {
            Console.WriteLine($"Picked {result.AssetIdentifier}");
            if (result.ItemProvider.CanLoadObject(typeof(UIImage)))
            {
                _ = result.ItemProvider.LoadObject<UIImage>((image, error) =>
                {
                    BeginInvokeOnMainThread(() =>
                    {
                        if (error is null)
                            _tcs?.SetResult(new MemoryStream(image.AsJPEG()?.ToArray() ?? []));
                    });
                });
            }
        }
    }
}