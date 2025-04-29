using Entities;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;
using static Entities.Const;

namespace Business.Printers
{
    public class PrintFormat
    {
        private static readonly NLog.Logger Log = LogManager.GetLogger("LOG");

        private List<PrinterTemplate> ListOfPrinterTemplate;

        private IDictionary<string, JToken> TicketsValues;

        private Core Core;

        public PrintFormat(Core core)
        {
            try
            {
                Core = core;
            }
            catch (Exception value) { Log.Fatal(value); }
        }

        public string GetTicketData(string ticketTempalte)
        {
            string result = string.Empty;
            PrintConverter printConverter = new PrintConverter();
            try
            {
                NDCTicketData_Type nDCTicketData = GetNDCTicketData(ticketTempalte);
                if (nDCTicketData != null)
                {
                    result = printConverter.GetCommands(nDCTicketData.PrinterData, isPrePrintedData: false, isStatement: false);
                }
                else
                {
                    Log.Error($"Printer template: {ticketTempalte} doesn\u00b4t exist");
                }
            }
            catch (Exception value) { Log.Fatal(value); }
            return result;
        }

        private NDCTicketData_Type GetNDCTicketData(string templateName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            NDCTicketData_Type nDCTicketData_Type = null;
            object language = string.Empty;
            try
            {
                Log.Debug($"/--->Template Name: {templateName}");
                if (PrinterTemplate.GetPrinterTemplate(out this.ListOfPrinterTemplate, this.Core.AlephATMAppData.PrinterTemplateFileName))
                {
                    PrinterTemplate printerTemplate = this.ListOfPrinterTemplate.Find((PrinterTemplate x) => x.TemplateType.Equals(templateName));
                    if (printerTemplate != null)
                    {
                        if (!GlobalAppData.Instance.GetScratchpad("selectedLanguage", out language))
                            language = this.Core.AlephATMAppData.DefaultLanguage;
                        XmlDocument xmlDocument = new XmlDocument();
                        string path = $"{Const.appPath}Screens\\Languages\\{language}.json";
                        if (File.Exists(path))
                        {
                            using (StreamReader streamReader = new StreamReader(path))
                            {
                                string json = streamReader.ReadToEnd();
                                IDictionary<string, JToken> source = JObject.Parse(json);
                                List<KeyValuePair<string, JToken>> list = (from x in source
                                                                           where x.Key == "tickets"
                                                                           select x).ToList();
                                TicketsValues = (IDictionary<string, JToken>)list[0].Value;
                                Log.Info($"{language}.json language file - loaded successfully.");
                            }
                        }
                        else
                        {
                            Log.Error($"{language}.json language file - Not found.");
                        }
                        foreach (string line in printerTemplate.Lines)
                        {
                            stringBuilder.Append(GetTransactionData(line));
                        }
                        Parser parser = new Parser();
                        nDCTicketData_Type = new NDCTicketData_Type();
                        nDCTicketData_Type.PrinterData = parser.GetPrinterData(stringBuilder.ToString());
                    }
                    else
                    {
                        Log.Error("Selected language - Not found.");
                    }
                }
                else
                {
                    Log.Error("Can't get ticket data.");
                }
            }
            catch (Exception value2) { Log.Fatal(value2); }
            return nDCTicketData_Type;
        }

        private string GetLanguageValue(string key)
        {
            string result = string.Empty;
            try
            {
                result = (string)TicketsValues[key];
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            return result;
        }

        private string GetTransactionData(string line)
        {
            string empty = string.Empty;
            string text = string.Empty;
            try
            {
                line = (line.Contains("escDATE") ? Utils.GetDateData(line) : line);
                line = (line.Contains("escTIME") ? Utils.GetTimeData(line) : line);
                line = line.Replace("escLINE", $"{'\n'}");
                line = line.Replace("escBOLD", $"{'\v'}");
                line = line.Replace("escFILL", $"{'\u000e'}");
                line = line.Replace("escBUFF", $"{'\u001b'}B");
                line = line.Replace("escSETP", $"{'\u001d'}");
                line = line.Replace("escCUTS", $"{'\u001c'}");
                line = line.Replace("lngDATE", GetLanguageValue("date"));
                line = line.Replace("lngTIME", GetLanguageValue("time"));
                line = line.Replace("lngTSN1", GetLanguageValue("secuence"));
                line = line.Replace("lngTXEN", GetLanguageValue("envelopeTxnName"));
                line = line.Replace("lngTXCA", GetLanguageValue("cashDepositTxnName"));
                line = line.Replace("lngTXSH", GetLanguageValue("shipoutTxnName"));
                line = line.Replace("lngTXLC", GetLanguageValue("localCountersTxnName"));
                line = line.Replace("lngINTX", GetLanguageValue("incompleteTxn"));
                line = line.Replace("lngRCTX", GetLanguageValue("retrievalTxn"));
                line = line.Replace("lngLUNO", GetLanguageValue("machineID").PadRight(8, ' '));
                line = line.Replace("lngBATC", GetLanguageValue("batch").PadRight(8, ' '));
                line = line.Replace("lngUSER", GetLanguageValue("userName").PadRight(8, ' '));
                line = GetCourtesyLines(line);
                //line = line.Replace("lngCOL1", GetLanguageValue("courtesyLine1"));
                //line = line.Replace("lngCOL2", GetLanguageValue("courtesyLine2"));
                //line = line.Replace("lngCOL3", GetLanguageValue("courtesyLine3"));
                line = line.Replace("lngMSG1", GetLanguageValue("messageToUser1"));
                line = line.Replace("lngMSG2", GetLanguageValue("messageToUser2"));
                line = line.Replace("lngNOVA", GetLanguageValue("noteValue"));
                line = line.Replace("lngNOQU", GetLanguageValue("noteQuantity"));
                line = line.Replace("lngNOTO", GetLanguageValue("noteTotal"));
                line = line.Replace("lngDPDE", GetLanguageValue("depositDeclinedTxn"));
                line = line.Replace("lngTXNC", GetLanguageValue("transactionCompleted"));
                line = line.Replace("lngDPD1", GetLanguageValue("declinedPayment1"));
                line = line.Replace("lngDPD2", GetLanguageValue("declinedPayment2"));
                line = line.Replace("lngACHL", GetLanguageValue("acHolder"));
                line = line.Replace("escBATC", Core.Counters.GetBATCH().ToString("0000"));
                line = line.Replace("escCTRL", Core.Counters.GetCLOSE().ToString("0000"));
                line = line.Replace("escLUNO", Core.TerminalInfo.LogicalUnitNumber);
                line = line.Replace("escADDR", Core.TerminalInfo.Address);
                line = line.Replace("escCITY", Core.TerminalInfo.City);
                line = line.Replace("escPHON", Core.TerminalInfo.Phone);
                //Agrega espacios en blanco
                if (line.Contains("escBLNK"))
                    line = line.Replace("escBLNK", this.AddBlanks(line));
                //Collection ID - Print Barcode 
                if (this.Core.Counters.GetCOLLECTIONID() != null)
                {
                    if (line.Contains("escBARN"))
                        line = line.Replace("escBARN", $"{'\u001b'}{'k'}{this.Core.Counters.GetCOLLECTIONID().Length}N{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escBARD"))
                        line = line.Replace("escBARD", $"{'\u001b'}{'k'}{this.Core.Counters.GetCOLLECTIONID().Length}D{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escBARC"))
                        line = line.Replace("escBARC", $"{'\u001b'}{'k'}{this.Core.Counters.GetCOLLECTIONID().Length}C{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escBARK"))
                        line = line.Replace("escBARK", $"{'\u001b'}{'k'}{this.Core.Counters.GetCOLLECTIONID().Length}K{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escCIDN"))
                        line = line.Replace("escCIDN", $"N{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escCIDD"))
                        line = line.Replace("escCIDD", $"D{this.Core.Counters.GetCOLLECTIONID()}");
                    if (line.Contains("escCOID"))
                        line = line.Replace("escCOID", this.Core.Bo.ExtraInfo.CollectionID);
                }
                //Transaction DATA
                line = GetNumOperation(line);
                //if (line.Contains("escTSN1"))
                //    line = line.Replace("escTSN1", this.Core.Counters.GetTSN().ToString("0000"));
                if (line.Contains("lngDPER") && this.Core.Bo.ExtraInfo.CashInMultiCashData != null)
                    line = line.Replace("lngDPER", this.GetDepositErrorInfo());//E8
                if (line.Contains("lngDPEER") && this.Core.Bo.ExtraInfo.CashInMultiCashData != null)
                    line = line.Replace("lngDPEER", this.GetDepositEndErrorInfo());
                if (this.Core.Bo.ExtraInfo != null)
                    if (line.Contains("escBDIN") && this.Core.Bo.ExtraInfo.BagDropInfo != null)
                        line = line.Replace("escBDIN", this.GetBagDropDataInfo());
                if (this.Core.Bo.ExtraInfo != null)
                    if (line.Contains("escMCAS") && this.Core.Bo.ExtraInfo.CashInMultiCashData != null)
                        line = line.Replace("escMCAS", this.GetMulticashDataInfo());
                if (this.Core.Bo.ExtraInfo != null)
                    if (line.Contains("escMCA2") && this.Core.Bo.ExtraInfo.CashInMultiCashData != null)
                        line = line.Replace("escMCA2", this.GetMulticashDataInfo2());
                if (line.Contains("escSPOU") && this.Core.Counters != null)
                    line = line.Replace("escSPOU", this.GetShipOutDataInfo());
                if (line.Contains("escSPNO") && this.Core.Counters != null)
                    line = line.Replace("escSPNO", this.GetShipOutNotesDataInfo());
                if (line.Contains("escSPBA") && this.Core.Counters != null)
                    line = line.Replace("escSPBA", this.GetShipOutBagsDataInfo());
                if (line.Contains("escLCOU"))
                    line = line.Replace("escLCOU", this.GetLocalCountersInfo());
                if (line.Contains("escCURR") && !string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.Currency))
                    line = line.Replace("escCURR", this.Core.Bo.ExtraInfo.Currency);
                if (this.Core.Bo != null && this.Core.Bo.ExtraInfo != null)
                {
                    //Print QR data                   
                    if (line.Contains("escPRQR"))
                        if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.QRdata))
                        {
                            line = line.Replace("escPRQR", $"{'\u001b'}{'l'}{this.Core.Bo.ExtraInfo.QRdata.Length.ToString("000")}{this.Core.Bo.ExtraInfo.QRdata}");
                            if (line.Contains("lngMSG5"))
                                line = line.Replace("lngMSG5", GetLanguageValue("messageToUser5"));
                        }
                        else
                            line = line.Replace("escPRQR", String.Empty).Replace("lngMSG5", String.Empty);
                    //Monto de la operación
                    if (line.Contains("escAMO1"))
                    {
                        int decimalPlaces = this.Core.AlephATMAppData.CurrencyFormatDecimals;
                        if (this.Core.Bo.ExtraInfo.Amount != 0)
                            line = line.Replace("escAMO1", Utils.FormatCurrency(this.Core.Bo.ExtraInfo.Amount, this.Core.Bo.ExtraInfo.Currency, 12, decimalPlaces));
                        else
                            line = line.Replace("escAMO1", Utils.FormatCurrency(0, "", 12, decimalPlaces));
                    }
                    //Barcodes del shipout
                    if (line.Contains("escBARC"))
                    {
                        line = (string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.BarcodeASCII) ? line.Replace("escBARC", string.Empty) : line.Replace("escBARC", this.Core.Bo.ExtraInfo.BarcodeASCII));
                    }
                    //Usuario principal
                    if (this.Core.Bo.ExtraInfo.UserProfileMain != null)
                    {
                        if (line.Contains("escUSER"))
                            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.UserProfileMain.User))
                                line = line.Replace("escUSER", this.Core.Bo.ExtraInfo.UserProfileMain.User);
                            else
                                line = line.Replace("escUSER", String.Empty);
                        if (line.Contains("escUSNA"))
                            if (!string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.UserProfileMain.UserName))
                                line = line.Replace("escUSNA", this.Core.Bo.ExtraInfo.UserProfileMain.UserName);
                            else
                                line = line.Replace("escUSNA", String.Empty);
                    }
                    else
                    {
                        if (line.Contains("escUSER"))
                            line = line.Replace("escUSER", String.Empty);
                        if (line.Contains("escUSNA"))
                            line = line.Replace("escUSNA", String.Empty);
                    }
                    //Usuario de Shipout
                    if (this.Core.Sdo.ShipoutUser != null)
                    {
                        if (line.Contains("escUSES") && !string.IsNullOrEmpty(this.Core.Sdo.ShipoutUser.User))
                            line = line.Replace("escUSES", this.Core.Sdo.ShipoutUser.User);
                        else
                            line = line.Replace("escUSES", String.Empty);
                        if (line.Contains("escUSNS") && !string.IsNullOrEmpty(this.Core.Sdo.ShipoutUser.UserName))
                            line = line.Replace("escUSNS", this.Core.Sdo.ShipoutUser.UserName);
                        else
                            line = line.Replace("escUSNS", String.Empty);
                    }
                    else
                    {
                        if (line.Contains("escUSES"))
                            line = line.Replace("escUSES", String.Empty);
                        if (line.Contains("escUSNS"))
                            line = line.Replace("escUSNS", String.Empty);
                    }
                    //Usuario alternativo
                    if (this.Core.Bo.ExtraInfo.UserProfileAlt != null)
                    {
                        if (line.Contains("escALUS") && !string.IsNullOrEmpty(this.Core.Bo.ExtraInfo.UserProfileAlt.User))
                            line = line.Replace("escALUS", $"{this.Core.Bo.ExtraInfo.UserProfileAlt.User}{'\n'}{' '}({this.Core.Bo.ExtraInfo.UserProfileAlt.UserName})");
                    }
                    else
                    {
                        if (line.Contains("escALUS"))
                            line = line.Replace("escALUS", String.Empty);
                    }
                    //Extra data
                    if (line.Contains("escEXDA"))
                        line = line.Replace("escEXDA", this.GetExtraData());
                    //Account holder
                    if (line.Contains("escACHL"))
                        line = line.Replace("escACHL", this.SplitStringByWidth(this.Core.Bo.ExtraInfo.AccountHolder, 26));
                    //Holder document
                    if (line.Contains("escHLDC"))
                    {

                        var depositorDocNo = this.Core.Bo.ExtraInfo.HostExtraData.FirstOrDefault(x => x.Key == "depositorDocNo");
                        var depositorDocType = this.Core.Bo.ExtraInfo.HostExtraData.FirstOrDefault(x => x.Key == "depositorDocType");
                        if (depositorDocNo.Value != null && depositorDocType.Value != null)
                        {
                            line = line.Replace("escHLDC", depositorDocType.Value.ToString().ToUpper() + " " + depositorDocNo.Value.ToString());
                        }
                    }
                    //ticket data received from host
                    if (line.Contains("escPYDA"))
                        line = line.Replace("escPYDA", this.GetPaymentData());
                    //transaction amounts
                    if (line.Contains("escPYAM"))
                        line = line.Replace("escPYAM", this.GetPaymentAmounts());
                    //error in case something went wrong
                    if (line.Contains("escERROR"))
                        line = line.Replace("escERROR", GetErrorData());
                }
            }
            catch (Exception value) { Log.Fatal(value); }
            return line;
        }

        private string AddBlanks(string line)
        {
            string text = line.Replace("escBLNK", "☼escBLNK");
            string[] array = text.Split('☼');
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("escBLNK"))
                {
                    int num = array[i].IndexOf("escBLNK");
                    int result = 0;
                    if (int.TryParse(array[i].Substring(num + 7, 3), out result))
                    {
                        array[i] = array[i].Remove(num + 7, 3);
                        array[i] = array[i].Replace("escBLNK", GenerateString(result, " "));
                    }
                    else
                        Log.Error("Size isn\u00b4t numeric in escBLNK.");
                }
                text = string.Join("", array);
            }
            return text;
        }

        private string GetExtraData()
        {
            string text = string.Empty;
            if (this.Core.Bo.ExtraInfo.ExtraData != null)
                if (this.Core.Bo.ExtraInfo.ExtraData.Count != 0)
                {
                    foreach (ExtraData extraDatum in this.Core.Bo.ExtraInfo.ExtraData)
                    {
                        switch (extraDatum.ExtraDataType)
                        {
                            case Enums.ExtraDataType.channel:
                                text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue("channel").PadRight(8, ' '), extraDatum.TagValue);
                                break;
                            case Enums.ExtraDataType.shifts:
                                text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue("shifts").PadRight(8, ' '), extraDatum.TagValue);
                                break;
                            case Enums.ExtraDataType.txInfo:
                                text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue("transactionInfo").PadRight(11, ' '), extraDatum.TagValue);
                                break;
                            case Enums.ExtraDataType.txRef:
                                text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue("transactionRef").PadRight(11, ' '), extraDatum.TagValue);
                                break;
                            case Enums.ExtraDataType.dynamic:
                                if (extraDatum.TagName.Equals("keyless"))
                                    text = string.Format("{0}{1}{2}", text, '\n', extraDatum.TagValue);
                                else if (this.Core.AlephATMAppData.Branding == Enums.Branding.RedPagosA)
                                    text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue(extraDatum.TagName).PadRight(8, ' '), extraDatum.TagValue);
                                break;
                            default:
                                text = string.Empty;
                                break;
                        }
                    }
                }
            return text;
        }

        private string GetPaymentData()
        {
            string text = string.Empty;
            if (this.Core.Bo.ExtraInfo.PaymentData != null)
                if (this.Core.Bo.ExtraInfo.PaymentData.Count != 0)
                {
                    foreach (ExtraData extraDatum in this.Core.Bo.ExtraInfo.PaymentData)
                    {
                        switch (extraDatum.ExtraDataType)
                        {
                            case Enums.ExtraDataType.dynamic:
                                if (extraDatum.TagName.Equals("keyless"))
                                    text = string.Format("{0}{1}{2}", text, '\n', extraDatum.TagValue);
                                else
                                    text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue(extraDatum.TagName).PadRight(8, ' '), extraDatum.TagValue);
                                break;
                            default:
                                text = string.Empty;
                                break;
                        }
                    }
                }
            return text;
        }

        /// <summary>
        /// Get courtesy lines from host extra data or from language file
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetCourtesyLines(string line)
        {
            if (this.Core.Bo.ExtraInfo.HostExtraData != null && this.Core.Bo.ExtraInfo.HostExtraData.Any())
            {
                foreach (KeyValuePair<string, object> keyValuePair in this.Core.Bo.ExtraInfo.HostExtraData)
                {
                    if (keyValuePair.Key.Equals(HostExtraDataKeys.HostMessage1))
                    {
                        line = line.Replace("lngCOL1", keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key.Equals(HostExtraDataKeys.HostMessage2))
                    {
                        line = line.Replace("lngCOL2", keyValuePair.Value.ToString());
                    }
                    else if (keyValuePair.Key.Equals(HostExtraDataKeys.HostMessage3))
                    {
                        line = line.Replace("lngCOL3", keyValuePair.Value.ToString());
                    }
                }
            }

            line = line.Replace("lngCOL1", GetLanguageValue("courtesyLine1"));
            line = line.Replace("lngCOL2", GetLanguageValue("courtesyLine2"));
            line = line.Replace("lngCOL3", GetLanguageValue("courtesyLine3"));

            return line;
        }

        /// <summary>
        /// Get number of operation from host extra data or from counters
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetNumOperation(string line)
        {
            if (this.Core.Bo.ExtraInfo.HostExtraData != null && this.Core.Bo.ExtraInfo.HostExtraData.Any())
            {
                foreach (KeyValuePair<string, object> keyValuePair in this.Core.Bo.ExtraInfo.HostExtraData)
                {
                    if (keyValuePair.Key.Equals(HostExtraDataKeys.NumOperation))
                    {
                        line = line.Replace("escTSN1", keyValuePair.Value.ToString());
                    }
                }
            }

            line = line.Replace("escTSN1", this.Core.Counters.GetTSN().ToString("0000"));

            return line;
        }

        private string GetPaymentAmounts()
        {
            string text = string.Empty;
            if (this.Core.Bo.ExtraInfo.PaymentAmounts != null)
                if (this.Core.Bo.ExtraInfo.PaymentAmounts.Count != 0)
                {
                    foreach (ExtraData extraDatum in this.Core.Bo.ExtraInfo.PaymentAmounts)
                    {
                        switch (extraDatum.ExtraDataType)
                        {
                            case Enums.ExtraDataType.dynamic:
                                if (extraDatum.TagName.Equals("keyless"))
                                    text = string.Format("{0}{1}{2}", text, '\n', extraDatum.TagValue);
                                else
                                    text = string.Format("{0}{1}{2}: {3}", text, '\n', GetLanguageValue(extraDatum.TagName).PadRight(8, ' '), extraDatum.TagValue);
                                break;
                            default:
                                text = string.Empty;
                                break;
                        }
                    }
                }
            return text;
        }

        private string GetErrorData()
        {
            string text = string.Empty;
            if (this.Core.Bo.ExtraInfo.ErrorCode != null)
                text = $"{'\u000e'} ERROR {(int)this.Core.Bo.ExtraInfo.ErrorCode.Code} {'\u000e'}\n{this.Core.Bo.ExtraInfo.ErrorCode.Message}";
            return text;
        }

        /// <summary>
        /// Operación con error de módulo de depósito E8
        /// </summary>
        /// <returns></returns>
        private string GetDepositErrorInfo()
        {
            string result = string.Empty;
            if (this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError)
            {
                result = string.Format("{0}{0}{1}", '\n', GetLanguageValue("depositErrorTxn"));
            }
            return result;
        }

        private string GetDepositEndErrorInfo()
        {
            string result = string.Empty;
            if (this.Core.Bo.ExtraInfo.CashInMultiCashData.DepositHardwareError)
            {
                result = string.Format("{0}{0}{1}", '\n', GetLanguageValue("depositEndErrorTxn"));
            }
            return result;
        }

        private string GenerateString(int num, string pattern)
        {
            string text = "";
            for (int i = 0; i < num; i++)
            {
                text += pattern;
            }
            return text;
        }

        private string GetLocalCountersInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            List<Bills> list = new List<Bills>();
            object value = null;
            bool existDeposit = false;
            bool ret = false;
            long totalNotes = 0L;
            try
            {
                if (GlobalAppData.Instance.GetScratchpad("localCounters", out value))
                {
                    CashInInfo cashInInfo = Utils.JsonDeserialize<CashInInfo>(out ret, value.ToString());
                    if (ret)
                    {
                        list = (from x in cashInInfo.Bills
                                orderby x.Value
                                orderby x.Currency
                                select x).ToList();
                        list.ForEach(delegate (Bills item)
                        {
                            decimal d = item.Value * item.Quantity;
                            //d *= 100m;
                            if (d > decimal.Zero)
                            {
                                totalNotes += item.Quantity;
                                string text = Utils.FormatCurrency(d, item.Currency, 12);
                                sb2.Append($"{item.Currency} {item.Value.ToString().PadLeft(6, ' ')}  {item.Quantity.ToString().PadLeft(5, ' ')} {text}{'\n'}");
                                existDeposit = true;
                            }
                        });
                        stringBuilder.Append(sb2);
                        stringBuilder.AppendLine(string.Format("{0}{1}: {2}", '\n', GetLanguageValue("depositQuantity"), totalNotes));
                    }
                    if (!existDeposit)
                    {
                        stringBuilder.AppendLine(GetLanguageValue("messageToUser3"));
                    }
                }
                else
                {
                    Log.Error("Unexpected print data");
                }
            }
            catch (Exception value2)
            {
                Log.Fatal(value2);
            }
            return stringBuilder.ToString();
        }

        private string GetShipOutNotesDataInfo()
        {
            List<Bills> list = new List<Bills>();
            List<Item> list2 = new List<Item>();
            List<Item> list3 = new List<Item>();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder stringBuilder = new StringBuilder();
            decimal total = default(decimal);
            bool existDeposit = false;
            try
            {
                if (Core.Counters.Contents.LstDetail.Count > 0)
                {
                    IEnumerable<string> source = from i in Core.Counters.Contents.LstDetail
                                                 where i.ContainerId == Detail.ContainerIDType.CashAcceptor
                                                 select i.Currency;
                    List<string> list4 = source.ToList();
                    if (list4.Count > 0)
                    {
                        foreach (string curr in list4)
                        {
                            total = default(decimal);
                            sb2 = new StringBuilder();
                            Detail detail = Core.Counters.Contents.LstDetail.Find((Detail x) => x.ContainerId == Detail.ContainerIDType.CashAcceptor && x.Currency.Equals(curr));
                            IEnumerable<Item> source2 = from i in detail.LstItems
                                                        group i by i.Denomination into @group
                                                        select new
                                                        {
                                                            Key = @group.Key,
                                                            Items = from x in @group
                                                                    orderby x.Denomination descending
                                                                    select x
                                                        } into g
                                                        select g.Items.First();
                            list2 = source2.ToList();
                            list3 = (from x in list2
                                     orderby x.Denomination
                                     select x).ToList();
                            list3.ForEach(delegate (Item item)
                            {
                                decimal num = (item.Denomination * item.Num_Items) / 100;
                                total += num;
                                if (num > decimal.Zero)
                                {
                                    string text = Utils.FormatCurrency(num, curr, 12);
                                    sb2.Append($"{curr} {(item.Denomination / 100).ToString().PadLeft(6, ' ')}  {item.Num_Items.ToString().PadLeft(5, ' ')} {text}{'\n'}");
                                    existDeposit = true;
                                }
                            });
                            if (total > decimal.Zero)
                            {
                                stringBuilder.Append(sb2);
                                stringBuilder.Append($"{'\n'}{this.GetLanguageValue("depositTotal")} {curr}:        {Utils.FormatCurrency(total, curr, 12)}");
                                stringBuilder.AppendLine($"{'\n'}-----------------------------------");
                            }
                        }
                        stringBuilder.AppendLine($"{GetLanguageValue("depositQuantity")}: {Core.Counters.TotalDepositedNotes}");
                    }
                    else
                    {
                        stringBuilder.AppendLine(string.Format("{0}: 0", GetLanguageValue("depositQuantity")));
                    }
                    if (!existDeposit)
                    {
                        stringBuilder.AppendLine(GetLanguageValue("messageToUser3"));
                    }
                }
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            return stringBuilder.ToString();
        }

        private string GetShipOutBagsDataInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                IEnumerable<Detail> source = from i in Core.Counters.Contents.LstDetail
                                             where i.ContainerId == Detail.ContainerIDType.Depository
                                             select i;
                List<Detail> list = source.ToList();
                if (list.Count > 0)
                {
                    sb.AppendLine($"{'\n'}{GetLanguageValue("messageToUser4")}: ");
                    foreach (Detail detail in list)
                    {
                        foreach (Item item in detail.LstItems)
                        {
                            string amount = Utils.FormatCurrency(item.Total / 100, detail.Currency, 0);
                            sb.AppendLine($"{GetLanguageValue("envelopeType").PadRight(12, ' ')}: {item.Category}{'\n'}{GetLanguageValue("envelopeAmount").PadRight(12, ' ')}: {detail.Currency} {amount}{'\n'}{GetLanguageValue("envelopeBarcode").PadRight(12, ' ')}: {item.Barcode}");
                        }
                    }
                    sb.AppendLine($"{'\n'}{GetLanguageValue("envelopeQuantity")}: {list.Count}");
                }
                else
                    sb.AppendLine(GetLanguageValue("messageToUser3"));
            }
            catch (Exception value) { Log.Fatal(value); }
            return sb.ToString();
        }

        /// <summary>
        /// Get Shipout data of validated notes a
        /// </summary>
        /// <returns></returns>
        private string GetShipOutDataInfo()
        {
            return $"{GetShipOutNotesDataInfo()}{'\n'}{GetShipOutBagsDataInfo()}";
        }

        private string GetBagDropDataInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                if (this.Core.Bo.ExtraInfo.BagDropInfo.baglist.Count > 0)
                {
                    foreach (BagDrop item in Core.Bo.ExtraInfo.BagDropInfo.baglist)
                    {
                        stringBuilder.AppendLine($"{GetLanguageValue("envelopeType").PadRight(12, ' ')}: {item.type}{'\n'}{GetLanguageValue("envelopeAmount").PadRight(12, ' ')}: {item.currency} {item.amount.Substring(2)}{'\n'}{GetLanguageValue("envelopeBarcode").PadRight(12, ' ')}: {item.barcode}");
                    }
                }
            }
            catch (Exception value) { Log.Fatal(value); }
            return stringBuilder.ToString();
        }

        private string GetMulticashDataInfo()
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            StringBuilder sb = new StringBuilder();
            long count = 0L;
            try
            {
                if (this.Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count > 0 && this.Core.Bo.ExtraInfo.CashInMultiCashData.TotalizedDeposit.total.Count > 0)
                {
                    List<Bills> bills = new List<Bills>();
                    List<Bills> list = new List<Bills>();
                    List<Bills> list2 = new List<Bills>();
                    CashInMultiCashData cashInMultiCashData = this.Core.Bo.ExtraInfo.CashInMultiCashData;

                    cashInMultiCashData.ListCashInInfo.ForEach(delegate (CashInInfo cashInInfo)
                    {
                        bills.AddRange(cashInInfo.Bills);
                        cashInInfo.Bills.ForEach(delegate (Bills b) { currencies.Add(b.Currency); });//Cargo todos los currencies
                    });
                    distinctCurrencies = currencies.Distinct().ToArray();
                    for (int k = 0; k < distinctCurrencies.Length; k++)
                    {
                        //Agrupa por divisa y ordena en forma descendente por valor
                        IEnumerable<Bills> source = from i in bills
                                                    where i.Currency.Equals(distinctCurrencies[k])
                                                    group i by i.Value into @group
                                                    select new
                                                    {
                                                        Key = @group.Key,
                                                        Items = from x in @group
                                                                orderby x.Value descending
                                                                select x
                                                    } into g
                                                    select g.Items.First();
                        list = source.ToList();
                        list2 = (from x in list
                                 orderby x.Value
                                 select x).ToList();
                        list2.ForEach(delegate (Bills bill)
                        {
                            count += bill.Quantity;
                            string text = Utils.FormatCurrency(((decimal)(bill.Value * bill.Quantity)), bill.Currency, 12, this.Core.AlephATMAppData.CurrencyFormatDecimals);
                            sb.Append($"{bill.Currency} {bill.Value.ToString().PadLeft(6, ' ')}  {bill.Quantity.ToString().PadLeft(5, ' ')}   {text}{'\n'}");
                        });
                        Values item = cashInMultiCashData.TotalizedDeposit.total.Find(e => e.currency.Equals(distinctCurrencies[k]));
                        sb.Append($"{'\n'}{GetLanguageValue("depositTotal")} {item.currency}:          {Utils.FormatCurrency(ConvertCurrencyToDecimal(item.amount, item.currency), item.currency, 12, this.Core.AlephATMAppData.CurrencyFormatDecimals)}");
                        sb.AppendLine($"{'\n'}-----------------------------------");
                    }
                    sb.AppendLine($"{GetLanguageValue("depositQuantity")}: {count}");
                }
            }
            catch (Exception value) { Log.Fatal(value); }
            return sb.ToString();
        }

        private string GetMulticashDataInfo2()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            try
            {
                if (Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total.Count > 0 && Core.Bo.ExtraInfo.CashInMultiCashData.TotalizedDeposit.total.Count > 0)
                {
                    foreach (Values item in Core.Bo.ExtraInfo.CashInMultiCashData.ListPartialDeposit.total)
                    {
                        num++;
                        stringBuilder.Append(string.Format("{0})- {1} {2}: {3}{4}", num.ToString().PadLeft(2, ' '), GetLanguageValue("depositDetail"), item.currency, item.amount, '\n'));
                    }
                    foreach (Values item2 in Core.Bo.ExtraInfo.CashInMultiCashData.TotalizedDeposit.total)
                    {
                        stringBuilder.AppendLine(string.Format("{0} {1}: {2}", GetLanguageValue("depositTotal"), item2.currency, item2.amount));
                    }
                }
            }
            catch (Exception value)
            {
                Log.Fatal(value);
            }
            return stringBuilder.ToString();
        }

        private string SplitStringByWidth(string input, int maxLineLength)
        {
            if (input == null) return null;

            StringBuilder result = new StringBuilder();
            string[] words = input.Split(' ');

            StringBuilder currentLine = new StringBuilder();


            foreach (string word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxLineLength)
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(" ");
                    }
                    currentLine.Append(word);
                }
                else
                {
                    result.Append(currentLine.ToString());
                    result.Append('\n');
                    currentLine.Clear();
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
            {
                result.Append(currentLine.ToString());
            }

            return result.ToString();
        }

        private decimal ConvertCurrencyToDecimal(string value, string currencyCode)
        {
            decimal result = 0m;
            try
            {
                if (value != null)
                {
                    var culture = (from c in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                   let r = new RegionInfo(c.LCID)
                                   where r != null
                                   && r.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper()
                                   select c).FirstOrDefault();

                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }

                    result = decimal.Parse(value, NumberStyles.Currency, culture);
                }
                else
                {
                    Log.Error("Value is null");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }

            return result;
        }
    }
}
