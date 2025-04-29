using Entities;
using Entities.PaymentService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.PrintState
{
    public class PrintState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        PrintStateTableData_Type PrintStateTableData; //Tabla con datos provenientes del download.
        PropertiesPrintState prop;
        public System.Timers.Timer timerOperation;
        public System.Timers.Timer timerScreen;
        bool ret = false;
        bool receiptDataSuccess = false;
        private StateEvent StateEventToProcess;

        #region "Constructor"


        public PrintState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "PrintState";
            this.PrintStateTableData = (PrintStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesPrintState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesPrintState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.PrintStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.GoodOperationNextState))
                    this.prop.GoodOperationNextState = this.PrintStateTableData.GoodOperationNextState;
                if (string.IsNullOrEmpty(this.prop.HardwareFaultNextState))
                    this.prop.HardwareFaultNextState = this.PrintStateTableData.HardwareFaultNextState;
                if (this.prop.UnitNumber == PrinterFlag_Type.none)
                    this.prop.UnitNumber = this.PrintStateTableData.UnitNumber;
                if (string.IsNullOrEmpty(this.prop.Operation))
                    this.prop.Operation = this.PrintStateTableData.Operation;
                if (this.prop.ScreenTimer == 0)
                    this.prop.ScreenTimer = this.PrintStateTableData.ScreenTimer;
                if (string.IsNullOrEmpty(this.prop.FdkActiveMask))
                    this.prop.FdkActiveMask = this.PrintStateTableData.FdkActiveMask;
                if (string.IsNullOrEmpty(this.prop.PrintBufferID))
                    this.prop.PrintBufferID = this.PrintStateTableData.PrintBufferID;
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
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleOtherKeysReturn);
                
                if (this.prop.OnPrint.Action == StateEvent.EventType.navigate)
                {
                    this.CallHandler(this.prop.OnPrint);
                }
                //Construye un ticket
                switch (this.prop.Operation)
                {
                    case "000"://Imprime ticket con datos ya obtenidos
                        {
                            //bypass
                            //this.Core.Bo.ExtraInfo.QRdata = "https://aplicaciones.redpagos.com.uy/cfe/areportesfacturaelectronica.aspx?PRODUCCION,908,2854178,04%2F06%2F24,eHxRNC,696041336";
                            if (this.Core.Bo.ExtraInfo.ReceiptRequired)
                            {
                                //Print ticket ok
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, true);
                                this.ProcessPrinterData(this.prop.OnSendTicketToBD, true);
                            }
                            else
                            {
                                //Print ticket host error
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError1, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError2, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterError, true);
                                this.ProcessPrinterData(this.prop.OnSendTicketToBDError, true);
                            }
                            this.StartTimer();
                            break;
                        }
                    case "001": //Autoriza datos de ticket e imprime
                        {
                            SetReceiptData();
                            if (this.Core.Bo.ExtraInfo.ReceiptRequired)
                            {
                                //Print ticket ok
                                if(receiptDataSuccess)
                                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, true);
                                else
                                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptDataFail, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, true);
                                this.ProcessPrinterData(this.prop.OnSendTicketToBD, true);
                            }
                            else
                            {
                                //Print ticket host error
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError1, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError2, true);
                                this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterError, true);
                                this.ProcessPrinterData(this.prop.OnSendTicketToBDError, true);
                            }
                            this.StartTimer();
                            break;
                        }
                    case "002": //Autoriza datos, ofrece ticket e imprime si es aceptado
                        {
                            SetReceiptData();
                            
                            this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, true);
                            this.ProcessPrinterData(this.prop.OnSendTicketToBD, true);
                            this.StartOperationTimer();
                            this.prop.OnShowReceiptOptions.Parameters = this.CreateReceiptOptions(
                                this.Core.Bo.ExtraInfo.LstPrintData.FirstOrDefault(
                                    i => i.HandlerName == "Pr1AccountDepositCash"
                            ).Parameters.ToString());
                            this.CallHandler(this.prop.OnShowReceiptOptions);
                            
                            break;
                        }

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
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOtherKeysReturn);
                if (timerScreen != null)
                {
                    this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
                }
                if (timerOperation != null)
                {
                    this.timerOperation.Elapsed -= new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
                }
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
            this.timerScreen.Interval = prop.ScreenTimer * 1000;
            this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
        }

        private void StartOperationTimer()
        {
            if (this.timerOperation == null)
                timerOperation = new System.Timers.Timer();
            this.timerOperation.AutoReset = false;
            this.timerOperation.Interval = prop.OperationTimer * 1000;
            this.timerOperation.Elapsed += new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
            this.timerOperation.Enabled = true;
            this.timerOperation.Start();
        }

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void timerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.StopTimer();
            this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodOperationNextState);
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
            if (timerOperation != null)
            {
                this.timerOperation.Elapsed -= new System.Timers.ElapsedEventHandler(timerScreen_Elapsed);
                this.timerOperation.Enabled = false;
                this.timerOperation.Stop();
            }
        }

        private void HandleOtherKeysReturn(string othersKeys)
        {
            Log.Info($"/--> Key press: {othersKeys}");
            switch (othersKeys)
            {
                case "PRINT":
                    HandleNextPrtDataItem();
                    this.CallHandler(this.prop.OnTakeReceipt);
                    this.StartTimer();
                    break;
                case "CONTINUE":
                    // remove all print data in list
                    this.Core.Bo.ExtraInfo.LstPrintData.RemoveAll(item => item.Action == StateEvent.EventType.printReceipt);
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.GoodOperationNextState);
                    break;
            }
        }

        // Opciones de offer receipt, harcodeadas
        public List<string> ReceiptOptions { get; set; } = new List<string>() {
            "PRINT", "CONTINUE"
        };

        /// <summary>
        /// Receipt options for the user.
        /// </summary>
        public string CreateReceiptOptions(string rcptData = null)
        {
            
            JArray result = new JArray();
            rcptData = rcptData.Replace("\n", "<br>");
            for (int i = 0; i < ReceiptOptions.Count; i++)
            {
                JObject jObject = new JObject
                {
                    ["enabled"] = (JToken)true,
                    ["id"] = (JToken)i,
                    ["type"] = ReceiptOptions[i],
                };
                result.Add(jObject);
            }

            JObject obj = new JObject
            {
                ["offerReceipt"] = (JToken)true,
                ["preview"] = new JObject
                {
                    ["enabled"] = (JToken)(string.IsNullOrEmpty(rcptData) ? false : true),
                    ["receiptData"] = rcptData
                },
                ["options"] = result
            };
            return obj.ToString(Formatting.None);
        }

        private void SetReceiptData()
        {
            //this.Core.Bo.ExtraInfo.ErrorCode = null;
            if (this.Core.Bo.ExtraInfo.ExtraData != null)
            {
                this.Core.Bo.ExtraInfo.ExtraData.Clear();
            } else
            {
                this.Core.Bo.ExtraInfo.ExtraData = new List<ExtraData>();
            }
            //DATA FROM API
            var apiReceiptData = GetReceiptDataFromApi();
            if (apiReceiptData.success)
            {
                receiptDataSuccess = true;
                if (!string.IsNullOrEmpty(apiReceiptData.receipt.ElectronicInvoice))
                    this.Core.Bo.ExtraInfo.QRdata = apiReceiptData.receipt.ElectronicInvoice;
                AddReceiptExtraField(apiReceiptData.receipt.Summary);
                //AddReceiptExtraField(string.Empty, "messageToUser1");
                if (apiReceiptData.receipt.Details != null)
                    foreach (var detail in apiReceiptData.receipt.Details)
                    {
                        AddReceiptPaymentDetails(detail);
                    }
            } else
            {
                // Transaction completed, receipt data failed
                receiptDataSuccess = false;
            }
            if (this.Core.Bo.ExtraInfo.Amount != 0)
            { //DATA FROM LOCAL 
                //amount to pay
                var payAmo = Utilities.Utils.FormatCurrency(this.Core.Bo.ExtraInfo.Amount, this.Core.Bo.ExtraInfo.Currency, 2);
                AddReceiptAmount(string.Format("{0}", payAmo), "noteTotal");
                //deposited amount
                var depositedAmount = GetDepositedAmount();
                var depoAmo = Utilities.Utils.FormatCurrency(depositedAmount, this.Core.Bo.ExtraInfo.Currency, 2);
                AddReceiptAmount(string.Format("{0}", depoAmo), "paymentPay");
                //change to be dispensed
                decimal dispenseAmount = depositedAmount - this.Core.Bo.ExtraInfo.Amount;
                var dispenseAmo = Utilities.Utils.FormatCurrency(dispenseAmount, this.Core.Bo.ExtraInfo.Currency, 2);
                AddReceiptAmount(string.Format("{0}", dispenseAmo), "paymentChange");
            }

            this.Core.Bo.ExtraInfo.ReceiptRequired = true;
        }

        private (bool success, PaymentReceipt receipt, string message) GetReceiptDataFromApi()
        {
            if (!this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("PaymentCartId") || this.Core.Bo.ExtraInfo.HostExtraData["PaymentCartId"] is null)
            {
                Log.Error("Unable to obtain PaymentCartId");
                return (false, null, "");
            }
            var requestData = new Dictionary<PropKey, object>
            {
                { PropKey.CartID, this.Core.Bo.ExtraInfo.HostExtraData["PaymentCartId"] },
            };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_RECEIPT, null, this.prop.HostName);
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            return (rep.sc, rep.sc ? (PaymentReceipt)rep.rs.Data : null, rep.rs.Message);
        }

        private (bool sc, T rs) CastResult<T>(AuthorizationResult authorizationResult)
        {
            //var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            //T data = (T)authorizationResult.Response;
            //return (ok, data);
            var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            try
            {
                T data = (T)authorizationResult.Response;
                return (ok, data);
            }
            catch (InvalidCastException)
            {
                return (false, default);
            }
        }

        private void AddReceiptExtraField(string value, string tagname = null)
        {
            this.Core.Bo.ExtraInfo.ExtraData.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }
        private void AddReceiptAmount(string value, string tagname = null)
        {
            if (this.Core.Bo.ExtraInfo.PaymentAmounts == null)
                this.Core.Bo.ExtraInfo.PaymentAmounts = new List<ExtraData>();
            this.Core.Bo.ExtraInfo.PaymentAmounts.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }
        private void AddReceiptPaymentDetails(string value, string tagname = null)
        {
            if (this.Core.Bo.ExtraInfo.PaymentData == null)
                this.Core.Bo.ExtraInfo.PaymentData = new List<ExtraData>();
            this.Core.Bo.ExtraInfo.PaymentData.Add(new ExtraData
            {
                ExtraDataType = Enums.ExtraDataType.dynamic,
                TagName = tagname ?? "keyless",
                TagValue = value
            });
        }

        private decimal GetDepositedAmount()
        {
            var itemsDep = new List<Bills>();
            this.Core.Bo.ExtraInfo.CashInMultiCashData.ListCashInInfo.ForEach(c => {
                itemsDep.AddRange(c.Bills.Where(b => b.Currency.Equals(this.Core.Bo.ExtraInfo.Currency)).ToList());
            });
            decimal depositedAmount = 0;
            itemsDep.ForEach(item => {
                depositedAmount += item.Quantity * item.Value;
            });
            return depositedAmount;
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
        /// </summary>
        private void HandleNextPrtDataItem()
        {
            this.StateEventToProcess = this.Core.Bo.ExtraInfo.LstPrintData
                .FirstOrDefault(item => item.Action == StateEvent.EventType.printReceipt);

            if (this.StateEventToProcess != null) { 
                this.PrintTicket(this.StateEventToProcess);
                this.Core.Bo.ExtraInfo.LstPrintData.Remove(this.StateEventToProcess);
                HandleNextPrtDataItem();
            }
        }

    }
}
