using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Foxconn.AOI.Editor.Controls
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl, IComponentConnector
    {
        private static readonly Regex __filterRegex = new Regex("^\\s*(?<err>\\!)?(?<name>[^!@]*)(\\@(?<block>[0-9]+))?\\s*$", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex __nameConvertRegex = new Regex("\\*|\\?|.", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly DependencyProperty FilterStringProperty = DependencyProperty.Register(nameof(FilterString), typeof(string), typeof(SearchControl), new PropertyMetadata(new PropertyChangedCallback(FilterStringChanged)));
        public Predicate<string> _currentPredicater = null;
        private ObservableCollection<string> searchedStrings = new ObservableCollection<string>();

        public string FilterString
        {
            get => (string)GetValue(FilterStringProperty);
            set => SetValue(FilterStringProperty, value);
        }

        public ObservableCollection<string> SearchedStrings => searchedStrings;

        public SearchControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!searchedStrings.Contains(FilterString))
                    searchedStrings.Insert(0, FilterString);
            }
        }

        private static Regex CreateNameRegex(string namePattern)
        {
            return new Regex(string.Format("^{0}$", __nameConvertRegex.Replace(namePattern, m =>
            {
                string str = m.Groups[0].Value;
                if (str == "*")
                    return ".*";
                return str == "?" ? "." : Regex.Escape(str);
            })), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        }

        private static Predicate<string> CreatePredicater(string filter)
        {
            return null;
        }

        private static void FilterStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SearchControl searchControl)
                return;
            searchControl.InvalidPredicater();
        }

        private void UpdatePredicater()
        {
            _currentPredicater = CreatePredicater(FilterString);
        }

        private void InvalidPredicater()
        {
            _currentPredicater = null;
        }

        private void AddSearchedString(string searchedString)
        {
            if (searchedString == null || searchedString == string.Empty)
                return;
            int oldIndex = searchedStrings.IndexOf(searchedString);
            switch (oldIndex)
            {
                case -1:
                    searchedStrings.Insert(0, searchedString);
                    break;
                case 0:
                    return;
                default:
                    searchedStrings.Move(oldIndex, 0);
                    break;
            }
            FilterString = searchedString;
            while (searchedStrings.Count > 20)
                searchedStrings.RemoveAt(searchedStrings.Count - 1);
        }

        private static void SearchFrist() { }
        private static void SearchPrevious() { }
        private static void SearchNext() { }
        private static void SearchLast() { }
    }
}
