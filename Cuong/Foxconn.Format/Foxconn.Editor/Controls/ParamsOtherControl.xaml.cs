using Foxconn.Editor.Enums;
using System;
using System.Windows.Controls;

namespace Foxconn.Editor.Controls
{
    public partial class ParamsOtherControl : UserControl
    {
        public WorkType WorkType
        {
            get => MachineParams.Current.WorkType;
            set => MachineParams.Current.WorkType = value;
        }
        public bool WorkerConfirm
        {
            get => MachineParams.Current.WorkerConfirm;
            set => MachineParams.Current.WorkerConfirm = value;
        }

        public int DelayCaptures
        {
            get => MachineParams.Current.DelayCaptures;
            set => MachineParams.Current.DelayCaptures = value;
        }

        public bool DebugMode
        {
            get => MachineParams.Current.DebugMode;
            set => MachineParams.Current.DebugMode = value;
        }
        public ParamsOtherControl()
        {
            InitializeComponent();
            DataContext = this;
            cmbWorkType.ItemsSource = Enum.GetValues(typeof(WorkType));
        }
    }
}
