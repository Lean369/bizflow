using Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Business
{
    public class State
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");

        public bool EnableJournal = true;

        public bool EnablePrintProperties = false;

        protected Core Core { get; set; }

        public State() { }

        public string GetScreenPath(string screenName)
        {
            string screenHtmPath = "";
            try
            {
                if (this.Core.ScreenConfiguration.UrlScreenEnable)
                {
                    screenHtmPath = $"{this.Core.ScreenConfiguration.UrlScreen}{screenName}";
                    Log.Info($"Screen Path: {screenHtmPath}");
                }
                else
                {
                    screenHtmPath = $"{Const.appPath}Screens\\{screenName}";
                    if (File.Exists(screenHtmPath))
                    {
                        Log.Info($"Screen Path: \"{screenHtmPath}\"");
                    }
                    else
                    {
                        Log.Error($"Screen {screenName} not found");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return screenHtmPath;
        }

        internal void PrintProperties(object obj, string stateNumber, string activityName)
        {
            StringBuilder sb = new StringBuilder();
            if (this.EnablePrintProperties)
            {
                this.EnablePrintProperties = false;
            }
        }

        protected void GetPerformaceData()
        {
            try
            {
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                Log.Info($"System performace: Available RAM: {ramCounter.NextValue()} MB; CPU used: {cpuCounter.NextValue()} %");
                this.GetProcessByName("AlephATM");
                this.GetProcessByName("AlephDEV");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void GetProcessByName(string processName)
        {
            Process[] processes;
            processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                Log.Error($"{processName} process doesn´t exist");
            }
            else
            {
                if (processes.Length > 1)
                {
                    Log.Error($"More than one {processName} process in execution");
                }
                else
                {
                    Process p = processes[0];
                    PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set", p.ProcessName);
                    PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);
                    double ram = ramCounter.NextValue();
                    double cpu = cpuCounter.NextValue();
                    Log.Info($"{processName} performance: RAM: {ram / 1024 / 1024} MB; CPU: {cpu} %");
                }
            }
        }

        protected void WriteEJ(string dataToWrite)
        {
            if (this.EnableJournal)
                GlobalAppData.Instance.WriteEJ(dataToWrite);
        }

        protected bool CallHandler(StateEvent stateEvent)
        {
            bool ret = false;
            KeyMask_Type keyMask = null;
            Log.Info($"/===> CallHandler: Action = {stateEvent.Action.ToString()} - HandlerName = {stateEvent.HandlerName}");
            switch (stateEvent.Action)
            {
                case StateEvent.EventType.runScript:
                    Log.Info($"/===> Parameters = {stateEvent.Parameters}");
                    this.Core.RaiseEvtScreenData(stateEvent);
                    ret = true;
                    break;
                case StateEvent.EventType.navigate:
                    Log.Info($"/===> Parameters = {stateEvent.Parameters}");
                    StateEvent se = stateEvent.Clone();
                    se.HandlerName = this.GetScreenPath(stateEvent.HandlerName);
                    this.Core.RaiseEvtScreenData(se);
                    ret = true;
                    break;
                case StateEvent.EventType.ndcScreen:
                    if (stateEvent.Parameters is Entities.KeyMask_Type)
                        keyMask = (KeyMask_Type)stateEvent.Parameters;
                    ret = this.Core.ShowGeneralNDCScreen(stateEvent.HandlerName, keyMask);
                    break;
                case StateEvent.EventType.printReceipt:
                    ret = PrintTicket(stateEvent.Parameters.ToString(), true, false, false);
                    break;
                case StateEvent.EventType.printJournalAndReceipt:
                    ret = PrintTicket(stateEvent.Parameters.ToString(), true, true, false);
                    break;
                case StateEvent.EventType.printJournal:
                    ret = PrintTicket(stateEvent.Parameters.ToString(), false, true, false);
                    break;
                case StateEvent.EventType.sendTicketToBD:
                    ret = PrintTicket(stateEvent.Parameters.ToString(), false, false, true);
                    break;
                case StateEvent.EventType.sendToHost:
                    Log.Info($"/===> Parameters = {stateEvent.Parameters}");
                    Enums.DeviceStatus errorType = (Enums.DeviceStatus)stateEvent.Parameters;
                    ret = this.Core.Sdo.SendDeviceStatus(errorType);
                    break;
                case StateEvent.EventType.ignore:
                    ret = true;
                    break;
            }
            if (!ret)
                Log.Error("/===> CallHandler {0} process error", stateEvent.HandlerName);
            return ret;
        }

        private bool PrintTicket(string ticketData, bool printReceipt, bool printJournal, bool sendToBD)
        {
            bool ret = true;
            try
            {
                if (printReceipt)
                    this.Core.Sdo.PTR_PrintRawData(ticketData);
                if (printJournal)
                    this.WriteEJ($"Ticket: {ticketData}");
                if (sendToBD)
                {
                    Entities.TicketLines tl = new Entities.TicketLines(ticketData);
                    string jsonData = Utilities.Utils.JsonSerialize(tl.Lines);
                    AuthorizationResult authorizationResult = this.Core.SendTicketData(jsonData, this.Core.Counters.GetTSN());
                    if (authorizationResult != null && authorizationResult.authorizationStatus != AuthorizationStatus.Authorized)
                        ret = false;
                }
            }
            catch (Exception ex) { throw ex; }
            return ret;
        }

        public void ProcessPrinterData(StateEvent stateEvent, bool directPrint)
        {
            Printers.PrintFormat pf;
            string ticketData;
            try
            {
                if (stateEvent.Action != StateEvent.EventType.ignore)
                {
                    pf = new Printers.PrintFormat(this.Core);
                    ticketData = pf.GetTicketData(stateEvent.HandlerName);
                    if (stateEvent.Action == StateEvent.EventType.sendTicketToBD || directPrint)
                    {
                        this.CallHandler(stateEvent.Clone(ticketData));
                    }
                    else
                    {
                        if (this.Core.Bo.ExtraInfo.LstPrintData == null)
                            this.Core.Bo.ExtraInfo.LstPrintData = new List<StateEvent>();
                        this.Core.Bo.ExtraInfo.LstPrintData.Add(stateEvent.Clone(ticketData));
                    }
                }
                else
                {
                    Log.Info($"Print ticket template {stateEvent.HandlerName} ignored");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void ChangeDEV_Supplies(Enums.Devices device, Const.Supplies supplies, Enums.DeviceStatus internalCode)
        {
            SDO_DeviceState CIM_State = this.Core.Sdo.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == device);
            if (CIM_State != null)
            {
                CIM_State.InternalCode = internalCode;
                CIM_State.Supplies = supplies;
            }
            else
                Log.Fatal($"Device {device} is null");
        }

        public void ChangeDEV_Fitness(Enums.Devices device, Const.Fitness fitness, Enums.DeviceStatus internalCode, string detail = null)
        {
            SDO_DeviceState DEV_State = this.Core.Sdo.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == device);
            if (DEV_State != null)
            {
                DEV_State.InternalCode = internalCode;
                DEV_State.Fitness = fitness;
                //about error details
                if (fitness == Const.Fitness.NoError) //reset details on no error
                    DEV_State.Details = null;
                else if (detail != null)
                {
                    if (DEV_State.Details == null)
                        DEV_State.Details = new System.Collections.Generic.List<string> { detail };
                    else
                        DEV_State.Details.Add(detail);
                } //end details
            }
            else
                Log.Fatal($"Device {device} is null");
        }

        internal bool GetPrinterState(Completion cr)
        {
            bool isPrinterError = false;
            if (cr.CompletionCode != CompletionCodeEnum.Success)
                isPrinterError = true;
            else
            {
                if (cr.ErrorDescription.Equals("ERROR") || cr.ErrorDescription.Equals("STATUS_MISSING"))
                    isPrinterError = true;
                else if (cr.ErrorDescription.Equals("WARNING"))
                {
                    this.ChangeDEV_Supplies(Enums.Devices.Printer, Const.Supplies.MediaLow, Enums.DeviceStatus.PRT_PaperLow);//Envía el status a host de printer fail
                    this.WriteEJ("Receipt printer: paper low");
                    Log.Warn("Printer state paper: low");
                }
                else//Printer OK
                {
                    this.ChangeDEV_Supplies(Enums.Devices.Printer, Const.Supplies.GoodState, Enums.DeviceStatus.PRT_DeviceSuccess);//Envía el status a host de printer OK
                    this.ChangeDEV_Fitness(Enums.Devices.Printer, Const.Fitness.NoError, Enums.DeviceStatus.PRT_DeviceSuccess);//Envía el status a host de printer OK
                    this.WriteEJ("Receipt printer: good state");
                    Log.Info("Printer state: good state");
                }
            }
            if (isPrinterError)
            {
                this.ChangeDEV_Fitness(Enums.Devices.Printer, Const.Fitness.Fatal, Enums.DeviceStatus.PRT_DeviceError);//Envía el status a host de printer fail
                this.WriteEJ("Receipt printer: error");
                Log.Error("Printer state: error");
            }
            return isPrinterError;
        }
    }
}