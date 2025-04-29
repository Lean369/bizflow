using Entities;
using Entities.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Business.CoinDispenserState
{
    public class CoinDispenserState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        CoinDispenserStateTableData_Type CoinDispenserStateTableData; //Tabla con datos provenientes del download.
        PropertiesCoinDispenserState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();

        #region "Constructor"
        public CoinDispenserState(StateTable_Type stateTable)
        {
            this.ActivityName = "CoinDispenserState";
            this.CoinDispenserStateTableData = (CoinDispenserStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesCoinDispenserState();
            this.prop = this.GetProperties<PropertiesCoinDispenserState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.CoinDispenserStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.CoinDispenserStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.CoinDispenserStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.CoinDispenserStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.CoinDispenserStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.CoinDispenserStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.CoinDispenserStateTableData.Item2;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }
        #endregion "Constructor"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public override bool InitializeActivity(Core core)
        {
            bool ret = false;
            try
            {
                this.Core = core;
                Log.Info($"/--> Activity Name: {this.ActivityName}");
                //this.PrepareMoreTime();
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
                this.ListOfAck = new List<string>();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                //this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                if (!DispenseAvailable())
                {
                    Log.Warn("Nothing to dispense.");
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                    return;
                }
                this.AddEventHandlers();
                //this.Core.Bo.ExtraInfo.Amount = 0;
                this.CallHandler(this.prop.OnShowScreen);

                //Dispense(this.Core.AlephATMAppData.DefaultCurrency);
                Open();

                //this.StartTimer(false);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        #region "Returns from SDO"
        ///// <summary>
        ///// Maneja los retornos tipo --EVENTOS--
        ///// </summary>SendContentsToHost
        ///// <param name="func"></param>
        ///// <param name="data"></param>
        //private void HandlerEventReceive(DeviceMessage dm)
        //{
        //    if (dm.Device == Enums.Devices.BarcodeReader)
        //    {
        //        this.StopTimer();//Detengo el timer de More Time
        //        Log.Info("/--> Cim event: {0}", dm.Payload.ToString());
        //    }
        //}

        /// <summary>
        /// Guarda los RequestID de los ACK recibidos
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerAckReceive(DeviceMessage dm)
        {
            try
            {
                Log.Info("/-->ACK request ID: {0}", dm.Header.RequestId);
                this.ListOfAck.Add(dm.Header.RequestId.ToString());
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos tipo --COMPLETION-- asegurando de que se haya recibido previamente un ACK con el mismo RequestID (FIX para desechar mensajes inesperados)
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerCompletionReceive(DeviceMessage dm)
        {

            var match = this.ListOfAck.FirstOrDefault(x => x.Equals(dm.Header.RequestId.ToString()));
            Completion cr = (Completion)dm.Payload;
            if (match != null || cr.CompletionCode == CompletionCodeEnum.TimeOut)
                this.ProcessCompletion(dm);
            else
                Log.Error("/-->ACK request ID: {0} not found", dm.Header.RequestId);
        }

        /// <summary>
        /// Procesa los retornos tipo Completion de los dispositivos
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void ProcessCompletion(DeviceMessage dm)
        {
            Completion cr;
            try
            {
                Log.Info("/--> {0}", dm.Device);
                cr = (Completion)dm.Payload;
                if (dm.Device == Enums.Devices.CoinDispenser)
                {
                    switch (dm.Command)
                    {
                        case Enums.Commands.Open:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                                Dispense(this.Core.AlephATMAppData.DefaultCurrency);
                            else
                            {
                                Log.Error("Could not open CoinDispenser");
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.COIN_OPEN_ERROR);
                            }
                            break;
                        case Enums.Commands.Dispense:
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                this.ChangeDEV_Fitness(Enums.Devices.CoinDispenser, Const.Fitness.NoError, Enums.DeviceStatus.COIN_DeviceSuccess);
                                this.WriteEJ("COIN Dispense -OK-");
                            }
                            else
                            {
                                this.ChangeDEV_Fitness(Enums.Devices.CoinDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.COIN_DeviceError);//Envía el status a host de error de CIM
                                this.WriteEJ("COIN Dispense -ERROR-");
                            }
                            var data = Utilities.Utils.NewtonsoftDeserialize<StatusListCOIN>(out bool rr, cr.Data);
                            if (!rr)
                            {
                                this.ChangeDEV_Fitness(Enums.Devices.CoinDispenser, Const.Fitness.Fatal, Enums.DeviceStatus.COIN_DeviceError);//Envía el status a host de error de CIM
                                Log.Error("No se pudo deserializar objeto StatusListCOIN con DATA: {0}", cr.Data);
                            }
                            if (cr.CompletionCode == CompletionCodeEnum.Success)
                            {
                                var contentsUpdate = UpdateLocalContent(data);
                                if (contentsUpdate.success)
                                {
                                    this.NotifyDispense(contentsUpdate.details);
                                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumber);
                                }
                                else
                                {
                                    Log.Error("Failed to update local content");
                                    ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.COIN_DISPENSE_ERROR);
                                }
                            }
                            else
                            {
                                Log.Error("Failed to dispense in CoinDispenser");
                                ErrorExit(StateResult.HWERROR, ErrorData.ErrorCodes.COIN_DISPENSE_ERROR);
                            };
                            Close();
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ErrorExit(StateResult errorState, ErrorData.ErrorCodes code, string message = null)
        {
            message = message ?? "Fallo de dispensado";
            this.Core.Bo.ExtraInfo.ErrorCode = new ErrorData { Code = code, Message = message };
            this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter, false);
            var stateDic = new Dictionary<StateResult, string> {
                { StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber },
                { StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber },
                { StateResult.CANCEL, this.prop.CancelNextStateNumber },
            };
            this.SetActivityResult(errorState, stateDic[errorState]);
        }

        #endregion "Returns from SDO"

        //private void HandlerFDKreturn(string FDKcode) //Todas las FDK cancelan la operación
        //{
        //    try
        //    {
        //        //this.Core.Sdo.BAR_StopScanBarcode(); //Apago el barcode
        //        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
        //    }
        //    catch (Exception ex) { Log.Fatal(ex); }
        //}

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            ////TODO: No se activa el enter y cancel
            Log.Info("/--> Key press: {0}", othersKeys);

            switch (othersKeys)
            {
                case "REQUEST":
                    {
                        if (this.Core.Bo.ExtraInfo.AmountToDispenseInCoins > 0)
                        {
                            // Informa al usuario el monto que se dispensa.
                            this.prop.OnTakeCoins.Parameters = this.Core.Bo.ExtraInfo.AmountToDispenseInCoins;
                            this.CallHandler(this.prop.OnTakeCoins);
                        }
                        break;
                    }
                    //    case "ENTER": //Confirma TX
                    //        {
                    //            //this.SetActivityResult(0, this.prop.GoodBarcodeReadStateNumber);
                    //            break;
                    //        }
                    //    case "CANCEL":
                    //        {
                    //            this.Core.sdo.StopScanBarcode(); //Apago el barcode
                    //            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                    //            break;
                    //        }
            }
        }



        private void NotifyDispense(List<Entities.Detail> details)
        {
            Log.Debug("/--->");
            StringBuilder sb = new StringBuilder();
            try
            {
                new Thread(new ParameterizedThreadStart((object obj) =>
                {
                    Contents contents = obj as Contents;
                    try
                    {
                        var notif = this.Core.AuthorizeTransaction(Enums.TransactionType.DISPENSE, contents, this.prop.HostName);
                        if (notif.authorizationStatus != AuthorizationStatus.Authorized)
                            Log.Warn(notif.authorizationStatus);
                    }
                    catch (Exception ex) { Log.Fatal(ex); }
                }))
                    .Start(new Contents { LstDetail = details });
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }



        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                //this.StopTimer();
                this.Quit();
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Core.SetNextState(result, nextState);
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
            //this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            //this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive += new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        private void RemoveEventHandlers()
        {
            //this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            //this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive -= new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        #region "Functions"

        private bool DispenseAvailable()
        {
            return this.Core.Bo.ExtraInfo.AmountToDispenseInCoins > 0;
        }

        private void Open()
        {
            this.Core.Sdo.COIN_Open();
        }
        private void Close()
        {
            this.Core.Sdo.COIN_Close();
        }
        private void Dispense(string currency)
        {
            var denomination = new DenominationCOIN
            {
                Amount = this.Core.Bo.ExtraInfo.AmountToDispenseInCoins,
                Currency = currency,
            };
            WriteEJ($"Amount to dispense {denomination.Currency} {denomination.Amount}");
            Log.Trace("Dispense COIN execution being processed.");
            this.Core.Sdo.COIN_Dispense(denomination);
        }

        private (bool success, List<Entities.Detail> details) UpdateLocalContent(StatusListCOIN statusList)
        {
            Log.Trace("Intentando hacer un UpdateLocalContent con DATA: {0}", Utilities.Utils.JsonSerialize(statusList));
            if (statusList?.Details is null)
            {
                Log.Error("No se ha recibido la lista de Details para ejecutar UpdateContents");
                return (false, null);
            }

            var sb = new StringBuilder();
            statusList.Details?.ForEach(dt =>
            {
                sb.Append($"\nCurrency {dt.Currency}");
                dt.LstItems?.ForEach(itm =>
                {
                    sb.Append($"\n\tDenomination {itm.Denomination / 100} | Num of Items: {itm.Num_Items}");
                });
            });
            this.WriteEJ("Dispensed values: \n" + sb.ToString());

            this.Core.Counters.UpdateContents(statusList.Details, Counters.TransactionType.DISPENSE);
            WriteEJ("Counter update -OK-");
            return (true, statusList.Details);
        }

        #endregion "Returns from SDO"
    }
}
