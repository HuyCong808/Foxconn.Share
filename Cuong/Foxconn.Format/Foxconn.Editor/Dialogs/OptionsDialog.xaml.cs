using Foxconn.Editor.Controls;
using System.Windows;
using System.Windows.Input;

namespace Foxconn.Editor.Dialogs
{
    public partial class OptionsDialog : Window
    {
        public static OptionsDialog Current;
        private ParamsCameraControl _paramCameraControl = new ParamsCameraControl();
        private ParamsLightControl _paramsLightControl = new ParamsLightControl();
        private ParamsPLCControl _paramsPLCControl = new ParamsPLCControl();
        private ParamsRobotControl _paramsRobotControl = new ParamsRobotControl();
        private ParamsScannerControl _paramsScannerControl = new ParamsScannerControl();
        private ParamsTerminalControl _paramsTerminalControl = new ParamsTerminalControl();
        private ParamsOtherControl _paramsOtherControl = new ParamsOtherControl();
        public OptionsDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnSaveParams_Click(object sender, RoutedEventArgs e)
        {
            MachineParams.Current.Save();
        }

        private void trviCameraParams_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramCameraControl);
        }

        private void trviLightParams_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsLightControl);
        }

        private void trviPLCParams_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsPLCControl);
        }

        private void trviRobotParams_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsRobotControl);
        }

        private void trviScanner_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsScannerControl);
        }

        private void trviTerminal_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsTerminalControl);
        }

        private void TrviOthersParams_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            dpnParamsProperties.Children.Clear();
            dpnParamsProperties.Children.Add(_paramsOtherControl);
        }
    }
}
