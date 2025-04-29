using Entities;
using System;
using System.Text;

namespace Business.PreSetOperationCodeBufferState
{
    public class PreSetOperationCodeBufferState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        PreSetOperationCodeBufferStateTableData_Type preSetOperationCodeBufferStateTableData; //Tabla con datos provenientes del download.
        PropertiesPreSetOperationCodeBufferState prop;
        bool ret = false;

        #region "Constructor"
        public PreSetOperationCodeBufferState(StateTable_Type stateTable)
        {
            this.ActivityName = "PreSetOperationCodeBufferState";
            this.preSetOperationCodeBufferStateTableData = (PreSetOperationCodeBufferStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesPreSetOperationCodeBufferState();
            PreSetOperationCodeBufferStateTableDataExtension_Type extensionTable = null;
            this.prop = this.GetProperties<PropertiesPreSetOperationCodeBufferState>(out ret, this.prop);
            if (ret)
            {
                if (this.preSetOperationCodeBufferStateTableData.Item1 != null)
                    extensionTable = (PreSetOperationCodeBufferStateTableDataExtension_Type)this.preSetOperationCodeBufferStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.preSetOperationCodeBufferStateTableData.NextStateNumber;
                if (this.prop.BufferEntriesCleared == -1)
                    this.prop.BufferEntriesCleared = this.preSetOperationCodeBufferStateTableData.BufferEntriesCleared;
                if (this.prop.BufferEntriesSetToA == -1)
                    this.prop.BufferEntriesSetToA = this.preSetOperationCodeBufferStateTableData.BufferEntriesSetToA;
                if (this.prop.BufferEntriesSetToB == -1)
                    this.prop.BufferEntriesSetToB = this.preSetOperationCodeBufferStateTableData.BufferEntriesSetToB;
                if (this.prop.BufferEntriesSetToC == -1)
                    this.prop.BufferEntriesSetToC = this.preSetOperationCodeBufferStateTableData.BufferEntriesSetToC;
                if (this.prop.BufferEntriesSetToD == -1)
                    this.prop.BufferEntriesSetToD = this.preSetOperationCodeBufferStateTableData.BufferEntriesSetToD;
                if (string.IsNullOrEmpty(this.prop.Extension.StateNumber) && extensionTable != null)
                    this.prop.Extension.StateNumber = extensionTable.StateNumber;
                if (this.prop.Extension.BufferEntriesSetToF == -1 && extensionTable != null)
                    this.prop.Extension.BufferEntriesSetToF = extensionTable.BufferEntriesSetToF;
                if (this.prop.Extension.BufferEntriesSetToG == -1 && extensionTable != null)
                    this.prop.Extension.BufferEntriesSetToG = extensionTable.BufferEntriesSetToG;
                if (this.prop.Extension.BufferEntriesSetToH == -1 && extensionTable != null)
                    this.prop.Extension.BufferEntriesSetToH = extensionTable.BufferEntriesSetToH;
                if (this.prop.Extension.BufferEntriesSetToI == -1 && extensionTable != null)
                    this.prop.Extension.BufferEntriesSetToI = extensionTable.BufferEntriesSetToI;
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
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                string text = this.Core.Bo.ExtraInfo.OperationCodeData;
                text = this.ClearOperationCode(text, this.prop.BufferEntriesCleared);
                text = this.SetOperationCode(text, this.prop.BufferEntriesSetToA, 'A');
                text = this.SetOperationCode(text, this.prop.BufferEntriesSetToB, 'B');
                text = this.SetOperationCode(text, this.prop.BufferEntriesSetToC, 'C');
                text = this.SetOperationCode(text, this.prop.BufferEntriesSetToD, 'D');
                text = this.SetOperationCode(text, this.prop.Extension.BufferEntriesSetToF, 'F');
                text = this.SetOperationCode(text, this.prop.Extension.BufferEntriesSetToG, 'G');
                text = this.SetOperationCode(text, this.prop.Extension.BufferEntriesSetToH, 'H');
                text = this.SetOperationCode(text, this.prop.Extension.BufferEntriesSetToI, 'I');
                this.Core.Bo.ExtraInfo.OperationCodeData = text;
                Log.Info("PreSetOperationCode.OperationCodeData=(" + this.Core.Bo.ExtraInfo.OperationCodeData + ")");
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private string ClearOperationCode(string operationCode, int iClearMask)
        {
            string clearMask = iClearMask.ToString();
            StringBuilder stringBuilder = new StringBuilder(operationCode);
            if (!string.IsNullOrEmpty(clearMask))
            {
                try
                {
                    byte b = byte.Parse(clearMask);
                    for (byte b2 = 0; b2 < 8; b2 += 1)
                    {
                        if ((b >> (int)b2 & 1) == 0)
                        {
                            stringBuilder[(int)b2] = ' ';
                        }
                    }
                }
                catch
                {
                }
            }
            return stringBuilder.ToString();
        }

        private string SetOperationCode(string operationCode, int iPreSetMask, char charToSet)
        {
            string preSetMask = iPreSetMask.ToString();
            StringBuilder stringBuilder = new StringBuilder(operationCode);
            if (!string.IsNullOrEmpty(preSetMask))
            {
                try
                {
                    byte b = byte.Parse(preSetMask);
                    for (byte b2 = 0; b2 < 8; b2 += 1)
                    {
                        if ((b >> (int)b2 & 1) == 1)
                        {
                            stringBuilder[(int)b2] = charToSet;
                        }
                    }
                }
                catch
                {
                }
            }
            return stringBuilder.ToString();
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
    }
}
