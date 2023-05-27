using Emgu.CV;
using Emgu.CV.Structure;
using Foxconn.App.Controllers.Image;
using Foxconn.App.Controllers.Variable;
using Foxconn.App.Helper;
using Foxconn.App.Helper.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.RuntimeConfiguration;
using static Foxconn.App.Models.RuntimeConfiguration.StepConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for CameraSetupWindow.xaml
    /// </summary>
    public partial class CameraSetupWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;
        private SelectionMouse _mouse { get; set; }
        private bool _drawing { get; set; }
        private bool _streaming { get; set; }
        private Image Setup { get; set; }
        private Image Setup0 { get; set; }
        private Image Setup1 { get; set; }
        private Image Setup2 { get; set; }

        public CameraSetupWindow()
        {
            InitializeComponent();
            _mouse = new SelectionMouse();
            _drawing = false;
            _streaming = false;
            Setup0 = new Image() { Index = 0 };
            Setup1 = new Image() { Index = 1 };
            Setup2 = new Image() { Index = 2 };
            Setup = Setup0;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => { Init(); });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Root.AppManager.DatabaseManager.Runtime.StepsForCamera0 = Setup0.Steps;
            Root.AppManager.DatabaseManager.Runtime.StepsForCamera1 = Setup1.Steps;
            Root.AppManager.DatabaseManager.Runtime.StepsForCamera2 = Setup2.Steps;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Init()
        {
            try
            {
                Root.CodeFlow("CAMERA");

                #region Image Manager
                Root.AppManager.ImageManager.GetSetupConfiguration();
                Setup0 = Root.AppManager.ImageManager.Setup0;
                Setup1 = Root.AppManager.ImageManager.Setup1;
                Setup2 = Root.AppManager.ImageManager.Setup2;
                Setup = Setup0;
                #endregion

                #region Image Icon
                AppUi.ShowImage(this, imgConnect, @"Assets/Camera/Disconnected.png");
                AppUi.ShowImage(this, imgSettings, @"Assets/Settings.png");
                AppUi.ShowImage(this, imgCapture, @"Assets/Camera/Capture.png");
                AppUi.ShowImage(this, imgStreaming, @"Assets/Camera/StopStreaming.png");
                AppUi.ShowImage(this, imgDrawing, @"Assets/PencilDrawing.png");
                AppUi.ShowImage(this, imgSave, @"Assets/Target.png");

                AppUi.ShowImage(this, imgAddStep, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveStep, @"Assets/Subtract.png");
                AppUi.ShowImage(this, imgAddComponent, @"Assets/Plus.png");
                AppUi.ShowImage(this, imgRemoveComponent, @"Assets/Subtract.png");
                #endregion

                #region ComboBox
                Dispatcher.Invoke(() =>
                {
                    cmbCameraName.ItemsSource = Enum.GetValues(typeof(CameraName));
                    cmbCameraName.Items.Refresh();

                    cmbCameraType.ItemsSource = Enum.GetValues(typeof(CameraType));
                    cmbCameraType.Items.Refresh();

                    cmbAlgorithm.ItemsSource = Enum.GetValues(typeof(Algorithm));
                    cmbAlgorithm.Items.Refresh();

                    cmbFunction.ItemsSource = Enum.GetValues(typeof(Function));
                    cmbFunction.Items.Refresh();

                    cmbThresholdType.ItemsSource = Enum.GetValues(typeof(ThresholdType));
                    cmbThresholdType.Items.Refresh();

                    cmbBarcodeMode.ItemsSource = Enum.GetValues(typeof(BarcodeMode));
                    cmbBarcodeMode.Items.Refresh();

                    cmbBarcodeType.ItemsSource = Enum.GetValues(typeof(BarcodeType));
                    cmbBarcodeType.Items.Refresh();
                });
                #endregion

                #region Show Camera Scan
                Dispatcher.Invoke(() =>
                {
                    tbcmbCameraScan.Items.Clear();
                    foreach (var item in Root.AppManager.CameraManager.CameraScan)
                    {
                        tbcmbCameraScan.Items.Add($"{item.CameraInformation.UserDefinedName} ({item.CameraInformation.ModelName}:{item.CameraInformation.SerialNumber})");
                    }
                    tbcmbCameraScan.Items.Refresh();
                });
                #endregion

                #region Event Camera Name & Show Normal
                Dispatcher.Invoke(() =>
                {
                    cmbCameraName.SelectionChanged -= cmbCameraName_SelectionChanged;
                    cmbCameraName.SelectedIndex = 0;
                    cmbCameraName.SelectionChanged += cmbCameraName_SelectionChanged;
                    cmbCameraName_SelectionChanged(null, null);

                    rdoNormal.Checked -= rdoNormal_Checked;
                    rdoNormal.IsChecked = true;
                    rdoNormal.Checked += rdoNormal_Checked;
                    rdoNormal_Checked(null, null);
                });
                #endregion

                _ = Task.Factory.StartNew(() => CameraLive(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbcmbCameraScan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                int selectedIndex = tbcmbCameraScan.SelectedIndex;
                object selectedItem = tbcmbCameraScan.SelectedItem;
                if (selectedIndex > -1)
                {
                    var cameraScan = Root.AppManager.CameraManager.CameraScan.Find(x => x.Index == selectedIndex);
                    if (cameraScan != null)
                    {
                        // Camera instance.
                        Setup.Camera = cameraScan.CameraInstance;
                        Setup.Camera.CameraInformation = cameraScan.CameraInformation;
                        // Runtime configuration.
                        var cameraStep = Setup.Step.Camera;
                        cameraStep.UserDefinedName = cameraScan.CameraInformation.UserDefinedName;
                        cameraStep.ModelName = cameraScan.CameraInformation.ModelName;
                        cameraStep.SerialNumber = cameraScan.CameraInformation.SerialNumber;
                        // Basic configuration.
                        var cameraDevice = Root.AppManager.DatabaseManager.Basic.Camera.Devices.Find(x => x.Index == cmbCameraName.SelectedIndex);
                        if (cameraDevice != null)
                        {
                            cameraDevice.CameraType = cameraScan.CameraInstance.CameraType;
                            cameraDevice.UserDefinedName = cameraScan.CameraInformation.UserDefinedName;
                            cameraDevice.ModelName = cameraScan.CameraInformation.ModelName;
                            cameraDevice.SerialNumber = cameraScan.CameraInformation.SerialNumber;
                        }
                        // Update camera type.
                        cmbCameraType.SelectionChanged -= cmbCameraType_SelectionChanged;

                        cmbCameraType.SelectedIndex = (int)cameraScan.CameraInstance.CameraType;

                        cmbCameraType.SelectionChanged += cmbCameraType_SelectionChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbcmbCameraScan.SelectedIndex > -1)
                {
                    Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                    if (!IsConnected())
                    {
                        tbcmbCameraScan_SelectionChanged(null, null);
                        if (Setup.Camera.OpenStrategies(true, null))
                        {
                            tbcmbCameraScan.IsEnabled = false;
                            AppUi.ShowImage(this, imgConnect, @"Assets/Camera/Connected.png");
                            if (Root.tbcmbModelName.SelectedIndex > -1)
                            {
                                GetParameters(Setup.Step, Setup.Component);
                                SetCameraParameters(Setup.Component);
                            }
                            else
                            {
                                GetCameraParameters(Setup);
                            }
                        }
                        else
                        {
                            AppUi.ShowMessage("Cannot connect.", MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        if (Setup.Camera.Disconnect())
                        //if (Setup.Camera.Disconnect() && Setup.Camera.Release())
                        {
                            tbcmbCameraScan.IsEnabled = true;
                            AppUi.ShowImage(this, imgConnect, @"Assets/Camera/Disconnected.png");
                        }
                        else
                        {
                            AppUi.ShowMessage("Cannot disconnect.", MessageBoxImage.Error);
                        }
                    }
                    Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Arrow; });
                }
                else
                {
                    AppUi.ShowMessage("No camera selected.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsConnected())
                {
                    Setup.Camera.SetParameter();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnCapture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                if (IsConnected())
                {
                    Setup.Camera.ExcuteTriggerSoftware();
                    using (var bitmap = Setup.Camera.GrabFrame())
                    {
                        if (bitmap != null)
                        {
                            using (var image = bitmap.ToImage<Bgr, byte>().Rotate((double)Setup.Step.Camera.Rotate, new Bgr(), false))
                            {
                                if (image != null)
                                {
                                    Setup.Step.Image = image.Clone();
                                    AppUi.ShowImageBox(this, imbSetup, image.Clone());
                                    Root.ShowMessage("Captured");
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (var ofd = new System.Windows.Forms.OpenFileDialog())
                    {
                        ofd.Filter = "Image file (*.jpg, *.png, *.tiff)|*.jpg; *.png; *.tiff|All files (*.*)|*.*";
                        ofd.FilterIndex = 0;
                        ofd.RestoreDirectory = true;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            using (var image = new Image<Bgr, byte>(ofd.FileName))
                            {
                                Setup.Step.Image = image.Clone();
                                AppUi.ShowImageBox(this, imbSetup, Setup.Draw(image));
                            }
                        }
                    }
                }
                Root.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Arrow; });
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnStreaming_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsConnected())
                    return;
                if (!_streaming)
                {
                    _streaming = true;
                    AppUi.ShowImage(this, imgStreaming, @"Assets/Camera/StartStreaming.png");
                    Root.ShowMessage("Streaming");
                }
                else
                {
                    _streaming = false;
                    AppUi.ShowImage(this, imgStreaming, @"Assets/Camera/StopStreaming.png");
                    Root.ShowMessage("Offline");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnDrawing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_drawing && imbSetup.Image != null)
                {
                    _drawing = true;
                    Root.ShowMessage("Allow drawing");
                }
                else
                {
                    _drawing = false;
                    Root.ShowMessage("Cannot drawing");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void tbbtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_drawing && imbSetup.Image != null)
                {
                    Setup.Component.Region = new BRectangle(_mouse.LastRectangle);
                    ShowComponentRegion(Setup.Component);
                    Root.ShowMessage("Saved");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void imbSetup_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            //if (imbSetup.Image != null && _mouse.Rectangle() != System.Drawing.Rectangle.Empty)
            //{
            //    e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Green, 2), _mouse.Rectangle());
            //}
        }

        private void imbSetup_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            if (!point.IsEmpty)
            {
                _mouse.IsMouseDown = true;
                _mouse.StartPoint = point;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                imbSetup.SetZoomScale(1, e.Location);
                imbSetup.AutoScrollOffset = new System.Drawing.Point(0, 0);
            }
        }

        private void imbSetup_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var point = _mouse.GetMousePosition(sender, e);
            var position = $"x = {point.X}, y = {point.Y}";
            sslblCoordinate.Content = position;

            if (e.Button != System.Windows.Forms.MouseButtons.Left || imbSetup.Image == null)
                return;
            if (_mouse.IsMouseDown && !point.IsEmpty)
            {
                _mouse.EndPoint = point;
                using (var imageDraw = Setup.Step.Image.Clone())
                {
                    var color = _drawing ? new Bgr(65, 205, 40) : new Bgr(48, 59, 255);
                    imageDraw.Draw(_mouse.Rectangle(), color, 1);
                    imbSetup.Image = imageDraw;
                    imbSetup.Refresh();
                }
            }
        }

        private void imbSetup_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mouse.Clear();
        }

        private void imbSetup_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (imbSetup.Image == null)
                return;
            if (e.Delta > 0)
            {
                imbSetup.SetZoomScale(1.2 * imbSetup.ZoomScale, e.Location);
            }
            else
            {
                imbSetup.SetZoomScale(0.8 * imbSetup.ZoomScale, e.Location);
            }
        }

        private void cmbCameraName_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbCameraName.SelectedIndex;
            object selectedItem = cmbCameraName.SelectedItem;
            if (selectedIndex > -1)
            {
                switch ((CameraName)selectedIndex)
                {
                    case CameraName.Camera0:
                        Setup = Setup0;
                        break;
                    case CameraName.Camera1:
                        Setup = Setup1;
                        break;
                    case CameraName.Camera2:
                        Setup = Setup2;
                        break;
                    default:
                        break;
                }
                ShowStepList(Setup.Steps);
                ShowComponentList(Setup.Step.Components);
                GetParameters(Setup.Step, Setup.Component);
                SetCameraParameters(Setup.Component);
                ShowCameraSetup(selectedIndex);
                AppUi.ShowGroupBoxName(this, grpSteps, $"Steps ({selectedItem})");
                AppUi.ShowGroupBoxName(this, grpComponents, $"Components ({selectedItem})");
            }
        }

        private void cmbCameraType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbCameraType.SelectedIndex;
            object selectedItem = cmbCameraType.SelectedItem;
            if (selectedIndex > -1 && cmbCameraName.SelectedIndex > -1)
            {
                var cameraDevice = Root.AppManager.DatabaseManager.Basic.Camera.Devices.Find(x => x.Index == cmbCameraName.SelectedIndex);
                if (cameraDevice != null)
                {
                    cameraDevice.CameraType = (CameraType)selectedIndex;
                }
            }
        }

        private void chkCameraStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                var cameraDevice = Root.AppManager.DatabaseManager.Basic.Camera.Devices.Find(x => x.Index == cmbCameraName.SelectedIndex);
                if (cameraDevice != null)
                {
                    cameraDevice.Enable = chkCameraStatus.IsChecked == true;
                    chkCameraStatus.Content = chkCameraStatus.IsChecked == true ? "Enable" : "Disable";
                }
            }
        }

        private void chkCameraStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                var cameraDevice = Root.AppManager.DatabaseManager.Basic.Camera.Devices.Find(x => x.Index == cmbCameraName.SelectedIndex);
                if (cameraDevice != null)
                {
                    cameraDevice.Enable = chkCameraStatus.IsChecked == true;
                    chkCameraStatus.Content = chkCameraStatus.IsChecked == true ? "Enable" : "Disable";
                }
            }
        }

        private void nudExposureTime_ValueChanged(object sender, EventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step.Camera.ExposureTime = (double)nudExposureTime.Value;
                if (IsConnected())
                {
                    if (Setup.Camera.CameraType == CameraType.Webcam)
                        return;
                    // This command is not use for USB Camera.
                    Setup.Camera.ExposureTime = (int)nudExposureTime.Value;
                }
            }
        }

        private void nudGain_ValueChanged(object sender, EventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step.Camera.Gain = (double)nudGain.Value;
                if (IsConnected())
                {
                    if (Setup.Camera.CameraType == CameraType.Webcam)
                        return;
                    // This command is not use for USB Camera.
                    //Setup.Camera.Gain = (int)nudGain.Value;
                }
            }
        }

        private void nudRotate_ValueChanged(object sender, EventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step.Camera.Rotate = (double)nudRotate.Value;
            }
        }

        private void cmbSteps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbSteps.SelectedIndex;
            object selectedItem = cmbSteps.SelectedItem;
            if (selectedIndex > -1 && cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step = Setup.Steps[selectedIndex];
                GetParameters(Setup.Step, null);
                ShowComponentList(Setup.Step.Components);
            }
        }

        private void btnAddStep_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                int index = Setup.Steps.Count;
                //Setup.Steps.Add(new StepConfiguration { Index = index });
                Setup.Steps.Add(new StepConfiguration().Clone());
                Setup.Step = Setup.Steps[^1];
                ShowStepList(Setup.Steps);
                ShowComponentList(Setup.Step.Components);
                Root.ShowMessage($"[Camera {cmbCameraName.SelectedIndex}] Add step: {index}");
            }
        }

        private void btnRemoveStep_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Steps.RemoveAt(cmbSteps.SelectedIndex);
                Setup.Step = Setup.Steps[^1];
                ShowStepList(Setup.Steps);
                ShowComponentList(Setup.Step.Components);
                Root.ShowMessage($"[Camera {cmbCameraName.SelectedIndex}] Remove step: {cmbSteps.SelectedIndex}");
            }
        }

        private void chkStepStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step.Enable = chkStepStatus.IsChecked == true;
                chkStepStatus.Content = chkStepStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStepStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1)
            {
                Setup.Step.Enable = chkStepStatus.IsChecked == true;
                chkStepStatus.Content = chkStepStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void cmbComponents_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbComponents.SelectedIndex;
            object selectedItem = cmbComponents.SelectedItem;
            if (selectedIndex > -1 && cmbCameraName.SelectedIndex > -1 && cmbSteps.SelectedIndex > -1)
            {
                Setup.Step = Setup.Steps[cmbSteps.SelectedIndex];
                Setup.Component = Setup.Step.Components[selectedIndex];
                GetParameters(Setup.Step, Setup.Component);
                SetCameraParameters(Setup.Component);
                ShowComponentRegion(Setup.Step.Components[selectedIndex]);
            }
        }

        private void btnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                int index = Setup.Step.Components.Count;
                //Setup.Step.Components.Add(new ComponentConfiguration { Index = index });
                Setup.Step.Components.Add(new ComponentConfiguration().Clone());
                Setup.Component = Setup.Step.Components[^1];
                ShowComponentList(Setup.Step.Components);
                Root.ShowMessage($"[Camera {cmbCameraName.SelectedIndex}] [Step {cmbSteps.SelectedIndex}] Add component: {index}");
            }
        }

        private void btnRemoveComponent_Click(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Step.Components.RemoveAt(cmbComponents.SelectedIndex);
                Setup.Component = Setup.Step.Components[^1];
                ShowComponentList(Setup.Step.Components);
                Root.ShowMessage($"[Camera {cmbCameraName.SelectedIndex}] [Step {cmbSteps.SelectedIndex}] Remove component: {cmbComponents.SelectedIndex}");
            }
        }

        private void chkComponentStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1 && cmbSteps.SelectedIndex > -1)
            {
                Setup.Component.Enable = chkComponentStatus.IsChecked == true;
                chkComponentStatus.Content = chkComponentStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkComponentStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cmbCameraName.SelectedIndex > -1 && cmbSteps.SelectedIndex > -1)
            {
                Setup.Component.Enable = chkComponentStatus.IsChecked == true;
                chkComponentStatus.Content = chkComponentStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void nudRegionX_ValueChanged(object sender, EventArgs e)
        {
            if (Setup.Step.Image != null)
            {
                Setup.Component.Region.X = (int)nudRegionX.Value;
                AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
            }
        }

        private void nudRegionY_ValueChanged(object sender, EventArgs e)
        {
            if (Setup.Step.Image != null)
            {
                Setup.Component.Region.Y = (int)nudRegionY.Value;
                AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
            }
        }

        private void nudRegionW_ValueChanged(object sender, EventArgs e)
        {
            if (Setup.Step.Image != null)
            {
                Setup.Component.Region.Width = (int)nudRegionW.Value;
                AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
            }
        }

        private void nudRegionH_ValueChanged(object sender, EventArgs e)
        {
            if (Setup.Step.Image != null)
            {
                Setup.Component.Region.Height = (int)nudRegionH.Value;
                AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
            }
        }

        private void cmbAlgorithm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbAlgorithm.SelectedIndex;
            object selectedItem = cmbAlgorithm.SelectedItem;
            if (selectedIndex > -1 && CanSetValue())
            {
                Setup.Component.Algorithm = (Algorithm)selectedIndex;
                ShowAlgorithm((Algorithm)selectedIndex);
            }
        }

        private void cmbFunction_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbFunction.SelectedIndex;
            object selectedItem = cmbFunction.SelectedItem;
            if (selectedIndex > -1 && CanSetValue())
            {
                Setup.Component.Function = (Function)selectedIndex;
                ShowFunction((Function)selectedIndex);
            }
        }

        private void chkPreprocessing_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                if (chkPreprocessing.IsChecked == true)
                {
                    ProcessImage();
                }
                else
                {
                    if (Setup.Step.Image != null)
                    {
                        AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
                    }
                }
                chkPreprocessing.Content = "Enable";
            }
        }

        private void chkPreprocessing_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                if (chkPreprocessing.IsChecked == true)
                {
                    ProcessImage();
                }
                else
                {
                    if (Setup.Step.Image != null)
                    {
                        AppUi.ShowImageBox(this, imbSetup, Setup.Draw(Setup.Step.Image));
                    }
                }
                chkPreprocessing.Content = "Disable";
            }
        }

        private void chkThreshold_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.Threshold.Enable = chkThreshold.IsChecked == true;
                ProcessImage();
                chkThreshold.Content = chkThreshold.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkThreshold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.Threshold.Enable = chkThreshold.IsChecked == true;
                ProcessImage();
                chkThreshold.Content = chkThreshold.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void cmbThresholdType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbThresholdType.SelectedIndex;
            object selectedItem = cmbThresholdType.SelectedItem;
            if (CanSetValue() && selectedIndex > -1)
            {
                Setup.Component.Preprocessing.Threshold.Type = (ThresholdType)selectedIndex;
                ShowThresholdType((ThresholdType)selectedIndex);
                ProcessImage();
            }
        }

        private void chkInvertedThreshold_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && cmbThresholdType.SelectedIndex > -1)
            {
                Setup.Component.Preprocessing.Threshold.Inverse = chkInvertedThreshold.IsChecked == true;
                chkInvertedThreshold.Content = chkInvertedThreshold.IsChecked == true ? "Enable" : "Disable";
                ProcessImage();
            }
        }

        private void chkInvertedThreshold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && cmbThresholdType.SelectedIndex > -1)
            {
                Setup.Component.Preprocessing.Threshold.Inverse = chkInvertedThreshold.IsChecked == true;
                chkInvertedThreshold.Content = chkInvertedThreshold.IsChecked == true ? "Enable" : "Disable";
                ProcessImage();
            }
        }

        private void nudThresholdValue1_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue() && cmbThresholdType.SelectedIndex > -1)
            {
                switch ((ThresholdType)cmbThresholdType.SelectedIndex)
                {
                    case ThresholdType.None:
                        break;
                    case ThresholdType.Binary:
                        Setup.Component.Preprocessing.Threshold.Binary.Value = (int)nudThresholdValue1.Value;
                        break;
                    case ThresholdType.Adaptive:
                        Setup.Component.Preprocessing.Threshold.Adaptive.BlockSize = (int)nudThresholdValue1.Value;
                        break;
                    case ThresholdType.Otsu:
                        Setup.Component.Preprocessing.Threshold.Otsu.Value = (int)nudThresholdValue1.Value;
                        break;
                    default:
                        break;
                }
                ProcessImage();
            }
        }

        private void nudThresholdValue2_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue() && cmbThresholdType.SelectedIndex > -1)
            {
                switch ((ThresholdType)cmbThresholdType.SelectedIndex)
                {
                    case ThresholdType.None:
                        break;
                    case ThresholdType.Binary:
                        break;
                    case ThresholdType.Adaptive:
                        Setup.Component.Preprocessing.Threshold.Adaptive.Param1 = (int)nudThresholdValue2.Value;
                        break;
                    case ThresholdType.Otsu:
                        break;
                    default:
                        break;
                }
                ProcessImage();
            }
        }

        private void chkGaussianBlur_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlurProp.Enable = chkGaussianBlur.IsChecked == true;
                ProcessImage();
            }
        }

        private void chkGaussianBlur_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlurProp.Enable = chkGaussianBlur.IsChecked == true;
                ProcessImage();
            }
        }

        private void nudGaussianBlurValue_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlurProp.Value = (int)nudGaussianBlurValue.Value;
                ProcessImage();
            }
        }

        private void chkBlobRemove_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlobProp.Enable = chkBlobRemove.IsChecked == true;
                ProcessImage();
            }
        }

        private void chkBlobRemove_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlobProp.Enable = chkBlobRemove.IsChecked == true;
                ProcessImage();
            }
        }

        private void nudBlobRemoveValue_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlobProp.Value = (int)nudBlobRemoveValue.Value;
                ProcessImage();
            }
        }

        private void nudDilateIteration_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlobProp.DilateIteration = (int)nudDilateIteration.Value;
                ProcessImage();
            }
        }

        private void nudErodeIteration_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.Preprocessing.BlobProp.ErodeIteration = (int)nudErodeIteration.Value;
                ProcessImage();
            }
        }

        private void rdoNormal_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && rdoNormal.IsChecked == true)
            {
                GetParameters(Setup.Step, Setup.Component);
                ProcessImage();
            }
        }

        private void rdoNormal_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && rdoNormal.IsChecked == true)
            {
                GetParameters(Setup.Step, Setup.Component);
                ProcessImage();
            }
        }

        private void rdoContours_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && rdoContours.IsChecked == true)
            {
                GetParameters(Setup.Step, Setup.Component);
                ProcessImage();
            }
        }

        private void rdoContours_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue() && rdoContours.IsChecked == true)
            {
                GetParameters(Setup.Step, Setup.Component);
                ProcessImage();
            }
        }

        private void cmbBarcodeMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbBarcodeMode.SelectedIndex;
            object selectedItem = cmbBarcodeMode.SelectedItem;
            if (selectedIndex > -1 && CanSetValue())
            {
                Setup.Component.BarcodeDetection.Mode = (BarcodeMode)selectedIndex;
            }
        }

        private void cmbBarcodeType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbBarcodeType.SelectedIndex;
            object selectedItem = cmbBarcodeType.SelectedItem;
            if (selectedIndex > -1 && CanSetValue())
            {
                Setup.Component.BarcodeDetection.Type = (BarcodeType)selectedIndex;
            }
        }

        private void nudBarcodeLength_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.BarcodeDetection.Length = (int)nudBarcodeLength.Value;
            }
        }

        private void btnSaveBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanSetValue() && Setup.Step.Image != null && _drawing)
                {
                    Setup.Component.Region = new BRectangle(_mouse.LastRectangle);
                    tbbtnDrawing_Click(null, null);
                    Root.ShowMessage("Saved barcode");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void chkInvertedTemplateMatching_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.TemplateMatching.Inverted = chkInvertedTemplateMatching.IsChecked == true;
                chkInvertedTemplateMatching.Content = chkInvertedTemplateMatching.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkInvertedTemplateMatching_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.TemplateMatching.Inverted = chkInvertedTemplateMatching.IsChecked == true;
                chkInvertedTemplateMatching.Content = chkInvertedTemplateMatching.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void nudTemplateMatchingScore_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.TemplateMatching.Score = (double)nudTemplateMatchingScore.Value;
            }
        }

        private void btnSaveTemplateMatching_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanSetValue() && Setup.Step.Image != null && _drawing)
                {
                    var rect = _mouse.LastRectangle;
                    var rect_padding = Image.PaddingRegion(rect, Setup.Step.Image.Size, GlobalVariable.PaddingMark);
                    Setup.Step.Image.ROI = rect;
                    Setup.Component.Region = new BRectangle(rect_padding);
                    Setup.Component.ObjectRegion = new BRectangle(rect);
                    Setup.Component.TemplateMatching.Image = Setup.Step.Image.Copy();
                    AppUi.ShowImageBox(this, imbTemplateMatching, Setup.Step.Image.Copy());
                    Setup.Step.Image.ROI = System.Drawing.Rectangle.Empty;
                    //using (var imageDraw = imbSetup.Image.GetInputArray().GetMat().ToImage<Bgr, byte>())
                    using (var imageDraw = Setup.Step.Image.Clone())
                    {
                        imageDraw.Draw(rect_padding, new Bgr(255, 122, 0), 2);
                        AppUi.ShowImageBox(this, imbSetup, Setup.Draw(imageDraw));
                    }
                    tbbtnDrawing_Click(null, null);
                    Root.ShowMessage("Saved template");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void chkInvertedFeatureMatching_Checked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.FeatureMatching.Inverted = chkInvertedFeatureMatching.IsChecked == true;
                chkInvertedFeatureMatching.Content = chkInvertedFeatureMatching.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkInvertedFeatureMatching_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.FeatureMatching.Inverted = chkInvertedFeatureMatching.IsChecked == true;
                chkInvertedFeatureMatching.Content = chkInvertedFeatureMatching.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void nudFeatureMatchingScore_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.FeatureMatching.Score = (double)nudFeatureMatchingScore.Value;
            }
        }

        private void btnSaveFeatureMatching_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanSetValue() && Setup.Step.Image != null && _drawing)
                {
                    var rect = _mouse.LastRectangle;
                    var rect_padding = Image.PaddingRegion(rect, Setup.Step.Image.Size, GlobalVariable.PaddingMark);
                    Setup.Step.Image.ROI = rect;
                    Setup.Component.Region = new BRectangle(rect_padding);
                    Setup.Component.ObjectRegion = new BRectangle(rect);
                    Setup.Component.FeatureMatching.Image = Setup.Step.Image.Copy();
                    AppUi.ShowImageBox(this, imbFeatureMatching, Setup.Step.Image.Copy());
                    Setup.Step.Image.ROI = System.Drawing.Rectangle.Empty;
                    //using (var imageDraw = imbSetup.Image.GetInputArray().GetMat().ToImage<Bgr, byte>())
                    using (var imageDraw = Setup.Step.Image.Clone())
                    {
                        imageDraw.Draw(rect_padding, new Bgr(255, 122, 0), 2);
                        AppUi.ShowImageBox(this, imbSetup, Setup.Draw(imageDraw));
                    }
                    tbbtnDrawing_Click(null, null);
                    Root.ShowMessage("Saved feature");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void nudContoursMinWidth_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.ContoursDetection.MinWidth = (int)nudContoursMinWidth.Value;
                ProcessImage();
            }
        }

        private void nudContoursMaxWidth_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.ContoursDetection.MaxWidth = (int)nudContoursMaxWidth.Value;
                ProcessImage();
            }
        }

        private void nudContoursMinHeight_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.ContoursDetection.MinHeight = (int)nudContoursMinHeight.Value;
                ProcessImage();
            }
        }

        private void nudContoursMaxHeight_ValueChanged(object sender, EventArgs e)
        {
            if (CanSetValue())
            {
                Setup.Component.ContoursDetection.MaxHeight = (int)nudContoursMaxHeight.Value;
                ProcessImage();
            }
        }

        private void btnSaveContours_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Setup.Step.Image != null)
                {
                    using (var inputImage = Setup.Step.Image.Clone())
                    using (var outputImage = Setup.Step.Image.Clone())
                    {
                        outputImage.ROI = Setup.Component.Region.Rectangle;
                        inputImage.ROI = Setup.Component.Region.Rectangle;
                        //var listContourArea = Setup.ProcessBoundaryDebug(inputImage.Copy(), outputImage, true);
                        imbSetup.Image = Setup.ProcessBoundaryDebug(inputImage.Copy(), Algorithm.BoundaryText, Setup.Component.Region.Rectangle);
                        inputImage.ROI = System.Drawing.Rectangle.Empty;
                        outputImage.ROI = System.Drawing.Rectangle.Empty;
                        //imbSetup.Image = outputImage.Clone();
                        tbbtnDrawing_Click(null, null);
                        Root.ShowMessage("Saved contours");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                AppUi.ShowImageBox(this, imbSetup, Setup.Step.Image.Clone());
                using (var inputImage = Setup.Step.Image.Clone())
                using (var outputImage = Setup.Step.Image.Clone())
                {
                    inputImage.ROI = Setup.Component.Region.Rectangle;
                    outputImage.ROI = Setup.Component.Region.Rectangle;
                    switch (Setup.Component.Algorithm)
                    {
                        case Algorithm.None:
                            break;
                        case Algorithm.BoundaryText:
                            break;
                        case Algorithm.BoundaryBox:
                            break;
                        case Algorithm.BoundaryTextBox:
                            break;
                        case Algorithm.FeatureMatching:
                            var taskFeatureResult = await Task.Run(() => Setup.FeatureImage(inputImage.Copy(), outputImage));
                            Root.ShowMessage($"Feature Matching: {taskFeatureResult}");
                            break;
                        case Algorithm.TemplateMatching:
                            var taskTemplateResult = await Task.Run(() => Setup.TemplateMatching(inputImage.Copy(), outputImage));
                            Root.ShowMessage($"Template Matching: {taskTemplateResult}");
                            break;
                        case Algorithm.BarcodeDetection:
                            var barcodeText = string.Empty;
                            var taskBarcodeTextResult = await Task.Run(() => Setup.BarcodeText(inputImage.Copy(), ref barcodeText, outputImage));
                            Root.ShowMessage($"Barcode Text: {taskBarcodeTextResult} ({barcodeText}:{barcodeText.Length})");
                            break;
                        case Algorithm.ColorDetection:
                            break;
                        default:
                            break;
                    }
                    inputImage.ROI = System.Drawing.Rectangle.Empty;
                    outputImage.ROI = System.Drawing.Rectangle.Empty;
                    AppUi.ShowImageBox(this, imbSetup, outputImage.Clone());
                }
                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.Message);
            }
        }

        private async Task CameraLive()
        {
            try
            {
                while (true)
                {
                    if (_streaming && Setup.Camera != null && Setup.Camera.IsGrabbing)
                    {
                        Setup.Camera.ExcuteTriggerSoftware();
                        using (var bitmap = Setup.Camera.GrabFrame())
                        {
                            if (bitmap != null)
                            {
                                using (var image = bitmap.ToImage<Bgr, byte>().Rotate((double)Setup.Step.Camera.Rotate, new Bgr(), false))
                                {
                                    if (image != null)
                                    {
                                        AppUi.ShowImageBox(this, imbSetup, image.Clone());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void ShowCameraSetup(int index)
        {
            cmbCameraType.SelectionChanged -= cmbCameraType_SelectionChanged;
            chkCameraStatus.Checked -= chkCameraStatus_Checked;

            var cameraDevice = Root.AppManager.DatabaseManager.Basic.Camera.Devices.Find(x => x.Index == index);
            if (cameraDevice != null)
            {
                cmbCameraType.Text = cameraDevice.CameraType.ToString();
                chkCameraStatus.IsChecked = cameraDevice.Enable;
                chkCameraStatus.Content = cameraDevice.Enable ? "Enable" : "Disable";
            }

            cmbCameraType.SelectionChanged += cmbCameraType_SelectionChanged;
            chkCameraStatus.Checked += chkCameraStatus_Checked;
        }

        private bool IsConnected()
        {
            if (Setup.Camera == null)
                return false;
            if (!Setup.Camera.IsConnected)
                return false;
            return true;
        }

        private void ShowStepList(List<StepConfiguration> steps)
        {
            Dispatcher.Invoke(() =>
            {
                cmbSteps.Items.Clear();
                cmbComponents.Items.Clear();
                for (int i = 0; i < steps.Count; i++)
                {
                    steps[i].Index = i;
                    cmbSteps.Items.Add(i);
                }
                cmbSteps.Items.Refresh();
                cmbSteps.SelectedIndex = steps.Count - 1;
            });
        }

        private void ShowComponentList(List<ComponentConfiguration> components)
        {
            Dispatcher.Invoke(() =>
            {
                cmbComponents.Items.Clear();
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Index = i;
                    cmbComponents.Items.Add(i);
                }
                cmbComponents.Items.Refresh();
                cmbComponents.SelectedIndex = components.Count - 1;
            });
        }

        private void GetParameters(StepConfiguration step, ComponentConfiguration component)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (step != null)
                    {
                        #region Camera
                        tbcmbCameraScan.SelectionChanged -= tbcmbCameraScan_SelectionChanged;

                        tbcmbCameraScan.SelectedIndex = -1;
                        if (step.Camera.UserDefinedName != null && step.Camera.ModelName != null && step.Camera.SerialNumber != null)
                        {
                            for (int i = 0; i < tbcmbCameraScan.Items.Count; i++)
                            {
                                if (tbcmbCameraScan.Items[i].ToString().Contains(step.Camera.UserDefinedName) &&
                                    tbcmbCameraScan.Items[i].ToString().Contains(step.Camera.ModelName) &&
                                    tbcmbCameraScan.Items[i].ToString().Contains(step.Camera.SerialNumber))
                                {
                                    tbcmbCameraScan.SelectedIndex = i;
                                    break;
                                }
                            }
                        }

                        tbcmbCameraScan.SelectionChanged += tbcmbCameraScan_SelectionChanged;
                        #endregion

                        #region Exposure Time
                        nudExposureTime.ValueChanged -= nudExposureTime_ValueChanged;

                        nudExposureTime.Value = (decimal)step.Camera.ExposureTime;

                        nudExposureTime.ValueChanged += nudExposureTime_ValueChanged;
                        #endregion

                        #region Gain
                        nudGain.ValueChanged -= nudGain_ValueChanged;

                        nudGain.Value = (decimal)step.Camera.Gain;

                        nudGain.ValueChanged += nudGain_ValueChanged;
                        #endregion

                        #region Rotate
                        nudRotate.ValueChanged -= nudRotate_ValueChanged;

                        nudRotate.Value = (decimal)step.Camera.Rotate;

                        nudRotate.ValueChanged += nudRotate_ValueChanged;
                        #endregion

                        #region Status
                        chkStepStatus.Checked -= chkStepStatus_Checked;

                        chkStepStatus.IsChecked = step.Enable;
                        chkStepStatus.Content = step.Enable ? "Enable" : "Disable";

                        chkStepStatus.Checked += chkStepStatus_Checked;
                        #endregion
                    }

                    if (component != null)
                    {
                        #region Algorithm
                        cmbAlgorithm.SelectionChanged -= cmbAlgorithm_SelectionChanged;

                        cmbAlgorithm.SelectedIndex = (int)component.Algorithm;
                        ShowAlgorithm(component.Algorithm);

                        cmbAlgorithm.SelectionChanged += cmbAlgorithm_SelectionChanged;
                        #endregion

                        #region Function
                        cmbFunction.SelectionChanged -= cmbFunction_SelectionChanged;

                        cmbFunction.SelectedIndex = (int)component.Function;
                        ShowFunction(component.Function);

                        cmbFunction.SelectionChanged += cmbFunction_SelectionChanged;
                        #endregion

                        #region Init Preprocessing
                        chkComponentStatus.Checked -= chkComponentStatus_Checked;
                        chkPreprocessing.Checked -= chkPreprocessing_Checked;
                        rdoNormal.Checked -= rdoNormal_Checked;
                        rdoContours.Checked -= rdoContours_Checked;

                        chkComponentStatus.IsChecked = component.Enable;
                        chkComponentStatus.Content = component.Enable ? "Enable" : "Disable";
                        chkPreprocessing.IsChecked = false;
                        //rdbShowNormal.IsChecked = true;
                        //rdbShowContours.IsChecked = false;
                        if (step.Image != null)
                        {
                            using (var imageDraw = step.Image.Clone())
                            {
                                if (!component.Region.IsEmpty)
                                {
                                    imageDraw.Draw(component.Region.Rectangle, new Bgr(255, 122, 0), 2);
                                }
                                imbSetup.Image?.Dispose();
                                imbSetup.Image = imageDraw.Clone();
                            }
                        }

                        chkComponentStatus.Checked += chkComponentStatus_Checked;
                        chkPreprocessing.Checked += chkPreprocessing_Checked;
                        rdoNormal.Checked += rdoNormal_Checked;
                        rdoContours.Checked += rdoContours_Checked;
                        #endregion

                        #region Preprocessing
                        // Threshold
                        chkThreshold.Checked -= chkThreshold_Checked;
                        cmbThresholdType.SelectionChanged -= cmbThresholdType_SelectionChanged;
                        chkInvertedThreshold.Checked -= chkInvertedThreshold_Checked;

                        chkThreshold.IsChecked = component.Preprocessing.Threshold.Enable;
                        chkThreshold.Content = component.Preprocessing.Threshold.Enable ? "Enable" : "Disable";
                        cmbThresholdType.SelectedIndex = (int)component.Preprocessing.Threshold.Type;
                        lblThresholdValue1.Visibility = Visibility.Hidden;
                        nudThresholdValue1.Visible = false;
                        nudThresholdValue1.DecimalPlaces = 0;
                        nudThresholdValue1.Increment = 1;
                        lblThresholdValue2.Visibility = Visibility.Hidden;
                        nudThresholdValue2.Visible = false;
                        nudThresholdValue2.DecimalPlaces = 0;
                        nudThresholdValue2.Increment = 1;
                        switch (component.Preprocessing.Threshold.Type)
                        {
                            case ThresholdType.None:
                                lblThresholdValue1.Visibility = Visibility.Hidden;
                                lblThresholdValue2.Visibility = Visibility.Hidden;
                                nudThresholdValue1.Visible = false;
                                nudThresholdValue2.Visible = false;
                                break;
                            case ThresholdType.Binary:
                                lblThresholdValue1.Visibility = Visibility.Visible;
                                lblThresholdValue1.Content = "Value";
                                nudThresholdValue1.Visible = true;
                                nudThresholdValue1.Value = component.Preprocessing.Threshold.Binary.Value;

                                lblThresholdValue2.Visibility = Visibility.Hidden;
                                lblThresholdValue2.Content = "0";
                                nudThresholdValue2.Visible = false;
                                nudThresholdValue2.Value = 0;
                                break;
                            case ThresholdType.Adaptive:
                                lblThresholdValue1.Visibility = Visibility.Visible;
                                lblThresholdValue1.Content = "Block Size";
                                nudThresholdValue1.Visible = true;
                                nudThresholdValue1.Value = component.Preprocessing.Threshold.Adaptive.BlockSize;
                                nudThresholdValue1.DecimalPlaces = 0;
                                nudThresholdValue1.Increment = 2;

                                lblThresholdValue2.Visibility = Visibility.Visible;
                                lblThresholdValue2.Content = "Param1";
                                nudThresholdValue2.Visible = true;
                                nudThresholdValue2.Value = component.Preprocessing.Threshold.Adaptive.Param1;
                                nudThresholdValue2.DecimalPlaces = 0;
                                nudThresholdValue2.Increment = 2;
                                break;
                            case ThresholdType.Otsu:
                                lblThresholdValue1.Visibility = Visibility.Visible;
                                lblThresholdValue1.Content = "Value";
                                nudThresholdValue1.Visible = true;
                                nudThresholdValue1.Value = component.Preprocessing.Threshold.Otsu.Value;

                                lblThresholdValue2.Visibility = Visibility.Hidden;
                                lblThresholdValue2.Content = "Value";
                                nudThresholdValue2.Visible = false;
                                nudThresholdValue2.Value = 0;
                                break;
                            default:
                                break;
                        }
                        chkInvertedThreshold.IsChecked = component.Preprocessing.Threshold.Inverse;
                        chkInvertedThreshold.Content = component.Preprocessing.Threshold.Inverse ? "Enable" : "Disable";

                        chkThreshold.Checked += chkThreshold_Checked;
                        cmbThresholdType.SelectionChanged += cmbThresholdType_SelectionChanged;
                        chkInvertedThreshold.Checked += chkInvertedThreshold_Checked;

                        // Morphology
                        chkGaussianBlur.Checked -= chkGaussianBlur_Checked;
                        nudGaussianBlurValue.ValueChanged -= nudGaussianBlurValue_ValueChanged;
                        chkBlobRemove.Checked -= chkBlobRemove_Checked;
                        nudBlobRemoveValue.ValueChanged -= nudBlobRemoveValue_ValueChanged;
                        nudDilateIteration.ValueChanged -= nudDilateIteration_ValueChanged;
                        nudErodeIteration.ValueChanged -= nudErodeIteration_ValueChanged;

                        chkGaussianBlur.IsChecked = component.Preprocessing.BlurProp.Enable;
                        nudGaussianBlurValue.Value = component.Preprocessing.BlurProp.Value;
                        chkBlobRemove.IsChecked = component.Preprocessing.BlobProp.Enable;
                        nudBlobRemoveValue.Value = component.Preprocessing.BlobProp.Value;
                        nudDilateIteration.Value = component.Preprocessing.BlobProp.DilateIteration;
                        nudErodeIteration.Value = component.Preprocessing.BlobProp.ErodeIteration;

                        chkGaussianBlur.Checked += chkGaussianBlur_Checked;
                        nudGaussianBlurValue.ValueChanged += nudGaussianBlurValue_ValueChanged;
                        chkBlobRemove.Checked += chkBlobRemove_Checked;
                        nudBlobRemoveValue.ValueChanged += nudBlobRemoveValue_ValueChanged;
                        nudDilateIteration.ValueChanged += nudDilateIteration_ValueChanged;
                        nudErodeIteration.ValueChanged += nudErodeIteration_ValueChanged;
                        #endregion

                        #region Feature Matching
                        nudFeatureMatchingScore.ValueChanged -= nudFeatureMatchingScore_ValueChanged;
                        chkInvertedFeatureMatching.Checked -= chkInvertedFeatureMatching_Checked;

                        nudFeatureMatchingScore.Value = (decimal)component.FeatureMatching.Score;
                        if (component.FeatureMatching.Image != null)
                        {
                            AppUi.ShowImageBox(this, imbFeatureMatching, component.FeatureMatching.Image.Clone());
                        }
                        chkInvertedFeatureMatching.IsChecked = component.FeatureMatching.Inverted;
                        chkInvertedFeatureMatching.Content = component.FeatureMatching.Inverted ? "Enable" : "Disable";

                        nudFeatureMatchingScore.ValueChanged += nudFeatureMatchingScore_ValueChanged;
                        chkInvertedFeatureMatching.Checked += chkInvertedFeatureMatching_Checked;
                        #endregion

                        #region Template Matching
                        nudTemplateMatchingScore.ValueChanged -= nudTemplateMatchingScore_ValueChanged;
                        chkInvertedTemplateMatching.Checked -= chkInvertedTemplateMatching_Checked;

                        nudTemplateMatchingScore.Value = (decimal)component.TemplateMatching.Score;
                        if (component.TemplateMatching.Image != null)
                        {
                            AppUi.ShowImageBox(this, imbTemplateMatching, component.TemplateMatching.Image.Clone());
                        }
                        chkInvertedTemplateMatching.IsChecked = component.TemplateMatching.Inverted;
                        chkInvertedTemplateMatching.Content = component.TemplateMatching.Inverted ? "Enable" : "Disable";

                        nudTemplateMatchingScore.ValueChanged += nudTemplateMatchingScore_ValueChanged;
                        chkInvertedTemplateMatching.Checked += chkInvertedTemplateMatching_Checked;
                        #endregion

                        #region Barcode Text
                        cmbBarcodeMode.SelectionChanged -= cmbBarcodeMode_SelectionChanged;
                        cmbBarcodeType.SelectionChanged -= cmbBarcodeType_SelectionChanged;
                        nudBarcodeLength.ValueChanged -= nudBarcodeLength_ValueChanged;

                        cmbBarcodeMode.SelectedIndex = (int)component.BarcodeDetection.Mode;
                        cmbBarcodeType.SelectedIndex = (int)component.BarcodeDetection.Type;
                        nudBarcodeLength.Value = component.BarcodeDetection.Length;

                        cmbBarcodeMode.SelectionChanged += cmbBarcodeMode_SelectionChanged;
                        cmbBarcodeType.SelectionChanged += cmbBarcodeType_SelectionChanged;
                        nudBarcodeLength.ValueChanged += nudBarcodeLength_ValueChanged;
                        #endregion

                        #region Contours
                        nudContoursMinWidth.ValueChanged -= nudContoursMinWidth_ValueChanged;
                        nudContoursMaxWidth.ValueChanged -= nudContoursMaxWidth_ValueChanged;
                        nudContoursMinHeight.ValueChanged -= nudContoursMinHeight_ValueChanged;
                        nudContoursMaxHeight.ValueChanged -= nudContoursMaxHeight_ValueChanged;

                        nudContoursMinWidth.Value = component.ContoursDetection.MinWidth;
                        nudContoursMaxWidth.Value = component.ContoursDetection.MaxWidth;
                        nudContoursMinHeight.Value = component.ContoursDetection.MinHeight;
                        nudContoursMaxHeight.Value = component.ContoursDetection.MaxHeight;

                        nudContoursMinWidth.ValueChanged += nudContoursMinWidth_ValueChanged;
                        nudContoursMaxWidth.ValueChanged += nudContoursMaxWidth_ValueChanged;
                        nudContoursMinHeight.ValueChanged += nudContoursMinHeight_ValueChanged;
                        nudContoursMaxHeight.ValueChanged += nudContoursMaxHeight_ValueChanged;
                        #endregion
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
            }
        }

        private void SetCameraParameters(ComponentConfiguration component)
        {
            if (IsConnected())
            {
                if (Setup.Camera.CameraType == CameraType.Webcam)
                    return;
                // This command is not use for USB Camera.
                Setup.Camera.ExposureTime = (int)nudExposureTime.Value;
                //Setup.Camera.Gain = (int)nudGain.Value;
            }
        }

        private void GetCameraParameters(Image setup)
        {
            if (setup.Camera.ExposureTime > -1)
            {
                nudExposureTime.Value = (decimal)setup.Camera.ExposureTime;
            }
            if (setup.Camera.Gain > -1)
            {
                nudGain.Value = (decimal)setup.Camera.Gain;
            }
        }

        private void ShowAlgorithm(Algorithm algorithm)
        {
            grpBarcodeDecode.Visibility = Visibility.Hidden;
            grpFeatureMatching.Visibility = Visibility.Hidden;
            grpTemplateMatching.Visibility = Visibility.Hidden;
            grpContours.Visibility = Visibility.Hidden;
            switch (algorithm)
            {
                case Algorithm.None:
                    break;
                case Algorithm.BoundaryText:
                    grpContours.Visibility = Visibility.Visible;
                    break;
                case Algorithm.BoundaryBox:
                    grpContours.Visibility = Visibility.Visible;
                    break;
                case Algorithm.BoundaryTextBox:
                    grpContours.Visibility = Visibility.Visible;
                    break;
                case Algorithm.FeatureMatching:
                    grpFeatureMatching.Visibility = Visibility.Visible;
                    break;
                case Algorithm.TemplateMatching:
                    grpTemplateMatching.Visibility = Visibility.Visible;
                    break;
                case Algorithm.BarcodeDetection:
                    grpBarcodeDecode.Visibility = Visibility.Visible;
                    break;
                case Algorithm.ColorDetection:
                    break;
                default:
                    break;
            }
        }

        private void ShowFunction(Function function)
        {
            switch (function)
            {
                case Function.None:
                    break;
                case Function.Mark:
                    break;
                case Function.ModelName:
                    break;
                case Function.SN:
                    break;
                default:
                    break;
            }
        }

        private void ShowThresholdType(ThresholdType thresholdType)
        {
            lblThresholdValue1.Visibility = Visibility.Hidden;
            nudThresholdValue1.Visible = false;
            nudThresholdValue1.DecimalPlaces = 0;
            nudThresholdValue1.Increment = 1;

            lblThresholdValue2.Visibility = Visibility.Hidden;
            nudThresholdValue2.Visible = false;
            nudThresholdValue2.DecimalPlaces = 0;
            nudThresholdValue2.Increment = 1;
            switch (thresholdType)
            {
                case ThresholdType.None:
                    lblThresholdValue1.Visibility = Visibility.Hidden;
                    nudThresholdValue1.Visible = false;
                    lblThresholdValue2.Visibility = Visibility.Hidden;
                    nudThresholdValue2.Visible = false;
                    break;
                case ThresholdType.Binary:
                    lblThresholdValue1.Visibility = Visibility.Visible;
                    lblThresholdValue1.Content = "Value";
                    nudThresholdValue1.Visible = true;
                    nudThresholdValue1.Value = Setup.Component.Preprocessing.Threshold.Binary.Value;

                    lblThresholdValue2.Visibility = Visibility.Hidden;
                    lblThresholdValue2.Content = "0";
                    nudThresholdValue2.Visible = false;
                    nudThresholdValue2.Value = 0;
                    break;
                case ThresholdType.Adaptive:
                    lblThresholdValue1.Visibility = Visibility.Visible;
                    lblThresholdValue1.Content = "Block Size";
                    nudThresholdValue1.Visible = true;
                    nudThresholdValue1.Value = Setup.Component.Preprocessing.Threshold.Adaptive.BlockSize;
                    nudThresholdValue1.DecimalPlaces = 0;
                    nudThresholdValue1.Increment = 2;

                    lblThresholdValue2.Visibility = Visibility.Visible;
                    lblThresholdValue2.Content = "Param1";
                    nudThresholdValue2.Visible = true;
                    nudThresholdValue2.Value = Setup.Component.Preprocessing.Threshold.Adaptive.Param1;
                    nudThresholdValue2.DecimalPlaces = 0;
                    nudThresholdValue2.Increment = 2;
                    break;
                case ThresholdType.Otsu:
                    lblThresholdValue1.Visibility = Visibility.Visible;
                    lblThresholdValue1.Content = "Value";
                    nudThresholdValue1.Visible = true;
                    nudThresholdValue1.Value = Setup.Component.Preprocessing.Threshold.Otsu.Value;

                    lblThresholdValue2.Visibility = Visibility.Hidden;
                    lblThresholdValue2.Content = "Value";
                    nudThresholdValue2.Visible = false;
                    nudThresholdValue2.Value = 0;
                    break;
                default:
                    break;
            }
        }

        private void ShowComponentRegion(ComponentConfiguration component)
        {
            nudRegionX.Value = component.Region.X;
            nudRegionY.Value = component.Region.Y;
            nudRegionW.Value = component.Region.Width;
            nudRegionH.Value = component.Region.Height;
        }

        private bool CanSetValue()
        {
            return cmbCameraName.SelectedIndex > -1 && cmbSteps.SelectedIndex > -1 && cmbComponents.SelectedIndex > -1;
        }

        private void ProcessImage(Image<Bgr, byte> image = null)
        {
            if (chkPreprocessing.IsChecked == true)
            {
                imbSetup.Image = ProcessDebug(image ?? Setup.Step.Image, Setup.Step, Setup.Component);
                imbSetup.Refresh();
            }
        }

        private Mat ProcessDebug(Image<Bgr, byte> image, StepConfiguration step, ComponentConfiguration component)
        {
            try
            {
                if (image == null)
                {
                    return new Mat();
                }

                if (rdoNormal.IsChecked == true)
                {
                    using (var outputImage = Setup.Preprocessing(image))
                    {
                        return outputImage.Mat.Clone();
                    }
                }

                if (rdoContours.IsChecked == true)
                {
                    using (var inputImage = Setup.Preprocessing(image))
                    {
                        var outputImage = Setup.Draw(image);
                        inputImage.ROI = component.Region.Rectangle;
                        outputImage.ROI = component.Region.Rectangle;
                        if (component.Algorithm == Algorithm.BoundaryText || component.Algorithm == Algorithm.BoundaryTextBox)
                        {
                            Setup.FindBoundaryText(inputImage, outputImage, debug: true);
                        }
                        else if (component.Algorithm == Algorithm.BoundaryBox)
                        {
                            Setup.FindBoundaryBox(inputImage, outputImage, debug: true);
                        }
                        inputImage.ROI = System.Drawing.Rectangle.Empty;
                        outputImage.ROI = System.Drawing.Rectangle.Empty;
                        return outputImage.Mat.Clone();
                    }
                }

                return image.Mat.Clone();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                return new Mat();
            }
        }
    }
}
