using System.Text;

namespace Entities
{
    public class Const
    {
        public static readonly string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string configPath = $"{Const.appPath}Config";
        public static readonly string key = "f173e890-13c8-4e0c-82c9-76be62cb9edf";
        public enum Severity
        {
            Info= 0,
            Warning= 1,
            Fatal = 2
        }

        public enum Fitness
        {
            NoError = 0,
            Routine = 1,
            Warning = 2,
            Suspend = 3,
            Fatal = 4
        }

        public enum Supplies
        {
            NoNewState = 0,
            GoodState = 1,
            MediaLow = 2,
            MediaOut = 3,
            Overfill = 4,
            NewState = 5,
            MediaNearFull = 6,
            Unknown = 7,
        }

        public enum TypeEnum
        {
            Type1 = 1,
            Type2 = 2,
            Type3 = 3,
            Type4 = 4,
            Type5 = 5,
            Type6 = 6,
            Type7 = 7
        }

        public enum TerminalMode { OutOfService, InSupervisor, OutOfSupervisor, InService, Suspend, OnLine, OffLine, InShipout }
        public enum TransactionType { CashDeposit, BagDropDeposit, EnvelopeDispenser, CashDispenser, ChequeDeposit, CoinDeposit }
        public enum LineMode { OffLine, OnLine }
        public enum InUseMode { InUse, NotInUse }
        public enum ActionOnCashAcceptError { Eject, Reset, EndSession, Retract, NoAction, Persist }
        public enum CountersType { Physical, Logical }
        public enum ActionOnPrinterError { EndSession, Ignore }
        public enum ActionOnTimeOut { EndSession, Persist }

        #region constantes char
        public const char
        #region hexa
            ESC = (char)0x1B,
            FS = (char)0x1C,
            GS = (char)0x1D,
            RS = (char)0x1E,
            TAB = (char)0x09,
            LF = (char)0x10,
        #endregion

        #region archivos de carga
            VarASCIIRecordType = 'A',
            VarBinaryRecordType = 'B',
            FixedRecordType = 'F';
        #endregion
        #endregion constantes char

        public enum Resolution { R640x480, R800x600, R600x1024, R1024x600, R1024x768, R1600x900, R768x1366, R800x480, R1280x1024, R768x1024, R1366x768 }

        public enum OperationMode { NDC, Batch, Mix }

        public enum EnvelopeState { Empty, Loaded, delivered }

        public enum ActivityResult
        {
            UnDefined = 0,

        }

        public enum MsgType
        {
            UnDefined = 0,
            Screen = 1, //Screen/Keyboard Data Load
            State = 2, //State Tables Load
            ConfParam = 3, //Configuration Parameters Load
            Fit = 4, //FIT Data Load
            ConfIDload = 5, //Configuration ID Number Load
            MacFieldSel = 6, //MAC Field Selection Load
            DateAndTime = 7, //Date and Time Load
            DispCurrCassetteMapping = 8, //Dispenser Currency Cassette Mapping Table
            XmlConfDownload = 9, //XML Configuration Download
            EncryptionKeyChange = 10, //Encryption Key Change
            ExtendedEncryptionKeyChange = 11, //Encryption Key Change
            //BillType = 5,
            //Template = 7,
            EnhConfParam = 12, //Enhanced Configuration Parameters Load
            Emv = 13,
            Aid = 14,
            Itr = 15, //Interactive Transaction Response
            TransactionReply = 16, //Interactive Transaction Response
            GoInServiceTermCmd = 17, //1...1
            GoOutOfServiceTermCmd = 18, //1...2
            SndConfIDTermCmd = 19, //1...3
            SndSupplyCountersTermCmd = 20, //1...4
            SndConfHwdTermCmd = 21, //1...71
            SndSuppiesStatusTermCmd = 22, //1...72
            SndHwdFitnessTermCmd = 23, //1...73
            SndSensorStatusTermCmd = 24, //1...74
            SndSoftIDTermCmd = 25, //1...75
            SndEnhConfDataTermCmd = 26, //1...76
            SndLocalConfTermCmd = 27, //1...77
            SndNoteDefinitionsTermCmd = 28, //1...78
        }

        //public const string CON_REL_DNI = "CON_REL_DNI";
        //public const string CON_REL_TAR = "CON_REL_TAR";
        //public const string VAL_PIN_DEB = "VAL_PIN_DEB";
        //public const string CON_SAL_CAH = "CON_SAL_CAH";
        //public const string CON_SAL_CCO = "CON_SAL_CCO";
        public const string PROXY_ERROR = "PRXY_ERR";

        public const bool
            DefaultTryRemoveNetworkHeaderIBM284 = true;
        public const int
            ByteLimitValue = 256;

        public static readonly Encoding
            DefaultEncoding = Encoding.Default,
            EBCDICEncoding = Encoding.GetEncoding("IBM284");

        //public static readonly Random r = new Random();
        //public static readonly string appPath = System.AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Color de paleta de esquema Bootstrap. Mas info: https://getbootstrap.com/docs/4.4/getting-started/theming/#theme-colors
        /// </summary>
        public static class Colors
        {
            public static readonly string Primary = "primary";
            public static readonly string Secondary = "secondary";
            public static readonly string Success = "success";
            public static readonly string Danger = "danger";
            public static readonly string Warning = "warning";
            public static readonly string Info = "info";
            public static readonly string Light = "light";
            public static readonly string Dark = "dark";
        }

        /// <summary>
        /// Iconos de libreria Boxicon. Mas info: https://boxicons.com/
        /// </summary>
        public static class Icons
        {
            public static readonly string Money = "bx bx-money";
            public static readonly string Envelope = "bx bxs-envelope";
            public static readonly string Exit = "bx bxs-log-out";
            public static readonly string Login = "bx bxs-log-in";
            public static readonly string Search = "bx bx-search-alt";
            public static readonly string Bank = "bx bxs-bank";
            public static readonly string Mobile = "bx bx-mobile";
            public static readonly string Barcode = "bx bxs-barcode";
            public static readonly string Keyboard = "bx bxs-keyboard";
        }

        public static class HostExtraDataKeys
        {
            public static string HostMessage1 = "HostMessage1";
            public static string HostMessage2 = "HostMessage2";
            public static string HostMessage3 = "HostMessage3";
            public static string NumOperation = "NumOperation";
        }

        public static class HostResponseExtraData
        {
            public static string NumOperation = "NumOperation";
            public static string HostMessage1 = "HostMessage1";
        }
    }
}

