using System;
using System.ComponentModel;
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
        public static readonly DependencyProperty CurrentPointProperty = DependencyProperty.Register("CurrentPoint", typeof(Point), typeof(ImageBox));
        public static readonly DependencyProperty CurrentColorProperty = DependencyProperty.Register("CurrentColor", typeof(Color), typeof(ImageBox));
        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register("IsPreview", typeof(bool), typeof(ImageBox));

        private ImageSource _source = null;
        private double _scale = 1;
        private double _minScale = 0.0390625;
        private double _maxScale = 5;
        private bool _isDrawing = false;
        private bool _isDraging = false;
        private Point _startPoint = new Point();
        private Rect _ROI = new Rect();

        private bool _selectedRectByCtrl = false;
        private Visibility _isShowText = Visibility.Hidden;
        private string _textContent = string.Empty;
        private SolidColorBrush _textColor = Brushes.Green;

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

        public ImageSource Source
        {
            get => _source;
            set
            {
                _source = value;
                _ROI = new Rect();
                NotifyPropertyChanged();
                int w = (value as BitmapSource).PixelWidth;
                int h = (value as BitmapSource).PixelHeight;
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

        public double ZoomScale
        {
            get => _scale;
            set
            {
                if (value <= _maxScale && value >= _minScale)
                {
                    SetScale(value);
                }
                else
                {
                    throw new InvalidOperationException("Zoom scale out of range!");
                }
            }
        }

        public double MinScale
        {
            get => _minScale;
            set => _minScale = value;
        }

        public double MaxScale
        {
            get => _maxScale;
            set => _maxScale = value;
        }

        public bool IsDrawing
        {
            get => _isDrawing;
            set
            {
                _isDrawing = value;
                NotifyPropertyChanged();
            }

        }

        public bool IsDraging
        {
            get => _isDraging;
            set
            {
                _isDraging = value;
                NotifyPropertyChanged();
            }

        }



        public Rect ROI
        {
            get => _ROI;
            set
            {
                if (value == null)
                    _ROI = new Rect();
                else
                    _ROI = value;
                SetSelectedRect();
            }
        }


        public bool SelectedRectByCtrl
        {
            get => _selectedRectByCtrl;
            set => _selectedRectByCtrl = value;
        }

        public Brush RectangleStroke
        {
            get => rectangle.Stroke;
            set => rectangle.Stroke = value;
        }

        public Visibility IsShowLine
        {
            get => _source != null ? Visibility.Visible : Visibility.Hidden;
        }

        public Visibility IsShowText
        {
            get => _isShowText;
            set => _isShowText = value;
        }

        public string TextContent
        {
            get => _textContent;
            set => _textContent = value;
        }

        public SolidColorBrush TextColor
        {
            get => _textColor;
            set => _textColor = value;
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

        private void imageBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_source != null)
            {
                _startPoint = Mouse.GetPosition(sender as UIElement);

                Point position = Mouse.GetPosition(sender as UIElement);
                double x = rectangle.Margin.Left;
                double y = rectangle.Margin.Top;
                double w = rectangle.Width;
                double h = rectangle.Height;
                bool isMouseOver = position.X >= x && position.X <= x + w && position.Y >= y && position.Y <= y + h;
                if (isMouseOver && !_isDrawing)
                {
                    _isDraging = true;
                    ColorRectangle(Colors.Yellow);

                }
                else if (!isMouseOver)
                {
                    _isDraging = false;
                    ColorRectangle(Colors.LimeGreen);
                }
            }
        }

        private void imageBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point position = Mouse.GetPosition(sender as UIElement);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_isDrawing)
                {
                    double x = Math.Min(position.X, _startPoint.X);
                    double y = Math.Min(position.Y, _startPoint.Y);
                    double w = Math.Abs(position.X - _startPoint.X);
                    double h = Math.Abs(position.Y - _startPoint.Y);
                    rectangle.Width = w;
                    rectangle.Height = h;
                    rectangle.Margin = new Thickness(x, y, 0, 0);
                    rectangle.Visibility = Visibility.Visible;
                }
                else if (_isDraging)
                {
                    double x = rectangle.Margin.Left;
                    double y = rectangle.Margin.Top;
                    double w = rectangle.Width;
                    double h = rectangle.Height;
                    double offsetX = position.X - _startPoint.X;
                    double offsetY = position.Y - _startPoint.Y;
                    if (position.X >= x && position.X <= x + w && position.Y >= y && position.Y <= y + h)
                    {
                        Site.Cursor = Cursors.Hand;
                        SetRectangle(x + offsetX, y + offsetY, w, h);
                    }
                    _startPoint = position;

                }
            }
            else
            {
                if (_isDrawing)
                {
                    Site.Cursor = Cursors.Cross;
                }
                else if (!_isDrawing)
                {
                    Site.Cursor = Cursors.Arrow;
                }

            }

            Point currentPoint = GetMousePoint();
            Color currentColor = GetPixelColor((BitmapSource)_source, (int)currentPoint.X, (int)currentPoint.Y);
            CurrentPoint = currentPoint;
            CurrentColor = currentColor;
        }


        public bool IsValidROIArea(double x, double y, double height, double width)
        {
            if (x < 0 || y < 0 || height < 0 || width < 0 || x + width > Site.ActualWidth || y + height > Site.ActualHeight)
            {
                return false;
            }
            return true;
        }

        private void imageBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraging = false;
            // _isDrawing = false;
            GetSelectedRect();
            SetSelectedRect();
            Site.Cursor = Cursors.Arrow;
            RectangleChanged(this, new SelectRectangleArgs(_ROI));
        }

        private void imageBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(imageBox);
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            // Calculate the new scale
            double newScale = _scale * zoomFactor;

            // Calculate the new offset to keep the mouse position fixed
            double offsetX = mousePos.X - (mousePos.X - ScrollView.HorizontalOffset) * zoomFactor;
            double offsetY = mousePos.Y - (mousePos.Y - ScrollView.VerticalOffset) * zoomFactor;

            // Set the new scale and offset
            SetScale(newScale);
            ScrollView.ScrollToHorizontalOffset(offsetX);
            ScrollView.ScrollToVerticalOffset(offsetY);

            e.Handled = true;
        }

        private void SetRectangle(double x, double y, double width, double height)
        {
            if (!IsValidROIArea(x, y, width, height))
            {
                return;
            }
            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Margin = new Thickness(x, y, 0, 0);
            rectangle.Visibility = Visibility.Visible;

            _ROI = new Rect(x / _scale, y / _scale, width / _scale, height / _scale);
            RectangleChanged(this, new SelectRectangleArgs(_ROI));
            NotifyPropertyChanged();
        }

        private void ColorRectangle(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            rectangle.Stroke = brush;
        }


        private void imageBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //Console.WriteLine($"H:{ScrollView.HorizontalOffset}, V:{ScrollView.VerticalOffset}, W:{ScrollView.ViewportWidth}");
        }

        private void mnuiLoadImage_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "All Pictures File | *.bmp;*jpg;*png";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(dialog.FileName);
                        bmp.EndInit();
                        _source = bmp;
                        FullScreen();
                        var wPixel = (_source as BitmapSource).PixelWidth;
                        var hPixel = (_source as BitmapSource).PixelHeight;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void mnuiSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (_source == null)
                return;

            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var image = (BitmapSource)imageBox.Source.Clone();

                    // ImageFormat imageFormat = ImageFormat.Png;
                    BitmapEncoder encoder = null;
                    string fileExtension = Path.GetExtension(dialog.FileName).ToLower();

                    switch (fileExtension)
                    {
                        case ".png":
                            {
                                encoder = new PngBitmapEncoder();
                                break;
                            }
                        case ".bmp":
                            {
                                encoder = new BmpBitmapEncoder();
                                break;
                            }
                        case ".jpeg":
                            {
                                encoder = new JpegBitmapEncoder();
                                break;
                            }
                    }
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);

                        // image.Save(fileStream, imageFormat);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void mnuiSelectedRect_Click(object sender, RoutedEventArgs e)
        {
            _isDrawing = true;
        }

        private void mnuiResetRect_Click(object sender, RoutedEventArgs e)
        {
            ClearSelectedRect();
        }

        private void mnuiZoomIn_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.2 * _scale);
        }

        private void mnuiZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(0.8 * _scale);
        }

        private void mnuiFitToWindow_Click(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }

        private void mnuiActualSize_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1);
        }

        private void mnuiRotateLeft_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiRotateRight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnuiFullScreen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Site_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_source != null)
            {
                Point p = Mouse.GetPosition(sender as UIElement);
                double x = p.X / _scale;
                double y = p.Y / _scale;
                var args = new ClickEventArgs(new Point(x, y));
                MouseClicked(this, args);
            }
        }

        public event ClickHandler Clicked;
        protected void MouseClicked(object sender, ClickEventArgs e)
        {
            if (Clicked != null)
                Clicked(sender, e);
        }

        public event SelectRectangleHandler SelectRectangleChanged;
        protected void RectangleChanged(object sender, SelectRectangleArgs e)
        {
            if (SelectRectangleChanged != null)
                SelectRectangleChanged(sender, e);
        }

        private void GetSelectedRect()
        {
            _ROI.X = rectangle.Margin.Left / _scale;
            _ROI.Y = rectangle.Margin.Top / _scale;
            _ROI.Width = rectangle.Width / _scale;
            _ROI.Height = rectangle.Height / _scale;
            //Console.WriteLine($"Rect: X[{(int)_selectedRect.X}], Y[{(int)_selectedRect.Y}], Width[{(int)_selectedRect.Width}], Height[{(int)_selectedRect.Height}]");
        }

        public void SetSelectedRect()
        {
            double x = _ROI.X * _scale;
            double y = _ROI.Y * _scale;
            double w = _ROI.Width * _scale;
            double h = _ROI.Height * _scale;
            rectangle.Width = w;
            rectangle.Height = h;
            rectangle.Margin = new Thickness(x, y, 0, 0);
            rectangle.Visibility = Visibility.Visible;
        }

        public void ClearSelectedRect()
        {
            _ROI = new Rect();
            //  _isSelectedRect = false;
            _isDrawing = false;
            Site.Cursor = Cursors.Arrow;
            SetSelectedRect();
            RectangleChanged(this, new SelectRectangleArgs(_ROI));
            NotifyPropertyChanged();
        }

        public void DrawRectangle(System.Drawing.Rectangle rect)
        {
            ClearSelectedRect();
            _ROI.X = rect.X;
            _ROI.Y = rect.Y;
            _ROI.Width = rect.Width;
            _ROI.Height = rect.Height;
            SetSelectedRect();
            RectangleChanged(this, new SelectRectangleArgs(_ROI));
            rectangle.Visibility = Visibility.Visible;
        }

        private void SetScale(double scale)
        {
            if (_source != null)
            {
                if (scale >= _minScale && scale <= _maxScale)
                {
                    scale = Math.Round(scale, 2);
                    Site.Width = _source.Width * scale;
                    Site.Height = _source.Height * scale;
                    _scale = scale;
                    SetSelectedRect();
                    ScrollView.UpdateLayout();
                    Site.UpdateLayout();
                }
            }
            NotifyPropertyChanged();
        }

        public void FullScreen()
        {
            if (_source != null)
            {
                double scaleX = (ScrollView.ActualWidth - 5) / _source.Width;
                double scaleY = (ScrollView.ActualHeight - 5) / _source.Height;
                SetScale(Math.Min(scaleX, scaleY));
            }
        }

        public Point GetMousePoint()
        {
            var p = Mouse.GetPosition(imageBox);
            double x = p.X / _scale;
            double y = p.Y / _scale;
            return new Point((int)x, (int)y);
        }

        public void GoPoint(Point point, bool resetScale = false)
        {
            if (resetScale)
            {
                SetScale(1);
            }
            double x = point.X * _scale;
            double y = point.Y * _scale;
            double crOffsetX = ScrollView.HorizontalOffset;
            double crOffsetY = ScrollView.VerticalOffset;
            double lX = ScrollView.ViewportWidth;
            double lY = ScrollView.ViewportHeight;
            double offsetX = crOffsetX + lX / 2;
            double offsetY = crOffsetY + lY / 2;
            if (x > lX / 2)
            {
                ScrollView.ScrollToHorizontalOffset(x - lX / 2);
            }
            if (y > lY / 2)
            {
                ScrollView.ScrollToVerticalOffset(y - lY / 2);
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return bitmapSource;
        }

        public void ClearSource()
        {
            _source = null;
            NotifyPropertyChanged();
        }

        public Color GetPixelColor(BitmapSource source, int x, int y)
        {
            Color color = Colors.White;
            if (source != null)
            {
                try
                {
                    var cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                    var pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    color = Color.FromRgb(pixels[2], pixels[1], pixels[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return color;
        }

        public void ShowText(string content, SolidColorBrush color, Visibility visibility = Visibility.Visible)
        {
            _textContent = content;
            _textColor = color;
            _isShowText = visibility;
            NotifyPropertyChanged();
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
        public SelectRectangleArgs(Rect rect)
        {
            SelectedRectangle = rect;
        }
    }
}
