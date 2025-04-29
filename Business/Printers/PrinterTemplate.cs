using System;
using System.Collections.Generic;
using Entities;
using System.IO;
using System.Xml.Serialization;

namespace Business.Printers
{
    [Serializable()]
    public class PrinterTemplate
    {
        private string TemplateTypeField;
        private List<String> LinesField;

        public PrinterTemplate() { }

		public static bool GetPrinterTemplate(out List<PrinterTemplate> listOfPrinterTemplate, string printerTemplateFileName)
		{
			bool ret = false;
			string text = $"{Const.appPath}Tickets\\{printerTemplateFileName}";
			try
			{
				if (!Directory.Exists($"{Const.appPath}Tickets"))
					Directory.CreateDirectory($"{Const.appPath}Tickets");
				listOfPrinterTemplate = GetTemplatePrinter();
				listOfPrinterTemplate = Utilities.Utils.GetGenericXmlData<List<PrinterTemplate>>(out ret, text, listOfPrinterTemplate);
			}
			catch (Exception ex)
			{
				ret = false;
				throw new Exception($"Error in {text} file. {ex.Message} - {ex.InnerException}");
			}
			return ret;
		}

		/// <summary>
		/// Genera los tickets por defecto por si no existe el archivo de datos de tickets.
		/// case "8":NumberOfSpaces_Type.Item8;
		//case "9":NumberOfSpaces_Type.Item9;
		//case ":":NumberOfSpaces_Type.Item10;
		//case ";":NumberOfSpaces_Type.Item11;
		//case "<":NumberOfSpaces_Type.Item12;
		//case "=":NumberOfSpaces_Type.Item13;
		//case ">":NumberOfSpaces_Type.Item12;
		//case "?":NumberOfSpaces_Type.Item15;
		/// </summary>
		/// <returns></returns>
		private static List<PrinterTemplate> GetTemplatePrinter()
		{
			List<PrinterTemplate> list = new List<PrinterTemplate>();
			try
			{
				//Ticket para TX de depósito de sobre (interno)
				PrinterTemplate printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1BagDropDepositTicket1";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXEN");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINEescBDIN");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG2");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de depósito de sobre (externo) - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1BagDropDepositTicket2";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXEN");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINEescBDIN");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de depósito de sobre (externo) - Host error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1BagDropDepositTicket2Error";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXEN");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINEescBDIN");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Journal TX de depósito de sobre (externo) - Host Ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2BagDropDepositTicket2";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXEN");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINEescBDIN");
				list.Add(printerTemplate);
				//Journal TX de depósito de sobre (externo) - Host error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2BagDropDepositTicket2Error";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXEN");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINEescBDIN");
				list.Add(printerTemplate);
				//Ticket para TX de depósito de billetes validados - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1DepositCash";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de recuperación de depósito de billetes validados - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1RetrievalDepositCash";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngRCTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCOID");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");				
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de depósito de billetes validados - Host error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1DepositCashError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de recuperación de depósito de billetes validados - Host error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1RetrievalDepositCashError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINElngRCTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCOID");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");				
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINElngCOL1");
				printerTemplate.Lines.Add($"escLINElngCOL2");
				printerTemplate.Lines.Add($"escLINElngCOL3");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Journal TX de depósito de billetes validados - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2DepositCash";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				//Journal TX de recuperación de depósito de billetes validados - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2RetrievalDepositCash";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngRCTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCOID");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				//Journal TX de depósito de billetes validados - Host Error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2DepositCashError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				//Journal TX de recuperación de depósito de billetes validados - Host Error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2RetrievalDepositCashError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXCA");
				printerTemplate.Lines.Add($"lngDPER");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINElngRCTX");
				printerTemplate.Lines.Add($"escLINEescLINECOLLECTIONID: escCOID");
				printerTemplate.Lines.Add($"escLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSER");
				printerTemplate.Lines.Add($"escLINE (escUSNA)");
				printerTemplate.Lines.Add($"escLINE          escALUS");				
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescMCAS");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				//Ticket para TX de operación de ShipOut - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1ShipOut";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescSPNO");
				printerTemplate.Lines.Add($"escLINEescBARN");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de operación de ShipOut Notes - Host OK
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1ShipOutNotes";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescSPNO");
				printerTemplate.Lines.Add($"escLINEescBARN");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de operación de ShipOut Notes - Host error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1ShipOutNotesError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINECOLLECTIONID: escCIDN");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescSPNO");
				printerTemplate.Lines.Add($"escLINEescBARN");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de operación de ShipOut Bags - Host ok
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1ShipOutBags";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescSPBA");
				printerTemplate.Lines.Add($"escLINEescBARD");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Ticket para TX de operación de ShipOut Bags - Host Error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1ShipOutBagsError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINECOLLECTIONID: escCIDD");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescSPBA");
				printerTemplate.Lines.Add($"escLINEescBARD");
				printerTemplate.Lines.Add($"escCUTS");
				list.Add(printerTemplate);
				//Journal TX de operación de ShipOut - Host OK
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2ShipOut";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescSPOU");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				//Journal TX de operación de ShipOut - Host Error
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr2ShipOutError";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXSH");
				printerTemplate.Lines.Add($"escLINEescLINElngINTX");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINElngUSER: escUSES");
				printerTemplate.Lines.Add($"escLINE escUSNS");
				printerTemplate.Lines.Add($"escEXDA");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescSPOU");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
				 //Contadores de depósito locales
				printerTemplate = new PrinterTemplate();
				printerTemplate.TemplateType = "Pr1LocalCounters";
				printerTemplate.Lines = new List<string>();
				printerTemplate.Lines.Add($"escLINE lngDATEescFILL9lngTIMEescFILL6lngTSN1");
				printerTemplate.Lines.Add($"escLINEescDATE(dd/MM/yy)escFILL6escTIME(HH:mm)escFILL8escTSN1");
				printerTemplate.Lines.Add($"escLINEescLINEescFILL1escADDR - escCITY");
				printerTemplate.Lines.Add($"escLINEescFILL9escPHON");
				printerTemplate.Lines.Add($"escLINEescLINElngTXLC");
				printerTemplate.Lines.Add($"escLINEescLINElngLUNO: escLUNO");
				printerTemplate.Lines.Add($"escLINElngBATC: escBATC");
				printerTemplate.Lines.Add($"escLINEescLINElngMSG1:");
				printerTemplate.Lines.Add($"escLINEescLINElngNOVA      lngNOQU     lngNOTO");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				printerTemplate.Lines.Add($"escLINEescLINEescLCOU");
				printerTemplate.Lines.Add($"escLINE-----------------------------------");
				list.Add(printerTemplate);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return list;
		}

		#region "Properties"
		/// <summary>
		/// 
		/// </summary>
		[XmlElement("TemplateType")]
        public string TemplateType
        {
            get { return this.TemplateTypeField; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception("Parámetro TemplateType es nulo o vacío.");
                }
                this.TemplateTypeField = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Lines")]
        public List<String> Lines
        {
            get { return this.LinesField; }
            set
            {
                //if (string.IsNullOrEmpty(value))
                //{
                //    throw new Exception("Parámetro Lines es nulo o vacío.");
                //}
                this.LinesField = value;
            }
        }
        #endregion

    }
}
