using Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace Business.ShipoutState
{
    public class ShipoutState : StateTransition
    {
        private static readonly NLog.Logger Log = LogManager.GetLogger("LOG");
        ShipoutStateTableData_Type shipoutStateStateTableData; //Tabla con datos provenientes del download.
        PropertiesShipoutState prop;
        bool ret = false;
        Const.TerminalMode CurrentTerminalMode;

        #region "Constructor"
        public ShipoutState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "ShipoutState";
            this.shipoutStateStateTableData = (ShipoutStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesShipoutState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesShipoutState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.SendCollection))
                    this.prop.SendCollection = this.shipoutStateStateTableData.SendCollection;
                if (string.IsNullOrEmpty(this.prop.SendCollectionDeclared))
                    this.prop.SendCollectionDeclared = this.shipoutStateStateTableData.SendCollectionDeclared;
                if (string.IsNullOrEmpty(this.prop.SendContents))
                    this.prop.SendContents = this.shipoutStateStateTableData.SendContents;
                if (string.IsNullOrEmpty(this.prop.PrintTicket))
                    this.prop.PrintTicket = this.shipoutStateStateTableData.PrintTicket;
                if (string.IsNullOrEmpty(this.prop.UpdateTSN))
                    this.prop.UpdateTSN = this.shipoutStateStateTableData.UpdateTSN;
                if (string.IsNullOrEmpty(this.prop.ClearLogicalCounters))
                    this.prop.ClearLogicalCounters = this.shipoutStateStateTableData.ClearLogicalCounters;
                if (string.IsNullOrEmpty(this.prop.NextState))
                    this.prop.NextState = this.shipoutStateStateTableData.NextState;
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
            Log.Info($"/--> Shipout mutex: {base.Core.ShipoutMutex}");
            this.CurrentTerminalMode = base.Core.Sdo.SOH.Mode;
            this.Core.Sdo.SOH.Mode = Const.TerminalMode.InShipout;
            this.EnableJournal = prop.Journal.EnableJournal;
            this.WriteEJ($"Next State [{base.Core.AlephATMAppData.StateShipout}] {ActivityName}");
            this.Core.Sdo.EvtCompletionReceive += HandlerCompletionReceive;
            if (!this.Core.AlephATMAppData.ShipOutInBackgroundEnable)
            {
                this.CallHandler(this.prop.OnShipoutStart);
            }
            this.ProcessShipout();
        }

        private void ProcessShipout()
        {
            try
            {
                if (this.Core.ShipoutMutex)
                {
                    this.Core.ShipoutMutex = false;
                    this.EnableJournal = this.prop.Journal.EnableJournal;
                    this.SendShipOutDataToHost();
                    if (this.prop.ClearLogicalCounters.Equals("001"))
                    {
                        if (this.Core.Counters.ClearContents())
                            Log.Info("Clear logical counters OK");
                        else
                            Log.Error($"Clear logical counters Error");
                    }
                    Thread.Sleep(30);
                    if (prop.SendContents.Equals("001"))
                    {
                        this.Core.SendContentsToHost();
                    }
                    if (this.prop.ClearPhysicalCounters.Equals("001"))
                    {
                        Log.Info("Physical counters: Open CIM");
                        this.Core.Sdo.CIM_AsyncOpen();
                    }
                    this.Core.Sdo.VerifyBagLevelFill();//Informo bolsa vacía
                }
                else
                    Log.Warn("ShipOut in progress...");
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            finally
            {
                base.Core.ShipoutMutex = true;
            }
        }

        private void HandlerCompletionReceive(DeviceMessage dm)
        {
            bool flag = false;
            if (dm.Device != Enums.Devices.CashAcceptor)
            {
                return;
            }
            Completion completion = (Completion)dm.Payload;
            if (completion.CompletionCode == CompletionCodeEnum.Success)
            {
                Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {completion.CompletionCode}");
            }
            else
            {
                Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {completion.CompletionCode}");
            }
            switch (dm.Command)
            {
                case Enums.Commands.Open:
                    if (completion.CompletionCode == CompletionCodeEnum.Success)
                        this.Core.Sdo.CIM_Status();
                    else
                        this.EndShipOut();
                    break;
                case Enums.Commands.Status:
                    base.Core.Sdo.CIM_GetCounters();
                    if (completion.CompletionCode == CompletionCodeEnum.Success)
                    {
                        completion = (dm.Payload as Completion);
                        StatusCIM statusCIM = Utils.JsonDeserialize<StatusCIM>(out flag, completion.Data);
                        if (flag)
                        {
                            if (!statusCIM.IntermediateStacker.Equals("0") && !statusCIM.IntermediateStacker.Equals("5"))
                            {
                                this.WriteEJ($"NOTES IN ESCROW PRESENT -WARNING-");
                                Log.Warn($"Notes in escrow detected");
                            }
                        }
                        else { Log.Error($"Status CIM parse error"); }
                    }
                    else { Log.Error($"Status CIM return: {completion.CompletionCode}"); }
                    break;
                case Enums.Commands.GetCounters:
                    GlobalAppData.Instance.SetScratchpad("localCounters", completion.Data);
                    this.CallHandler(prop.OnPrintLocalCounters);
                    this.Core.Sdo.CIM_ClearCounters();
                    break;
                case Enums.Commands.ClearCounters:
                    this.EndShipOut();
                    break;
            }
        }

        internal bool SendShipOutDataToHost()
        {
            bool flag = false;
            List<Detail> listDetail = new List<Detail>();
            try
            {
                this.Core.Counters.UpdateBATCH();
                if (this.prop.SendCollection.Equals("001"))
                {
                    Contents contents = base.Core.Counters.GetContents(Detail.ContainerIDType.CashAcceptor);
                    contents.LstDetail.ForEach(d =>
                    {
                        listDetail.Add(new Detail(d.Currency, d.ContainerId, d.ContainerType, this.Core.GetCollectionId(Enums.TransactionType.COLLECTION), d.LstItems));
                    });
                    AuthorizationResult authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.COLLECTION, new Contents(listDetail), this.prop.HostName);
                    if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                    {
                        if (this.prop.PrintTicket.Equals("001"))
                        {
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterCashDeposit, true);
                            this.ProcessPrinterData(this.prop.OnPrintTicketToBDCashDeposit, true);
                        }
                        flag = true;
                    }
                    else
                    {
                        Log.Error($"Collection: communication host error");
                        if (this.prop.PrintTicket.Equals("001"))
                        {
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterCashDepositError, true);
                            this.ProcessPrinterData(this.prop.OnPrintTicketToBDCashDepositError, true);
                        }
                        flag = false;
                    }
                    this.UpdateSecuence();
                }
                Thread.Sleep(100);
                if (this.prop.SendCollectionDeclared.Equals("001"))
                {
                    Contents contents = base.Core.Counters.GetContents(Detail.ContainerIDType.Depository);
                    listDetail = new List<Detail>();
                    contents.LstDetail.ForEach(d =>
                    {
                        listDetail.Add(new Detail(d.Currency, d.ContainerId, d.ContainerType, this.Core.GetCollectionId(Enums.TransactionType.COLLECTION_DECLARED), d.LstItems));
                    });
                    AuthorizationResult authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.COLLECTION_DECLARED, new Contents(listDetail), this.prop.HostName);
                    if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized && flag)
                    {
                        if (this.prop.PrintTicket.Equals("001"))
                        {
                            this.ProcessPrinterData(prop.OnPrintTicketOnReceiptPrinterBagDropDeposit, true);
                            this.ProcessPrinterData(prop.OnPrintTicketToBDBagDropDeposit, true);
                        }
                        flag = true;
                    }
                    else
                    {
                        Log.Error($"Collection declared: communication host error");
                        if (this.prop.PrintTicket.Equals("001"))
                        {
                            this.ProcessPrinterData(prop.OnPrintTicketOnReceiptPrinterBagDropDepositError, true);
                            this.ProcessPrinterData(prop.OnPrintTicketToBDBagDropDepositError, true);
                        }
                        flag = false;
                    }
                    this.UpdateSecuence();
                }
                if (flag)
                {
                    if (this.prop.PrintTicket.Equals("001"))
                    {
                        this.ProcessPrinterData(prop.OnPrintTicketOnJournalPrinter, true);
                    }
                }
                else if (this.prop.PrintTicket.Equals("001"))
                {
                    this.ProcessPrinterData(prop.OnPrintTicketOnJournalPrinterError, true);
                }
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            return flag;
        }

        private void UpdateSecuence()
        {
            if (this.prop.UpdateTSN.Equals("001"))
            {
                this.Core.Counters.UpdateTSN();
            }
        }

        private void EndShipOut()
        {
            this.Core.Sdo.SOH.ShipOutAvailable = true;
            this.Core.Sdo.CIM_AsyncClose();
            this.Core.Sdo.EvtCompletionReceive -= HandlerCompletionReceive;
            if (this.Core.AlephATMAppData.ShipOutInBackgroundEnable)
            {
                this.Core.Sdo.SOH.Mode = CurrentTerminalMode;
            }
            else
            {
                this.GoToOutOfShipoutMode();
            }
        }

        internal void GoToOutOfShipoutMode()
        {
            try
            {
                Log.Debug("/--->");
                if (this.Core.AlephATMAppData.OperationMode == Const.OperationMode.Batch)
                {
                    this.Core.RequestChangeMode(Const.TerminalMode.InService);
                }
                else
                {
                    this.Core.RequestChangeMode(Const.TerminalMode.OutOfService);
                }
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
        }


        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                //this.ActivityResult = result;
                //this.Quit();
                //this.Core.SetNextState(result, nextState);
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
