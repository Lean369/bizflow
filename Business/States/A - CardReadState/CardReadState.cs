using System;
using Entities;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Business.CardReadState
{
    public class CardReadState : StateTransition
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        CardReadStateTableData_Type cardReadStateTableData; //Tabla con datos provenientes del download.
        PropertiesCardReadState prop;
        private string CardLessTxNextState = "000";
        private bool ret = false;
        private System.Timers.Timer TimerClearKeyBuffer;
        private string KeyBuffer = string.Empty;

        public CardReadState(StateTable_Type stateTable, AlephATMAppData alephATMAppData)
        {
            this.ActivityName = "CardReadState";
            this.cardReadStateTableData = (CardReadStateTableData_Type)stateTable.Item;
            this.prop = new PropertiesCardReadState(alephATMAppData);
            this.prop = this.GetProperties<PropertiesCardReadState>(out ret, this.prop);
            if (ret)
            {
                if (string.IsNullOrEmpty(this.prop.ScreenName))
                    this.prop.ScreenName = this.cardReadStateTableData.ScreenNumber;
                if (string.IsNullOrEmpty(this.prop.GoodReadNextState))
                    this.prop.GoodReadNextState = this.cardReadStateTableData.GoodReadNextState;
                if (string.IsNullOrEmpty(this.prop.ErrorScreenNumber))
                    this.prop.ErrorScreenNumber = this.cardReadStateTableData.ErrorScreenNumber;
                if (this.prop.ReadCondition1 == null)
                    this.prop.ReadCondition1 = this.cardReadStateTableData.ReadCondition1;
                if (this.prop.ReadCondition2 == null)
                    this.prop.ReadCondition2 = this.cardReadStateTableData.ReadCondition2;
                if (this.prop.ReadCondition3 == null)
                    this.prop.ReadCondition3 = this.cardReadStateTableData.ReadCondition3;
                if (this.prop.CardReturnFlag == CardReturnFlag_Type.none)
                    this.prop.CardReturnFlag = this.cardReadStateTableData.CardReturnFlag;
                if (string.IsNullOrEmpty(this.prop.NoFitMatchNextState))
                    this.prop.NoFitMatchNextState = this.cardReadStateTableData.NoFitMatchNextState;
            }
            else { Log.Error($"->Can´t get properties of Activity: {this.ActivityName}"); }
            this.PrintProperties(this.prop, stateTable.StateNumber);
        }

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
                this.Core.InitBo();                
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public override void ActivityStart()
        {
            Option_Type optionRet;
            try
            {
                Log.Debug("/--->");
                this.CurrentState = ProcessState.INPROGRESS;
                this.GetPerformaceData();
                this.TimerClearKeyBuffer = new System.Timers.Timer(5000);
                this.Core.EvtFDKscreenPress += new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                this.TimerClearKeyBuffer.Elapsed += new System.Timers.ElapsedEventHandler(this.TimerClearKeyBuffer_Elapsed);
                this.TimerClearKeyBuffer.Enabled = true;
                GlobalAppData.Instance.SetScratchpad("app-version", $"v.{this.Core.AppVersion}");
                this.EnableJournal = this.prop.Journal.EnableJournal;
                this.WriteEJ($"Next State [{this.Core.CurrentTransitionState}] {this.ActivityName}");
                if (this.Core.GetEnhParameterOption(ItemChoiceOption_Type.AANDCNextStateNumber, out optionRet) == 0)
                {
                    this.CardLessTxNextState = optionRet.Item.ToString(); //Transacción sin tarjeta.
                    Log.Info($"CardLess next state: {this.CardLessTxNextState}");
                    //Thread.Sleep(100);//Fix to blank screen
                    List<string> availableLanguagesList = new List<string>();
                    foreach (LanguageItem lang in this.Core.AlephATMAppData.LanguageList) //Chequeo JSON de idiomas disponibles.
                    {
                        if (lang.Enabled)
                        {
                            if (File.Exists($"{Const.appPath}Screens\\Languages\\{lang.Name}.json"))
                            {
                                availableLanguagesList.Add(lang.Name.ToString());
                            }
                            else
                            {
                                Log.Warn($"{lang.Name}.json was not found on the application.");
                            }
                        }
                    }
                    GlobalAppData.Instance.SetScratchpad("availableLanguages", JsonConvert.SerializeObject(availableLanguagesList));
                    GlobalAppData.Instance.SetScratchpad("selectedLanguage", this.Core.AlephATMAppData.DefaultLanguage);//Load language
                    this.CallHandler(this.prop.OnWelcome);
                    this.prop.OnWelcomeOptions.Parameters = new JObject
                    {
                        ["enableSupportQrCode"] = (JToken)this.prop.EnableSupportQrCode,
                        ["helpDeskChatURL"] = (JToken)this.prop.HelpDeskChatURL,
                        ["helpDeskPhone"] = (JToken)this.prop.HelpDeskPhone,
                        ["helpDeskEmail"] = (JToken)this.prop.HelpDeskEmail

                    };
                    this.CallHandler(this.prop.OnWelcomeOptions);
                }
                else
                    Log.Warn($"->Can´t get enh parameter option: {ItemChoiceOption_Type.AANDCNextStateNumber}");
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void TimerClearKeyBuffer_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            this.KeyBuffer = string.Empty;
        }

        private void HandlerFDKreturn(string FDKdata)
        {
            object selectedLanguage = string.Empty;
            try
            {
                Log.Info($"->FDK pressed: {FDKdata}");
                switch(FDKdata)
                {
                    case "A":
                        this.Core.InitVisit();
                        this.Core.Bo.ExtraInfo.NewTrack1 = "";
                        this.Core.Bo.ExtraInfo.NewTrack2 = this.prop.VirtualTrackData;
                        this.Core.Bo.ExtraInfo.NewTrack3 = "";
                        this.Core.Bo.ExtraInfo.Currency = this.Core.AlephATMAppData.DefaultCurrency;
                        this.WriteEJ($"START CARDLESS TRANSACTION");
                        if (GlobalAppData.Instance.GetScratchpad("selectedLanguage", out selectedLanguage))
                        {
                            this.WriteEJ($"Selected Language: {selectedLanguage}");
                            Log.Info($"Selected Language: {selectedLanguage}");
                        }
                        this.SetScreenData();
                        this.SetActivityResult(StateResult.SUCCESS, this.CardLessTxNextState); //Transacción sin tarjeta. 
                        break;
                    case "B":
                        this.KeyBuffer += FDKdata;
                        if(this.KeyBuffer.Length > 5)
                            this.Core.StartSupervisorMode(true);
                        break;
                    case "C":
                        this.KeyBuffer += FDKdata;
                        if (this.KeyBuffer.Length > 9)
                        {
                            this.Core.InitVisit();
                            this.SetActivityResult(0, this.Core.AlephATMAppData.ConfigurationState);
                        }
                        break;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void SetScreenData()
        {
            if (this.Core.Counters != null)
            {
                GlobalAppData.Instance.SetScratchpad("notes_count", this.Core.Counters.TotalDepositedNotes);
                GlobalAppData.Instance.SetScratchpad("full_threshold", this.Core.Counters.LogicalFullBinThreshold);
            }
            else
                Log.Warn("Counters is null");
            GlobalAppData.Instance.SetScratchpad("country_id", this.Core.AlephATMAppData.CountryId);
            GlobalAppData.Instance.SetScratchpad("currency", this.Core.AlephATMAppData.DefaultCurrency);
            GlobalAppData.Instance.SetScratchpad("terminal_model", this.Core.AlephATMAppData.TerminalModel.ToString());
            GlobalAppData.Instance.SetScratchpad("branding", this.Core.AlephATMAppData.Branding.ToString());
            GlobalAppData.Instance.SetScratchpad("region", this.Core.AlephATMAppData.Region);
        }

        public override void SetActivityResult(StateResult result, string nextState)
        {
            try
            {
                Log.Debug("/--->");
                this.ActivityResult = result;
                this.Quit();
                this.WriteEJ($"State result of {this.ActivityName}: {result.ToString()}");
                this.Core.SetNextState(result, nextState);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public override void Quit()
        {
            try
            {
                Log.Debug("/--->");
                this.Core.EvtFDKscreenPress -= new Core.DelegateSendFDKscreenPress(this.HandlerFDKreturn);
                if(this.TimerClearKeyBuffer != null)
                {
                    this.TimerClearKeyBuffer.Elapsed -= new System.Timers.ElapsedEventHandler(this.TimerClearKeyBuffer_Elapsed);
                    this.TimerClearKeyBuffer.Enabled = false;
                }
                this.CurrentState = ProcessState.FINALIZED;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }
    }
}
