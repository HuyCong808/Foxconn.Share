using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Foxconn.AOI.Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for StringInputDialog.xaml
    /// </summary>
    public partial class StringInputDialog : Window, IComponentConnector
    {
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(StringInputDialog));
        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(nameof(InputText), typeof(string), typeof(StringInputDialog));
        public static readonly DependencyProperty HintTextProperty = DependencyProperty.Register(nameof(HintText), typeof(string), typeof(StringInputDialog));
        public static readonly DependencyProperty ValidationRulesProperty = DependencyProperty.Register(nameof(ValidationRules), typeof(ValidationRule[]), typeof(StringInputDialog), new PropertyMetadata(new PropertyChangedCallback(ValidationRulesPropertyChanged)));

        public new object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public string InputText
        {
            get => (string)GetValue(InputTextProperty);
            set => SetValue(InputTextProperty, value);
        }

        public string HintText
        {
            get => (string)GetValue(HintTextProperty);
            set => SetValue(HintTextProperty, value);
        }

        private static void ValidationRulesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is StringInputDialog stringInputDialog))
                return;
            BindingExpression bindingExpression = stringInputDialog.InputBox.GetBindingExpression(TextBox.TextProperty);
            Binding parentBinding = bindingExpression.ParentBinding;
            if (parentBinding == null)
                return;
            parentBinding.ValidationRules.Clear();
            if (e.NewValue is ValidationRule[] newValue)
            {
                for (int index = 0; index < newValue.Length; ++index)
                {
                    ValidationRule validationRule = newValue[index];
                    parentBinding.ValidationRules.Add(validationRule);
                }
            }
            bindingExpression.UpdateSource();
        }

        public ValidationRule[] ValidationRules
        {
            get => (ValidationRule[])GetValue(ValidationRulesProperty);
            set => SetValue(ValidationRulesProperty, value);
        }

        public StringInputDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            InputBox.Focus();
            DataContext = this;
        }

        public StringInputDialog(string hintText, string proposedText) : this()
        {
            HintText = hintText;
            InputText = proposedText;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
