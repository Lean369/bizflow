using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace BizFlow
{
    public partial class Form1 : Form
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private Business.Core Core;
        private Queue<StateEvent> QueueHtmlScreen; //Cola para manejar las pantallas que se proyectan de manera asicrónica.
        public delegate void GUI_JSfunctionInvoke(string functionName, object param);
        public ChromiumWebBrowser MainWebBrowser;
        public DevToolsContext BrowserDevToolsContext;
        private bool MainBrowserInitialized = false;
        private System.Threading.Timer ScreeenTimer;
        private Stopwatch Watch = new Stopwatch();
        private AdBanner adbanner;

        public Form1()
        {
            InitializeComponent();
            // Inicializar WebView2
            this.InitializeAsync();

        }

        async void InitializeAsync()
        {
            try
            {
                // Asegurarse de que WebView2 esté inicializado
                await webView2.EnsureCoreWebView2Async(null);

                // Navegar a la URL después de la inicialización
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "browser/index.html");
                webView2.CoreWebView2.Navigate($"file:///{path}");

                webView2.CoreWebView2.WebMessageReceived += async (sender, args) =>
                {
                    string message = args.TryGetWebMessageAsString();
                    MessageBox.Show($"Mensaje recibido desde JavaScript: {message}");
                };
            }
            catch (Exception ex)
            {
                // Manejar errores de inicialización
                MessageBox.Show($"Error al inicializar WebView2: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await webView2.CoreWebView2.ExecuteScriptAsync("alert('Hola desde C#!')");
        }
    }
}

