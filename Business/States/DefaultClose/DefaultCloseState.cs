using System;
using Entities;

namespace Business.DefaultCloseState
{
    public class DefaultCloseState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        PropertiesDefaultCloseState prop;
        public System.Timers.Timer timerScreen;
        bool ret = false;

        public DefaultCloseState()
        {
            this.ActivityName = "DefaultCloseState";
            this.prop = new PropertiesDefaultCloseState();
            this.prop = this.GetProperties<PropertiesDefaultCloseState>(out ret, this.prop);
            if (!ret)
                Log.Error($"->Can´t get properties of Activity: {this.ActivityName}");
            this.PrintProperties(this.prop, "ZZZ");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_core"></param>
        /// <returns></returns>
        public override bool InitializeActivity(Core core)
        {
            bool ret = false;
            try
            {
                this.Core = core;
                Log.Info("/--> Activity Name: {0}", this.ActivityName);
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override void ActivityStart()
        {
            try
            {
                Log.Info("/--> State error: {0}", this.Core.ErrorTransitionState);
                this.CurrentState = ProcessState.INPROGRESS;
                this.Core.EndVisit();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                if (!this.Core.ErrorTransitionState.Equals("000"))
                    this.StartTimer();

                if (this.prop.OnClose.Action == StateEvent.EventType.ndcScreen)
                {
                    KeyMask_Type keyMask = null;
                    this.prop.OnClose.HandlerName = this.prop.Screens.Error;
                    this.prop.OnClose.Parameters = keyMask;
                }
                else if (this.prop.OnClose.Action == StateEvent.EventType.navigate)
                {
                    this.prop.OnClose.HandlerName = string.Format("E00.htm");
                }
                if (!this.CallHandler(this.prop.OnClose))
                {
                    this.SetActivityResult(StateResult.SWERROR, "000");
                    Log.Error(string.Format("Can´t show screen: {0}", this.prop.OnClose.HandlerName));
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.Quit();
                this.WriteEJ(string.Format("State result of {0}: {1}", this.ActivityName, result.ToString()));
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer()
        {
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.TimeOut.ErrorScreen * 1000;
            this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
        }

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void timerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.StopTimer();
            this.SetActivityResult(StateResult.SUCCESS, "000");
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            if (timerScreen != null)
            {
                this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
                this.timerScreen.Enabled = false;
                this.timerScreen.Stop();
            }
        }
    }
}
