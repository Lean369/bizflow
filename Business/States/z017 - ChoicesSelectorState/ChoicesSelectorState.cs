using Entities;
using Entities.PaymentService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Metadata;
using System.Threading.Tasks;

namespace Business.ChoicesSelectorState
{
    public class ChoicesSelectorState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        ChoicesSelectorStateTableData_Type ChoicesSelectorStateTableData; //Tabla con datos provenientes del download.
        PropertiesChoicesSelectorState prop;
        bool ret = false;
        private bool MoreTimeSubscribed = false;
        private List<string> ListOfAck = new List<string>();
        private ServicesPaymentConfig servicesPaymentConfig;

        #region "Constructor"
        public ChoicesSelectorState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "ChoicesSelectorState";
            this.ChoicesSelectorStateTableData = (ChoicesSelectorStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesChoicesSelectorState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesChoicesSelectorState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.NextStateNumberA))
                    this.prop.NextStateNumberA = this.ChoicesSelectorStateTableData.NextStateNumberA;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberB))
                    this.prop.NextStateNumberB = this.ChoicesSelectorStateTableData.NextStateNumberB;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberC))
                    this.prop.NextStateNumberC = this.ChoicesSelectorStateTableData.NextStateNumberC;
                if (string.IsNullOrEmpty(this.prop.NextStateNumberD))
                    this.prop.NextStateNumberD = this.ChoicesSelectorStateTableData.NextStateNumberD;
                if (string.IsNullOrEmpty(this.prop.HardwareErrorNextStateNumber))
                    this.prop.HardwareErrorNextStateNumber = this.ChoicesSelectorStateTableData.HardwareErrorNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeoutNextStateNumber))
                    this.prop.TimeoutNextStateNumber = this.ChoicesSelectorStateTableData.TimeoutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.ChoicesSelectorStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.BackNextStateNumber))
                    this.prop.BackNextStateNumber = this.ChoicesSelectorStateTableData.BackNextStateNumber;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
            if (!ServicesPaymentConfig.GetMapping(out servicesPaymentConfig))
                Log.Error("Could not get ServicesPaymentConfig configuration.");
        }
        #endregion "Constructor"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
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
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                //Bypass
                //this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberA);
                if (CheckCurrentStep() != Continuation.Stay)
                    return;
                //Bypass
                this.AddEventHandlers();

                //setting operation currency
                this.Core.Bo.ExtraInfo.Currency = this.Core.AlephATMAppData.DefaultCurrency;

                this.Core.Bo.ExtraInfo.Amount = 0;
                this.CallHandler(this.prop.OnShowScreen);

                this.prop.OnChoicesSelector.Parameters = new JObject
                {
                    ["isMobileTopUp"] = (JToken)this.Core.Bo.ExtraInfo.IsMobileTopup,
                    ["shoppingCartItems"] = (JToken)this.Core.Bo.ExtraInfo.ShoppingCartItems
                };
                this.CallHandler(this.prop.OnChoicesSelector);
                IsUserNotificationPending(); //mostrar mensajes pendientes de ser mostrados

                this.StartTimer(false);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerOthersKeysReturn(string othersKeys)
        {

            Log.Info("/--> Key press: {0}", othersKeys);
            switch (othersKeys)
            {
                case "CONTINUE":
                    this.Core.HideScreenModals(); //Quito los avisos de pantalla
                    break;
                case "ENTER": //Confirma TX
                    {
                        Log.Trace("Ejecutnado ENTER, yendo a {0}", this.prop.NextStateNumberC);
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberC);
                        break;
                    }
                case "CANCEL":
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
            }
        }

        private async void HandlerInputData(string dataInput, string dataLink)
        {
            try
            {
                Log.Info("-> Input data: {0}", dataInput);
                this.timerScreen.Stop();
                if (!string.IsNullOrEmpty(dataInput))
                {
                    switch (dataLink)
                    {
                        case "FindService":
                            {
                                var services = await Task.Run(() => FindService(dataInput));
                                if (services.success)
                                {
                                    //filter out services not enabled
                                    services.data = services.data.Where(s => s.Enabled).ToArray();
                                    this.prop.OnFoundServices.Parameters = Utilities.Utils.NewtonsoftSerialize(services.data);
                                    this.CallHandler(this.prop.OnFoundServices);
                                }
                                else
                                {
                                    Log.Trace("Falló busqueda de servicios");
                                    this.CallHandler(this.prop.OnConnectionFailed);
                                }
                                break;
                            }
                        case "StartPayment":
                            {
                                this.StopTimer();
                                // dataInput es el service id del servicio a iniciar el proceso de pago
                                //this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB);
                                long.TryParse(dataInput, out long serviceID);
                                this.WriteEJ($"Starting payment wizard on service ID {serviceID}");
                                this.BillProcess(serviceID);
                                break;
                            }
                        case "GoToShoppingCart":
                            Log.Trace($"Going to shopping cart");
                            this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberD);
                            break;
                    }
                }
                this.timerScreen.Start();
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
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.RemoveTopupProp();
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.RemoveEventHandlers();
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void AddEventHandlers()
        {
            this.Core.EvtOthersKeysPress += new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData += new Core.DelegateSendInputData(this.HandlerInputData);
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandlerOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
        }

        #region "PAYMENT BUSINESS"
        private enum Continuation { NextWizardStep, NextState, Stay }

        /// <summary>
        /// Checks if there are pending steps to be executed to complete the payment wizard.
        /// DETAIL: It verifies if Bo.ExtraInfo.ExtraData contains user's entered data from prevoius state (setExtraData). 
        /// In case it has, then calls BillProcess to continue the payment process.
        /// BillProcess may require several steps to fulfill an add payment wizard.
        /// </summary>
        private Continuation CheckCurrentStep()
        {
            var fieldsExtraData = this.Core.Bo.ExtraInfo.ExtraData?.Where(a => a.ExtraDataType == Enums.ExtraDataType.dynamic);
            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.Barcode)) //RECEIVED BARCODE
            {
                int? selectedId = null;

                if (fieldsExtraData.Count() == 1 &&
                    (fieldsExtraData is object && fieldsExtraData.Any()) &&
                    fieldsExtraData.First().TagValue.ToUpper().Contains("COLISION"))
                {
                    // collision resolution
                    Log.Trace("Resolviendo colisión");
                    var colissionRes = Utilities.Utils.NewtonsoftDeserialize<FieldDetail>(out bool ret, fieldsExtraData.First().TagValue);
                    if (ret)
                    {
                        selectedId = Convert.ToInt32(colissionRes.SelectedOptionsID[0]);
                        this.Core.Bo.ExtraInfo.ExtraData.RemoveAll(a => a.ExtraDataType == Enums.ExtraDataType.dynamic); //remove collision opts
                    }
                }

                var _barcode = this.Core.Bo.ExtraInfo.Barcode;
                this.Core.Bo.ExtraInfo.Barcode = null; //clear before calling billProcess
                BillProcess(serviceId: selectedId, barcode: _barcode);
                return Continuation.NextState;
            }
            else if (!this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("billId") || !this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("currentStep"))
            {
                
                this.Core.Bo.ExtraInfo.ExtraData.RemoveAll(a => a.ExtraDataType == Enums.ExtraDataType.dynamic);
                return Continuation.Stay;
            }
            //we have user's entered data, process must be made 
            var fieldList = new List<FieldDetail>();
            foreach(var fxd in fieldsExtraData)
            {
                var dataObj = Utilities.Utils.NewtonsoftDeserialize<FieldDetail>(out bool ret, fxd.TagValue);
                if (!ret) continue;
                fieldList.Add(dataObj);
            }
            this.Core.Bo.ExtraInfo.ExtraData.RemoveAll(a => a.ExtraDataType == Enums.ExtraDataType.dynamic); //remove all dynamic fields received from previous form
            var billID = (long)this.Core.Bo.ExtraInfo.HostExtraData["billId"];
            var curStep = (short)(Convert.ToInt16(this.Core.Bo.ExtraInfo.HostExtraData["currentStep"]) + 1);
            this.BillProcess(billid: billID, step: curStep, fields: fieldList);
            return Continuation.NextState;
        }


        /// <summary>
        /// Start a bill process from here by providing a serviceId or barcode. 
        /// In subsequent steps you'll have to call this method again but only providing the FieldDetail list with the user's option selection.
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="barcode"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private async void BillProcess(long? serviceId = null, string barcode = null, long? billid = null, short step = 0, List<FieldDetail> fields = null)
        {
            (bool success, Bill data, string message, AuthorizationStatus authStatus) res;
            if (serviceId.HasValue || !string.IsNullOrEmpty(barcode))
                res = await Task.Run(() => InitBillPayment(serviceId, barcode));
            else if (billid.HasValue && fields != null)
                res = await Task.Run(() => ProcessBillPayment(billid.Value, step, fields));
            else return;
            if (!res.success)
            {
                Log.Error("Error on execution of BillPayment. OPERATION: " + res.authStatus.ToString());
                //MANEJAR EL ERROR
                switch (res.authStatus)
                {
                    case AuthorizationStatus.UnavailableService:
                        this.Core.Bo.ExtraInfo.PendingUserNotification = res.message ?? "Error user input";
                        IsUserNotificationPending();
                        break;
                    case AuthorizationStatus.Declined:
                        //cargar mensaje de error para mostrar al usuario y volver al formulario (setextradata)
                        this.Core.Bo.ExtraInfo.PendingUserNotification = res.message ?? "Error user input";
                        this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to reload form in SetExtraData
                        break;
                    case AuthorizationStatus.CommsError:
                        //posible comunication error occurred, should we cancel operation?
                        if(!string.IsNullOrWhiteSpace(barcode))
                            this.SetActivityResult(StateResult.HWERROR, this.prop.HardwareErrorNextStateNumber); //si viene por barcode, directamente cancelar
                        else
                            this.CallHandler(this.prop.OnConnectionFailed); //mostrar popup de error de comunicacion
                        break;
                }
                return;
            }
            if (res.data.Status == PaymentStatus.Ready) //bill process is done so we can jump to next state
            {
                Log.Trace(" RESULT -> status ready");
                if (!this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("billId"))
                    this.Core.AddHostExtraData("billId", res.data.BillID);

                if(this.Core.Bo.ExtraInfo.CurrentTxn == Enums.AvTxn.paymentServicesTx || this.Core.Bo.ExtraInfo.CurrentTxn == Enums.AvTxn.mobileTopupTx)
                {
                    var curStep = this.Core.Bo.ExtraInfo.HostExtraData.ContainsKey("currentStep") ? (short)Convert.ToInt16(this.Core.Bo.ExtraInfo.HostExtraData["currentStep"]) : 0;
                    if (curStep == 0)
                    {
                        curStep++;
                        this.Core.AddHostExtraData("currentStep", curStep);
                    }
                }

                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberA); //go to next state
                return;
            }
            else if (res.data.Status == PaymentStatus.Paid)
            {
                //cargar mensaje de error para mostrar al usuario y volver al formulario (setextradata)
                this.Core.Bo.ExtraInfo.PendingUserNotification = "language.common.msg.billAlreadyPaid";
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to reload form in SetExtraData
            }   
            else if (res.data.Status == PaymentStatus.Pending)
            {
                //validar currency del pago para descartar currencies no manejados por el cajero
                var billCurrency = res.data.InputFieldDetails.FirstOrDefault(f => f.Type == Field.FieldType.Currency);
                if(billCurrency != null && !billCurrency.Value.Equals(this.Core.Bo.ExtraInfo.Currency))
                {
                    //el pago actual es de un currency diferente del local, no se puede continuar con este pago, avisar al cliente
                    Log.Warn("This teller can not process payments for currency id {0}", billCurrency.Value);
                    this.Core.Bo.ExtraInfo.PendingUserNotification = "language.choicesSelector.msg.currencyNotSupported";
                    this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to reload form in SetExtraData
                    return;
                }

                Log.Trace(" RESULT -> status PENDING");
                //set payment data in business object
                this.Core.AddHostExtraData("billId", res.data.BillID); //this id will be required to add bill to cart
                this.Core.AddHostExtraData("currentStep", res.data.StepCurrent);

                //Set fields to display in SetExtraDataState
                this.Core.Bo.ExtraInfo.SetExtraDataFields = new List<ExtraDataConf>(); //initialize or clear
                this.Core.Bo.ExtraInfo.SetExtraDataInfo = new SetExtraDataInfo
                {
                    CurrentStep = res.data.StepCurrent,
                    StepLength = res.data.StepLast,
                    ServiceId = res.data.Service.ServiceID ?? this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceId,
                    ServiceName = res.data.Service.ServiceName ?? this.Core.Bo.ExtraInfo.SetExtraDataInfo.ServiceName
                };
                foreach (var field in res.data.InputFieldDetails)
                {
                    var fieldType = field.Editable ? Enums.ExtraDataType.dynamic : Enums.ExtraDataType.displayOnly;
                    switch (field.Type)
                    {
                        case Field.FieldType.Numeric:
                        case Field.FieldType.Document:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, field.Name, field.DisplayText, true, "input", "numeric", 0, field.Length, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }, field.Value, field.Mandatory));
                            break;
                        case Field.FieldType.Amount:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, "amountLimit", field.DisplayText, true, "input", "numeric", 0, 20, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }, field.Value, field.Mandatory));
                            break;
                        case Field.FieldType.Text:
                        case Field.FieldType.Description:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, field.Name, field.DisplayText, true, "input", "text", 0, field.Length, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }, field.Value, field.Mandatory));
                            break;
                        case Field.FieldType.Select:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, field.Name, field.DisplayText, true, "radio", "", 0, 0, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }));
                            break;
                        case Field.FieldType.Multiselect:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, field.Name, field.DisplayText, true, "select-multiple", "", 0, 0, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }));
                            break;
                        case Field.FieldType.Boolean:
                            this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, fieldType, true, field.Name, field.DisplayText, true, "single-checkbox", "", 0, 0, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }));
                            break;
                    }
                }
                Log.Trace(" RESULT -> return to SetExtraData.");
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to display form in SetExtraData
            }
            else if (res.data.Status == PaymentStatus.Other && res.data.Service.ServiceID == 0) //COLISION
            {
                Log.Trace($"Boleta tiene colisión entre dos o más servicios. | ESTADO: {res.data.Status}");
                //cargar mensaje de error para mostrar al usuario y volver al formulario (setextradata)
                this.Core.Bo.ExtraInfo.Barcode = barcode; //guardar barcode para reintentar
                foreach(var field in res.data.InputFieldDetails)
                {
                    if (field.Name.ToUpper() == "COLISION")
                    {
                        //this.Core.Bo.ExtraInfo.PendingUserNotification = res.message;
                        this.Core.Bo.ExtraInfo.SetExtraDataFields = new List<ExtraDataConf>(); //initialize or clear
                        this.Core.Bo.ExtraInfo.SetExtraDataInfo = new SetExtraDataInfo
                        {
                            CurrentStep = 1,
                            StepLength = 1
                        };

                        field.DisplayText = "Seleccione el servicio a pagar";
                           
                        foreach (var fieldOption in field.Options)
                        {
                            // Reassing service id to radio values 
                            var curServiceId = fieldOption.Fields.Find(e => e.Name.ToUpper() == "SERVICIOID").Value;

                            fieldOption.OptionID = Convert.ToInt32(curServiceId);

                            if (fieldOption.Fields != null && fieldOption.Fields.Count > 0)
                            {
                                fieldOption.Fields.RemoveAt(0); // Remove the first option
                            }
                        }

                        this.Core.Bo.ExtraInfo.SetExtraDataFields.Add(new ExtraDataConf(0, Enums.ExtraDataType.dynamic, true, field.Name, field.DisplayText, true, "radio", "", 0, 0, new string[] { Utilities.Utils.NewtonsoftSerialize(field) }));
                        break;
                    }
                }
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to reload form in SetExtraData

                //TODO: setear campos en setextradata (o select option?) (aunque sean tipo TEXTO) y luego ejecutar la llamada
                // de iinic. cobranza c codigo de barras incluyendo el service id
            }
            else
            {
                Log.Error($"Estado de boleta es diferete de pendienteo o listo. | ESTADO: {res.data.Status}");
                //cargar mensaje de error para mostrar al usuario y volver al formulario (setextradata)
                this.Core.Bo.ExtraInfo.PendingUserNotification = "language.common.msg.billFailedState";
                this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to reload form in SetExtraData
            }
        }

        internal void IsUserNotificationPending()
        {
            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.PendingUserNotification))
            {
                //call to display notif to user
                this.CallHandler(new StateEvent(StateEvent.EventType.runScript, "UserNotificationModal", this.Core.Bo.ExtraInfo.PendingUserNotification));
                this.Core.Bo.ExtraInfo.PendingUserNotification = null;//clear
            }
        }

        internal void RequestUserPhoneNumber()
        {
            Log.Trace(" RESULT -> return to SetExtraData.");
            this.SetActivityResult(StateResult.SUCCESS, this.prop.NextStateNumberB); //jump to display form in SetExtraData
        }

        internal void RemoveTopupProp()
        {
            try
            {
                if (this.Core.Bo.ExtraInfo.IsMobileTopup) this.Core.Bo.ExtraInfo.IsMobileTopup = false;
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region "API CALLS"

        private (bool success, Bill data, string message, AuthorizationStatus authStatus) InitBillPayment(long? serviceId = null, string barcode = null)
        {
            var requestData = new Dictionary<PropKey, object>();
            if (serviceId.HasValue)
                requestData.Add(PropKey.ServiceID, serviceId.Value);
            else if (barcode != null)
                requestData.Add(PropKey.Barcode, barcode);
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_INIT, null, this.prop.HostName);
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            if (authResult.authorizationStatus != AuthorizationStatus.Authorized)
            {
                return (false, null, rep.rs?.Message ?? "No se pudo iniciar la operacion", authResult.authorizationStatus);
            }
            return (rep.sc, rep.sc ? (Bill)rep.rs?.Data : null, rep.rs.Message, authResult.authorizationStatus);
        }

        private (bool success, Bill data, string message, AuthorizationStatus authStatus) ProcessBillPayment(long billId, short step, List<FieldDetail> fields) 
        {
            var requestData = new Dictionary<PropKey, object>
            {
                { PropKey.BillID, billId },
                { PropKey.CurrentOrder, step },
                { PropKey.FieldDetailList, fields },
            };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_PROCESS, null, this.prop.HostName);  //check if autorization status declined
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            if (authResult.authorizationStatus != AuthorizationStatus.Authorized)
            {
                return (false, null, rep.rs?.Message ?? "No se pudo procesar la operacion", authResult.authorizationStatus);
            }
            return (rep.sc, rep.sc ? (Bill)rep.rs.Data : null, rep.rs.Message, authResult.authorizationStatus);
        }


        private (bool success, Entities.PaymentService.Service[] data) FindService(string query)
        {
            if(string.IsNullOrWhiteSpace(query))
                return (false, null);

            var requestData = new Dictionary<PropKey, object>
            {
                { PropKey.SearchQuery, query }
            };
            this.Core.AddHostExtraData(PSConst.PAYMENT_SERVICE_DATA, requestData);
            var authResult = this.Core.AuthorizeTransaction(Enums.TransactionType.PAYMENT_FINDSERVICE, null, this.prop.HostName);
            if (authResult.authorizationStatus != AuthorizationStatus.Authorized)
            {
                return (false, null);
            }
            var rep = CastResult<Entities.PaymentService.ResponseBody>(authResult);
            return (rep.sc, rep.sc ? (Entities.PaymentService.Service[])rep.rs.Data : null);
        }

        private (bool sc, T rs) CastResult<T>(AuthorizationResult authorizationResult)
        {
            var ok = authorizationResult.authorizationStatus == AuthorizationStatus.Authorized;
            try
            {
                T data = (T)authorizationResult.Response;
                return (ok, data);
            }
            catch (InvalidCastException)
            {
                return (false, default);
            }
        }

        #endregion

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
            this.AddEventHandlers();//Coloco nuevamente los subscriptores de los eventos 
            switch (result)
            {
                case MoreTimeResult.Continue:
                    {
                        this.StartTimer(false);
                        this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    }
                case MoreTimeResult.Timeout:
                    {
                        this.SetActivityResult(StateResult.TIMEOUT, this.prop.TimeoutNextStateNumber);
                        break;
                    }
            }
        }

        /// <summary>
        /// Starts timer to control screens timeout.
        /// </summary>
        private void StartTimer(bool resetMoreTimeRetry)
        {
            Log.Debug($"/--->ResetRetry: {resetMoreTimeRetry}");
            if (this.timerScreen == null)
                timerScreen = new System.Timers.Timer();
            this.timerScreen.AutoReset = false;
            this.timerScreen.Interval = prop.MoreTime.MaxTimeOut * 1000;
            this.SubscribeMoreTime(true);
            this.timerScreen.Enabled = true;
            this.timerScreen.Start();
            this.timeout = false;
            if (resetMoreTimeRetry)
                this.moreTime.ResetRetry();
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        private void StopTimer()
        {
            Log.Debug("/--->");
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

        private void TimerScreen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timeout = true;
            this.StopTimer();
            this.RemoveEventHandlers();
            this.moreTime.StartMoreTime();
        }
        #endregion "More time"
    }
}
