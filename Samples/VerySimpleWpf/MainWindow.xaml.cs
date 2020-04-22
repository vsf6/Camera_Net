using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Camera_NET;

namespace VerySimpleWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMoniker _cameraMoniker;
        SnapshotWindow snapshotWindow = new SnapshotWindow();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Camera choice
            CameraChoice _CameraChoice = new CameraChoice();

            // Get List of devices (cameras)
            _CameraChoice.UpdateDeviceList();

            // To get an example of camera and resolution change look at other code samples 
            if (_CameraChoice.Devices.Count > 0)
            {
                // Device moniker. It's like device id or handle.
                // Run first camera if we have one
                _cameraMoniker = _CameraChoice.Devices[0].Mon;

                var resolutionList = Camera.GetResolutionList(_cameraMoniker);
                foreach (var res in resolutionList)
                {
                    ResolutionsComboBox.Items.Add(res);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopButton_Click(this, new RoutedEventArgs());
            snapshotWindow.Close();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Set selected camera to camera control with default resolution
            cameraControl.CameraControl.SetCamera(_cameraMoniker, ResolutionsComboBox.SelectionBoxItem as Resolution);
            cameraControl.CameraControl.Camera.DirectShowLogFilepath = @"C:\temp\dxlog.log";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Close camera. It's safe to call CloseCamera() even if no camera was set.
            cameraControl.CameraControl.CloseCamera();
        }

        private void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = cameraControl.CameraControl.Camera.SnapshotSourceImage();
            //var bitmap = new Bitmap(@"c:\temp\zzzzzz.bmp");
            snapshotWindow.SnapshotImage.Source = ImageSourceFromBitmap(bitmap);// new BitmapImage(new Uri(@"c:\temp\zzzzzz.bmp"));
            snapshotWindow.Show();
        }

        public BitmapImage BitmapToBitmapImage(Bitmap src)
        {
            using (var ms = new MemoryStream())
            {
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }
    }
}
