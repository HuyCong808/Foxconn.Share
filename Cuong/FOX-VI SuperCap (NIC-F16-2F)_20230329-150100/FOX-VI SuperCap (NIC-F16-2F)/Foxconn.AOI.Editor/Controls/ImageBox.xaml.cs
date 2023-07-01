using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.AOI.Editor.Controls
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
        private Point _startPoint = new Point();
        private bool _isSelectedRect = false;
        private Rect _selectedRect = new Rect();
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
                _selectedRect = new Rect();
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
            set => _isDrawing = value;
        }

        public bool IsSelectedRect
        {
            get => _isSelectedRect;
            set
            {
                _isSelectedRect = value;
                if (_isSelectedRect)
                {
                    Site.Cursor = Cursors.Cross;
                }
                else
                {
                    Site.Cursor = Cursors.Arrow;
                }
                NotifyPropertyChanged();
            }
        }

        public Rect SelectedRect
        {
            get => _selectedRect;
            set
            {
                if (value == null)
                    _selectedRect = new Rect();
                else
                    _selectedRect = value;
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
            get => Rect.Stroke;
            set => Rect.Stroke = value;
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

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (_selectedRectByCtrl && _source != null)
                {
                    _isSelectedRect = true;
                }
            }
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (_selectedRectByCtrl)
                {
                    _isSelectedRect = false;
                }
            }
        }

        private void imageBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSelectedRect)
            {
                _startPoint = Mouse.GetPosition(sender as UIElement);
                _isDrawing = true;
            }
        }

        private void imageBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelectedRect && _isDrawing)
            {
                Point p = Mouse.GetPosition(sender as UIElement);
                double x = Math.Min(p.X, _startPoint.X);
                double y = Math.Min(p.Y, _startPoint.Y);
                double w = Math.Abs(p.X - _startPoint.X);
                double h = Math.Abs(p.Y - _startPoint.Y);
                Rect.Width = w;
                Rect.Height = h;
                Rect.Margin = new Thickness(x, y, 0, 0);
                Rect.Visibility = Visibility.Visible;
            }
            Point currentPoint = GetMousePoint();
            Color currentColor = GetPixelColor((BitmapSource)_source, (int)currentPoint.X, (int)currentPoint.Y);
            CurrentPoint = currentPoint;
            CurrentColor = currentColor;
        }

        private void imageBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSelectedRect)
            {
                _isSelectedRect = false;
                _isDrawing = false;
                GetSelectedRect();
                SetSelectedRect();
                RectangleChanged(this, new SelectRectangleArgs(_selectedRect));
            }
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
                DefaultExt = "png",
                Filter = "PNG | *.png"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var image = (BitmapSource)_source.Clone();
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);
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
            _selectedRect.X = Rect.Margin.Left / _scale;
            _selectedRect.Y = Rect.Margin.Top / _scale;
            _selectedRect.Width = Rect.Width / _scale;
            _selectedRect.Height = Rect.Height / _scale;
            Console.WriteLine($"Rect: X[{(int)_selectedRect.X}], Y[{(int)_selectedRect.Y}], Width[{(int)_selectedRect.Width}], Height[{(int)_selectedRect.Height}]");
        }

        public void SetSelectedRect()
        {
            double x = _selectedRect.X * _scale;
            double y = _selectedRect.Y * _scale;
            double w = _selectedRect.Width * _scale;
            double h = _selectedRect.Height * _scale;
            Rect.Width = w;
            Rect.Height = h;
            Rect.Margin = new Thickness(x, y, 0, 0);
        }

        public void ClearSelectedRect()
        {
            _selectedRect = new Rect();
            _isSelectedRect = false;
            if (_isSelectedRect)
            {
                Site.Cursor = Cursors.Cross;
            }
            else
            {
                Site.Cursor = Cursors.Arrow;
            }
            SetSelectedRect();
            RectangleChanged(this, new SelectRectangleArgs(_selectedRect));
            NotifyPropertyChanged();
        }

        public void DrawRectangle(System.Drawing.Rectangle rect)
        {
            ClearSelectedRect();
            _selectedRect.X = rect.X;
            _selectedRect.Y = rect.Y;
            _selectedRect.Width = rect.Width;
            _selectedRect.Height = rect.Height;
            SetSelectedRect();
            RectangleChanged(this, new SelectRectangleArgs(_selectedRect));
            Rect.Visibility = Visibility.Visible;
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
