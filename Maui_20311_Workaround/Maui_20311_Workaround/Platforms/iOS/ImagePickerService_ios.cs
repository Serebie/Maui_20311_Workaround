using Foundation;
using Microsoft.Extensions.Logging;
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

        if (OperatingSystem.IsIOSVersionAtLeast(14))
        {
            var readWriteStatus = PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite);

            PHPhotoLibrary.RequestAuthorization(PHAccessLevel.ReadWrite, status =>
            {
                switch (status)
                {
                    case PHAuthorizationStatus.NotDetermined:
                        break;
                    case PHAuthorizationStatus.Restricted:
                        break;
                    case PHAuthorizationStatus.Denied:
                        break;
                    case PHAuthorizationStatus.Authorized:
                        break;
                    case PHAuthorizationStatus.Limited:
                        if (!OperatingSystem.IsIOSVersionAtLeast(14))
                        {
                            return;
                        }
                        
                        BeginInvokeOnMainThread(() => PHPhotoLibrary.SharedPhotoLibrary.PresentLimitedLibraryPicker(viewController,
                            strings =>
                            {
                                foreach (var identifiers in strings)
                                {
                                    Console.WriteLine(identifiers);
                                }
                            }));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }
            });
            
            

            var filter = PHPickerFilter.ImagesFilter;
            var picker = new PHPickerViewController(new PHPickerConfiguration(PHPhotoLibrary.SharedPhotoLibrary)
            {
                Filter = filter
            });
            picker.Delegate = this;
            viewController.PresentViewController(picker, true, null);
        }
        else
        {
            _imagePickerController = new UIImagePickerController
            {
                AllowsEditing = false,
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            _imagePickerController.FinishedPickingMedia += ImagePickerController_FinishedPickingMedia;
        }

        return await _tcs.Task;
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