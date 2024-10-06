using System.Windows;
using System.Windows.Input;

namespace AppLauncher
{
    /// <summary>
    /// Interaction logic for PromptNameDialog.xaml
    /// </summary>
    public partial class PromptNameDialog : Window
    {
        public PromptNameDialog()
        {
            InitializeComponent();

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            this.Loaded += Window_Loaded;
        }

        public string NameText
        {
            get { return NameTextBox.Text; }
            set { NameTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(NameTextBox.Text))
            {
                MessageBox.Show("You must provide a new name for the shortcut.", "Name is mandatory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DialogResult = true;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void HandleEsc(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus();
        }
    }
}
