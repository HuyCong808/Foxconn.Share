using System.Windows.Controls;
using System.Windows.Input;

namespace Foxconn.UI.Controls
{
    public class ClearableTextBox : TextBox
    {
        public static readonly RoutedCommand ClearTextCommand = new RoutedCommand("ClearText", typeof(ClearableTextBox));

        static ClearableTextBox() => CommandManager.RegisterClassCommandBinding(typeof(ClearableTextBox), new CommandBinding(ClearTextCommand, new ExecutedRoutedEventHandler(ExecutedClearTextCommand), new CanExecuteRoutedEventHandler(CanExecuteClearTextCommand)));

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !string.IsNullOrEmpty(Text))
            {
                ClearText();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private bool CanClearText() => IsEnabled && !IsReadOnly && !string.IsNullOrEmpty(Text);

        public void ClearText() => Text = null;

        private static void ExecutedClearTextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is ClearableTextBox clearableTextBox))
                return;
            clearableTextBox.ClearText();
            e.Handled = true;
        }

        private static void CanExecuteClearTextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(sender is ClearableTextBox clearableTextBox))
                return;
            e.CanExecute = clearableTextBox.CanClearText();
        }
    }
}
