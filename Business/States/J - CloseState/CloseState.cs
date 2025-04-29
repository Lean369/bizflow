using Entities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Business.CloseState
{
    public class CloseState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        CloseStateTableData_Type closeStateTableData; //Tabla con datos provenientes del download.
        PropertiesCloseState prop;
        public System.Timers.Timer timerScreen;
        bool ret = false;
        private StateEvent StateEventToProcess;

        #region "Constructor"
        public CloseState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "CloseState";
            this.closeStateTableData = (CloseStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesCloseState(alephATMAppData);
            CloseStateTableDataExtension_Type extensionTable = null;
            this.prop = this.GetProperties<PropertiesCloseState>(out ret, this.prop);
            if (ret)
            {
                if (this.closeStateTableData.Item1 != null)
                    extensionTable = (CloseStateTableDataExtension_Type)this.closeStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.ReceiptDeliveredScreenNumber))
                    this.prop.ReceiptDeliveredScreenNumber = this.closeStateTableData.ReceiptDeliveredScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.closeStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.NoReceiptDeliveredScreenNumber))
                    this.prop.NoReceiptDeliveredScreenNumber = this.closeStateTableData.NoReceiptDeliveredScreenNumber;
                if (string.IsNullOrEmpty(this.prop.CardRetainedScreenNumber))
                    this.prop.CardRetainedScreenNumber = this.closeStateTableData.CardRetainedScreenNumber;
                if (string.IsNullOrEmpty(this.prop.StatementDeliveredScreenNumber))
                    this.prop.StatementDeliveredScreenNumber = this.closeStateTableData.StatementDeliveredScreenNumber;
                if (string.IsNullOrEmpty(this.prop.BNANotesReturnedScreenNumber))
                    this.prop.BNANotesReturnedScreenNumber = this.closeStateTableData.BNANotesReturnedScreenNumber;
                if (string.IsNullOrEmpty(this.prop.Extension.StateNumber) && extensionTable != null)
                    this.prop.Extension.StateNumber = extensionTable.StateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension.CPMTakeDocumentScreenNumber) && extensionTable != null)
                    this.prop.Extension.CPMTakeDocumentScreenNumber = extensionTable.CPMTakeDocumentScreenNumber;
                if (this.prop.Extension.CPMDocumentRetainReturnFlag == CPMDocumentRetainReturnFlag_Type.Undefined && extensionTable != null)
                    this.prop.Extension.CPMDocumentRetainReturnFlag = extensionTable.CPMDocumentRetainReturnFlag;
                if (string.IsNullOrEmpty(this.prop.Extension.BCACoinsReturnedScreenNumber) && extensionTable != null)
                    this.prop.Extension.BCACoinsReturnedScreenNumber = extensionTable.BCACoinsReturnedScreenNumber;
                if (this.prop.Extension.BCACoinsReturnRetainFlag == BNANotesReturnRetainFlag_Type.Undefined && extensionTable != null)
                    this.prop.Extension.BCACoinsReturnRetainFlag = extensionTable.BCACoinsReturnRetainFlag;
                if (this.prop.Extension.BNANotesReturnRetainFlag == BNANotesReturnRetainFlag_Type.Undefined && extensionTable != null)
                    this.prop.Extension.BNANotesReturnRetainFlag = extensionTable.BNANotesReturnRetainFlag;
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
                this.PrepareMoreTime();
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
                this.AddEventHandlers();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                //Manejo de impresión y oferta de ticket
                if (this.Core.Bo.ExtraInfo != null && this.Core.Bo.ExtraInfo.LstPrintData.Count > 0)
                {
                    this.HandleNextPrtDataItem();
                }
                else
                {
                    this.SetCloseScreen();
                }


            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void PrintTicket(StateEvent se)
        {
            try
            {
                if (this.CallHandler(se))
                    Log.Info($"Print ticket: {se.HandlerName} OK");
                else
                    Log.Error($"Print ticket: {se.HandlerName} Error");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Recursive function to empty print data list.
        /// Analyzes next print data item on list.
        /// Holds state on standby on offer receipt.
        /// TODO: To be removed, this should be handled by the print state.
        /// </summary>
        private void HandleNextPrtDataItem()
        {
            this.StateEventToProcess = this.Core.Bo.ExtraInfo.LstPrintData[0];
            if (this.prop.BNANotesReturnedScreenNumber.Equals("001") && this.StateEventToProcess.Action == StateEvent.EventType.printReceipt)
            {
                this.StartTimer();
                this.CallHandler(this.prop.OnOfferReceipt);
            }
            else
            {
                this.PrintTicket(this.StateEventToProcess);
                this.Core.Bo.ExtraInfo.LstPrintData.Remove(this.StateEventToProcess);
                if (this.Core.Bo.ExtraInfo.LstPrintData.Count > 0)
                {
                    HandleNextPrtDataItem();
                }
                else SetCloseScreen();
            }
        }

        /// <summary>
        /// When theres no print data on list, shows closing screen and ends state.
        /// </summary>
        private void SetCloseScreen()
        {
            if (this.prop.ReceiptDeliveredScreenNumber != "000")
            {
                //Manejo de pantalla de despedida
                if (this.prop.OnClose.Action == StateEvent.EventType.ndcScreen)
                {
                    KeyMask_Type keyMask = null;
                    this.prop.OnClose.HandlerName = this.prop.ReceiptDeliveredScreenNumber;
                    this.prop.OnClose.Parameters = keyMask;
                }
                else if (this.prop.OnClose.Action == StateEvent.EventType.navigate)
                {
                    this.prop.OnClose.HandlerName = $"{this.prop.ReceiptDeliveredScreenNumber}.htm";
                }
                if (!this.CallHandler(this.prop.OnClose))
                {
                    this.SetActivityResult(StateResult.SWERROR, "000");
                    Log.Error($"Can´t show screen: {this.prop.OnClose.HandlerName}");
                }

                this.StartTimer();
            }
            else
            {
                // Sin pantalla
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
            }

        }

        /// <summary>
        /// Handles response from frontend on offer receipt.
        /// </summary>
        /// <param name="dataInput"></param>
        /// <param name="dataLink"></param>
        private void HandleInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", dataInput);
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "ReceiptOption":
                            {
                                if (dataInput == "print") this.PrintTicket(this.StateEventToProcess);
                                this.Core.Bo.ExtraInfo.LstPrintData.Remove(this.StateEventToProcess);
                                if (this.Core.Bo.ExtraInfo.LstPrintData.Count > 0)
                                {
                                    HandleNextPrtDataItem();
                                }
                                else SetCloseScreen();
                                this.StopTimer();
                                break;
                            }
                    }
                }
                this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "More time"

        public MoreTime moreTime;
        // Indicates if time-out occurs
        public bool timeout = false;

        private void PrepareMoreTime()
        {
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, false, this.ActivityName);
            this.moreTime.EvtMoreTime += new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
        }

        private void AnalyzeMoreTimeResult(MoreTimeResult result)
        {
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.ActivityStart();
                        break;
                    }
                case MoreTimeResult.Cancel:
                case MoreTimeResult.Timeout:
                    {
                        // Do not print anything
                        SetCloseScreen();
                        break;
                    }
            }
        }


        #endregion "More time"

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                switch (this.prop.StatementDeliveredScreenNumber)
                {
                    case "000":
                        this.Core.EndVisit();
                        break;
                    case "001":
                        Cleanup_OnLocal();
                        break;
                    case "002":
                        Cleanup_OnRemote();
                        Cleanup_OnLocal();
                        this.Core.EndVisit();
                        break;
                }
                this.ActivityResult = result;
                this.Quit();
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Cleanup_OnRemote()
        {
            Log.Debug("/--->");
            //api cleanup request
            if (this.prop.ApiCleanupRequestAfterCancel)
                AuthorizeCleanup(); //cart cleanup
        }
        private void Cleanup_OnLocal()
        {
            Log.Debug("/--->");
            if (this.Core.Bo.ExtraInfo != null)
                this.Core.Bo.ExtraInfo.SetExtraDataFields = new List<ExtraDataConf>(); //remove all fields that may be left from the previous operation. This is necessary when operating with barcodes since it doesnt pass through ChoicesSelectorState that clears it
        }

        private void AuthorizeCleanup()
        {
            Log.Debug("/--->");
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            try
            {
                Thread prtWndThd = new Thread(this.AuthorizeTransaction);
                prtWndThd.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void AuthorizeTransaction()
        {
            try
            {
                Log.Debug("/--->");
                var authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_EMTPYCART, null, "");
                if (authorizationResult.authorizationStatus != AuthorizationStatus.Authorized)
                {
                    Log.Error("Failed API cleanup request after cancel operation.");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AddEventHandlers()
        {
            this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandleInputData);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandleInputData);
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
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
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
