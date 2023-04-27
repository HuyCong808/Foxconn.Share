using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foxconn
{
    public class SelectionMouse
    {
        public bool IsMouseDown { get; set; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Rectangle LastRectangle { get; set; }

        public SelectionMouse()
        {
            IsMouseDown = false;
        }

        public Rectangle Rectangle()
        {
            if (IsMouseDown && StartPoint != Point.Empty && EndPoint != Point.Empty)
            {
                Rectangle rect = new Rectangle();
                rect.X = Math.Min(StartPoint.X, EndPoint.X);
                rect.Y = Math.Min(StartPoint.Y, EndPoint.Y);
                rect.Width = Math.Abs(StartPoint.X - EndPoint.X);
                rect.Height = Math.Abs(StartPoint.Y - EndPoint.Y);
                return rect;


            }
            else
            {
                return System.Drawing.Rectangle.Empty;
            }
        }
        public bool IsEmpty => !(IsMouseDown && StartPoint != Point.Empty && EndPoint != Point.Empty);
        public static SelectionMouse Empty => new SelectionMouse();
        public void Clear()
        {
            LastRectangle = Rectangle();
            IsMouseDown = false;
            StartPoint = Point.Empty;
            EndPoint = Point.Empty;
        }

        /// <summary>
        /// Get mouse position on ImageBox of EmguCV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public Point GetMousePosition(object sender, MouseEventArgs e)
        {
            Emgu.CV.UI.ImageBox imageBox = (Emgu.CV.UI.ImageBox)sender;
            Point position = Point.Empty;
            switch (imageBox.SizeMode)
            {
                case PictureBoxSizeMode.Normal:
                    {
                        int offsetX = (int)(e.Location.X / imageBox.ZoomScale);
                        int offsetY = (int)(e.Location.Y / imageBox.ZoomScale);
                        int horizontalScrollBarValue = imageBox.HorizontalScrollBar.Visible ? imageBox.HorizontalScrollBar.Value : 0;
                        int verticalScrollBarValue = imageBox.VerticalScrollBar.Visible ? imageBox.VerticalScrollBar.Value : 0;
                        position = new Point
                        {
                            X = offsetX + horizontalScrollBarValue,
                            Y = offsetY + verticalScrollBarValue
                        };
                    }
                    break;
                case PictureBoxSizeMode.StretchImage:
                    break;
                case PictureBoxSizeMode.AutoSize:
                    break;
                case PictureBoxSizeMode.CenterImage:
                    break;
                case PictureBoxSizeMode.Zoom:
                    {
                        int offsetX = (int)(e.Location.X / imageBox.ZoomScale);
                        int offsetY = (int)(e.Location.Y / imageBox.ZoomScale);
                        int horizontalScrollBarValue = imageBox.HorizontalScrollBar.Visible ? imageBox.HorizontalScrollBar.Value : 0;
                        int verticalScrollBarValue = imageBox.VerticalScrollBar.Visible ? imageBox.VerticalScrollBar.Value : 0;
                        var curPos = new Point
                        {
                            X = offsetX + horizontalScrollBarValue,
                            Y = offsetY + verticalScrollBarValue
                        };
                        position = TranslateZoomMousePosition(curPos, imageBox.Size, imageBox.PreferredSize);
                    }
                    break;
                default:
                    break;
            }
            bool result = !position.IsEmpty && 0 <= position.X && position.X <= imageBox.PreferredSize.Width && 0 <= position.Y && position.Y <= imageBox.PreferredSize.Height;
            return result ? position : Point.Empty;
        }
        /// <summary>
        /// Gets the mouse position over the image when the Image Box set sizeMode is zoom
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="imb"></param>
        /// <param name="imgSize"></param>
        /// <returns></returns>
        private Point TranslateZoomMousePosition(Point coordinates, Size imb, Size imgSize)
        {
            int Width = imb.Width, Height = imb.Height;
            // Test to make sure our image is not null
            if (imgSize.IsEmpty)
            {
                return coordinates;
            }
            // Make sure our control width and height are not 0 and our 
            // image width and height are not 0
            if (Width == 0 || Height == 0 || imgSize.Width == 0 || imgSize.Height == 0)
            {
                return coordinates;
            }
            // This is the one that gets a little tricky. Essentially, need to check 
            // the aspect ratio of the image to the aspect ratio of the control
            // to determine how it is being rendered
            float imageAspect = (float)imgSize.Width / imgSize.Height;
            float controlAspect = (float)Width / Height;
            float newX = coordinates.X;
            float newY = coordinates.Y;
            if (imageAspect > controlAspect)
            {
                // This means that we are limited by width, 
                // meaning the image fills up the entire control from left to right
                float ratioWidth = (float)imgSize.Width / Width;
                newX *= ratioWidth;
                float scale = (float)Width / imgSize.Width;
                float displayHeight = scale * imgSize.Height;
                float diffHeight = Height - displayHeight;
                diffHeight /= 2;
                newY -= diffHeight;
                newY /= scale;
            }
            else
            {
                // This means that we are limited by height, 
                // meaning the image fills up the entire control from top to bottom
                float ratioHeight = (float)imgSize.Height / Height;
                newY *= ratioHeight;
                float scale = (float)Height / imgSize.Height;
                float displayWidth = scale * imgSize.Width;
                float diffWidth = Width - displayWidth;
                diffWidth /= 2;
                newX -= diffWidth;
                newX /= scale;
            }
            return new Point((int)newX, (int)newY);
        }
    }
}
