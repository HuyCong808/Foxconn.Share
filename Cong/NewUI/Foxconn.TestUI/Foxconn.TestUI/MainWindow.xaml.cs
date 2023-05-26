using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Foxconn.TestUI.Config;
using Foxconn.TestUI.Enums;
using Foxconn.TestUI.OpenCV;
using Newtonsoft.Json;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Foxconn.TestUI.Editor;
using System.Collections.Generic;
using Foxconn.TestUI.Camera;
using Point = System.Drawing.Point;

namespace Foxconn.TestUI
{
    public partial class MainWindow : Window
    {
        private Board _program
        {
            get => ProgramManager.Instance.Program;
            set => ProgramManager.Instance.Program = value;
        }
        private SelectionMouse _mouse { get; set; }
        private bool _drawing { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        private Rectangle _rect { get; set; }
        private double currentZoom = 1.0;
        DeviceManager device = DeviceManager.Current;
        MachineParams param = MachineParams.Current;
        public MainWindow()
        {
            InitializeComponent();
            _mouse = new SelectionMouse();
            _drawing = false;
            MachineParams.Reload();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUp();

        }
        public void StartUp()
        {
            Console.WriteLine("Startup Application");
            ProgramManager.Instance.OpenProgram();
            LoadFOV();
            DeviceManager.Current.Open();
            cmbCameraType.ItemsSource = Enum.GetValues(typeof(CameraType));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DeviceManager.Current.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Bạn có muốn đóng ứng dụng không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Kiểm tra kết quả lựa chọn
            if (result == MessageBoxResult.No)
            {
                // Nếu người dùng chọn "No", hủy đóng ứng dụng
                e.Cancel = true;
            }
        }


        #region Camera
        private void btnGrabFrame_Click(object sender, RoutedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            ICamera pCamera = GetFOVParams(_program.FOVs[index].CameraMode);  // lay camera
            if (pCamera != null)
            {
                using (Bitmap bmp = GetFOVBitmap(pCamera, _program.FOVs[index])) // lay anh bitmap theo camera
                {
                    if (bmp != null)
                    {
                        SaveBitmapWithTimestamp(bmp);
                    }
                }
            }
        }
        public void SaveBitmapWithTimestamp(Bitmap bitmap)
        {
            int index = cmbFOV.SelectedIndex;

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"image_{timestamp}.png";
            string path = Path.Combine("data\\image", fileName);
            _program.FOVs[index].PathImage = path;
            // Lưu ảnh Bitmap vào tệp
            bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
        private Bitmap GetFOVBitmap(ICamera pCamera, FOV pFOV)
        {
            pCamera.SetParameter(KeyName.ExposureTime, pFOV.ExposureTime);
            pCamera.ClearImageBuffer();
            pCamera.SetParameter(KeyName.TriggerSoftware, 1);
            using (Bitmap bmp = pCamera.GrabFrame())
            {
                if (bmp != null)
                {
                    return (Bitmap)bmp.Clone();
                }
            }
            return null;
        }
        private ICamera GetFOVParams(CameraMode mode)
        {
            DeviceManager device = DeviceManager.Current;
            ICamera camera;
            if (mode == CameraMode.Top)
            {
                camera = device.Camera1;
            }
            else if (mode == CameraMode.Bottom)
            {
                camera = device.Camera2;
            }
            else
            {
                camera = null;
            }
            return camera;
        }
        public void GetImage(string pathcapture)
        {
            if (pathcapture != null)
            {
                var img = new Bitmap(pathcapture).ToImage<Bgr, byte>();
                _image = img;
                imbCamera.Image = img;

            }
        }
        #endregion


        #region Load
        public void LoadFOV()
        {
            cmbFOV.Items.Clear();
            foreach (var itemFOV in _program.FOVs)
            {
                cmbFOV.Items.Add(itemFOV.Name);
            }
        }

        public void LoadSMD()
        {
            cmbSMD.Items.Clear();
            var index = cmbFOV.SelectedIndex;
            foreach (var itemSMD in _program.FOVs[index].SMDs)
            {
                cmbSMD.Items.Add(itemSMD.Name);
            }
        }
        private void LoadFOVProperties()
        {
            int index = cmbFOV.SelectedIndex;
            DataContext = _program.FOVs[index];
            cmbFOVType.ItemsSource = Enum.GetValues(typeof(FOVType)).Cast<FOVType>();
            cmbCameraType.ItemsSource = Enum.GetValues(typeof(CameraMode)).Cast<CameraMode>();
        }

        private void FOVProperties()
        {
            txtIDFOV.DataContext = DataContext;
            txtNameFOV.DataContext = DataContext;
            txtExposureTimeFOV.DataContext = DataContext;
            cmbFOVType.DataContext = DataContext;
            cmbCameraType.DataContext = DataContext;
        }

        private void cmbFOV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            if (index > -1)
            {
                LoadFOVProperties();
                FOVProperties();
                LoadSMD();
                GetImage(_program.FOVs[index].PathImage);
            }
        }

        private void cmbAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAlgorithm.SelectedIndex == 1)
            {
                tabAlgorithm.SelectedIndex = 1;
            }
            if (cmbAlgorithm.SelectedIndex == 2)
            {
                tabAlgorithm.SelectedIndex = 0;
            }
            if (cmbAlgorithm.SelectedIndex == 3)
            {
                tabAlgorithm.SelectedIndex = 2;
            }
            if (cmbAlgorithm.SelectedIndex == 4)
            {
                tabAlgorithm.SelectedIndex = 3;
            }
        }

        private void cmbSMD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            int index2 = cmbSMD.SelectedIndex;
            int indexSMD = cmbSMDType.SelectedIndex;
            if (index2 > -1 && index > -1)
            {
                DataContext = _program.FOVs[index].SMDs[index2];
                cmbSMDType.ItemsSource = Enum.GetValues(typeof(SMDType)).Cast<SMDType>();
                cmbAlgorithm.ItemsSource = Enum.GetValues(typeof(Algorithm)).Cast<Algorithm>();
                cmbMode.ItemsSource = Enum.GetValues(typeof(CodeMode)).Cast<CodeMode>();
                cmbFormat.ItemsSource = Enum.GetValues(typeof(CodeFormat)).Cast<CodeFormat>();
                txtpreflix.DataContext = DataContext;
                txtLength.DataContext = DataContext;
                txtSTM_Low.DataContext = DataContext;
                txtSTM_Up.DataContext = DataContext;
                txtTMScore.DataContext = DataContext;
                LoadROI();
            }
        }

        #endregion


        #region Button

        private void btnAddFOV_Click_1(object sender, RoutedEventArgs e)
        {
            _program.AddFOV();
            LoadFOV();
        }
        private void btnAddSMD_Click_1(object sender, RoutedEventArgs e)
        {
            int fovIndex = cmbFOV.SelectedIndex;
            if (fovIndex > -1)
            {
                _program.AddSMD(fovIndex);
                LoadSMD();
            }
        }
        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnDelete_Click_1(object sender, RoutedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            int index2 = cmbSMD.SelectedIndex;
            if (index2 > -1)
            {
                _program.FOVs[index].SMDs.RemoveAt(index2);
                _program.SortSMD(index);
                LoadSMD();
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            int index2 = cmbSMD.SelectedIndex;
            imbTemplate.Image = _program.FOVs[index].SMDs[index2].TemplateMatching.Template;
        }
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                ProgramManager.Instance.SaveProgram();
                System.Windows.MessageBox.Show("Save");
            }
            else if (e.Key == Key.F5)
            {
                mnuiTest_Click(null, null);
            }
        }
        private void UpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            int fovIndex = cmbFOV.SelectedIndex;
            int smdIndex = cmbSMD.SelectedIndex;
            if (fovIndex > -1 && smdIndex > -1)
            {
                var fov = _program.FOVs.Find(x => x.ID == fovIndex);
                if (fov != null)
                {
                    var smd = fov.SMDs.Find(x => x.Id == smdIndex);
                    if (smd != null)
                    {
                        DataContext = _program.FOVs[fovIndex].SMDs[smdIndex];
                        if (_mouse.LastRectangle != null)
                        {
                            _rect = new Rectangle(_mouse.LastRectangle.X, _mouse.LastRectangle.Y, _mouse.LastRectangle.Height, _mouse.LastRectangle.Width);
                            if (_image != null)
                            {
                                // Hien thi dst.Clone() ra imageBox
                                var dst = _image.Clone();
                                // Vẽ 1 hình chữ nhật trên ảnh
                                dst.ROI = _rect;
                                dst.Save("data\\image1.jpg");
                                _program.FOVs[fovIndex].SMDs[smdIndex].TemplateMatching.Template = dst.Copy();
                            }
                        }
                    }
                }
            }
        }
        private void btnOpenImage_Click_1(object sender, RoutedEventArgs e)
        {
            string PathCapture = @"anhupdate.bmp";
            var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
            _image = img;
            imbCamera.Image = img;
        }

        private void GetHSVColorButton_Click(object sender, RoutedEventArgs e)
        {
            int fovIndex = cmbFOV.SelectedIndex;
            int smdIndex = cmbSMD.SelectedIndex;
            if (fovIndex > -1 && smdIndex > -1)
            {
                var fov = _program.FOVs.Find(x => x.ID == fovIndex);
                if (fov != null)
                {
                    var smd = fov.SMDs.Find(x => x.Id == smdIndex);
                    if (smd != null)
                    {
                        if (_mouse.LastRectangle != null)
                        {
                            _rect = new Rectangle(_mouse.LastRectangle.X, _mouse.LastRectangle.Y, _mouse.LastRectangle.Height, _mouse.LastRectangle.Width);
                            if (_image != null)
                            {
                                _image.ROI = _rect;
                                (ValueRange H, ValueRange S, ValueRange V) = _program.FOVs[fovIndex].SMDs[smdIndex].HSVExtraction.HSVRange(_image.Copy());
                                {
                                    _program.FOVs[fovIndex].SMDs[smdIndex].HSVExtraction.Hue = H;
                                    _program.FOVs[fovIndex].SMDs[smdIndex].HSVExtraction.Saturation = S;
                                    _program.FOVs[fovIndex].SMDs[smdIndex].HSVExtraction.Value = V;
                                    _image.ROI = new Rectangle();
                                }
                                _rect = new Rectangle();
                            }
                        }
                    }
                }
            }
        }

        private void GetSMD()
        {

        }

        private void btnSelectROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_drawing && imbCamera.Image != null)
                {
                    _drawing = true;
                }
                else
                {
                    _drawing = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void mnuiTechnicalSupport_Click(object sender, RoutedEventArgs e)
        {
            string message = "Engineer: Ha Huy Cong \r\nMobile: (+84)385-349-667";
            System.Windows.MessageBox.Show(message, "Technical Support", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
        }
        #endregion


        #region Mouse Event

        private void imbCamera_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            var position = $"x = {point.X}, y = {point.Y}";
            lblPosition.Content = position;
            if (e.Button != System.Windows.Forms.MouseButtons.Left || imbCamera.Image == null)
                return;
            if (_mouse.IsMouseDown && !point.IsEmpty)
            {
                _mouse.EndPoint = point;
                using (var imageDraw = _image.Clone())
                {
                    var color = _drawing ? new Bgr(65, 205, 40) : new Bgr(48, 59, 255);
                    imageDraw.Draw(_mouse.Rectangle(), color, 2);
                    imbCamera.Image = imageDraw;
                    imbCamera.Refresh();
                }
            }
        }

        private void imbCamera_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            if (!point.IsEmpty)
            {
                _mouse.IsMouseDown = true;
                _mouse.StartPoint = point;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbCamera.SetZoomScale(1, e.Location);
                imbCamera.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbCamera_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ImageBox imageBox = (ImageBox)sender;
                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

                // Thêm các ToolStripMenuItem vào ContextMenuStrip
                ToolStripMenuItem menuItem = new ToolStripMenuItem("Draw");
                menuItem.Click += MenuItem_Click1;

                ToolStripMenuItem menuItem2 = new ToolStripMenuItem("Save");
                menuItem2.Click += MenuItem_Click2;

                ToolStripMenuItem menuItem3 = new ToolStripMenuItem("Reset");
                menuItem2.Click += MenuItem_Click3;

                contextMenuStrip.Items.Add(menuItem);
                contextMenuStrip.Items.Add(menuItem2);
                contextMenuStrip.Items.Add(menuItem3);


                // Hiển thị ContextMenuStrip tại vị trí chuột phải được nhấp
                contextMenuStrip.Show(imageBox, e.Location);
            }

            _mouse.Clear();
        }
        private void btnSlectROI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_drawing && imbCamera.Image != null)
                {
                    _drawing = true;
                }
                else
                {
                    _drawing = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void imbCamera_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Lấy giá trị cuộn chuột
            int delta = e.Delta;

            // Lấy tỷ lệ zoom hiện tại
            double currentZoom = imbCamera.ZoomScale;

            // Tính toán tỷ lệ zoom mới
            double newZoom = currentZoom + (delta > 0 ? 0.2 : -0.2);

            // Giới hạn tỷ lệ zoom trong khoảng từ 0.1 đến 10 (tuỳ ý)
            newZoom = Math.Max(0.1, Math.Min(10, newZoom));

            // Đặt tỷ lệ zoom mới
            imbCamera.SetZoomScale(newZoom, new System.Drawing.Point(e.X, e.Y));
        }
        private void imbCamera_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {

        }

        #endregion


        #region ROI

        public void LoadROI()
        {
            int fovIndex = cmbFOV.SelectedIndex;
            int smdIndex = cmbSMD.SelectedIndex;
            if (fovIndex > -1 && smdIndex > -1)
            {
                var fov = _program.FOVs.Find(x => x.ID == fovIndex);
                if (fov != null)
                {
                    var smd = fov.SMDs.Find(x => x.Id == smdIndex);
                    if (smd != null)
                    {

                        GetRect(smd);
                        txtROIX.Text = smd.ROI.X.ToString();
                        txtROIY.Text = smd.ROI.Y.ToString();
                        txtROIW.Text = smd.ROI.Width.ToString();
                        txtROIH.Text = smd.ROI.Height.ToString();
                    }
                }
            }
        }
        public void ReloadROIArea()
        {
            int fovIndex = cmbFOV.SelectedIndex;
            int smdIndex = cmbSMD.SelectedIndex;
            if (fovIndex > -1 && smdIndex > -1)
            {
                var smd = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
                if (smd != null)
                {
                    smd.ROI.Width = int.Parse(txtROIW.Text);
                    smd.ROI.Height = int.Parse(txtROIH.Text);
                    LoadROI();
                }
            }

        }
        public BRectangle GetRect(SMD smd)
        {
            int X = smd.ROI.X;
            int Y = smd.ROI.Y;
            int Width = smd.ROI.Width;
            int Height = smd.ROI.Height;

            _rect = new Rectangle(X, Y, Width, Height);

            if (_image != null)
            {
                // Hien thi dst.Clone() ra imageBox
                var dst = _image.Clone();
                // Vẽ 1 hình chữ nhật trên ảnh
                dst.Draw(_rect, new Bgr(System.Drawing.Color.Green), 4);
                // Hiển thị ảnh với 1 vùng ROI
                imbCamera.Image = dst.Clone();
                //  SaveImage();
            }
            return smd.ROI;
        }
        public void SaveROI(int smdId)
        {
            int fovIndex = cmbFOV.SelectedIndex;
            int smdIndex = cmbFOV.SelectedIndex;
            if (fovIndex > -1 && smdIndex > -1)
            {
                var currentSMD = _program.FOVs[fovIndex].SMDs.Find(x => x.Id == smdId);
                if (currentSMD != null)
                {
                    if (_mouse.LastRectangle != null)
                    {
                        int X = currentSMD.ROI.X = _mouse.LastRectangle.X;
                        int Y = currentSMD.ROI.Y = _mouse.LastRectangle.Y;
                        int Width = currentSMD.ROI.Width = _mouse.LastRectangle.Width;
                        int Height = currentSMD.ROI.Height = _mouse.LastRectangle.Height;

                        Console.WriteLine("X : " + _mouse.LastRectangle.X);
                        Console.WriteLine("Y : " + _mouse.LastRectangle.Y);
                        Console.WriteLine("Width : " + _mouse.LastRectangle.Width);
                        Console.WriteLine("Height : " + _mouse.LastRectangle.Height);
                    }
                }
            }

        }
        private void btnSaveROI_Click_1(object sender, RoutedEventArgs e)
        {
            int index2 = cmbSMD.SelectedIndex;
            if (index2 > -1)
            {
                try
                {
                    if (_drawing == true)
                    {
                        SaveROI(index2);
                        LoadROI();
                        _drawing = false;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
        }
        #endregion


        #region MenuItem
        private void MenuItem_Click1(object sender, EventArgs e)
        {
            try
            {
                if (!_drawing && imbCamera.Image != null)
                {
                    _drawing = true;
                }
                else
                {
                    _drawing = false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void MenuItem_Click2(object sender, EventArgs e)
        {
            int index2 = cmbSMD.SelectedIndex;
            if (index2 > -1)
            {
                try
                {
                    if (_drawing == true)
                    {
                        SaveROI(index2);
                        LoadROI();
                        _drawing = false;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
        }
        private void MenuItem_Click3(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

        }

        #endregion


        #region Test
        private void mnuiTest_Click(object sender, RoutedEventArgs e)
        {
            string PathCapture = @"anhupdate.bmp";
            var img = new Bitmap(PathCapture);
            if (img != null)
            {
                using (Image<Bgr, byte> src = img.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> dst = img.ToImage<Bgr, byte>())
                {
                    IEnumerable<SMD> pSMDs = _program.FOVs[0].SMDs;
                    foreach (SMD pSMD in pSMDs)
                    {
                        src.ROI = pSMD.ROI.Rectangle;
                        dst.ROI = pSMD.ROI.Rectangle;
                        switch (pSMD.Algorithm)
                        {
                            case Algorithm.Unknow:
                                break;
                            case Algorithm.TemplateMatching:
                                {
                                    CvResult cvRet = pSMD.TemplateMatching.Run(src.Copy(), dst, src.ROI);
                                    if (cvRet.Result)
                                    {
                                        pSMD.TemplateMatching.Center = new JPoint { X = cvRet.Center.X, Y = cvRet.Center.Y };
                                    }
                                    pSMD.TemplateMatching.Center.X = cvRet.Center.X;
                                    pSMD.TemplateMatching.Center.Y = cvRet.Center.Y;
                                    string message = $"{pSMD.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nCenter: {cvRet.Center}";
                                    System.Windows.MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case Algorithm.CodeRecognition:
                                {
                                    CvResult cvRet = pSMD.CodeRecognition.Run(src.Copy(), dst, src.ROI);
                                    string message = $"{pSMD.Algorithm}: {cvRet.Result}\r\nSN: {cvRet.Content}\r\nLength: {cvRet.Content.Length}";
                                    System.Windows.MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                                    break;
                                }
                            case Algorithm.HSVExtraction:
                                {
                                    CvResult cvRet = pSMD.HSVExtraction.Preview(src.Copy(), dst, src.ROI);
                                    string message = $"{pSMD.Algorithm}: {cvRet.Result}\r\nScore: {cvRet.Score}\r\nQty: {cvRet.Qty}";
                                    txt_ScoreHSV.Text = cvRet.Score.ToString();
                                    System.Windows.MessageBox.Show(message, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly); break;
                                }
                            default:
                                break;
                        }
                        src.ROI = new System.Drawing.Rectangle();
                        dst.ROI = new System.Drawing.Rectangle();
                        Dispatcher.Invoke(() =>
                        {
                            imbCamera.Image = dst.Clone();
                        });
                    }
                }
            }
        }
        #endregion



        private void mnuiAutorun_click(object sender, RoutedEventArgs e)
        {
            // Tạo một instance mới của form mới
            AutoRun newWindow = new AutoRun();

            // Hiển thị form mới
            newWindow.Show();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
        }
       

    }
}
