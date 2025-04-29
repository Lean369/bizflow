using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Entities;

namespace BizFlow
{
    public partial class MainBrowser : Form
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private Business.Core Core;
        private Queue<StateEvent> QueueHtmlScreen; //Cola para manejar las pantallas que se proyectan de manera asicrónica.
        public delegate void GUI_JSfunctionInvoke(string functionName, object param);
        private bool MainBrowserInitialized = false;
        private System.Threading.Timer ScreeenTimer;
        private Stopwatch Watch = new Stopwatch();
        private AdBanner adbanner;

        public MainBrowser()
        {
            bool ret = false;
            Log.Debug("Starting MainBrowser...");
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            this.Core = new Business.Core(out ret);
            this.QueueHtmlScreen = new Queue<StateEvent>();
            this.MainWebBrowser.CoreWebView2InitializationCompleted += this.WebView_CoreWebView2InitializationCompleted;
            this.InitializeWebView2Async();
            this.Core.EvtShowHtmlData += new Business.Core.DelegateSendHtmlData(this.EnqueueScreenData);
            this.Startup();
            if (ret)
            {
                Log.Info("--->--->Initialization WebBrowser ok");
            }
            else
            {
                Log.Error("--->--->ERROR");
                this.EnqueueScreenData(this.BuildScreenError());
            }
        }

        private void Startup()
        {
            try
            {
                Log.Debug("/--->");
                this.SetFormSize(this.Core.ScreenConfiguration.MainBrowserResolution);
                this.TopMost = this.Core.ScreenConfiguration.MainBrowserTopMost;
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(this.Core.ScreenConfiguration.MainBrowserPosX, this.Core.ScreenConfiguration.MainBrowserPosY);
                this.ScheduleTask(() =>
                {
                    this.Callback();
                });
                this.MainBrowserInitialized = true;
                if (this.Core.AlephATMAppData.SupervisorAppEnabled)
                {
                    this.Core.EvtChangeTerminalMode += new Business.Core.DelegateChangeTerminalMode(this.ChangeTerminalMode);
                }
                else { Log.Warn($"Supervisor App is disabled"); }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        public void ScheduleTask(Action task)
        {
            this.ScreeenTimer = new System.Threading.Timer(x =>
            {
                task.Invoke();
            }, null, this.Core.ScreenConfiguration.RefreshInterval, Timeout.Infinite);
        }

        /// <summary>
        /// LL)- Manejador para el timer que maneja la cola de eventos de pantalla
        /// </summary>
        private void Callback()
        {
            StateEvent stateEvent;
            try
            {
                this.Watch = new Stopwatch();
                this.Watch.Start();
                if (this.QueueHtmlScreen.Count != 0 && this.MainBrowserInitialized)
                {
                    stateEvent = this.QueueHtmlScreen.Peek();
                    if (!string.IsNullOrEmpty(stateEvent.HandlerName))
                    {
                        if (stateEvent.Action == StateEvent.EventType.navigate)
                        {
                            this.MainWebBrowser.CoreWebView2.Navigate(stateEvent.HandlerName);
                            if (this.Core.ScreenConfiguration.AdBannerEnable && adbanner != null)
                            {
                                if (stateEvent.HandlerName.EndsWith("\\welcome.htm"))
                                    adbanner.SetIdleImage();
                                else
                                    adbanner.SetTxnImage();
                            }
                            Log.Info($"--->Show screen: \"{Path.GetFileName(stateEvent.HandlerName)}\" -OK-");
                        }
                        else if (stateEvent.Action == StateEvent.EventType.runScript)
                        {
                            this.ExecuteJSfunctionInvoke(stateEvent.HandlerName, stateEvent.Parameters);
                            Log.Info($"--->Run Script: \"{Path.GetFileName(stateEvent.HandlerName)}\" -OK-");
                            this.ScreeenTimer.Change(Math.Max(0, this.Core.ScreenConfiguration.RefreshInterval - Watch.ElapsedMilliseconds), Timeout.Infinite);
                        }
                    }
                    else
                    {
                        stateEvent = this.BuildScreenError();
                        this.MainWebBrowser.CoreWebView2.Navigate(stateEvent.HandlerName);
                    }
                    stateEvent = this.QueueHtmlScreen.Dequeue();
                }
                else { this.ScreeenTimer.Change(Math.Max(0, this.Core.ScreenConfiguration.RefreshInterval - Watch.ElapsedMilliseconds), Timeout.Infinite); }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void EnqueueScreenData(StateEvent stateEvent)
        {
            try
            {
                if (this.QueueHtmlScreen.Count < 1000)
                {
                    this.QueueHtmlScreen.Enqueue(stateEvent);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ExecuteJSfunctionInvoke(string functionName, object param)
        {
            try
            {
                GUI_JSfunctionInvoke s = new GUI_JSfunctionInvoke(ExecuteJavascriptFunction);
                this.Invoke(s, new object[] { functionName, param });
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ChangeTerminalMode(Const.TerminalMode terminalMode)
        {
            try
            {
                if (terminalMode == Const.TerminalMode.InSupervisor)
                {
                    if (this.Core.ScreenConfiguration.SecBrowserEnable)
                    {
                        //Invoke(new Action(() => {
                        //    using (var AdminMenu = new AdminMenu(this.Core.ScreenConfiguration))
                        //    {
                        //        AdminMenu.EvtExit += AdminMenu_EvtExit;
                        //        AdminMenu.ShowDialog();
                        //    }
                        //}));
                        Log.Info("Go to Supervisor mode from Secondary browser");
                    }
                    else
                    {
                        Log.Info("Seconday browser disabled");
                        Invoke(new Action(() => { this.WindowState = FormWindowState.Minimized; }));
                    }
                }
                else if (terminalMode == Const.TerminalMode.InService || terminalMode == Const.TerminalMode.Suspend)
                {
                    Invoke(new Action(() => { this.WindowState = FormWindowState.Maximized; }));
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AdminMenu_EvtExit()
        {
            string commandData = "{\"executionType\":\"Full\",\"data\":\"Halt\"}";
            DeviceMessage dm = new DeviceMessage(Types.Unsolicited, Enums.Devices.Terminal, Enums.Commands.OutOfSupervisor, commandData);
            this.Core.Sdo.RaiseEvtEventReceive(dm);
            Log.Info("Out of Supervisor mode from Secondary browser");

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(100);
                Invoke(new Action(() => { this.WindowState = FormWindowState.Maximized; }));
            }).Start();
        }

        /// <summary>
        /// Ejecuta una función Javascript del HTML code.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="param"></param>
        public async void ExecuteJavascriptFunction(string functionName, object param)
        {
            try
            {
                Log.Info($"Execute function: {functionName}()");
                await this.MainWebBrowser.CoreWebView2.ExecuteScriptAsync(functionName);

                //if (this.MainWebBrowser.CanExecuteJavascriptInMainFrame)
                //    this.MainWebBrowser.ExecuteScriptAsync(functionName, new String[] { param.ToString() });
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public StateEvent BuildScreenError()
        {
            //var xDocument = new XDocument();
            string screenHtmPath = string.Empty;
            StateEvent stateEvent = null;
            try
            {
                this.MainBrowserInitialized = true;
                screenHtmPath = $"{Const.appPath}Screens\\E00.htm";
                stateEvent = new StateEvent(StateEvent.EventType.navigate, screenHtmPath, "");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return stateEvent;
        }

        private void SetFormSize(Const.Resolution resolution)
        {
            int screenSizeWidth = 640;
            int screenSizeHeight = 480;
            string[] sResolution = resolution.ToString().Substring(1).Split('x');
            if (sResolution.Length > 1)
            {
                if (int.TryParse(sResolution[0], out screenSizeWidth) && int.TryParse(sResolution[1], out screenSizeHeight))
                {
                    Log.Info("Set screen resolution: {0}", resolution);
                }
                else
                    Log.Error("The resolution: \"{0}\" is wrong.", resolution);
            }
            else
                Log.Error("Resolution: \"{0}\" error.", resolution);
            //Agregado
            //Modificamos la posicion del pictureBox cargando responsivamente.
            this.MaximumSize = new Size(screenSizeWidth, screenSizeHeight);
            this.Size = new Size(screenSizeWidth, screenSizeHeight);
            //this.panel1.Size = new Size(screenSizeWidth, screenSizeHeight);
        }

        async void InitializeWebView2Async()
        {
            try
            {
                await this.MainWebBrowser.EnsureCoreWebView2Async(null);

                //// Navegar a la URL después de la inicialización
                //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "browser/index.html");
                //this.MainWebBrowser.CoreWebView2.Navigate($"file:///{path}");

                this.MainWebBrowser.CoreWebView2.WebMessageReceived += async (sender, args) =>
                {
                    string message = args.TryGetWebMessageAsString();
                    MessageBox.Show($"Mensaje recibido desde JavaScript: {message}");
                };
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
                this.MainBrowserInitialized = true;
            else
                Log.Error($"WebView2 initialation error: {e.InitializationException.Message}");
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            //this.Core.WriteEJ(string.Format("<==CLOSE APPLICATION<=="));
            //if (this.Core.AlephATMAppData.AlephDEVEnable)
            //{
            //    if (this.Core.Sdo != null)
            //        if (this.Core.Sdo.TimerAutoStartAlephDEV != null)
            //            this.Core.Sdo.TimerAutoStartAlephDEV.Stop();

            //    if (this.Core.AlephATMAppData.KillAlephDev)
            //        this.Core.KillProcess("AlephDEV");
            //}
            //if (this.Core.TransitionStateFlow != null)
            //    this.Core.TransitionStateFlow.End();
            NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
            System.Environment.Exit(0);
        }
    }
}


