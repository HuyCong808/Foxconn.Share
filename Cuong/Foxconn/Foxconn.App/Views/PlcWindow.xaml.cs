using Foxconn.App.Controllers.Plc;
using Foxconn.App.Helper;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Foxconn.App.Models.BasicConfiguration.PlcConfiguration;

namespace Foxconn.App.Views
{
    /// <summary>
    /// Interaction logic for PlcWindow.xaml
    /// </summary>
    public partial class PlcWindow : Window
    {
        private readonly MainWindow Root = MainWindow.Current;
        private Plc _plc { get; set; }

        public PlcWindow()
        {
            InitializeComponent();
            _plc = null;
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
                Root.CodeFlow("PLC");

                #region Signal
                Dispatcher.Invoke(() =>
                {
                    var plc = Root.AppManager.DatabaseManager.Basic.Plc;

                    txtHome.TextChanged -= txtHome_TextChanged;
                    txtPlcState.TextChanged -= txtPlcState_TextChanged;
                    txtSoftwareState.TextChanged -= txtSoftwareState_TextChanged;
                    txtAllowJogging.TextChanged -= txtAllowJogging_TextChanged;
                    txtIncreaseAxisX.TextChanged -= txtIncreaseAxisX_TextChanged;
                    txtDecreaseAxisX.TextChanged -= txtDecreaseAxisX_TextChanged;
                    txtIncreaseAxisY.TextChanged -= txtIncreaseAxisY_TextChanged;
                    txtDecreaseAxisY.TextChanged -= txtDecreaseAxisY_TextChanged;
                    txtIncreaseAxisZ.TextChanged -= txtIncreaseAxisZ_TextChanged;
                    txtDecreaseAxisZ.TextChanged -= txtDecreaseAxisZ_TextChanged;
                    txtClockwiseAxisR.TextChanged -= txtClockwiseAxisR_TextChanged;
                    txtCounterClockwiseAxisR.TextChanged -= txtCounterClockwiseAxisR_TextChanged;
                    txtSpeedAxisX.TextChanged -= txtSpeedAxisX_TextChanged;
                    txtSpeedAxisY.TextChanged -= txtSpeedAxisY_TextChanged;
                    txtSpeedAxisZ.TextChanged -= txtSpeedAxisZ_TextChanged;
                    txtSpeedAxisR.TextChanged -= txtSpeedAxisR_TextChanged;
                    txtSpeedJogAxisX.TextChanged -= txtSpeedJogAxisX_TextChanged;
                    txtSpeedJogAxisY.TextChanged -= txtSpeedJogAxisY_TextChanged;
                    txtSpeedJogAxisZ.TextChanged -= txtSpeedJogAxisZ_TextChanged;
                    txtSpeedJogAxisR.TextChanged -= txtSpeedJogAxisR_TextChanged;
                    txtSpeedBasic.TextChanged -= txtSpeedBasic_TextChanged;
                    txtSpeedAngle.TextChanged -= txtSpeedAngle_TextChanged;
                    txtCoorX0.TextChanged -= txtCoorX0_TextChanged;
                    txtCoorX1.TextChanged -= txtCoorX1_TextChanged;
                    txtCoorY0.TextChanged -= txtCoorY0_TextChanged;
                    txtCoorY1.TextChanged -= txtCoorY1_TextChanged;
                    txtCoorZ0.TextChanged -= txtCoorZ0_TextChanged;
                    txtCoorZ1.TextChanged -= txtCoorZ1_TextChanged;
                    txtCoorR0.TextChanged -= txtCoorR0_TextChanged;
                    txtCoorR1.TextChanged -= txtCoorR1_TextChanged;
                    txtSuckAirCylinder0.TextChanged -= txtSuckAirCylinder0_TextChanged;
                    txtReleaseAirCylinder0.TextChanged -= txtReleaseAirCylinder0_TextChanged;
                    txtSuckAirCylinder1.TextChanged -= txtSuckAirCylinder1_TextChanged;
                    txtReleaseAirCylinder1.TextChanged -= txtReleaseAirCylinder1_TextChanged;
                    txtSuckAirVacuumPads0.TextChanged -= txtSuckAirVacuumPads0_TextChanged;
                    txtReleaseAirVacuumPads0.TextChanged -= txtReleaseAirVacuumPads0_TextChanged;
                    txtSuckAirVacuumPads1.TextChanged -= txtSuckAirVacuumPads1_TextChanged;
                    txtReleaseAirVacuumPads1.TextChanged -= txtReleaseAirVacuumPads1_TextChanged;
                    txtSuckAirVacuumPads2.TextChanged -= txtSuckAirVacuumPads2_TextChanged;
                    txtReleaseAirVacuumPads2.TextChanged -= txtReleaseAirVacuumPads2_TextChanged;
                    txtSuckAirVacuumPads3.TextChanged -= txtSuckAirVacuumPads3_TextChanged;
                    txtReleaseAirVacuumPads3.TextChanged -= txtReleaseAirVacuumPads3_TextChanged;
                    txtLight.TextChanged -= txtLight_TextChanged;
                    txtDoor.TextChanged -= txtDoor_TextChanged;
                    txtSafety.TextChanged -= txtSafety_TextChanged;
                    txtStart.TextChanged -= txtStart_TextChanged;
                    txtPause.TextChanged -= txtPause_TextChanged;
                    txtReset.TextChanged -= txtReset_TextChanged;
                    txtResetOK.TextChanged -= txtResetOK_TextChanged;
                    txtStop.TextChanged -= txtStop_TextChanged;
                    txtSettings.TextChanged -= txtSettings_TextChanged;
                    txtHasStarted.TextChanged -= txtHasStarted_TextChanged;
                    txtHasPaused.TextChanged -= txtHasPaused_TextChanged;
                    txtHasReset.TextChanged -= txtHasReset_TextChanged;
                    txtHasResetOK.TextChanged -= txtHasResetOK_TextChanged;
                    txtHasStopped.TextChanged -= txtHasStopped_TextChanged;
                    txtHasSet.TextChanged -= txtHasSet_TextChanged;

                    txtHome.Text = plc.Home;
                    txtPlcState.Text = plc.PlcState;
                    txtSoftwareState.Text = plc.SoftwareState;
                    txtAllowJogging.Text = plc.AllowJogging;
                    txtIncreaseAxisX.Text = plc.IncreaseAxisX;
                    txtDecreaseAxisX.Text = plc.DecreaseAxisX;
                    txtIncreaseAxisY.Text = plc.IncreaseAxisY;
                    txtDecreaseAxisY.Text = plc.DecreaseAxisY;
                    txtIncreaseAxisZ.Text = plc.IncreaseAxisZ;
                    txtDecreaseAxisZ.Text = plc.DecreaseAxisZ;
                    txtClockwiseAxisR.Text = plc.ClockwiseAxisR;
                    txtCounterClockwiseAxisR.Text = plc.CounterClockwiseAxisR;
                    txtSpeedAxisX.Text = plc.SpeedAxisX;
                    txtSpeedAxisY.Text = plc.SpeedAxisY;
                    txtSpeedAxisZ.Text = plc.SpeedAxisZ;
                    txtSpeedAxisR.Text = plc.SpeedAxisR;
                    txtSpeedJogAxisX.Text = plc.SpeedJogAxisX;
                    txtSpeedJogAxisY.Text = plc.SpeedJogAxisY;
                    txtSpeedJogAxisZ.Text = plc.SpeedJogAxisZ;
                    txtSpeedJogAxisR.Text = plc.SpeedJogAxisR;
                    txtSpeedBasic.Text = plc.SpeedBasic;
                    txtSpeedAngle.Text = plc.SpeedAngle;
                    txtCoorX0.Text = plc.CoorX0;
                    txtCoorX1.Text = plc.CoorX1;
                    txtCoorY0.Text = plc.CoorY0;
                    txtCoorY1.Text = plc.CoorY1;
                    txtCoorZ0.Text = plc.CoorZ0;
                    txtCoorZ1.Text = plc.CoorZ1;
                    txtCoorR0.Text = plc.CoorR0;
                    txtCoorR1.Text = plc.CoorR1;
                    txtSuckAirCylinder0.Text = plc.SuckAirCylinder0;
                    txtReleaseAirCylinder0.Text = plc.ReleaseAirCylinder0;
                    txtSuckAirCylinder1.Text = plc.SuckAirCylinder1;
                    txtReleaseAirCylinder1.Text = plc.ReleaseAirCylinder1;
                    txtSuckAirVacuumPads0.Text = plc.SuckAirVacuumPads0;
                    txtReleaseAirVacuumPads0.Text = plc.ReleaseAirVacuumPads0;
                    txtSuckAirVacuumPads1.Text = plc.SuckAirVacuumPads1;
                    txtReleaseAirVacuumPads1.Text = plc.ReleaseAirVacuumPads1;
                    txtSuckAirVacuumPads2.Text = plc.SuckAirVacuumPads2;
                    txtReleaseAirVacuumPads2.Text = plc.ReleaseAirVacuumPads2;
                    txtSuckAirVacuumPads3.Text = plc.SuckAirVacuumPads3;
                    txtReleaseAirVacuumPads3.Text = plc.ReleaseAirVacuumPads3;
                    txtLight.Text = plc.Light;
                    txtDoor.Text = plc.Door;
                    txtSafety.Text = plc.Safety;
                    txtStart.Text = plc.Status.Start;
                    txtPause.Text = plc.Status.Pause;
                    txtReset.Text = plc.Status.Reset;
                    txtResetOK.Text = plc.Status.ResetOK;
                    txtStop.Text = plc.Status.Stop;
                    txtSettings.Text = plc.Status.Settings;
                    txtHasStarted.Text = plc.Status.HasStarted;
                    txtHasPaused.Text = plc.Status.HasPaused;
                    txtHasReset.Text = plc.Status.HasReset;
                    txtHasResetOK.Text = plc.Status.HasResetOK;
                    txtHasStopped.Text = plc.Status.HasStopped;
                    txtHasSet.Text = plc.Status.HasSet;

                    txtHome.TextChanged += txtHome_TextChanged;
                    txtPlcState.TextChanged += txtPlcState_TextChanged;
                    txtSoftwareState.TextChanged += txtSoftwareState_TextChanged;
                    txtAllowJogging.TextChanged += txtAllowJogging_TextChanged;
                    txtIncreaseAxisX.TextChanged += txtIncreaseAxisX_TextChanged;
                    txtDecreaseAxisX.TextChanged += txtDecreaseAxisX_TextChanged;
                    txtIncreaseAxisY.TextChanged += txtIncreaseAxisY_TextChanged;
                    txtDecreaseAxisY.TextChanged += txtDecreaseAxisY_TextChanged;
                    txtIncreaseAxisZ.TextChanged += txtIncreaseAxisZ_TextChanged;
                    txtDecreaseAxisZ.TextChanged += txtDecreaseAxisZ_TextChanged;
                    txtClockwiseAxisR.TextChanged += txtClockwiseAxisR_TextChanged;
                    txtCounterClockwiseAxisR.TextChanged -= txtCounterClockwiseAxisR_TextChanged;
                    txtSpeedAxisX.TextChanged += txtSpeedAxisX_TextChanged;
                    txtSpeedAxisY.TextChanged += txtSpeedAxisY_TextChanged;
                    txtSpeedAxisZ.TextChanged += txtSpeedAxisZ_TextChanged;
                    txtSpeedAxisR.TextChanged += txtSpeedAxisR_TextChanged;
                    txtSpeedJogAxisX.TextChanged += txtSpeedJogAxisX_TextChanged;
                    txtSpeedJogAxisY.TextChanged += txtSpeedJogAxisY_TextChanged;
                    txtSpeedJogAxisZ.TextChanged += txtSpeedJogAxisZ_TextChanged;
                    txtSpeedJogAxisR.TextChanged += txtSpeedJogAxisR_TextChanged;
                    txtSpeedBasic.TextChanged += txtSpeedBasic_TextChanged;
                    txtSpeedAngle.TextChanged += txtSpeedAngle_TextChanged;
                    txtCoorX0.TextChanged += txtCoorX0_TextChanged;
                    txtCoorX1.TextChanged += txtCoorX1_TextChanged;
                    txtCoorY0.TextChanged += txtCoorY0_TextChanged;
                    txtCoorY1.TextChanged += txtCoorY1_TextChanged;
                    txtCoorZ0.TextChanged += txtCoorZ0_TextChanged;
                    txtCoorZ1.TextChanged += txtCoorZ1_TextChanged;
                    txtCoorR0.TextChanged += txtCoorR0_TextChanged;
                    txtCoorR1.TextChanged += txtCoorR1_TextChanged;
                    txtSuckAirCylinder0.TextChanged += txtSuckAirCylinder0_TextChanged;
                    txtReleaseAirCylinder0.TextChanged += txtReleaseAirCylinder0_TextChanged;
                    txtSuckAirCylinder1.TextChanged += txtSuckAirCylinder1_TextChanged;
                    txtReleaseAirCylinder1.TextChanged += txtReleaseAirCylinder1_TextChanged;
                    txtSuckAirVacuumPads0.TextChanged += txtSuckAirVacuumPads0_TextChanged;
                    txtReleaseAirVacuumPads0.TextChanged += txtReleaseAirVacuumPads0_TextChanged;
                    txtSuckAirVacuumPads1.TextChanged += txtSuckAirVacuumPads1_TextChanged;
                    txtReleaseAirVacuumPads1.TextChanged += txtReleaseAirVacuumPads1_TextChanged;
                    txtSuckAirVacuumPads2.TextChanged += txtSuckAirVacuumPads2_TextChanged;
                    txtReleaseAirVacuumPads2.TextChanged += txtReleaseAirVacuumPads2_TextChanged;
                    txtSuckAirVacuumPads3.TextChanged += txtSuckAirVacuumPads3_TextChanged;
                    txtReleaseAirVacuumPads3.TextChanged += txtReleaseAirVacuumPads3_TextChanged;
                    txtLight.TextChanged += txtLight_TextChanged;
                    txtDoor.TextChanged += txtDoor_TextChanged;
                    txtSafety.TextChanged += txtSafety_TextChanged;
                    txtStart.TextChanged += txtStart_TextChanged;
                    txtPause.TextChanged += txtPause_TextChanged;
                    txtReset.TextChanged += txtReset_TextChanged;
                    txtResetOK.TextChanged += txtResetOK_TextChanged;
                    txtStop.TextChanged += txtStop_TextChanged;
                    txtSettings.TextChanged += txtSettings_TextChanged;
                    txtHasStarted.TextChanged += txtHasStarted_TextChanged;
                    txtHasPaused.TextChanged += txtHasPaused_TextChanged;
                    txtHasReset.TextChanged += txtHasReset_TextChanged;
                    txtHasResetOK.TextChanged += txtHasResetOK_TextChanged;
                    txtHasStopped.TextChanged += txtHasStopped_TextChanged;
                    txtHasSet.TextChanged += txtHasSet_TextChanged;
                });
                #endregion

                ShowDevice();
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex.StackTrace);
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void txtHome_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Home = txtHome.Text;
        }

        private void txtPlcState_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.PlcState = txtPlcState.Text;
        }

        private void txtSoftwareState_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SoftwareState = txtSoftwareState.Text;
        }

        private void txtAllowJogging_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.AllowJogging = txtAllowJogging.Text;
        }

        private void txtIncreaseAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisX = txtIncreaseAxisX.Text;
        }

        private void txtDecreaseAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisX = txtDecreaseAxisX.Text;
        }

        private void txtIncreaseAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisY = txtIncreaseAxisY.Text;
        }

        private void txtDecreaseAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisY = txtDecreaseAxisY.Text;
        }

        private void txtIncreaseAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.IncreaseAxisZ = txtIncreaseAxisZ.Text;
        }

        private void txtDecreaseAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.DecreaseAxisZ = txtDecreaseAxisZ.Text;
        }

        private void txtClockwiseAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ClockwiseAxisR = txtClockwiseAxisR.Text;
        }

        private void txtCounterClockwiseAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CounterClockwiseAxisR = txtCounterClockwiseAxisR.Text;
        }

        private void txtSpeedAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisX = txtSpeedAxisX.Text;
        }

        private void txtSpeedAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisY = txtSpeedAxisY.Text;
        }

        private void txtSpeedAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisZ = txtSpeedAxisZ.Text;
        }

        private void txtSpeedAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedAxisR = txtSpeedAxisR.Text;
        }

        private void txtSpeedJogAxisX_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisX = txtSpeedJogAxisX.Text;
        }

        private void txtSpeedJogAxisY_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisY = txtSpeedJogAxisY.Text;
        }

        private void txtSpeedJogAxisZ_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisZ = txtSpeedJogAxisZ.Text;
        }

        private void txtSpeedJogAxisR_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedJogAxisR = txtSpeedJogAxisR.Text;
        }

        private void txtSpeedBasic_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedBasic = txtSpeedBasic.Text;
        }

        private void txtSpeedAngle_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SpeedAngle = txtSpeedAngle.Text;
        }

        private void txtCoorX0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorX0 = txtCoorX0.Text;
        }

        private void txtCoorX1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorX1 = txtCoorX1.Text;
        }

        private void txtCoorY0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorY0 = txtCoorY0.Text;
        }

        private void txtCoorY1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorY1 = txtCoorY1.Text;
        }

        private void txtCoorZ0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorZ0 = txtCoorZ0.Text;
        }

        private void txtCoorZ1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorZ1 = txtCoorZ1.Text;
        }

        private void txtCoorR0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorR0 = txtCoorR0.Text;
        }

        private void txtCoorR1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.CoorR1 = txtCoorR1.Text;
        }

        private void txtSuckAirCylinder0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirCylinder0 = txtSuckAirCylinder0.Text;
        }

        private void txtReleaseAirCylinder0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirCylinder0 = txtReleaseAirCylinder0.Text;
        }

        private void txtSuckAirCylinder1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirCylinder1 = txtSuckAirCylinder1.Text;
        }

        private void txtReleaseAirCylinder1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirCylinder1 = txtReleaseAirCylinder1.Text;
        }

        private void txtSuckAirVacuumPads0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads0 = txtSuckAirVacuumPads0.Text;
        }

        private void txtReleaseAirVacuumPads0_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads0 = txtReleaseAirVacuumPads0.Text;
        }

        private void txtSuckAirVacuumPads1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads1 = txtSuckAirVacuumPads1.Text;
        }

        private void txtReleaseAirVacuumPads1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads1 = txtReleaseAirVacuumPads1.Text;
        }

        private void txtSuckAirVacuumPads2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads2 = txtSuckAirVacuumPads2.Text;
        }

        private void txtReleaseAirVacuumPads2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads2 = txtReleaseAirVacuumPads2.Text;
        }

        private void txtSuckAirVacuumPads3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.SuckAirVacuumPads3 = txtSuckAirVacuumPads3.Text;
        }

        private void txtReleaseAirVacuumPads3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.ReleaseAirVacuumPads3 = txtReleaseAirVacuumPads3.Text;
        }

        private void txtLight_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Light = txtLight.Text;
        }

        private void txtDoor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Door = txtDoor.Text;
        }

        private void txtSafety_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Safety = txtSafety.Text;
        }

        private void txtStart_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.Start = txtStart.Text;
        }

        private void txtPause_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.Pause = txtPause.Text;
        }

        private void txtReset_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.Reset = txtReset.Text;
        }

        private void txtResetOK_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.ResetOK = txtResetOK.Text;
        }

        private void txtStop_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.Stop = txtStop.Text;
        }

        private void txtSettings_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.Settings = txtSettings.Text;
        }

        private void txtHasStarted_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasStarted = txtHasStarted.Text;
        }

        private void txtHasPaused_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasPaused = txtHasPaused.Text;
        }

        private void txtHasReset_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasReset = txtHasReset.Text;
        }

        private void txtHasResetOK_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasResetOK = txtHasResetOK.Text;
        }

        private void txtHasStopped_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasStopped = txtHasStopped.Text;
        }

        private void txtHasSet_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Root.AppManager.DatabaseManager.Basic.Plc.Status.HasSet = txtHasSet.Text;
        }

        private void cmbDevices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                ShowParameter(Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex]);
            }
        }

        private void chkStatus_Checked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void chkStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Enable = chkStatus.IsChecked == true;
                chkStatus.Content = chkStatus.IsChecked == true ? "Enable" : "Disable";
            }
        }

        private void txtIndex_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Index = Utilities.ConvertStringToInt(txtIndex.Text);
            }
        }

        private void txtAlias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Alias = txtAlias.Text;
            }
        }

        private void txtHost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Host = txtHost.Text;
            }
        }

        private void txtPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int selectedIndex = cmbDevices.SelectedIndex;
            object selectedItem = cmbDevices.SelectedItem;
            if (selectedIndex > -1)
            {
                Root.AppManager.DatabaseManager.Basic.Plc.Devices[selectedIndex].Port = Utilities.ConvertStringToInt(txtPort.Text);
            }
        }

        private void txtDevice_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtValue_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IPAddress.TryParse(txtHost.Text, out IPAddress ip) && int.TryParse(txtPort.Text, out int port))
                {
                    using (var client = new TcpClient())
                    {
                        client.SendTimeout = 500;
                        client.ReceiveTimeout = 500;
                        var result = client.ConnectAsync(ip, port).Wait(500);
                        if (result)
                            AppUi.ShowMessage($"Ping is successful ({ip}:{port}).", MessageBoxImage.Information);
                        else
                            AppUi.ShowMessage($"Cannot ping to ({ip}:{port}).", MessageBoxImage.Error);
                        AppUi.ShowButton(this, btnConnect, result);
                    }
                }
                else
                {
                    AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IPAddress.TryParse(txtHost.Text, out IPAddress ip) && int.TryParse(txtPort.Text, out int port))
                {
                    if (_plc != null)
                        _plc = null;
                    _plc = new Plc()
                    {
                        Index = 0,
                        Alias = "",
                        Host = ip.ToString(),
                        Port = port,
                    };
                    if (_plc.StartPlc())
                    {
                        AppUi.ShowMessage($"Connected ({ip}:{port})");
                    }
                    else
                    {
                        AppUi.ShowMessage($"Cannot ping to ({ip}:{port})", MessageBoxImage.Error);
                    }
                }
                else
                {
                    AppUi.ShowMessage($"Host or port is wrong format.", MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
            }
        }

        private void btnGetDevice_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_plc == null)
                            AppUi.ShowMessage("Cannot get device.", MessageBoxImage.Error);

                        var device = txtDevice.Text;
                        var value = -1;
                        _plc.GetDevice(device, ref value);
                        txtValue.Text = value.ToString();
                    });
                }
                catch (Exception ex)
                {
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void btnSetDevice_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_plc == null)
                            AppUi.ShowMessage("Cannot set device.", MessageBoxImage.Error);

                        var value = Utilities.ConvertStringToInt(txtValue.Text);
                        var device = txtDevice.Text;
                        if (value > -1)
                            _plc.SetDevice(device, value);
                    });
                }
                catch (Exception ex)
                {
                    AppUi.ShowMessage(ex.Message, MessageBoxImage.Error);
                }
            });
        }

        private void ShowDevice()
        {
            Dispatcher.Invoke(() =>
            {
                cmbDevices.Items.Clear();
                //var devices = Root.AppManager.DatabaseManager.Basic.Plc.Devices;
                var devices = Root.AppManager.DatabaseManager.Basic.Plc.Devices.OrderBy(x => x.Alias).ToList();
                Root.AppManager.DatabaseManager.Basic.Plc.Devices = devices;
                for (int i = 0; i < devices.Count; i++)
                {
                    devices[i].Index = i;
                    cmbDevices.Items.Add($"[{i}] {devices[i].Alias}");
                }

                if (devices.Count > 0)
                {
                    cmbDevices.SelectedIndex = devices.Count - 1;
                }
            });
        }

        private void ShowParameter(DeviceInformation device)
        {
            chkStatus.Checked -= chkStatus_Checked;
            txtIndex.TextChanged -= txtIndex_TextChanged;
            txtAlias.TextChanged -= txtAlias_TextChanged;
            txtHost.TextChanged -= txtHost_TextChanged;
            txtPort.TextChanged -= txtPort_TextChanged;

            chkStatus.IsChecked = device.Enable;
            chkStatus.Content = device.Enable ? "Enable" : "Disable";
            txtIndex.Text = device.Index.ToString();
            txtAlias.Text = device.Alias;
            txtHost.Text = device.Host;
            txtPort.Text = device.Port.ToString();

            chkStatus.Checked += chkStatus_Checked;
            txtIndex.TextChanged += txtIndex_TextChanged;
            txtAlias.TextChanged += txtAlias_TextChanged;
            txtHost.TextChanged += txtHost_TextChanged;
            txtPort.TextChanged += txtPort_TextChanged;
        }
    }
}
