using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace webview2Demo
{
    /// <summary>
    /// Interaction logic for AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        public AlertWindow()
        {
            InitializeComponent();
        }
        public bool? ModalAlert(string message, string title, string ok, string cancel)
        {
            AlertTitle.Text = title;
            Message.Text = message;

            if (!String.IsNullOrEmpty(ok))
                OKButton.Content = ok;
            if (String.IsNullOrEmpty(cancel))
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
