using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Business.VerifyNotesState
{
    public class VerifyNotesState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        VerifyNotesStateTableData_Type VerifyNotesStateTableData; //Tabla con datos provenientes del download.
        PropertiesVerifyNotesState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();
        private bool CashInEnable = false;
        private string NextState;
        private bool CancelFlag = false;
        private bool FlagUniqueReset = true; //Bandera para que se realice solo un reset por sesión de depósitos
        private bool ShowInputNotes = true; //Indica que es la primera ejecución de CashIn

        #region "Constructor"
        public VerifyNotesState(StateTable_Type stateTable)
        {
            this.ActivityName = "VerifyNotesState";
            this.VerifyNotesStateTableData = (VerifyNotesStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesVerifyNotesState();
            this.prop = this.GetProperties<PropertiesVerifyNotesState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.VerifyNotesStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.NextStateNumber))
                    this.prop.NextStateNumber = this.VerifyNotesStateTableData.NextStateNumber;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.VerifyNotesStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.VerifyNotesStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.VerifyNotesStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.VerifyNotesStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.VerifyNotesStateTableData.Item2;
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
                this.ListOfAck = new List<string>();
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                this.AddEventHandlers();
                this.FlagUniqueReset = true;
                this.ShowInputNotes = true;
                Thread.Sleep(100);
                this.Core.Bo.ExtraInfo.Amount = 0;
                this.CallHandler(this.prop.OnShowScreen);
                this.Open_CIM();
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
                Log.Info($"/--> Cim event: {dm.Payload.ToString()}");
                switch (dm.Payload.ToString())
                {
                    case "WFS_SRVE_CIM_ITEMSINSERTED":
                        this.CallHandler(this.prop.OnCashInsertedAdvice);
                        break;
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
                this.ProcessCompletion(dm);
            else
                Log.Error($"/-->ACK request ID: {dm.Header.RequestId} not found");
        }

        /// <summary>
        /// Procesa los retornos tipo Completion de los dispositivos
        /// </summary>
        /// <param name="func"></param>
        /// <param name="data"></param>
        private void ProcessCompletion(DeviceMessage dm)
        {
            Completion cr;
            bool ret = false;
            try
            {
                cr = (Completion)dm.Payload;
                //Logeo resultado
                if (cr.CompletionCode == CompletionCodeEnum.Success)
                    Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                else
                    Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {cr.CompletionCode}");
                switch (dm.Command)//Switcheo respuestas de los comandos
                {
                    case Enums.Commands.Open:
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                        {
                            this.Core.Sdo.CIM_AsyncCashInStart();
                        }
                        else
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                            this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                        }
                        break;
                    case Enums.Commands.Status:
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                        {
                            StatusCIM statusCIM = Utilities.Utils.JsonDeserialize<StatusCIM>(out ret, cr.Data);
                            if (ret)
                            {
                                ret = false;
                                if (statusCIM.IntermediateStacker.Equals("0") || statusCIM.IntermediateStacker.Equals("5"))//A)- Verifica si hay valores en escrow
                                    if (statusCIM.Device.Equals("0") && statusCIM.Acceptor.Equals("0")) //B)- Verifico si el dispositivo esta ok
                                    {
                                        ret = true;
                                        this.Core.Sdo.CIM_AsyncCashInStart();
                                    }
                            }
                        }
                        if (!ret)
                        {
                            this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                            this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                        }
                        break;
                    case Enums.Commands.CashInStart:
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                        {
                            this.CashInEnable = true;
                            //this.StopTimer();
                            if (this.ShowInputNotes)
                            {
                                this.ShowInputNotes = false;
                                Thread.Sleep(200);
                                this.CallHandler(this.prop.OnDepositCashPrepare);
                            }
                            this.Core.Sdo.CIM_AsyncCashIn();
                        }
                        else
                        {
                            if (this.FlagUniqueReset)
                            {
                                this.Core.Sdo.CIM_AsyncReset(); //Se envía un reset
                                this.FlagUniqueReset = false;
                                this.CancelFlag = false;
                            }
                            else
                                this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                        }
                        break;
                    case Enums.Commands.CashIn:
                        this.CashInEnable = false;
                        if (cr.CompletionCode == CompletionCodeEnum.TimeOut)
                        {
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.Core.Sdo.CIM_AsyncCancel();
                        }
                        else if (cr.CompletionCode == CompletionCodeEnum.Canceled)
                        {
                            this.CancelFlag = true;
                            this.CallHandler(this.prop.OnPleaseWait);
                            this.Core.Sdo.CIM_AsyncReset();
                        }
                        else
                            this.HandleDoCashInResponse(cr.Data, cr.CompletionCode);
                        break;
                    case Enums.Commands.Reset:
                        if (this.CancelFlag)
                            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        else
                        {
                            this.Core.Sdo.CIM_AsyncCashInStart();
                            this.ShowInputNotes = true;
                        }
                        break;
                    case Enums.Commands.RollBack:
                        if (cr.CompletionCode == CompletionCodeEnum.Success)
                            this.Core.Sdo.CIM_AsyncCashInStart();
                        else
                            this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber);
                        break;
                    case Enums.Commands.Close:
                        this.Quit();
                        this.Core.SetNextState(this.ActivityResult, this.NextState);
                        break;
                }
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
        private void HandleDoCashInResponse(string jsonData, CompletionCodeEnum completionCode)
        {
            StringBuilder sb = new StringBuilder();
            bool ret = false;
            CashInInfo cashInInfo;
            try
            {
                Log.Debug("/--->");
                this.Core.HideScreenModals(); //Quito los avisos de pantalla
                cashInInfo = Utilities.Utils.JsonDeserialize<CashInInfo>(out ret, jsonData);
                if (cashInInfo == null)
                {
                    cashInInfo = new CashInInfo();
                }
                cashInInfo.MoreAvailable = false;
                cashInInfo.EnterAvailable = false;
                cashInInfo.CancelAvailable = true;
                if (cashInInfo.Bills.Count != 0)//Lectura OK
                {
                    this.Core.Sdo.CIM_RollBack();
                }
                else
                {
                    if (completionCode == CompletionCodeEnum.HardwareError)
                    {
                        this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
                    }
                    else
                    {
                        this.Core.Sdo.CIM_AsyncCashIn();//Reject
                        this.CashInEnable = true;
                    }
                }
                this.prop.OnConfirmDepositedCash.Parameters = Utilities.Utils.JsonSerialize(cashInInfo);
                this.CallHandler(this.prop.OnConfirmDepositedCash);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                this.SetActivityResult(StateResult.SWERROR, this.prop.HardwareErrorNextStateNumber);
            }
        }

        private void Open_CIM()
        {
            try
            {
                if (this.prop.UpdateConfigurationFile)
                {
                    if (this.Core.WriteIniConfigFileAsync(true, true).GetAwaiter().GetResult())
                    {
                        Thread.Sleep(100);
                        this.Core.Sdo.CIM_AsyncOpen();
                    }
                    else
                        Log.Error("Write config file error");
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
                if (this.prop.UpdateConfigurationFile)
                    if (!this.Core.WriteIniConfigFileAsync(false, true).GetAwaiter().GetResult())
                        Log.Error("Write config file error");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "Returns from SDO"

        private void HandlerFDKreturn(string FDKcode) //Todas las FDK cancelan la operación
        {
            try
            {
                Log.Info($"/--> Key press: {FDKcode}");
                this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {
            Log.Info($"/--> Key press: {othersKeys}");
            switch (othersKeys)
            {
                case "CANCEL":
                    {
                        this.CancelFlag = true;
                        this.CallHandler(this.prop.OnPleaseWait);
                        if (this.CashInEnable)
                        {
                            this.Core.Sdo.CIM_AsyncCancel();
                        }
                        else
                        {
                            this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        }
                        break;
                    }
            }
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                Log.Info($"State result: {result.ToString()}");
                //this.StopTimer();
                this.ActivityResult = result;
                this.NextState = nextState;
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Close_CIM(); //Cierro CIM y espero la respuesta para hacer el NextState
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
            this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        //if (this.CashInEnable)
                        //{
                        //    this.CallHandler(this.prop.OnDepositCashPrepare);
                        //    this.Core.Sdo.CIM_AsyncCashIn();
                        //    this.StopTimer();
                        //}
                        //else
                        //    this.Core.Sdo.CIM_AsyncCashInStart();
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber);
                        break;
                    }
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

        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            //this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
            //if (this.CashInEnable)
            //{
            //    this.Core.Sdo.CIM_AsyncCancel();
            //}
        }
        #endregion "More time"
    }
}
