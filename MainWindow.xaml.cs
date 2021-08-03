using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace webview2Demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //webView.NavigationStarting += EnsureHttps;

            InitializeAsync();
        }


        private void OnConsoleMessage(object sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs e)
        {
            if (e?.ParameterObjectAsJson != null)
            {
                Trace.WriteLine("WebView2:" + e.ParameterObjectAsJson);
            }
        }
        private void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            var uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.WebMessageReceived += UpdateAddressBar;
            grabMessages();
            webView.CoreWebView2.ScriptDialogOpening += (object s, CoreWebView2ScriptDialogOpeningEventArgs e) =>
            {
                var title = "";
                var text = "";
                var okButton = "";
                var cancelButton = "";

                switch (e.Kind)
                {
                    case CoreWebView2ScriptDialogKind.Alert:
                        title = e.Uri;
                        text = e.Message;
                        okButton = "OK";
                        break;
                    case CoreWebView2ScriptDialogKind.Confirm:
                        title = e.Uri;
                        text = e.Message;
                        okButton = "OK";
                        cancelButton = "Cancel";
                        break;
                    case CoreWebView2ScriptDialogKind.Beforeunload:
                        title = e.Uri;
                        text = "BeforeUnload is not yet implemented";
                        okButton = "OK I guess";
                        break;
                    case CoreWebView2ScriptDialogKind.Prompt:
                        title = e.Uri;
                        text = "prompt('" + e.Message + "') is not yet implemented";
                        okButton = "OK I guess";
                        break;
                }

                title = title + " says";
                this.Dispatcher.Invoke(() =>
                {
                    var alert = new AlertWindow();

                    var ok = alert.ModalAlert(text, title, okButton, cancelButton) == true;
                    if (ok) e.Accept();
                });

            };
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;



            const string testScript = @"
(async function foo () {
  const api = chrome.webview.hostObjects.api
  const ver = await api.Random()
  const user = 'foo.bar'
  api.Username = user
  const retrieved = await api.Username
  const confirmed = confirm (retrieved)
  await api.CopyToClipboard('https://nytimes.com/')
  //const result = await api.SysModalAlert('alert', 'title', 'cancel', 'file not found')
  alert ('foo')
  setTimeout (
    function () { 
      alert('foobar') 
      console.error('foobar')
    }, 
    3000)

})();

";


            var foo = await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(testScript);

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
        }

        private void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var uri = args.TryGetWebMessageAsString();
            addressBar.Text = uri;
            webView.CoreWebView2.PostWebMessageAsString(uri);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            webView?.CoreWebView2?.Navigate(addressBar.Text);
        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            //webView.CoreWebView2.OpenDevToolsWindow();
            var apiObject = new Api(this);
            webView.CoreWebView2.AddHostObjectToScript("api", apiObject);
        }

        private async void grabMessages ()
        {
            //TODO this isn't working. It doesn't get my own console.log or console.error stuff, only system stuff.
            await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Log.enable", "{}");
            webView.CoreWebView2.GetDevToolsProtocolEventReceiver("Log.entryAdded").DevToolsProtocolEventReceived += OnConsoleMessage;

        }
    }
}