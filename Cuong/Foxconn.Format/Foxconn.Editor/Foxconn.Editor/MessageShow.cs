using System.Windows;

namespace Foxconn.Editor
{
    public static class MessageShow
    {
        public static void Info(string content, string title)
        {
            MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        public static void Error(string content, string title)
        {
            MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        public static void Warning(string content, string title)
        {
            MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        public static bool Question(string content, string title)
        {
            return MessageBox.Show(content, title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK;
        }

        public static MessageBoxResult YesNoCancel(string content, string title)
        {
            return MessageBox.Show(content, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

    }
}
