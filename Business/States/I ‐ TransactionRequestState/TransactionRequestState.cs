using Entities;
using Entities.PaymentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Business.TransactionRequestState
{
    public class TransactionRequestState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public System.Timers.Timer timerCommunicationsResponse;
        TransactionRequestStateTableData_Type transactionRequestStateTableData; //Tabla con datos provenientes del download.
        PropertiesTransactionRequestState prop;
        private InteractiveTransactionResponse_Type Itr;
        private bool IsITR;
        private string ItrKeyBuffer;
        private bool MoreTimeSubscribed = false;
        private enum Auth_Result_Type
        {
            Id_1_DepositAndPrint,
            Id_2_DispenseAndPrint,
            Id_3_DisplayAndPrint,
            Id_4_PrintImmediate,
            Id_5_SetNextSateAndPrint,
            Id_6_NightSafeDepositAndPrint,
            Id_A_CardBeforeCash,
            Id_B_ParallelDispenseAndPrintAndCardEject,
            Id_F_CardBeforeParallelDispenseAndPrint,
            Id_P_PrintStatementAndWait,
            Id_Q_PrintStatementAndNextState,
            Id_S_ProcessDocumentWithCash,
            Id_T_DP_ATM_DepositEnvelope,
            Id_ASTERISK_RefundNotesAndNextState,
            Id_HYPHEN_EncashNotesAndNextState,
            Id_APOSTROPHE_EncashNotesAndWait,
            Id_COLON_ProcessCPMCheque,
            Id_M_PrintOnPassbookAndWait,
            Id_N_PrintOnPassbookAndSetNextState,
            Any_Other_Function_ID = 99,
            ResultOnError = -1
        }

        #region "Constructor"
        public TransactionRequestState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "TransactionRequestState";
            this.transactionRequestStateTableData = (TransactionRequestStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesTransactionRequestState(alephATMAppData);
            TransactionRequestStateTableDataExtended_Type extensionOrBufFersBCStateTableDataExtended = null;
            TransactionRequestStateTableDataExtended_Type extensionOrBufFersBCState = null;
            bool ret = false;
            try
            {
                this.prop = this.GetProperties<PropertiesTransactionRequestState>(out ret, this.prop);
                if (ret)
                {
                    if (string.IsNullOrEmpty(this.prop.NextState))
                        this.prop.NextState = this.transactionRequestStateTableData.NextState;
                    if (string.IsNullOrEmpty(this.prop.CentralResponseTimeoutNextStateNumber))
                        this.prop.CentralResponseTimeoutNextStateNumber = this.transactionRequestStateTableData.CentralResponseTimeoutNextStateNumber;
                    if (this.prop.SendTrack2Data == -1)
                        this.prop.SendTrack2Data = this.transactionRequestStateTableData.SendTrack2Data;
                    if (this.prop.TransactionType == Enums.TransactionType.NONE)
                        this.prop.TransactionType = this.transactionRequestStateTableData.TransactionType;
                    if (this.prop.SendOperationCodeData == -1)
                        this.prop.SendOperationCodeData = this.transactionRequestStateTableData.SendOperationCodeData;
                    if (this.prop.SendAmountData == -1)
                        this.prop.SendAmountData = this.transactionRequestStateTableData.SendAmountData;
                    if (this.prop.SendPINBufferADataSelectExtendedFormat == null)
                        this.prop.SendPINBufferADataSelectExtendedFormat = this.transactionRequestStateTableData.SendPINBufferADataSelectExtendedFormat;
                    //Si los datos del estado extensión del download existen intento cargarlos en la propiedad del estado.
                    if (this.transactionRequestStateTableData.Item != null)
                    {
                        if (this.transactionRequestStateTableData.Item is TransactionRequestStateTableDataExtended_Type)
                        {
                            extensionOrBufFersBCStateTableDataExtended = (TransactionRequestStateTableDataExtended_Type)this.transactionRequestStateTableData.Item;
                            if (this.prop.ExtensionOrBufFersBC is TransactionRequestStateTableDataExtended_Type)
                            {
                                extensionOrBufFersBCState = (TransactionRequestStateTableDataExtended_Type)this.prop.ExtensionOrBufFersBC;
                                if (string.IsNullOrEmpty(extensionOrBufFersBCState.StateNumber))
                                    extensionOrBufFersBCState.StateNumber = extensionOrBufFersBCStateTableDataExtended.StateNumber;
                                if (extensionOrBufFersBCState.SendGeneralPurposeBuffersBAndOrC == null)
                                    extensionOrBufFersBCState.SendGeneralPurposeBuffersBAndOrC = extensionOrBufFersBCStateTableDataExtended.SendGeneralPurposeBuffersBAndOrC;
                                if (extensionOrBufFersBCState.SendOptionalDataFieldsAH == null)
                                    extensionOrBufFersBCState.SendOptionalDataFieldsAH = extensionOrBufFersBCStateTableDataExtended.SendOptionalDataFieldsAH;
                                if (extensionOrBufFersBCState.SendOptionalDataFieldsIL == null)
                                    extensionOrBufFersBCState.SendOptionalDataFieldsIL = extensionOrBufFersBCStateTableDataExtended.SendOptionalDataFieldsIL;
                                if (extensionOrBufFersBCState.SendOptionalDataFieldsQV == null)
                                    extensionOrBufFersBCState.SendOptionalDataFieldsQV = extensionOrBufFersBCStateTableDataExtended.SendOptionalDataFieldsQV;
                                if (extensionOrBufFersBCState.SendOptionalData == null)
                                    extensionOrBufFersBCState.SendOptionalData = extensionOrBufFersBCStateTableDataExtended.SendOptionalData;
                                if (extensionOrBufFersBCState.EMVCAMProcessingFlag == null)
                                    extensionOrBufFersBCState.EMVCAMProcessingFlag = extensionOrBufFersBCStateTableDataExtended.EMVCAMProcessingFlag;
                                this.prop.ExtensionOrBufFersBC = extensionOrBufFersBCState;
                            }
                            else
                            {
                                this.prop.ExtensionOrBufFersBC = extensionOrBufFersBCStateTableDataExtended;
                            }
                        }
                        else
                        {
                            string[] SendBufferBOrC = { "000", "001", "002", "003", "004", "005", "006", "007" };
                            string result = SendBufferBOrC.FirstOrDefault(x => x.Equals(this.transactionRequestStateTableData.Item));
                            if (result != null)
                            {
                                this.prop.ExtensionOrBufFersBC = result;
                            }
                        }
                    }
                }
                else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
                this.PrintProperties(this.prop, stateTable.StateNumber);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                this.Itr = new InteractiveTransactionResponse_Type();
                Log.Info($"/--> Activity Name: {this.ActivityName}");
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override async void ActivityStart()
        {
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ(string.Format("Next State [{0}] {1}", this.Core.CurrentTransitionState, this.ActivityName));
                this.IsITR = false;
                this.ItrKeyBuffer = string.Empty;
                // this.CallHandler(this.prop.OnShowScreen);
                if (this.Core.AlephATMAppData.OperationMode == Const.OperationMode.NDC)
                {
                    this.Core.EvtRcvMsgReply += new Core.DelegateRcvMsgReply(HandlerEvtReceiveReply);
                    TransactionRequest_Type transactionRequest = this.BuildTRMessage();
                    this.SendTransactionRequest(transactionRequest);
                }
                else
                {
                    await this.AuthorizeTransaction();
                }

            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        private async Task AuthorizeTransaction()
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            try
            {
                Log.Debug("/--->");

                if(this.Core.Bo.ExtraInfo.HostExtraData != null)
                {
                    Dictionary<PropKey, object> requestData = new Dictionary<PropKey, object>();
                    foreach (var item in this.Core.Bo.ExtraInfo.HostExtraData)
                    {
                        switch (item.Key)
                        {
                            case "depositorDocType":
                                requestData.Add(PropKey.TipoDocOp, item.Value);
                                break;
                            case "depositorDocNo":
                                requestData.Add(PropKey.NroDocOp, item.Value);
                                break;
                            case "holderDocType":
                                requestData.Add(PropKey.TipoDoc, item.Value);
                                break;
                            case "holderDocNo":
                                requestData.Add(PropKey.NroDoc, item.Value);
                                break;
                            case "holderAccountNo":
                                requestData.Add(PropKey.NroCta, item.Value);
                                break;
                            default:
                                break;
                        }
                    }
                    this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
                }

                authorizationResult = this.Core.AuthorizeTransaction(this.prop.TransactionType, null, this.prop.HostName);
                this.Core.Bo.ExtraInfo.AuthorizationStatus = authorizationResult.authorizationStatus; //Guardo el resultado de la autorización
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, false);
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, false);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBD, false);
                    //save txn data from response
                    if(this.prop.SendOperationCodeData == 2) // Save response data
                    {
                        this.Core.AddHostExtraData("HostResultData", authorizationResult.Response);
                    }
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextState);
                }
                else
                {
                    // this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterDeclined, true); //Print ticket host error
                    // this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterDeclined, true);
                    // this.ProcessPrinterData(this.prop.OnSendTicketToBDError, false);
                    var response = authorizationResult.Response as ResponseBody;
                    if(response != null)
                    {
                        this.Core.Bo.ExtraInfo.PendingUserNotification = response.Message + "<br/><small>" + response.ErrorCode + "</small>";
                    }
                    else
                    {
                        this.Core.Bo.ExtraInfo.PendingUserNotification = "Servicio no disponible";
                    }
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove(PSConst.PAYMENT_SERVICE_DATA);
                    // borrar datos ingresados en setextradata
                    // TODO: deteminar que se borra y que no, segun branding bancarios y tipo de error.
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("depositorDocType");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("depositorDocNo");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("holderDocType");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("holderDocNo");
                    this.Core.Bo.ExtraInfo.HostExtraData.Remove("holderAccountNo");
                    this.Core.Bo.ExtraInfo.ExtraData = null;
                    this.SetActivityResult(StateResult.DECLINED, this.prop.CentralResponseTimeoutNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "NDC"

        /// <summary>
        /// Recibe la contestación al Transaction Request desde el DH.
        /// </summary>
        /// <param name="param"></param>
        private void HandlerEvtReceiveReply(object param)
        {
            try
            {
                this.StopTimerCommunicationsResponse();
                if (param is InteractiveTransactionResponse_Type)
                {
                    this.IsITR = true;
                    InteractiveTransactionResponse_Type iTR = param as InteractiveTransactionResponse_Type;
                    this.Itr = iTR;
                    this.PrepareMoreTime();
                    this.StartInteractiveTransaction();
                }
                else if (param is TransactionReplyCommand_Type)
                {
                    this.IsITR = false;
                    TransactionReplyCommand_Type tR = param as TransactionReplyCommand_Type;
                    this.FunctionToPerform(tR);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Transaction Reply"
        private void FunctionToPerform(TransactionReplyCommand_Type tR)
        {
            if (this.Core.Bo.ExtraInfo.ndcData == null)
                this.Core.Bo.ExtraInfo.ndcData = new NDCData();
            if (tR.ScreenDisplay.ScreenUpdates.ScreenUpdate.Item != null)
            {
                if (this.Core.ShowGeneralNDCScreen(tR.ScreenDisplay.ScreenNumberToDisplay.Item, null))
                {
                    switch (tR.FunctionId)
                    {
                        case FunctionId_Type.SetNextStateAndPrint:
                            {
                                //this.BuildJournalPrintData(tR);
                                PrinterFlag_Type[] printerTypes = new PrinterFlag_Type[]
                                {
                                    PrinterFlag_Type.ReceiptPrinterOnly,
                                    PrinterFlag_Type.JournalAndReceiptPrinter
                                };
                                this.Core.Bo.ExtraInfo.ndcData.ReceiptData = this.GetPrintInstructions(tR, printerTypes);
                                this.Core.Bo.ExtraInfo.ReceiptRequired = true;
                                this.PrintReceiptData();
                                this.SetActivityResult(StateResult.SUCCESS, tR.NextStateId);
                                break;
                            }
                        default:
                            {
                                Log.Error("Unknown function ID: {0}", tR.FunctionId);
                                this.Core.SendEspecificCommandRejectStatus("A06");
                                this.SetActivityResult(StateResult.SWERROR, "ZZZ");
                                break;
                            }
                    }
                }
                else
                    this.SetActivityResult(StateResult.SWERROR, "ZZZ");
            }
            else
                Log.Warn("Without screen update data");
        }

        private void BuildJournalPrintData(TransactionReplyCommand_Type tR)
        {
            PrinterFlag_Type[] printerTypes = new PrinterFlag_Type[]
            {
                PrinterFlag_Type.JournalPrinterOnly,
                PrinterFlag_Type.JournalAndReceiptPrinter,
                PrinterFlag_Type.PPDAndJournalPrinterIfFunctionIDIs1Or7_DPATMAndJournalPrinterIfFunctionIDIsT
            };
            this.Core.Bo.ExtraInfo.ndcData.JournalData = null;
            try
            {
                this.Core.Bo.ExtraInfo.ndcData.JournalData = this.GetPrintInstructions(tR, printerTypes);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void PrintReceiptData()
        {
            PrintConverter printers = new PrintConverter();
            string printData = string.Empty;
            try
            {
                if (this.Core.Bo.ExtraInfo.ndcData.ReceiptData != null && this.Core.Bo.ExtraInfo.ReceiptRequired)
                {
                    if (this.Core.Download.simulatedPrePrintedReceiptScreenR00 != null)
                        printData = printers.GetCommands(this.Core.Download.simulatedPrePrintedReceiptScreenR00.SimulatedPrePrintedReceiptData, false, false);
                    if (this.Core.Download.simulatedPrePrintedReceiptScreenR01 != null)
                        printData += printers.GetCommands(this.Core.Download.simulatedPrePrintedReceiptScreenR01.SimulatedPrePrintedReceiptData, false, false);
                    printData += printers.GetCommandsStr(this.Core.Bo.ExtraInfo.ndcData.ReceiptData, false);
                    //Envío a imprimir el ticket
                    this.Core.Sdo.PTR_PrintRawData(printData);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private PrintingInstruction_Type[] GetPrintInstructions(TransactionReplyCommand_Type transactionReply, PrinterFlag_Type[] printerTypes)
        {
            PrintingInstruction_Type[] array = new PrintingInstruction_Type[0];
            int num = 0;
            if (transactionReply.PrintingInstruction1 != null)
            {
                //NDCPrintInstruction.Trace("PrintInstruction1:" + transactionReply.PrintingInstruction1.Printer.ToString());
                if (this.VerifyPrintInstructionType(printerTypes, transactionReply.PrintingInstruction1.Printer))
                {
                    PrintingInstruction_Type printingInstruction_Type = new PrintingInstruction_Type();
                    printingInstruction_Type.Printer = transactionReply.PrintingInstruction1.Printer;
                    printingInstruction_Type.PrinterData = transactionReply.PrintingInstruction1.PrinterData;
                    array = (PrintingInstruction_Type[])this.Redim(array, num + 1);
                    array[num++] = printingInstruction_Type;
                }
            }
            if (transactionReply.PrintingInstruction2 != null)
            {
                //NDCPrintInstruction.Trace("PrintInstruction2:" + transactionReply.PrintingInstruction2.Printer.ToString());
                if (this.VerifyPrintInstructionType(printerTypes, transactionReply.PrintingInstruction2.Printer))
                {
                    PrintingInstruction_Type printingInstruction_Type2 = new PrintingInstruction_Type();
                    printingInstruction_Type2.Printer = transactionReply.PrintingInstruction2.Printer;
                    printingInstruction_Type2.PrinterData = transactionReply.PrintingInstruction2.PrinterData;
                    array = (PrintingInstruction_Type[])this.Redim(array, num + 1);
                    array[num++] = printingInstruction_Type2;
                }
            }
            if (transactionReply.PrintingInstruction3 != null)
            {
                //NDCPrintInstruction.Trace("PrintInstruction3:" + transactionReply.PrintingInstruction3.Printer.ToString());
                if (this.VerifyPrintInstructionType(printerTypes, transactionReply.PrintingInstruction3.Printer))
                {
                    PrintingInstruction_Type printingInstruction_Type3 = new PrintingInstruction_Type();
                    printingInstruction_Type3.Printer = transactionReply.PrintingInstruction3.Printer;
                    printingInstruction_Type3.PrinterData = transactionReply.PrintingInstruction3.PrinterData;
                    array = (PrintingInstruction_Type[])this.Redim(array, num + 1);
                    array[num++] = printingInstruction_Type3;
                }
            }
            if (transactionReply.AdditionalPrintingInstruction != null)
            {
                for (int i = 0; i < transactionReply.AdditionalPrintingInstruction.Length; i++)
                {
                    //NDCPrintInstruction.Trace("AdditionalPrintingInstruction:" + transactionReply.AdditionalPrintingInstruction[i].Printer.ToString());
                    if (this.VerifyPrintInstructionType(printerTypes, transactionReply.AdditionalPrintingInstruction[i].Printer))
                    {
                        PrintingInstruction_Type printingInstruction_Type4 = new PrintingInstruction_Type();
                        printingInstruction_Type4.Printer = transactionReply.AdditionalPrintingInstruction[i].Printer;
                        printingInstruction_Type4.PrinterData = transactionReply.AdditionalPrintingInstruction[i].PrinterData;
                        array = (PrintingInstruction_Type[])this.Redim(array, num + 1);
                        array[num] = printingInstruction_Type4;
                    }
                }
            }
            if (num > 0)
            {
                return array;
            }
            return null;
        }

        private bool VerifyPrintInstructionType(PrinterFlag_Type[] printerTypes, PrinterFlag_Type instructionPrintType)
        {
            try
            {
                for (int i = 0; i < printerTypes.Length; i++)
                {
                    PrinterFlag_Type printerFlag_Type = printerTypes[i];
                    if (printerFlag_Type == instructionPrintType)
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private Array Redim(Array oldArray, int newSize)
        {
            Type elementType = oldArray.GetType().GetElementType();
            Array array = Array.CreateInstance(elementType, newSize);
            Array.Copy(oldArray, 0, array, 0, Math.Min(oldArray.Length, newSize));
            return array;
        }
        #endregion "Transaction Reply"

        #region "ITR"
        private void StartInteractiveTransaction()
        {
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
            EntryModeAndBufferConfiguration_Type entryModeAndBufferConfiguration = new EntryModeAndBufferConfiguration_Type();
            entryModeAndBufferConfiguration.DisplayAndBufferParameters = new DisplayAndBufferParameters_Type();
            entryModeAndBufferConfiguration.DisplayAndBufferParameters = DisplayAndBufferParameters_Type.ITR;
            this.ItrKeyBuffer = string.Empty;
            KeyMask_Type keyMask = this.GetKeyMask(this.Itr.Activations);
            if (this.Itr.Activations.NumericKeysSpecified && this.Itr.Activations.NumericKeys == Activation_Type.Activate)//Activa teclado numerico
            {
                if (this.Core.ScreenConfiguration.KeyboardEntryMode != KeyboardEntryMode_Type.none)
                    keyMask = new KeyMask_Type();//Si se activa el teclado en pantalla desactivo las FDKs
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                if (this.Core.ShowInformationEntryNDCScreen("ITR", keyMask, entryModeAndBufferConfiguration))
                    this.StartTimer();
                else
                    this.SetActivityResult(StateResult.SWERROR, "ZZZ");
            }
            else
            {
                if (this.Core.ShowGeneralNDCScreen("ITR", keyMask))
                    this.StartTimer();
                else
                    this.SetActivityResult(StateResult.SWERROR, "ZZZ");
            }
        }

        private void HandlerInputData(string keyCode, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", keyCode);
                this.ItrKeyBuffer = keyCode;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandleFDKreturn(string FDKcode)
        {
            TransactionRequest_Type transactionRequest = null;
            try
            {
                Log.Info("-> FDK data: {0}", FDKcode);
                this.ItrKeyBuffer = string.Format("{0}{1}", this.ItrKeyBuffer, this.Core.Bo.LastFDKPressed);
                transactionRequest = this.BuildTRMessageForITR(this.ItrKeyBuffer);
                this.SendTransactionRequest(transactionRequest);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            TransactionRequest_Type transactionRequest = null;
            Log.Info("/--> Key prees: {0}", othersKeys);
            if (this.IsITR)
            {
                switch (othersKeys)
                {
                    case "ENTER": //Confirma TX
                        {
                            if (this.Itr.Activations.FDKAtouchAreaSpecified)
                            {
                                this.Core.Bo.LastFDKPressed = "A";
                                this.ItrKeyBuffer = string.Format("{0}{1}", this.ItrKeyBuffer, this.Core.Bo.LastFDKPressed);
                                transactionRequest = this.BuildTRMessageForITR(this.ItrKeyBuffer);
                                this.SendTransactionRequest(transactionRequest);
                            }
                            else
                                Log.Warn(string.Format("-> FDKA disable"));
                            break;
                        }
                    case "CANCEL":
                        {
                            transactionRequest = this.BuildTRMessageForITR("E");
                            this.SendTransactionRequest(transactionRequest);
                            break;
                        }
                }
            }
        }

        private TransactionRequest_Type BuildTRMessageForITR(string generalPurposeBufferB)
        {
            TransactionRequest_Type transactionRequest = new TransactionRequest_Type();
            try
            {
                Log.Debug("/--->");
                transactionRequest.LogicalUnitNumber = this.Core.TerminalInfo.LogicalUnitNumber;
                if (this.SecurityFlag(2))
                {
                    this.Core.Bo.ExtraInfo.ndcData.TimeVariant = this.GetTimeVariant();
                    transactionRequest.TimeVariant = this.Core.Bo.ExtraInfo.ndcData.TimeVariant;
                }
                else
                {
                    this.Core.Bo.ExtraInfo.ndcData.TimeVariant = "";
                }
                if (this.prop.TopOfReceiptTransactionFlag)
                {
                    transactionRequest.TopOfReceipt = TopOfReceipt_Type.WithPrinting;
                }
                else
                {
                    transactionRequest.TopOfReceipt = TopOfReceipt_Type.WithoutPrinting;
                }
                this.Core.Bo.ExtraInfo.ndcData.CoOrdination = this.GetNextMessageCoordinationNumber();
                transactionRequest.MessageCoordinatorNumber = this.Core.Bo.ExtraInfo.ndcData.CoOrdination;
                transactionRequest.GeneralPurposeBufferB = generalPurposeBufferB;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return transactionRequest;
        }

        private KeyMask_Type GetKeyMask(InteractiveTransactionResponseActivations_Type activations)
        {
            KeyMask_Type keyMask = new KeyMask_Type();
            if (activations.FDKAtouchAreaSpecified)
                keyMask.FDKA = activations.FDKAtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKBtouchAreaSpecified)
                keyMask.FDKB = activations.FDKBtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKCtouchAreaSpecified)
                keyMask.FDKC = activations.FDKCtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKDtouchAreaSpecified)
                keyMask.FDKD = activations.FDKDtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKFtouchAreaSpecified)
                keyMask.FDKF = activations.FDKFtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKGtouchAreaSpecified)
                keyMask.FDKG = activations.FDKGtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKHtouchAreaSpecified)
                keyMask.FDKH = activations.FDKHtouchArea == Activation_Type.Activate ? true : false;
            if (activations.FDKItouchAreaSpecified)
                keyMask.FDKI = activations.FDKItouchArea == Activation_Type.Activate ? true : false;
            return keyMask;
        }
        #endregion "ITR"

        #region "General Functions"
        private void SendTransactionRequest(TransactionRequest_Type tr)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                Log.Debug("/--->");
                if (this.IsITR)
                {
                    this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                    this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                    this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                    this.StopTimer();
                }
                if (tr != null)
                {
                    sb.Append("11").Append(Entities.Const.FS);
                    sb.Append(tr.LogicalUnitNumber).Append(Entities.Const.FS).Append(Entities.Const.FS);
                    sb.Append(tr.TimeVariant).Append(Entities.Const.FS);
                    sb.Append(string.Format("{0}", (int)tr.TopOfReceipt));
                    sb.Append(tr.MessageCoordinatorNumber).Append(Entities.Const.FS);
                    sb.Append(tr.Track2Data).Append(Entities.Const.FS);
                    sb.Append(tr.Track3Data).Append(Entities.Const.FS);
                    sb.Append(tr.OperationCodeData).Append(Entities.Const.FS);
                    sb.Append(tr.AmountEntry).Append(Entities.Const.FS);
                    sb.Append(tr.PinBufferA).Append(Entities.Const.FS);
                    sb.Append(tr.GeneralPurposeBufferB).Append(Entities.Const.FS);
                    sb.Append(tr.GeneralPurposeBufferC).Append(Entities.Const.FS);
                    sb.Append(tr.BarcodeData.ScannedBarcodeData);
                    if (this.Core.NDChost != null)
                    {
                        if (this.Core.NDChost.SendDataToDH(sb.ToString()))
                        {
                            this.StartTimerCommunicationsResponse();
                        }
                        else
                        {
                            this.SetActivityResult(StateResult.TIMEOUT, this.prop.CentralResponseTimeoutNextStateNumber);
                            Log.Error(string.Format("Communication error."));
                        }
                    }
                    else
                    {
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.CentralResponseTimeoutNextStateNumber);
                        Log.Error(string.Format("NDChost object is null."));
                    }
                }
                else
                {
                    Log.Error(string.Format("TransactionRequest is null."));
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimerCommunicationsResponse()
        {
            Timer_Type timerRet;
            if (this.timerCommunicationsResponse == null)
                this.timerCommunicationsResponse = new System.Timers.Timer();
            this.timerCommunicationsResponse.AutoReset = false;
            if (this.Core.GetEnhParameterTimer(ItemChoiceTimer_Type.CommunicationsResponseTimeout, out timerRet) == 0)
            {
                this.timerCommunicationsResponse.Interval = timerRet.Item * 800;
                this.timerCommunicationsResponse.Elapsed += new System.Timers.ElapsedEventHandler(timerComunicationsResponse_Elapsed);
                this.timerCommunicationsResponse.Enabled = true;
                this.timerCommunicationsResponse.Start();
            }
            else
                Log.Warn(string.Format("->Can´t get enh parameter timer: {0}", ItemChoiceTimer_Type.CommunicationsResponseTimeout));
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimerCommunicationsResponse()
        {
            if (timerCommunicationsResponse != null)
            {
                this.timerCommunicationsResponse.Elapsed -= new System.Timers.ElapsedEventHandler(timerComunicationsResponse_Elapsed);
                this.timerCommunicationsResponse.Enabled = false;
                this.timerCommunicationsResponse.Stop();
            }
        }

        /// <summary>
        /// Time out de espera de respuesta de DH
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void timerComunicationsResponse_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Error(string.Format("Communication time out."));
            this.StopTimerCommunicationsResponse();
            this.SetActivityResult(StateResult.TIMEOUT, this.prop.CentralResponseTimeoutNextStateNumber);
        }

        private TransactionRequest_Type BuildTRMessage()
        {
            TransactionRequest_Type transactionRequest = new TransactionRequest_Type();
            TransactionRequestStateTableDataExtended_Type extOrBufFersBCState = null;
            try
            {
                Log.Debug("/--->");
                if (this.prop.ExtensionOrBufFersBC is TransactionRequestStateTableDataExtended_Type)
                    extOrBufFersBCState = (TransactionRequestStateTableDataExtended_Type)this.prop.ExtensionOrBufFersBC;
                transactionRequest.LogicalUnitNumber = this.Core.TerminalInfo.LogicalUnitNumber;
                if (this.SecurityFlag(2))
                {
                    this.Core.Bo.ExtraInfo.ndcData.TimeVariant = this.GetTimeVariant();
                    transactionRequest.TimeVariant = this.Core.Bo.ExtraInfo.ndcData.TimeVariant;
                }
                else
                {
                    this.Core.Bo.ExtraInfo.ndcData.TimeVariant = "";
                }
                if (this.prop.TopOfReceiptTransactionFlag)
                {
                    transactionRequest.TopOfReceipt = TopOfReceipt_Type.WithPrinting;
                }
                else
                {
                    transactionRequest.TopOfReceipt = TopOfReceipt_Type.WithoutPrinting;
                }
                this.Core.Bo.ExtraInfo.ndcData.CoOrdination = this.GetNextMessageCoordinationNumber();
                transactionRequest.MessageCoordinatorNumber = this.Core.Bo.ExtraInfo.ndcData.CoOrdination;
                if (this.prop.SendTrack2Data == 1)
                {
                    transactionRequest.Track2Data = ";" + this.Core.Bo.ExtraInfo.NewTrack2 + "?";
                }
                //if (this.prop.SendTrack1Or3Data.SendTrack3Data)
                //{
                //    transactionRequest.Track3Data = ";" + this.Core.Bo.ExtraInfo.NewTrack3 + "?";
                //}
                if (this.prop.SendOperationCodeData == 1)
                {
                    transactionRequest.OperationCodeData = this.Core.Bo.ExtraInfo.OperationCodeData.Replace("0", " ");
                }
                Log.Info("BuildTransactionRequest.OperationCodeData=(" + this.Core.Bo.ExtraInfo.OperationCodeData + ")");

                if (this.prop.SendAmountData == 1)
                {
                    decimal amount = this.Core.Bo.ExtraInfo.Amount;
                    int totalWitdth = 8;
                    if (this.prop.TwelveDigitsAmountBufferLength)
                    {
                        totalWitdth = 12;
                    }
                    transactionRequest.AmountEntry = amount.ToString().PadLeft(totalWitdth, '0');
                }
                if (this.prop.SendPINBufferADataSelectExtendedFormat.SendPINBufferA)
                {
                    transactionRequest.PinBufferA = this.FormatPinBlock();
                }
                if (this.SendBuffer('B'))
                {
                    transactionRequest.GeneralPurposeBufferB = this.Core.Bo.ExtraInfo.BufferB;
                }
                if (this.SendBuffer('C'))
                {
                    transactionRequest.GeneralPurposeBufferC = this.Core.Bo.ExtraInfo.BufferC;
                }
                //if (this.prop.SendTrack1Or3Data.SendTrack1Data)
                //{
                //    transactionRequest.Track1Data = new Track1Data_Type
                //    {
                //        Track1 = "%" + this.Core.Bo.ExtraInfo.NewTrack1 + "?"
                //    };
                //}
                if (extOrBufFersBCState != null)
                    if (extOrBufFersBCState.SendOptionalData.SendFielde)
                    {
                        transactionRequest.BarcodeData = new BarcodeData_Type
                        {
                            ScannedBarcodeData = "e" + this.Core.Bo.ExtraInfo.Barcode
                        };
                    }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return transactionRequest;
        }

        private bool SendBuffer(char bufferIndicator)
        {
            bool flag;
            bool flag2;
            if (bufferIndicator == 'C')
            {
                flag = this.prop.SendGeneralPurposeBufferC;
                flag2 = this.prop.ExtensionSendGeneralPurposeBufferC;
            }
            else
            {
                flag = this.prop.SendGeneralPurposeBufferB;
                flag2 = this.prop.ExtensionSendGeneralPurposeBufferB;
            }
            return flag || flag2;
        }

        private string FormatPinBlock()
        {
            StringBuilder stringBuilder = new StringBuilder(""); //TODO: TpaActivity.bo.Visit.Customer.PINBlock
            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (stringBuilder[i] > '9')
                {
                    StringBuilder stringBuilder2;
                    //int index;
                    //(stringBuilder2 = stringBuilder)[index = i] = stringBuilder2[index] - '\a';
                }
            }
            return stringBuilder.ToString();
        }

        private string GetNextMessageCoordinationNumber()
        {
            Option_Type optionRet;
            string hexValue = "1";
            int i = 31;
            try
            {
                var random = new System.Random();
                i = random.Next(49, 63);
                if (this.Core.GetEnhParameterOption(ItemChoiceOption_Type.MCNRange, out optionRet) == 0)
                {
                    if (!optionRet.Item.ToString().Equals("000"))
                    {
                        int.TryParse(optionRet.Item.ToString(), out i);
                    }
                }
                else
                    Log.Warn(string.Format("->Can´t get enh parameter option: {0}", ItemChoiceOption_Type.MCNRange));
                hexValue = i.ToString("X");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return Utilities.Utils.HexToStr(hexValue);
        }

        private bool SecurityFlag(int flagNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(this.Core.TerminalInfo.MacFlags))
                {
                    bool result = false;
                    return result;
                }
                string macFlags = this.Core.TerminalInfo.MacFlags;
                if (flagNumber > 0 && macFlags.Length >= flagNumber - 1 && macFlags.Substring(flagNumber - 1, 1) == "1")
                {
                    bool result = true;
                    return result;
                }
            }
            catch
            {
            }
            return false;
        }

        private string GetTimeVariant()
        {
            DateTime now = DateTime.Now;
            return ((long)(now.DayOfYear * 100000) + (long)now.TimeOfDay.TotalSeconds).ToString("X8");
        }
        #endregion

        #endregion "NDC"

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
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
                if (this.Core.AlephATMAppData.OperationMode == Const.OperationMode.NDC)
                    this.Core.EvtRcvMsgReply -= new Core.DelegateRcvMsgReply(HandlerEvtReceiveReply);
                if (this.IsITR)
                    this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "More time"
        // More time.
        public MoreTime moreTime;

        // Timeout
        public System.Timers.Timer timerScreen;

        // Indicates if time-out occurs
        public bool timeout = false;

        /// <summary>
        /// Instantiates the MoreTime class. Enables MoreTime beep if 
        /// MoreTimeBeepEnabled property of activity is enabled.
        /// </summary>
        private void PrepareMoreTime()
        {
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, false, this.ActivityName);
            this.moreTime.EvtMoreTime += new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
        }

        private void AnalyzeMoreTimeResult(MoreTimeResult result)
        {
            TransactionRequest_Type transactionRequest = null;
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.StartInteractiveTransaction();
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        transactionRequest = this.BuildTRMessageForITR("E");
                        this.SendTransactionRequest(transactionRequest);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        transactionRequest = this.BuildTRMessageForITR("T");
                        this.SendTransactionRequest(transactionRequest);
                        break;
                    }
            }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer()
        {
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            timeout = false;
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            if (timerScreen != null)
            {
                this.SubscribeMoreTime(false);
                this.timerScreen.Enabled = false;
                this.timerScreen.Stop();
            }
        }

        private void SubscribeMoreTime(bool enabled)
        {
            if (!enabled) this.timerScreen.Elapsed -= new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);
            else if (!this.MoreTimeSubscribed) this.timerScreen.Elapsed += new System.Timers.ElapsedEventHandler(TimerScreen_Elapsed);

            this.MoreTimeSubscribed = enabled;
        }

        /// <summary>
        /// It controls timeout for data entry. 
        /// </summary>
        /// <param name="sender">Who fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            this.StopTimer();
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandleFDKreturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"
    }
}
