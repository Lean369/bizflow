using Entities;
using System;

namespace Business.LoginState
{
    public class LoginState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        //private bool Phase1 = true;
        private Enums.Phases Phase = Enums.Phases.Phase_0;
        private string User = string.Empty;
        private string Password = string.Empty;
        private bool OperationByCheck = true;
        LoginStateTableData_Type LoginStateTableData; //Tabla con datos provenientes del download.
        PropertiesLoginState prop;
        bool ret = false;
        AlephHost alephHost;
        private bool MoreTimeSubscribed = false;

        #region "Constructor"
        public LoginState(StateTable_Type stateTable)
        {
            this.ActivityName = "LoginState";
            this.LoginStateTableData = (LoginStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesLoginState();
            LoginStateTableExtension1_Type extensionTable1 = null;
            this.prop = this.GetProperties<PropertiesLoginState>(out ret, this.prop);
            if (ret)
            {
                if (this.prop.Roles.Count == 0)
                {
                    this.prop.LoadDefaultConfiguration();
                    string pathFile = $"{Const.appPath}StatesSets\\Properties{this.ActivityName}.xml";
                    System.IO.File.Delete(pathFile);
                    this.GetProperties<PropertiesLoginState>(out ret, this.prop);
                }
                if (this.LoginStateTableData.Item != null)
                    extensionTable1 = (LoginStateTableExtension1_Type)this.LoginStateTableData.Item;
                if (string.IsNullOrEmpty(this.prop.ScreenNumber))
                    this.prop.ScreenNumber = this.LoginStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.GoodANextStateNumber))
                    this.prop.GoodANextStateNumber = this.LoginStateTableData.GoodANextStateNumber;
                if (string.IsNullOrEmpty(this.prop.GoodBNextStateNumber))
                    this.prop.GoodBNextStateNumber = this.LoginStateTableData.GoodBNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.GoodCNextStateNumber))
                    this.prop.GoodCNextStateNumber = this.LoginStateTableData.GoodCNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.CancelNextStateNumber))
                    this.prop.CancelNextStateNumber = this.LoginStateTableData.CancelNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.TimeOutNextStateNumber))
                    this.prop.TimeOutNextStateNumber = this.LoginStateTableData.TimeOutNextStateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.StateNumber) && extensionTable1 != null)
                    this.prop.Extension1.StateNumber = extensionTable1.StateNumber;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language1) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language1;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language2) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language2;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language3) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language3;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language4) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language4;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language5) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language5;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language6) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language6;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language7) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language7;
                if (string.IsNullOrEmpty(this.prop.Extension1.Language8) && extensionTable1 != null)
                    this.prop.Extension1.Language1 = extensionTable1.Language8;
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
                    if (this.Phase == Enums.Phases.Phase_2)
                        GlobalAppData.Instance.SetScratchpad("loadTagLogin", true);
                    else
                        GlobalAppData.Instance.SetScratchpad("loadTagLogin", false);
                    this.alephHost.Host.EvtFindUserByLogin += this.HandlerFindUserByLogin;
                    this.EnableJournal = this.prop.Journal.EnableJournal;
                    this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                    //Muestra la pantalla
                    this.CallHandler(this.prop.OnInsertUserData);
                    this.StartTimer();
                    this.Core.Bo.ExtraInfo.UserProfileMain = new UserProfile_Type();
                    if (this.prop.OperationByEnable && this.OperationByCheck)
                    {
                        GlobalAppData.Instance.SetScratchpad("operationBy", true);
                        this.OperationByCheck = false;
                    }
                    else
                        GlobalAppData.Instance.SetScratchpad("operationBy", false);
                }
                else
                {
                    Log.Error(string.Format("Host is null"));
                    this.SetActivityResult(StateResult.SWERROR, this.prop.CancelNextStateNumber);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandleOthersKeysReturn(string othersKeys)
        {
            string md5 = string.Empty;
            try
            {
                Log.Debug("Key press: {0}", othersKeys);
                this.WriteEJ(string.Format("Key press: {0}", othersKeys));
                switch (othersKeys)
                {
                    case "ENTER": //Confirma TX
                        switch (this.Phase)
                        {
                            case Enums.Phases.Phase_0:
                                Log.Info("Entry -> Phase 0");
                                //this.alephHost.Host.FindUserByLogin(this.User);
                                AlephHost hostObject = this.Core.GetHostObject(this.prop.HostName);
                                hostObject.Host.FindUserByLogin(this.User);
                                break;
                            case Enums.Phases.Phase_1: //Verifico si hay control de password
                                Log.Info("Entry -> Phase 1");
                                if (this.Core.Bo.ExtraInfo.UserProfileMain.PasswordMD5 != null)
                                {
                                    md5 = Utilities.Utils.StringToHexaMD5String(this.Password);
                                    //Usuario: 4444 Password: b6417f112bd27848533e54885b66c288 == 8113
                                    if (this.Core.Bo.ExtraInfo.UserProfileMain.PasswordMD5.Equals(md5.ToLower()))
                                    {
                                        //this.AuthorizeUser(this.Core.Bo.ExtraInfo.UserProfileMain);
                                        object operationBy = false;
                                        if (GlobalAppData.Instance.GetScratchpad("operationBy", out operationBy))
                                        {
                                            //OperationBy enable
                                            if (this.prop.OperationByEnable && bool.Parse(operationBy.ToString()) && this.Core.Bo.ExtraInfo.UserProfileMain.Role == Enums.UserRoles.SUPERVISOR)
                                            {
                                                this.ExecuteOperationBy();
                                            }
                                            else//OperationBy disable, autorizo ingreso
                                            {
                                                this.AuthorizeUser(this.Core.Bo.ExtraInfo.UserProfileMain);
                                            }
                                        }
                                        else
                                            Log.Error($"OperationBy Tag not found");
                                    }
                                    else
                                    {
                                        Log.Warn("Wrong password");
                                        this.CallHandler(this.prop.OnUserValidateError);
                                    }
                                }
                                else
                                {
                                    Log.Warn("Wrong password");
                                    this.CallHandler(this.prop.OnUserValidateError);
                                }
                                break;
                            case Enums.Phases.Phase_2: //
                                Log.Info("Entry -> Phase 2");
                                if (this.User.Equals(this.Core.Bo.ExtraInfo.UserProfileMain.User))
                                {
                                    Log.Warn("Same user");
                                    this.CallHandler(this.prop.OnUserValidateError);
                                }
                                else
                                    this.alephHost.Host.FindUserByLogin(this.User);
                                break;
                        }
                        break;
                    case "CANCEL":
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
                        break;
                    case "CLEAR":
                        this.User = string.Empty;
                        this.Password = string.Empty;
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void HandlerFindUserByLogin2(UserProfile_Type userProfileData)
        {
            if (this.prop.OperationByEnable && !this.OperationByCheck)
            {

            }

        }

        /// <summary>
        /// Maneja el retorno de los datos de login
        /// </summary>
        /// <param name="userProfileData"></param>
        private void HandlerFindUserByLogin(UserProfile_Type userProfileData)
        {
            try
            {
                Log.Debug("/--->");
                if (!userProfileData.Deactivated)
                {
                    if (userProfileData.User != null && userProfileData.UserName != null)
                    {
                        if (userProfileData.Role == Enums.UserRoles.USER || userProfileData.Role == Enums.UserRoles.SUPERVISOR || userProfileData.Role == Enums.UserRoles.ADMIN) //Sin control de password
                        {
                            this.Core.Bo.ExtraInfo.UserProfileMain.User = userProfileData.User;
                            this.Core.Bo.ExtraInfo.UserProfileMain.UserName = userProfileData.UserName;
                            this.Core.Bo.ExtraInfo.UserProfileMain.Role = userProfileData.Role;
                            this.Core.Bo.ExtraInfo.UserProfileMain.PasswordMD5 = userProfileData.PasswordMD5;
                            this.Core.AddHostExtraData("userProfile", userProfileData);
                            if (this.Phase == Enums.Phases.Phase_0)
                            {
                                Log.Info("Entry -> Phase 0");
                                RoleAttributes roleAttributes = this.prop.Roles.Find(x => x.Role == userProfileData.Role);
                                if (roleAttributes != null)
                                {
                                    if (roleAttributes.CheckPassword)
                                    {
                                        this.CallHandler(this.prop.OnVisiblePassword);
                                        this.Phase = Enums.Phases.Phase_1;
                                        Log.Info("Change -> Phase 1");
                                    }
                                    else
                                    {
                                        object operationBy = false;
                                        if (GlobalAppData.Instance.GetScratchpad("operationBy", out operationBy))
                                        {
                                            //OperationBy enable
                                            if (this.prop.OperationByEnable && bool.Parse(operationBy.ToString()) && userProfileData.Role == Enums.UserRoles.SUPERVISOR)
                                            {
                                                this.ExecuteOperationBy();
                                            }
                                            else//OperationBy disable, autorizo ingreso
                                            {
                                                this.AuthorizeUser(userProfileData);
                                            }
                                        }
                                        else
                                            Log.Error($"OperationBy Tag not found");
                                    }
                                }
                                else
                                {
                                    Log.Warn("Null role");
                                    this.CallHandler(this.prop.OnUserValidateError);
                                }
                            }
                            else if (this.Phase == Enums.Phases.Phase_2)
                            {
                                Log.Info("Entry -> Phase 2");
                                RoleAttributes roleAttributes = this.prop.Roles.Find(x => x.Role == userProfileData.Role);
                                if (roleAttributes != null)
                                {
                                    if (roleAttributes.Role == Enums.UserRoles.USER)
                                    {
                                        this.AuthorizeUser(userProfileData);
                                    }
                                    else
                                    {
                                        Log.Warn("Improper role");
                                        this.CallHandler(this.prop.OnUserValidateError);
                                    }
                                }
                                else
                                {
                                    Log.Warn("Null role");
                                    this.CallHandler(this.prop.OnUserValidateError);
                                }
                            }
                        }
                        else
                        {
                            Log.Warn("Unknow role");
                            this.CallHandler(this.prop.OnUserValidateError);
                        }
                    }
                    else
                    {
                        Log.Warn("User or UserName is null");
                        this.CallHandler(this.prop.OnUserValidateError);
                    }
                }
                else
                {
                    Log.Warn("Inactive user");
                    this.CallHandler(this.prop.OnUserValidateError);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void ExecuteOperationBy()
        {
            Log.Info("Change -> Phase 2");
            this.Phase = Enums.Phases.Phase_2;
            this.Core.Bo.ExtraInfo.UserProfileAlt = this.Core.Bo.ExtraInfo.UserProfileMain;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.RemoveEventHandlers();
            this.StopTimer();
            this.ActivityStart();
        }

        private void AuthorizeUser(UserProfile_Type userProfileData)
        {
            Log.Debug("/--->");
            this.Core.Bo.ExtraInfo.UserProfileMain.Password = string.Empty;
            string msg = $"Login: {this.Core.Bo.ExtraInfo.UserProfileMain.UserName} ({this.Core.Bo.ExtraInfo.UserProfileMain.User}) - Role: {this.Core.Bo.ExtraInfo.UserProfileMain.Role}";
            this.WriteEJ(msg);
            Log.Info(msg);
            this.SetScreenData(userProfileData);
            switch (userProfileData.Role)
            {
                case Enums.UserRoles.USER:
                    this.SetActivityResult(0, this.prop.GoodANextStateNumber);
                    break;
                case Enums.UserRoles.SUPERVISOR:
                    this.SetActivityResult(0, this.prop.GoodBNextStateNumber);
                    break;
                case Enums.UserRoles.ADMIN:
                    this.SetActivityResult(0, this.prop.GoodCNextStateNumber);
                    break;
            }
        }


        private void HandlerInputData(string keyCode, string dataLink)
        {
            try
            {
                this.ResetTimer();
                switch (dataLink)
                {
                    case "dlTextBox1Field"://Valores recibidos a partir de las teclas presionadas por el usuario en componentes InputBox 1
                        {
                            this.User += keyCode;
                            break;
                        }
                    case "dlTextBox2Field": //Valores recibidos a partir de las teclas presionadas por el usuario en componentes InputBox 2
                        {
                            if (!string.IsNullOrEmpty(keyCode))
                                this.Password += keyCode;
                            break;
                        }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetScreenData(UserProfile_Type userProfile)
        {
            GlobalAppData.Instance.SetScratchpad("user_role", userProfile.Role);
            GlobalAppData.Instance.SetScratchpad("user_name", userProfile.UserName);
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.Phase = Enums.Phases.Phase_0;
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
                this.RemoveEventHandlers();
                this.User = string.Empty;
                this.Password = string.Empty;
                this.OperationByCheck = true;
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void RemoveEventHandlers()
        {
            this.Core.EvtOthersKeysPress -= new Core.DelegateSendEvtOthersKeysPress(this.HandleOthersKeysReturn);
            this.Core.EvtInputData -= new Core.DelegateSendInputData(this.HandlerInputData);
            this.alephHost.Host.EvtFindUserByLogin -= HandlerFindUserByLogin;
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
                        //this.StartTimer();
                        //this.Core.HideScreenModals(); //Quito los avisos de pantalla
                        break;
                    }
                case MoreTimeResult.Cancel:
                    {
                        this.SetActivityResult(StateResult.CANCEL, this.prop.CancelNextStateNumber);
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
            this.RemoveEventHandlers();
            this.User = string.Empty;
            this.Password = string.Empty;
            this.OperationByCheck = true;
            this.Phase = Enums.Phases.Phase_0;
            this.moreTime.StartMoreTime();
        }

        #endregion "More time"

    }
}
