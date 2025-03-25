using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI;
using Windows.Graphics;
using WinRT.Interop;
using Microsoft.UI.Windowing;


namespace LuxExport
{
    public sealed partial class Export : Window
    {
        private AppWindow _appWindow;

        public Export()
        {
            this.InitializeComponent();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(myWndId);

            if (_appWindow != null)
            {
                _appWindow.Resize(new SizeInt32(600, 400));
            }
        }
    }

}
