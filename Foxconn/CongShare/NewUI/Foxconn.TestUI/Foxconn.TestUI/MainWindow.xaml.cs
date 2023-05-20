using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.TestUI.Config;
using Foxconn.TestUI.Enums;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Foxconn.TestUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Board Board { get; set; }
        private SelectionMouse _mouse { get; set; }
        private bool _drawing { get; set; }
        private Image<Bgr, byte> _image { get; set; }
        private Rectangle _rect { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            _mouse = new SelectionMouse();
            _drawing = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Params\\board.json"))
            {
                var data = JsonConvert.DeserializeObject<Board>(File.ReadAllText("Params\\board.json"));
                if (data != null)
                {
                    Board = data;
                }
                LoadFOV();
            }
            else
            {
                Board = new Board();
                Board.Name = "TEST_PROGRAM";
            }
           // MachineParams.Reload();
            DeviceManager.Current.Open();

        }


        private void Window_Closed(object sender, EventArgs e)
        {
            var data = JsonConvert.SerializeObject(Board);
            File.WriteAllText("Params\\board.json", data);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
        private void btnAddFOV_Click(object sender, RoutedEventArgs e)
        {
            Board.AddFOV();
            LoadFOV();
        }

        private void btnAddSMD_Click(object sender, RoutedEventArgs e)
        {
            int fovIndex = cmbFOV.SelectedIndex;
            if (fovIndex > -1)
            {
                Board.AddSMD(fovIndex);
                LoadSMD();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            int index2 = cmbSMD.SelectedIndex;
            if (index2 > -1)
            {
                Board.FOVs[index].SMDs.RemoveAt(index2);
                Board.SortSMD(index);
                LoadSMD();
            }
        }
        private void btnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            string PathCapture = @"taytrai.bmp";
            var img = new Bitmap(PathCapture).ToImage<Bgr, byte>();
            _image = img;
            imbCamera.Image = img;
            var barcode = string.Empty;
        }


        #region Load
        public void LoadFOV()
        {
            cmbFOV.Items.Clear();
            foreach (var itemFOV in Board.FOVs)
            {
                cmbFOV.Items.Add(itemFOV.Name);
            }
        }
        public void LoadSMD()
        {
            cmbSMD.Items.Clear();
            var index = cmbFOV.SelectedIndex;
            foreach (var itemSMD in Board.FOVs[index].SMDs)
            {
                cmbSMD.Items.Add(itemSMD.Name);
            }
        }
        private void LoadFOVProperties()
        {
            int index = cmbFOV.SelectedIndex;
            DataContext = Board.FOVs[index];
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

            }
        }

        private void cmbSMD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cmbFOV.SelectedIndex;
            int index2 = cmbSMD.SelectedIndex;
            int indexSMD = cmbSMDType.SelectedIndex;
            if (index2 > -1 && index > -1)
            {
                DataContext = Board.FOVs[index].SMDs[index2];
                cmbSMDType.ItemsSource = Enum.GetValues(typeof(SMDType)).Cast<SMDType>();
               // cmbAlgorithm.ItemsSource = Enum.GetValues(typeof(Algorithm)).Cast<Algorithm>();
                LoadROI();

            }


        }
        #endregion


        #region Mouse Event

        private void imbCamera_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            var position = $"x = {point.X}, y = {point.Y}";
            if (e.Button != System.Windows.Forms.MouseButtons.Left || imbCamera.Image == null)
                return;
            if (_mouse.IsMouseDown && !point.IsEmpty)
            {
                _mouse.EndPoint = point;
                using (var imageDraw = _image.Clone())
                {
                    var color = _drawing ? new Bgr(65, 205, 40) : new Bgr(48, 59, 255);
                    imageDraw.Draw(_mouse.Rectangle(), color, 1);
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
                var fov = Board.FOVs.Find(x => x.ID == fovIndex);
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
                var smd = Board.FOVs[fovIndex].SMDs.Find(x => x.Id == smdIndex);
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
                var currentSMD = Board.FOVs[fovIndex].SMDs.Find(x => x.Id == smdId);
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

                        //     Khởi tạo một đối tượng Image<Bgr, byte> từ một hình ảnh có chứa vùng ROI
                        var image = new Image<Bgr, byte>(@"taytrai.bmp");
                        var roi = new Rectangle(X, Y, Width, Height);
                        // Cắt ảnh từ vùng ROI
                        var roiImage = image.Copy(roi);
                        // Lưu ảnh cắt được ra file
                        // roiImage.Save(@"temp\roiSMD" + smdId + ".png");
                    }
                }
            }

        }
        private void btnSaveROI_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show(ex.ToString());
                }
            }

        }


        #endregion

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
