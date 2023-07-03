using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ImageBox.xaml
    /// </summary>
    public partial class ImageBox : UserControl, INotifyPropertyChanged
    {
        #region Binding Property
        public static readonly DependencyProperty PixelPointProperty = DependencyProperty.Register("PixelPoint", typeof(Point), typeof(ImageBox));
        public static readonly DependencyProperty PixelColorProperty = DependencyProperty.Register("PixelColor", typeof(Color), typeof(ImageBox));
        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register("IsPreview", typeof(bool), typeof(ImageBox));

        public Point PixelPoint
        {
            get => (Point)GetValue(PixelPointProperty);
            set => SetValue(PixelPointProperty, value);
        }

        public Color PixelColor
        {
            get => (Color)GetValue(PixelColorProperty);
            set => SetValue(PixelColorProperty, value);
        }

        public bool IsPreview
        {
            get => (bool)GetValue(IsPreviewProperty);
            set => SetValue(IsPreviewProperty, value);
        }

        private ImageSource _source = null;
        private Point _startPoint = new Point();
        private Point _endPoint = new Point();
        private bool _isPressed = false;
        private bool _isDrawing = false;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private double _scale = 1;
        private List<double> _scales = new List<double> { 0.015625, 0.03125, 0.0625, 0.125, 0.25, 0.5, 1, 2, 4, 8 };
        private double _minimumScale = 0.015625;
        private double _maximumScale = 8;
        private Rect _ROI = new Rect();

        public ImageSource Source
        {
            get => _source;
            set
            {
                _source = value;
                FullScreen();
                NotifyPropertyChanged();
            }
        }

        public System.Drawing.Bitmap SourceFromBitmap
        {
            set
            {
                _source = BitmapToBitmapSource(value);
                FullScreen();
                NotifyPropertyChanged();
            }
        }

        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                _isPressed = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsDrawing
        {
            get => _isDrawing;
            set
            {
                _isDrawing = value;
                if (_isDrawing)
                {
                    _isDragging = false;
                    _isResizing = false;
                }
                NotifyPropertyChanged();
            }
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                _isDragging = value;
                if (_isDragging)
                {
                    _isDrawing = false;
                    _isResizing = false;
                }
                NotifyPropertyChanged();
            }
        }

        public bool IsResizing
        {
            get => _isResizing;
            set
            {
                _isResizing = value;
                if (_isResizing)
                {
                    _isDrawing = false;
                    _isDragging = false;
                }
                NotifyPropertyChanged();
            }
        }

        public double ZoomScale
        {
            get => _scale;
            set
            {
                if (value <= _maximumScale && value >= _minimumScale)
                {
                    SetScale(value);
                }
                else
                {
                    throw new InvalidOperationException("Zoom scale out of range!");
                }
            }
        }

        public double MinimumScale
        {
            get => _minimumScale;
            set => _minimumScale = value;
        }

        public double MaximumScale
        {
            get => _maximumScale;
            set => _maximumScale = value;
        }

        public Rect ROI
        {
            get => _ROI;
            set => _ROI = value;
        }

        public Visibility IsShowLine
        {
            get => _source != null ? Visibility.Visible : Visibility.Hidden;
        }

        // Declare event
        public event PropertyChangedEventHandler PropertyChanged;
        // NotifyPropertyChanged method to update property value in binding
        public void NotifyPropertyChanged(string info = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public ImageBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FullScreen();
        }

        private void Site_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_source != null)
            {
                Point position = Mouse.GetPosition(sender as UIElement);
                double x = position.X / _scale;
                double y = position.Y / _scale;
                var args = new ClickEventArgs(new Point(x, y));
                MouseClicked(this, args);
            }
        }

        private void ImageBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                _startPoint = Mouse.GetPosition(sender as UIElement);
            }
        }

        private void ImageBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point position = Mouse.GetPosition(sender as UIElement);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_isDrawing)
                {
                    double x = Math.Min(position.X, _startPoint.X);
                    double y = Math.Min(position.Y, _startPoint.Y);
                    double width = Math.Abs(position.X - _startPoint.X);
                    double height = Math.Abs(position.Y - _startPoint.Y);
                    SetRectangle(x, y, width, height);
                }
                else if (_isDragging)
                {
                    double x = rectangle.Margin.Left;
                    double y = rectangle.Margin.Top;
                    double offsetX = position.X - _startPoint.X;
                    double offsetY = position.Y - _startPoint.Y;
                    double width = rectangle.Width;
                    double height = rectangle.Height;
                    bool isMouseOver = position.X >= x && position.X <= x + width && position.Y >= y && position.Y <= y + height;
                    if (isMouseOver)
                        SetRectangle(x + offsetX, y + offsetY, width, height);
                    _startPoint = position;
                }
                else if (_isResizing)
                {
                    double x = rectangle.Margin.Left;
                    double y = rectangle.Margin.Top;
                    double width = rectangle.Width;
                    double height = rectangle.Height;
                    bool isLeft = Math.Abs(position.X - x) <= 5;
                    bool isRight = Math.Abs(position.X - (x + width)) <= 5;
                    bool isTop = Math.Abs(position.Y - y) <= 5;
                    bool isBottom = Math.Abs(position.Y - (y + height)) <= 5;

                    if (isLeft)
                        SetRectangle(x + (position.X - _startPoint.X), y, width - (position.X - _startPoint.X), height);
                    else if (isTop)
                        SetRectangle(x, y + (position.Y - _startPoint.Y), width, height - (position.Y - _startPoint.Y));
                    else if (isRight)
                        SetRectangle(x, y, width + (position.X - _startPoint.X), height);
                    else if (isBottom)
                        SetRectangle(x, y, width, height + (position.Y - _startPoint.Y));
                    _startPoint = position;
                }
            }
            else
            {
                if (_isDrawing)
                {
                    site.Cursor = Cursors.Cross;
                }
                else if (_isDragging)
                {
                    double x = rectangle.Margin.Left;
                    double y = rectangle.Margin.Top;
                    double width = rectangle.Width;
                    double height = rectangle.Height;
                    bool isMouseOver = position.X >= x && position.X <= x + width && position.Y >= y && position.Y <= y + height;
                    site.Cursor = isMouseOver ? Cursors.SizeAll : Cursors.Arrow;
                    ColorRectangle(isMouseOver ? "#FFFF453A" : "#FF32D74B");
                }
                else if (_isResizing)
                {
                    double x = rectangle.Margin.Left;
                    double y = rectangle.Margin.Top;
                    double width = rectangle.Width;
                    double height = rectangle.Height;
                    bool isLeft = Math.Abs(position.X - x) <= 1;
                    bool isRight = Math.Abs(position.X - (x + width)) <= 1;
                    bool isTop = Math.Abs(position.Y - y) <= 1;
                    bool isBottom = Math.Abs(position.Y - (y + height)) <= 1;
                    bool isMouseOver = isLeft || isTop || isRight || isBottom;

                    if (isLeft || isRight)
                        site.Cursor = Cursors.SizeWE;
                    else if (isTop || isBottom)
                        site.Cursor = Cursors.SizeNS;
                    else
                        site.Cursor = Cursors.Arrow;

                    ColorRectangle(isMouseOver ? "#FFFF453A" : "#FF32D74B");
                }
            }

            Point point = GetPixelPoint();
            Color color = GetPixelColor((BitmapSource)_source, (int)point.X, (int)point.Y);
            PixelPoint = point;
            PixelColor = color;
        }


        private void ImageBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                _isDrawing = false;
                _isDragging = false;
                _isResizing = false;
                double x = rectangle.Margin.Left;
                double y = rectangle.Margin.Top;
                double width = rectangle.Width;
                double height = rectangle.Height;
                SetRectangle(x, y, width, height);
                ColorRectangle("#FF32D74B");
                site.Cursor = Cursors.Arrow;
            }
        }

        private void mnuiLoadImage_Click(object sender, RoutedEventArgs e)
        {
            LoadImage();
            FullScreen();
        }

        private void mnuiSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }

        private void mnuiDrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            IsDrawing = true;
        }

        private void mnuiDragRectangle_Click(object sender, RoutedEventArgs e)
        {
            IsDragging = true;
        }

        private void mnuiResizeRectangle_Click(object sender, RoutedEventArgs e)
        {
            IsResizing = true;
        }

        private void mnuiClearRectangle_Click(object sender, RoutedEventArgs e)
        {
            ClearRectangle();
        }

        private void mnuiZoomIn_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.1 * _scale);
        }

        private void mnuiZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(0.9 * _scale);
        }

        private void mnuiFitToWindow_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }

        private void mnuiActualSize_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0);
        }

        private void mnuiRotateLeft_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiRotateRight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiFullScreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }

        private void LoadImage()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "All Pictures Files | *.bmp;*jpg;*png";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _source = new BitmapImage(new Uri(dialog.FileName));
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        private void SaveImage()
        {
            if (_source == null)
            {
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dialog.ShowDialog() == true)
            {
                var fileExtension = System.IO.Path.GetExtension(dialog.FileName).ToLower();

                BitmapEncoder encoder = null;
                switch (fileExtension)
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                }

                if (encoder != null)
                {
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_source));
                        encoder.Save(fileStream);
                    }
                }
            }
        }

        private bool IsValidROI(double x, double y, double width, double height)
        {
            if (width < 0 || height < 0 || x < 0 || y < 0)
            {
                return false;
            }

            return true;
        }

        public void SetROI(double x, double y, double width, double height)
        {
            if (!IsValidROI(x, y, width, height))
            {
                return;
            }

            _ROI = new Rect(x, y, width, height);
            Console.WriteLine($"ROI [{_ROI.X}, {_ROI.Y}, {_ROI.Width}, {_ROI.Height}]");

            RectangleChanged(this, new SelectRectangleArgs(_ROI));
            NotifyPropertyChanged();
        }

        private bool IsValidRectangle(double x, double y, double width, double height)
        {
            if (width < 0 || height < 0 || x < 0 || y < 0 || x + width > site.ActualWidth || y + height > site.ActualHeight)
            {
                return false;
            }

            return true;
        }

        private void SetRectangle(double x, double y, double width, double height)
        {
            if (!IsValidRectangle(x, y, width, height))
            {
                return;
            }

            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Margin = new Thickness(x, y, 0, 0);
            rectangle.Visibility = Visibility.Visible;
            Console.WriteLine($"Rectangle [{x}, {y}, {width}, {height}]");

            _ROI = new Rect(x / _scale, y / _scale, width / _scale, height / _scale);
            Console.WriteLine($"ROI [{_ROI.X}, {_ROI.Y}, {_ROI.Width}, {_ROI.Height}]");

            RectangleChanged(this, new SelectRectangleArgs(_ROI));
            NotifyPropertyChanged();
        }

        private void ColorRectangle(string hexColor = "#FF32D74B")
        {
            Color color = (Color)ColorConverter.ConvertFromString(hexColor);
            SolidColorBrush brush = new SolidColorBrush(color);
            rectangle.Stroke = brush;
        }

        public void DrawRectangle()
        {
            double x = _ROI.X * _scale;
            double y = _ROI.Y * _scale;
            double width = _ROI.Width * _scale;
            double height = _ROI.Height * _scale;

            if (!IsValidRectangle(x, y, width, height))
            {
                return;
            }

            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Margin = new Thickness(x, y, 0, 0);
            rectangle.Visibility = Visibility.Visible;
        }

        public void ClearRectangle()
        {
            SetRectangle(0, 0, 0, 0);
        }

        public void HiddenRectangle()
        {
            rectangle.Visibility = Visibility.Hidden;
        }

        private void SetScale(double scale)
        {
            if (_source != null && scale >= _minimumScale && scale <= _maximumScale)
            {
                _scale = Math.Round(scale, 2);
                site.Width = _source.Width * _scale;
                site.Height = _source.Height * _scale;
                site.UpdateLayout();
                scrollViewer.UpdateLayout();
                DrawRectangle();
            }
            NotifyPropertyChanged();
        }

        public void FullScreen()
        {
            double scrollViewerWidth = scrollViewer.ActualWidth;
            double scrollViewerHeight = scrollViewer.ActualHeight;

            if (_source != null && scrollViewerWidth > 5 && scrollViewerHeight > 5)
            {
                double scaleX = (scrollViewerWidth - 5) / _source.Width;
                double scaleY = (scrollViewerHeight - 5) / _source.Height;
                SetScale(Math.Min(scaleX, scaleY));
            }
        }

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap, bool release = true)
        {
            var pixelFormat = bitmap.PixelFormat;
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

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                format, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            if (release)
            {
                bitmap.Dispose();
            }

            return bitmapSource;
        }

        public Point GetPixelPoint()
        {
            try
            {
                var p = Mouse.GetPosition(imageBox);
                return new Point((int)(p.X / _scale), (int)(p.Y / _scale));
            }
            catch (Exception)
            {
                return new Point(0, 0);
            }
        }


        public Color GetPixelColor(BitmapSource source, int x, int y)
        {
            try
            {
                var cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                var pixels = new byte[4];
                cb.CopyPixels(pixels, 4, 0);
                return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            }
            catch (Exception)
            {
                return Colors.White;
            }
        }


        public void ShowText(string content, SolidColorBrush color, Visibility visibility = Visibility.Visible)
        {
            lblResult.Visibility = visibility;
            lblResult.Content = content;
            lblResult.Foreground = color;
        }

        public event ClickHandler Clicked;
        protected void MouseClicked(object sender, ClickEventArgs e)
        {
            Clicked?.Invoke(sender, e);
        }

        public event SelectRectangleHandler SelectRectangleChanged;
        protected void RectangleChanged(object sender, SelectRectangleArgs e)
        {
            SelectRectangleChanged?.Invoke(sender, e);
        }
    }

    public delegate void ClickHandler(object sender, ClickEventArgs e);
    public class ClickEventArgs : EventArgs
    {
        public Point ClickPoint { get; set; }
        public ClickEventArgs(Point point)
        {
            ClickPoint = point;
        }
    }

    public delegate void SelectRectangleHandler(object sender, SelectRectangleArgs e);
    public class SelectRectangleArgs : EventArgs
    {
        public Rect SelectedRectangle { get; set; }
        public SelectRectangleArgs(Rect rectangle)
        {
            SelectedRectangle = rectangle;
        }
    }
}
