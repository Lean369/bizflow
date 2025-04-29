using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Entities;

namespace Business
{
    public class NDChost
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private static readonly NLog.Logger Trace = NLog.LogManager.GetLogger("TRACE");
        protected Core Core { get; set; }
        private Utilities.ServerSocket SktServer;
        private Utilities.ClientSocket SktClient;
        private System.Timers.Timer TimerCommClient;

        public NDChost(Core core)
        {
            this.Core = core;
            this.InitializeSockets();
        }

        /// <summary>
        /// Envía datos hacia el DH.
        /// </summary>
        /// <param name="sDataToSend"></param>
        public bool SendDataToDH(string sDataToSend)
        {
            string sData = sDataToSend;
            bool ret = false;
            try
            {
                Log.Debug("/--->");
                if (this.Core.AlephATMAppData.HeaderEnable)
                    sData = Utilities.Header.AddHeader(sDataToSend);
                if (this.Core.AlephATMAppData.RoleServerNDCHost)
                {
                    if (this.SktServer.StateConnection)
                    {
                        ret = this.SktServer.SendData(sData);
                        Trace.Trace($"SND from NDC-SERVER ==> {Environment.NewLine}{sDataToSend}");
                        Log.Trace("SND data to NDC host: {0}", sDataToSend.Replace(Const.FS, '?'));
                    }
                    else
                    {
                        Log.Warn("Lost connection.");
                    }
                }
                else //Client mode
                {
                    if (this.SktClient.StateConnection)
                    {
                        this.SktClient.SendStringData(sData);
                        ret = true;
                        Trace.Trace($"SND from NDC-CLIENT ==>{Environment.NewLine}{sDataToSend}");
                        Log.Trace("SND data to NDC host: {0}", sDataToSend.Replace(Const.FS, '?'));
                    }
                    else
                    {
                        Log.Warn("Lost connection.");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Manejador para los datos entrates que provienen del socket contra DH.
        /// </summary>
        /// <param name="sDataIn"></param>
        private void Handler_SocketReceivedDataEVT(string sDataIn)
        {
            List<string> aDataOut = new List<string>();
            try
            {
                Log.Debug("/--->");
                if (this.Core.AlephATMAppData.HeaderEnable)
                {
                    aDataOut = Utilities.Header.RemoveHeader(sDataIn, 50); //Verifico header y separo los mensajes si es necesario.
                    foreach (string message in aDataOut)
                    {
                        this.ProcessSwitchMessage(message);
                    }
                }
                else
                {
                    this.ProcessSwitchMessage(sDataIn);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Identifica y procesa los mensajes recibidos desde el switch.
        /// </summary>
        /// <param name="dataIn"></param>
        private void ProcessSwitchMessage(string dataIn)
        {
            string[] dataInParsed;
            Const.MsgType msgType = Const.MsgType.UnDefined;
            List<StateTable_Type> listStateTable = new List<StateTable_Type>();
            try
            {
                Trace.Trace($"RCV from NDC-HOST <== {Environment.NewLine}{dataIn}");
                Log.Trace("RCV data from NDChost: {0}", dataIn.Replace(Const.FS, '?'));
                dataInParsed = dataIn.Split(Const.FS);
                if (dataInParsed.Length > 2)
                {
                    msgType = this.Core.Parser.GetMessageType(dataIn);
                    Log.Info("Process MSG: \"{0}\"", msgType);
                    switch (msgType)
                    {
                        case Const.MsgType.TransactionReply:
                            {
                                TransactionReplyCommand_Type tR = this.Core.Parser.TransactionReplyMessageParser(dataIn);
                                ScreenData_Type screenData = new ScreenData_Type();
                                if (tR.ScreenDisplay.ScreenUpdates.ScreenUpdate.Item != null)
                                {
                                    if (tR.ScreenDisplay.ScreenUpdates.ScreenUpdate.Item is Digits2GroupScreen_Type)
                                    {
                                        Digits2GroupScreen_Type digits2GroupScreen = tR.ScreenDisplay.ScreenUpdates.ScreenUpdate.Item as Digits2GroupScreen_Type;
                                        screenData.Command = digits2GroupScreen.ScreenData;
                                        this.Core.AddScreenData(digits2GroupScreen.ScreenNumber, screenData);//Actualizo los datos de pantalla. 
                                    }
                                }
                                else
                                    Log.Warn(string.Format("Without screen update data"));
                                this.Core.RaiseEvtRcvMsgReply(tR);
                                break;
                            }
                        case Const.MsgType.Itr:
                            {
                                InteractiveTransactionResponse_Type iTR = this.Core.Parser.ITRMessageParser(dataIn);
                                ScreenData_Type screenData = new ScreenData_Type();
                                screenData.Command = iTR.ScreenData;
                                this.Core.AddScreenData("ITR", screenData); //Actualizo los datos de pantalla.                             
                                this.Core.RaiseEvtRcvMsgReply(iTR);
                                break;
                            }
                        case Const.MsgType.GoInServiceTermCmd:
                            {
                                this.Core.RequestChangeMode(Const.TerminalMode.InService);
                                break;
                            }
                        case Const.MsgType.GoOutOfServiceTermCmd:
                            {
                                this.Core.RequestChangeMode(Const.TerminalMode.OutOfService);
                                break;
                            }
                        case Const.MsgType.SndEnhConfDataTermCmd:
                            {
                                //SendConfigurationInformation_Type sendConfigurationInformation;
                                this.SendDataToDH(String.Format("22{0}000{0}{0}F{0}11651{0}00004000000000044440000000000000004000{0}187F000501000680000000D8000101030200007F7F0000000002000000000000000003000000{0}00000011000000000000000000{0}010110000000", Entities.Const.FS));
                                break;
                            }
                        case Const.MsgType.SndSupplyCountersTermCmd:
                            {
                                SendSupplyCounters_Type sendSupplyCounters;
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.SndConfIDTermCmd:
                        case Const.MsgType.DateAndTime:
                        case Const.MsgType.SndConfHwdTermCmd:
                        case Const.MsgType.SndSuppiesStatusTermCmd:
                        case Const.MsgType.SndHwdFitnessTermCmd:
                        case Const.MsgType.SndSensorStatusTermCmd:
                        case Const.MsgType.SndSoftIDTermCmd:
                        case Const.MsgType.SndLocalConfTermCmd:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.SndNoteDefinitionsTermCmd:
                            {
                                this.SendDataToDH(String.Format("22{0}000{0}{0}F{0}NA!01ARS2A   !02ARS5B   #03ARS10B  !04ARS20B  #05ARS50B  #06ARS100B #07ARS200A #08ARS500A #09ARS1KA  ", Entities.Const.FS));
                                break;
                            }
                        case Const.MsgType.Screen:
                            {
                                foreach (var item in this.Core.Parser.ScreenDataMessageParser(dataIn))
                                {
                                    if (item.Value is ScreenData_Type)
                                    {
                                        ScreenData_Type screenData = (ScreenData_Type)item.Value;
                                        this.Core.AddScreenData(item.Key, screenData);
                                    }
                                    else if (item.Value is SimulatedPrePrintedReceiptScreen_Type)
                                    {
                                        SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreen = (SimulatedPrePrintedReceiptScreen_Type)item.Value;
                                        this.Core.AddSimulatedPrePrintedReceiptScreen(simulatedPrePrintedReceiptScreen);
                                    }
                                }
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.State:
                            {
                                if (this.Core.Download.StateTables == null)
                                    this.Core.Download.StateTables = new List<StateTable_Type>();
                                listStateTable = this.Core.Parser.StateTablesMessageParser(dataIn);
                                foreach (StateTable_Type item in listStateTable) //Sobrescribo con los estados nuevos
                                {
                                    this.Core.Download.StateTables.RemoveAll(x => x.StateNumber.Equals(item.StateNumber, StringComparison.Ordinal));
                                }
                                this.Core.Download.StateTables.AddRange(this.Core.Parser.StateTablesMessageParser(dataIn));
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.ConfParam:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.EnhConfParam:
                            {
                                this.Core.Download.EnhancedConfigurationParametersData = this.Core.Parser.EnhancedParametersMessageParser(dataIn);
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.Fit:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.Emv:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.Aid:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.ConfIDload:
                            {
                                ///Serializa el download!!!
                                this.Core.UpdateDownloadXMLfiles();
                                this.SendReadyStatus("9");
                                break;
                            }
                        case Const.MsgType.EncryptionKeyChange:
                            {
                                this.SendReadyStatus("9");
                                break;
                            }
                        default:
                            {
                                Log.Warn("Mensaje de switch desconocido.");
                                break;
                            }
                    }
                }
                else
                {
                    Log.Error("Cantidad de campos insuficiente.");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Envía una respuesta Ready al DH.
        /// </summary>
        /// <param name="ready"></param>
        public void SendReadyStatus(string ready)
        {
            this.SendDataToDH(String.Format("22{0}{1}{0}{0}{2}", Const.FS, this.Core.TerminalInfo.LogicalUnitNumber, ready));
        }

        /// <summary>
        /// Envía una respuesta Ready al DH.
        /// </summary>
        /// <param name="ready"></param>
        public void SendEspecificCommandRejectStatus(string statusValue)
        {
            this.SendDataToDH(String.Format("22{0}{1}{0}{0}C{2}", Const.FS, this.Core.TerminalInfo.LogicalUnitNumber, statusValue));
        }

        /// <summary>
        /// Maneja las notificaciones de las desconexiones contra el DH
        /// </summary>
        /// <param name="IDTerminal"></param>
        private void Handler_SocketFinalizedConnectionEVT(System.Net.IPEndPoint IDTerminal)
        {
            try
            {
                string sTextToShow = string.Format("DISCONNECTED WITH SWITCH: {0}:{1}", IDTerminal.Address.ToString(), IDTerminal.Port);
                Log.Trace(sTextToShow.Replace("\r", "\\r").Replace("\n", "\\n"));
                Trace.Trace($"ATM_INF {sTextToShow}");
                this.Core.RequestChangeMode(Const.TerminalMode.OffLine);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Maneja las notificaciones de nuevas conexiones contra el DH
        /// </summary>
        /// <param name="IDTerminal">Identificación de la terminal que se conecto.</param>
        private void Handler_SocketNewConnectionEVT(System.Net.IPEndPoint IDTerminal)
        {
            try
            {
                string sTextToShow = string.Format("CONNECTED WITH: {0}:{1}", IDTerminal.Address.ToString(), IDTerminal.Port);
                Log.Trace(sTextToShow.Replace("\r", "\\r").Replace("\n", "\\n"));
                Trace.Trace($"ATM_INF {sTextToShow}");
                this.Core.RequestChangeMode(Const.TerminalMode.OnLine);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Manejador para el evento del timer. Conecta el ATM.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void SktClientAtm_OnTimedEVT(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.SktClient.Connect();
                this.TimerCommClient.Enabled = false;
                System.Net.IPAddress ipAddr;
                ipAddr = System.Net.IPAddress.Parse(this.Core.AlephATMAppData.IpHostClientNDCHost);
                System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAddr, this.Core.AlephATMAppData.RemotePortNDCHost);
                this.Handler_SocketNewConnectionEVT(remoteEP);
            }
            catch (Exception) { }
        }

        private void InitializeSockets()
        {
            try
            {
                Log.Debug("/--->");
                if (this.Core.AlephATMAppData.RoleServerNDCHost)
                {
                    Log.Info("Role: Server. IpHost: {0} - LocalPort: {1}", this.Core.AlephATMAppData.IpHostServerNDCHost, this.Core.AlephATMAppData.LocalPortNDCHost);
                    this.SktServer = new Utilities.ServerSocket(this.Core.AlephATMAppData.IpHostServerNDCHost, this.Core.AlephATMAppData.LocalPortNDCHost, Encoding.Default);
                    //Conecta el evento que informa una nueva conexión contra el DH con un manejador.
                    this.SktServer.NewConnectionEVT += new Utilities.ServerSocket.EventNewConnection(this.Handler_SocketNewConnectionEVT);
                    //Conecta el evento que informa una desconexión contra el DH con un manejador.
                    this.SktServer.FinalizedConnectionEVT += new Utilities.ServerSocket.EventFinalizedConnection(this.Handler_SocketFinalizedConnectionEVT);
                    //Conecta el evento que envía los datos recibidos del DH con un manejador.
                    this.SktServer.ReceivedDataEVT += new Utilities.ServerSocket.EventReceivedData(this.Handler_SocketReceivedDataEVT);
                    this.SktServer.Listen();
                    string sTextToShow = string.Format("LISTEN: {0}:{1}", this.Core.AlephATMAppData.IpHostServerNDCHost, this.Core.AlephATMAppData.LocalPortNDCHost);
                    Trace.Trace($"ATM_INF {sTextToShow}");
                    Log.Trace(sTextToShow);
                }
                else
                {
                    Log.Info("Role: Client. IpHost: {0} - RemotePort: {1}", this.Core.AlephATMAppData.IpHostClientNDCHost, this.Core.AlephATMAppData.RemotePortNDCHost);
                    this.SktClient = new Utilities.ClientSocket(this.Core.AlephATMAppData.IpHostClientNDCHost, this.Core.AlephATMAppData.RemotePortNDCHost, Encoding.Default, true);
                    //Conecta el evento que informa una desconexión contra el DH con un manejador.
                    this.SktClient.FinalizedConnectionEVT += new Utilities.ClientSocket.EventFinalizedConnection(this.Handler_SocketFinalizedConnectionEVT);
                    //Conecta el evento que envía los datos recibidos del DH con un manejador.
                    this.SktClient.ReceivedStringDataEVT += new Utilities.ClientSocket.EventReceivedStringData(this.Handler_SocketReceivedDataEVT);
                    this.TimerCommClient = new System.Timers.Timer(100);
                    this.TimerCommClient.Elapsed += new System.Timers.ElapsedEventHandler(SktClientAtm_OnTimedEVT);
                    this.TimerCommClient.Enabled = true;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
    }
}
