using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{
    public class ErrorCodesTable
    {
        private Enums.Devices DeviceField;
        private Enums.DeviceStatus InternalCodeField; //Código interno generado por la app
        private string ExternalCodeField; //Código para enviar al exterior
        private Const.Severity SeverityField; //Define la severidad del retorno
        private bool SendErrorField;
        private string DeviceNameField; //Mensaje para enviar al exterior
        private string ExternalMessageField; //Mensaje para enviar al exterior

        public ErrorCodesTable()
        {
            this.InternalCode = 0;
            this.ExternalCode = "";
            this.Severity = Const.Severity.Fatal;
            this.SendError = false;
        }

        public ErrorCodesTable(Enums.Devices device, Enums.DeviceStatus internalError, string externalCode, Const.Severity severity, bool sendError, string deviceName, string externalMessage)
        {
            this.Device = device;
            this.InternalCode = internalError;
            this.ExternalCode = externalCode;
            this.Severity = severity;
            this.SendError = sendError;
            this.DeviceName = deviceName;
            this.ExternalMessage = externalMessage;
        }

        public static bool GetErrorData(out List<Entities.ErrorCodesTable> listTD)
        {
            bool ret = false;
            string fileName = string.Format(@"{0}\Config\ErrorCodesTable.xml", Entities.Const.appPath);
            listTD = new List<Entities.ErrorCodesTable>();
            try
            {
                if (!Directory.Exists(string.Format(@"{0}\Config", Entities.Const.appPath)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}\Config", Entities.Const.appPath));
                }
                //List of errors
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.UNK_Undefined, "000", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - ERROR DE ACEPTADOR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_DeviceError, "021", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM-ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_DeviceSuccess, "021R", Const.Severity.Info, true, "NOTEACCEPTOR", "CIM-SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_CashInError, "022", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - E2 E4 E6 ATASCO SUPERIOR DE BILLETES"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_CashInEndError, "023", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - E8 ATASCO INFERIOR DE BILLETES"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_CashInEndSuccess, "023R", Const.Severity.Info, true, "NOTEACCEPTOR", "CIM - E8 SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_StatusNotesInEscrow, "024", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - VALORES EN ESCROW PREVIOS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_RetractSuccess, "026R", Const.Severity.Info, true, "NOTEACCEPTOR", "CIM - RETRACT SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_RetractError, "026", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - RETRACT ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_RollBackError, "027", Const.Severity.Fatal, true, "NOTEACCEPTOR", "CIM - ROLLBACK ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_BagFillLevel_0, "BL000", Const.Severity.Info, true, "NOTEACCEPTOR", "CIM - BOLSA LLENA AL 0% DE CAPACIDAD"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_BagFillLevel_50, "BL050", Const.Severity.Warning, true, "NOTEACCEPTOR", "CIM - BOLSA LLENA AL 50% DE CAPACIDAD"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_BagFillLevel_75, "BL075", Const.Severity.Warning, true, "NOTEACCEPTOR", "CIM - BOLSA LLENA AL 75% DE CAPACIDAD"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_BagFillLevel_90, "BL090", Const.Severity.Warning, true, "NOTEACCEPTOR", "CIM - BOLSA LLENA AL 90% DE CAPACIDAD"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashAcceptor, Enums.DeviceStatus.CIM_CassetteFull, "025", Const.Severity.Warning, true, "NOTEACCEPTOR", "CIM - BOLSA LLENA"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_DeviceError, "031", Const.Severity.Fatal, true, "SENSORS", "SENSOR - ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_DeviceSuccess, "031R", Const.Severity.Info, true, "SENSORS", "SENSOR - SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CabinetDoorSensorOpen, "032", Const.Severity.Info, true, "SENSORS", "SENSOR - puerta de gabinete abierta"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CabinetDoorSensorClose, "032R", Const.Severity.Info, true, "SENSORS", "SENSOR - puerta de gabinete cerrada"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_ChestDoorSensorOpen, "033", Const.Severity.Info, true, "SENSORS", "SENSOR - puerta de tesoro abierta"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_ChestDoorSensorClose, "033R", Const.Severity.Info, true, "SENSORS", "SENSOR - puerta de tesoro cerrada"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CoverSensorOpen, "034R", Const.Severity.Info, true, "SENSORS", "SENSOR - bolsa no llena"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CoverSensorClose, "034", Const.Severity.Info, true, "SENSORS", "SENSOR - bolsa llena"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_PresenceSensorShipIn, "035R", Const.Severity.Info, true, "SENSORS", "SENSOR - bolsa colocada"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_PresenceSensorShipOut, "035", Const.Severity.Info, true, "SENSORS", "SENSOR - bolsa no colocada"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_PreDoorSensorOpen, "036", Const.Severity.Info, true, "SENSORS", "SENSOR - Puerta de pre-tesoro abierto"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_PreDoorSensorClose, "036R", Const.Severity.Info, true, "SENSORS", "SENSOR - Puerta de pre-tesoro cerrada"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CombSensorOpen, "037", Const.Severity.Info, true, "SENSORS", "SENSOR - Peine abierto"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.IOBoard, Enums.DeviceStatus.AIO_CombSensorClose, "037R", Const.Severity.Info, true, "SENSORS", "SENSOR - Peine cerrado"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.Printer, Enums.DeviceStatus.PRT_DeviceError, "044", Const.Severity.Fatal, true, "PRINTER", "PRT - Impresora Error desconocido"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.Printer, Enums.DeviceStatus.PRT_DeviceSuccess, "044R", Const.Severity.Info, true, "PRINTER", "PRT - SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.Printer, Enums.DeviceStatus.PRT_PaperLow, "045", Const.Severity.Warning, true, "PRINTER", "PRT - PAPER_LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.Terminal, Enums.DeviceStatus.TER_InService, "050R", Const.Severity.Info, true, "TERMINAL", "TER - IN SERVICE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.Terminal, Enums.DeviceStatus.TER_InSupervisor, "050", Const.Severity.Info, true, "TERMINAL", "TER - IN SUPERVISOR"));

                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_DeviceError, "060", Const.Severity.Fatal, true, "CASHDISPENSER", "CDM - ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_DeviceSuccess, "060R", Const.Severity.Info, true, "CASHDISPENSER", "CDM - SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_FraudAttempt, "061", Const.Severity.Fatal, true, "CASHDISPENSER", "CDM - FRAUD ATTEMPT"));

                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type1_GoodState, "062R", Const.Severity.Info, true, "CASHDISPENSER", "CDM - TYPE 1 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type1_MediaLow, "062", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 1 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type1_MediaOut, "063", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 1 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type2_GoodState, "064R", Const.Severity.Info, true, "CASHDISPENSER", "CDM - TYPE 2 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type2_MediaLow, "064", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 2 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type2_MediaOut, "065", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 2 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type3_GoodState, "066R", Const.Severity.Info, true, "CASHDISPENSER", "CDM - TYPE 3 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type3_MediaLow, "066", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 3 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type3_MediaOut, "067", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 3 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type4_GoodState, "068R", Const.Severity.Info, true, "CASHDISPENSER", "CDM - TYPE 4 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type4_MediaLow, "068", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 4 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CashDispenser, Enums.DeviceStatus.CDM_Type4_MediaOut, "069", Const.Severity.Warning, true, "CASHDISPENSER", "CDM - TYPE 4 - SUPPLY OUT"));

                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_DeviceError, "070", Const.Severity.Fatal, true, "COINDISPENSER", "COIN - ERROR"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_DeviceSuccess, "070R", Const.Severity.Info, true, "COINDISPENSER", "COIN - SUCCESS"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_1_GoodState, "071R", Const.Severity.Fatal, true, "COINDISPENSER", "COIN - HOPPER 1 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_1_MediaLow, "072", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 1 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_1_MediaOut, "071", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 1 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_2_GoodState, "073R", Const.Severity.Fatal, true, "COINDISPENSER", "COIN - HOPPER 2 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_2_MediaLow, "074", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 2 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_2_MediaOut, "073", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 2 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_3_GoodState, "075R", Const.Severity.Fatal, true, "COINDISPENSER", "COIN - HOPPER 3 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_3_MediaLow, "076", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 3 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_3_MediaOut, "075", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 3 - SUPPLY OUT"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_4_GoodState, "077R", Const.Severity.Fatal, true, "COINDISPENSER", "COIN - HOPPER 4 - SUPPLY GOOD STATE"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_4_MediaLow, "078", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 4 - SUPPLY LOW"));
                listTD.Add(new ErrorCodesTable(Enums.Devices.CoinDispenser, Enums.DeviceStatus.COIN_HOPPER_4_MediaOut, "077", Const.Severity.Warning, true, "COINDISPENSER", "COIN - HOPPER 4 - SUPPLY OUT"));

                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "108", Const.Severity.FATAL, true, "", "HARDWARE: Estado de la UPS desconocido"));
                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "108R", Const.Severity.INFO, true, "", "HARDWARE: REV Estado de la UPS desconocido"));
                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "109", Const.Severity.FATAL, true, "", "HARDWARE:  UPS: carga baja de baterias"));
                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "109R", Const.Severity.INFO, true, "", "HARDWARE:  REV UPS: carga baja de baterias"));
                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "110", Const.Severity.FATAL, true, "", "HARDWARE:  Backup UPS: funcionando con baterias"));
                //listTD.Add(new ErrorCodesTable(Enums.Devices.UPS, Enums.DeviceStatus.UNK_Undefined, "110R", Const.Severity.INFO, true, "", "HARDWARE:  REV Backup UPS: funcionando con baterias"));


                listTD = Utilities.Utils.GetGenericXmlData<List<Entities.ErrorCodesTable>>(out ret, fileName, listTD);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        //////////////////////////////////////////////////////////////////
        ////////////////////////PROPERTIES////////////////////////////////
        //////////////////////////////////////////////////////////////////
        [XmlAttributeAttribute(attributeName: "Device")]
        public Enums.Devices Device
        {
            get { return this.DeviceField; }
            set { this.DeviceField = value; }
        }

        [XmlAttributeAttribute(attributeName: "InternalCode")]
        public Enums.DeviceStatus InternalCode
        {
            get { return this.InternalCodeField; }
            set { this.InternalCodeField = value; }
        }

        [XmlAttributeAttribute(attributeName: "ExternalCode")]
        public string ExternalCode
        {
            get { return this.ExternalCodeField; }
            set { this.ExternalCodeField = value; }
        }

        [XmlAttributeAttribute(attributeName: "DeviceName")]
        public string DeviceName
        {
            get { return this.DeviceNameField; }
            set { this.DeviceNameField = value; }
        }

        [XmlAttributeAttribute(attributeName: "ExternalMessage")]
        public string ExternalMessage
        {
            get { return this.ExternalMessageField; }
            set { this.ExternalMessageField = value; }
        }

        [XmlAttributeAttribute(attributeName: "Severity")]
        public Const.Severity Severity
        {
            get { return this.SeverityField; }
            set { this.SeverityField = value; }
        }

        [XmlAttributeAttribute(attributeName: "SendError")]
        public bool SendError
        {
            get { return this.SendErrorField; }
            set { this.SendErrorField = value; }
        }
    }
}