using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace webview2Demo
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Api
    {
        private static readonly Random RandomGen = new Random();
        private readonly MainWindow _mainWindow;
        public string Version = "1.1.1";

        public Api(MainWindow mw)
        {
            _mainWindow = mw;
        }

        public string Username { get; set; }

        public int Random() => RandomGen.Next();

        public string Platform() => "Win";

        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public void CopyToClipboard(string s)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                s = s.Replace("\n", "\r\n"); // it's Windows
                for (var i = 0; i < 10; i++) // B 10438
                {
                    try
                    {
                        if (i % 1 == 1)
                            Clipboard.SetDataObject(s);
                        else
                            Clipboard.SetText(s);
                        return;
                    }
                    catch
                    {
                        /* empty, intentionally: ignore clipboard-set problems */
                    }

                    Thread.Sleep(10);
                }
            });
        }

        public bool SysModalAlert(string text, string title, string okbutton, string cancelbutton)
        {
            var ok = false;
            _mainWindow.Dispatcher.Invoke(() =>
            {
                var alert = new AlertWindow();

                ok = alert.ModalAlert(text, title, okbutton, cancelbutton) == true;
            });

            return ok;
        }
    }
}