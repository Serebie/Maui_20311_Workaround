using UIKit;

namespace Maui_20311_Workaround;

public static class UIApplicationExtensions
{
    public static UIViewController? GetCurrentViewController(this UIApplication application, bool throwIfNull = true)
    {
        UIViewController? viewController = null;

        var window = GetKeyWindow();

        if (window is not null && window.WindowLevel == UIWindowLevel.Normal)
            viewController = window.RootViewController;

        if (viewController == null)
        {
            window = GetWindows()?
                .OrderByDescending(w => w.WindowLevel)
                .FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

            if (window is null && throwIfNull)
                throw new InvalidOperationException("Could not find the current view controller.");
            else
                viewController = window?.RootViewController;
        }

        while (viewController?.PresentedViewController is not null)
            viewController = viewController.PresentedViewController;

        if (viewController is null && throwIfNull)
            throw new InvalidOperationException("Could not find the current view controller");

        return viewController;
    }

    private static UIWindow? GetKeyWindow()
    {
        if (!OperatingSystem.IsIOSVersionAtLeast(13)) return UIApplication.SharedApplication.KeyWindow;
        using var scenes = UIApplication.SharedApplication.ConnectedScenes;
        var windowScene = scenes.ToArray().OfType<UIWindowScene>().FirstOrDefault(
            scene =>
                scene.Session.Role == UIWindowSceneSessionRole.Application);
        return windowScene?.Windows.FirstOrDefault();
    }

    private static UIWindow[]? GetWindows()
    {
        // if we have scene support, use that
        if (!OperatingSystem.IsIOSVersionAtLeast(13) && !OperatingSystem.IsMacCatalystVersionAtLeast(13))
            return UIApplication.SharedApplication.Windows;
        try
        {
            using var scenes = UIApplication.SharedApplication.ConnectedScenes;
            var windowScene = scenes.ToArray().OfType<UIWindowScene>().FirstOrDefault(
                scene =>
                    scene.Session.Role == UIWindowSceneSessionRole.Application);
            return windowScene?.Windows;
        }
        catch (InvalidCastException)
        {
            // HACK: Workaround for https://github.com/xamarin/xamarin-macios/issues/13704
            //       This only throws if the collection is empty.
            return null;
        }

        // use the windows property (up to 15.0)
        return UIApplication.SharedApplication.Windows;
    }
}