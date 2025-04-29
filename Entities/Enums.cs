using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable()]
    public class Enums
    {
        public enum MessageType
        {
            ERRORS, LOGS, TRANSACTIONS, CONTENTS, GET_CONTENTS, GET_DEVICE_STATUS, GET_MACHINE_MESSAGES,
            GET_USERS, ADD_USERS, UPDATE_USERS,
            GET_USERS_PROFILE, ADD_USERS_PROFILE, UPDATE_USERS_PROFILE
        }

        public enum TransactionType { 
            NONE, FINDUSER, DEPOSIT, DEPOSIT_DECLARED, SEND_CONTENTS, COLLECTION, COLLECTION_DECLARED, DISPENSE, REFILL, MOVE_IN, MOVE_OUT, GET_ACCOUNTS, COIN_DEPOSIT, CHEQUE_DEPOSIT,
            PAYMENT_FINDSERVICE, PAYMENT_INIT, PAYMENT_PROCESS, PAYMENT_SUMMARY, PAYMENT_ADDTOCART, PAYMENT_GETCART, PAYMENT_REMOVEFROMCART, PAYMENT_EMTPYCART, PAYMENTS_EXECUTE, PAYMENT_RECEIPT, SENDCASHOUT, SENDCASHIN, INQUIRY
        }

        public enum Results
        {
            UNK,
            SUCCESS,
            TIMEOUT,
            HWERROR,
            REJECT,
            NOTELIST_EMPTY,
            CANCEL
        }

        public enum Devices { Unknown, Terminal, CashAcceptor, CashDispenser, Printer, StatementPrinter, FingerPrintReader, BarcodeReader, ADM, CoinDispenser, IOBoard, CardReader, Camera, Pinpad, SensorsAndIndicators, UPS, Bag, Host }

        public enum Commands
        {
            UNK,
            Event,
            Open,
            Close,
            CashInStart,
            CashIn,
            CashInEnd,
            Cancel,
            RollBack,
            Reject,
            Retract,
            Present,
            Status,
            Reset,
            Test,
            OpenRetractShutter,
            CloseRetractShutter,
            OpenEscrowShutter,
            CloseEscrowShutter,
            OpenRejectShutter,
            CloseRejectShutter,
            OpenInputShutter,
            CloseInputShutter,
            GetBankNoteTypes,
            ConfigureNoteTypes,
            GetCounters,
            ClearCounters,
            PrintRawData,
            PrintForm,
            StartScan,
            StopScan,
            Invoke,
            Show,
            Home,
            Dispense,
            Load,
            Capabilities,
            TakePic,
            State,
            StartShaker,
            ShipOut,
            ShipIn,
            GoToSupervisor,
            OutOfSupervisor,
            Init,
            GetUserData,
            GetUsers,
            AddUser,
            EditUser,
            DeleteUser,
            FindInUsers,
            ImportKey,
            GetKeyPressData,
            CashUnitInfo,
            PresentStatus,
            AddCash,
            Enable,
            GetSerial,
            GetTerminalInfo
        }

        [SerializableAttribute()]
        public enum DeviceStatus
        {
            UNK_Undefined = 0,
            AIO_DeviceError = 1,
            AIO_DeviceSuccess = 2,
            AIO_PresenceSensorShipIn = 3,
            AIO_PresenceSensorShipOut = 4,
            AIO_ChestDoorSensorOpen = 5,
            AIO_ChestDoorSensorClose = 6,
            AIO_CabinetDoorSensorOpen = 7,
            AIO_CabinetDoorSensorClose = 8,
            AIO_CoverSensorOpen = 9,
            AIO_CoverSensorClose = 10,
            PRT_DeviceError = 11,
            PRT_DeviceSuccess = 12,
            PRT_PaperLow = 13,
            CIM_DeviceError = 14,
            CIM_DeviceSuccess = 15,
            CIM_CashInError = 16,
            CIM_CashInEndError = 17,
            CIM_CashInEndSuccess = 18,
            CIM_StatusNotesInEscrow = 19,
            CIM_RetractSuccess = 20,
            CIM_RetractError = 21,
            CIM_BagFillLevel_0 = 22,
            CIM_BagFillLevel_50 = 23,
            CIM_BagFillLevel_75 = 24,
            CIM_BagFillLevel_90 = 25,
            CIM_CassetteFull = 26,
            CIM_RollBackError = 27,
            TER_InSupervisor = 28,
            TER_InService = 29,
            CDM_DeviceSuccess = 30,
            CDM_DeviceError = 31,
            CDM_FraudAttempt= 32,
            CDM_DeviceWarning = 33,
            CDM_Type1_GoodState = 34,
            CDM_Type1_MediaLow = 35,
            CDM_Type1_MediaOut = 36,
            CDM_Type1_Error = 37,
            CDM_Type2_GoodState = 38,
            CDM_Type2_MediaLow = 39,
            CDM_Type2_MediaOut = 40,
            CDM_Type2_Error = 41,
            CDM_Type3_GoodState = 42,
            CDM_Type3_MediaLow = 43,
            CDM_Type3_MediaOut = 44,
            CDM_Type3_Error = 45,
            CDM_Type4_GoodState = 46,
            CDM_Type4_MediaLow = 47,
            CDM_Type4_MediaOut = 48,
            CDM_Type4_Error = 49,
            CDM_RetractSuccess = 50,
            CDM_RetractError = 51,
            COIN_DeviceError = 52,
            COIN_DeviceSuccess = 53,
            COIN_HOPPER_1_GoodState = 54,
            COIN_HOPPER_1_MediaLow = 55,
            COIN_HOPPER_1_MediaOut = 56,
            COIN_HOPPER_2_GoodState = 57,
            COIN_HOPPER_2_MediaLow = 58,
            COIN_HOPPER_2_MediaOut = 59,
            COIN_HOPPER_3_GoodState = 60,
            COIN_HOPPER_3_MediaLow = 61,
            COIN_HOPPER_3_MediaOut = 62,
            COIN_HOPPER_4_GoodState = 63,
            COIN_HOPPER_4_MediaLow = 64,
            COIN_HOPPER_4_MediaOut = 65,
            COIN_HOPPER_5_GoodState = 66,
            COIN_HOPPER_5_MediaLow = 67,
            COIN_HOPPER_5_MediaOut = 68,
            BCR_DeviceError = 69,
            BCR_DeviceSuccess = 70,
            AIO_PreDoorSensorOpen = 71,
            AIO_PreDoorSensorClose = 72,
            AIO_CombSensorOpen = 73,
            AIO_CombSensorClose = 74
        }

        public enum UserRoles
        {
            USER,
            SUPERVISOR,
            ADMIN
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ExtraDataType
        {
            [EnumMember(Value = "channel")]
            channel,
            [EnumMember(Value = "txInfo")]
            txInfo,
            [EnumMember(Value = "txRef")]
            txRef,
            [EnumMember(Value = "shifts")]
            shifts,
            [EnumMember(Value = "collectionId")]
            collectionId,
            [EnumMember(Value = "currency")]
            currency,
            [EnumMember(Value = "amountLimit")]
            amountLimit,
            [EnumMember(Value = "dynamic")]
            dynamic,
            [EnumMember(Value = "displayOnly")]
            displayOnly,
            [EnumMember(Value = "phoneNumber")]
            phoneNumber,
            [EnumMember(Value = "helpDesk")]
            helpDesk
        }

        public enum ControlType
        {
            input,
            select
        }

        [System.SerializableAttribute()]
        public enum boxtype
        {

            /// <comentarios/>
            COINACCEPTOR,

            /// <comentarios/>
            COINDISPENSER,

            /// <comentarios/>
            NOTEDISPENSER,

            /// <comentarios/>
            NOTEACCEPTOR,

            /// <comentarios/>
            DROPSAFE,

            /// <comentarios/>
            TRANSPORTBOX,

            /// <comentarios/>
            NOTESREJECTED,

            /// <comentarios/>
            COINRECYCLER,

            /// <comentarios/>
            NOTERECYCLER,
        }

        public enum Phases
        {
            Phase_0,
            Phase_1,
            Phase_2,
            Phase_3,
            Phase_4,
        }

        public enum TerminalModel
        {
            NONE,
            SNBC_CTE1,
            SNBC_CTI90,
            Glory_DE_50,
            GRG_P2600,
            MiniBank_JH6000_D,
            MiniBank_JH600_A,
            ADM,
            CTR50,
            TAS1,
            CTIUL,
            V200,
            MADERO_BR,
            SNBC_IN,
            BDM_300,
            Depositario,
            Glory_DE_70
        }  

        public enum Branding
        {
            Prosegur,
            Macro,
            Galicia,
            PlanB,
            RedPagosA,
            Ciudad,
            Coto,
            DepositarioRetail,
            Atlas,
            GNB,
            FIC,
            DepositarioBancos
        }

        public enum CimModel
        {
            SNBC,
            Glory,
            GRG,
            MEI,
            None
        }

        public enum IOBoardModel
        {
            AIO,
            PicBoard,
            SIU,
            BtLNX,
            NONE
        }

        public enum BarcodeModel
        {
            ZBCR,
            Opticon_M11, //Lector frontal
            Opticon_L51X, //Pistola Opticon
            XFS,
            NONE
        }

        public enum PrinterModel
        {
            [XmlEnum(Name = "58mm Series Printer")]//BDM-300
            SPRT,
            [XmlEnum(Name = "NPI Integration Driver")]//Glory ARG
            NII_Printer_DS,
            [XmlEnum(Name = "BK-C310(U) 1")]
            BK_C310,
            [XmlEnum(Name = "PLUSII")]//Colombia
            PLUSII,
            [XmlEnum(Name = "POS-80C")]//Paraguay
            POS_80,
            [XmlEnum(Name = "EPSON TM-T82III Receipt")]//India
            EPSON_TM_T82III,
            [XmlEnum(Name = "EPSON TM-T82II Receipt")]//India
            EPSON_TM_T82II,
            [XmlEnum(Name = "XFS")]
            XFS,
            [XmlEnum(Name = "None")]
            NONE
        }

        public enum AvTxn
        {
            cashDepositTx,
            cashWithdrawalTx,
            paymentServicesTx,
            mobileTopupTx,
            none
        }

        public enum ProcessAction
        {
            StartProcess,
            KillProcess,
            StartService,
            StopService
        }
    }
}
