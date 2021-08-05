using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace webview2Demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {


        private const string TestScript = @"
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
          /*
          setTimeout (
            function () { 
              alert('foobar') 
              console.error('foobar')
            }, 
            3000)
          */
        })().then()

        async function frobozz(a) {
          console.log ('in frobozz', a)
          const api = chrome.webview.hostObjects.api
          const q = a + ' ' + await api.GetVersion()
          alert (q)
        }
        ";



        private readonly string _cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2Demo");

        public MainWindow()
        {
            InitializeComponent();
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
            var webView2Environment = await CoreWebView2Environment.CreateAsync(null, _cacheFolderPath);
            await webView.EnsureCoreWebView2Async(webView2Environment);
            var foo = await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(TestScript);
            GetConsoleLogMessages(webView);

            webView.CoreWebView2.WebMessageReceived += UpdateAddressBar;
            webView.CoreWebView2.NavigationCompleted += CheckForError;
            webView.CoreWebView2.ScriptDialogOpening += HandleWebDialog;
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
        }

        private void HandleWebDialog(object sender, CoreWebView2ScriptDialogOpeningEventArgs e)
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
            webView.Dispatcher.Invoke(() =>
            {
                var alert = new AlertWindow();

                var ok = alert.ModalAlert(text, title, okButton, cancelButton) == true;
                if (ok) e.Accept();
            });


        }

        private void CheckForError(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var s = sender as CoreWebView2;
            var target = s?.Source.ToString();

            if (!e.IsSuccess) Console.WriteLine("Navigation failed " + target + " " + e.WebErrorStatus.ToString());


            RunAsyncJavascript("frobozz('Hello')");

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

        private async void GetConsoleLogMessages(WebView2 view)
        {
            /* request browser log events */
            //TODO the DevTools console API is deprecated, and Runtime.consoleAPICalled doesn't seem to be invoked
            /* these events come from the Network and other tabs */
            view.CoreWebView2.GetDevToolsProtocolEventReceiver("Log.entryAdded").DevToolsProtocolEventReceived +=
                OnConsoleMessage;
            await view.CoreWebView2.CallDevToolsProtocolMethodAsync("Log.enable", "{}");
            /* these events come from console.*() */
            view.CoreWebView2.GetDevToolsProtocolEventReceiver("Console.messageAdded").DevToolsProtocolEventReceived +=
                OnConsoleMessage;
            await view.CoreWebView2.CallDevToolsProtocolMethodAsync("Console.enable", "{}");
            /* these events supposedly come from console.*(). But as of early Aug-2021, they don't come */
            view.CoreWebView2.GetDevToolsProtocolEventReceiver("Runtime.consoleAPICalled").DevToolsProtocolEventReceived +=
                OnConsoleMessage;
        }

        private const string AsyncScriptTemplate = @"
            (async function () { 
                await ---SCRIPT---
            } )().catch(console.error)";

        public async void RunAsyncJavascript(string script)
        {
            var s = AsyncScriptTemplate.Replace("---SCRIPT---", script);
            await webView.CoreWebView2.ExecuteScriptAsync(s);
        }

    }
}