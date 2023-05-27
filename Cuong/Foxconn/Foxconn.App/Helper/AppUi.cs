using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.App.Helper.Enums;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Foxconn.App.Helper
{
    public class AppUi
    {
        public static void ShowConsole(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void ShowWindow(Window root, Window newWindow, bool waiting = false)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (waiting)
                {
                    newWindow.Owner = root;
                    newWindow.ShowDialog();
                    newWindow.Close();
                }
                else
                {
                    newWindow.Owner = root;
                    newWindow.Show();
                }
            });
        }

        public static object FindElement(Window window, string elementName)
        {
            object item = window.FindName(elementName);
            return item;
        }

        private static SolidColorBrush ConvertDrawingColorToBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public static void ShowDataGrid(Window root, DataGrid dataGrid, string message, AppColor coreColor = AppColor.None)
        {
            root.Dispatcher.Invoke(() =>
            {
                string currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff");
                Brush Color = null;
                Color = coreColor switch
                {
                    AppColor.None => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                    AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                    AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                    AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                    AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                    AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                    AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                    AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                    AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                    AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                    AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                    AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                    AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                    AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                    AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                    AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                    AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                    AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                    AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                    AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                    AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                    _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                };
                dataGrid.Items.Add(new ViewModels.DataGridViewModel() { DateTime = currentTime, Information = message, BrushForBackGroundColor = Color });
                dataGrid.Items.Refresh();
                dataGrid.ScrollIntoView(dataGrid.Items.GetItemAt(dataGrid.Items.Count - 1));   // if small data datagrid
                Logger.Instance.Write(message, LoggerLevel.Info);
            });
        }

        public static void ClearDataGrid(Window root, DataGrid dataGrid, int numberRow = 0)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (numberRow == 0)
                {
                    dataGrid.Items.Clear();
                }
                else
                {
                    if (dataGrid.Items.Count >= numberRow)
                    {
                        dataGrid.Items.Clear();
                    }
                }
            });
        }

        private static ImageSource BitmapFromUri(Uri source)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = source;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource bitmapsource)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                bitmapEncoder.Save(memoryStream);
                using (System.Drawing.Bitmap tempBitmap = new System.Drawing.Bitmap(memoryStream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new System.Drawing.Bitmap(tempBitmap);
                }
            }
        }

        public static void ShowImage(Window root, Image image, System.Drawing.Bitmap bitmap)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (bitmap != null)
                {
                    image.Source = BitmapToImageSource(bitmap);
                }
            });
        }

        public static void ShowImage(Window root, string imageName, System.Drawing.Bitmap bitmap)
        {
            root.Dispatcher.Invoke(() =>
            {
                var image = (Image)FindElement(root, imageName);
                if (bitmap != null && image != null)
                {
                    image.Source = BitmapToImageSource(bitmap);
                }
            });
        }

        public static void ShowImage(Window root, Image image, string filePath)
        {
            root.Dispatcher.Invoke(() =>
            {
                string fullPath = Path.GetFullPath(filePath);
                if (File.Exists(fullPath))
                {
                    image.Source = BitmapFromUri(new Uri(fullPath));
                }
            });
        }

        public static void ShowImage(Window root, string imageName, string filePath)
        {
            root.Dispatcher.Invoke(() =>
            {
                string fullPath = Path.GetFullPath(filePath);
                var image = (Image)FindElement(root, imageName);
                if (File.Exists(fullPath) && image != null)
                {
                    image.Source = BitmapFromUri(new Uri(fullPath));
                }
            });
        }

        public static void ClearImage(Window root, Image image)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (image.Source != null)
                {
                    image.Source = null;
                }
            });
        }

        public static void ClearImage(Window root, string imageName)
        {
            root.Dispatcher.Invoke(() =>
            {
                var image = (Image)FindElement(root, imageName);
                if (image.Source != null)
                {
                    image.Source = null;
                }
            });
        }

        public static void ShowImageBox(Window root, Emgu.CV.UI.ImageBox imageBox, System.Drawing.Bitmap bitmap)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (bitmap != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = bitmap.ToImage<Bgr, byte>();
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowImageBox(Window root, string imageName, System.Drawing.Bitmap bitmap)
        {
            root.Dispatcher.Invoke(() =>
            {
                var imageBox = (Emgu.CV.UI.ImageBox)FindElement(root, imageName);
                if (bitmap != null && imageBox != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = bitmap.ToImage<Bgr, byte>();
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowImageBox(Window root, Emgu.CV.UI.ImageBox imageBox, Image<Bgr, byte> image)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (image != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = image;
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowImageBox(Window root, string imageName, Image<Bgr, byte> image)
        {
            root.Dispatcher.Invoke(() =>
            {
                var imageBox = (Emgu.CV.UI.ImageBox)FindElement(root, imageName);
                if (image != null && imageBox != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = image;
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowImageBox(Window root, Emgu.CV.UI.ImageBox imageBox, string filePath)
        {
            root.Dispatcher.Invoke(() =>
            {
                string fullPath = Path.GetFullPath(filePath);
                if (File.Exists(fullPath))
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = new Image<Bgr, byte>(fullPath);
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowImageBox(Window root, string imageName, string filePath)
        {
            root.Dispatcher.Invoke(() =>
            {
                string fullPath = Path.GetFullPath(filePath);
                var imageBox = (Emgu.CV.UI.ImageBox)FindElement(root, imageName);
                if (File.Exists(fullPath) && imageBox != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = new Image<Bgr, byte>(fullPath);
                    imageBox.Refresh();
                }
            });
        }

        public static void ClearImageBox(Window root, Emgu.CV.UI.ImageBox imageBox)
        {
            root.Dispatcher.Invoke(() =>
            {
                if (imageBox.Image != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = null;
                    imageBox.Refresh();
                }
            });
        }

        public static void ClearImageBox(Window root, string imageName)
        {
            root.Dispatcher.Invoke(() =>
            {
                var imageBox = (Emgu.CV.UI.ImageBox)FindElement(root, imageName);
                if (imageBox.Image != null)
                {
                    imageBox.Image?.Dispose();
                    imageBox.Image = null;
                    imageBox.Refresh();
                }
            });
        }

        public static void ShowWindowTitle(Window root, string title = "Window")
        {
            root.Dispatcher.Invoke(() =>
            {
                root.Title = title;
            });
        }

        public static void ShowLabel(Window root, Label label, string content = "Label")
        {
            root.Dispatcher.Invoke(() =>
            {
                label.Content = content;
            });
        }

        public static void ShowLabel(Window root, string labelName, string content = "Label")
        {
            root.Dispatcher.Invoke(() =>
            {
                var label = (Label)FindElement(root, labelName);
                if (label != null)
                {
                    label.Content = content;
                }
            });
        }

        public static void ShowLabel(Window root, Label label, string content, AppColor background, AppColor foreground)
        {
            root.Dispatcher.Invoke(() =>
            {
                Brush backgroundColor = null;
                Brush foregoundColor = null;
                backgroundColor = background switch
                {
                    AppColor.None => null,
                    AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                    AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                    AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                    AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                    AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                    AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                    AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                    AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                    AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                    AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                    AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                    AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                    AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                    AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                    AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                    AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                    AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                    AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                    AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                    AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                    _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                };
                foregoundColor = foreground switch
                {
                    AppColor.None => null,
                    AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                    AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                    AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                    AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                    AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                    AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                    AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                    AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                    AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                    AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                    AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                    AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                    AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                    AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                    AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                    AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                    AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                    AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                    AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                    AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                    _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                };
                if (backgroundColor != null)
                {
                    label.Background = backgroundColor;
                }
                if (foregoundColor != null)
                {
                    label.Foreground = foregoundColor;
                }
                label.Content = content;
            });
        }

        public static void ShowLabel(Window root, string labelName, string content, AppColor background, AppColor foreground)
        {
            root.Dispatcher.Invoke(() =>
            {
                var label = (Label)FindElement(root, labelName);
                if (label != null)
                {
                    Brush backgroundColor = null;
                    Brush foregoundColor = null;
                    backgroundColor = background switch
                    {
                        AppColor.None => null,
                        AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                        AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                        AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                        AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                        AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                        AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                        AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                        AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                        AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                        AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                        AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                        AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                        AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                        AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                        AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                        AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                        AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                        AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                        AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                        AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                        _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                    };
                    foregoundColor = foreground switch
                    {
                        AppColor.None => null,
                        AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                        AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                        AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                        AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                        AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                        AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                        AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                        AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                        AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                        AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                        AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                        AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                        AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                        AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                        AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                        AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                        AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                        AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                        AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                        AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                        _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                    };
                    if (backgroundColor != null)
                    {
                        label.Background = backgroundColor;
                    }
                    if (foregoundColor != null)
                    {
                        label.Foreground = foregoundColor;
                    }
                    label.Content = content;
                }
            });
        }

        public static void ShowBorderBackground(Window root, Border border, AppColor coreColor = AppColor.None)
        {
            root.Dispatcher.Invoke(() =>
            {
                Brush Color = null;
                Color = coreColor switch
                {
                    AppColor.None => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                    AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                    AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                    AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                    AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                    AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                    AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                    AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                    AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                    AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                    AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                    AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                    AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                    AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                    AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                    AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                    AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                    AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                    AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                    AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                    AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                    _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                };
                border.Background = Color;
            });
        }

        public static void ShowBorderBackground(Window root, string borderName, AppColor coreColor = AppColor.None)
        {
            root.Dispatcher.Invoke(() =>
            {
                var border = (Border)FindElement(root, borderName);
                if (border != null)
                {
                    Brush Color = null;
                    Color = coreColor switch
                    {
                        AppColor.None => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                        AppColor.Red => ConvertDrawingColorToBrush(Properties.Settings.Default.Red),
                        AppColor.Orange => ConvertDrawingColorToBrush(Properties.Settings.Default.Orange),
                        AppColor.Yellow => ConvertDrawingColorToBrush(Properties.Settings.Default.Yellow),
                        AppColor.Green => ConvertDrawingColorToBrush(Properties.Settings.Default.Green),
                        AppColor.Mint => ConvertDrawingColorToBrush(Properties.Settings.Default.Mint),
                        AppColor.Teal => ConvertDrawingColorToBrush(Properties.Settings.Default.Teal),
                        AppColor.Cyan => ConvertDrawingColorToBrush(Properties.Settings.Default.Cyan),
                        AppColor.Blue => ConvertDrawingColorToBrush(Properties.Settings.Default.Blue),
                        AppColor.Indigo => ConvertDrawingColorToBrush(Properties.Settings.Default.Indigo),
                        AppColor.Purple => ConvertDrawingColorToBrush(Properties.Settings.Default.Purple),
                        AppColor.Pink => ConvertDrawingColorToBrush(Properties.Settings.Default.Pink),
                        AppColor.Brown => ConvertDrawingColorToBrush(Properties.Settings.Default.Brown),
                        AppColor.Gray => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray),
                        AppColor.Gray2 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray2),
                        AppColor.Gray3 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray3),
                        AppColor.Gray4 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray4),
                        AppColor.Gray5 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray5),
                        AppColor.Gray6 => ConvertDrawingColorToBrush(Properties.Settings.Default.Gray6),
                        AppColor.Black => ConvertDrawingColorToBrush(Properties.Settings.Default.Black),
                        AppColor.White => ConvertDrawingColorToBrush(Properties.Settings.Default.White),
                        _ => new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush,
                    };
                    border.Background = Color;
                }
            });
        }

        public static void ShowButton(Window root, Button button, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                button.IsEnabled = state;
            });
        }

        public static void ShowButton(Window root, string buttonName, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                var button = (Button)FindElement(root, buttonName);
                if (button != null)
                {
                    button.IsEnabled = state;
                }
            });
        }

        public static void ShowItem(Window root, MenuItem menuItem, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                menuItem.IsEnabled = state;
            });
        }

        public static void ShowItem(Window root, string menuItemName, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                var button = (Button)FindElement(root, menuItemName);
                if (button != null)
                {
                    button.IsEnabled = state;
                }
            });
        }

        public static void ShowComboBox(Window root, ComboBox comboBox, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                comboBox.IsEnabled = state;
            });
        }

        public static void ShowComboBox(Window root, string comboBoxName, bool state)
        {
            root.Dispatcher.Invoke(() =>
            {
                var comboBox = (ComboBox)FindElement(root, comboBoxName);
                if (comboBox != null)
                {
                    comboBox.IsEnabled = state;
                }
            });
        }

        public static void ShowListToComboBox(Window root, ComboBox comboBox, System.Collections.Generic.List<string> list)
        {
            root.Dispatcher.Invoke(() =>
            {
                comboBox.Items.Clear();
                if (list.Count > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        comboBox.Items.Add(list[i]);
                    }
                    //comboBox.ItemsSource = list;
                }
                comboBox.Items.Refresh();
            });
        }

        public static void ShowListToComboBox(Window root, string comboBoxName, System.Collections.Generic.List<string> list)
        {
            root.Dispatcher.Invoke(() =>
            {
                var comboBox = (ComboBox)FindElement(root, comboBoxName);
                if (comboBox != null)
                {
                    comboBox.Items.Clear();
                    comboBox.ItemsSource = list;
                    comboBox.Items.Refresh();
                }
            });
        }

        public static void ShowToolTipButton(Window root, Button button, string content)
        {
            root.Dispatcher.Invoke(() =>
            {
                button.ToolTip = content;
            });
        }

        public static void ShowToolTipButton(Window root, string buttonName, string content)
        {
            root.Dispatcher.Invoke(() =>
            {
                var button = (Button)FindElement(root, buttonName);
                if (button != null)
                {
                    button.ToolTip = content;
                }
            });
        }

        public static void ShowGroupBoxName(Window root, GroupBox groupBox, string content)
        {
            root.Dispatcher.Invoke(() =>
            {
                groupBox.Header = content;
            });
        }

        public static void ShowGroupBoxName(Window root, string groupBoxName, string content)
        {
            root.Dispatcher.Invoke(() =>
            {
                var groupBox = (GroupBox)FindElement(root, groupBoxName);
                if (groupBox != null)
                {
                    groupBox.Header = content;
                }
            });
        }

        public static MessageBoxResult ShowMessage(string messageBoxText, MessageBoxImage messageBoxImage = MessageBoxImage.None)
        {
            string caption;
            MessageBoxButton button;
            MessageBoxImage icon;
            MessageBoxResult defaultResult;
            MessageBoxOptions options;
            MessageBoxResult result;
            if (messageBoxImage == MessageBoxImage.Information)
            {
                caption = "Information";
                button = MessageBoxButton.OKCancel;
                icon = MessageBoxImage.Information;
                defaultResult = MessageBoxResult.OK;
                options = MessageBoxOptions.DefaultDesktopOnly;
                result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
            else if (messageBoxImage == MessageBoxImage.Warning)
            {
                caption = "Warning";
                button = MessageBoxButton.OK;
                icon = MessageBoxImage.Warning;
                defaultResult = MessageBoxResult.OK;
                options = MessageBoxOptions.DefaultDesktopOnly;
                result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
            else if (messageBoxImage == MessageBoxImage.Error)
            {
                caption = "Error";
                button = MessageBoxButton.OK;
                icon = MessageBoxImage.Error;
                defaultResult = MessageBoxResult.OK;
                options = MessageBoxOptions.DefaultDesktopOnly;
                result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
            else if (messageBoxImage == MessageBoxImage.Question)
            {
                caption = "Question";
                button = MessageBoxButton.YesNoCancel;
                icon = MessageBoxImage.Question;
                defaultResult = MessageBoxResult.Cancel;
                options = MessageBoxOptions.DefaultDesktopOnly;
                result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
            }
            else
            {
                result = MessageBox.Show(messageBoxText);
            }
            return result;
        }
    }
}
