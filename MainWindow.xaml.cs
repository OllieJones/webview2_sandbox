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
            webView.NavigationStarting += EnsureHttps;
            InitializeAsync();
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

            const string script = @"
(async function foo () {
  const api = chrome.webview.hostObjects.api
  const ver = await api.Random()
  const user = 'foo.bar'
  api.Username = user
  const retrieved = await api.Username
  alert (retrieved)
  await api.CopyToClipboard('https://nytimes.com/')
  const result = await api.SysModalAlert('alert', 'title', 'cancel', 'file not found')
  alert (result)

})();

";


            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
            //await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            //await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
        }

        private void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var uri = args.TryGetWebMessageAsString();
            addressBar.Text = uri;
            webView.CoreWebView2.PostWebMessageAsString(uri);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null) webView.CoreWebView2.Navigate(addressBar.Text);
        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var apiObject = new Api(this);
            webView.CoreWebView2.AddHostObjectToScript("api", apiObject);
        }
    }
}