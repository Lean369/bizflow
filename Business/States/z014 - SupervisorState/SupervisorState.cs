using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Business.SupervisorState
{
    public class SupervisorState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private System.Timers.Timer TimerClearKeyBuffer;
        private System.Timers.Timer TimerForcedExit;
        private string KeyBuffer = string.Empty;
        PropertiesSupervisorState prop;
        private ModulesVerifier modulesVerifier;

        #region "Constructor"
        public SupervisorState(StateTable_Type stateTable)
        {
            bool ret = false;
            this.ActivityName = "SupervisorState";
            this.prop = new PropertiesSupervisorState();
            this.prop = this.GetProperties<PropertiesSupervisorState>(out ret, this.prop);
            if (!ret)
                Log.Error($"->Can´t get properties of Activity: {this.ActivityName}");
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

                modulesVerifier = new ModulesVerifier(this.Core);
                modulesVerifier.SubscribeCompletion();
                modulesVerifier.ChangeFitnessEvt += this.ChangeDEV_Fitness;
                modulesVerifier.ChangeSuppliesEvt += this.ChangeDEV_Supplies;
                modulesVerifier.NotifyDependenciesStateEvt += this.DependencyCheckComplation;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override void ActivityStart()
        {
            try
            {
                Log.Debug("/--->");
                this.Core.Sdo.SOH.Mode = Const.TerminalMode.InSupervisor;
                this.EnableJournal = prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{base.Core.AlephATMAppData.StateSupervisor}] {ActivityName}");
                this.Core.EvtFDKscreenPress += HandlerFDKreturn;
                this.TimerClearKeyBuffer = new System.Timers.Timer(50000);
                this.TimerClearKeyBuffer.Elapsed += TimerClearKeyBuffer_Elapsed;
                this.TimerClearKeyBuffer.Enabled = true;
                this.TimerForcedExit = new System.Timers.Timer(60000); //Forzado de salida de Supervisor
                this.TimerForcedExit.Elapsed += TimerForcedExit_Elapsed;
                this.TimerForcedExit.Enabled = false;
                this.Core.Sdo.EvtCompletionReceive += this.HandlerCashInDataReceive;
                this.Core.Sdo.EvtEventReceive += this.HandlerEventReceive;
                this.CallHandler(this.prop.OnSupervisorStart);
                if (base.Core.AlephATMAppData.SupervisorAppEnabled)
                {
                    if (!this.Core.ScreenConfiguration.SecBrowserEnable)
                    {
                        if (File.Exists(base.Core.AlephATMAppData.SupervisorAppPath))
                        {
                            this.Core.KillExternalApp(base.Core.AlephATMAppData.SupervisorAppPath);
                            this.Core.StartExternalApp(base.Core.AlephATMAppData.SupervisorAppPath, base.Core.AlephATMAppData.SupervisorWindowStyle);
                        }
                        else { Log.Warn($"File \"{base.Core.AlephATMAppData.SupervisorAppPath}\" not found."); }
                    }
                }
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        private void HandlerFDKreturn(string FDKdata)
        {
            try
            {
                if (FDKdata.Equals("B"))
                {
                    this.KeyBuffer += FDKdata;
                    if (this.KeyBuffer.Length > 3)
                    {
                        this.ExitSupervisor();
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ExitSupervisor()
        {
            try
            {
                Log.Debug("/--->");
                this.CallHandler(this.prop.OnPleaseWait); //Coloco un modal de pantalla de espera
                if (this.prop.ResetCimAtExit && this.Core.Sdo.DevConf.CIMconfig.Enable)
                {
                    //this.Core.Sdo.CIM_AsyncOpen();
                    this.Core.Sdo.CIM_Init();
                    this.TimerForcedExit.Enabled = true;
                }
                else
                    //this.SetActivityResult(StateResult.SUCCESS, "");
                    VerifyModules();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerEventReceive(DeviceMessage dm)
        {
            CommandData commandData;
            bool ret = false;
            if (dm.Header.Type == Types.Unsolicited && dm.Device == Enums.Devices.Terminal)
            {
                if (dm.Command == Enums.Commands.OutOfSupervisor)
                {
                    commandData = Utilities.Utils.NewtonsoftDeserialize<CommandData>(out ret, dm.Payload.ToString());
                    if (commandData.Data.Equals("Halt"))
                    {
                        this.ExitSupervisor();
                    }
                    else
                    {
                        if (!this.Core.ScreenConfiguration.SecBrowserEnable)
                            this.KillSupervisorApp();
                        else
                            Log.Error("SecBrowser is enabled");
                    }

                }
            }
        }

        private void HandlerCashInDataReceive(DeviceMessage dm)
        {
            try
            {
                Completion completion = (Completion)dm.Payload;
                if (dm.Device == Enums.Devices.CashAcceptor && this.TimerForcedExit.Enabled && dm.Command == Enums.Commands.Init)
                {
                    if (completion.CompletionCode == CompletionCodeEnum.Success)
                    {
                        Log.Info($"Dev: {dm.Device} Func: {dm.Command} Result: {completion.CompletionCode}");
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.NoError, Enums.DeviceStatus.CIM_DeviceSuccess);//Envía el status a host de error de CIM
                        //SetActivityResult(StateResult.SUCCESS, "");
                        VerifyModules();
                    }
                    else
                    {
                        Log.Warn($"Dev: {dm.Device} Func: {dm.Command} Result: {completion.CompletionCode}");
                        this.ChangeDEV_Fitness(Enums.Devices.CashAcceptor, Const.Fitness.Fatal, Enums.DeviceStatus.CIM_DeviceError);//Envía el status a host de error de CIM
                        this.WriteEJ("Init CIM Error");
                        SetActivityResult(StateResult.HWERROR, "");
                    }
                }
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        private void TimerClearKeyBuffer_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            this.KeyBuffer = string.Empty;
        }

        private void TimerForcedExit_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            Log.Debug("/--->");
            this.SetActivityResult(StateResult.TIMEOUT, "");
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug($"Activity result: {result.ToString()}");
                this.Quit();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.Core.Sdo.EvtCompletionReceive -= new SDO.DelegateCompletionReceive(this.HandlerCashInDataReceive);
                this.Core.Sdo.EvtEventReceive -= new SDO.DelegateEventReceive(this.HandlerEventReceive);
                this.TimerForcedExit.Elapsed -= new System.Timers.ElapsedEventHandler(this.TimerForcedExit_Elapsed);
                this.CurrentState = ProcessState.FINALIZED;
                this.Core.HideScreenModals(); //Quito los avisos de pantalla
                this.GoToOutOfSupervisorMode();
                this.modulesVerifier.UnsubscribeCompletion();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void GoToOutOfSupervisorMode()
        {
            try
            {
                Log.Debug("/--->");
                this.Core.Sdo.SOH.Mode = Const.TerminalMode.OutOfSupervisor;
                WriteEJ(string.Format("==> Supervisor mode exit"));
                if (!this.Core.ScreenConfiguration.SecBrowserEnable)
                    this.KillSupervisorApp();
                if (this.Core.AlephATMAppData.OperationMode == Const.OperationMode.Batch)
                    this.Core.RequestChangeMode(Const.TerminalMode.InService);
                else
                    this.Core.RequestChangeMode(Const.TerminalMode.OutOfService);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void KillSupervisorApp()
        {
            try
            {
                Log.Debug("/--->");
                if (this.Core.AlephATMAppData.SupervisorAppEnabled)
                    if (File.Exists(this.Core.AlephATMAppData.SupervisorAppPath))
                        this.Core.KillExternalApp(this.Core.AlephATMAppData.SupervisorAppPath);
                    else
                        Log.Warn($"File \"{this.Core.AlephATMAppData.SupervisorAppPath}\" not found.");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void VerifyModules()
        {
            UpdateContentObject();

            var devices = new List<Enums.Devices>();
            if (this.Core.Sdo.DevConf.CIMconfig.Enable)
                devices.Add(Enums.Devices.CashAcceptor);
            if (this.Core.Sdo.DevConf.CDMconfig.Enable)
                devices.Add(Enums.Devices.CashDispenser);
            if (this.Core.Sdo.DevConf.COINconfig.Enable)
                devices.Add(Enums.Devices.CoinDispenser);
            if(devices.Any())
                modulesVerifier.Verify(devices);
            else
                SetActivityResult(StateResult.SUCCESS, "");
        }
        private void UpdateContentObject()
        {
            int logicalFullBinThreshold = this.Core.Sdo.DevConf.BAGconfig != null ? this.Core.Sdo.DevConf.BAGconfig.BagCapacity : 0;
            if(!this.Core.Counters.GetInitialCounters(out this.Core.Counters, logicalFullBinThreshold))
                Log.Error("Counter object could not be updated after supervisor mode exit.");
        }

        private void DependencyCheckComplation(List<UnitDependency> unitDependencyList)
        {
            //var blockIfError = new[] { Enums.Devices.CashAcceptor, Enums.Devices.CashDispenser, Enums.Devices.CoinDispenser };
            if (unitDependencyList.All(d => d.Status == UnitDependency.DepStatus.OK))
            {
                SetActivityResult(StateResult.SUCCESS, "");
            }
            else
            {
                SetActivityResult(StateResult.HWERROR, "");
            }
        }

    }
}
