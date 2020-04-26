using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Interop;
using GalaSoft.MvvmLight;

namespace Monitors
{
    public class DpiViewModel : ViewModelBase, IDisposable
    {
        private Screen _selectedScreen;
        private string _info;
        public ObservableCollection<Screen> AllScreens { get; set; } = new ObservableCollection<Screen>();
        private readonly TargetWindow _targetWindow = new TargetWindow();

        public void Reload()
        {
            AllScreens.Clear();
            foreach (var s in Screen.AllScreens)
            {
                AllScreens.Add(s);
            }

            Info = GetDisplayInfo();
        }

        public Screen SelectedScreen
        {
            get => _selectedScreen;
            set
            {
                if (Set(ref _selectedScreen, value))
                {
                    ShowTaggingWindow(_selectedScreen);
                }
            }
        }


        #region Win32 PInvoke

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511(v=vs.85).aspx
        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);
        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);
        enum DpiType
        {
            Effective = 0,
            // Angular = 1,
            // Raw = 2,
        }
        private const int HWND_TOP = 0;
        private const int HWND_BOTTOM = 1;
        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;
        private const int SWP_NOSIZE = 0x0001;
        #endregion
        private void ShowTaggingWindow(Screen targetScreen)
        {
            var wnd=new WindowInteropHelper(_targetWindow);
            wnd.EnsureHandle();
            
            // Display a new window at the left/top of the selected screen.
            if (_selectedScreen != null)
            {
                var transform = GetTransformMatrix(_selectedScreen);
                var actualDeviceSize = transform.Transform(new System.Windows.Point(_targetWindow.Width, _targetWindow.Width));
                SetWindowPos(wnd.Handle, (IntPtr)HWND_TOP, (int)(targetScreen.Bounds.X-actualDeviceSize.X/4), (int)(targetScreen.Bounds.Y-actualDeviceSize.Y/4), (int)actualDeviceSize.X,
                    (int)actualDeviceSize.Y, SWP_NOSIZE);
                _targetWindow.Show();

            }
            else
            {
                _targetWindow.Hide();
                ;
            }
        }


        public static System.Windows.Media.Matrix GetTransformMatrix(Screen screen)
        {
            var pnt = new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
            GetDpiForMonitor(mon, DpiType.Effective, out var dpiX, out var dpiY);
            return new System.Windows.Media.Matrix(dpiX / 96.0, 0, 0, dpiY / 96.0, 0, 0);
        }

        public string Info
        {
            get => _info;
            set => Set(ref _info, value);
        }

        private string GetDisplayInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Total {Screen.AllScreens.Length} monitor(s)");
            foreach (var screen in Screen.AllScreens)
            {
                sb.AppendLine($"{screen.DeviceName}: BitsPerPixel: {screen.BitsPerPixel}");
                var bound = screen.Bounds;

                sb.AppendLine($"Location: {bound.Location}, Size: {bound.Size}");
                if (screen.Primary)
                {
                    sb.AppendLine($"Is Primary");
                }
            }

            return sb.ToString();
        }


        public void Dispose()
        {
            _targetWindow.Close();
        }
    }
}
