using Foxconn.Editor.Configuration;
using Foxconn.Editor.FoxconnEdit;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV;

namespace Foxconn.Editor
{
    /// <summary>
    /// Interaction logic for AutoRun.xaml
    /// </summary>
    public partial class AutoRun : Window
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static AutoRun Current;
        private Worker _loopWorker;
        private Board _program
        {
            get => ProgramManager.Current.Program;
            set => ProgramManager.Current.Program = value;
        }

        private MachineParams _param = MachineParams.Current;
        private DeviceManager _device = DeviceManager.Current;
        private Image<Bgr, byte> _image { get; set; }
        public string CycleTime { get; set; }

         
        public AutoRun()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            InitializeComponent();
            DataContext = this;
            _loopWorker = new Worker(new ThreadStart(AutoRunProcess));

            stopwatch.Stop();
            CycleTime = stopwatch.Elapsed.ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void Stop()
        {
            MainWindow.Current.Show();
        }

        public void AutoRunProcess()
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool alowZoom = false;
            alowZoom = !alowZoom;
            if (alowZoom == false)
            {
                imbCamera.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
                mnuiZoom.IsChecked = false;
            }
            else
            {
                imbCamera.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.PanAndZoom;
                imbCamera.AutoScrollOffset = new System.Drawing.Point(0, 0);
                mnuiZoom.IsChecked = true;
            }
        }
    }

}
