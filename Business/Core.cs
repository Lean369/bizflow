using Entities;
using Entities.PaymentService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Utilities;
using static Entities.Const;

namespace Business
{
    public class Core
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");

        #region "Eventos"
        //1)- Evento que informa las teclas FDK que fueron presionadas por pantalla de cliente.
        public delegate void DelegateSendFDKscreenPress(string FDKdata);
        public event DelegateSendFDKscreenPress EvtFDKscreenPress;
        //2)- Evento que informa las teclas NO FDK (ENTER, CANCEL, CLEAR) que fueron presionadas por pantalla de cliente.
        public delegate void DelegateSendEvtOthersKeysPress(string OthersKeysdata);
        public event DelegateSendEvtOthersKeysPress EvtOthersKeysPress;
        //3)- Evento que informa los datos ingresados a través de los input de pantalla de cliente y envío de datos generales desde la UI
        public delegate void DelegateSendInputData(string inputData, string dataLink);
        public event DelegateSendInputData EvtInputData;
        //4)- Evento que envía un documento HTML a renderizar
        public delegate void DelegateSendHtmlData(StateEvent stateEvent);
        public event DelegateSendHtmlData EvtShowHtmlData;
        //5)- Evento que invoca la ejecución de una función javascript.
        //public delegate void DelegateInvokeJavascript(string functionName, object param);
        //public event DelegateInvokeJavascript EvtInvokeJavascript;
        //6)- Evento que informa los datos del Transaction Reply e ITR recibidos desde el DH
        public event DelegateRcvMsgReply EvtRcvMsgReply;
        public delegate void DelegateRcvMsgReply(object msg);
        //7)- Evento que informa el cambio de modo del ATM
        public delegate void DelegateChangeTerminalMode(Const.TerminalMode terminalMode);
        public event DelegateChangeTerminalMode EvtChangeTerminalMode;
        #endregion "Eventos"

        public string AppVersion = string.Empty;
        public AlephATMAppData AlephATMAppData { get; set; } //Datos de aplicación leidos desde archivo.
        public TerminalInfo TerminalInfo; //Datos de terminal leidos desde archivo.
        internal Counters Counters; //Guarda contadores del ATM
        public ScreenConfiguration ScreenConfiguration; //Configuración de pantalla.
        internal Thread ThreadTransitionState; //Hilo para ejecutar transiciones de estados.
        public TransitionHandler TransitionStateFlow; //Objeto para control de transición de estados.
        public TransitionHandler StateExecute; //Objeto ejecutar estados por fuera del flujo de transición.
        public string CurrentTransitionState = string.Empty; //Variable que contiene el número del estado en ejecución actual
        public string CurrentStateName = string.Empty; //Variable que contiene el nombre del estado en ejecución actual
        public string NextTransitionState = "000"; //Variable para activar las transiciones de estados.
        public string ErrorTransitionState = string.Empty; //Variable que contiene el nombre del estado que generó error.
        private bool FlagFirstStateTransition = true; //Indica si se inicia la transición por primera vez
        internal bool ShipoutMutex = true; //Indica si el proceso de Shipout esta finalizado
        public Download Download;
        public NDChost NDChost;
        public Parser Parser;
        private HtmlGenerator HtmlGenerator; //Objeto para armado de pantallas HTML
        public BusinessObject Bo; //Objetos para almacenar datos de transacción (volátiles)
        public SDO Sdo;
        private Queue<Const.TerminalMode> QueueAttemptChangeMode;
        private System.Timers.Timer TimerAttemptChangeMode;
        internal MoreTimeConfigurationType MoreTimeConfig;
        private static Dictionary<string, AlephHost> MappingHosts = new Dictionary<string, AlephHost>();
        private Dictionary<string, object> ShipoutExtraData = new Dictionary<string, object>();

        public Core(out bool initSuccess)
        {
            initSuccess = false;
            if (this.Startup()) //Startup Core
            {
                initSuccess = true;
                //0.00417 = 15 sec . 0.00139 = 5 sec
                TaskScheduler.Instance.ScheduleTask(00, 05, 24,
                () =>
                {
                    this.StartOfDay();
                });
                WriteEJ($"==>STARTING ALEPHATM v.{this.AppVersion}==>", true);
            }
            else
            {
                this.WriteEJ("==>STARTUP ALEPHATM ERROR==>", true);
            }
        }

        private void StartOfDay()
        {
            Log.Debug("/--->");
            this.WriteEJ($">>TERMINAL NUMBER : {this.TerminalInfo.LogicalUnitNumber}", true);
            this.WriteEJ($">>ALEPHATM VERSION: {this.AppVersion}", true);
            this.WriteEJ($">>TERMINAL MODEL  : {this.AlephATMAppData.TerminalModel}", true);
            this.WriteEJ($">>BRANDING        : {this.AlephATMAppData.Branding}", true);
        }

        #region "-------HOST--------"
        public void MapHost(string hostName, AlephHost alephHost)
        {
            MappingHosts.Add(hostName, alephHost);
        }

        internal AlephHost GetHostObject(string hostName)
        {
            AlephHost alephHost;
            if (MappingHosts.Keys.Contains(hostName))
            {
                alephHost = MappingHosts[hostName];
            }
            else
            {
                alephHost = MappingHosts[this.AlephATMAppData.DefaultHostName];
            }
            Log.Info($"Selected host: {alephHost.Host.GetType().Name}");
            return alephHost;
        }

        internal string GetCollectionId(Enums.TransactionType transactionType)
        {
            string tagValue = "";
            try
            {
                if (this.AlephATMAppData.AppendCollectionId)
                {
                    if (this.Bo == null)
                        this.Bo = new BusinessObject();
                    if (this.Bo.ExtraInfo == null)
                        this.Bo.ExtraInfo = new ApplVisitExtraInfo();
                    if (this.Bo.ExtraInfo.ExtraData == null)
                        this.Bo.ExtraInfo.ExtraData = new List<ExtraData>();
                    switch (transactionType)
                    {
                        case Enums.TransactionType.DEPOSIT:
                        case Enums.TransactionType.COLLECTION:
                            tagValue = $"N{Counters.GetCOLLECTIONID()}";
                            break;
                        case Enums.TransactionType.DEPOSIT_DECLARED:
                        case Enums.TransactionType.COLLECTION_DECLARED:
                            tagValue = $"D{Counters.GetCOLLECTIONID()}";
                            break;
                        case Enums.TransactionType.COIN_DEPOSIT:
                            tagValue = $"C{Counters.GetCOLLECTIONID()}";
                            break;
                        case Enums.TransactionType.CHEQUE_DEPOSIT:
                            tagValue = $"K{Counters.GetCOLLECTIONID()}";
                            break;
                        default:
                            Log.Error("Unknown transaction type: {0}", transactionType);
                            break;
                    }
                }
                Log.Info($"TransactionType: {transactionType} - Tag value: {tagValue}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return tagValue;
        }

        internal AuthorizationResult AuthorizeTransaction(Enums.TransactionType transactionType, Contents contents, string hostName)
        {
            AuthorizationResult authorizationResult = null;
            string empty = string.Empty;
            try
            {
                Log.Debug("/---> {0}", transactionType.ToString());
                string host = string.IsNullOrEmpty(hostName) ? this.AlephATMAppData.DefaultHostName : hostName;
                string transactionNumber = this.Counters.GetTSN().ToString("0000");
                int batch = this.Counters.GetBATCH();
                authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, transactionNumber);
                AlephHost hostObject = this.GetHostObject(host);
                switch (transactionType)
                {
                    case Enums.TransactionType.FINDUSER:
                        //authorizationResult = hostObject.Host.FindUserByLogin(this.User);
                        break;
                    case Enums.TransactionType.DEPOSIT:
                    case Enums.TransactionType.DEPOSIT_DECLARED:
                        authorizationResult = hostObject.Host.CashDeposit(batch, transactionType, transactionNumber, contents, this.Bo.ExtraInfo.HostExtraData);
                        break;
                    case Enums.TransactionType.SEND_CONTENTS:
                        authorizationResult = hostObject.Host.SendContents(contents);
                        break;
                    case Enums.TransactionType.COLLECTION:
                        this.LoadShipoutExtraData("userProfileShipout", this.Sdo.ShipoutUser);
                        authorizationResult = hostObject.Host.SendCollection(transactionNumber, contents, batch, this.ShipoutExtraData);
                        break;
                    case Enums.TransactionType.COLLECTION_DECLARED:
                        this.LoadShipoutExtraData("userProfileShipout", this.Sdo.ShipoutUser);
                        authorizationResult = hostObject.Host.SendCollectionDeclared(transactionNumber, contents, batch, this.ShipoutExtraData);
                        break;
                    case Enums.TransactionType.GET_ACCOUNTS:
                        authorizationResult = hostObject.Host.CashDeposit(batch, transactionType, transactionNumber, contents, this.Bo.ExtraInfo.HostExtraData);
                        break;
                    case Enums.TransactionType.DISPENSE:
                        authorizationResult = hostObject.Host.CashDispense(batch, transactionType, transactionNumber, contents, 0, this.Bo.ExtraInfo.HostExtraData);
                        break;
                    case Enums.TransactionType.PAYMENT_FINDSERVICE:
                        authorizationResult = hostObject.Host.PaymentFindService((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_INIT:
                        authorizationResult = hostObject.Host.PaymentInit((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_PROCESS:
                        authorizationResult = hostObject.Host.PaymentProcess((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_SUMMARY:
                        authorizationResult = hostObject.Host.PaymentSummary((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_ADDTOCART:
                        authorizationResult = hostObject.Host.PaymentAddToCart((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_GETCART:
                        authorizationResult = hostObject.Host.PaymentGetCart((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_REMOVEFROMCART:
                        authorizationResult = hostObject.Host.PaymentRemoveFromCart((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_EMTPYCART:
                        authorizationResult = hostObject.Host.PaymentEmptyCart();
                        break;
                    case Enums.TransactionType.PAYMENTS_EXECUTE:
                        authorizationResult = hostObject.Host.PaymentsExecute((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.PAYMENT_RECEIPT:
                        authorizationResult = hostObject.Host.PaymentGetReceipt((Dictionary<Entities.PaymentService.PropKey, object>)this.Bo.ExtraInfo.HostExtraData[PSConst.PAYMENT_SERVICE_DATA]);
                        break;
                    case Enums.TransactionType.SENDCASHIN:
                        authorizationResult = hostObject.Host.SendCashin(contents, batch, this.Bo.ExtraInfo.HostExtraData);
                        break;
                    case Enums.TransactionType.SENDCASHOUT:
                        authorizationResult = hostObject.Host.SendCashin(contents, batch, this.Bo.ExtraInfo.HostExtraData);
                        break;

                }

                AddHEDFromAuthorization(authorizationResult);

                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                {
                    Log.Info($"--> TX {transactionType} #{transactionNumber} AUTHORIZED BY {host}");
                    this.WriteEJ($"TX {transactionNumber} ({transactionType}) AUTHORIZED BY {host}");
                }
                else
                {
                    Log.Warn($"-->TX {transactionType} #{transactionNumber} DECLINED BY {host}");
                    this.WriteEJ($"TX {transactionNumber} ({transactionType}) DECLINED BY {host}");
                }
            }
            catch (KeyNotFoundException ex) { Log.Fatal("KeyNotFoundException: {0} {1} ", ex.Message, ex.InnerException?.Message ?? "| (NoInnerMsg)"); }
            catch (Exception ex) { Log.Fatal(ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Agrega los datos extra del host al HostExtraData de la transacción.
        /// </summary>
        /// <param name="result"></param>
        private void AddHEDFromAuthorization(AuthorizationResult result)
        {
            try
            {
                if (result.HostResponseExtraData != null)
                {
                    foreach (var item in result.HostResponseExtraData)
                    {
                        if (this.Bo.ExtraInfo.HostExtraData.ContainsKey(item.Key))
                            this.Bo.ExtraInfo.HostExtraData.Remove(item.Key);
                        this.Bo.ExtraInfo.HostExtraData.Add(item.Key, item.Value);
                        Log.Info($"Key name \"{item.Key}\" added ok");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void AddHostExtraData(string keyName, object extraData)
        {
            try
            {
                if (extraData != null)
                {
                    if (this.Bo.ExtraInfo.HostExtraData.ContainsKey(keyName))
                        this.Bo.ExtraInfo.HostExtraData.Remove(keyName);//Si existe una clave igual, la borro
                    this.Bo.ExtraInfo.HostExtraData.Add(keyName, extraData);
                    Log.Info($"Key name \"{keyName}\" added ok");
                }
                else
                    Log.Warn($"Extra data is null");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Agrega datos de servicio de pago al HostExtraData ya existente.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        public void AddPaymentServiceData(string keyName, KeyValuePair<PropKey, object> value)
        {
            if (this.Bo.ExtraInfo.HostExtraData != null)
            {
                if (this.Bo.ExtraInfo.HostExtraData.ContainsKey(keyName))
                {
                    Dictionary<PropKey, object> serviceData = this.Bo.ExtraInfo.HostExtraData[keyName] as Dictionary<PropKey, object>;
                    if (serviceData != null)
                    {
                        serviceData.Add(value.Key, value.Value);

                        this.Bo.ExtraInfo.HostExtraData[keyName] = serviceData;
                    }
                }
            }
        }

        private void LoadShipoutExtraData(string keyName, object extraData)
        {
            if (extraData != null)
            {
                if (this.ShipoutExtraData.ContainsKey(keyName))
                    this.ShipoutExtraData.Remove(keyName);//Si existe una clave igual, la borro
                this.ShipoutExtraData.Add(keyName, extraData);
                Log.Info($"Key name \"{keyName}\" added ok");
            }
            else
                Log.Warn($"Extra data is null");
        }

        internal AuthorizationResult SendTicketData(string jsonData, int tsn)
        {
            AuthorizationResult authorizationResult = null;
            try
            {
                authorizationResult = this.GetHostObject("PrdmHost").Host.SendTicketData(jsonData, tsn);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Envía contadores a través del socket cliente del PRDM
        /// </summary>
        internal void SendContentsToHost()
        {
            Contents contents;
            AuthorizationResult authorizationResult;
            AlephHost alephHost;
            try
            {
                Log.Debug("/--->");
                contents = this.Counters.GetContents();
                alephHost = this.GetHostObject("");
                authorizationResult = alephHost.Host.SendContents(contents);
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                    Log.Info("Send contents ok");
                else
                    Log.Warn("Send contents error");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Envía contadores a través del socket server del PRDM (Balance)
        /// </summary>
        internal void SendGetContentsToHost()
        {
            Contents contents = null;
            AuthorizationResult authorizationResult;
            AlephHost alephHost = null;
            try
            {
                Log.Debug("/--->");
                contents = this.Counters.GetContents();
                alephHost = this.GetHostObject("");
                authorizationResult = alephHost.Host.SendGetContents(contents);
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                    Log.Info("Send get contents ok");
                else
                    Log.Error("Send get contents error");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Procesa los requerimientos provenietes del Host
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessHostRequest(string msg)
        {
            try
            {
                Log.Debug($"msg: {msg}");
                if (msg.Equals("CONTENTS"))
                {
                    this.SendContentsToHost();
                }
                else if (msg.Equals("GET_CONTENTS"))
                {
                    this.SendGetContentsToHost();
                }
                else if (msg.Equals("GET_DEVICE_STATUS"))
                {
                    //TODO: enviar status
                }
                else if (msg.Contains("AUTHORIZED BY"))
                    this.WriteEJ(msg);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "-------HOST--------"

        #region "-------AM - Application Manager--------"
        /// <summary>WriteEJ
        /// Lanza el evento para informar los datos del Transaction Reply e ITR recibidos desde el DH para luego ser capturado por el estado I.
        /// </summary>
        /// <param name="html">Mensaje de Log.</param>
        public virtual void RaiseEvtRcvMsgReply(object msg)
        {
            DelegateRcvMsgReply tmp = EvtRcvMsgReply;
            if (tmp != null)
                tmp(msg);
        }

        internal void StartSupervisorMode(bool logicSupervisorEntry)
        {
            try
            {
                string arg = logicSupervisorEntry ? "Logic Supervisor entry" : "Sensor Supervisor entry";
                this.RequestChangeMode(Const.TerminalMode.InSupervisor);
                Log.Info($"{arg}");
                WriteEJ($"==> {arg}");
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
        }

        internal void StartShipoutMode(bool shipoutLogic)
        {
            try
            {
                string arg = shipoutLogic ? "Logic Shipout entry" : "Sensor Shipout entry";
                this.RequestChangeMode(Const.TerminalMode.InShipout);
                Log.Info($"{arg}");
                WriteEJ($"==> {arg}");
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
        }

        /// <summary>
        /// Lanza el evento para informar el cambio de modo del ATM
        /// </summary>
        /// <param name="html">Mensaje de Log.</param>
        protected virtual void RaiseChangeTerminalMode(Const.TerminalMode terminalMode)
        {
            DelegateChangeTerminalMode tmp = EvtChangeTerminalMode;
            if (tmp != null)
                tmp(terminalMode);
        }

        internal void RequestChangeMode(Const.TerminalMode mode)
        {
            try
            {
                Log.Debug("/--->");
                this.QueueAttemptChangeMode.Enqueue(mode);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Gestiona los pedidos de cambio de modo de operación.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void TimerAttemptChangeMode_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            Const.TerminalMode mode;
            try
            {
                if (this.QueueAttemptChangeMode.Count != 0)
                {
                    this.TimerAttemptChangeMode.Stop();
                    mode = this.QueueAttemptChangeMode.Peek();
                    if (this.AttemptChangeMode(mode))
                        mode = this.QueueAttemptChangeMode.Dequeue();
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            finally { this.TimerAttemptChangeMode.Start(); }
        }

        /// <summary>
        /// Intenta cambiar el modo de operación si el equipo no se encuentra en uso
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private bool AttemptChangeMode(Const.TerminalMode mode)
        {
            bool ret = false;
            try
            {
                if (this.Sdo.SOH.InUseState == Const.InUseMode.NotInUse)
                {
                    this.Sdo.SOH.Mode = mode;
                    Log.Info($"MODE: {mode}");
                    this.WriteEJ($"<TERMINAL MODE: {mode}>");

                    switch (mode)
                    {
                        case Const.TerminalMode.InService:
                            this.GoToInServiceMode();
                            break;
                        case Const.TerminalMode.InSupervisor:
                            this.SetNextState(StateTransition.StateResult.SUSPEND, AlephATMAppData.StateSupervisor);
                            break;
                        case Const.TerminalMode.OutOfService:
                            this.GoToOutOfServiceMode();
                            break;
                        case Const.TerminalMode.Suspend:
                            this.GoToSuspendMode();
                            break;
                        case Const.TerminalMode.OffLine:
                            this.GoToOffLineMode();
                            break;
                        case Const.TerminalMode.OnLine:
                            this.GoToOnLineMode();
                            break;
                        case Const.TerminalMode.InShipout:
                            this.SetNextState(StateTransition.StateResult.SUSPEND, AlephATMAppData.StateShipout);
                            break;
                    }
                    ret = true;
                    this.RaiseChangeTerminalMode(mode);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Actualiza el Flag de InService e inicia el flujo de transición de estados.
        /// </summary>
        internal void GoToInServiceMode()
        {
            try
            {
                Log.Debug("/--->");
                this.Sdo.SOH.Mode = Const.TerminalMode.InService;
                if (this.FlagFirstStateTransition)
                {
                    this.FlagFirstStateTransition = false;
                    this.TransitionStateFlow = new TransitionHandler(this);
                    this.ThreadTransitionState = new Thread(new ThreadStart(this.TransitionStateFlow.Work));
                    this.ThreadTransitionState.Start(); //Inicia el hilo que maneja las transiciones de estados.
                }
                else
                {
                    this.NextTransitionState = "000"; //Inicia siempre en el estado "000"
                    this.TransitionStateFlow.Play();
                }
                if (this.AlephATMAppData.OperationMode == Const.OperationMode.NDC)
                    this.SendReadyStatus("9");//TODO: Falta verificar si se puso en servicio correctamente.
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Actualiza el Flag de InService y detiene el flujo de transición de estados.
        /// </summary>
        private void GoToOutOfServiceMode()
        {
            try
            {
                Log.Debug("/--->");
                this.PauseTransactionFlow("C02.htm");
                this.SendReadyStatus("9");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Lleva la terminal al modo On line
        /// </summary>
        private void GoToOnLineMode()
        {
            try
            {
                Log.Debug("/--->");
                this.Sdo.SOH.Line = Const.LineMode.OnLine; //Setea el flag de OnLine
                                                           //this.UpdateStateOfHealth();
                                                           //this.ShowGeneralNDCScreen("C01", null);
                StateEvent stateEvent = new StateEvent(StateEvent.EventType.navigate, "C03.htm", "");
                this.RaiseEvtScreenData(stateEvent);
                //this.Navigate("C03.htm");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Lleva la terminal al modo Off line
        /// </summary>
        private void GoToOffLineMode()
        {
            try
            {
                Log.Debug("/--->");
                this.Sdo.SOH.Line = Const.LineMode.OffLine; //Setea el flag de OnLine
                this.PauseTransactionFlow("C01.htm");

            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void GoToSuspendMode()
        {
            try
            {
                Log.Debug("/--->");
                this.PauseTransactionFlow("C05.htm");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Proyecta una pantalla y cierra los estados en curso de los siguientes modos de operación:
        /// -Off line
        /// -Out of Supervisor
        /// -Out of service
        /// </summary>
        /// <param name="screen"></param>
        private void PauseTransactionFlow(string screen)
        {
            try
            {
                if (this.TransitionStateFlow != null)
                {
                    this.TransitionStateFlow.VerifyAndCloseProcessState();//Cierro todos los posibles estados en curso
                    if (!this.TransitionStateFlow.paused)
                        this.TransitionStateFlow.Pause();
                }
                StateEvent stateEvent = new StateEvent(StateEvent.EventType.navigate, screen, "");
                this.RaiseEvtScreenData(stateEvent);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Guarda los datos recibidos del download en archivos XML
        /// </summary>
        internal void UpdateDownloadXMLfiles()
        {
            Log.Debug("/--->");
            this.SerializeStates(this.Download.StateTables);
            this.SerializeScreens(this.Download.ScreenData);
            if (this.Download.simulatedPrePrintedReceiptScreenR00 != null)
                this.SerializeSimulatedPrePrintedReceiptScreen(this.Download.simulatedPrePrintedReceiptScreenR00);
            if (this.Download.simulatedPrePrintedReceiptScreenR01 != null)
                this.SerializeSimulatedPrePrintedReceiptScreen(this.Download.simulatedPrePrintedReceiptScreenR01);
            this.SerializeEnhParameters(this.Download.EnhancedConfigurationParametersData);
            this.UpdateStateTransition();
        }

        /// <summary>
        /// Envía una respuesta Ready al DH.
        /// </summary>
        /// <param name="ready"></param>
        internal void SendReadyStatus(string ready)
        {
            if (this.AlephATMAppData.OperationMode == Const.OperationMode.NDC)
                this.NDChost.SendReadyStatus(ready);
        }

        /// <summary>
        /// Envía una respuesta Ready al DH.
        /// </summary>
        /// <param name="ready"></param>
        internal void SendEspecificCommandRejectStatus(string statusValue)
        {
            this.NDChost.SendEspecificCommandRejectStatus(statusValue);
        }
        #endregion

        #region "States"
        /// <summary>
        /// Genera un pasaje al estado indicado.
        /// </summary>
        /// <param name="nextState"></param>
        internal void SetNextState(StateTransition.StateResult result, string nextState)
        {
            Log.Info($"/--> Next State: {nextState} - Result: {result}");
            this.NextTransitionState = nextState; //Aqui genera la transición al estado indicado.
        }

        /// <summary>
        /// Guarda la tabla de estados en un archivo XML luego de completarse el Download.
        /// </summary>
        /// <param name="stateTable"></param>
        private void SerializeStates(List<StateTable_Type> stateTable)
        {
            //XmlSerializer xmlSerializer;
            //StreamWriter streamWriter = null;
            bool ret = false;
            string path = string.Empty;
            string dir = $"{Entities.Const.appPath}States";
            //StateTable_Type st1;
            try
            {
                Log.Debug("/--->");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                path = $"{Const.appPath}Config\\{this.AlephATMAppData.FlowFileName}";
                //foreach (StateTable_Type st2 in obj)                                      
                //{
                //    st1 = st2;
                //    if (st2.StateNumber.Equals("832", StringComparison.Ordinal))
                //    {
                //        string q;
                //    }
                //    streamWriter = new StreamWriter(fileName);
                //    xmlSerializer = new XmlSerializer(typeof(StateTable_Type));
                //    xmlSerializer.Serialize(streamWriter, st2);
                //    streamWriter.Close();
                //}

                //Serializa el XML con firma
                //if (this.SerializeXMLSigned<List<StateTable_Type>>(fileName, stateTable))
                //    Log.Info("Serialize XML signed ok");
                //else
                //    Log.Error("Serialize XML signed error");

                ////Serializa el XML en plano
                //streamWriter = new StreamWriter(fileName);
                //xmlSerializer = new XmlSerializer(typeof(List<StateTable_Type>));
                //xmlSerializer.Serialize(streamWriter, stateTable);
                //streamWriter.Close();
                //if (File.Exists($"{fileName}.corrupted"))
                //    File.Delete($"{fileName}.corrupted");

                if (this.AlephATMAppData.SecureDownload)
                    GlobalAppData.Instance.CreateAlephObject<List<StateTable_Type>>(path, this.AlephATMAppData.KeyCertificate, stateTable);
                else
                    Utilities.Utils.ObjectToXml<List<StateTable_Type>>(out ret, stateTable, path);

                if (ret)
                    Log.Info("Serialize States ok");
                else
                    Log.Error("Serialize States error");
            }
            catch (Exception ex)
            {
                this.Download.StateTables.Clear();
                //streamWriter.Close();
                if (File.Exists(path))
                {
                    File.Copy(path, $"{path}.corrupted", true);
                    File.Delete(path);
                    Log.Error("Serialize States error");
                }
                Log.Fatal(ex);
            }
        }

        /// <summary>
        /// Obtiene los objetos de transición correspondientes a cada estado.
        /// </summary>
        /// <param name="stateTable"></param>
        /// <returns></returns>
        private StateTransition GetStateTransition(StateTable_Type stateTable)
        {
            StateTransition st = null; //Es el estado en si con todas sus propiedades y comportamientos.
            try
            {
                switch (stateTable.ItemElementName)
                {
                    case ItemChoiceStateTable_Type.CardReadStateTableData: //A
                        st = new CardReadState.CardReadState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.PINEntryStateTableData: //B
                        st = new PINEntryState.PINEntryState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.EnvelopeDispenserStateTableData: //C
                        st = new EnvelopeDispenserState.EnvelopeDispenserState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.PreSetOperationCodeBufferStateTableData: //D
                        st = new PreSetOperationCodeBufferState.PreSetOperationCodeBufferState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.FourFDKSelectionFunctionStateTableData: //E
                        st = new FourFDKSelectionFunctionState.FourFDKSelectionFunctionState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.AmountEntryStateTableData: //F
                        st = new AmountEntryState.AmountEntryState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.InformationEntryStateTableData: //H
                        st = new InformationEntryState.InformationEntryState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.TransactionRequestStateTableData://I
                        st = new TransactionRequestState.TransactionRequestState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.CloseStateTableData:// J
                        st = new CloseState.CloseState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.PrintStateTableData:// P
                        st = new PrintState.PrintState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.DefaultCloseStateTableData:// Estado interno
                        st = new DefaultCloseState.DefaultCloseState();
                        break;
                    case ItemChoiceStateTable_Type.FDKSwitchStateTableData: // W
                        st = new FDKSwitchState.FDKSwitchState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.EightFDKSelectionFunctionStateTableData: // Y
                        st = new EightFDKSelectionFunctionState.EightFDKSelectionFunctionState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.CashAcceptStateTableData: // >
                        st = new CashAcceptState.CashAcceptState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.BarcodeReadStateTableData: // &
                        st = new BarcodeReadState.BarcodeReadState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.AccountSelectorStateTableData: // d
                        st = new AccountSelectorState.AccountSelectorState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.FingerPrintCaptureStateTableData: // z006
                        st = new FingerPrintCaptureState.FingerPrintCaptureState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.LoginStateTableData: // z008
                        st = new LoginState.LoginState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.BagDropDepositStateTableData: // z009
                        st = new BagDropDepositState.BagDropDepositState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.SettlementOperationStateTableData: // z010
                        st = new Business.SettlementOperationState.SettlementOperationState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.SetExtraDataStateTableData:
                        st = new SetExtraDataState.SetExtraDataState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.MultiCashAcceptStateTableData: // z012 Multi Cash Accept State
                        st = new MultiCashAcceptState.MultiCashAcceptState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.ShipoutStateTableData: // z013 Shipout State
                        st = new ShipoutState.ShipoutState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.SupervisorStateTableData: // z014 Supervisor State
                        st = new SupervisorState.SupervisorState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.ConfigurationStateTableData: // z015 Configuration State
                        st = new ConfigurationState.ConfigurationState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.CashDispenseStateTableData: // z016 - CashDispenseState
                        st = new CashDispenseState.CashDispenseState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.ChoicesSelectorStateTableData: // z017 - ChoicesSelectorState
                        st = new ChoicesSelectorState.ChoicesSelectorState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.CheckDepositStateTableData: // z018 - CheckDepositState
                        st = new CheckDepositState.CheckDepositState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.VerifyNotesStateTableData: // z019 - VerifyNotesState
                        st = new VerifyNotesState.VerifyNotesState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.PinPadPaymentStateTableData: // z020 - PinPadPaymentState
                        st = new PinPadPaymentState.PinPadPaymentState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.TransactionMenuStateTableData: // z021 - TransactionMenuState
                        st = new TransactionMenuState.TransactionMenuState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.ShoppingCartStateTableData: // z022 - ShoppingCartState
                        st = new ShoppingCartState.ShoppingCartState(stateTable, this.AlephATMAppData);
                        break;
                    case ItemChoiceStateTable_Type.ChangeHandlerStateTableData: // z023 - ChangeHandlerState
                        st = new ChangeHandlerState.ChangeHandlerState(stateTable);
                        break;
                    case ItemChoiceStateTable_Type.CoinDispenserStateTableData: // z024 - CoinDispenserState
                        st = new CoinDispenserState.CoinDispenserState(stateTable);
                        break;
                    default:
                        //string aux = string.Format("Unknown state: {0}", stateTable.StateNumber);
                        //this.LogAppMessage(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, aux);
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return st;
        }

        public StateTransition GetStateProperties()
        {
            StateTransition stateTransition = null;
            //object prop = null;
            try
            {
                Log.Debug("/--->");
                if (this.Download.StateTables.Count != 0)
                {
                    foreach (StateTable_Type st in this.Download.StateTables)
                    {
                        stateTransition = this.GetStateTransition(st);
                        if (stateTransition != null)
                        {
                            if (stateTransition.ActivityName.Equals("LoginState"))
                            {
                                //prop = stateTransition.GetProperties<Business.CashAcceptState.PropertiesCashAcceptState>();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return stateTransition;
        }

        /// <summary>
        /// Carga el diccionario de transiciones de estados a partir de la tabla de estados recibida del download (son los estados en si).
        /// Se ejecuta al recibir un nuevo download o al iniciar la aplicación.
        /// </summary>
        private void UpdateStateTransition()
        {
            StateTransition stateTransition;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-");
            try
            {
                Log.Debug("/--->");
                if (this.Download.StateTables != null)
                {
                    if (this.Download.StateTables.Count != 0)
                    {
                        this.Download.DicOfStatesTransitions.Clear();//Borro completamente el diccionario de Transiciones de estados.
                        foreach (StateTable_Type st in this.Download.StateTables)
                        {
                            stateTransition = this.GetStateTransition(st);
                            if (stateTransition != null)
                            {
                                stateTransition.CurrentState = StateTransition.ProcessState.FINALIZED; //Seteo inicial por defecto en "Termindo"
                                if (this.Download.DicOfStatesTransitions.ContainsKey(st.StateNumber))
                                {
                                    this.Download.DicOfStatesTransitions.Remove(st.StateNumber); //Si ya existe el state, lo elimino para almacenar lo ultimo que se envió
                                    sb.AppendLine($"Delete state: {st.StateNumber} - {st.ItemElementName}");
                                }
                                this.Download.DicOfStatesTransitions.Add(st.StateNumber, stateTransition);
                                sb.AppendLine($"Added state: {st.StateNumber} [{st.ItemElementName}]");
                            }
                            else
                                Log.Error($"State {st.ItemElementName} - number {st.StateNumber} : is null");
                        }
                    }
                    else
                        Log.Warn("State table is empty");
                }
                else
                    Log.Warn("State table is null");
                //Agrego el estado interno "DefaultClose"
                StateTable_Type stateTable = new StateTable_Type();
                stateTable.StateNumber = "ZZZ";
                stateTable.ItemElementName = ItemChoiceStateTable_Type.DefaultCloseStateTableData;
                Business.DefaultCloseState.DefaultCloseState defaultCloseState = new DefaultCloseState.DefaultCloseState();
                stateTable.Item = defaultCloseState;
                stateTransition = this.GetStateTransition(stateTable);
                stateTransition.CurrentState = StateTransition.ProcessState.FINALIZED;
                if (stateTransition != null)
                {
                    //stateTransition.CurrentState = StateTransition.ProcessState.Terminated; //Seteo inicial por defecto en "Terminado"
                    this.Download.DicOfStatesTransitions.Add(stateTable.StateNumber, stateTransition);
                    sb.AppendLine($"Added state: {stateTable.StateNumber} [{stateTable.ItemElementName}]");
                }
                Log.Info(sb.ToString());
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "States"

        #region "Screens"
        internal void AddPlaceHolder(string placeHolder, string data)
        {
            Log.Info(string.Format("/--> Placeholder: {0}", placeHolder));
            this.HtmlGenerator.AddPlaceHolder(placeHolder, data);
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla GENERAL ndc y
        /// activa las FDKs correspondientes.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <returns></returns>
        internal bool ShowGeneralNDCScreen(string screenNumber, KeyMask_Type keyMask)
        {
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, string.Empty, Business.HtmlGenerator.ScreenType.General, string.Empty);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla GENERAL ndc y
        /// un texto para informar un error por pantalla de cliente.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="messageToShow"></param>
        /// <returns></returns>
        internal bool ShowMessageNDCScreen(string screenNumber, string messageToShow)
        {
            string htmlDocument = string.Empty;
            KeyMask_Type keyMask = this.GetKeyMaskData("000");
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, string.Empty, Business.HtmlGenerator.ScreenType.General, messageToShow);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla de AMOUNT ENTRY ndc y
        /// activa las FDKs correspondientes.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <param name="inputScreenTemplate"></param>
        /// <returns></returns>
        internal bool ShowAmountEntryNDCScreen(string screenNumber, KeyMask_Type keyMask, string inputScreenTemplate)
        {
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, inputScreenTemplate, Business.HtmlGenerator.ScreenType.AmountEntry, string.Empty);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla de INFORMATION ENTRY ndc y
        /// activa las FDKs correspondientes.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <param name="displayAndBufferParameters"></param>
        /// <returns></returns>
        internal bool ShowInformationEntryNDCScreen(string screenNumber, KeyMask_Type keyMask, EntryModeAndBufferConfiguration_Type entryModeAndBufferConfiguration)
        {
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, entryModeAndBufferConfiguration, Business.HtmlGenerator.ScreenType.InformationEntry, string.Empty);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla de PIN ENTRY ndc y
        /// activa las FDKs correspondientes.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <param name="track1Screen"></param>
        /// <returns></returns>
        internal bool ShowPinEntryNDCScreen(string screenNumber, KeyMask_Type keyMask, string track1Screen)
        {
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, track1Screen, Business.HtmlGenerator.ScreenType.PinEntry, string.Empty);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de una pantalla de Table Sector ndc y
        /// activa las FDKs correspondientes.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <returns></returns>
        internal bool ShowTableSelectorNDCScreen(string screenNumber, KeyMask_Type keyMask, object optionalParameter)
        {
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                ret = this.ShowNDCScreen(screenNumber, keyMask, optionalParameter, Business.HtmlGenerator.ScreenType.TableSelector, string.Empty);
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Genera el código HTML a partir de cualquier tipo de pantalla que se quiera generar (Para TODAS las pantallas.)
        /// activa las FDKs correspondientes.
        /// <param name="screenName"></param>
        /// <param name="keyMask"></param>
        /// <param name="optionalParameter"></param>
        /// <param name="screenType"></param>
        /// <param name="messageToShow"></param>
        /// <returns></returns>
        private bool ShowNDCScreen(string screenName, KeyMask_Type keyMask, object optionalParameter, Business.HtmlGenerator.ScreenType screenType, string messageToShow)
        {
            string htmlDocument = string.Empty, screenHtmPath, path;
            int decimalKeyMask = 0;
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                path = Parser.GetScreenFolder(screenName);
                screenHtmPath = string.Format(@"{0}\{1}.htm", path, screenName);
                string screensPath = string.Format("{0}Screens", Entities.Const.appPath);
                this.AddPlaceHolder("|_PATH_SCREENS_|", screensPath);//Cargo el PlaceHolder que se reemplazará por el path del archivo JavaScript
                this.AddPlaceHolder("|_RELATIVE_PATH_|", screensPath.Replace('\\', ','));
                if (File.Exists(screenHtmPath))
                {
                    htmlDocument = XDocument.Load(screenHtmPath).ToString();
                    htmlDocument = HtmlGenerator.ReplacePlaceHolder(htmlDocument);
                    ret = true;
                }
                else
                {
                    decimalKeyMask = this.GetKeyMaskDecimalData(keyMask);
                    ret = this.HtmlGenerator.GetHtmlDocument(screenName, keyMask, optionalParameter, screenType, messageToShow, out htmlDocument);
                }
                if (ret)
                {
                    Log.Info("Show Screen: {0} - FDK mask: {1}", screenName, decimalKeyMask);
                    StateEvent stateEvent = new StateEvent(StateEvent.EventType.ndcScreen, htmlDocument, "");
                    this.ShowGeneralNDCScreen(stateEvent.HandlerName, keyMask);
                    //this.RaiseEvtShowHtmlData(htmlDocument); //Si el armado del HTML es ok, envío a mostrar por pantalla.
                }
                else
                {
                    Log.Error(string.Format("Show Screen error: {0} - FDK mask: {1}", screenName, decimalKeyMask));
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Checkeamos que haya subcriptores al evento re lanzado que informa las teclas FDK presionadas por touch screen
        /// </summary>
        /// <param name="FDKdata">.</param>
        public virtual void RaiseEvtFDKscreenPress(string FDKdata)
        {
            DelegateSendFDKscreenPress tmp = EvtFDKscreenPress;
            if (tmp != null)
                tmp(FDKdata);
        }

        protected virtual void RaiseEvtOthersKeysPress(string OthersKeys)
        {
            DelegateSendEvtOthersKeysPress tmp = EvtOthersKeysPress;
            if (tmp != null)
                tmp(OthersKeys);
        }

        /// <summary>
        /// Checkeamos que haya subcriptores al evento re lanzado que informa los datos ingresados por el cliente a través de los input de pantalla.
        /// </summary>
        /// <param name="inputData">.</param>
        protected virtual void RaiseEvtInputData(string inputData, string dataLink)
        {
            DelegateSendInputData tmp = EvtInputData;
            if (tmp != null)
                tmp(inputData, dataLink);
        }

        /// <summary>
        /// Manejador que recibe el evento de tecla presionada desde la GUI. Luego re lanza un evento para informar la FDK presionada.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FDKeyPressEventHandler(object sender, System.EventArgs e)
        {
            try
            {
                HtmlElement element = sender as HtmlElement;
                if (element != null)
                {
                    Log.Info("FDK press: {0}", element.Id);
                    this.Bo.LastFDKPressed = element.Id;
                    this.RaiseEvtFDKscreenPress(element.Id);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Manejador que recibe el el tipo de tecla (ENTER, CANCEL) presionada desde la GUI. Luego re lanza un evento para informarlo.
        /// </summary>
        /// <param name="keyPress"></param>
        public void OthersKeyPressHandler(string keyPress)
        {
            Log.Info(string.Format("Key press: {0}", keyPress));
            this.RaiseEvtOthersKeysPress(keyPress);
        }

        /// <summary>
        /// Manejador que recibe el valor ingresado en los input o envío de datos en general. Luego lanza un evento para informarlo.
        /// </summary>
        /// <param name="inputData"></param>
        public void InputFieldDataHandler(string inputData, string dataLink)
        {
            //Log.Info(string.Format("InputData: {0}", inputData));
            this.RaiseEvtInputData(inputData, dataLink);
        }

        /// <summary>
        /// Lanza el evento para informar al WebBrowser que hay un nuevo Screen para mostrar.
        /// </summary>
        /// <param name="html">Mensaje de Log.</param>
        public virtual void RaiseEvtScreenData(StateEvent stateEvent)
        {
            DelegateSendHtmlData tmp = EvtShowHtmlData;
            if (tmp != null)
                tmp(stateEvent);
        }

        /// <summary>
        /// Quito los avisos de pantalla
        /// </summary>
        /// <param name="html">Mensaje de Log.</param>
        public void HideScreenModals()
        {
            //this.RaiseInvokeJavascript("HideMetroDialogs", "");
            StateEvent stateEvent = new StateEvent(StateEvent.EventType.runScript, "HideMetroDialogs", "");
            RaiseEvtScreenData(stateEvent);
        }

        /// <summary>
        /// Ingresa o sobrescribe una pantalla en el diccionario general de pantallas.
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="screenData"></param>
        internal void AddScreenData(string screenName, ScreenData_Type screenData)
        {
            if (this.Download.ScreenData == null)
                this.Download.ScreenData = new Dictionary<string, ScreenData_Type>();
            if (this.Download.ScreenData.ContainsKey(screenName))
            {
                this.Download.ScreenData.Remove(screenName); //Sobre escribo con las pantallas nuevas
            }
            this.Download.ScreenData.Add(screenName, screenData);
        }

        /// <summary>
        /// Ingresa o sobrescribe una pantalla SimulatedPrePrintedReceiptScreen
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="screenData"></param>
        internal void AddSimulatedPrePrintedReceiptScreen(SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreen)
        {
            if (simulatedPrePrintedReceiptScreen.ScreenNumber.Equals("R00"))
                this.Download.simulatedPrePrintedReceiptScreenR00 = simulatedPrePrintedReceiptScreen;
            else if (simulatedPrePrintedReceiptScreen.ScreenNumber.Equals("R01"))
                this.Download.simulatedPrePrintedReceiptScreenR01 = simulatedPrePrintedReceiptScreen;
        }

        /// <summary>
        /// Obtiene la máscara de activación de FDKs a partir de un valor decimal.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal KeyMask_Type GetKeyMaskData(string text)
        {
            Log.Debug("/--->");
            KeyMask_Type keyMask = new KeyMask_Type();
            keyMask.FDKA = false;
            keyMask.FDKB = false;
            keyMask.FDKC = false;
            keyMask.FDKD = false;
            keyMask.FDKF = false;
            keyMask.FDKG = false;
            keyMask.FDKH = false;
            keyMask.FDKI = false;
            byte[] arrByte;
            int readCond;
            if (int.TryParse(text, out readCond))
            {
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        keyMask.FDKA = true;
                    if (arrByte[6] == 1)
                        keyMask.FDKB = true;
                    if (arrByte[5] == 1)
                        keyMask.FDKC = true;
                    if (arrByte[4] == 1)
                        keyMask.FDKD = true;
                    if (arrByte[3] == 1)
                        keyMask.FDKF = true;
                    if (arrByte[2] == 1)
                        keyMask.FDKG = true;
                    if (arrByte[1] == 1)
                        keyMask.FDKH = true;
                    if (arrByte[0] == 1)
                        keyMask.FDKI = true;
                }
            }
            return keyMask;
        }

        /// <summary>
        /// Obtiene la máscara de activación de FDKs a partir de un valor decimal.
        /// </summary>
        /// <param name="keyMask"></param>
        /// <returns></returns>
        internal int GetKeyMaskDecimalData(KeyMask_Type keyMask)
        {
            int[] arrByte = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                Log.Debug("/--->");
                if (keyMask != null)
                {
                    if (keyMask.FDKA == true)
                        arrByte[7] = 1;
                    if (keyMask.FDKB == true)
                        arrByte[6] = 2;
                    if (keyMask.FDKC == true)
                        arrByte[5] = 4;
                    if (keyMask.FDKD == true)
                        arrByte[4] = 8;
                    if (keyMask.FDKF == true)
                        arrByte[3] = 16;
                    if (keyMask.FDKG == true)
                        arrByte[2] = 32;
                    if (keyMask.FDKH == true)
                        arrByte[1] = 64;
                    if (keyMask.FDKI == true)
                        arrByte[0] = 128;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return arrByte[0] + arrByte[1] + arrByte[2] + arrByte[3] + arrByte[4] + arrByte[5] + arrByte[6] + arrByte[7];
        }

        /// <summary>
        /// Guarda la tabla de pantallas en un archivo XML luego de completarse el Download.
        /// </summary>
        /// <param name="screenData"></param>
        internal void SerializeScreens(Dictionary<string, ScreenData_Type> screenData)
        {
            XmlSerializer xmlSerializer;
            StreamWriter streamWriter;
            string fileName = string.Empty;
            string path;
            string name;
            //int i = 0;
            ScreenData_Type scr;
            try
            {
                Log.Debug("/--->");
                foreach (KeyValuePair<string, ScreenData_Type> pair in screenData)
                {
                    name = string.Format("{0}.ndc", pair.Key);
                    path = Parser.GetScreenFolder(pair.Key);
                    if (!Directory.Exists(string.Format(@"{0}", path)))
                        Directory.CreateDirectory(string.Format(@"{0}", path));
                    scr = pair.Value;
                    fileName = string.Format(@"{0}\{1}", path, name);
                    streamWriter = new StreamWriter(fileName);
                    xmlSerializer = new XmlSerializer(typeof(ScreenData_Type));
                    xmlSerializer.Serialize(streamWriter, pair.Value);
                    streamWriter.Close();
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Guarda la tabla de pantallas en un archivo XML luego de completarse el Download.
        /// </summary>
        /// <param name="screenData"></param>
        internal void SerializeSimulatedPrePrintedReceiptScreen(SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreen)
        {
            XmlSerializer xmlSerializer;
            StreamWriter streamWriter;
            string fileName = string.Empty;
            string path;
            string name;
            //int i = 0;
            //ScreenData_Type scr;
            try
            {
                Log.Debug("/--->");

                path = string.Format(@"{0}Screens\Screens\{1}", Entities.Const.appPath, simulatedPrePrintedReceiptScreen.ScreenNumber.Substring(0, 1));
                name = string.Format("{0}.ndc", simulatedPrePrintedReceiptScreen.ScreenNumber);
                if (!Directory.Exists(string.Format(@"{0}", path)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}", path));
                }
                fileName = string.Format(@"{0}\{1}", path, name);
                streamWriter = new StreamWriter(fileName);
                xmlSerializer = new XmlSerializer(typeof(SimulatedPrePrintedReceiptScreen_Type));
                xmlSerializer.Serialize(streamWriter, simulatedPrePrintedReceiptScreen);
                streamWriter.Close();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "Screens"

        #region "Enhanced parameters"
        internal int GetEnhParameterOption(ItemChoiceOption_Type op, out Option_Type optionRet)
        {
            optionRet = new Option_Type();
            int ret = -1;
            optionRet.Item = string.Empty;
            Option_Type[] OptionList = this.Download.EnhancedConfigurationParametersData.OptionList;
            try
            {
                Log.Debug("/--->");
                if (OptionList != null)
                {
                    var option = OptionList.FirstOrDefault(p => p.ItemElementName.Equals(op));
                    if (option == null)
                    {
                        Log.Warn($"Option: {op} not found.");
                    }
                    else
                    {
                        optionRet = option as Option_Type;
                        ret = 0;
                    }
                }
                else
                    Log.Error("OptionList is null.");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        internal int GetEnhParameterTimer(ItemChoiceTimer_Type timer, out Timer_Type timerRet)
        {
            timerRet = new Timer_Type();
            int ret = -1;
            timerRet.Item = 0;
            Timer_Type[] timerList = this.Download.EnhancedConfigurationParametersData.TimerList;
            try
            {
                Log.Debug("/--->");
                if (timerList != null)
                {
                    var option = timerList.FirstOrDefault(p => p.ItemElementName.Equals(timer));
                    if (option == null)
                    {
                        Log.Warn($"Timer: {timer} not found.");
                    }
                    else
                    {
                        timerRet = option as Timer_Type;
                        ret = 0;
                    }
                }
                else
                    Log.Error("TimerList is null.");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }
        /// <summary>
        /// Guarda la tabla de estados en un archivo XML luego de completarse el Download.
        /// </summary>
        /// <param name="stateTable"></param>
        private void SerializeEnhParameters(EnhancedConfigurationParametersData_Type enhancedConfigurationParametersData)
        {
            XmlSerializer xmlSerializer;
            StreamWriter streamWriter;
            string fileName = $"{Const.appPath}Config\\{this.AlephATMAppData.EnhParamFileName}";
            try
            {
                Log.Debug("/--->");
                if (!Directory.Exists($"{Const.appPath}Config"))
                    Directory.CreateDirectory($"{Const.appPath}Config");
                streamWriter = new StreamWriter(fileName);
                xmlSerializer = new XmlSerializer(typeof(EnhancedConfigurationParametersData_Type));
                xmlSerializer.Serialize(streamWriter, enhancedConfigurationParametersData);
                streamWriter.Close();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion

        #region "Electronic Journal"
        public void WriteEJ(string dataToWrite, bool writeLog = false)
        {
            GlobalAppData.Instance.WriteEJ(dataToWrite);
            if (writeLog)
                Log.Info(dataToWrite);
        }

        internal void PrintEJCounters(Detail.ContainerIDType containerIDType)
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            List<Item> lstItemsCurr;
            string text;
            try
            {
                this.WriteEJ($"{containerIDType} counters: ");
                List<Detail> details = this.Counters.GetDetails(containerIDType);
                IEnumerable<string> curr = from i in details
                                           select i.Currency;
                currencies = curr.ToList();
                distinctCurrencies = currencies.Distinct().ToArray();
                for (int k = 0; k < distinctCurrencies.Length; k++)
                {
                    lstItemsCurr = new List<Item>();
                    Detail det = details.Find(e => e.Currency.Equals(distinctCurrencies[k]));
                    det.LstItems.ForEach(delegate (Item b)
                    {
                        if (b.Num_Items != 0)
                        {
                            text = b.Num_Items == 1 ? " note  of " : " notes of ";
                            this.WriteEJ($"{b.Num_Items.ToString().PadLeft(3, ' ')}{text}{distinctCurrencies[k]} {(b.Denomination / 100).ToString().PadLeft(5, ' ')}");
                        }
                    });
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
        #endregion "Electronic Journal"

        #region "Initialization"
        /// <summary>
        /// Inicializa el core
        /// </summary>
        private bool Startup()
        {
            bool ret = false;
            //External_Interface.HostConnector.ListOfIHost hostConnector;
            try
            {
                Log.Debug("/--->");
                this.AppVersion = this.GetFileVersion("AlephATM.exe");//Imprimo las versiones de los ensamblados
                if (this.GetConfigurationData())
                {
#if DEBUG
                    this.SetBrandingFiles(this.AlephATMAppData.Branding); // Cambiar archivos de branding en modo debug
#endif
                    Thread thd2;
                    thd2 = new Thread(new ThreadStart(this.PrintAssembliesVersions));
                    thd2.Start();
                    if (this.AlephATMAppData.HostEnabled)
                    {
                        //hostConnector = External_Interface.HostConnector.GetHostsInstance(out ret, this.AlephATMAppData.Branding);//Get the available host objects
                        //if (ret)
                        //{
                        //    foreach (Ihost host in hostConnector)
                        //    {
                        //        AlephHost alephHost = new AlephHost(host);
                        //        alephHost.Host.EvtGetHostData += this.ProcessHostRequest;
                        //        this.MapHost(host.GetType().Name, alephHost);
                        //    }
                        //    Log.Info("Start AlephHost OK");
                        //    ret = false;
                        //}
                        //else
                        //    Log.Error("Initialization Host objects error");
                    }
                    else
                        Log.Warn("AlephHost disable");
                    if (this.InitializeDownload()) //Obtine el flujo de estados.
                    {
                        if (this.AlephATMAppData.TerminalModel == Enums.TerminalModel.MiniBank_JH6000_D || this.AlephATMAppData.TerminalModel == Enums.TerminalModel.MiniBank_JH600_A)
                            if (!this.WriteIniConfigFileAsync(true, false).GetAwaiter().GetResult())
                                Log.Error("Write config file error");
                        this.Sdo = new SDO(this);
                        if (this.InitializeApp())
                        {
                            if (this.AlephATMAppData.RetrievalTransactionEnable)
                                Task.Run(() => ExecuteAsyncRetrievalTransaction()).Wait();
                            else
                                this.HandleOperationMode();
                            ret = true;
                        }
                    }
                    else
                        Log.Error("Initialization CORE ERROR.");
                }
                else
                    Log.Error("Can´t get configuration app Data");
            }
            catch (Exception value) { Log.Fatal(value); }
            return ret;
        }

        private void HandleOperationMode()
        {
            try
            {
                if (this.AlephATMAppData.AlephDEVEnable && this.Sdo.DevConf.CIMconfig.Enable)
                {
                    this.Sdo.EvtCompletionReceive += Sdo_EvtCompletionReceive;
                    Thread.Sleep(100);
                    this.Sdo.TER_InitVerify();
                }
                else
                {
                    this.StartupRoutine();
                }
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        private void Sdo_EvtCompletionReceive(DeviceMessage dm)
        {
            string countersPath = $"{Const.appPath}Counters\\Counters.xml";
            try
            {
                if (dm.Device == Enums.Devices.Terminal)//Solo recibe eventos de la terminal en general
                {
                    if (this.Sdo.DevConf.IOBoardConfig.Enable)//Consulto estado de la IOBoard
                        this.Sdo.IOBoard_GetState();
                    SDO_DeviceState DEV_State = this.Sdo.SOH.SDO_DevicesState.FirstOrDefault(x => x.Device == Enums.Devices.CashAcceptor);
                    Completion cr = (Completion)dm.Payload;
                    if (cr.CompletionCode == CompletionCodeEnum.Success)
                    {
                        if (!File.Exists(countersPath))//Si el archivo de contadores no existe, hay que generarlo con los nuevos datos del template de CIM
                        {
                            this.GetInitialCounters();
                        }
                        Log.Info("Terminal initialization success");
                        this.WriteEJ("<TERMINAL INIT SUCCESS>");
                        this.StartupRoutine();
                        //Envío evento CIM ok
                        if (DEV_State != null)
                        {
                            DEV_State.InternalCode = Enums.DeviceStatus.CIM_DeviceSuccess;
                            DEV_State.Fitness = Const.Fitness.NoError;
                        }
                        else
                            Log.Fatal($"Device CIM fitness is null");
                    }
                    else //CIM error
                    {
                        Log.Error("Terminal initialization failure");
                        this.WriteEJ("<TERMINAL INIT FAILURE>");
                        if (this.Sdo.DevConf.CIMconfig.Required)
                            this.RequestChangeMode(Const.TerminalMode.OutOfService);
                        else
                            this.StartupRoutine();
                        //Envío evento CIM error
                        if (DEV_State != null)
                        {
                            DEV_State.InternalCode = Enums.DeviceStatus.CIM_DeviceError;
                            DEV_State.Fitness = Const.Fitness.Fatal;
                        }
                        else
                            Log.Fatal($"Device CIM fitness is null");
                    }
                    this.Sdo.EvtCompletionReceive -= Sdo_EvtCompletionReceive;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void StartupRoutine()
        {
            if (this.AlephATMAppData.OperationMode == OperationMode.Batch)
            {
                this.RequestChangeMode(Const.TerminalMode.InService);
            }
            else if (this.AlephATMAppData.OperationMode == Const.OperationMode.NDC)
            {
                this.NDChost = new NDChost(this); //Configura sockets NDC
                this.RequestChangeMode(Const.TerminalMode.OffLine);
            }
            else if (this.AlephATMAppData.OperationMode == Const.OperationMode.Mix)
            {
                this.NDChost = new NDChost(this); //Configura sockets NDC
                this.RequestChangeMode(Const.TerminalMode.OnLine);
            }
        }

        private bool GetConfigurationData()
        {
            bool ret = false;
            bool retGetAppData = false;
            string staesSetsDir = $"{Entities.Const.appPath}StatesSets";
            List<Printers.PrinterTemplate> listOfPrinterTemplate = new List<Printers.PrinterTemplate>();
            try
            {
                Log.Debug("/--->");
                if (!Directory.Exists(staesSetsDir))
                    Directory.CreateDirectory(staesSetsDir);
                Printers.PrinterTemplate.GetPrinterTemplate(out listOfPrinterTemplate, "Template.xml");
                if (TerminalInfo.GetTerminalInfo(out this.TerminalInfo))
                {
                    this.AlephATMAppData = AlephATMAppData.GetAppData(out retGetAppData);
                    this.PrintObjectProperties(this.AlephATMAppData);
                    this.PrintObjectProperties(this.TerminalInfo);
                    if (retGetAppData)
                    {
                        if (ScreenConfiguration.GetScreenConfiguration(out this.ScreenConfiguration, this.AlephATMAppData))
                        {
                            this.PrintObjectProperties(this.ScreenConfiguration);
                            Thread.CurrentThread.CurrentCulture = new CultureInfo(this.AlephATMAppData.Region);//Load regional configuration
                            GlobalAppData.Instance.SetScratchpad("selectedLanguage", this.AlephATMAppData.DefaultLanguage);//Load language
                            this.StartOfDay();//Logea inicio del día
                            this.MoreTimeConfig = MoreTimeConfigurationType.LoadConfig<MoreTimeConfigurationType>(out ret);
                            if (ret)
                                this.PrintObjectProperties(this.MoreTimeConfig);
                            else
                                Log.Error("MoreTimeConfiguration XML file error.");
                        }
                        else
                            Log.Error("ScreenConfiguration XML file error.");
                    }
                    else
                        Log.Error("AlephATMAppData XML file error.");
                }
                else
                    Log.Error("TerminalInfo XML file error.");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        internal void PrintObjectProperties(object obj)
        {
            try
            {
                if (this.AlephATMAppData.PrintConfigProperties)
                    Log.Info(ObjectDumper.Dump(obj));
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void PrintAssembliesVersions()
        {
            try
            {
                Log.Info($"Assembly version of \"AlephATM.exe\": {this.AppVersion}");
                Log.Info($"Assembly version of \"Business.dll\": {this.GetFileVersion("Business.dll")}");
                Log.Info($"Assembly version of \"External_Interface.dll\": {this.GetFileVersion("External_Interface.dll")}");
                Log.Info($"Assembly version of \"Entities.dll\": {this.GetFileVersion("Entities.dll")}");
                Log.Info($"Assembly version of \"ExternalDevices.dll\": {this.GetFileVersion("ExternalDevices.dll")}");
                Log.Info($"Assembly version of \"DeviceControl.dll\": {this.GetFileVersion("DeviceControl.dll")}");
                Log.Info($"Assembly version of \"XFSHandler.dll\": {this.GetFileVersion("XFSHandler.dll")}");
                Log.Info($"Assembly version of \"Utilities.dll\": {this.GetFileVersion("Utilities.dll")}");
                Log.Info($"Assembly version of \"AdminMenuCore.dll\": {this.GetFileVersion("AdminMenuCore.dll")}");
                //PerformanceCounter ramCounter = new PerformanceCounter("Paging File", "% Usage", "_Total", machineName);
                var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                Log.Info($"Physical RAM (MB): {computerInfo.TotalPhysicalMemory / 1024 / 1024}");
                Log.Info($"Available Physical Memory (MB): {computerInfo.AvailablePhysicalMemory / 1024 / 1024}");
                Log.Info($"UI Region: {CultureInfo.CurrentCulture}");
                Log.Info($"OS full name: {computerInfo.OSFullName}");
                Log.Info($"Machine name: {Environment.MachineName}");
                Log.Info($"User name: {Environment.UserName}");
                Log.Info($"User domain name: {Environment.UserDomainName}");
                Log.Info($"64 bits environment: {Environment.Is64BitOperatingSystem}");
                Log.Info($"64 bits process: {Environment.Is64BitProcess}");
                Log.Info($"Working set (MB): {Environment.WorkingSet / 1024 / 1024}");
                this.PrintIPAddresses();
                this.PrintHardDiskInfo();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void PrintHardDiskInfo()
        {
            DriveInfo[] arrayOfDrives = DriveInfo.GetDrives();
            foreach (var d in arrayOfDrives)
            {
                Log.Info($"Drive {d.Name}");
                Log.Info($"--Drive type: {d.DriveType}");
                if (d.IsReady == true)
                {
                    Log.Info($"--Volume label: {d.VolumeLabel}");
                    Log.Info($"--File system: {d.DriveFormat}");
                    Log.Info($"--AvailableFreeSpace for current user (MB): {d.AvailableFreeSpace / 1024 / 1024}");
                    Log.Info($"--TotalFreeSpace (MB): {d.TotalFreeSpace / 1024 / 1024}");
                    Log.Info($"--Total size of drive (MB): {d.TotalSize / 1024 / 1024}");
                    Log.Info($"--Used space: {((d.TotalSize - d.TotalFreeSpace) * 100) / d.TotalSize,2} %");
                }
            }
        }

        private string GetFileVersion(string path)
        {
            FileVersionInfo fileVersionInfo = null;
            string version = "";
            try
            {
                string filePath = $"{Const.appPath}{path}";
                if (File.Exists(filePath))
                {
                    fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    version = fileVersionInfo.FileVersion;
                }
                else
                    Log.Error($"File \"{filePath}\" not found");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return version;
        }

        /// <summary> 
        /// This utility function displays all the IP (v4, not v6) addresses of the local computer. 
        /// </summary> 
        public void PrintIPAddresses()
        {
            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = network.GetIPProperties();
                // Each network interface may have multiple IP addresses 
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now 
                    if (address.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        continue;
                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (System.Net.IPAddress.IsLoopback(address.Address))
                        continue;
                    Log.Info($"IP: {address.Address.ToString()} NAME: {network.Name}");
                }
            }
            Log.Info($"Network Available: {NetworkInterface.GetIsNetworkAvailable()}");
        }

        private bool InitializeApp()
        {
            string countersPath = $"{Const.appPath}Counters\\Counters.xml";
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                if (!Directory.Exists($"{Const.appPath}\\StatesSets"))
                    Directory.CreateDirectory($"{Const.appPath}\\StatesSets");
                this.QueueAttemptChangeMode = new Queue<Const.TerminalMode>();
                this.TimerAttemptChangeMode = new System.Timers.Timer(30);
                this.TimerAttemptChangeMode.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerAttemptChangeMode_Elapsed);
                this.TimerAttemptChangeMode.Enabled = true;
                this.StateExecute = new TransitionHandler(this);
                //ret = true;
                //Initialize counters
                if (!Directory.Exists($"{Const.appPath}Counters"))
                    Directory.CreateDirectory($"{Const.appPath}Counters");
                //if (File.Exists(countersPath))//Si el archivo de contadores no existe, hay que generarlo despues de consultar el template del CIM
                //{
                ret = this.GetInitialCounters();
                //}
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Obtiene los contadores iniciales desde un archivo o genera los contadores por defecto si no existiera el archivo
        /// </summary>
        /// <returns></returns>
        private bool GetInitialCounters()
        {
            bool ret = false;
            this.Counters = new Counters();
            int logicalFullBinThreshold = this.Sdo.DevConf.BAGconfig != null ? this.Sdo.DevConf.BAGconfig.BagCapacity : 0;
            if (this.Counters.GetInitialCounters(out this.Counters, logicalFullBinThreshold))
            {
                //Log.Info(ObjectDumper.Dump(this.Counters));
                ret = true;
            }
            else
                Log.Error("Can´t get counters.");
            return ret;
        }

        /// <summary>
        /// Obtiene el download desde los archivos XML 
        /// </summary>
        private bool InitializeDownload()
        {
            bool ret = false;
            List<ScreenColorMapping> listOfScreenColorMapping;
            List<ScreenColumnMapping> listOfScreenColumnMapping;
            List<ScreenRowMapping> listOfScreenRowMapping;
            string messageToShow = "";
            Option_Type optionRet;
            try
            {
                Log.Debug("/--->");
                this.Parser = new Business.Parser();
                this.Download = new Download();
                string path = $"{Const.appPath}Config\\{this.AlephATMAppData.FlowFileName}";
                Log.Info($"Flow file name: {this.AlephATMAppData.FlowFileName}");
                if (File.Exists(path))
                {
                    //A)- STATES: Obtengo los estados desde el archivo XML
                    this.Download.StateTables = Download.GetStateTable(path, out ret, this.AlephATMAppData.KeyCertificate, this.AlephATMAppData.SecureDownload);
                    this.PrintObjectProperties(this.Download.StateTables);
                    if (ret)
                    {
                        messageToShow = $"Added {this.Download.StateTables.Count} states to State Table";
                        if (this.Download.StateTables.Count == 0)
                            Log.Error(messageToShow);
                        else
                            Log.Info(messageToShow);
                    }
                    else
                        Log.Error("Deserialize XML error");
                }
                else
                    Log.Error($"File \"{path}\"not found");
                //B)- ENHANCED PARAMETERS: Obtengo los enhanced parameters desde el archivo XML
                this.Download.EnhancedConfigurationParametersData = Download.GetEnhParameters(this.AlephATMAppData.KeyCertificate, this.AlephATMAppData.EnhParamFileName, this.AlephATMAppData.SecureDownload);
                Log.Info("Append enhParam to enhParam Table.");
                if (this.GetEnhParameterOption(ItemChoiceOption_Type.AANDCNextStateNumber, out optionRet) == 0)
                    Log.Info($"CardLess next state: {optionRet.Item.ToString()}");
                else
                    Log.Warn($"->Can´t get enh parameter option: {ItemChoiceOption_Type.AANDCNextStateNumber}");
                //Log.Info(ObjectDumper.Dump(this.Download.EnhancedConfigurationParametersData));
                //C)- SCREENS: Obtengo las pantalla desde el archivo XML
                this.Download.ScreenData = Download.GetScreenData();
                Log.Info($"Append {this.Download.ScreenData.Count} screens to Screen Table.");
                //D- SIMULATED PRE PRINTED RECEIPT SCREEN: 
                this.Download.simulatedPrePrintedReceiptScreenR00 = Download.GetSimulatedPrePrintedReceiptScreenData("R00");
                this.Download.simulatedPrePrintedReceiptScreenR01 = Download.GetSimulatedPrePrintedReceiptScreenData("R01");
                //E)- Instancio el generador de documentos HTML
                if (ScreenColorMapping.GetScreenColorMapping(out listOfScreenColorMapping))
                {
                    if (ScreenColumnMapping.GetScreenColumnMapping(out listOfScreenColumnMapping, this.ScreenConfiguration.MainBrowserResolution))
                    {
                        if (ScreenRowMapping.GetScreenRowMapping(out listOfScreenRowMapping, this.ScreenConfiguration.MainBrowserResolution))
                        {
                            this.HtmlGenerator = new HtmlGenerator(this.Download.ScreenData, listOfScreenRowMapping, listOfScreenColumnMapping,
                            listOfScreenColorMapping, $"{Const.appPath}Screens", this.ScreenConfiguration, this.ScreenConfiguration.MainBrowserResolution);
                            //E)- Actualizo el diccionario de Transiciones de estados
                            this.UpdateStateTransition();
                            ret = true;
                        }
                        else
                            Log.Error("ScreenRowMapping XML file error.");
                    }
                    else
                        Log.Error("ScreenColumnMapping XML file error.");
                }
                else
                    Log.Error("ScreenColorMapping XML file error.");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        internal void InitVisit()
        {
            try
            {
                Log.Debug("/--->");
                this.Bo.ExtraInfo = new ApplVisitExtraInfo();
                this.Sdo.SOH.InUseState = Const.InUseMode.InUse;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void InitBo()
        {
            try
            {
                Log.Debug("/--->");
                this.Bo = new Entities.BusinessObject();
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void EndVisit()
        {
            Log.Debug("/--->");
            if (this.Bo != null)
                this.Bo.ExtraInfo = null;
            this.Sdo.SOH.InUseState = Const.InUseMode.NotInUse;
            GlobalAppData.Instance.ClearScratchpad();
        }

        #endregion "Initialization"       

        #region "Retrieval transaction"
        public async Task ExecuteAsyncRetrievalTransaction()
        {
            Task<bool> longRunningTask = ProcessRetrievalTransaction();
            bool result = await longRunningTask;
            Log.Info("Result Retrieval Transaction: {0}", result);
        }

        public async Task<bool> ProcessRetrievalTransaction()
        {
            bool ret = false;
            CashDepositTransaction cashDepositTransaction = null;
            string filePath = $"{Const.appPath}Retrieval\\RetrievalTransaction.xml";
            string certPath = $"{Const.appPath}AlephCERT.pfx";
            try
            {
                Log.Debug("/--->");
                if (File.Exists(certPath))
                {
                    if (File.Exists(filePath))
                    {
                        Log.Warn("Retrieval transaction detected");
                        cashDepositTransaction = this.DeserializeSigned<CashDepositTransaction>(out ret, filePath);
                        if (ret)
                        {
                            if (cashDepositTransaction.tsn == this.Counters.GetTSN())
                            {
                                this.RequestChangeMode(Const.TerminalMode.Suspend);
                                this.PersistRetrievalDeposit(cashDepositTransaction);
                            }
                            else
                            {
                                Log.Error("Retrieval transaction detected rejected for TSN");
                                this.HandleOperationMode();
                                ret = false;
                            }
                        }
                        else
                        {
                            Log.Error("Deserialize error");
                            this.HandleOperationMode();
                        }
                        File.Delete(filePath);
                    }
                    else
                        this.HandleOperationMode();
                }
                else
                    Log.Error($"Certificate path not found: {certPath}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        private void PersistRetrievalDeposit(CashDepositTransaction cashDepositTransaction)
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            List<Detail> lstDetail = new List<Detail>();
            List<Item> lstItems = new List<Item>();
            List<Bills> lstBills = new List<Bills>();
            Contents contents;
            StringBuilder sb = new StringBuilder();
            decimal updValue = 0;
            CashInInfo cashInInfo = new CashInInfo();
            try
            {
                string msg1 = $"Retrieval transaction #: {this.Counters.GetTSN()}";
                string msg2 = $"Transaction date: {cashDepositTransaction.txDateTime}";
                Log.Info(msg1);
                this.WriteEJ(msg1);
                Log.Info(msg2);
                this.WriteEJ(msg2);
                foreach (CashInInfo ci in cashDepositTransaction.ListCashInInfo)
                {
                    foreach (Bills b in ci.Bills)
                    {
                        currencies.Add(b.Currency);//Cargo todos los currencies
                        Bills bills = lstBills.Find(x => x.Value == b.Value && x.Currency.Equals(b.Currency));//Solo trae los Items Acceptor del mismo currency
                        if (bills != null)//TRUE: Ya exixte un Detail de dinero validado con el mismo currency entrante
                        {
                            bills.Quantity += b.Quantity;
                        }
                        else
                        {
                            lstBills.Add(b);
                        }
                    }
                }
                distinctCurrencies = currencies.Distinct().ToArray();
                for (int k = 0; k < distinctCurrencies.Length; k++)
                {
                    lstItems = new List<Item>();
                    sb = new StringBuilder();
                    sb.AppendLine($"Retrieval deposit currency: {distinctCurrencies[k]}");
                    foreach (Bills b in lstBills)
                    {
                        if (b.Currency.Equals(distinctCurrencies[k]))
                        {
                            updValue = (decimal)(b.Value * 100);
                            sb.Append($"{Environment.NewLine}--> ID: {b.Id.ToString().PadLeft(3, ' ')} - CUR: {b.Currency} - QTY: {b.Quantity.ToString().PadLeft(3, ' ')} - VAL: {Utilities.Utils.FormatCurrency(b.Value, b.Currency, 4)} - NDC: {b.NDCNoteID}");
                            lstItems.Add(new Item(updValue, (decimal)b.Quantity, (decimal)(updValue * b.Quantity), "NOTE", "", ""));
                        }
                    }
                    Log.Info(sb.ToString());
                    this.WriteEJ(sb.ToString());
                    Detail detail = new Detail(distinctCurrencies[k], Detail.ContainerIDType.CashAcceptor, "NOTEACCEPTOR", cashDepositTransaction.CollectionId, lstItems);
                    lstDetail.Add(detail);
                }
                //Cargo los datos de transacción
                this.Bo = new BusinessObject();
                this.Bo.ExtraInfo = new ApplVisitExtraInfo();
                this.Bo.ExtraInfo.ExtraData = cashDepositTransaction.ExtraData;
                this.AddHostExtraData("extraData", cashDepositTransaction.ExtraData);
                this.Bo.ExtraInfo.CollectionID = cashDepositTransaction.CollectionId;
                cashInInfo.Bills = lstBills;
                this.Bo.ExtraInfo.CashInInfo = cashInInfo;
                this.Bo.ExtraInfo.CashInMultiCashData.UpdateMultiCashData(this.Bo.ExtraInfo.CashInInfo);
                this.Bo.ExtraInfo.UserProfileMain = new UserProfile_Type(cashDepositTransaction.User, cashDepositTransaction.UserName);
                this.AddHostExtraData("userProfile", this.Bo.ExtraInfo.UserProfileMain);
                this.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError = cashDepositTransaction.depositHardwareError;
                //Envío a host
                contents = new Contents(lstDetail);
                ////B)- Send transaction message to host
                Thread prtWndThd;
                prtWndThd = new Thread(new ParameterizedThreadStart(this.AuthorizeTransaction));
                prtWndThd.Start(contents);
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
                authorizationResult = this.AuthorizeTransaction(Enums.TransactionType.DEPOSIT, contents, "");
                if (authorizationResult.authorizationStatus == AuthorizationStatus.Authorized)
                    this.Bo.ExtraInfo.ReceiptRequired = true;
                else
                    this.Bo.ExtraInfo.ReceiptRequired = false;
                this.StateExecute.StartActivity(this.AlephATMAppData.PrintState);
                Thread.Sleep(200);
                //Aumento el número de transacción
                this.Counters.UpdateTSN();
                this.Bo.ExtraInfo = new ApplVisitExtraInfo();
            }
            catch (Exception ex) { Log.Fatal(ex); }
            finally { this.HandleOperationMode(); }
        }

        public T DeserializeSigned<T>(out bool ret, string filePath)
        {
            ret = false;
            T output = default(T);
            string certPath = $"{Const.appPath}AlephCERT.pfx";
            try
            {
                var cert = Utilities.Encryption.GetX509CertFromFile(certPath, Utils.HexToStr(this.AlephATMAppData.KeyCertificate));
                if (cert.PublicKey == null)
                    throw new Exception("Invalid public key");
                if (Utilities.SignAndVerify.XmlFileIsValid(filePath, cert))
                {
                    output = Utilities.Utils.GetGenericXmlData<T>(out ret, filePath, output);
                    if (!ret)
                        Log.Error("Deserialize error");
                }
                else
                    Log.Error("XMLfile is corrupt or has been tampered with - signature could not be verified");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return output;
        }

        public bool SerializeXMLSigned<T>(string filePath, object obj)
        {
            bool ret = true;
            var unsignedXML = new XmlDocument();
            var signedXML = new XmlDocument();
            XmlSerializer xmlSerializer;
            XmlDocument xmlDocument = new XmlDocument();
            string certPath = $"{Const.appPath}AlephCERT.pfx";
            try
            {
                Type outType = typeof(T);
                X509Certificate2 currentCert = new X509Certificate2(certPath, Utils.HexToStr(this.AlephATMAppData.KeyCertificate), X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                if (currentCert.PrivateKey == null)
                    throw new Exception("Invalid private key");
                xmlSerializer = new XmlSerializer(outType);
                using (MemoryStream ms = new MemoryStream())
                {
                    xmlSerializer.Serialize(ms, obj);
                    ms.Position = 0;
                    unsignedXML.Load(ms);
                    ms.Close();
                }
                var cspParams = new System.Security.Cryptography.CspParameters(24) { KeyContainerName = "XML_DSIG_RSA_KEY" };
                var key = new System.Security.Cryptography.RSACryptoServiceProvider(cspParams);
                key.FromXmlString(currentCert.PrivateKey.ToXmlString(true));
                string signedData = Utilities.SignAndVerify.SignXml(unsignedXML, key);
                signedXML.LoadXml(signedData);
                var fileDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                signedXML.Save(filePath);
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                ret = false;
            }
            return ret;
        }
        #endregion "Retrieval transaction"

        #region "Handler external apps"
        internal void LoopObserver()
        {
            Process[] processes;
            string[] chunks;
            string processName;
            string fileNameAndPath = $"{Const.appPath}AlephDEV.exe";
            if (!string.IsNullOrEmpty(this.AlephATMAppData.AlephDEVPath)) //Si defino una ruta, la tomo como válida.
                fileNameAndPath = $"{this.AlephATMAppData.AlephDEVPath}\\AlephDEV.exe";
            try
            {
                if (File.Exists(fileNameAndPath))
                {
                    chunks = fileNameAndPath.Split('\\');
                    processName = chunks[chunks.Length - 1];
                    int pos = processName.IndexOf(".");
                    processName = processName.Substring(0, pos);
                    processes = Process.GetProcessesByName(processName);
                    //When there are several processes started, I kill the processes.
                    if (processes.Length > 1)
                    {
                        foreach (Process p in processes)
                        {
                            p.Kill();
                            Log.Error($"--/\\--several process {p.ProcessName} started.");
                        }
                    }

                    //When the process doesn't response, I kill the process.
                    if (processes.Length == 1)
                    {
                        if (processes[0].Responding == false)
                        {
                            processes[0].Kill();
                            Log.Error($"--/\\--Process {processes[0].ProcessName} doesn't response. Kill process.");
                        }
                    }

                    //When there isn´t any started process, I start the process.
                    if (processes.Length == 0)
                    {
                        this.StartExternalApp(fileNameAndPath, this.AlephATMAppData.AlephDEVWindowStyle);
                        Log.Info("--/\\--Start process AlephDEV");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        internal void StartExternalApp(string fileNameAndPath, ProcessWindowStyle windowStyle)
        {
            ProcessStartInfo startInfo;
            try
            {
                if (File.Exists(fileNameAndPath))
                {
                    startInfo = new ProcessStartInfo(fileNameAndPath);
                    startInfo.WindowStyle = windowStyle;
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void KillExternalApp(string fileNameAndPath)
        {
            string[] chunks;
            string processName;

            try
            {
                if (File.Exists(fileNameAndPath))
                {
                    chunks = fileNameAndPath.Split('\\');
                    processName = chunks[chunks.Length - 1];
                    int pos = processName.IndexOf(".");
                    processName = processName.Substring(0, pos);
                    this.KillProcess(processName);
                }
                else
                    Log.Error($"File \"{fileNameAndPath}\" not found.");

            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void KillProcess(string processName)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    foreach (Process p in processes)
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal async Task<bool> WriteIniConfigFileAsync(bool verifyNotes, bool killProcess)
        {
            bool ret = false;
            try
            {
                Log.Info($"Simulate escrow: {verifyNotes}, Kill SP: {killProcess}");
                StringBuilder sb = new StringBuilder();
                string fileName = $"C:\\XFS\\Configuration\\CimMeiEBDS.ini";
                string simulateEscrow = "false";
                string escrowItems = "0";
                string notSupportedRollbackFinishesTransaction = "true";

                if (File.Exists(fileName))
                {
                    if (verifyNotes)
                    {
                        simulateEscrow = "true";
                        escrowItems = "1";
                        notSupportedRollbackFinishesTransaction = "false";
                    }

                    // Leer el contenido existente del archivo INI
                    Dictionary<string, Dictionary<string, string>> iniData = new Dictionary<string, Dictionary<string, string>>();
                    if (File.Exists(fileName))
                    {
                        string[] lines = File.ReadAllLines(fileName);
                        string currentSection = null;

                        foreach (string line in lines)
                        {
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                currentSection = line.Trim('[', ']');
                                if (!iniData.ContainsKey(currentSection))
                                {
                                    iniData[currentSection] = new Dictionary<string, string>();
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(line) && currentSection != null)
                            {
                                string[] keyValue = line.Split(new char[] { '=' }, 2);
                                if (keyValue.Length == 2)
                                {
                                    iniData[currentSection][keyValue[0].Trim()] = keyValue[1].Trim();
                                }
                            }
                        }

                        iniData["Service"]["SimulateEscrow"] = simulateEscrow;
                        iniData["Service"]["EscrowItems"] = escrowItems;
                        iniData["Service"]["NotSupportedRollbackFinishesTransaction"] = notSupportedRollbackFinishesTransaction;

                        // Escribir el contenido actualizado en el archivo INI
                        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                        {
                            foreach (var section in iniData)
                            {
                                await writer.WriteLineAsync($"[{section.Key}]");
                                foreach (var keyValue in section.Value)
                                {
                                    await writer.WriteLineAsync($"{keyValue.Key}={keyValue.Value}");
                                }
                                await writer.WriteLineAsync(); // Añadir una línea en blanco entre secciones
                            }
                        }
                        ret = true;
                        Log.Info("INI file write ok!");
                    }
                    else
                    {
                        Log.Error($"File {fileName} not found");
                    }
                    if (killProcess)
                    {
                        Process[] processes = Process.GetProcessesByName("MeiEBDSSharedProtocol");
                        if (processes.Length > 0)
                        {
                            foreach (Process p in processes)
                            {
                                p.Kill();
                                Log.Info($"Kill process: {p.ProcessName}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        //internal bool WriteIniConfigFile(bool verifyNotes, bool killProcess)
        //{
        //    bool ret = false;
        //    bool wrtiteFile = false;
        //    try
        //    {
        //        Log.Info($"Simulate escrow: {verifyNotes}, Kill SP: {killProcess}");
        //        StringBuilder sb = new StringBuilder();
        //        string fileName = $"C:\\XFS\\Configuration\\CimMeiEBDS.ini";
        //        string simulateEscrow = "false";
        //        string escrowItems = "0";
        //        string notSupportedRollbackFinishesTransaction = "true";

        //        if (File.Exists(fileName))
        //        {
        //            if (verifyNotes)
        //            {
        //                simulateEscrow = "true";
        //                escrowItems = "1";
        //                notSupportedRollbackFinishesTransaction = "false";
        //            }
        //            string[] files = File.ReadAllLines(fileName);
        //            for (int i = 0; i < files.Length; i++)
        //            {
        //                if (files[i].Equals("[Service]"))
        //                {
        //                    if (files[i + 9].Contains("SimulateEscrow"))
        //                    {
        //                        bool origVal = bool.Parse(files[i + 9].Substring(15));
        //                        if (origVal != verifyNotes)//Solo aplica cambio en caso de diferencias entre conf. del archivo y lo que indica la app
        //                        {
        //                            files[i + 9] = $"SimulateEscrow={simulateEscrow}";
        //                            if (files[i + 10].Contains("EscrowItems"))
        //                            {
        //                                files[i + 10] = $"EscrowItems={escrowItems}";
        //                            }
        //                            else
        //                                Log.Error($"TAG \"EscrowItems\" not found");
        //                            if (files[i + 11].Contains("NotSupportedRollbackFinishesTransaction"))
        //                            {
        //                                files[i + 11] = $"NotSupportedRollbackFinishesTransaction={notSupportedRollbackFinishesTransaction}";
        //                            }
        //                            else
        //                                Log.Error($"TAG \"NotSupportedRollbackFinishesTransaction\" not found");
        //                            wrtiteFile = true;
        //                        }
        //                        else
        //                            killProcess = false; //Anulo el reinicio de proceso SP
        //                    }
        //                    else
        //                        Log.Error($"TAG \"SimulateEscrow\" not found");

        //                }
        //                sb.AppendLine(files[i]);
        //            }
        //            if (wrtiteFile)
        //            {
        //                Utils.WriteUTF8FileStream(fileName, sb.ToString());//Pisa el archivo existente
        //                Log.Info("ConfigFile was updated!");
        //            }
        //            else
        //                Log.Info("ConfigFile didn´t to be updated");
        //            ret = true;
        //        }
        //        else
        //        {
        //            Log.Error($"File {fileName} not found");
        //            ret = true;
        //        }
        //        if (killProcess)
        //        {
        //            Process[] processes = Process.GetProcessesByName("MeiEBDSSharedProtocol");
        //            if (processes.Length > 0)
        //            {
        //                foreach (Process p in processes)
        //                {
        //                    p.Kill();
        //                    Log.Info($"Kill process: {p.ProcessName}");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex) { Log.Fatal(ex); }
        //    return ret;
        //}

        #endregion "Handler external apps"

        #region basic utilities
        internal bool IsBNE_S110M()
        {
            switch (this.AlephATMAppData.TerminalModel)
            {
                case Enums.TerminalModel.SNBC_CTE1:
                case Enums.TerminalModel.CTIUL:
                case Enums.TerminalModel.SNBC_CTI90:
                case Enums.TerminalModel.Depositario:
                case Enums.TerminalModel.MADERO_BR:
                case Enums.TerminalModel.SNBC_IN:
                case Enums.TerminalModel.BDM_300:
                    return true;
                default:
                    return false;
            }
        }
        //MixCalculator / Core.cs
        /// <summary>
        /// Shifft dot on decimal number according to the number of places.
        /// </summary>
        public decimal ShifftDot(decimal number, int places)
        {
            if (places > 0) // Si el número de lugares es positivo, desplaza el punto hacia la izquierda
            {
                for (int i = 0; i < places; i++)
                {
                    number *= 10;
                }
            }
            else if (places < 0) // Si el número de lugares es negativo, desplaza el punto hacia la derecha
            {
                for (int i = 0; i > places; i--)
                {
                    number /= 10;
                }
            }
            return number;
        }
        #endregion

        /// <summary>
        /// Set branding files for debug mode only.
        /// </summary>
        /// <param name="branding">Current branding</param>
        internal void SetBrandingFiles(Enums.Branding branding)
        {
            if (branding != Enums.Branding.Prosegur)
            {
                string scriptPath = "";
                try
                {
                    switch (branding)
                    {
                        case Enums.Branding.Prosegur:
                            scriptPath = "Prosegur";
                            break;
                        case Enums.Branding.Macro:
                            scriptPath = "BancoMacro";
                            break;
                        case Enums.Branding.Galicia:
                            scriptPath = "Prosegur";
                            break;
                        case Enums.Branding.PlanB:
                            scriptPath = "Prosegur";
                            break;
                        case Enums.Branding.RedPagosA:
                            scriptPath = "RedPagos";
                            break;
                        case Enums.Branding.Ciudad:
                            scriptPath = "BancoCiudad";
                            break;
                        case Enums.Branding.DepositarioRetail:
                            scriptPath = "DepositarioRetail";
                            break;
                        case Enums.Branding.Atlas:
                            scriptPath = "BancoAtlas";
                            break;
                        default:
                            Log.Error($"Unknown branding: {branding}");
                            return;

                    }
                    ProcessStartInfo startInfo;
                    scriptPath = $"{Const.appPath}Themes\\UpdateScreens{scriptPath}.bat";
                    if (File.Exists(scriptPath))
                    {
                        startInfo = new ProcessStartInfo(scriptPath);
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        Process.Start(startInfo);
                        Log.Info($"Branding {branding} change executed");
                    }
                    else
                        Log.Error($"File not found: {scriptPath}");
                }
                catch (Exception ex) { Log.Fatal(ex); }
            }
        }

    }
}
