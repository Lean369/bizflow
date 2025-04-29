using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using System.Diagnostics;
using PRDM_Interface;
using System.Threading;

namespace Business
{
    public delegate void DelegateFindUserByLogin(Users user);
    public class PrdmHost : Ihost
    {
        public event DelegateFindUserByLogin EvtFindUserByLogin; //Evento que informa la llegada de un usuario de la BD
        Core core;
        internal PRDM Prdm;

        public PrdmHost(Core _core)
        {
            this.Prdm = PRDM.GetInstance();
            this.Prdm.PrdmCore.EvtPRDMDataReceive += new PrdmCore.DelegatePRDMDataReceive(this.ProcessPrdmRequest);//Recibe los pedido de datos del PRDM
            this.core = _core;
        }

        private void ProcessPrdmRequest(string msg)
        {
            Contents contents;
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                if (msg.Contains("GET_CONTENTS"))
                {
                    GetContentsRequest GetContents = new GetContentsRequest(msg);
                    if (GetContents.machine_id.Equals(this.Prdm.PrdmCore.PrdmAppData.MachineID))
                    {
                        contents = this.core.Counters.GetContents();
                        this.Prdm.PrdmCore.ProcessGetContentsResponse(contents);
                    }
                    else
                        GlobalAppData.Instance.GetLog().ProcessLog(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Error, string.Format("Wrong machineID: {0}", GetContents.machine_id));
                }
                if (msg.Contains("GET_DEVICE_STATUS"))
                {
                    //TODO: enviar status
                }
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
        }

        AuthorizationResult Ihost.CashDeposit(Enums.TransactionType transactionType, string transactionNumber, Contents contents)
        {
            bool ret = false;
            string extraDataName = string.Empty;
            string extraDataValue = string.Empty;
            string jsonContents = string.Empty;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, transactionNumber);
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                if (this.core.Bo.ExtraInfo.Channel != null)
                    if (!string.IsNullOrEmpty(this.core.Bo.ExtraInfo.Channel.Legajo) && !string.IsNullOrEmpty(this.core.Bo.ExtraInfo.Channel.Terminal) && !string.IsNullOrEmpty(this.core.Bo.ExtraInfo.Channel.Transaccion))
                    {
                        extraDataName = "CANAL";
                        extraDataValue = string.Format("{0}-{1}-{2}", this.core.Bo.ExtraInfo.Channel.Legajo, this.core.Bo.ExtraInfo.Channel.Terminal, this.core.Bo.ExtraInfo.Channel.Transaccion);
                    }
                ret = this.Prdm.PrdmCore.ProcessTransactionMsg(transactionType, this.core.Bo.ExtraInfo.UserProfile.User,this.core.Bo.ExtraInfo.UserProfile.UserName, transactionNumber.ToString(), extraDataName, extraDataValue, contents);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                    //D)- Send counters message
                    Thread.Sleep(30);
                    this.core.AlephHost.Host.SendContents();
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        AuthorizationResult Ihost.SendContents()
        {
            bool ret = false;
            Contents contents;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "0000");
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                contents = this.core.Counters.GetContents();
                ret = this.Prdm.PrdmCore.ProcessCountersMsg(contents);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Envío de contadores de valores validados en caso de ShipOut
        /// </summary>
        AuthorizationResult Ihost.SendCollection(string secuence)
        {
            bool ret = false;
            Contents contents;
            int batchNumber = 0;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "0000");
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                contents = this.core.Counters.GetContents(Detail.ContainerIDType.CashAcceptor);
                batchNumber = this.core.Counters.GetBATCH();
                ret = this.Prdm.PrdmCore.ProcessCollectionMsg(Enums.TransactionType.COLLECTION, "", "", secuence, batchNumber, "", "", contents);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Envío de contadores de valores declarados en caso de ShipOut
        /// </summary>
        AuthorizationResult Ihost.SendCollectionDeclared(string secuence)
        {
            bool ret = false;
            Contents contents;
            int batchNumber = 0;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "0000");
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                contents = this.core.Counters.GetContents(Detail.ContainerIDType.Depository);
                batchNumber = this.core.Counters.GetBATCH();
                ret = this.Prdm.PrdmCore.ProcessCollectionMsg(Enums.TransactionType.COLLECTION_DECLARED, "", "", secuence, batchNumber, "", "", contents);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Envío los datos del los tickets para almacenar en la BD
        /// </summary>
        AuthorizationResult Ihost.SendTicketData(string jsonData, int ticketID)
        {
            bool ret = true;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "0000");
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                this.Prdm.PrdmCore.StoreTicket(jsonData, ticketID);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Envío de errores de dispositivos al PRDM
        /// </summary>
        AuthorizationResult Ihost.SendDeviceError(List<Device> lstDevices)
        {
            bool ret = false;
            AuthorizationResult authorizationResult = new AuthorizationResult(AuthorizationStatus.Declined, "0000");
            string jsonLstDevices = string.Empty;
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                ret = this.Prdm.PrdmCore.ProcessErrorMsg(lstDevices);
                if (ret)
                {
                    authorizationResult.authorizationStatus = AuthorizationStatus.Authorized;
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
                }
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Host Status: {0}", authorizationResult.authorizationStatus.ToString()));
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("Core.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return authorizationResult;
        }

        /// <summary>
        /// Solicita en forma asincrónica los datos del usuario a la BD
        /// </summary>
        /// <param name="userNumber"></param>
        void Ihost.FindUserByLogin(string userNumber)
        {
            Thread prdmThd;
            try
            {
                prdmThd = new Thread(new ParameterizedThreadStart(this.FindUserByLoginThd));
                prdmThd.Start(userNumber);
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
        }

        protected virtual void RaiseEvtFindUserByLogin(Users userNumber)
        {
            DelegateFindUserByLogin tmp = EvtFindUserByLogin;
            if (tmp != null)
                tmp(userNumber);
        }

        /// <summary>
        /// Obtiene los datos de un usuario de la BD de PRDM
        /// </summary>
        /// <param name="param"></param>
        private void FindUserByLoginThd(object param)
        {
            Users user = new Users();
            try
            {
                GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Debug, string.Format("|--->"));
                ////bypass
                //user.Deactivated = false;
                //user.Name = "Roberto Perez";
                //user.Role = "USUARIO";
                //this.RaiseEvtFindUserByLogin(user); //Lanza los datos de usuario a través de un evento
                ////Bypass


                if (this.Prdm.PrdmCore.FindUserByLogin(param.ToString(), out user))
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Get user data: {0}", user.Name));
                else
                    GlobalAppData.Instance.GetLog().ProcessLog(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Warning, string.Format("Can´t get user data: {0}", param.ToString()));
                this.RaiseEvtFindUserByLogin(user); //Lanza los datos de usuario a través de un evento
            }
            catch (Exception ex) { GlobalAppData.Instance.GetLog().ProcessLogException(string.Format("PrdmHost.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
        }
    }
}
