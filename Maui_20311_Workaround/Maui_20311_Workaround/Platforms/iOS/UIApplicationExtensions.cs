using UIKit;

namespace Maui_20311_Workaround;

public static class UIApplicationExtensions
{
    public static UIViewController? GetCurrentViewController ( this UIApplication application, bool throwIfNull = true )
    {
        UIViewController? viewController = null;

        var window = application.KeyWindow;

        if ( window is not null && window.WindowLevel == UIWindowLevel.Normal )
            viewController = window.RootViewController;

        if ( viewController == null )
        {
            window = application
                .Windows
                .OrderByDescending ( w => w.WindowLevel )
                .FirstOrDefault ( w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal );

            if ( window is null && throwIfNull )
                throw new InvalidOperationException ( "Could not find the current view controller." );
            else
                viewController = window?.RootViewController;
        }

        while ( viewController?.PresentedViewController is not null )
            viewController = viewController.PresentedViewController;

        if ( viewController is null && throwIfNull )
            throw new InvalidOperationException ( "Could not find the current view controller" );

        return viewController;
    }
}