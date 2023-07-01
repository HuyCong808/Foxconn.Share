using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageViewer));
        public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageViewer));
        public static readonly DependencyProperty ZoomScaleProperty = DependencyProperty.Register("ZoomScale", typeof(double), typeof(ImageViewer), new PropertyMetadata(1.0));
        public static readonly DependencyProperty ROIProperty = DependencyProperty.Register("ROI", typeof(Rect), typeof(ImageViewer), new PropertyMetadata(new Rect()));
        public static readonly DependencyProperty CurrentPointProperty = DependencyProperty.Register("CurrentPoint", typeof(Point), typeof(ImageViewer));
        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), typeof(ImageViewer));
        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register("IsPreview", typeof(bool), typeof(ImageViewer));

        private double _imageWidth = 2448;
        private double _imageHeight = 2048;
        private List<double> _zoomScaleRange = new List<double> { 0.03125, 0.0625, 0.125, 0.25, 0.5, 1, 1.5, 200, 400, 800 };

        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        public double ZoomScale
        {
            get => (double)GetValue(ZoomScaleProperty);
            set => SetValue(ZoomScaleProperty, value);
        }

        public Rect ROI
        {
            get => (Rect)GetValue(ROIProperty);
            set => SetValue(ROIProperty, value);
        }

        public Point CurrentPoint
        {
            get => (Point)GetValue(CurrentPointProperty);
            set => SetValue(CurrentPointProperty, value);
        }

        public Color CurrentColor
        {
            get => (Color)GetValue(CurrentColorProperty);
            set => SetValue(CurrentColorProperty, value);
        }

        public bool IsPreview
        {
            get => (bool)GetValue(IsPreviewProperty);
            set => SetValue(IsPreviewProperty, value);
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public ImageViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GoPoint(CurrentPoint);
            //double left = (cvMain.ActualWidth - cvImage.ActualWidth) / 2;
            //double top = (cvMain.ActualHeight - cvImage.ActualHeight) / 2;
            //Canvas.SetLeft(cvImage, left);
            //Canvas.SetTop(cvImage, top);
        }

        private void DragDrop_Drop(object sender, DragEventArgs e)
        {

        }

        private void DragDrop_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (cvImage.Children.Count > 0)
            {
                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }
            }
        }

        private void cvImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void cvImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            CurrentPoint = GetMousePoint();
        }

        private void cvImage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        public void SetSource(int width, int height, System.Drawing.Bitmap bitmap)
        {
            _imageWidth = width;
            _imageHeight = height;
            cvImage.Children.Clear();
            cvImage.Width = width;
            cvImage.Height = height;
            Image item = new Image
            {
                Width = width,
                Height = height,
                Source = BitmapToBitmapSource(bitmap)
            };
            cvImage.Children.Add(item);
            GoOrigin();
            ZoomToFit();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NotifyPropertyChanged();
        }

        public void SetSource(object obj)
        {
            cvImage.Children.Clear();
            cvImage.Width = Width;
            cvImage.Height = Height;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NotifyPropertyChanged();
        }

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap, bool release = true)
        {
            System.Drawing.Imaging.PixelFormat pixelFormat = bitmap.PixelFormat;
            PixelFormat format = PixelFormats.Bgr24;
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    format = PixelFormats.Gray8;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    format = PixelFormats.Bgr24;
                    break;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    format = PixelFormats.Bgra32;
                    break;
                default:
                    break;
            }
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bitmap.PixelFormat);
            BitmapSource bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                format, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            if (release)
            {
                bitmap.Dispose();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return bitmapSource;
        }

        public void GoOrigin()
        {
            Point p = new Point(ImageWidth / 2, ImageHeight / 2);
            CurrentPoint = p;
            double viewWidth = ActualWidth;
            double viewHeight = ActualHeight;
            double left = viewWidth / 2 - p.X * ZoomScale;
            double top = viewHeight / 2 - p.Y * ZoomScale;
            Canvas.SetLeft(cvMain, left);
            Canvas.SetTop(cvMain, top);
        }

        public void GoPoint(Point p)
        {
            CurrentPoint = p;
            double viewWidth = ActualWidth;
            double viewHeight = ActualHeight;
            double left = viewWidth / 2 - p.X * ZoomScale;
            double top = viewHeight / 2 - p.Y * ZoomScale;
            Canvas.SetLeft(cvMain, left);
            Canvas.SetTop(cvMain, top);
        }

        private void SetZoom(double scale)
        {
            GoPoint(CurrentPoint);
            cvScale.ScaleX = scale;
            cvScale.ScaleY = scale;
            selectedRect.StrokeThickness = 1 / scale;
        }

        public void ZoomIn()
        {
            int id = _zoomScaleRange.IndexOf(ZoomScale);
            if (id < _zoomScaleRange.Count - 1)
            {
                ZoomScale = _zoomScaleRange[id + 1];
                SetZoom(ZoomScale);
            }
        }

        public void ZoomOut()
        {
            int id = _zoomScaleRange.IndexOf(ZoomScale);
            if (id > 0)
            {
                ZoomScale = _zoomScaleRange[id - 1];
                SetZoom(ZoomScale);
            }
        }

        public void ZoomToFit()
        {
            double scaleX = cvMain.ActualWidth / _imageWidth;
            double scaleY = cvMain.ActualHeight / _imageHeight;
            SetZoom(Math.Min(scaleX, scaleY));
        }

        public Point GetMousePoint()
        {
            var p = Mouse.GetPosition(cvImage);
            double x = p.X / ZoomScale;
            double y = p.Y / ZoomScale;
            return new Point((int)x, (int)y);
        }
    }
}
