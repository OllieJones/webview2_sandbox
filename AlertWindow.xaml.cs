using System.Windows;

namespace webview2Demo
{
    /// <summary>
    ///     Interaction logic for AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow
    {
        public AlertWindow()
        {
            InitializeComponent();
        }

        public bool? ModalAlert(string message, string title, string ok, string cancel)
        {
            AlertTitle.Text = title;
            Message.Text = message;

            if (!string.IsNullOrEmpty(ok))
                OKButton.Content = ok;
            if (string.IsNullOrEmpty(cancel))
            {
                OKButton.Margin = CancelButton.Margin;
                CancelButton.Visibility = Visibility.Hidden;
            }
            else
            {
                CancelButton.Content = cancel;
            }

            return ShowDialog();
        }

        public bool? ModalAlert(string message, string title, string ok)
        {
            return ModalAlert(message, title, ok, null);
        }

        public bool? ModalAlert(string message, string title)
        {
            return ModalAlert(message, title, OKButton.Content.ToString(), CancelButton.Content.ToString());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}