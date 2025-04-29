using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
//using System.Threading.Tasks;

namespace Entities
{
    public class Health
    {
        public Const.TerminalMode Mode = Const.TerminalMode.OutOfService;
        public Const.LineMode Line = Const.LineMode.OffLine;
        //public Const.InUseMode InUseState = Const.InUseMode.NotInUse;
        public SensorsState SensorsState;
        public ConfigurationInformation_Type ConfigurationInformation;
        public HardwareConfigurationData_Type HardwareConfigurationData;
        public SuppliesData_Type SuppliesData;
        public FitnessData_Type FitnessData;
        public TamperAndSensorStatusData_Type TamperAndSensorStatusData;
        public SoftwareIDAndReleaseNumberData_Type SoftwareIDAndReleaseNumberData;
        public LocalConfigurationOptionDigits_Type LocalConfigurationOptionDigits;
        public NoteDefinitions_Type NoteDefinitions;
        public SendConfigurationID_Type SendConfigurationID;
        public SupplyCounters_Type SupplyCounters;
 

        public Health()
        {
            this.ConfigurationInformation = new ConfigurationInformation_Type();
            this.HardwareConfigurationData = new HardwareConfigurationData_Type();
            this.SuppliesData = new SuppliesData_Type();
            this.FitnessData = new FitnessData_Type();
            this.TamperAndSensorStatusData = new TamperAndSensorStatusData_Type();
            this.SoftwareIDAndReleaseNumberData = new SoftwareIDAndReleaseNumberData_Type();
            this.LocalConfigurationOptionDigits = new LocalConfigurationOptionDigits_Type();
            this.NoteDefinitions = new NoteDefinitions_Type();
            this.SendConfigurationID = new SendConfigurationID_Type();
            this.SupplyCounters = new SupplyCounters_Type();
            this.SensorsState = new SensorsState();

        }

        public void SetFitnessState(string itemElementName, int state)
        {
            if (FitnessData != null)
            {
                foreach (DeviceFitnessData_Type dev in FitnessData.HardwareFitnessData.Device)
                {
                    if (dev.ItemElementName.ToString().Equals(itemElementName))
                    {

                    }
                    switch (dev.ItemElementName.ToString())
                    {
                        case "BunchNoteAcceptorError":
                            {
                                BunchNoteAcceptorFitnessData_Type bunchNoteAcceptorFitnessData = (BunchNoteAcceptorFitnessData_Type)dev.Item;
                                dev.Item = bunchNoteAcceptorFitnessData.CashAcceptorFitness = (ErrorSeverity_Type)state;
                                break;
                            }
                    }
                }
            }
        }


        public int GetFitnessState(string itemElementName, int state)
        {
            int ret = -1;
            if (FitnessData != null)
            {
                foreach (DeviceFitnessData_Type dev in FitnessData.HardwareFitnessData.Device)
                {
                    if (dev.ItemElementName.ToString().Equals(itemElementName))
                    {

                    }
                    switch (dev.ItemElementName.ToString())
                    {
                        case "BunchNoteAcceptorError":
                            {
                                BunchNoteAcceptorFitnessData_Type bunchNoteAcceptorFitnessData = (BunchNoteAcceptorFitnessData_Type)dev.Item;
                                ret = (int)bunchNoteAcceptorFitnessData.CashAcceptorFitness;
                                break;
                            }
                    }
                }
            }
            return ret;
        }

        public string ViewAtmState()
        {
            StringBuilder sbCounters = new StringBuilder();
            String sCRD = string.Empty;
            String sCDM = string.Empty;
            String sCDM0 = string.Empty;
            String sCDM1 = string.Empty;
            String sCDM2 = string.Empty;
            String sCDM3 = string.Empty;
            String sCDM4 = string.Empty;
            String sCIM = string.Empty;
            String sEPP = string.Empty;
            String sPRT = string.Empty;
            String sPRJ = string.Empty;
            String sIPM = string.Empty;
            if (SuppliesData != null)
            {
                foreach (DeviceSuppliesStatus_Type dev in SuppliesData.SuppliesStatus)
                {
                    switch (dev.ItemElementName.ToString())
                    {
                        case "CardCaptureBinSuppliesStatus":
                            {
                                SuppliesStatusField_Type SuppliesStatus = (SuppliesStatusField_Type)dev.Item;
                                sCRD = SuppliesStatus.ToString();
                                break;
                            }
                        case "CashHandlerSuppliesStatus":
                            {
                                CashHandlerSupplies_Type cashHandlerSupplies = (CashHandlerSupplies_Type)dev.Item;
                                sCDM0 = cashHandlerSupplies.RejectBinState.ToString();
                                sCDM1 = cashHandlerSupplies.CassetteType1State.ToString();
                                sCDM2 = cashHandlerSupplies.CassetteType2State.ToString();
                                sCDM3 = cashHandlerSupplies.CassetteType3State.ToString();
                                sCDM4 = cashHandlerSupplies.CassetteType4State.ToString();
                                break;
                            }
                        case "BunchNoteAcceptorSuppliesStatus":
                            {
                                BNASuppliesStatus_Type bNASuppliesStatus = (BNASuppliesStatus_Type)dev.Item;
                                sCIM = bNASuppliesStatus.BNAState.ToString();
                                break;
                            }
                        case "EncryptorSuppliesStatus":
                            {
                                SuppliesStatusField_Type SuppliesStatus = (SuppliesStatusField_Type)dev.Item;
                                sEPP = SuppliesStatus.ToString();
                                break;
                            }
                        case "ReceiptPrinterSuppliesStatus":
                            {
                                PrinterSuppliesData_Type printerSuppliesData = (PrinterSuppliesData_Type)dev.Item;
                                sPRT = printerSuppliesData.PrinterPaperStatus.ToString();
                                break;
                            }
                        case "JournalPrinterSuppliesStatus":
                            {
                                PrinterSuppliesData_Type printerSuppliesData = (PrinterSuppliesData_Type)dev.Item;
                                sPRJ = printerSuppliesData.PrinterPaperStatus.ToString();
                                break;
                            }
                        case "ChequeProcessingModuleSuppliesStatus":
                            {
                                CPMSuppliesStatus_Type cPMSuppliesStatus = (CPMSuppliesStatus_Type)dev.Item;
                                //sIPM = cPMSuppliesStatus.BinSuppliesStatusList[0].ToString();
                                break;
                            }
                    }
                }
            }
            if (FitnessData != null)
            {
                foreach (DeviceFitnessData_Type dev in FitnessData.HardwareFitnessData.Device)
                {
                    switch (dev.ItemElementName.ToString())
                    {
                        case "MagneticCardReaderWriterError":
                            {
                                sCRD = string.Format("{0} - {1}", sCRD, dev.Item);
                                break;
                            }
                        case "CashHandlerError":
                            {
                                CashHandlerErrorSeverity_Type cashHandlerErrorSeverity = (CashHandlerErrorSeverity_Type)dev.Item;
                                sCDM = string.Format("{0} - {1}", sCDM, cashHandlerErrorSeverity.CompleteDevice);
                                sCDM1 = string.Format("{0} - {1}", sCDM1, cashHandlerErrorSeverity.CassetteType1ErrorSeverity);
                                sCDM2 = string.Format("{0} - {1}", sCDM2, cashHandlerErrorSeverity.CassetteType2ErrorSeverity);
                                sCDM3 = string.Format("{0} - {1}", sCDM3, cashHandlerErrorSeverity.CassetteType3ErrorSeverity);
                                sCDM4 = string.Format("{0} - {1}", sCDM4, cashHandlerErrorSeverity.CassetteType4ErrorSeverity);
                                break;
                            }
                        case "BunchNoteAcceptorError":
                            {
                                BunchNoteAcceptorFitnessData_Type bunchNoteAcceptorFitnessData = (BunchNoteAcceptorFitnessData_Type)dev.Item;
                                sCIM = string.Format("{0} - {1}", sCIM, bunchNoteAcceptorFitnessData.CashAcceptorFitness);
                                break;
                            }
                        case "EncryptorError":
                            {
                                sEPP = string.Format("{0} - {1}", sEPP, dev.Item);
                                break;
                            }
                        case "ReceiptPrinterError":
                            {
                                sPRT = string.Format("{0} - {1}", sPRT, dev.Item);
                                break;
                            }
                        case "JournalPrinterError":
                            {
                                sPRJ = string.Format("{0} - {1}", sPRJ, dev.Item);
                                break;
                            }
                        case "CheckProcessingModuleError":
                            {
                                CheckProcessingModuleFitnessData_Type checkProcessingModuleFitnessData = (CheckProcessingModuleFitnessData_Type)dev.Item;
                                sIPM = string.Format("{0} - {1}", sIPM, checkProcessingModuleFitnessData.FitnessCPM);
                                break;
                            }
                            //case "BarcodeReaderError":
                            //    {
                            //        this.BAR = this.ModuleColors[3].ToString();//Verde
                            //        if ((ErrorSeverity_Type)dev.Item == ErrorSeverity_Type.Fatal)
                            //            this.BAR = this.ModuleColors[1].ToString();//Red indicator
                            //        break;
                            //    }
                            //case "SystemDisplayError":
                            //    {
                            //        this.LCD = this.ModuleColors[3].ToString();//Verde
                            //        if ((ErrorSeverity_Type)dev.Item == ErrorSeverity_Type.Fatal)
                            //            this.LCD = this.ModuleColors[1].ToString();//Red indicator
                            //        break;
                            //    }
                    }
                }
            }
            sbCounters.Append(string.Format("ESTADO DE MÓDULOS:{0}", Environment.NewLine));
            sbCounters.Append(string.Format("|{0}", Environment.NewLine));
            sbCounters.Append(string.Format("├>Lectora de Tarjetas    : {0}{1}", sCRD, Environment.NewLine));
            sbCounters.Append(string.Format("├>Impresora de Cliente   : {0}{1}", sPRT, Environment.NewLine));
            sbCounters.Append(string.Format("├>Dispensador de Billetes: Si{0}", Environment.NewLine));
            sbCounters.Append(string.Format("├>Presentador de Billetes: Si{0}", Environment.NewLine));
            sbCounters.Append(string.Format("└>Cartucho de Rechazos   : {0}{1}", sCDM0, Environment.NewLine));
            sbCounters.Append(string.Format("|           ├> Cartucho 1: {0}{1}", sCDM1, Environment.NewLine));
            sbCounters.Append(string.Format("|           ├> Cartucho 2: {0}{1}", sCDM2, Environment.NewLine));
            sbCounters.Append(string.Format("|           ├> Cartucho 3: {0}{1}", sCDM3, Environment.NewLine));
            sbCounters.Append(string.Format("|           └> Cartucho 4: {0}{1}", sCDM4, Environment.NewLine));
            sbCounters.Append(string.Format("├>Aceptador de Billetes  : {0}{1}", sCIM, Environment.NewLine));
            sbCounters.Append(string.Format("├>Impresora Auditora     : {0}{1}", sPRJ, Environment.NewLine));
            sbCounters.Append(string.Format("├>Auditora Electronica   : Si{0}", Environment.NewLine));
            sbCounters.Append(string.Format("├>Deposito de cheques    : {0}{1}", sIPM, Environment.NewLine));
            sbCounters.Append(string.Format("└>Teclado Encriptor      : {0}{1}", sEPP, Environment.NewLine));
            return sbCounters.ToString();
        }
    }
}