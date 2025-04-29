using Entities;
using System;
using System.IO;
using System.Timers;

namespace Business
{
    public class MoreTime 
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public delegate void DelegateMoreTime(MoreTimeResult result);
        public event DelegateMoreTime EvtMoreTime; //Evento que envía un documento HTML para mostrar por pantalla de cliente.
        private Business.Core Core;
        private System.Timers.Timer timer;
        private string screenName;
        private int maxTimeout;
        private int maxRetries;
        public int countRetries;
        private bool Active;
        private bool CancelRequested;
        private bool EnableNDCScreen;
        private string ActivityName;

        //public MoreTime(string screenName, int maxTimeout, int maxRetries, bool enableKeyboard, IBusinessObject bo, string advertID, AccessibilityType accessibility)
        public MoreTime(string screenName, int maxTimeout, int maxRetries, bool enableKeyboard, Business.Core core, bool enableNDCScreen, string activityName)
        {
            this.Core = core;
            this.screenName = screenName.ToUpper().PadLeft(3, '0').Replace(".NDC", "");
            this.maxTimeout = maxTimeout;
            this.maxRetries = maxRetries;
            this.countRetries = 0;
            this.Active = false;
            this.CancelRequested = false;
            this.EnableNDCScreen = enableNDCScreen;
            this.ActivityName = activityName;
        }

        public MoreTime() { }

        public void StartMoreTime()
        {
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleKeyPressreturn);
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleKeyPressreturn);
            this.Active = true;
            this.AskCustomer();
        }

        private void HandleKeyPressreturn(string FDKdata)
        {
            string yesKey = this.Core.MoreTimeConfig.YesKeys.ToString();
            string noKey = this.Core.MoreTimeConfig.NoKeys.ToString();
            try
            {
                Log.Info($"└->MORE TIME-> FDK data: {FDKdata}");
                if (FDKdata.Equals(yesKey) || FDKdata.Equals("ENTER"))
                {
                    this.Continue();
                }
                else if (FDKdata.Equals(noKey) || FDKdata.Equals("CANCEL"))
                {
                    this.Cancel();
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private bool HasAnotherRetry()
        {
            if (this.maxRetries == 0)
            {
                return true;
            }
            else
            {
                if (this.countRetries < this.maxRetries)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Lanza el evento para informar el resultado del more time.
        /// </summary>
        /// <param name="html">Mensaje de Log.</param>
        protected virtual void RaiseEvtMoreTime(MoreTimeResult result)
        {
            Log.Info($"└->MORE TIME-> EVT result: {result.ToString()} Count Retries: {this.countRetries} - Activity trigger: {this.ActivityName}");
            DelegateMoreTime tmp = EvtMoreTime;
            if (tmp != null)
                tmp(result);
        }

        private int CountRetries
        {
            get
            {
                return this.countRetries;
            }
        }
        public void ResetRetry()
        {
            Log.Info("└->MORE TIME-> Reset");
            this.countRetries = 0;
        }

        private void AskCustomer()
        {
            this.countRetries++;
            this.StartTimer();
            try
            {
                if (HasAnotherRetry())
                {
                    KeyMask_Type keyMask = this.Core.GetKeyMaskData("3");
                    this.Core.WriteEJ($"START MORE TIME: {this.countRetries}");
                    Log.Info($"└->MORE TIME-> Count Retries: {this.countRetries} - Activity trigger: {this.ActivityName}");
                    if (this.EnableNDCScreen)
                    {
                        this.Core.MoreTimeConfig.OnMoreTimeAdvice.HandlerName = this.screenName;
                    }
                    if (this.CallHandler(this.Core.MoreTimeConfig.OnMoreTimeAdvice))
                    {
                        this.StartTimer();
                    }
                    else
                    {
                        Log.Error($"Can´t show screen: {this.Core.MoreTimeConfig.OnMoreTimeAdvice.HandlerName}");
                        this.Cancel();
                    }
                }
                else
                {
                    this.TimeOut();
                }
            }
            catch
            {
                this.Cancel();
            }
        }

        protected bool CallHandler(StateEvent stateEvent)
        {
            bool ret = false;
            Log.Info($"/===> CallHandler: Action = {stateEvent.Action.ToString()} - HandlerName = {stateEvent.HandlerName} Parameters = {stateEvent.Parameters.ToString()}");
            switch (stateEvent.Action)
            {
                case StateEvent.EventType.runScript:
                    //this.Core.RaiseInvokeJavascript(stateEvent.HandlerName, stateEvent.Parameters);
                    this.Core.RaiseEvtScreenData(stateEvent);
                    ret = true;
                    break;
                case StateEvent.EventType.navigate:
                    StateEvent se = stateEvent.Clone();
                    se.HandlerName = this.GetScreenPath(stateEvent.HandlerName);
                    this.Core.RaiseEvtScreenData(se);
                    ret = true;
                    break;
                case StateEvent.EventType.ndcScreen:
                    Entities.KeyMask_Type keyMask = (Entities.KeyMask_Type)stateEvent.Parameters;
                    ret = this.Core.ShowGeneralNDCScreen(stateEvent.HandlerName, keyMask);
                    break;
            }
            return ret;
        }

        private void TimeOut()
        {
            Log.Info($"└->MORE TIME-> TIMEOUT - Activity trigger: {this.ActivityName} - Count reach limit: {this.countRetries}");
            this.Core.WriteEJ("MORE TIME RESULT: TIMEOUT");
            this.CancelRequested = true;
            if (this.Active)
            {
                this.StopTimer();
            }
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleKeyPressreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleKeyPressreturn);
            this.RaiseEvtMoreTime(MoreTimeResult.Timeout);
            this.CancelRequested = false;
        }

        private void Cancel()
        {
            Log.Info($"└->MORE TIME-> CANCEL - Activity trigger: {this.ActivityName}");
            this.Core.WriteEJ("MORE TIME RESULT: CANCEL");
            this.CancelRequested = true;
            if (this.Active)
            {
                this.StopTimer();
            }
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleKeyPressreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleKeyPressreturn);
            this.RaiseEvtMoreTime(MoreTimeResult.Cancel);
            this.CancelRequested = false;
        }

        private void Continue()
        {
            Log.Info($"└->MORE TIME-> CONTINUE - Activity trigger: {this.ActivityName}");
            this.Core.WriteEJ("MORE TIME RESULT: ENTER");
            this.CancelRequested = true;
            if (this.Active)
            {
                this.StopTimer();
            }
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleKeyPressreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleKeyPressreturn);
            this.RaiseEvtMoreTime(MoreTimeResult.Continue);
            this.CancelRequested = false;
        }

        private void StartTimer()
        {
            if (this.timer == null)
            {
                this.timer = new System.Timers.Timer();
            }
            this.timer.AutoReset = false;
            this.timer.Interval = (double)(this.maxTimeout * 1000);
            this.timer.Elapsed += new ElapsedEventHandler(this.ElapsedTimer);
            this.timer.Enabled = true;
            this.timer.Start();
        }

        private void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Elapsed -= new ElapsedEventHandler(this.ElapsedTimer);
                this.timer.Enabled = false;
                this.timer.Stop();
            }
        }

        private void ElapsedTimer(object sender, ElapsedEventArgs e)
        {
            if (this.Active)
            {
                Log.Info($"└->MORE TIME-> ABANDON - Activity trigger: {this.ActivityName}");
                this.Core.WriteEJ("MORE TIME RESULT: ABANDON");
                this.Active = false;
                this.StopTimer();
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleKeyPressreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleKeyPressreturn);
                this.RaiseEvtMoreTime(MoreTimeResult.Timeout);
            }
        }

        public string GetScreenPath(string screenName)
        {
            string screenHtmPath = "";
            try
            {
                if (this.Core.ScreenConfiguration.UrlScreenEnable)
                {
                    screenHtmPath = $"{this.Core.ScreenConfiguration.UrlScreen}{screenName}";
                    Log.Info($"Screen Path: {screenHtmPath}");
                }
                else
                {
                    screenHtmPath = $"{Const.appPath}Screens\\{screenName}";
                    if (File.Exists(screenHtmPath))
                    {
                        Log.Info($"Screen Path: \"{screenHtmPath}\"");
                    }
                    else
                    {
                        Log.Error($"Screen {screenName} not found");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return screenHtmPath;
        }
    }
}

