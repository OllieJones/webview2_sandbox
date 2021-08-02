using System;
using System.Runtime.InteropServices;

namespace webview2Demo
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]

    public class Api
    {
        public string Username { get; set; }
        public string Version = "1.1.1";
        private object menuWindow;
        private static readonly Random random = new Random();
        private MainWindow mainWindow = null;
        public Api(MainWindow mw)
        {
            mainWindow = mw;
        }

        public int Random()
        {
            return random.Next();
        }

        public string Platform()
        {
            return "Win";
        }

        public string GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public void CopyToClipboard(string s)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                s = s.Replace("\n", "\r\n");                // it's Windows
                for (int i = 0; i < 10; i++)                // B 10438
                {
                    try
                    {
                        if (i % 1 == 1)
                            System.Windows.Clipboard.SetDataObject(s);
                        else
                            System.Windows.Clipboard.SetText(s);
                        return;
                    }
                    catch { }
                    System.Threading.Thread.Sleep(10);
                }
            });
        }

        public bool SysModalAlert(string text, string title, string okbutton, string cancelbutton)
        {
            bool ok = false;
            mainWindow.Dispatcher.Invoke(() =>
            {
                AlertWindow alert = new AlertWindow();

                ok = alert.ModalAlert(text, title, okbutton, cancelbutton) == true;
            });

            return ok;
        }


    }
}
