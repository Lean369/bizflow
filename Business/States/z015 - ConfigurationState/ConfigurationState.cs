using Entities;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Utilities;
using static Entities.Enums;

namespace Business.ConfigurationState
{
    public class ConfigurationState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        //private bool Phase1 = true;
        ConfigurationStateTableData_Type ConfigurationStateTableData; //Tabla con datos provenientes del download.
        PropertiesConfigurationState prop;
        bool ret = false;
        AlephHost alephHost;
        private bool MoreTimeSubscribed = false;
        List<CashInAcceptedNotes> acceptedNotes;
        ExternalApps extAppMap = new ExternalApps();

        #region "Constructor"
        public ConfigurationState(StateTable_Type stateTable)
        {
            this.ActivityName = "ConfigurationState";
            this.ConfigurationStateTableData = (ConfigurationStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesConfigurationState();
            this.prop = this.GetProperties<PropertiesConfigurationState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.ConfigurationStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.ExitNextStateNumber))
                    this.prop.ExitNextStateNumber = this.ConfigurationStateTableData.ExitNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.ConfigurationStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.ErrorNextStateNumber))
                    this.prop.ErrorNextStateNumber = this.ConfigurationStateTableData.ErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Item1))
                    this.prop.Item1 = this.ConfigurationStateTableData.Item1;
                if (string.IsNullOrEmpty(this.prop.Item2))
                    this.prop.Item2 = this.ConfigurationStateTableData.Item2;
                if (string.IsNullOrEmpty(this.prop.Item3))
                    this.prop.Item3 = this.ConfigurationStateTableData.Item3;
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
                this.CurrentState = ProcessState.INPROGRESS;
                this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
                this.alephHost = this.Core.GetHostObject(this.prop.HostName);
                if (alephHost != null)
                {
                    this.EnableJournal = this.prop.Journal.EnableJournal;
                    this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                    if (this.CallHandler(this.prop.OnConfigurationStart))
                    {
                        this.prop.OnConfigurationOptions.Parameters = this.GetConfigurationOptions();
                        this.CallHandler(this.prop.OnConfigurationOptions);
                        this.prop.OnConfigurationLoad.Parameters = this.GetCurrentConfiguration();
                        this.CallHandler(this.prop.OnConfigurationLoad);
                        this.StartTimer();
                    }
                    else
                        this.SetActivityResult(StateResult.SWERROR, this.prop.ErrorNextStateNumber); ;
                }
                else
                {
                    Log.Error("Host is null");
                    this.SetActivityResult(StateResult.SWERROR, this.prop.ErrorNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region Receive configuration data
        /// <summary>
        /// Función que recibe los datos para cambiar la configuración de la aplicación
        /// </summary>
        /// <param name="dataInput"></param>
        /// <param name="dataLink"></param>
        private void HandlerInputData(string dataInput, string dataLink)
        {
            string channel = string.Empty;
            List<ExtraDataInfo> extraDataInfo = new List<ExtraDataInfo>();
            bool res = false;
            this.ResetTimer();
            string[] config;
            try
            {
                Log.Info($"-> Input data: {dataInput}");
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "ConfigData":
                            Dictionary<string, object> extraDataResponse = Utils.NewtonsoftDeserialize<Dictionary<string, object>>(out res, dataInput);
                            if (res)
                            {
                                foreach (KeyValuePair<string, object> entry in extraDataResponse)
                                {
                                    // do something with entry.Value or entry.Key
                                    config = entry.Key.ToString().Split('.');
                                    if (config.Length >= 2)
                                    {
                                        switch (config[0]) //devices
                                        {
                                            case "helpDesk":
                                                this.SetHelpDeskValues(extraDataResponse);
                                                break;
                                            case "externalApps":
                                                this.ProcessExternalApps(extraDataResponse[entry.Key].ToString());
                                                break;
                                            case "application":
                                                this.ProcessApplicationExtraData(extraDataResponse);
                                                break;
                                            case "devices":
                                                this.SetParameters(extraDataResponse[entry.Key].ToString(), config[0], config[1], config[2]);
                                                break;
                                            case var option when option.StartsWith("denominations"):
                                                var denomindex = int.Parse(config[0].Substring(13).Trim('[', ']'));
                                                this.SetDenominationActivation(this.acceptedNotes[denomindex], bool.Parse(extraDataResponse[entry.Key].ToString()));
                                                break;
                                            case "terminalInfo":
                                                switch (config[1])
                                                {
                                                    case "machineID":
                                                        this.SetMachineID(extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "address":
                                                        Utils.UpdateXmlElement($@"{Const.appPath}Config\\TerminalInfo.xml", "TerminalInfo/Address", extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "city":
                                                        Utils.UpdateXmlElement($@"{Const.appPath}Config\\TerminalInfo.xml", "TerminalInfo/City", extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "phone":
                                                        Utils.UpdateXmlElement($@"{Const.appPath}Config\\TerminalInfo.xml", "TerminalInfo/Phone", extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "model":
                                                    case "branding":
                                                        this.SetParameters(extraDataResponse[entry.Key].ToString(), config[0], config[1], "");
                                                        break;
                                                    default:
                                                        Log.Error("Unknown terminalInfo setting");
                                                        break;
                                                }
                                                break;
                                            case "system":
                                                switch (config[1])
                                                {
                                                    case "ipV4Address":
                                                        this.SetEthernetAddresses(extraDataResponse[entry.Key].ToString(), "", "");
                                                        break;
                                                    case "subNetMask":
                                                        this.SetEthernetAddresses("", extraDataResponse[entry.Key].ToString(), "");
                                                        break;
                                                    case "defaultGateway":
                                                        this.SetEthernetAddresses("", "", extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "computerName":
                                                        this.SetComputerName(extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "timeZone":
                                                        this.SetSystemTimeZone(extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "region":
                                                        this.SetSystemRegion(extraDataResponse[entry.Key].ToString());
                                                        break;
                                                    case "extraData":
                                                        this.ProcessApplicationExtraData(extraDataResponse);
                                                        break;
                                                    case "terminalReset":
                                                        Process.Start("shutdown.exe", "-r -t 3");
                                                        break;
                                                    default:
                                                        Log.Error("Unknown system setting");
                                                        break;
                                                }
                                                break;
                                            default:
                                                Log.Error("Unknown element setting");
                                                break;
                                        }
                                    }
                                    else
                                        Log.Error("Incorrect lenght");
                                }
                            }
                            else
                                Log.Error("ExtraData empty");
                            break;
                    }
                }
                this.timerScreen.Start();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        #region "ProcessApplicationExtraData"
        private void ProcessApplicationExtraData(Dictionary<string, object> extraDataResponse)
        {
            if (extraDataResponse.ContainsKey("application.extraData[0].options")) { this.SetParameters(extraDataResponse["application.extraData[0].options"].ToString(), "SetExtraDataState", "options", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].options")) { this.SetParameters(extraDataResponse["application.extraData[1].options"].ToString(), "SetExtraDataState", "options", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].options")) { this.SetParameters(extraDataResponse["application.extraData[2].options"].ToString(), "SetExtraDataState", "options", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].options")) { this.SetParameters(extraDataResponse["application.extraData[3].options"].ToString(), "SetExtraDataState", "options", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].options")) { this.SetParameters(extraDataResponse["application.extraData[4].options"].ToString(), "SetExtraDataState", "options", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].options")) { this.SetParameters(extraDataResponse["application.extraData[5].options"].ToString(), "SetExtraDataState", "options", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].options")) { this.SetParameters(extraDataResponse["application.extraData[6].options"].ToString(), "SetExtraDataState", "options", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].options")) { this.SetParameters(extraDataResponse["application.extraData[7].options"].ToString(), "SetExtraDataState", "options", "amountLimit"); }
            //application.extraData[1].controlType "input"
            else if (extraDataResponse.ContainsKey("application.extraData[0].controlType")) { this.SetParameters(extraDataResponse["application.extraData[0].controlType"].ToString(), "SetExtraDataState", "controlType", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].controlType")) { this.SetParameters(extraDataResponse["application.extraData[1].controlType"].ToString(), "SetExtraDataState", "controlType", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].controlType")) { this.SetParameters(extraDataResponse["application.extraData[2].controlType"].ToString(), "SetExtraDataState", "controlType", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].controlType")) { this.SetParameters(extraDataResponse["application.extraData[3].controlType"].ToString(), "SetExtraDataState", "controlType", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].controlType")) { this.SetParameters(extraDataResponse["application.extraData[4].controlType"].ToString(), "SetExtraDataState", "controlType", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].controlType")) { this.SetParameters(extraDataResponse["application.extraData[5].controlType"].ToString(), "SetExtraDataState", "controlType", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].controlType")) { this.SetParameters(extraDataResponse["application.extraData[6].controlType"].ToString(), "SetExtraDataState", "controlType", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].controlType")) { this.SetParameters(extraDataResponse["application.extraData[7].controlType"].ToString(), "SetExtraDataState", "controlType", "amountLimit"); }
            //application.extraData[1].enable
            else if (extraDataResponse.ContainsKey("application.extraData[0].enable")) { this.SetParameters(extraDataResponse["application.extraData[0].enable"].ToString(), "SetExtraDataState", "enabled", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].enable")) { this.SetParameters(extraDataResponse["application.extraData[1].enable"].ToString(), "SetExtraDataState", "enabled", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].enable")) { this.SetParameters(extraDataResponse["application.extraData[2].enable"].ToString(), "SetExtraDataState", "enabled", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].enable")) { this.SetParameters(extraDataResponse["application.extraData[3].enable"].ToString(), "SetExtraDataState", "enabled", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].enable")) { this.SetParameters(extraDataResponse["application.extraData[4].enable"].ToString(), "SetExtraDataState", "enabled", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].enable")) { this.SetParameters(extraDataResponse["application.extraData[5].enable"].ToString(), "SetExtraDataState", "enabled", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].enable")) { this.SetParameters(extraDataResponse["application.extraData[6].enable"].ToString(), "SetExtraDataState", "enabled", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].enable")) { this.SetParameters(extraDataResponse["application.extraData[7].enable"].ToString(), "SetExtraDataState", "enabled", "amountLimit"); }
            //application.extraData[1].label
            else if (extraDataResponse.ContainsKey("application.extraData[0].label")) { this.SetParameters(extraDataResponse["application.extraData[0].label"].ToString(), "SetExtraDataState", "label", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].label")) { this.SetParameters(extraDataResponse["application.extraData[1].label"].ToString(), "SetExtraDataState", "label", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].label")) { this.SetParameters(extraDataResponse["application.extraData[2].label"].ToString(), "SetExtraDataState", "label", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].label")) { this.SetParameters(extraDataResponse["application.extraData[3].label"].ToString(), "SetExtraDataState", "label", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].label")) { this.SetParameters(extraDataResponse["application.extraData[4].label"].ToString(), "SetExtraDataState", "label", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].label")) { this.SetParameters(extraDataResponse["application.extraData[5].label"].ToString(), "SetExtraDataState", "label", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].label")) { this.SetParameters(extraDataResponse["application.extraData[6].label"].ToString(), "SetExtraDataState", "label", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].label")) { this.SetParameters(extraDataResponse["application.extraData[7].label"].ToString(), "SetExtraDataState", "label", "amountLimit"); }
            //application.extraData[1].controlModel
            else if (extraDataResponse.ContainsKey("application.extraData[0].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[0].controlModel"].ToString(), "SetExtraDataState", "controlModel", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[1].controlModel"].ToString(), "SetExtraDataState", "controlModel", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[2].controlModel"].ToString(), "SetExtraDataState", "controlModel", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[3].controlModel"].ToString(), "SetExtraDataState", "controlModel", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[4].controlModel"].ToString(), "SetExtraDataState", "controlModel", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[5].controlModel"].ToString(), "SetExtraDataState", "controlModel", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[6].controlModel"].ToString(), "SetExtraDataState", "controlModel", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].controlModel")) { this.SetParameters(extraDataResponse["application.extraData[7].controlModel"].ToString(), "SetExtraDataState", "controlModel", "amountLimit"); }
            //application.extraData[1].maxLength
            else if (extraDataResponse.ContainsKey("application.extraData[0].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[0].maxLength"].ToString(), "SetExtraDataState", "maxLength", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[1].maxLength"].ToString(), "SetExtraDataState", "maxLength", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[2].maxLength"].ToString(), "SetExtraDataState", "maxLength", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[3].maxLength"].ToString(), "SetExtraDataState", "maxLength", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[4].maxLength"].ToString(), "SetExtraDataState", "maxLength", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[5].maxLength"].ToString(), "SetExtraDataState", "maxLength", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[6].maxLength"].ToString(), "SetExtraDataState", "maxLength", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].maxLength")) { this.SetParameters(extraDataResponse["application.extraData[7].maxLength"].ToString(), "SetExtraDataState", "maxLength", "amountLimit"); }
            //application.extraData[1].minLength
            else if (extraDataResponse.ContainsKey("application.extraData[0].minLength")) { this.SetParameters(extraDataResponse["application.extraData[0].minLength"].ToString(), "SetExtraDataState", "minLength", "currencySelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[1].minLength")) { this.SetParameters(extraDataResponse["application.extraData[1].minLength"].ToString(), "SetExtraDataState", "minLength", "channel1"); }
            else if (extraDataResponse.ContainsKey("application.extraData[2].minLength")) { this.SetParameters(extraDataResponse["application.extraData[2].minLength"].ToString(), "SetExtraDataState", "minLength", "channel2"); }
            else if (extraDataResponse.ContainsKey("application.extraData[3].minLength")) { this.SetParameters(extraDataResponse["application.extraData[3].minLength"].ToString(), "SetExtraDataState", "minLength", "channel3"); }
            else if (extraDataResponse.ContainsKey("application.extraData[4].minLength")) { this.SetParameters(extraDataResponse["application.extraData[4].minLength"].ToString(), "SetExtraDataState", "minLength", "transactionInfo"); }
            else if (extraDataResponse.ContainsKey("application.extraData[5].minLength")) { this.SetParameters(extraDataResponse["application.extraData[5].minLength"].ToString(), "SetExtraDataState", "minLength", "transactionRef"); }
            else if (extraDataResponse.ContainsKey("application.extraData[6].minLength")) { this.SetParameters(extraDataResponse["application.extraData[6].minLength"].ToString(), "SetExtraDataState", "minLength", "shiftSelection"); }
            else if (extraDataResponse.ContainsKey("application.extraData[7].minLength")) { this.SetParameters(extraDataResponse["application.extraData[7].minLength"].ToString(), "SetExtraDataState", "minLength", "amountLimit"); }
        }
        #endregion "ProcessApplicationExtraData"

        private void ProcessExternalApps(string appName)
        {
            try
            {
                ExternalApp extApp = this.extAppMap.List.Where(List => List.Description == appName).FirstOrDefault();
                if (extApp != null)
                {
                    switch (extApp.ProcessAction)
                    {
                        case ProcessAction.StartProcess:
                            if (this.ProcessStart(extApp.Arguments, extApp.Path))
                                Log.Info($"Start process: \"{extApp.Description}\" OK");
                            else
                                Log.Error($"Start process error: \"{extApp.Description}\"");
                            break;
                        case ProcessAction.KillProcess:
                            Process[] processes = Process.GetProcessesByName(extApp.Path);
                            if(processes != null && processes.Length > 0)
                            {
                                foreach (Process process in processes)
                                {
                                    process.Kill();
                                    Log.Info($"Kill process: \"{extApp.Description}\" OK");
                                }
                            }
                            else
                                Log.Warn($"Process not found: \"{extApp.Description}\"");
                            break;
                        case ProcessAction.StopService:
                            this.StopWindowsService(extApp.Path);
                            break;
                        case ProcessAction.StartService:
                            this.StartWindowsService(extApp.Path);
                            break;
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetHelpDeskValues(Dictionary<string, object> newValue)
        {
            string pathFile = $"{Const.appPath}StatesSets\\PropertiesCardReadState.xml";
            CardReadState.PropertiesCardReadState prop = new CardReadState.PropertiesCardReadState();
            prop = Utils.GetGenericXmlData<CardReadState.PropertiesCardReadState>(out ret, pathFile, prop);

            // Actualizar el valor de EnableSupportQrCode si existe en newValue
            if (newValue.ContainsKey("helpDesk.EnableSupportQrCode"))
            {
                prop.EnableSupportQrCode = bool.Parse(newValue["helpDesk.EnableSupportQrCode"].ToString());
            }
            else if (newValue.ContainsKey("helpDesk.ChatURL"))
            {
                prop.HelpDeskChatURL = newValue["helpDesk.ChatURL"].ToString();          
            }
            else if (newValue.ContainsKey("helpDesk.Phone"))
            {
                prop.HelpDeskPhone = newValue["helpDesk.Phone"].ToString();          
            }
            else if (newValue.ContainsKey("helpDesk.Email"))
            {
                prop.HelpDeskEmail = newValue["helpDesk.Email"].ToString();          
            }

            // Guardar los cambios en el archivo XML
            Utils.ObjectToXml<CardReadState.PropertiesCardReadState>(out ret, prop, pathFile);
        }


        private void SetParameters(string newOptions, string itemName, string extraDataElement, string elementName)
        {
            bool ret = false;
            AlephATMAppData alephATMAppData;
            DeviceConfigurations devConf;
            Log.Info($"Set configuration item: {itemName}, type: {extraDataElement}, elementName: {elementName}");
            string pathDC = $"{Const.appPath}Config\\DeviceConfigurations.xml";
            string pathSC = $"{Const.appPath}Config\\ScreenConfiguration.xml";
            string pathMC = $"{Const.appPath}StatesSets\\PropertiesMultiCashAcceptState.xml";
            string pathBD = $"{Const.appPath}StatesSets\\PropertiesBagDropDepositState.xml";
            string pathED = $"{Const.appPath}StatesSets\\PropertiesSetExtraDataState.xml";
            string pathCA = $"{Const.appPath}StatesSets\\PropertiesCashAcceptState.xml";
            try
            {
                switch (itemName)
                {
                    case "devices":
                    case "terminalInfo":
                        devConf = Utils.GetGenericXmlData<DeviceConfigurations>(out ret, pathDC, null);
                        MultiCashAcceptState.PropertiesMultiCashAcceptState propMC = Utils.GetGenericXmlData<MultiCashAcceptState.PropertiesMultiCashAcceptState>(out ret, pathMC, null);
                        CashAcceptState.PropertiesCashAcceptState propCA = Utils.GetGenericXmlData<CashAcceptState.PropertiesCashAcceptState>(out ret, pathCA, null);
                        BagDropDepositState.PropertiesBagDropDepositState propBD = Utils.GetGenericXmlData<BagDropDepositState.PropertiesBagDropDepositState>(out ret, pathBD, null);

                        switch (extraDataElement)
                        {                            
                            case "screen":
                                if (elementName.Equals("resolution"))
                                {
                                    var screenconf = Utils.GetGenericXmlData<ScreenConfiguration>(out ret, pathSC, null);
                                    // ScreenConfiguration screenConfiguration = new ScreenConfiguration();
                                    screenconf.MainBrowserResolution = Utils.StringToEnum<Const.Resolution>("R" + newOptions);
                                    Utils.ObjectToXml<ScreenConfiguration>(out ret, screenconf, pathSC);
                                }
                                break;
                            case "model":
                                this.SetTerminalModel(newOptions, pathBD, pathCA, pathDC, pathMC, pathSC);
                                break;
                            case "branding":
                                this.SetBranding(newOptions, pathDC);
                                break;
                            case "aio":
                                if (elementName.Equals("enable"))
                                {
                                    devConf.IOBoardConfig.Enable = bool.Parse(newOptions);
                                }
                                else if (elementName.Equals("model"))
                                {
                                    devConf.IOBoardConfig.Model = Utils.StringToEnum<Enums.IOBoardModel>(newOptions);
                                    switch(devConf.IOBoardConfig.Model)
                                    {
                                        case IOBoardModel.AIO:
                                            devConf.IOBoardConfig.Baud = "38400";
                                            break;
                                        case IOBoardModel.BtLNX:
                                            devConf.IOBoardConfig.Baud = "115200";
                                            break;
                                    }
                                }
                                else if (elementName.Equals("autoDetectName"))
                                    devConf.IOBoardConfig.AutoDetectName = newOptions;
                                else if (elementName.Equals("baud"))
                                    devConf.IOBoardConfig.Baud = newOptions;
                                else if (elementName.Equals("port"))
                                    devConf.IOBoardConfig.Port = newOptions;
                                Utils.ObjectToXml<DeviceConfigurations>(out ret, devConf, pathDC);
                                break;
                            case "cim":
                                if (elementName.Equals("enable"))
                                {
                                    devConf.CIMconfig.Enable = bool.Parse(newOptions);
                                    Utils.ObjectToXml<DeviceConfigurations>(out ret, devConf, pathDC);
                                }
                                else if (elementName.Equals("autoDeposit"))
                                {
                                    propMC.AutoDeposit = bool.Parse(newOptions);
                                    propMC.DepositWithoutEscrowFull = bool.Parse(newOptions) ? false : true;
                                    Utils.ObjectToXml<MultiCashAcceptState.PropertiesMultiCashAcceptState>(out ret, propMC, pathMC);
                                    propCA.Extension2.AutoDeposit = bool.Parse(newOptions) ? "001" : "000";
                                    Utils.ObjectToXml<CashAcceptState.PropertiesCashAcceptState>(out ret, propCA, pathCA);
                                }
                                else if (elementName.Equals("depositWithoutEscrowFull"))
                                {
                                    propMC.DepositWithoutEscrowFull = bool.Parse(newOptions);
                                    Utils.ObjectToXml<MultiCashAcceptState.PropertiesMultiCashAcceptState>(out ret, propMC, pathMC);
                                }
                                else if (elementName.Equals("verifyLogicalBinFull"))
                                {
                                    propCA.VerifyLogicalFullBin = bool.Parse(newOptions);
                                    Utils.ObjectToXml<CashAcceptState.PropertiesCashAcceptState>(out ret, propCA, pathCA);
                                }
                                else if (elementName.Equals("verifyPrinter"))
                                {
                                    propCA.VerifyPrinter = bool.Parse(newOptions);
                                    Utils.ObjectToXml<CashAcceptState.PropertiesCashAcceptState>(out ret, propCA, pathCA);
                                    propBD.VerifyPrinter = bool.Parse(newOptions);
                                    Utils.ObjectToXml<BagDropDepositState.PropertiesBagDropDepositState>(out ret, propBD, pathBD);
                                }
                                else if (elementName.Equals("verifySensors"))
                                {
                                    //propCA.VerifySensors = bool.Parse(newOptions);
                                    //Utils.ObjectToXml<CashAcceptState.PropertiesCashAcceptState>(out ret, propCA, pathCA);
                                    //propBD.VerifySensors = bool.Parse(newOptions);
                                    //Utils.ObjectToXml<BagDropDepositState.PropertiesBagDropDepositState>(out ret, propBD, pathBD);
                                }
                                break;
                            case "counters":
                                alephATMAppData = AlephATMAppData.GetAppData(out ret);
                                switch (elementName)
                                {
                                    case "logicalFullBinThreshold":
                                        int bagCapacity = int.Parse(newOptions);
                                        this.Core.Counters.UpdateLogicalFullBinThreshold(bagCapacity);
                                        devConf = Utils.GetGenericXmlData<DeviceConfigurations>(out ret, pathDC, null);
                                        devConf.BAGconfig.BagCapacity = bagCapacity;
                                        Utils.ObjectToXml<DeviceConfigurations>(out ret, devConf, pathDC);
                                        //Actualización de la propiedad BagCapacity en AlephDEV.exe.config
                                        string path = $"{Const.appPath}AlephDEV.exe.config";
                                        if(File.Exists(path))
                                        {
                                            string xpath = "configuration/Dispositivos/CIM/ConfigureCashInUnits";
                                            if (Utils.UpdateXmlAttribute(path, xpath, "CashInMaximum", bagCapacity.ToString()))
                                                Log.Info("Update xml attribute: {0} - New value: {1} - Path: {2}", xpath, bagCapacity, path);
                                            else
                                                Log.Warn("Element \"{0}\" not found", xpath);
                                        }
                                        else
                                            Log.Warn("File \"{0}\" not found", path);
                                        break;
                                    case "tsn":
                                        int tsn = int.Parse(newOptions);
                                        this.Core.Counters.ReplaceTSN(tsn);
                                        break;
                                }
                                break;
                            case "prt":
                                switch(elementName)
                                {
                                    case "enable":
                                        devConf.PRTconfig.Enable = bool.Parse(newOptions);
                                        break;
                                    case "printerName":
                                        devConf.PRTconfig.PrinterName = Utils.StringToEnum<Enums.PrinterModel>(newOptions);
                                        switch(devConf.PRTconfig.PrinterName)
                                        {
                                            case PrinterModel.PLUSII:
                                            case PrinterModel.NII_Printer_DS:
                                                devConf.PRTconfig.LogoConfig.X = 50;
                                                devConf.PRTconfig.BodyConfig.X = 0;
                                                devConf.PRTconfig.BarcodeConfig.X = 20;
                                                devConf.PRTconfig.FontSize = 6;
                                                devConf.PRTconfig.BoldActive = true;
                                                break;
                                            case PrinterModel.POS_80:
                                                devConf.PRTconfig.LogoConfig.X = 50;
                                                devConf.PRTconfig.BodyConfig.X = 0;
                                                devConf.PRTconfig.BarcodeConfig.X = 20;
                                                devConf.PRTconfig.FontSize = 7;
                                                devConf.PRTconfig.BoldActive = false;
                                                break;
                                            case PrinterModel.EPSON_TM_T82II:
                                            case PrinterModel.EPSON_TM_T82III:
                                                devConf.PRTconfig.LogoConfig.X = 95;
                                                devConf.PRTconfig.BodyConfig.X = 20;
                                                devConf.PRTconfig.BarcodeConfig.X = 20;
                                                devConf.PRTconfig.FontSize = 8;
                                                devConf.PRTconfig.BoldActive = true;
                                                break;
                                            default:
                                                devConf.PRTconfig.LogoConfig.X = 95;
                                                devConf.PRTconfig.BodyConfig.X = 45;
                                                devConf.PRTconfig.BarcodeConfig.X = 65;
                                                devConf.PRTconfig.FontSize = 7;
                                                devConf.PRTconfig.BoldActive = false;
                                                break;
                                        }
                                        break;
                                    case "logoFileName":
                                        devConf.PRTconfig.LogoFileName = newOptions;
                                        break;
                                    case "fontType":
                                        devConf.PRTconfig.FontType = newOptions;
                                        break;
                                    case "fontSize":
                                        devConf.PRTconfig.FontSize = int.Parse(newOptions);
                                        break;
                                    case "printerTemplateFileName":
                                        alephATMAppData = AlephATMAppData.GetAppData(out ret);
                                        alephATMAppData.PrinterTemplateFileName = newOptions;
                                        Utils.ObjectToXml<AlephATMAppData>(out ret, alephATMAppData, $"{Const.appPath}Config\\AlephATMAppData.xml");
                                        break;
                                }
                                Utils.ObjectToXml<DeviceConfigurations>(out ret, devConf, pathDC);
                                break;
                        }
                        break;
                    case "SetExtraDataState":
                        SetExtraDataState.PropertiesSetExtraDataState propED = Utils.GetGenericXmlData<SetExtraDataState.PropertiesSetExtraDataState>(out ret, pathED, null);
                        ExtraDataConf extraDataConf = propED.ExtraDataConfigurations.FirstOrDefault(x => x.name.Equals(elementName));
                        switch (extraDataElement)
                        {
                            case "options":
                                string[] options = Utils.NewtonsoftDeserialize<string[]>(out ret, newOptions);
                                if (ret)
                                {
                                    extraDataConf.options = new string[options.Length];
                                    for (int i = 0; i < options.Length; i++)
                                    {
                                        extraDataConf.options[i] = options[i];
                                    }
                                    Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                }
                                break;
                            case "controlType":
                                extraDataConf.controlType = newOptions;
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            case "controlModel":
                                extraDataConf.controlModel = newOptions;
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            case "enabled":
                                extraDataConf.enabled = bool.Parse(newOptions);
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            case "label":
                                extraDataConf.label = newOptions;
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            case "maxLength":
                                extraDataConf.maxLength = int.Parse(newOptions);
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            case "minLength":
                                extraDataConf.minLength = int.Parse(newOptions);
                                Utils.ObjectToXml<SetExtraDataState.PropertiesSetExtraDataState>(out ret, propED, pathED);
                                break;
                            default:
                                Log.Error($"Unknow extra data element: {extraDataElement}");
                                break;
                        }

                        break;
                    default:
                        Log.Error($"Unknow activity name: {itemName}");
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetBranding(string newOption, string pathDC)
        {
            Enums.Branding branding;
            string scriptPath = "";
            try
            {
                branding = Utils.StringToEnum<Enums.Branding>(newOption);
                AlephATMAppData alephATMAppData = AlephATMAppData.GetAppData(out ret);
                alephATMAppData.Branding = branding;
                switch (branding)
                {
                    case Enums.Branding.Prosegur:
                        scriptPath = "UpdateScreensProsegur.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-Prosegur.xml";
                        alephATMAppData.FlowFileName = "StatesRTLA.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersRTLA.aph";
                        break;
                    case Enums.Branding.Macro:
                        scriptPath = "UpdateScreensBancoMacro.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-Prosegur.xml";
                        alephATMAppData.FlowFileName = "StatesWOED.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersRTLA.aph";
                        break;
                    case Enums.Branding.Galicia:
                        scriptPath = "UpdateScreensProsegur.bat";
                        alephATMAppData.FlowFileName = "StatesADMA.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersRTLA.aph";
                        break;
                    case Enums.Branding.PlanB:
                        scriptPath = "UpdateScreensProsegur.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-PlanB.xml";
                        alephATMAppData.FlowFileName = "StatesRTLB.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersRTLB.aph";
                        break;
                    case Enums.Branding.RedPagosA:
                        scriptPath = "UpdateScreensRedPagos.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-RedPagos.xml";
                        alephATMAppData.FlowFileName = "StatesPAYB.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersPAYB.aph";
                        break;
                    case Enums.Branding.Ciudad:
                        scriptPath = "UpdateScreensBancoCiudad.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-RedPagos.xml";
                        alephATMAppData.FlowFileName = "StatesPAYA.aph";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersPAYA.aph";
                        break;
                    case Enums.Branding.DepositarioRetail:
                        scriptPath = "UpdateScreensProsegur.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-Prosegur.xml";
                        alephATMAppData.FlowFileName = "StatesDEP.xml";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersDEP.xml";
                        break;
                    case Enums.Branding.FIC:
                        scriptPath = "UpdateScreensFIC.bat";
                        alephATMAppData.PrinterTemplateFileName = "Template56mm-Depositarios.xml";
                        alephATMAppData.FlowFileName = "StatesDEP.xml";
                        alephATMAppData.EnhParamFileName = "EnhancedParametersDEP.xml";
                        break;
                    default:
                        Log.Error($"Unknow branding: {branding}");
                        break;

                }
                Utils.ObjectToXml<AlephATMAppData>(out ret, alephATMAppData, $"{Const.appPath}Config\\AlephATMAppData.xml");
                scriptPath = $"{Const.appPath}Themes\\{scriptPath}";
                if (File.Exists(scriptPath))
                {
                    if (this.ProcessStart("", scriptPath))
                        Log.Info($"Branding {newOption} updated OK");
                    else
                        Log.Error($"Error updating branding {newOption}");
                }
                else
                    Log.Error($"File not found: {scriptPath}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetTerminalModel(string newOptions, string pathBD, string pathCA, string pathDC, string pathMC, string pathSC)
        {
            try
            {
                Enums.TerminalModel terminalModel = Utils.StringToEnum<Enums.TerminalModel>(newOptions);
                AlephATMAppData alephATMAppData = AlephATMAppData.GetAppData(out ret);
                alephATMAppData.TerminalModel = terminalModel;
                switch (alephATMAppData.TerminalModel)
                {
                    case Enums.TerminalModel.MiniBank_JH6000_D:
                    case Enums.TerminalModel.MiniBank_JH600_A:
                    case Enums.TerminalModel.CTR50:
                        alephATMAppData.AlephDEVTimeOut = 240000;
                        break;
                    case Enums.TerminalModel.GRG_P2600:
                        alephATMAppData.AlephDEVTimeOut = 60000;
                        break;
                    default:
                        alephATMAppData.AlephDEVTimeOut = 50000;
                        break;
                }
                Utils.ObjectToXml<AlephATMAppData>(out ret, alephATMAppData, $"{Const.appPath}Config\\AlephATMAppData.xml");
                //
                switch (alephATMAppData.TerminalModel)
                {
                    case Enums.TerminalModel.SNBC_CTI90:
                    case Enums.TerminalModel.SNBC_CTE1:
                    case Enums.TerminalModel.CTR50:
                    case Enums.TerminalModel.CTIUL:
                    case TerminalModel.Depositario:
                        this.SetParameters("true", "SetExtraDataState", "enabled", "currencySelection"); //Forzado de seteo manual de divisa
                        break;
                    default:
                        this.SetParameters("false", "SetExtraDataState", "enabled", "currencySelection"); //Forzado de seteo manual de divisa
                        break;
                }
                //
                File.Delete(pathBD);
                Thread.Sleep(100);
                BagDropDepositState.PropertiesBagDropDepositState propBD = new BagDropDepositState.PropertiesBagDropDepositState(alephATMAppData);
                propBD = Utils.GetGenericXmlData<BagDropDepositState.PropertiesBagDropDepositState>(out ret, pathBD, propBD);
                //
                File.Delete(pathCA);
                Thread.Sleep(100);
                CashAcceptState.PropertiesCashAcceptState propCA = new CashAcceptState.PropertiesCashAcceptState(alephATMAppData);
                propCA = Utils.GetGenericXmlData<CashAcceptState.PropertiesCashAcceptState>(out ret, pathCA, propCA);
                //
                File.Delete(pathMC);
                Thread.Sleep(100);
                MultiCashAcceptState.PropertiesMultiCashAcceptState propMC = new MultiCashAcceptState.PropertiesMultiCashAcceptState(alephATMAppData);
                propMC = Utils.GetGenericXmlData<MultiCashAcceptState.PropertiesMultiCashAcceptState>(out ret, pathCA, propCA);
                //
                File.Delete(pathDC);
                DeviceConfigurations devConf = DeviceConfigurations.GetObject(out ret, alephATMAppData.TerminalModel, alephATMAppData.DefaultCurrency);
                //
                File.Delete(pathSC);
                ScreenConfiguration sc = new ScreenConfiguration(alephATMAppData);
                sc = Utils.GetGenericXmlData<ScreenConfiguration>(out ret, pathSC, sc);

                //if(File.Exists($"{Const.appPath}Config\\CashInAcceptedNotes.xml"))
                //    File.Delete($"{Const.appPath}Config\\CashInAcceptedNotes.xml");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetRegistryCashAcceptorModel(Enums.CimModel cimModel)
        {
            Log.Info($"Selected CIM model: {cimModel}");
            string keyPath = @".DEFAULT\XFS\LOGICAL_SERVICES\CashAcceptor";
            string value = "unknown";
            switch (cimModel)
            {
                case Enums.CimModel.SNBC:
                    {
                        value = "SnbcTCR";
                        break;
                    }
                case Enums.CimModel.Glory:
                    {
                        value = "CimGloryGDB10DE50";
                        break;
                    }
                default:
                    Log.Error("Unknown cim model");
                    break;
            }
            this.UpdateRegKey("Provider", value, keyPath);
        }

        internal void UpdateRegKey(string keyName, string KeyValue, string keyPath)
        {
            RegistryKey rk = Registry.Users;
            try
            {
                RegistryKey key = rk.OpenSubKey(keyPath, true); //Abrimos en modo lectura y escritura
                if (key == null)
                    key = rk.CreateSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (key.GetValue(keyName) != null)
                    key.DeleteValue(keyName);
                key.SetValue(keyName, KeyValue);
                Log.Info($"Update Registry key: {keyPath}\\{keyName} - value: {KeyValue}");
                key.Close();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        private bool SetEthernetAddresses(string _ipV4Address, string _subNetMask, string _defaultGateway)
        {
            bool result = false;
            string networkName = string.Empty, ipV4Address = string.Empty, subNetMask = string.Empty, defaultGateway = string.Empty;
            try
            {
                ipV4Address = string.IsNullOrEmpty(_ipV4Address) ? this.GetLocalIPAddress() : _ipV4Address;
                subNetMask = string.IsNullOrEmpty(_subNetMask) ? this.GetSubnetMask() : _subNetMask;
                defaultGateway = string.IsNullOrEmpty(_defaultGateway) ? this.GetDefaultGateway() : _defaultGateway;
                NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                Log.Info("NetworkInterface:");
                for (int i = 0; i < allNetworkInterfaces.Length; i++)
                {
                    Log.Info($"NetworkName: {allNetworkInterfaces[i].Name}");
                    if (allNetworkInterfaces[i].Name.Contains("Ethernet")) //carga solo la conexión cuyo nombre contenga "Ethernet"
                        networkName = allNetworkInterfaces[i].Name;
                }
                string arg = $"interface ip set address \"{networkName}\" static {ipV4Address} {subNetMask} {defaultGateway}";
                Log.Info("Execute command: netsh, arguments: {0}", arg);
                result = this.ProcessStart(arg, "netsh");

            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }

        private bool ProcessStart(string arg, string fileName)
        {
            bool result = false;
            ProcessStartInfo process = new ProcessStartInfo();
            process.FileName = fileName;
            process.Arguments = arg;
            using (Process proc = Process.Start(process))
            {
                proc.WaitForExit();
                Log.Info($"Exit code = {proc.ExitCode}");
                result = true;
            }
            return result;
        }

        private void StopWindowsService(string serviceName)
        {
            try
            {
                using (ServiceController serviceController = new ServiceController(serviceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Stopped &&
                        serviceController.Status != ServiceControllerStatus.StopPending)
                    {
                        Log.Info($"Trying stop service: {serviceName}");
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                        Log.Info("Service is stopped");
                    }
                    else
                    {
                        Log.Warn("Service is stopping");
                    }
                }
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        private void StartWindowsService(string serviceName)
        {
            try
            {
                using (ServiceController serviceController = new ServiceController(serviceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Running &&
                        serviceController.Status != ServiceControllerStatus.StartPending)
                    {
                        Log.Info($"Trying start service: {serviceName}");
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                        Log.Info("Service is started");
                    }
                    else
                    {
                        Log.Warn("Service is already started");
                    }
                }
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        private void SetMachineID(string machineId)
        {
            if (this.prop.UpdateExternalMachineID_1)//Update PRDM machineID
            {
                if (Utils.UpdateXmlElement(this.prop.PathExternalMachineID_1, this.prop.XPathExternalMachineID_1, machineId))
                    Log.Info("Update xml element 1: {0} - New value: {1} - Path: {2}", this.prop.XPathExternalMachineID_1, machineId, this.prop.PathExternalMachineID_1);
                else
                    Log.Warn("Element \"{0}\" not found", this.prop.XPathExternalMachineID_1);
            }
            if (this.prop.UpdateExternalMachineID_2)//Update XX machineID
            {
                if (Utils.UpdateXmlElement(this.prop.PathExternalMachineID_2, this.prop.XPathExternalMachineID_2, machineId))
                    Log.Info("Update xml element 2: {0} - New value: {1} - Path: {2}", this.prop.XPathExternalMachineID_2, machineId, this.prop.PathExternalMachineID_2);
                else
                    Log.Warn("Element \"{0}\" not found", this.prop.XPathExternalMachineID_2);
            }
            if (Utils.UpdateXmlElement($"{Const.appPath}Config\\TerminalInfo.xml", "TerminalInfo/LogicalUnitNumber", machineId))
                Log.Info("Update xml element 3: ok");
            else
                Log.Warn("Update xml element 3: error");
            //Update computer name with machineID
            if (this.prop.UpdateComputerName)
            {
                string CountryId = String.Empty;
                if (Utils.GetXmlElement(this.prop.PathExternalMachineID_1, this.prop.XPathCountryId, out CountryId))
                {
                    if (this.SetComputerName($"{CountryId}{machineId}"))
                        Log.Info("Set computer name success!");
                    else
                        Log.Error("Set computer name error");
                }
                else
                    Log.Warn("Getxml element: error");
            }
        }

        private bool SetComputerName(string newName)
        {
            bool result = false;
            try
            {
                string arg = $"COMPUTERSYSTEM WHERE NAME=\'{Environment.MachineName}\' RENAME NAME={newName}";
                Log.Info("Execute command: WMIC.exe, arguments: {0}", arg);
                ProcessStartInfo process = new ProcessStartInfo();
                process.FileName = "WMIC.exe";
                process.Arguments = arg;
                process.Verb = "runas";
                using (Process proc = Process.Start(process))
                {
                    proc.WaitForExit();
                    Log.Info($"Exit code = {proc.ExitCode}");
                    if (proc.ExitCode == 0)
                        result = true;
                }
            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }

        public void SetSystemTimeZone(string timeZoneId)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "tzutil.exe",
                Arguments = "/s \"" + timeZoneId + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit();
                TimeZoneInfo.ClearCachedData();
            }
        }

        public void SetSystemRegion(string regionId)
        {
            try
            {
                Log.Info($"Current region: {CultureInfo.CurrentCulture}");
                AlephATMAppData alephATMAppData = AlephATMAppData.GetAppData(out ret);
                alephATMAppData.Region = regionId;
                Utils.ObjectToXml<AlephATMAppData>(out ret, alephATMAppData, $"{Const.appPath}Config\\AlephATMAppData.xml");
                Thread.CurrentThread.CurrentCulture = new CultureInfo(regionId);
                Log.Info($"New region: {CultureInfo.CurrentCulture}"); ;
            }
            catch (Exception value) { Log.Fatal(value); }
        }


        //private bool SetComputerName(string newName)
        //{
        //    bool result = false;
        //    string networkName = string.Empty, ipV4Address = string.Empty, subNetMask = string.Empty, defaultGateway = string.Empty;
        //    try
        //    {
        //        //string arg = "computersystem where caption='" + System.Environment.MachineName + "' rename " + newName;
        //        string path = $"{Const.appPath}Config\\changeComputerName.ps1";
        //        string arg = @"& '" + path + "'";
        //        Log.Info("Execute command: powerShell, arguments: {0}", path);
        //        ProcessStartInfo process = new ProcessStartInfo();
        //        process.FileName = @"C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe";
        //        //process.Arguments = $"-File \"{path}\""; 
        //        process.Arguments = @"–ExecutionPolicy Bypass -File ""C:\my\changeComputerName.ps1"""; ;
        //        process.Verb = "runas";
        //        using (Process proc = Process.Start(process))
        //        {
        //            proc.WaitForExit();
        //            Log.Info($"Exit code = {proc.ExitCode}");
        //            result = true;
        //        }
        //    }
        //    catch (Exception value)
        //    {
        //        Log.Fatal(value);
        //    }
        //    return result;
        //}
        #endregion Receive configuration data

        #region Send configuration data

        private string GetCurrentConfiguration()
        {
            string result = string.Empty;
            bool ret = false;
            try
            {
                ScreenConfiguration screenConf = new ScreenConfiguration();

                DeviceConfigurations deviceConfigurations = this.GetDeviceConfiguration();
                CashAcceptState.PropertiesCashAcceptState propertiesCashAcceptState = this.GetCashDepositConfiguration();
                MultiCashAcceptState.PropertiesMultiCashAcceptState propertiesMultiCashAcceptState = this.GetMultiCashAcceptStateConfiguration();
                AlephATMAppData alephATMAppData = AlephATMAppData.GetAppData(out ret);
                bool screenc = ScreenConfiguration.GetScreenConfiguration(out screenConf, alephATMAppData);
                TerminalInfo terminalInfo = null;
                ret = TerminalInfo.GetTerminalInfo(out terminalInfo);
                JObject jObject = new JObject
                {
                    ["terminalInfo"] = new JObject
                    {
                        ["machineID"] = (JToken)terminalInfo.LogicalUnitNumber.ToString(),
                        ["address"] = (JToken)terminalInfo.Address.ToString(),
                        ["city"] = (JToken)terminalInfo.City.ToString(),
                        ["phone"] = (JToken)terminalInfo.Phone.ToString(),
                        //["agency"] = (JToken)terminalInfo.Subagencia.ToString(),
                        //["tellerStation"] = (JToken)terminalInfo.Caja.ToString(),
                        ["model"] = (JToken)alephATMAppData.TerminalModel.ToString(),
                        ["branding"] = (JToken)alephATMAppData.Branding.ToString()
                    },
                    ["system"] = new JObject
                    {
                        ["timeZone"] = (JToken)TimeZone.CurrentTimeZone.StandardName,
                        ["region"] = (JToken)CultureInfo.CurrentCulture.Name,
                        ["computerName"] = (JToken)Environment.MachineName,
                        ["ipV4Address"] = (JToken)this.GetLocalIPAddress(),
                        ["subNetMask"] = (JToken)this.GetSubnetMask(),
                        ["defaultGateway"] = (JToken)this.GetDefaultGateway(),
                    },
                    ["externalApps"] = JArray.FromObject(this.GetExternalApps()),
                    ["denominations"] = JArray.FromObject(this.GetDenominations()),
                    ["application"] = new JObject
                    {
                        ["extraData"] = this.GetExtraDataConfiguration(),
                        ["helpDesk"] = JToken.FromObject(this.GetHelpDeskInformation()),
                    },
                    ["devices"] = new JObject
                    {
                        ["aio"] = new JObject
                        {
                            ["enable"] = (JToken)deviceConfigurations.IOBoardConfig.Enable,
                            ["model"] = (JToken)deviceConfigurations.IOBoardConfig.Model.ToString(),
                            ["port"] = (JToken)deviceConfigurations.IOBoardConfig.Port,
                            ["baud"] = (JToken)deviceConfigurations.IOBoardConfig.Baud,
                            ["autoDetectName"] = (JToken)deviceConfigurations.IOBoardConfig.AutoDetectName
                        },
                        ["cim"] = new JObject
                        {
                            ["enable"] = (JToken)deviceConfigurations.CIMconfig.Enable,
                            ["openEscrowAtInit"] = (JToken)deviceConfigurations.CIMconfig.OpenEscrowAtInit,
                            ["autoDeposit"] = (JToken)propertiesMultiCashAcceptState.AutoDeposit,
                            ["depositWithoutEscrowFull"] = (JToken)propertiesMultiCashAcceptState.DepositWithoutEscrowFull,
                            ["verifySensors"] = (JToken)"true",
                            ["verifyPrinter"] = (JToken)propertiesCashAcceptState.VerifyPrinter,
                            ["verifyLogicalBinFull"] = (JToken)propertiesCashAcceptState.VerifyLogicalFullBin
                        },
                        ["prt"] = new JObject
                        {
                            ["enable"] = (JToken)true,
                            ["printerName"] = (JToken)deviceConfigurations.PRTconfig.PrinterName.ToString(),
                            ["templateName"] = (JToken)alephATMAppData.PrinterTemplateFileName,
                            ["logoFileName"] = (JToken)deviceConfigurations.PRTconfig.LogoFileName,
                            ["fontType"] = (JToken)deviceConfigurations.PRTconfig.FontType,
                            ["fontSize"] = (JToken)deviceConfigurations.PRTconfig.FontSize
                        },
                        ["counters"] = new JObject
                        {
                            ["tsn"] = this.Core.Counters != null ? (JToken)this.Core.Counters.TSN : "",
                            ["logicalFullBinThreshold"] = (JToken)deviceConfigurations.BAGconfig.BagCapacity
                        },
                        ["screen"] = new JObject
                        {
                            ["resolution"] = (JToken)screenConf.MainBrowserResolution.ToString().Substring(1)
                        }
                    }
                };
                result = Utils.JsonSerialize(jObject.ToString().Replace("\r\n      ", "").Replace("\r\n     ", "")
                    .Replace("\r\n    ", "")
                    .Replace("\r\n   ", "")
                    .Replace("\r\n  ", "")
                    .Replace("\r\n ", "")
                    .Replace("\r\n", ""));
            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }

        private string GetConfigurationOptions()
        {

            string result = string.Empty;
            // convert List<Enum.Resolution> to string array
            var resolutions = GetAvailableResolutions().Select(
                x => x.ToString().Substring(1)
            ).ToArray();

            try
            {
                JObject obj = new JObject
                {
                    ["devices"] = new JObject
                    {
                        ["screen"] = new JObject
                        {
                            ["resolution"] = JArray.FromObject(resolutions)
                        }
                    }
                };
                result = Utils.JsonSerialize(obj.ToString().Replace("\r\n      ", "").Replace("\r\n     ", "")
                    .Replace("\r\n    ", "")
                    .Replace("\r\n   ", "")
                    .Replace("\r\n  ", "")
                    .Replace("\r\n ", "")
                    .Replace("\r\n", ""));
            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }

        private string GetSystemTimeZone()
        {
            string ret = string.Empty;
            foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
            {
                Console.WriteLine("ID: {0}", z.Id);
                Console.WriteLine("   Display Name: {0, 40}", z.DisplayName);
            }
            return ret;
        }

        private JArray GetExtraDataConfiguration()
        {
            string jsonData = string.Empty;
            List<ExtraDataConf> conf = new List<ExtraDataConf>();
            JArray jArray = null;
            string pathFile = $"{Const.appPath}StatesSets\\PropertiesSetExtraDataState.xml";
            SetExtraDataState.PropertiesSetExtraDataState propertiesSetExtraDataState = new SetExtraDataState.PropertiesSetExtraDataState();
            propertiesSetExtraDataState = Utils.GetGenericXmlData<SetExtraDataState.PropertiesSetExtraDataState>(out ret, pathFile, propertiesSetExtraDataState);
            if (ret)
            {
                jsonData = Utils.NewtonsoftSerialize(propertiesSetExtraDataState.ExtraDataConfigurations);
                jArray = JArray.Parse(jsonData);
            }
            else
            {
                Log.Error(string.Format("->Can´t get properties of Activity: SetExtraDataState"));
            }
            return jArray;
        }
        
        private Object GetHelpDeskInformation()
        {
            Object obj = new { };
            string pathFile = $"{Const.appPath}StatesSets\\PropertiesCardReadState.xml";
            CardReadState.PropertiesCardReadState prop = new CardReadState.PropertiesCardReadState();
            prop = Utils.GetGenericXmlData<CardReadState.PropertiesCardReadState>(out ret, pathFile, prop);
            if (ret)
            {
                obj = new { EnableSupportQrCode = prop.EnableSupportQrCode, ChatURL = prop.HelpDeskChatURL, Phone = prop.HelpDeskPhone, Email = prop.HelpDeskEmail};
            }
            else
            {
                Log.Error(string.Format("->Can´t get properties of Activity: CardReadState"));
            }
            return obj;
        }

        private DeviceConfigurations GetDeviceConfiguration()
        {
            DeviceConfigurations deviceConfigurations = new DeviceConfigurations(this.Core.AlephATMAppData.TerminalModel, this.Core.AlephATMAppData.DefaultCurrency);
            string pathFile = $"{Const.appPath}Config\\DeviceConfigurations.xml";
            deviceConfigurations = Utils.GetGenericXmlData<DeviceConfigurations>(out ret, pathFile, deviceConfigurations);
            return deviceConfigurations;
        }

        private List<CashInAcceptedNotes> GetDenominations()
        {
            var ret = CashInAcceptedNotes.GetCashInAcceptedNotes(out acceptedNotes);
            return acceptedNotes;
        }

        private List<ExternalApp> GetExternalApps()
        {
            extAppMap.List = new List<ExternalApp>();
            if (!ExternalApps.GetMapping(out extAppMap, this.Core.AlephATMAppData.Branding))
            {
                Log.Error("GetExternalApps: Error getting external apps");
                return new List<ExternalApp>();
            }
            return extAppMap.List;
        }

        private void SetDenominationActivation(CashInAcceptedNotes denom, bool enabled)
        {
            this.acceptedNotes.FirstOrDefault(acceptedNotes => acceptedNotes.CurId == denom.CurId && acceptedNotes.Values == denom.Values).Configured = enabled;
            string pathFile = $"{Const.appPath}Config\\CashInAcceptedNotes.xml";
            Utils.ObjectToXml<List<CashInAcceptedNotes>>(out ret, this.acceptedNotes, pathFile);
        }

        private CashAcceptState.PropertiesCashAcceptState GetCashDepositConfiguration()
        {
            CashAcceptState.PropertiesCashAcceptState propertiesCashAcceptState = new CashAcceptState.PropertiesCashAcceptState(this.Core.AlephATMAppData);
            string pathFile = $"{Const.appPath}StatesSets\\PropertiesCashAcceptState.xml";
            propertiesCashAcceptState = Utils.GetGenericXmlData<CashAcceptState.PropertiesCashAcceptState>(out ret, pathFile, propertiesCashAcceptState);
            return propertiesCashAcceptState;
        }

        private MultiCashAcceptState.PropertiesMultiCashAcceptState GetMultiCashAcceptStateConfiguration()
        {
            MultiCashAcceptState.PropertiesMultiCashAcceptState propertiesMultiCashAcceptState = new MultiCashAcceptState.PropertiesMultiCashAcceptState();
            string pathFile = $"{Const.appPath}StatesSets\\PropertiesMultiCashAcceptState.xml";
            propertiesMultiCashAcceptState = Utils.GetGenericXmlData<MultiCashAcceptState.PropertiesMultiCashAcceptState>(out ret, pathFile, propertiesMultiCashAcceptState);
            return propertiesMultiCashAcceptState;
        }
        #endregion Send configuration data

        public string GetLocalIPAddress()
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    StringBuilder sb = new StringBuilder();
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            string[] array = ip.ToString().Split('.');
                            for (int i = 0; i < array.Length; i++)
                            {
                                array[i] = array[i].PadLeft(3, '0');
                                sb.Append($"{array[i]}.");
                            }
                            return sb.ToString().Remove(sb.Length - 1, 1);
                        }
                    }
                    Log.Error("No network adapters with an IPv4 address in the system!");
                }
                else
                    Log.Warn("No network adapters available");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return "0.0.0.0 ";
        }

        public string GetSubnetMask()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("0.0.0.0 ");
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    string mask = string.Empty;
                    foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                        {
                            if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork && !unicastIPAddressInformation.Address.ToString().Equals("127.0.0.1"))
                                mask = unicastIPAddressInformation.IPv4Mask.ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(mask))
                    {
                        sb = new StringBuilder();
                        string[] array = mask.ToString().Split('.');
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i] = array[i].PadLeft(3, '0');
                            sb.Append($"{array[i]}.");
                        }
                    }
                }
                else
                    Log.Warn("No network adapters available");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return sb.ToString().Remove(sb.Length - 1, 1);
        }

        public string GetDefaultGateway()
        {
            StringBuilder sb = new StringBuilder();
            string ret = "0.0.0.0";
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    string defaultGateway = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                    .Select(g => g?.Address)
                    .Where(a => a != null)
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    // .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                    .FirstOrDefault().ToString();
                    string[] array = defaultGateway.ToString().Split('.');
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = array[i].PadLeft(3, '0');
                        sb.Append($"{array[i]}.");
                    }
                    if (sb.Length > 1)
                        ret = sb.ToString().Remove(sb.Length - 1, 1);
                    else
                        Log.Warn("Can´t get default gateway");
                }
                else
                    Log.Warn("No network adapters available");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public List<Const.Resolution> GetAvailableResolutions()
        {
            //get all resolutions from enum
            List<Const.Resolution> resolutions = Enum.GetValues(typeof(Const.Resolution)).Cast<Const.Resolution>().ToList();
            var deviceResolution = GetScreenResolution();

            //remove resolutions that are bigger than the device resolution
            foreach (var resolution in resolutions.ToList())
            {
                string[] sResolution = resolution.ToString().Substring(1).Split('x');
                int width = int.Parse(sResolution[0]);
                int height = int.Parse(sResolution[1]);

                if (width > deviceResolution.Width || height > deviceResolution.Height)
                    resolutions.Remove(resolution);
            }


            return resolutions;
        }

        public Size GetScreenResolution()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            Size screen = new Size(width, height);
            return screen;
        }

        private void HandleOthersKeysReturn(string othersKeys)
        {
            string md5 = string.Empty;
            this.ResetTimer();
            try
            {
                //Log.Debug($"Key press: {othersKeys}");
                //this.WriteEJ($"Key press: {othersKeys}");
                switch (othersKeys)
                {
                    case "ENTER": //Confirma TX
                        break;
                    case "CANCEL": //Confirma TX
                        this.SetActivityResult(StateResult.SWERROR, this.prop.ExitNextStateNumber);
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerFindUserByLogin(UserProfile_Type userProfileData)
        {
            try
            {
                Log.Debug("/--->");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.StopTimer();
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
                this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
                this.moreTime.EvtMoreTime -= new MoreTime.DelegateMoreTime(AnalyzeMoreTimeResult);
                this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
                this.alephHost.Host.EvtFindUserByLogin -= HandlerFindUserByLogin;
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
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.ActivityStart();
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.ExitNextStateNumber);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeOutNextStateNumber);
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
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.alephHost.Host.EvtFindUserByLogin -= HandlerFindUserByLogin;
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"

    }
}
