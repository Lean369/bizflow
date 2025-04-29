using Entities;
using System;

namespace Business.FDKSwitchState
{
    public class FDKSwitchState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        FDKSwitchStateTableData_Type fDKSwitchStateTableData; //Tabla con datos provenientes del download.
        PropertiesFDKSwitchState prop;
        bool ret = false;

        #region "Constructor"
        public FDKSwitchState(StateTable_Type stateTable)
        {
            this.ActivityName = "FDKSwitchState";
            this.fDKSwitchStateTableData = (FDKSwitchStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesFDKSwitchState();
            this.prop = this.GetProperties<PropertiesFDKSwitchState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.FDKANextStateNumber))
                    this.prop.FDKANextStateNumber = this.fDKSwitchStateTableData.FDKANextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKBNextStateNumber))
                    this.prop.FDKBNextStateNumber = this.fDKSwitchStateTableData.FDKBNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKCNextStateNumber))
                    this.prop.FDKCNextStateNumber = this.fDKSwitchStateTableData.FDKCNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKDNextStateNumber))
                    this.prop.FDKDNextStateNumber = this.fDKSwitchStateTableData.FDKDNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKFNextStateNumber))
                    this.prop.FDKFNextStateNumber = this.fDKSwitchStateTableData.FDKFNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKGNextStateNumber))
                    this.prop.FDKGNextStateNumber = this.fDKSwitchStateTableData.FDKGNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKHNextStateNumber))
                    this.prop.FDKHNextStateNumber = this.fDKSwitchStateTableData.FDKHNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.FDKINextStateNumber))
                    this.prop.FDKINextStateNumber = this.fDKSwitchStateTableData.FDKINextStateNumber;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }
        #endregion "Constructor"

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
                Log.Info($"/--> Activity Name: {this.ActivityName}");
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override void ActivityStart()
        {
            string lastFDKPressed, nextStateNumber = string.Empty;
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                switch (lastFDKPressed = this.Core.Bo.LastFDKPressed)
                {
                    case "A":
                        nextStateNumber = this.prop.FDKANextStateNumber;
                        break;
                    case "B":
                        nextStateNumber = this.prop.FDKBNextStateNumber;
                        break;
                    case "C":
                        nextStateNumber = this.prop.FDKCNextStateNumber;
                        break;
                    case "D":
                        nextStateNumber = this.prop.FDKDNextStateNumber;
                        break;
                    case "F":
                        nextStateNumber = this.prop.FDKFNextStateNumber;
                        break;
                    case "G":
                        nextStateNumber = this.prop.FDKGNextStateNumber;
                        break;
                    case "H":
                        nextStateNumber = this.prop.FDKHNextStateNumber;
                        break;
                    case "I":
                        nextStateNumber = this.prop.FDKINextStateNumber;
                        break;
                }
                this.SetActivityResult(StateResult.SUCCESS, nextStateNumber);
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
    }
}
