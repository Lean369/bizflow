using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utilities;

namespace Business.CashAcceptState
{
    public class CashAcceptState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private bool FlagReset = true; //Bandera para que se realice solo un reset por sesión de depósitos
        private bool FlagIsCancel = false; //Indica que ocurrió una cancelación
        private bool CashInError = false; //Detecta los cashIn error para luego de un TimeOut poder devolver los billetes y desactivar el auto-deposit
        private bool CashInEndStatusAvailable = false; //Indica que se verifica el status de CIM luego del CashInEnd
        private bool RollbackAvailable = false;
        private bool RejectBinEmpty = true;
        private bool EscrowShutterOpen = false;
        private bool CancelAvailable = false;
        private bool CancelButtonAvailable = true;
        private bool NotesInEscrowDetected = false;
        private bool PrinterNotAvailable = false;
        private bool ItemsInserted = false; //Indica que se insertaron billetes al equipo. Al mostrar la pantalla se libera.
        private bool CashInEndInProgress = false;
        private bool CashInEndBufferNull = false; //Indica que el buffer del CashInEnd vino en null
        private bool DepositHardwareError = false;
        private bool ForceOpenShutterAtReject = false;
        private string NextState;
        private Enums.Commands Phase = Enums.Commands.UNK;
        CashAcceptStateTableData_Type cashAcceptStateTableData; //Tabla con datos provenientes del download.
        PropertiesCashAcceptState prop;
        private bool CashInAvailable = true;
        private bool InputPositionNotEmpty = false; //True: si hay billetes presentes en la bandeja de entrada
        private bool CompleteCashInStart = false;
        private List<string> ListOfAck = new List<string>();
        private bool MoreTimeSubscribed = false;
        private bool AuthInProgress = false; //Indica que la autorización esta en progreso
        private bool IsDeclined = false;//Indica que la transacción fue declinada por host

        #region "Constructor"
        public CashAcceptState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            bool ret = false;
            this.ActivityName = "CashAcceptState";
            this.cashAcceptStateTableData = (CashAcceptStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesCashAcceptState(alephATMAppData);
            CashAcceptStateTableDataExtension1_Type extensionTable1 = null;
            CashAcceptStateTableDataExtension2_Type extensionTable2 = null;
            CashAcceptStateTableDataExtension3_Type extensionTable3 = null;
            this.prop = this.GetProperties<PropertiesCashAcceptState>(out ret, this.prop);
            if (ret)
            {
                if (this.cashAcceptStateTableData.Item != null)
                    extensionTable1 = (CashAcceptStateTableDataExtension1_Type)this.cashAcceptStateTableData.Item;
                if (this.cashAcceptStateTableData.Item1 != null)
                    extensionTable2 = (CashAcceptStateTableDataExtension2_Type)this.cashAcceptStateTableData.Item1;
                if (this.cashAcceptStateTableData.Item2 != null)
                    extensionTable3 = (CashAcceptStateTableDataExtension3_Type)this.cashAcceptStateTableData.Item2;
                if (string.IsNullOrEmpty(this.prop.CancelKeyMask))
                    this.prop.CancelKeyMask = this.cashAcceptStateTableData.CancelKeyMask;
                if (string.IsNullOrEmpty(this.prop.DepositKeyMask))
                    this.prop.DepositKeyMask = this.cashAcceptStateTableData.DepositKeyMask;
                if (string.IsNullOrEmpty(this.prop.AddMoreKeyMask))
                    this.prop.AddMoreKeyMask = this.cashAcceptStateTableData.AddMoreKeyMask;
                if (string.IsNullOrEmpty(this.prop.RefundKeyMask))
                    this.prop.RefundKeyMask = this.cashAcceptStateTableData.RefundKeyMask;
                if (string.IsNullOrEmpty(this.prop.Extension1.PleaseEnterNotesScreen) && extensionTable1 != null)
                    this.prop.Extension1.PleaseEnterNotesScreen = extensionTable1.PleaseEnterNotesScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.PleaseRemoveNotesScreen) && extensionTable1 != null)
                    this.prop.Extension1.PleaseRemoveNotesScreen = extensionTable1.PleaseRemoveNotesScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.ConfirmationScreen) && extensionTable1 != null)
                    this.prop.Extension1.ConfirmationScreen = extensionTable1.ConfirmationScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.HardwareErrorScreen) && extensionTable1 != null)
                    this.prop.Extension1.HardwareErrorScreen = extensionTable1.HardwareErrorScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.EscrowFullScreen) && extensionTable1 != null)
                    this.prop.Extension1.EscrowFullScreen = extensionTable1.EscrowFullScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.ProcessingNotesScreen) && extensionTable1 != null)
                    this.prop.Extension1.ProcessingNotesScreen = extensionTable1.ProcessingNotesScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.PleaseRemoveMoreThan90NotesScreen) && extensionTable1 != null)
                    this.prop.Extension1.PleaseRemoveMoreThan90NotesScreen = extensionTable1.PleaseRemoveMoreThan90NotesScreen;
                if (string.IsNullOrEmpty(this.prop.Extension1.PleaseWaitScreen) && extensionTable1 != null)
                    this.prop.Extension1.PleaseWaitScreen = extensionTable1.PleaseWaitScreen;
                if (string.IsNullOrEmpty(this.prop.Extension2.GoodNextStateNumber) && extensionTable2 != null)
                    this.prop.Extension2.GoodNextStateNumber = extensionTable2.GoodNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.CancelNextStateNumber) && extensionTable2 != null)
                    this.prop.Extension2.CancelNextStateNumber = extensionTable2.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.DeviceErrorNextStateNumber) && extensionTable2 != null)
                    this.prop.Extension2.DeviceErrorNextStateNumber = extensionTable2.DeviceErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.TimeOutNextStateNumber) && extensionTable2 != null)
                    this.prop.Extension2.TimeOutNextStateNumber = extensionTable2.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.DeclinedNextStateNumber) && extensionTable2 != null)
                    this.prop.Extension2.DeclinedNextStateNumber = extensionTable2.DeclinedNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension2.OperationMode) && extensionTable2 != null)
                    this.prop.Extension2.OperationMode = extensionTable2.OperationMode;
                if (string.IsNullOrEmpty(this.prop.Extension2.AutoDeposit) && extensionTable2 != null)
                    this.prop.Extension2.AutoDeposit = extensionTable2.AutoDeposit;
                if (string.IsNullOrEmpty(this.prop.Extension2.RetractingNotesScreen) && extensionTable2 != null)
                    this.prop.Extension2.RetractingNotesScreen = extensionTable2.RetractingNotesScreen;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations112) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations112 = extensionTable3.SetDenominations112;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations1324) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations1324 = extensionTable3.SetDenominations1324;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations2536) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations2536 = extensionTable3.SetDenominations2536;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations3748) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations3748 = extensionTable3.SetDenominations3748;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations4960) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations4960 = extensionTable3.SetDenominations4960;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations6172) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations6172 = extensionTable3.SetDenominations6172;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations7384) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations7384 = extensionTable3.SetDenominations7384;
                if (string.IsNullOrEmpty(this.prop.Extension3.SetDenominations8596) && extensionTable3 != null)
                    this.prop.Extension3.SetDenominations8596 = extensionTable3.SetDenominations8596;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.EnablePrintProperties = this.prop.PrintPropertiesEnable;
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
                this.CurrentState = ProcessState.INPROGRESS;
                this.GetPerformaceData();
                this.ListOfAck = new List<string>();
                this.RejectBinEmpty = true;
                this.FlagReset = true;
                this.FlagIsCancel = false;
                this.CashInError = false;
                this.RollbackAvailable = true;
                this.CancelAvailable = true;
                this.CancelButtonAvailable = true;
                this.EscrowShutterOpen = false;
                this.NotesInEscrowDetected = false;
                this.CompleteCashInStart = false;
                this.CashInEndStatusAvailable = false;
                this.ItemsInserted = false;
                this.CashInEndInProgress = false;
                this.CashInEndBufferNull = false;
                this.AuthInProgress = false;
                this.IsDeclined = false;
                this.DepositHardwareError = false;
                this.ForceOpenShutterAtReject = false;
                this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositEscrowFull = false;
                this.AddEventHandlers();
                this.Phase = Enums.Commands.UNK;
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();//Borro los datos parciales
                this.PrinterNotAvailable = false;
                this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull = false;
                if (string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.Currency))
                    this.Core.Bo.ExtraInfo.Currency = this.prop.Currency;
                if (!this.Core.Sdo.StateConnection)
                {
                    Log.Error("AlephDEV disconnected");
                    this.SetActivityResult2(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                }
                else
                {
                    this.ShowInsertNotesScreen();//Muestro la pantalla principal
                    if (this.prop.VerifyLogicalFullBin && this.Core.Counters.LogicalFullBin)
                    {
                        Log.Warn("Logical: Cassette Full");
                        this.WriteEJ("Logical Full Bin");
                        this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFullAdvice);
                    }
                    else
                    {
                        if (this.prop.VerifyPrinter)
                        {
                            this.Core.Sdo.PTR_GetState();
                        }
                        else
                        {
                            Log.Warn("VERIFY PRINTER: DISABLED");
                            if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                            {
                                this.Core.Sdo.IOBoard_GetState();
                            }
                            else
                            {
                                this.Open_CIM();
                                Log.Warn("VERIFY SENSORS: DISABLED");
                                this.WriteEJ("VERIFY SENSORS: DISABLED");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Returns from SDO"
        /// <summary>
        /// Maneja los retornos tipo --EVENTOS--
        /// </summary>SendContentsToHost
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void HandlerEventReceive(DeviceMessage dm)
        {
            if (dm.Device == Enums.Devices.CashAcceptor)
            {
                this.StopTimer();//Detengo el timer de More Time
                Log.Info($"/--> Cim event: {dm.Payload.ToString()}");
                switch (dm.Payload.ToString())
                {
                    case "WFS_SRVE_CIM_ITEMSTAKEN_WFS_CIM_POSNULL": //Informa retiro de billetes de reject para GLORY
                        if (this.Phase == Enums.Commands.CashIn && !this.RejectBinEmpty)
                        {
                            this.WriteEJ("Rejected items taken -OK-");
                            this.RejectBinEmpty = true;
                            this.Core.HideScreenModals(); //Quito los avisos de pantalla
                            this.ShowConfirmationScreen(true);
                        }
                        else if (this.Phase == Enums.Commands.Status && !this.RejectBinEmpty)
                        {
                            this.RejectBinEmpty = true;
                            this.Core.Sdo.CIM_Status();
                        }
                        else if (this.Phase == Enums.Commands.RollBack)
                            this.HandleItemsTakenOnRollback();
                        else
                            this.StartTimer(true);//Activo more time - Fix Glory
                        break;
                    case "WFS_SRVE_CIM_ITEMSTAKEN_WFS_CIM_POSOUTCENTER": //Informa retiro de billetes de reject para SNBC y TCR
                        this.RejectBinEmpty = true;
                        if (this.Phase == Enums.Commands.CashIn)
                        {
                            this.WriteEJ("Rejected items taken -OK-");
                            this.Core.HideScreenModals(); //Quito los avisos de pantalla
                            this.ShowConfirmationScreen(true);
                        }
                        this.HandleItemsTakenOnRollback(); //TODO: verificar si esto es necesario
                        break;
                    case "WFS_SRVE_CIM_ITEMSTAKEN_WFS_CIM_POSOUTLEFT": //Informa retiro de billetes de escrow
                        this.HandleItemsTakenOnRollback();
                        break;
                    case "WFS_SRVE_CIM_ITEMSTAKEN_WFS_CIM_POSINCENTER": //Informa retiro de billetes de bandeja de entrada
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        break;
                    case "WFS_SRVE_CIM_ITEMSINSERTED": //Informa billetes insertados en la bandeja de entrada
                        this.ItemsInserted = true;
                        if (this.prop.AutoCashIn)//MEI
                        {
                            this.CallHandler(this.prop.OnCashInsertedAdvice);
                        }
                        else if (this.CashInAvailable && (this.Phase == Enums.Commands.CashInStart || this.Phase == Enums.Commands.CashIn))
                            this.ExecuteDoCashIn(true);
                        else
                            if (this.Phase != Enums.Commands.CashIn)
                            this.StartTimer(true);
                        break;
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_UNKNOWN": //Informa el rechazo de billetes
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_CASHINUNITFULL":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_INVALIDBILL":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_NOBILLSTODEPOSIT":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_DEPOSITFAILURE":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_COMMINPCOMPFAILURE":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_FOREIGN_ITEMS_DETECTED":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_INVALIDBUNCH":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_COUNTERFEIT":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_LIMITOVERTOTALITEMS":
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_LIMITOVERAMOUNT":
                        Log.Info("...Reject notes evt...");
                        this.RejectBinEmpty = false;
                        if (this.prop.ForceOpenShutterAtReject)
                        {
                            Log.Info("Forced by property to open shutter at reject");
                            this.ForceOpenShutterAtReject = true;
                        }
                        break;
                    case "WFS_EXEE_CIM_INPUTREFUSE_WFS_CIM_STACKERFULL":
                        Log.Info("...Escrow full evt...");
                        this.RejectBinEmpty = false;
                        if (this.prop.ForceOpenInputShutterAtEscrowFull)
                        {
                            Log.Info("Forced by property to open input shutter at escrow full");
                            this.Core.Sdo.CIM_Status(); //Consulto si hay billetes en la bandeja de entrada
                        }
                        break;
                }
            }
        }

        private void HandleItemsTakenOnRollback()
        {
            if (this.Phase == Enums.Commands.RollBack)
            {
                this.StopTimer();
                this.WriteEJ("RollBack remove items -OK-");
                if (this.IsDeclined)
                {
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.Extension2.DeclinedNextStateNumber);
                }
                else
                {
                    if (this.FlagIsCancel)
                        this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                    else
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                }
            }
        }

        /// <summary>
        /// Guarda los RequestID de los ACK recibidos
        /// </summary>
        /// <param name="dm"></param>
        private void HandlerAckReceive(DeviceMessage dm)
        {
            try
            {
                Log.Info($"/-->ACK request ID: {dm.Header.RequestId}");
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
            {
                this.ProcessCompletion(dm);
            }
            else
            {
                Log.Error($"/-->ACK request ID: {dm.Header.RequestId} not found");
                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
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
                Log.Info($"/--> {dm.Device}");
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (dm.Device == Enums.Devices.IOBoard || dm.Device == Enums.Devices.CashAcceptor || dm.Device == Enums.Devices.Printer)
                {
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                        Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                    else
                        Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                }
                if (dm.Device == Enums.Devices.Printer && dm.Command == Enums.Commands.State)
                {
                    this.PrinterNotAvailable = this.GetPrinterState(cr);
                    if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
                    {
                        this.Core.Sdo.IOBoard_GetState();
                    }
                    else
                    {
                        this.Open_CIM();
                        Log.Warn("VERIFY SENSORS: DISABLED");
                        this.WriteEJ("VERIFY SENSORS: DISABLED");
                    }
                }
                else if (dm.Device == Enums.Devices.IOBoard)
                {
                    if (dm.Command == Enums.Commands.State)
                    {
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.NoError, Enums.DeviceStatus.AIO_DeviceSuccess);
                            if (this.AnalyzeSensorsState(cr.Data))
                                this.Open_CIM();
                        }
                        else
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.IOBoard, Const.Fitness.Fatal, Enums.DeviceStatus.AIO_DeviceError);
                            this.ShowModalAdvice(this.prop.OnSensorErrorAdvice);
                        }
                    }
                }
                else if (dm.Device == Enums.Devices.CashAcceptor)
                {
                    this.StopTimer();//Detengo el timer de More Time
                    switch (dm.Command)//Switcheo respuestas de los comandos
                    {
                        case Enums.Commands.Open:
                            this.Process_Completion_CIM_Open(cr);
                            break;
                        case Enums.Commands.Status:
                            this.Process_Completion_CIM_Status(cr);
                            break;
                        case Enums.Commands.GetBankNoteTypes:
                            this.Process_Completion_CIM_GetBankNoteTypes(cr);
                            break;
                        case Enums.Commands.ConfigureNoteTypes:
                            this.Process_Completion_CIM_ConfigureNoteTypes(cr);
                            break;
                        case Enums.Commands.CashInStart:
                            this.Process_Completion_CIM_CashInStart(cr);
                            break;
                        case Enums.Commands.Reset:
                            this.Process_Completion_CIM_Reset(cr);
                            break;
                        case Enums.Commands.CashIn:
                            this.Process_Completion_CIM_CashIn(cr);
                            break;
                        case Enums.Commands.Cancel:
                            this.Process_Completion_CIM_Cancel(cr);
                            break;
                        case Enums.Commands.CashInEnd:
                            this.Process_Completion_CIM_CashInEnd(cr);
                            break;
                        case Enums.Commands.RollBack:
                            this.Process_Completion_CIM_RollBack(cr);
                            break;
                        case Enums.Commands.OpenEscrowShutter:
                            this.Process_Completion_CIM_OpenEscrowShutter(cr);
                            break;
                        case Enums.Commands.Retract:
                            this.Process_Completion_CIM_Retract(cr);
                            break;
                        case Enums.Commands.Close:
                            this.Process_Completion_CIM_Close(cr);
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Open(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                        this.Core.Sdo.CIM_Status();
                        break;
                    case CompletionCodeEnum.TimeOut:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                        if (this.prop.CashHandlingOnOpenCimTimeOut == Const.ActionOnCashAcceptError.Reset)
                        {
                            this.Core.Sdo.AD_Close();
                            Log.Error("--Close AD--");
                            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        }
                        else
                            this.SetActivityResult2(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        break;
                    default:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "Process_Completion_CIM_Status"
        private void Process_Completion_CIM_Status(Completion cr)
        {
            Log.Debug($"/-->Phase: {this.Phase}");
            bool ret1 = false;
            try
            {
                if (cr.CompletionCode == CompletionCodeEnum.Success)
                {
                    StatusCIM statusCIM = Utils.JsonDeserialize<StatusCIM>(out ret1, cr.Data);
                    if (ret1)
                    {
                        switch (this.Phase)
                        {
                            case Enums.Commands.CashIn:
                                if (statusCIM.Pos[0].PositionStatus.Equals("1") ? true : false)//Billetes detectados en bandeja de entrada
                                {
                                    this.Core.Sdo.CIM_OpenInputShutter();
                                    this.ShowRejectScreen(true);
                                    Log.Info("Input position not empty");
                                }
                                break;
                            default:
                                if (this.CashInEndStatusAvailable)
                                    this.ProcessCashInEndStatus(statusCIM);
                                else
                                    this.ProcessInitialStatus(statusCIM);
                                break;
                        }
                    }
                    else
                        this.ProcessCashInEndError();
                }
                else
                    this.ProcessCashInEndError();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ProcessCashInEndStatus(StatusCIM statusCIM)
        {
            if (((statusCIM.IntermediateStacker.Equals("0") || statusCIM.IntermediateStacker.Equals("5")) && (statusCIM.Device.Equals("0") || statusCIM.Device.Equals("5"))) && !this.DepositHardwareError)
            {
                this.SetActivityResult(StateResult.SUCCESS, this.prop.Extension2.GoodNextStateNumber);
                this.WriteEJ("CashInEnd -OK-");
                Log.Info("CashInEnd -OK-");
                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);//Envía el status a host de error de CIM
            }
            else
                this.ProcessCashInEndError();
        }

        private void ProcessCashInEndError()
        {
            if (!this.CashInEndBufferNull)
            {
                Log.Warn("DepositHardwareErrorDetected: {0}", this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError);//Hadware error detectado en CashAccept state "E8"
                this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError = true; //Marco la transacción como errónea (E8)
            }
            this.WriteEJ("CashInEnd -ERROR-");
            Log.Info("CashInEnd -ERROR-");
            if (this.prop.AutoCashIn) //MEI
            {
                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
            else
            {
                this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterError, false);
                this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterError, false);
                this.ShowConfirmationScreen(true);
                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_CashInEndError);//Envía el status a host de error de CIM
                this.CallHandler(this.prop.OnCashInEndErrorAdvice);
            }
        }

        private void ProcessInitialStatus(StatusCIM statusCIM)
        {
            this.Phase = Enums.Commands.Status;
            if (!this.FlagIsCancel)
            {
                if (statusCIM.IntermediateStacker.Equals("0") || statusCIM.IntermediateStacker.Equals("5"))//A)- Verifica si hay valores en escrow
                {
                    if ((this.prop.VerifyAcceptor && !statusCIM.Acceptor.Equals("0")) || (this.prop.VerifyBanknoteReader && !statusCIM.BanknoteReader.Equals("0")))
                    {
                        this.CimStatusErrorHandler(statusCIM);
                    }
                    else
                    {
                        this.InputPositionNotEmpty = statusCIM.Pos[0].PositionStatus.Equals("1") ? true : false; //C)- Billetes presentes en la bandeja de entrada
                        if (statusCIM.Pos.Count > 2) //SNBC y Glory
                        {
                            this.RejectBinEmpty = statusCIM.Pos[2].PositionStatus.Equals("0"); //D)- Billetes presentes en la bandeja de reject
                            this.EscrowShutterOpen = statusCIM.Pos[2].Shutter.Equals("1"); //E)- Compuerta de escrow abierta "1"
                        }
                        if (this.EscrowShutterOpen)
                            this.CimStatusErrorHandler(statusCIM); //Status Error + Reset
                        else if (this.RejectBinEmpty)
                        {
                            if (this.prop.ConfigBankNotes)
                            {
                                if (this.prop.CurrencyAcceptedUnique)
                                    this.Core.Sdo.CIM_GetBankNoteTypes(); //Solicito tabla de divisas aceptadas
                                else
                                    this.Core.Sdo.CIM_ConfigureNoteTypes(this.GetConfigBankNotesTypes()); //Mando a configurar según configuración de estado
                            }
                            else
                            {
                                this.Phase = Enums.Commands.CashInStart;
                                this.Core.Sdo.CIM_AsyncCashInStart();
                            }
                        }
                        else
                            this.ShowRejectScreen(true);
                    }
                }
                else //Se detectan notas en escrow
                {
                    if (statusCIM.Device.Equals("0")) //Verifico si el dispositivo esta ok
                    {
                        Log.Warn("Notes in escrow detected");
                        this.CashInAvailable = false; //Si hay error, no activo cashIn
                        this.NotesInEscrowDetected = true;
                        this.ShowConfirmationScreen(true); //Muestro nuevamente la pantalla con los billetes en escrow para que se reintente
                        this.WriteEJ("NOTES IN ESCROW PRESENT -WARNING-");
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_StatusNotesInEscrow);//Envía el status a host de error de CIM
                        this.CallHandler(this.prop.OnStatusNotesInEscrowAdvice); //Muertro aviso de notas en escrow
                    }
                    else
                        this.CimStatusErrorHandler(statusCIM); //Status Error + Reset
                }
            }
            else
                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
        }

        private void CimStatusErrorHandler(StatusCIM statusCIM)
        {
            Log.Warn($"CIM device error a) Device: {statusCIM.Device} - Acceptor: {statusCIM.Acceptor} - BanknoteReader: {statusCIM.BanknoteReader}");
            this.WriteEJ("CIM DEVICE ERROR");
            switch (this.prop.CashHandlingOnStatusError)
            {
                case Const.ActionOnCashAcceptError.Reset:
                    this.CallHandler(this.prop.OnPleaseWait);
                    if (this.FlagReset)
                    {
                        this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                        this.FlagReset = false;
                    }
                    else
                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                    break;
                default:
                    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                    break;
            }
            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
        }
        #endregion "Process_Completion_CIM_Status"

        private void Process_Completion_CIM_GetBankNoteTypes(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                        bool ret = false;
                        CimNoteTypeInfo cimNoteTypeInfo = Utilities.Utils.JsonDeserialize<CimNoteTypeInfo>(out ret, cr.Data);
                        if (ret)
                        {
                            Note note = cimNoteTypeInfo.Notes.FirstOrDefault(x => x.configured == true);
                            if (note != null)
                            {
                                if (this.Core.Bo.ExtraInfo.Currency.Equals(note.curId))//Si el currency seteado es el mismo del CIM lo mando a abrir
                                {
                                    this.Phase = Enums.Commands.CashInStart;
                                    this.Core.Sdo.CIM_AsyncCashInStart();
                                }
                                else
                                    this.Core.Sdo.CIM_ConfigureNoteTypes(this.GetConfigBankNotesTypes(cimNoteTypeInfo));//Si son distintos los mando a configurar
                            }
                            else
                            {
                                ret = false;
                                Log.Error("Note template is null");
                            }
                        }
                        if (ret == false)
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        }
                        break;
                    default:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_ConfigureNoteTypes(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                        {
                            this.Phase = Enums.Commands.CashInStart;
                            this.Core.Sdo.CIM_AsyncCashInStart();
                        }
                        break;
                    default:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_CashInStart(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                        this.CompleteCashInStart = true;
                        if (this.prop.AutoCashIn)//MEI
                        {
                            this.ExecuteDoCashIn(true);
                            this.ShowDepositCashPrepareScreen();
                        }
                        else if (this.InputPositionNotEmpty || this.prop.DirectCashIn)
                        {
                            this.ExecuteDoCashIn(true);
                        }
                        else
                        {
                            this.CashInAvailable = true; //Activo el manejo del evento que habilita el CashIn
                            this.ShowDepositCashPrepareScreen();
                            if (this.prop.ForceOpenInputShutterAtCashInStart)
                            {
                                Log.Info("Forced by property to open shutter at CashInStart");
                                this.Core.Sdo.CIM_OpenInputShutter();
                            }
                        }
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);//Envía el status a host de device Ok
                        break;
                    case CompletionCodeEnum.Reject:
                        if (!this.prop.AutoCashIn)//MEI
                            this.CompleteCashInStart = true;//Fix para que no se quede esperando en modo AutoDeposit
                        this.CashInAvailable = true; //Activo el manejo del evento que habilita el CashIn
                        this.ShowRejectScreen(true);
                        break;
                    default:
                        this.CashInAvailable = false; //Desactivo el manejo del evento que habilita el CashIn
                        if (this.FlagReset)
                        {
                            if (this.prop.ActiveResetAtCashInStart)
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                            else
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                            this.FlagReset = false;
                        }
                        else
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        }
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Reset(Completion cr)
        {
            Log.Debug($"/-->Fase: {this.Phase}");
            try
            {
                this.WriteEJ($"Reset CIM - Phase: {this.Phase}");
                switch (this.Phase)
                {
                    case Enums.Commands.Status:
                        this.Core.Sdo.CIM_Status();
                        break;
                    case Enums.Commands.Open:
                        switch (cr.CompletionCode)
                        {
                            case CompletionCodeEnum.Success:
                                this.Core.Sdo.CIM_AsyncCashInStart();
                                break;
                            default:
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    case Enums.Commands.CashInStart://Despues de un time out con buffer en cero se manda a reiniciar siempre
                    case Enums.Commands.CashIn:
                        switch (cr.CompletionCode)
                        {
                            case CompletionCodeEnum.Success:
                                this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                break;
                            default:
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    case Enums.Commands.CashInEnd:
                        switch (cr.CompletionCode)
                        {
                            case CompletionCodeEnum.Success:
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.Extension2.GoodNextStateNumber);
                                break;
                            default:
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    case Enums.Commands.Cancel:
                        switch (cr.CompletionCode)
                        {
                            case CompletionCodeEnum.Success:
                                this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                break;
                            default:
                                this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    case Enums.Commands.RollBack:
                        if (this.prop.CashHandlingOnRollback == Const.ActionOnCashAcceptError.Reset)
                        {
                            this.Core.Sdo.CIM_OpenRejectShutter();
                            this.ShowModalAdvice(this.prop.OnRollbackCashAdvice);
                        }
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_CashIn(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                this.CashInAvailable = true; //Activo al ejecutar CashIn
                this.FlagReset = true;
                this.CancelAvailable = true;
                if (this.ForceOpenShutterAtReject)
                {
                    this.ForceOpenShutterAtReject = false;
                    this.Core.Sdo.CIM_OpenRejectShutter();
                }
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Reject:
                    case CompletionCodeEnum.Success:
                    case CompletionCodeEnum.NotelistEmpty:
                        if (this.prop.ForceOpenInputShutterAtPickFail && cr.CompletionCode == CompletionCodeEnum.NotelistEmpty)//Forzar apertura de shutter en caso de pick fail
                        {
                            Log.Info("Forced by property to open input shutter at pick fail");
                            this.Core.Sdo.CIM_OpenInputShutter();
                        }
                        bool rejectActive = false;
                        if (!this.RejectBinEmpty || cr.CompletionCode == CompletionCodeEnum.Reject)
                        {
                            this.RejectBinEmpty = true;
                            rejectActive = true;
                        }
                        this.HandleDoCashInResponse(cr.Data, rejectActive);
                        break;
                    case CompletionCodeEnum.TimeOut:
                        this.ItemsInserted = false;
                        this.Core.Sdo.CIM_AsyncCancel();
                        break;
                    case CompletionCodeEnum.Canceled:
                        if (this.prop.AutoCashIn)//MEI
                        {
                            if (this.Phase == Enums.Commands.CashInEnd)
                            {
                                this.CallHandler(this.prop.OnStoringCashAdvice);
                                Thread.Sleep(200);
                                this.ExecuteCashInEnd();//Envío a depositar y aguardo la confirmación 
                            }
                            else if (this.Phase == Enums.Commands.CashIn)
                            {
                                Thread.Sleep(200);
                                this.ExecuteDoCashIn(true);
                            }
                            else if (this.prop.ActiveResetAtCancel)
                            {
                                this.FlagIsCancel = true;
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                            }
                            else
                                this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                        }
                        else
                            this.ShowConfirmationScreen(true);
                        break;
                    default:
                        this.CashInError = true;
                        this.WriteEJ("CashIn -ERROR-");
                        this.CashInAvailable = false; //Si hay error, no activo cashIn
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_CashInError);//Envía el status a host de error de CIM
                        this.HandleDoCashInResponse(cr.Data, false);
                        if (this.prop.AutoCashIn && this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)//MEI
                        {
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                        }
                        else
                        {
                            switch (this.prop.CashHandlingOnCashInError)
                            {
                                case Const.ActionOnCashAcceptError.Eject:
                                    this.ExecuteRollBack(true);
                                    break;
                                case Const.ActionOnCashAcceptError.Reset:
                                    this.CallHandler(this.prop.OnPleaseWait);
                                    this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                    break;
                                case Const.ActionOnCashAcceptError.EndSession:
                                    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                    break;
                                case Const.ActionOnCashAcceptError.Persist:
                                    this.UpdateDepositCounters(cr.Data);//Persiste contadores
                                    this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError = true; //Marco la transacción como errónea (E8)
                                    break;
                                default:
                                    //no action
                                    break;
                            }
                        }
                        this.CallHandler(this.prop.OnCashInErrorAdvice); //Muertro aviso de error en cashIn
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Cancel(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                if (cr.CompletionCode != CompletionCodeEnum.Success)
                    Log.Warn($"Cancel return: {cr.CompletionCode.ToString()}");
                else
                    Log.Info($"Cancel return: {cr.CompletionCode.ToString()}");

                if (this.prop.AutoCashIn)//MEI
                {
                    if (this.Phase == Enums.Commands.CashInEnd)
                    {
                        this.CallHandler(this.prop.OnStoringCashAdvice);
                        Thread.Sleep(500); //Fix WFS_ERR_DEV_NOT_READY in CashInEnd return
                        this.ExecuteCashInEnd();//Envío a depositar y aguardo la confirmación 
                    }
                    else
                        this.ShowConfirmationScreen(true);
                }
                else if (this.Phase == Enums.Commands.CashIn)
                    this.ShowConfirmationScreen(true);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_CashInEnd(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                if (!this.prop.AutoCashIn)
                    this.UpdateDepositCounters(cr.Data);//Persiste contadores si no es MEI
                this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError = false;
                this.CashInEndStatusAvailable = true;
                this.Core.Sdo.CIM_Status();
                if (cr.CompletionCode != CompletionCodeEnum.Success && cr.CompletionCode != CompletionCodeEnum.Reject)
                {
                    this.DepositHardwareError = true;
                }
                //SIEMPRE activo Shaker
                if (this.prop.QtyActiveShaker < this.Core.Counters.TotalDepositedNotes && this.prop.QtyActiveShaker != 0)
                    this.Core.Sdo.AIO_StartShaker();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_RollBack(Completion cr)
        {
            Log.Debug($"/-->Return: {cr.CompletionCode}");
            try
            {
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                        this.WriteEJ("RollBack -OK-");
                        Log.Warn("RollBack -OK-");
                        this.ShowModalAdvice(this.prop.OnRollbackCashAdvice);
                        break;
                    case CompletionCodeEnum.Reject:
                        if (this.prop.CashHandlingOnRollback == Const.ActionOnCashAcceptError.Reset)
                        {
                            this.WriteEJ("RollBack -RJ-");
                            Log.Warn("RollBack -RJ-");
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                        }
                        else
                        {
                            this.WriteEJ("RollBack -OK-");
                            Log.Warn("RollBack -OK-");
                            this.ShowModalAdvice(this.prop.OnRollbackCashAdvice);
                        }
                        break;
                    default:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_RollBackError);//Envía el status a host de error de CIM
                        this.WriteEJ("RollBack -ERROR-");
                        Log.Error("RollBack -ERROR-");
                        this.Core.Sdo.CIM_OpenEscrowShutter();
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_OpenEscrowShutter(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                if (cr.CompletionCode != CompletionCodeEnum.Success)
                {
                    this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_RollBackError);//Envía el status a host de error de CIM
                    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                }
                else
                {
                    this.ShowModalAdvice(this.prop.OnRollbackCashAdvice, false);
                    WriteEJ("ESCROW GATE OPEN");
                    Log.Warn("ESCROW GATE OPEN");
                }
                if (this.IsDeclined)
                {
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.Extension2.DeclinedNextStateNumber);
                }
                else
                {
                    if (this.FlagIsCancel)
                        this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                    else
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Retract(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (Bills b in this.Core.Bo.ExtraInfo.CashInInfo.Bills)
                {
                    sb.Append($"{Environment.NewLine}--> ID: {b.Id.ToString().PadLeft(3, ' ')} - CUR: {b.Currency} - QTY: {b.Quantity.ToString().PadLeft(3, ' ')} - VAL: {b.Value.ToString().PadLeft(4, ' ')} - NDC: {b.NDCNoteID}");
                }
                string msg = "Retract--> bills buffer: ";
                Log.Warn(msg);
                Log.Warn(sb.ToString().Replace("\r", "\\r").Replace("\n", "\\n"));
                this.WriteEJ(msg);
                this.WriteEJ(sb.ToString());
                switch (cr.CompletionCode)
                {
                    case CompletionCodeEnum.Success:
                    case CompletionCodeEnum.Reject:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Warning, Enums.DeviceStatus.CIM_RetractSuccess);//Envía el status a host de retract success
                        this.WriteEJ("Retract -OK-");
                        Log.Info("Retract -OK-");
                        break;
                    default:
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_RetractError);//Envía el status a host de error de retract
                        this.WriteEJ("Retract -ERROR-");
                        Log.Error("Retract -ERROR-");
                        break;
                }
                if (this.FlagIsCancel)
                {
                    this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                }
                else
                    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Process_Completion_CIM_Close(Completion cr)
        {
            Log.Debug("/-->");
            try
            {
                if (cr.CompletionCode != CompletionCodeEnum.Success)
                    Log.Error("Close CIM error.");
                if (this.prop.Extension2.OperationMode.Equals("001"))//Demora para dar tiempo al siguiente Open
                    Thread.Sleep(500);
                //Luego de recibir respuesta al cierre del CIM salgo del estado.
                this.Quit();
                this.Core.SetNextState(this.ActivityResult, this.NextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #endregion "Returns from SDO"

        #region "Returns from UI"
        /// <summary>
        /// Maneja los retornos de las teclas presionadas que no son FDKs
        /// </summary>
        /// <param name="othersKeys"></param>
        private void HandlerOthersKeysReturn(string othersKeys)
        {
            try
            {
                Log.Debug("/--->");
                this.StopTimer();//Detengo el timer de More Time
                this.WriteEJ($"Key press: {othersKeys}");
                switch (othersKeys)
                {
                    case "CONTINUE":
                        if (this.prop.OnShowConfirmModal.Action == StateEvent.EventType.runScript)
                        {
                            this.StartTimer(false);
                            this.CallHandler(this.prop.OnShowConfirmModal);//Activación de modal de confirmación
                        }
                        else
                            this.ProcessDeposit();//Confirma TX
                        break;
                    case "ENTER":
                        this.ProcessDeposit();//Confirma TX
                        break;
                    case "CANCEL":
                        if (this.Phase == Enums.Commands.UNK)
                            this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                        else
                        {
                            if (!this.ItemsInserted)
                            {
                                if (this.CancelAvailable)
                                {
                                    if (this.prop.AutoCashIn)//MEI
                                    {
                                        this.Phase = Enums.Commands.Cancel;
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        this.Core.Sdo.CIM_AsyncCancel();
                                    }
                                    else if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)
                                    {
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        if (this.prop.ActiveResetAtCancel)
                                        {
                                            this.FlagIsCancel = true;
                                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                        }
                                        else
                                            this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                    }
                                    else
                                    {
                                        if (this.CashInAvailable && !this.CashInEndInProgress)
                                            this.ExecuteRollBack(true);
                                        else
                                            Log.Warn($"Couldnot execute RollBack - CashInAvailable: {this.CashInAvailable} - CashInEndInProgress: {this.CashInEndInProgress}");
                                    }
                                }
                                else
                                    Log.Warn("Cancel disabled");
                            }
                            else
                                Log.Warn("Items already inserted");
                        }
                        break;
                    case "MORE":
                        if (this.CashInAvailable && this.CompleteCashInStart) //El boton MORE, solo se habilita cuando no esta habilidado el autodeposit (previo error de hardware)
                        {
                            this.ExecuteDoCashIn(true);
                        }
                        else
                        {
                            this.Core.HideScreenModals(); //Quito los avisos de pantalla
                            Log.Warn("CashIn not available");
                        }
                        break;
                    case "CASHINERROR":
                        Log.Warn($"CashIn Error. CashHandlingOnCashInError: {this.prop.CashHandlingOnCashInError.ToString()}");
                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                        break;
                    case "CASHINENDERROR":
                        Log.Warn($"CashInEnd Error. CashHandlingOnCashInEndError: {this.prop.CashHandlingOnCashInEndError.ToString()}");
                        switch (this.prop.CashHandlingOnCashInEndError)
                        {
                            case Const.ActionOnCashAcceptError.Eject:
                                this.ExecuteRollBack(true);
                                break;
                            case Const.ActionOnCashAcceptError.Reset:
                                this.CallHandler(this.prop.OnPleaseWait);
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                break;
                            default:
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    case "STATUSNOTESINESCROW":
                        Log.Warn($"CashInEnd Error. CashHandlingOnCashInEndError: {this.prop.CashHandlingOnCashInEndError.ToString()}");
                        switch (this.prop.CashHandlingOnStatusNotesInEscrow)
                        {
                            case Const.ActionOnCashAcceptError.Reset:
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                break;
                            default:
                                this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                break;
                        }
                        break;
                    default:
                        Log.Warn($"Unexpected key: {othersKeys}");
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja los retornos de las FDK presionadas.
        /// SOLO SE UTILIZA PARA MANEJAR EL RETORNO DEL BOTON DEL DIV DE AVISO DE BILLETES REJECTADOS!!!!!!
        /// </summary>
        /// <param name="FDKdata"></param>
        private void HandlerFDKreturn(string FDKdata)
        {
            try
            {
                Log.Debug("/--->");
                this.StopTimer(); //Detengo el timer del More Time
                switch (this.Phase)
                {
                    case Enums.Commands.CashInStart:
                        {
                            this.Core.Sdo.CIM_AsyncCashInStart();
                            break;
                        }
                    case Enums.Commands.CashIn:
                        {
                            this.ShowConfirmationScreen(true);
                            break;
                        }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "Returns from UI"

        #region "Miscelaneos"
        /// <summary>
        /// Update logical counters after a confirm deposit and verify cashInEndBuffer
        /// </summary>
        private void UpdateDepositCounters(string cashInEndBuffer)
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            List<Detail> lstDetail = new List<Detail>();
            List<Item> lstItemsCurr;
            bool ret = true;
            decimal updValue = 0; //Valor adaptado para mostrar dos decimales al final
            try
            {
                Log.Debug("/-->");
                if (this.prop.AnalyzeCashInEndBuffer)
                {
                    CashInInfo cashInInfo = Utilities.Utils.JsonDeserialize<CashInInfo>(out ret, cashInEndBuffer);  //REVISAR: cashInEndBuffer llega vacio !!! | lo llama Process_Completion_CIM_CashInEnd(Completion cr)
                    //A)- Analizo el buffer del retorno de CashInEnd
                    this.WriteEJ("Accepted notes: ");
                    if (ret)
                    {
                        cashInInfo.Bills.ForEach(delegate (Bills b)
                        {
                            string text = b.Quantity == 1 ? " note  of " : " notes of ";
                            this.WriteEJ($"{b.Quantity.ToString().PadLeft(3, ' ')}{text}{b.Currency} {b.Value.ToString().PadLeft(5, ' ')} release {b.Release}");
                        });
                    }
                }
                if (ret)
                {
                    //B)- Guardo los datos del depósito parcial en memoria
                    this.Core.Bo.ExtraInfo.CashInMultiCashData.UpdateMultiCashData(this.Core.Bo.ExtraInfo.CashInInfo);
                    //C)- Actualizo contadores lógicos
                    IEnumerable<string> source = from i in this.Core.Bo.ExtraInfo.CashInInfo.Bills
                                                 select i.Currency;
                    currencies = source.ToList();
                    distinctCurrencies = currencies.Distinct().ToArray();
                    for (int k = 0; k < distinctCurrencies.Length; k++)
                    {
                        lstItemsCurr = new List<Item>();
                        List<Bills> lstItems = this.Core.Bo.ExtraInfo.CashInInfo.Bills.Where(e => e.Currency.Equals(distinctCurrencies[k])).ToList();
                        lstItems.ForEach(delegate (Bills b)
                        {
                            updValue = (decimal)(b.Value * 100);
                            lstItemsCurr.Add(new Item(updValue, (decimal)b.Quantity, (decimal)(updValue * b.Quantity), "NOTE", "", ""));
                        });
                        Detail detail = new Detail(distinctCurrencies[k], Detail.ContainerIDType.CashAcceptor, "NOTEACCEPTOR", this.Core.GetCollectionId(Enums.TransactionType.DEPOSIT), lstItemsCurr);
                        lstDetail.Add(detail);
                    }
                    if (!this.Core.Counters.UpdateContents(lstDetail)) //Actualizo contadores
                    {
                        Log.Error("Can not update counters");
                        this.WriteEJ("ERROR WHEN UPDATE COUNTERS");
                    }
                    this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();//Borro los datos parciales                                                     
                    this.Core.PrintEJCounters(Detail.ContainerIDType.CashAcceptor);//Logeo en EJ los contadores
                    //D)- Solo para modo RETAIL, cargo los datos de recupero de transaccion
                    if (this.prop.Extension2.OperationMode.Equals("001") && this.Core.AlephATMAppData.RetrievalTransactionEnable)
                    {
                        this.LoadRetrievalTransaction(this.Core.Bo.ExtraInfo.CashInMultiCashData.ListCashInInfo);
                    }
                    else if (this.prop.Extension2.OperationMode.Equals("002"))//Solo para modo PAY invoice, imprimo ticket
                    {
                        this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter1, false); //Print ticket ok
                        this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinter2, false);
                        this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinter, false);
                        this.ProcessPrinterData(this.prop.OnSendTicketToBD, false);
                        this.Core.Counters.UpdateTSN();
                        //Informo contadores
                        //Thread prtWndThd;
                        //prtWndThd = new Thread(this.SendContents);
                        //prtWndThd.Start();
                    }
                }
                else
                {
                    this.CashInEndBufferNull = true;
                    Log.Error("CashInEndBuffer IS NULL");
                    WriteEJ("CASHINENDBUFFER IS NULL");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void SendContents()
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            try
            {
                Log.Debug("/--->");
                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.SEND_CONTENTS, this.Core.Counters.Contents, this.prop.HostName);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Load a transaction object on a signed XML file
        /// </summary>
        private void LoadRetrievalTransaction(List<CashInInfo> listCashInInfo)
        {
            string filePath = $"{Const.appPath}Retrieval\\RetrievalTransaction.xml";
            CashDepositTransaction cashDepositTransaction = new CashDepositTransaction(
            listCashInInfo,
            this.Core.Bo.ExtraInfo.Currency,
            this.Core.Bo.ExtraInfo.UserProfileMain.User,
            this.Core.Bo.ExtraInfo.UserProfileMain.UserName,
            this.Core.GetCollectionId(Enums.TransactionType.DEPOSIT),
            this.Core.Bo.ExtraInfo.ExtraData,
            DateTime.Now,
            this.Core.Counters.GetTSN(),
            this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError);
            if (this.Core.SerializeXMLSigned<CashDepositTransaction>(filePath, cashDepositTransaction))
                Log.Info("Serialize XML signed ok");
            else
                Log.Error("Serialize XML signed error");
        }

        /// <summary>
        /// Get denomination settings from template filtered by selected currency
        /// </summary>
        /// <returns></returns>
        private string GetConfigBankNotesTypes(CimNoteTypeInfo cimNoteTypeInfo)
        {
            string jsonData = string.Empty;
            bool currDetected = false;
            try
            {
                Log.Debug("/--->");
                cimNoteTypeInfo.Notes.ForEach(x =>
                {
                    if (x.curId.Equals(this.Core.Bo.ExtraInfo.Currency))
                    {
                        x.configured = true;
                        currDetected = true;
                    }
                    else
                        x.configured = false;
                });
                cimNoteTypeInfo.SelectedCurrency = this.Core.Bo.ExtraInfo.Currency;
                jsonData = Utilities.Utils.JsonSerialize(cimNoteTypeInfo);
                if (!currDetected)
                    Log.Error($"Currency \"{this.Core.Bo.ExtraInfo.Currency}\" not detected in template");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return jsonData;
        }

        /// <summary>
        /// Get denomination settings from configuration of state.
        /// </summary>
        /// <returns></returns>
        private string GetConfigBankNotesTypes()
        {
            string jsonData = string.Empty;
            byte[] SetDenom;
            try
            {
                Log.Debug("/--->");
                CimNoteTypeInfo cimNoteTypeInfo = new CimNoteTypeInfo();
                SetDenom = this.GetEnableDenominations(this.prop.Extension3.SetDenominations112);
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations1324));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations2536));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations3748));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations4960));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations6172));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations7384));
                SetDenom = this.Combine(SetDenom, this.GetEnableDenominations(this.prop.Extension3.SetDenominations8596));
                for (int i = 0; i < SetDenom.Length - 1; i++)
                {
                    if (i < SetDenom.Length - 2)
                    {
                        if (SetDenom[i] == 0x01)
                            cimNoteTypeInfo.Notes.Add(new Note(true, 0, 0, i + 1, ""));
                        else
                            cimNoteTypeInfo.Notes.Add(new Note(false, 0, 0, i + 1, ""));
                    }
                }
                cimNoteTypeInfo.SelectedCurrency = this.Core.Bo.ExtraInfo.Currency;
                jsonData = Utilities.Utils.JsonSerialize(cimNoteTypeInfo);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return jsonData;
        }

        internal byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private byte[] GetEnableDenominations(string value)
        {
            byte[] bits;
            int entero;
            byte[] SetDenom = new byte[12];
            char[] chunk;
            int count1 = 0;
            int count2 = 3;
            try
            {
                chunk = value.ToCharArray();
                for (int i = 0; i < 3; i++)
                {
                    entero = Utilities.Utils.HexToInt(chunk[i].ToString());
                    bits = Utilities.Utils.ByteToBits((byte)entero, Utilities.Utils.Endian.LittleEndian);
                    for (int j = 0; j < 4; j++)
                    {
                        SetDenom[count1 + j] = bits[count2];
                        count2--;
                    }
                    count2 = 3;
                    count1 += 4;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return SetDenom;
        }
        #endregion "Miscelaneos"

        #region "Execute device functions"
        private void Open_CIM()
        {
            try
            {
                if (this.prop.UpdateConfigurationFile)//MEI
                {
                    if (this.Core.WriteIniConfigFileAsync(false, true).GetAwaiter().GetResult())
                    {
                        Thread.Sleep(100);
                        this.Core.Sdo.CIM_AsyncOpen();
                    }
                    else
                    {
                        Log.Error("Write config file error");
                        this.SetActivityResult2(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                    }
                }
                else
                    this.Core.Sdo.CIM_AsyncOpen();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void Close_CIM()
        {
            try
            {
                this.Core.Sdo.CIM_AsyncClose();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Inicia el proceso de recolección de billetes
        /// </summary>
        private void ExecuteDoCashIn(bool showNotification)
        {
            this.CashInAvailable = false;
            Log.Debug($"ShowNotification: {showNotification}");
            this.Phase = Enums.Commands.CashIn;
            this.CancelAvailable = false;
            if (this.prop.AutoCashIn)//MEI
            {
                this.Core.HideScreenModals(); //Quito los avisos de pantalla
                this.CancelAvailable = true;
                this.StartTimer(true);
            }
            else if (showNotification)
            {
                this.CallHandler(this.prop.OnCashInsertedAdvice);
            }
            if (this.CompleteCashInStart)
            {
                this.Core.Sdo.CIM_AsyncCashIn();
            }
            else
            {
                Log.Error("CashInStart failure");
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        private void ExecuteCashInEnd()
        {
            this.Phase = Enums.Commands.CashInEnd;
            try
            {
                if (!this.CashInEndInProgress)
                {
                    this.CashInEndInProgress = true;
                    this.Core.Sdo.CIM_AsyncCashInEnd();
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ProcessDeposit()
        {
            if (this.prop.AutoCashIn)
                Thread.Sleep(200);// Fix cancel cashIn
            if (this.Core.Sdo.DevConf.IOBoardConfig.Enable)
            {
                if (this.AnalyzeMemorySensorsData())
                    this.ExecuteDeposit();
            }
            else
                this.ExecuteDeposit();
        }

        /// <summary>
        /// Procesa la operación luego de presionar la tecla ENTER o Abandono de session. Se solicita el envío de billetes a la bóveda
        /// </summary>
        private void ExecuteDeposit()
        {
            bool cashInActive = this.Phase == Enums.Commands.CashIn ? true : false;
            try
            {
                Log.Info($"Operation Mode: {this.prop.Extension2.OperationMode} - CashInActive: {cashInActive}");
                if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count > 0 || this.prop.AutoCashIn) //Verifico si hay billetes en escrow
                {
                    switch (this.prop.Extension2.OperationMode)
                    {
                        case "000": //"ATM" - En la pantalla de billetes leídos, luego de presionar ENTER KEY, realiza un NextState sin depositar los billetes
                            {
                                this.SetActivityResult(StateResult.SUCCESS, this.prop.Extension2.GoodNextStateNumber);
                                break;
                            }
                        case "001": //001: "RETAIL" - En la pantalla de billetes leídos, luego de presionar ENTER KEY, deposita los billetes y pasa al estado MultiCash
                            {
                                this.RollbackAvailable = false;//Desactivo el rollback
                                if (!this.ItemsInserted)
                                {
                                    if (this.prop.AutoCashIn)//MEI
                                    {
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        if (cashInActive && this.CancelAvailable)
                                        {
                                            this.Core.Sdo.CIM_AsyncCancel();//Si viene de un CashIn Activo, debo cancelar
                                            this.Phase = Enums.Commands.CashInEnd;
                                        }
                                        else
                                            this.ExecuteCashInEnd();//Si viene de un CashIn Cancelado, lo mando a procesar
                                    }
                                    else
                                    {
                                        this.CallHandler(this.prop.OnStoringCashAdvice);
                                        this.ExecuteCashInEnd();//Envío a depositar y aguardo la confirmación  
                                    }
                                }
                                else
                                    Log.Warn("Items already inserted");
                                break;
                            }
                        case "002"://002: "PAY" - En la pantalla de billetes leídos, luego de presionar ENTER KEY, envía la transacción a autorizar para decidir si deposita o no
                            {
                                //FIN DE DEPÓSITO DE SOBRE EXITOSO
                                this.PersistDeposit();
                                break;
                            }
                        default:
                            {
                                Log.Error($"Wrong configuration: {this.prop.Extension2.OperationMode}");
                                break;
                            }
                    }
                }
                else
                {
                    Log.Error($"Can´t show screen: {this.prop.OnStoringCashAdvice.HandlerName}");
                    this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void PersistDeposit()
        {
            List<Detail> lstDetail = new List<Detail>();
            List<Item> lstItems = new List<Item>();
            List<Bills> lstBills = new List<Bills>();
            Contents contents;
            StringBuilder sb = new StringBuilder();
            decimal updValue = 0;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            try
            {
                Log.Debug("/--->");
                if (!this.AuthInProgress)
                {
                    this.AuthInProgress = true; //Para prevenir más de un disparo
                    //A)- Obtengo los datos de los billetes depositados
                    foreach (Bills b in this.Core.Bo.ExtraInfo.CashInInfo.Bills)
                    {
                        updValue = (decimal)(b.Value * 100);
                        sb.Append($"{Environment.NewLine}--> ID: {b.Id.ToString().PadLeft(3, ' ')} - CUR: {b.Currency} - QTY: {b.Quantity.ToString().PadLeft(3, ' ')} - VAL: {Utils.FormatCurrency(b.Value, b.Currency, 4)} - NDC: {b.NDCNoteID}");
                        lstItems.Add(new Item(updValue, (decimal)b.Quantity, (decimal)(updValue * b.Quantity), "NOTE", "", ""));
                    }
                    Log.Info(sb.ToString());
                    Detail detail = new Detail(this.Core.Bo.ExtraInfo.Currency, Detail.ContainerIDType.CashAcceptor, "NOTEACCEPTOR", this.Core.GetCollectionId(Enums.TransactionType.DEPOSIT), lstItems);
                    lstDetail.Add(detail);
                    contents = new Contents(lstDetail);
                    this.CallHandler(this.prop.OnPleaseWait);

                    Thread prtWndThd;
                    prtWndThd = new Thread(new ParameterizedThreadStart(this.AuthorizeTransaction));
                    prtWndThd.Start(contents);
                }
                else
                    Log.Warn($"Transaction authorization already in progress");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void AuthorizeTransaction(object obj)
        {
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "");
            Contents contents = obj as Contents;
            try
            {
                Log.Debug("/--->");

                authorizationResult = this.Core.AuthorizeTransaction(Enums.TransactionType.DEPOSIT, contents, this.prop.HostName);

                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    this.CallHandler(this.prop.OnStoringCashAdvice);
                    this.ExecuteCashInEnd();//Envío a depositar 
                }
                else
                {
                    this.IsDeclined = true;
                    this.prop.OnRollbackCashAdvice.Parameters = this.IsDeclined;
                    this.ExecuteRollBack(true);//Envío a devolver los billetes
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnReceiptPrinterDeclined, true); //Print ticket host error
                    this.ProcessPrinterData(this.prop.OnPrintTicketOnJournalPrinterDeclined, true);
                    this.ProcessPrinterData(this.prop.OnSendTicketToBDError, false);
                    this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();
                    this.Core.Bo.ExtraInfo.AmountLimit = 0;
                }
                this.Core.Bo.ExtraInfo.CashInMultiCashData = new CashInMultiCashData();//Borro datos
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja el retorno del comando DoCashIn con la info de los billetes leídos.
        /// -Almacena los datos de los billetes leídos en la pasada 
        /// -Almacena los datos acumulados del lote
        /// -Logea los datos de los billetes leidos
        /// </summary>
        /// <param name="jsonData"></param>
        private void HandleDoCashInResponse(string jsonData, bool rejectActive)
        {
            StringBuilder sb = new StringBuilder();
            bool ret = false;
            long qtyNotes = 0;
            List<Bills> lstBillsEJ = new List<Bills>();
            decimal value = 0;
            try
            {
                Log.Debug("/--->");
                CashInInfo cashInInfo = Utils.JsonDeserialize<CashInInfo>(out ret, jsonData);
                if (cashInInfo == null)
                {
                    cashInInfo = new CashInInfo();
                }
                cashInInfo.MoreAvailable = true;
                cashInInfo.EnterAvailable = true;
                cashInInfo.CancelAvailable = true;
                this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull = false;
                this.ItemsInserted = false;
                if (ret)
                {
                    if (cashInInfo.Bills.Count != 0)
                    {
                        //Extraigo los nuevos billetes depositados para grabar en EJ
                        if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)
                        {
                            this.Core.Bo.ExtraInfo.CashInInfo = cashInInfo;
                            lstBillsEJ = cashInInfo.Bills.ToList();//EJ
                        }
                        else
                        {
                            if (this.prop.AddRecognizedValues) //Glory - MEI: Se van sumando los depósitos parciales
                            {
                                foreach (Bills b in cashInInfo.Bills)
                                {
                                    var existingBill = this.Core.Bo.ExtraInfo.CashInInfo.Bills.FirstOrDefault(x => x.Value == b.Value && x.Currency == b.Currency && x.Release == b.Release);
                                    if (existingBill == null)
                                        this.Core.Bo.ExtraInfo.CashInInfo.Bills.Add(b);
                                    else
                                    {
                                        this.Core.Bo.ExtraInfo.CashInInfo.Bills.Remove(existingBill);
                                        this.Core.Bo.ExtraInfo.CashInInfo.Bills.Add(new Bills(b.Currency, b.Quantity + existingBill.Quantity, b.Id, b.Value, b.Release, b.NDCNoteID));
                                    }
                                }
                                lstBillsEJ = cashInInfo.Bills.ToList();//EJ
                            }
                            else //SNBC: No se van sumando los depositos parciales porque ya vienen sumados.
                            {
                                //Obtengo los billes repetidos para informar en EJ cuantos billetes ingresaron en la tanda actual.
                                var items1 = from i in this.Core.Bo.ExtraInfo.CashInInfo.Bills //Query principal 
                                             join j in cashInInfo.Bills on i.Value equals j.Value //Joineo 
                                             where j.Quantity > i.Quantity
                                             select new Bills(j.Currency, j.Quantity - i.Quantity, j.Id, j.Value, j.Release, j.NDCNoteID);
                                lstBillsEJ = items1.ToList();
                                //Obtengo los billetes NO repetidos
                                foreach (Bills b in cashInInfo.Bills)
                                {
                                    var existingBill = this.Core.Bo.ExtraInfo.CashInInfo.Bills.FirstOrDefault(x => x.Value == b.Value);
                                    if (existingBill == null)
                                        lstBillsEJ.Add(b);
                                }
                                //Guardo los datos de los billetes leidos *Piso todo lo anterior*
                                this.Core.Bo.ExtraInfo.CashInInfo = cashInInfo;
                            }
                        }
                        //Registro en EJ los billetes reconocidos que ingresaron en la tanda actual
                        this.WriteEJ("Recognized notes: ");
                        if (lstBillsEJ.Count != 0)
                        {
                            foreach (Bills b1 in lstBillsEJ)
                            {
                                string text = b1.Quantity == 1 ? " note  of " : " notes of ";
                                this.WriteEJ($"{b1.Quantity.ToString().PadLeft(3, ' ')}{text}{b1.Currency} {b1.Value.ToString().PadLeft(5, ' ')} release {b1.Release}");
                            }
                        }
                        //Logeo de billetes reconocidos (todas las tandas sumadas)
                        foreach (Bills b in this.Core.Bo.ExtraInfo.CashInInfo.Bills)
                        {
                            qtyNotes += b.Quantity;
                            sb.Append($"{Environment.NewLine}--> ID: {b.Id.ToString().PadLeft(3, ' ')} - CUR: {b.Currency} - QTY: {b.Quantity.ToString().PadLeft(3, ' ')} - VAL: {b.Value.ToString().PadLeft(4, ' ')} - NDC: {b.NDCNoteID}");
                        }
                        Log.Info(sb.ToString());
                        //-Verificación de Escrow Full 
                        if (qtyNotes >= this.prop.MaxNotesToAccept)
                        {
                            Log.Info($"ESCROW FULL: Max. accepted notes was reach: {this.prop.MaxNotesToAccept}");
                            this.WriteEJ($"ESCROW FULL: {this.prop.MaxNotesToAccept} NOTES");
                            this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositEscrowFull = true; //bandera para depositar automáticamente sin escrow full
                            this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull = true; //Indicador para armar mensaje a usuario de escrow full
                            if (this.prop.Extension2.AutoDeposit.Equals("001"))
                            {
                                if (!this.CashInError)
                                {
                                    this.ExecuteCashInEnd();//Envío a depositar y aguardo la confirmación  
                                    this.Core.Bo.ExtraInfo.CashInInfo.MoreAvailable = false;
                                    this.Core.Bo.ExtraInfo.CashInInfo.EnterAvailable = false;
                                    this.Core.Bo.ExtraInfo.CashInInfo.CancelAvailable = false;
                                    this.WriteEJ("Auto deposit -CashAccept-");
                                    Log.Info("AutoDeposit in progress... ");
                                }
                                else
                                    Log.Warn("AutoDeposit disable due to CashIn error");
                            }
                            else
                            {
                                Log.Info("MoreAvailable: disabled - AutoDeposit: disabled");
                                this.Core.Bo.ExtraInfo.CashInInfo.MoreAvailable = false;
                            }
                        }
                    }
                    else
                    {
                        this.Core.Bo.ExtraInfo.CashInInfo.MoreAvailable = true;
                        if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)
                            this.Core.Bo.ExtraInfo.CashInInfo.EnterAvailable = false;
                        else
                            this.Core.Bo.ExtraInfo.CashInInfo.EnterAvailable = true;
                        this.Core.Bo.ExtraInfo.CashInInfo.CancelAvailable = true;
                        Log.Warn("Bills buffer is empty");
                    }
                    //Verificación de monto mínimo
                    value = this.Core.Bo.ExtraInfo.GetTotalAmount(this.Core.Bo.ExtraInfo.Currency);
                    Log.Info("Amount Limit: {0} | Amount Deposit: {1}", this.Core.Bo.ExtraInfo.AmountLimit, value);
                    if (this.prop.MinAmountVerification && value < this.Core.Bo.ExtraInfo.AmountLimit)
                    {
                        Log.Info("insufficient amount");
                        this.Core.Bo.ExtraInfo.CashInInfo.EnterAvailable = false;
                    }
                    //--Manejo específico para MEI (Manejo de divisa única)--
                    if (this.prop.AutoCashIn)
                    {
                        Log.Info($"AutoCashIn - Recognized amount: {this.Core.Bo.ExtraInfo.Currency} {value}");
                        if (!this.CashInError && !this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull)
                        {
                            //Verificación de monto limite
                            if (cashInInfo.Bills.Count == 0)
                            {
                                Thread.Sleep(800);//FIX por bucle infinito
                                this.ExecuteDoCashIn(true);
                                Log.Info($"AutoCashIn - Bills.Count == 0");
                            }
                            else if (this.Core.Bo.ExtraInfo.AmountLimit == 0 || (this.Core.Bo.ExtraInfo.AmountLimit) > value)
                            {
                                Thread.Sleep(500);//FIX por bucle infinito
                                this.ExecuteDoCashIn(true);
                            }
                            else
                            {
                                this.Phase = Enums.Commands.Cancel;//Manejar timeOut en moreTime
                                Log.Info($"Typed amount limit: {this.Core.Bo.ExtraInfo.AmountLimit}");
                                Log.Info($"Reach the amount limit: {value}");
                                this.WriteEJ($"Typed amount limit: {this.Core.Bo.ExtraInfo.AmountLimit}");
                            }
                        }
                        else
                        {
                            Log.Warn($"CashInError: {this.CashInError} - EscrowFull: {this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull}");
                            this.CancelAvailable = false;
                        }
                        if (value > 0)//Manejo de botones, recuperación de TX y persistencia
                        {
                            this.CancelButtonAvailable = false;
                            this.Core.Bo.ExtraInfo.CashInInfo.MoreAvailable = false;
                            this.Core.Bo.ExtraInfo.CashInInfo.CancelAvailable = false;
                            this.ShowConfirmationScreen(true);//Muestro datos por pantalla
                            this.UpdateDepositCounters(jsonData);//Actualizo contadores
                        }
                        else if (this.CancelButtonAvailable) //Solo se puede cancelar sin no hubo ingresos previos de billetes validados
                        {
                            this.Core.Bo.ExtraInfo.CashInInfo.MoreAvailable = false;
                            this.Core.Bo.ExtraInfo.CashInInfo.CancelAvailable = true;
                            this.ShowConfirmationScreen(true);//Muestro datos por pantalla
                        }
                        //--Fin manejo específico para MEI (Manejo de divisa única)--
                    }
                    else
                    {
                        this.ShowConfirmationScreen(true);//Muestro datos por pantalla
                        if (rejectActive && !this.prop.AutoCashIn && qtyNotes < this.prop.MaxNotesToAccept) //Fix reject screen SNBC
                            this.ShowRejectScreen(false);
                    }
                }
                else
                {
                    if (rejectActive && !this.prop.AutoCashIn)
                        this.ShowRejectScreen(true);
                    else
                    {

                        Log.Warn("JsonData data error");
                        this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        private void ExecuteExecuteRetract()
        {
            try
            {
                Log.Debug("/--->");
                this.Phase = Enums.Commands.Retract;
                this.Core.Sdo.CIM_AsyncRetract();
                this.ShowModalAdvice(this.prop.OnRetractCashAdvice);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Cancela una operación de depósito de billetes.
        /// Muestra una pantalla que indica que se cancela la operación.
        /// Si no llegó a leer ningún billetes, no ejecuta el rollback.
        /// </summary>
        private void ExecuteRollBack(bool isCancel)
        {
            try
            {
                Log.Debug("/--->");
                this.FlagIsCancel = isCancel;
                this.Phase = Enums.Commands.RollBack;
                if (this.RollbackAvailable)
                {
                    this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositEscrowFull = false;
                    this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();//se pasa a cero el contador parcial
                    this.Core.Sdo.CIM_AsyncRollBack();
                    this.CallHandler(this.prop.OnPleaseWait);
                }
                else
                {
                    Log.Error("Rollback is disabled");
                    this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #endregion "Execute device functions"

        #region "Screen handlers"
        private void ShowInsertNotesScreen()
        {
            try
            {
                Log.Debug("/--->");
                this.CallHandler(this.prop.OnInsertNotes);
                this.Core.Bo.ExtraInfo.CashInInfo = new CashInInfo();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        private void ShowDepositCashPrepareScreen()
        {
            try
            {
                Log.Debug("/--->");
                this.CallHandler(this.prop.OnDepositCashPrepare);
                this.StartTimer(true);//Activo more time
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        /// <summary>
        /// Muestra la pantalla con el detalle de los billetes leídos que se encuentran en escrow para confirmar o rechazar la operación.
        /// </summary>
        private void ShowConfirmationScreen(bool resetMoreTimeRetry)
        {
            string[] distinctCurrencies;
            List<string> currencies = new List<string>();
            try
            {
                Log.Debug("/--->");
                if (this.prop.OnConfirmDepositedCash.Action == StateEvent.EventType.ndcScreen)
                    this.prop.OnConfirmDepositedCash.HandlerName = this.prop.Extension1.ConfirmationScreen;
                else if (this.prop.OnConfirmDepositedCash.Action == StateEvent.EventType.runScript)
                {
                    List<Bills> billsCurrency = new List<Bills>();
                    List<Bills> billsCurrency2 = new List<Bills>();
                    List<Bills> billsDistinct = new List<Bills>();
                    List<Bills> billsSorted = new List<Bills>();
                    CashInInfo cashInInfoToSort = (CashInInfo)this.Core.Bo.ExtraInfo.CashInInfo.Clone();//Evita que se modifique el objeto principal
                    CashInInfo cashInInfoToSort2 = new CashInInfo();
                    CashInInfo cashInInfoSorted = new CashInInfo();
                    //Ordenamiento por valor decreciente de valor
                    foreach (Bills b in cashInInfoToSort.Bills)
                    {
                        currencies.Add(b.Currency);//Cargo todos los currencies
                        cashInInfoToSort2.Bills.Add(b);
                    }
                    distinctCurrencies = currencies.Distinct().ToArray();

                    for (int k = 0; k < distinctCurrencies.Length; k++)
                    {
                        billsCurrency = cashInInfoToSort2.Bills.FindAll(b => b.Currency == distinctCurrencies[k]);
                        billsCurrency2 = new List<Bills>();
                        billsCurrency.ForEach(x =>
                        {
                            var b = billsCurrency2.FirstOrDefault(y => y.Value == x.Value);
                            if (b == null)
                            {
                                billsCurrency2.Add(x);
                            }
                            else
                            {
                                billsCurrency2.Remove(b);
                                billsCurrency2.Add(new Bills(x.Currency, x.Quantity + b.Quantity, x.Id, x.Value, x.Release, x.NDCNoteID));
                            }
                        });
                        //Ordenamiento de cada uno de los grupos de divisas
                        var result = billsCurrency2.GroupBy(i => i.Value)
                                     .Select(group =>
                                           new
                                           {
                                               Key = group.Key,
                                               Items = group.OrderByDescending(x => x.Value)
                                           })
                                     .Select(g => g.Items.First());
                        billsDistinct = result.ToList();
                        billsSorted = billsDistinct.OrderBy(x => x.Value).ToList();
                        cashInInfoSorted.Bills.AddRange(billsSorted);
                    }
                    if (this.prop.AutoCashIn && this.Core.Bo.ExtraInfo.CashInMultiCashData.TotalizedDeposit.total.Count > 0)
                    {
                        cashInInfoSorted.EnterAvailable = true;
                        cashInInfoSorted.CancelAvailable = false;
                    }
                    else
                    {
                        cashInInfoSorted.EnterAvailable = cashInInfoToSort.EnterAvailable;
                        cashInInfoSorted.CancelAvailable = cashInInfoToSort.CancelAvailable;
                    }
                    cashInInfoSorted.MoreAvailable = cashInInfoToSort.MoreAvailable;
                    cashInInfoSorted.EscrowFull = cashInInfoToSort.EscrowFull;
                    cashInInfoSorted.PrinterNotAvailable = this.PrinterNotAvailable;
                    cashInInfoSorted.Bills.OrderBy(x => x.Value).ToList();
                    //Muestro detalle de depósito en curso
                    this.prop.OnConfirmDepositedCash.Parameters = Utilities.Utils.JsonSerialize(cashInInfoSorted);
                    this.CallHandler(this.prop.OnConfirmDepositedCash);
                    //Muestro el detalle de los depósitos previos
                    this.ShowDepositedAmount();
                    this.PrinterNotAvailable = false; //Notifico solo 1 vez
                }
                this.StartTimer(resetMoreTimeRetry);//Activo more time
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        private void ShowDepositedAmount()
        {
            try
            {
                Log.Debug("/--->");
                //1)- Agrego monto total depositado previamente
                if (this.Core.Bo.ExtraInfo.CashInMultiCashData.TotalizedDeposit.total.Count == 0)
                {
                    RecognizedAmount recognizedAmount = new RecognizedAmount();
                    recognizedAmount.total.Add(new Values("", "0"));
                    this.prop.OnShowDepositedCash.Parameters = Utilities.Utils.JsonSerialize(recognizedAmount);
                }
                else
                {
                    RecognizedAmount depositedAmount1 = this.Core.Bo.ExtraInfo.CashInMultiCashData.GetRecognizedAmount(null);
                    this.prop.OnShowDepositedCash.Parameters = Utilities.Utils.JsonSerialize(depositedAmount1);
                }
                this.CallHandler(this.prop.OnShowDepositedCash);
                //2)- Agrego monto total depositado previamente + monto de depósito en curso
                RecognizedAmount depositedAmount2 = this.Core.Bo.ExtraInfo.CashInMultiCashData.GetRecognizedAmount(this.Core.Bo.ExtraInfo.CashInInfo);
                if (depositedAmount2.total.Count != 0)
                {
                    this.prop.OnShowRecognizedCash.Parameters = Utilities.Utils.JsonSerialize(depositedAmount2);
                    this.CallHandler(this.prop.OnShowRecognizedCash);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Muestra la pantalla para informar que quedan billetes en la zona de reject
        /// </summary>
        private void ShowRejectScreen(bool showDepositedAmount)
        {
            try
            {
                Log.Debug("/--->");

                if (showDepositedAmount) //Muestro monto total depositado previamente + monto de depósito en curso
                    this.ShowDepositedAmount();
                this.prop.OnReturningCashAdvice.Parameters = this.prop.ShowDismissButtonOnReject; //Habilito o no el boton de aceptar dependiendo el modelo
                this.CallHandler(this.prop.OnReturningCashAdvice);
                if (this.Phase == Enums.Commands.CashInStart)
                    this.StartTimer(true);//Activo more time
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
            }
        }

        private void ShowModalAdvice(StateEvent stateEvent, bool resetMoreTimeRetry = true)
        {
            Log.Debug("/--->");
            this.Core.HideScreenModals(); //Quito los avisos de pantalla
            this.CallHandler(stateEvent);
            this.StartTimer(resetMoreTimeRetry);
        }
        #endregion "Screens"

        #region "Sensors handlers"
        private bool AnalyzeMemorySensorsData()
        {
            bool ret = false;
            if (this.Core.Sdo.SOH.SensorsState.Door == false)
            {
                Log.Warn("Sensor: Door or lock open");
                this.ShowModalAdvice(this.prop.OnChestDoorOpenAdvice);
                this.WriteEJ("Chest door open");
            }
            else
            {
                if (this.Core.Sdo.SOH.SensorsState.Presence == false)
                {
                    Log.Warn("Sensor: Cassette NO present");
                    this.ShowModalAdvice(this.prop.OnCassetteNotPresentAdvice);
                    this.WriteEJ("Cassette not present");
                }
                else
                {
                    if (this.Core.Sdo.SOH.SensorsState.Cover == true)
                    {
                        Log.Warn("Sensor: Cassette Full");
                        this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFullAdvice);
                        this.WriteEJ("Cassette full");
                    }
                    else
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private bool AnalyzeSensorsState(string jsonData)
        {
            bool ret = false;
            SensorsState sensorsState = new SensorsState();
            try
            {
                sensorsState = Utils.JsonDeserialize<SensorsState>(out ret, jsonData);
                //this.WriteEJ(string.Format("Sensors state: {0}", sensorsState.ToString("\t")));
                if (ret)
                {
                    ret = false;
                    if (sensorsState.Door == false)
                    {
                        Log.Warn("Sensor: Door or lock open");
                        this.ShowModalAdvice(this.prop.OnChestDoorOpenAdvice);
                        this.WriteEJ("Chest door open");
                    }
                    else
                    {
                        if (sensorsState.Presence == false)
                        {
                            Log.Warn("Sensor: Cassette NO present");
                            this.ShowModalAdvice(this.prop.OnCassetteNotPresentAdvice);
                            this.WriteEJ("Cassette not present");
                        }
                        else//Caso de operatividad
                        {
                            if (sensorsState.Cover == true)
                            {
                                Log.Warn("Sensor: Cassette Full");
                                this.ShowModalAdvice(this.prop.OnCashAcceptCassetteFullAdvice);
                                this.WriteEJ("Cassette full");
                            }
                            else
                            {
                                if (this.Phase == Enums.Commands.UNK) //Fix para doble disparo de sensor
                                    ret = true;
                                else
                                    Log.Warn("Unexpected state");
                            }
                        }
                    }
                }
                else
                {
                    Log.Error("Inválid sensor data");
                    this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        #endregion "sensors handlers"

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
            bool enableNDCScreen = false;
            this.moreTime = new MoreTime(prop.MoreTime.MoreTimeScreenName, prop.MoreTime.MaxTimeOut,
                prop.MoreTime.MaxTimeOutRetries, prop.MoreTime.MoreTimeKeyboardEnabled, this.Core, enableNDCScreen, this.ActivityName);
            this.moreTime.EvtMoreTime += new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
        }

        private void AnalyzeMoreTimeResult(MoreTimeResult result)
        {
            this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos
            switch (result)
            {
                case MoreTimeResult.Continue:
                    switch (this.Phase)
                    {
                        case Enums.Commands.UNK:
                            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                            break;
                        case Enums.Commands.Status:
                            this.Core.Sdo.CIM_Status();
                            break;
                        case Enums.Commands.RollBack:
                            this.ShowModalAdvice(this.prop.OnRollbackCashAdvice, false);
                            break;
                        default:
                            this.CashInAvailable = true;
                            //if (this.prop.AutoCashIn && this.Phase == Enums.Commands.CashIn)//MEI
                            //{
                            //    this.Core.HideScreenModals(); //Quito los avisos de pantalla
                            //}
                            //else
                            //    this.ShowConfirmationScreen(true);
                            if (this.prop.AutoCashIn && this.Phase == Enums.Commands.CashIn)//MEI
                            {
                                if (!this.Core.Bo.ExtraInfo.CashInInfo.EscrowFull)
                                    this.ExecuteDoCashIn(false);
                                else
                                    this.ShowConfirmationScreen(true);
                            }
                            else
                                this.ShowConfirmationScreen(true);
                            break;
                    }
                    break;
                case MoreTimeResult.Cancel:
                    if (this.prop.AutoCashIn)//MEI
                    {
                        if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)//MEI
                        {
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                        }
                        else
                        {
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.ExecuteCashInEnd();
                        }
                    }
                    else
                        switch (this.Phase)
                        {
                            case Enums.Commands.UNK:
                                this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                break;
                            case Enums.Commands.RollBack:
                                if (this.prop.CashHandlingOnRetract == Const.ActionOnCashAcceptError.Retract)
                                    this.ExecuteExecuteRetract();
                                else
                                    this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                break;
                            //case Enums.Commands.CashIn:
                            //    switch (this.prop.CashHandlingOnCashInError)
                            //    {
                            //        case Const.ActionOnCashAcceptError.Eject:
                            //            this.ExecuteRollBack(true);
                            //            break;
                            //        case Const.ActionOnCashAcceptError.Reset:
                            //            this.FlagIsCancel = true;
                            //            this.CallHandler(this.prop.OnPleaseWait);
                            //            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                            //            break;
                            //        default:
                            //            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                            //            break;
                            //    }
                            //    break;
                            case Enums.Commands.CashInEnd:
                                switch (this.prop.CashHandlingOnCashInEndError)
                                {
                                    case Const.ActionOnCashAcceptError.Eject:
                                        this.ExecuteRollBack(true);
                                        break;
                                    case Const.ActionOnCashAcceptError.Reset:
                                        this.FlagIsCancel = true;
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                        break;
                                    default:
                                        this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                        break;
                                }
                                break;
                            default:
                                if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count != 0)
                                    this.ExecuteRollBack(true);//devuelve los billetes
                                else
                                {
                                    if (this.prop.ActiveResetAtCancel && !this.NotesInEscrowDetected)
                                    {
                                        this.FlagIsCancel = true;
                                        this.CallHandler(this.prop.OnPleaseWait);
                                        this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                    }
                                    else
                                        this.SetActivityResult(StateResult.CANCEL, this.prop.Extension2.CancelNextStateNumber);
                                }
                                break;
                        }
                    break;
                case MoreTimeResult.Timeout:
                    if (this.CashInError && !this.prop.AutoCashIn)
                    {
                        this.ExecuteRollBack(false);
                    }
                    else
                    {
                        if (this.prop.AutoCashIn)//MEI
                        {
                            if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)//MEI
                            {
                                this.CallHandler(this.prop.OnPleaseWait);
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                            }
                            else
                            {
                                this.CallHandler(this.prop.OnPleaseWait);
                                this.ExecuteCashInEnd();
                            }
                        }
                        else
                            switch (this.Phase)
                            {
                                case Enums.Commands.UNK:
                                    this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                                    break;
                                case Enums.Commands.RollBack:
                                    if (this.prop.CashHandlingOnRetract == Const.ActionOnCashAcceptError.Retract)
                                        this.ExecuteExecuteRetract();
                                    else
                                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                                    break;
                                case Enums.Commands.CashInEnd:
                                    switch (this.prop.CashHandlingOnCashInEndError)
                                    {
                                        case Const.ActionOnCashAcceptError.Eject:
                                            this.ExecuteRollBack(true);
                                            break;
                                        case Const.ActionOnCashAcceptError.Reset:
                                            this.CallHandler(this.prop.OnPleaseWait);
                                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                            break;
                                        default:
                                            this.SetActivityResult(StateResult.HWERROR, this.prop.Extension2.DeviceErrorNextStateNumber);
                                            break;
                                    }
                                    break;
                                default:
                                    if (this.Core.Bo.ExtraInfo.CashInInfo.Bills.Count == 0)
                                    {
                                        if (this.prop.ActiveResetAtCancel && !this.NotesInEscrowDetected)
                                        {
                                            this.FlagIsCancel = true;
                                            this.CallHandler(this.prop.OnPleaseWait);
                                            this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                        }
                                        else
                                            this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                                    }
                                    else
                                    {
                                        if (this.Phase != Enums.Commands.OpenEscrowShutter)
                                            if (this.prop.Extension2.OperationMode.Equals("001"))
                                                this.ProcessDeposit(); //Deposita los billetes
                                            else
                                                this.ExecuteRollBack(true);
                                        else
                                            this.SetActivityResult(StateResult.TIMEOUT, this.prop.Extension2.TimeOutNextStateNumber);
                                    }
                                    break;
                            }
                    }
                    break;
            }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer(bool resetMoreTimeRetry)
        {
            Log.Debug($"/--->ResetRetry: {resetMoreTimeRetry}");
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            this.timeout = false;
            if (resetMoreTimeRetry)
                this.moreTime.ResetRetry();
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            Log.Debug("/--->");
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

        private void ResetTimer()
        {
            if (this.timerScreen != null)
            {
                this.timerScreen.Stop();
                this.timerScreen.Start();
            }
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
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
            if (this.prop.AutoCashIn)//MEI
            {
                if (this.Phase == Enums.Commands.CashIn)
                    this.Core.Sdo.CIM_AsyncCancel();
            }
        }

        #endregion "More time"

        private void AddEventHandlers()
        {
            this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive += new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive += new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive += new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCompletionReceive);
            this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
            this.Core.Sdo.EvtAckReceive -= new SDO.DelegateAckReceive(this.HandlerAckReceive);
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Info($"State result: {result.ToString()}");
                this.StopTimer();
                this.ActivityResult = result;
                this.NextState = nextState;
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                if (this.Phase != Enums.Commands.UNK)
                {
                    if (!this.Core.Sdo.DevConf.CIMconfig.KeepConnectionOpen)
                        this.Close_CIM();
                    else
                    {
                        this.Quit();
                        this.Core.SetNextState(this.ActivityResult, this.NextState);
                    }
                }
                else
                {
                    this.Quit();
                    this.Core.SetNextState(this.ActivityResult, this.NextState);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Solo se utiliza para cerrar el estado en caso de que no haya conexión con el DM
        /// </summary>
        /// <param name="result"></param>
        /// <param name="nextState"></param>
        public void SetActivityResult2(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.StopTimer();
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
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
                if (this.prop.UpdateConfigurationFile)//MEI
                {
                    if (!this.Core.WriteIniConfigFileAsync(true, true).GetAwaiter().GetResult())
                        Log.Error("Write config file error");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
    }
}
