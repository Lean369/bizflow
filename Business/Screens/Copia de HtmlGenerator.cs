using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Diebold.Agilis.EmPower.NDC.Messages;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Business
{
    public class HtmlGenerator
    {
        private List<ScreenRowMapping> ListOfScreenRowMapping;
        private List<ScreenColumnMapping> ListOfScreenColumnMapping;
        private List<ScreenColorMapping> ListOfScreenColorMapping;
        private int ScreenRowPosition = 0;
        private int ScreenColumnPosition = 0;
        private string CurBackgroundColor = "#ffffff";//"#16777215";
        private string CurDefaultForegroundColor = "#000000";
        private string CurDefaultBackgroundColor = "#ffffff";
        private string CurForegroundColor = "#000000";
        private string CurrentPrimaryCharset = "SingleSizeAlphanumeric1";
        private string CurrentSecondaryCharset = "NDCFontSingleSizeAlphanumeric1_Entry_box";
        private bool isTransparentOn = true;
        private bool isBlinkOn = false;
        public Dictionary<string, ScreenData_Type> DiccScreenData;
        private string ScreensPath;
        private Utilities.Logger LoggerField;
        private bool LogAppEnable = false;
        public enum ScreenType { General, AmountEntry, InformationEntry, PinEntry }

        public HtmlGenerator(Dictionary<string, ScreenData_Type> diccScreenData, List<ScreenRowMapping> listOfScreenRowMapping, List<ScreenColumnMapping> listOfScreenColumnMapping,
            List<ScreenColorMapping> listOfScreenColorMapping, string screensPath, Utilities.Logger loggerField, bool logAppEnable, bool Digit7aEnable)
        {
            this.ListOfScreenRowMapping = listOfScreenRowMapping;
            this.ListOfScreenColumnMapping = listOfScreenColumnMapping;
            this.ListOfScreenColorMapping = listOfScreenColorMapping;
            this.DiccScreenData = diccScreenData;
            this.ScreensPath = screensPath;
            this.LoggerField = loggerField;
            this.LogAppEnable = logAppEnable;
        }

        /// <summary>
        /// Genera un documento HTML a partir del número de pantalla y máscara de FDKs activas solicitados.
        /// </summary>
        /// <param name="screenNumber"></param>
        /// <param name="keyMask"></param>
        /// <returns></returns>
        public string GetHtmlDocument(string screenNumber, KeyMask_Type keyMask, object optionalParameter, ScreenType screenType)
        {
            var xDocument = new XDocument(new XDocumentType("html", null, null, null));
            string insertScreen = string.Empty, screenFileName, templateFileName = "NDCTemplate";
            List<XElement> ListOfxElements1 = new List<XElement>();
            List<XElement> ListOfxElements2 = new List<XElement>();
            List<SingleScreenScreenData_Type> ListOfInsertScreens = new List<SingleScreenScreenData_Type>();
            List<XElement> ListOfXElements = new List<XElement>();
            List<SingleScreenScreenData_Type> ListOfSingleScreenScreenData = new List<SingleScreenScreenData_Type>();
            try
            {
                this.ProcessLog(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("|-->Start of function<--|"));
                //1)- Cargo los elementos HTMl estáticos desde el archivo NDCTemplate.htm
                templateFileName = this.GetScreenTemplate(screenType);
                screenNumber = screenNumber.ToUpper().PadLeft(3, '0');
                screenFileName = string.Format(@"{0}\{1}{2}.htm", this.ScreensPath, templateFileName, screenNumber);
                if (File.Exists(screenFileName))//Template específico para la pantalla a mostrar.
                {
                    xDocument = XDocument.Load(screenFileName);
                }
                else//Template general
                {
                    screenFileName = string.Format(@"{0}\{1}.htm", this.ScreensPath, templateFileName);
                    if (File.Exists(screenFileName))
                        xDocument = XDocument.Load(screenFileName);
                    else
                        throw new Exception(string.Format("HTML Template file doesn´t exist: {0}", screenFileName));
                }
                //2)- Traigo los elementos HTML dinámicos de pantalla
                if (this.DiccScreenData.ContainsKey(screenNumber))
                {
                    ScreenData_Type screenDataCommands = this.DiccScreenData[screenNumber];
                    //3)- Separo los comandos "InsertScreen" del resto y los guardo en listas separadas
                    foreach (SingleScreenScreenData_Type singleScreenScreenData in screenDataCommands.Command)
                    {
                        if (singleScreenScreenData.ItemElementName == ItemChoiceSingleScreenScreenData_Type.InsertScreen)
                            ListOfInsertScreens.Add(singleScreenScreenData); //Insert screens
                        else
                            ListOfSingleScreenScreenData.Add(singleScreenScreenData); //Screen commands
                    }
                    //4)- Inserto los comandos inputBox, si es que corresponde.
                    //ListOfSingleScreenScreenData.AddRange(this.GetInputCommand(screenType, optionalParameter));
                    //ScreenData_Type sd = new ScreenData_Type();
                    //sd.Command = this.GetInputCommand(screenType, optionalParameter).ToArray();
                    //ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(sd));
                    //5)- Agrego en una lista los comandos de pantalla contenidos en los INSERT SCREEN
                    if (ListOfInsertScreens.Count != 0)
                    {
                        foreach (SingleScreenScreenData_Type screenCommand in ListOfInsertScreens)
                        {
                            if (screenCommand.ItemElementName == ItemChoiceSingleScreenScreenData_Type.SetCursor)
                            {
                                ScreenData_Type sd = new ScreenData_Type();
                                sd.Command = this.GetInputCommand(screenType, optionalParameter).ToArray();
                                ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(sd));
                            }
                            else
                            {
                                ListOfSingleScreenScreenData.AddRange(this.GetInputCommand(screenType, optionalParameter));
                            }
                        }
                        foreach (SingleScreenScreenData_Type screenCommand in ListOfInsertScreens)
                        {
                            ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(this.DiccScreenData[screenCommand.Item.ToString()]));
                        }
                    }
                    //ScreenData_Type sd = new ScreenData_Type();
                    //sd.Command = this.GetInputCommand(screenType, optionalParameter).ToArray();
                    //ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(sd));

                    //6)- Agrego en una lista los comandos de pantalla contenidos en la pantalla principal
                    if (ListOfSingleScreenScreenData.Count != 0)
                    {
                        ScreenData_Type sd1 = new ScreenData_Type();
                        sd1.Command = ListOfSingleScreenScreenData.ToArray();
                        ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(sd1));
                    }
                    //7)- Agrego los elementos HTML dinámicos dentro del tag "body" y "head"
                    foreach (XNode node in xDocument.DescendantNodes())
                    {
                        if (node is XElement)
                        {
                            //A)- Inserto elementos en el Head
                            XElement element = (XElement)node;
                            if (element.Name.LocalName.Equals("head"))
                            {
                                element.AddFirst(new XElement("link", new XAttribute("rel", "stylesheet"), new XAttribute("type", "text/css"),
                                    new XAttribute("href", string.Format(@"{0}\style.css", this.ScreensPath))));//El archivo "Style.css" lo cargo aqui porque no lo toma desde el template.
                            }
                            //B)- Inserto elementos en el Body
                            if (element.Name.LocalName.Equals("body"))
                            {
                                foreach (XElement xE in ListOfXElements)
                                {
                                    element.Add(xE);//Agrego los comandos de pantalla
                                }
                                element.Add(this.BuildScreenAreasFDKs(keyMask)); //Agrego la activación de zonas de TS
                            }
                        }
                    }
                }
                else
                {
                    this.ProcessLog(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Error, string.Format("Screen: {0} not found.", screenNumber));
                }
            }
            catch (Exception ex)
            {
                this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex);
                xDocument = this.GetMessageErrorForWebBrowser(string.Format("Error when trying to create screen: {0}", screenNumber));
            }
            return xDocument.ToString().Replace("&amp;nbsp;", "&nbsp;");
        }

        /// <summary>
        /// Convierte los comandos de pantalla NDC a código HTML que luego se representará en el Web Browser.
        /// </summary>
        /// <param name="screenDataCommands"></param>
        /// <returns></returns>
        private List<XElement> ConvertNDCCommandToHTMLElements(ScreenData_Type screenDataCommands)
        {
            var xDocument = new XDocument(new XDocumentType("html", null, null, null));
            XElement xElement;
            List<XElement> ListOfxElements = new List<XElement>();
            string id = string.Empty;
            try
            {
                this.ProcessLog(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("|-->Start of function<--|"));
                if (screenDataCommands.Command != null)
                {
                    foreach (SingleScreenScreenData_Type singleScreenScreenData in screenDataCommands.Command)
                    {
                        switch (singleScreenScreenData.ItemElementName)
                        {
                            case ItemChoiceSingleScreenScreenData_Type.ClearScreen://Comando que realiza el borrado de pantalla
                                {
                                    this.ScreenRowPosition = 0;
                                    this.ScreenColumnPosition = 0;
                                    this.ResetBlinkColorSettings();
                                    this.CurrentPrimaryCharset = "SingleSizeAlphanumeric1";
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.SetCursor: //Comando que setea el posicionamiento del cursor de pantalla
                                {
                                    SetCursor_Type setCursor = (SetCursor_Type)singleScreenScreenData.Item;
                                    this.ScreenRowPosition = this.GetScreenRowMapp(setCursor.Row);
                                    this.ScreenColumnPosition = this.GetScreenColumnMapp(setCursor.Column);
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.ScreenBlinkingColorControl: //Comando para activar el parpadeo de texto
                                {
                                    ScreenBlinkingColorControl_Type screenBlinkingColorControl = (ScreenBlinkingColorControl_Type)singleScreenScreenData.Item;
                                    this.SetHtmlColorCode(screenBlinkingColorControl);
                                    if (screenBlinkingColorControl.command1 == BlinkingCommand_Type.SetBlinkingOn || screenBlinkingColorControl.command2 == BlinkingCommand_Type.SetBlinkingOn || screenBlinkingColorControl.command3 == BlinkingCommand_Type.SetBlinkingOn)
                                        id = "blink";
                                    if (screenBlinkingColorControl.command1 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff || screenBlinkingColorControl.command2 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff || screenBlinkingColorControl.command3 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff)
                                        ResetBlinkColorSettings();
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.SelectPrimaryCharSet: //Comando para setear el tipo de fuente primario
                                {
                                    this.CurrentPrimaryCharset = singleScreenScreenData.Item.ToString();
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.SelectSecondaryCharSet: //Comando para setear el tipo de fuente secundario
                                {
                                    this.CurrentSecondaryCharset = singleScreenScreenData.Item.ToString();
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.DrawPicture: //Comando para insertar una imágen JPG en pantalla
                                {
                                    xElement = this.GetDivElementForText(0, 0);
                                    string screenPath = string.Format(@"{0}\Images\icon{1}.jpg", this.ScreensPath, singleScreenScreenData.Item);
                                    xElement.Add(new XElement("p", new XElement("img", new XAttribute("border", "0"), new XAttribute("src", screenPath))));
                                    ListOfxElements.Add(xElement);
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.Print: //Comando para insertar un texto en pantalla
                                {
                                    xElement = this.GetDivElementForText(0, 0);
                                    string text = singleScreenScreenData.Item.ToString().Replace(" ", "&nbsp;");
                                    xElement.Add(new XElement("p", new XAttribute("id", id), new XAttribute("class", this.CurrentPrimaryCharset), new XAttribute("style", "letter-spacing:6px"), new XElement("font", new XAttribute("color", this.CurForegroundColor), text)));
                                    ListOfxElements.Add(xElement);
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.DrawImageFile: //Comando para insertar un video mpg en pantalla
                                {
                                    xElement = this.GetDivElementForText(0, 0);
                                    string videoPath = string.Format(@"{0}", singleScreenScreenData.Item);
                                    xElement.Add(new XElement("p", new XElement("embed", new XAttribute("src", string.Format("{0}", singleScreenScreenData.Item.ToString())), new XAttribute("loop", "true"), new XAttribute("border", "0"))));
                                    ListOfxElements.Add(xElement);
                                    break;
                                }
                            case ItemChoiceSingleScreenScreenData_Type.InputAmountEntry: //Comando que inserta un InputBox para una pantalla de Amount Entry a partir de una máscara de monto.
                                {
                                    string mask = singleScreenScreenData.Item.ToString().ToUpper();
                                    char[] ch = { '*', 'Z', 'T' };
                                    int sizeMask = mask.Split(ch).Length - 1;//El tamaño de la mascara lo determina la cantidad de patrones '*','Z'
                                    int sizeInputBox = mask.Length * 2;
                                    string MonetaryField = string.Empty;
                                    if (mask.Contains("U$S"))
                                        MonetaryField = "U$S";
                                    else if (mask.Contains("$"))
                                        MonetaryField = "$";
                                    string value = mask.Replace("*", "").Replace("Z", "");
                                    //Agrego el inputBox
                                    xElement = this.GetDivElementForInput("InputLayer", "NDCamountentry_box");
                                    xElement.Add(new XElement("form", new XAttribute("name", "InputForm"), new XAttribute("ID", "Form1"), new XElement("INPUT", new XAttribute("id", "Text1"),
                                        new XAttribute("value", string.Format("{0}", value)), new XAttribute("type", "text"), new XAttribute("size", sizeInputBox.ToString()), new XAttribute("name", "TextBox"),
                                        new XAttribute("style", string.Format("color:{0}; letter-spacing:6px; TEXT-ALIGN:right; left: {1}px; top: {2}px", this.CurForegroundColor, this.ScreenColumnPosition + (290 - (sizeInputBox * 15)), this.ScreenRowPosition - 3)),
                                        new XAttribute("class", this.CurrentSecondaryCharset), new XAttribute("onfocus", "blur()"))));
                                    //Agrego las invocaciones a funciones Javascript
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), "document.onkeypress = InputFields;window.onload = function() {document.getElementById(\"TextBox\").focus();};"));
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), string.Format("SetFieldLength( \"{0}\" )", sizeMask)));
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), string.Format("SetMonetaryField(( \"{0}\" )", MonetaryField)));
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), string.Format("SetFieldMask( \"{0}\" )", mask)));
                                    ListOfxElements.Add(xElement);
                                    break;
                                    //TEXT-ALIGN: center; , new XAttribute("dir", "RTL")
                                }
                            case ItemChoiceSingleScreenScreenData_Type.InputInformationEntry: //Comando que inserta un InputBox para una pantalla de Information Entry
                                {
                                    DisplayAndBufferParameters_Type displayAndBufferParameters = (DisplayAndBufferParameters_Type) singleScreenScreenData.Item;
                                    string mask = "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
                                    char[] ch = { '*', 'Z', 'T'};
                                    int sizeMask = mask.Split(ch).Length - 1;//El tamaño de la mascara lo determina la cantidad de patrones '*','Z'
                                    int sizeInputBox = mask.Length;
                                    string value = string.Empty;
                                    //Agrego el inputBox
                                    xElement = this.GetDivElementForInput("InputLayer", "NDCtext_box");
                                    xElement.Add(new XElement("form", new XAttribute("name", "InputForm"), new XAttribute("ID", "Form1"), new XElement("INPUT", new XAttribute("id", "Text1"),
                                        new XAttribute("value", string.Format("{0}", value)), new XAttribute("type", "text"), new XAttribute("size", sizeInputBox.ToString()), new XAttribute("name", "TextBox"),
                                        new XAttribute("style", string.Format("color:{0}; TEXT-ALIGN:left; left: {1}px; top: {2}px", this.CurForegroundColor, this.ScreenColumnPosition, this.ScreenRowPosition - 20)),
                                        new XAttribute("class", "NDCFontSingleSizeAlphanumeric2_Entry_box"), new XAttribute("onfocus", "blur()")),
                                        new XAttribute("dir", "RTL")));
                                    //Agrego las invocaciones a funciones Javascript
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), "document.onkeypress = InputFields;window.onload = function() {document.getElementById(\"TextBox\").focus();};"));
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), string.Format("SetFieldLength( \"{0}\" )", sizeMask)));
                                    xElement.Add(new XElement("script", new XAttribute("language", "javascript"), string.Format("SetInfoFieldMask( \"{0}\" )", mask)));
                                    ListOfxElements.Add(xElement);
                                    break;
                                    //TEXT-ALIGN: center; , new XAttribute("dir", "RTL")
                                }
                        }
                    }
                }
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return ListOfxElements;
        }

        private string GetScreenTemplate(ScreenType screenType)
        {
            string templateFileName = "NDCTemplate";
            switch (screenType)
            {
                case ScreenType.AmountEntry:
                    {
                        templateFileName = "NDCAmountEntryTemplate";
                        break;
                    }
                case ScreenType.InformationEntry:
                    {
                        templateFileName = "NDCInformationEntryTemplate";
                        break;
                    }
                case ScreenType.PinEntry:
                    {
                        templateFileName = "NDCPinEntryTemplate";
                        break;
                    }
            }
            return templateFileName;
        }

        /// <summary>
        /// Retorna los elementos HTML para formar un InputBox
        /// </summary>
        /// <param name="screenType"></param>
        /// <returns></returns>
        private List<SingleScreenScreenData_Type> GetInputCommand(ScreenType screenType, object optionalParameter)
        {
            List<SingleScreenScreenData_Type> ListOfCommands = new List<SingleScreenScreenData_Type>();
            string entryMask = string.Empty;
            DisplayAndBufferParameters_Type displayAndBufferParameters;
            switch (screenType)
            {
                case ScreenType.AmountEntry:
                    {
                        entryMask = optionalParameter as string;
                        if (this.DiccScreenData.ContainsKey(entryMask))
                        {
                            ScreenData_Type screenTemplateCommands = this.DiccScreenData[entryMask];
                            foreach (Business.SingleScreenScreenData_Type sSd in screenTemplateCommands.Command)
                            {
                                if (sSd.ItemElementName == ItemChoiceSingleScreenScreenData_Type.Print)
                                    sSd.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InputAmountEntry;//Cambio el comando para luego insertar un input data.
                                ListOfCommands.Add(sSd);
                            }
                        }
                        else
                            this.ProcessLog(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Error, string.Format("Entry mask not found: {0}", entryMask));
                        break;
                    }
                case ScreenType.InformationEntry:
                    {
                        displayAndBufferParameters = (DisplayAndBufferParameters_Type) optionalParameter;
                        Business.SingleScreenScreenData_Type sSd = new SingleScreenScreenData_Type();
                        sSd.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InputInformationEntry;
                        sSd.Item = displayAndBufferParameters;
                        ListOfCommands.Add(sSd);
                        break;
                    }
                case ScreenType.PinEntry:
                    {
                        break;
                    }
            }
            return ListOfCommands;
        }

        private XElement GetDivElementForInput(string id, string clas)
        {
            XElement xElement;
            xElement = new XElement("div", new XAttribute("id", id), new XAttribute("class", clas));
            return xElement;
        }

        private XElement GetForm(string name, string id)
        {
            XElement xElement;
            xElement = new XElement("form", new XAttribute("name", name), new XAttribute("ID", id));
            return xElement;
        }

        private XElement GetDivElementForText(int addScreenColumnPosition, int addScreenRowPosition)
        {
            XElement xElement;
            if (this.isTransparentOn)
                xElement = new XElement("div", new XAttribute("style", string.Format("position: absolute; left: {0}px; top: {1}px;", this.ScreenColumnPosition + addScreenColumnPosition, this.ScreenRowPosition + addScreenRowPosition)));
            else
                xElement = new XElement("div", new XAttribute("style", string.Format("position: absolute; left: {0}px; top: {1}px; background-color:{2}", this.ScreenColumnPosition, this.ScreenRowPosition, this.CurBackgroundColor)));
            return xElement;
        }


        /// <summary>
        /// Construye una tabla HTML que definira las zonas de FDK Touch Screen que se activarán. 
        /// El tamaño de las zonas se configura en el archivo "Style.css" a partir de los atributos "class: FX"
        /// </summary>
        /// <param name="keyMask"></param>
        /// <returns></returns>
        private XElement BuildScreenAreasFDKs(KeyMask_Type keyMask)
        {
            XElement xTable = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "ScreenTable"));
            if (keyMask.FDKI)
            {
                XElement xTableI = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F1"), new XAttribute("ID", "I"));
                xTableI.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableI);
            }
            if (keyMask.FDKH)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F2"), new XAttribute("ID", "H"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKG)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F3"), new XAttribute("ID", "G"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKF)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F4"), new XAttribute("ID", "F"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKA)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F9"), new XAttribute("ID", "A"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKB)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F10"), new XAttribute("ID", "B"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKC)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F11"), new XAttribute("ID", "C"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            if (keyMask.FDKD)
            {
                XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "F12"), new XAttribute("ID", "D"));
                xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                xTable.Add(xTableH);
            }
            return xTable;
        }

        /// <summary>
        /// Setea un una variable global el color de fuente.
        /// </summary>
        /// <param name="screenBlinkingColorControl"></param>
        private void SetHtmlColorCode(ScreenBlinkingColorControl_Type screenBlinkingColorControl)
        {
            try
            {
                this.SetHtmlColorCode(screenBlinkingColorControl.command2);
                this.SetHtmlColorCode(screenBlinkingColorControl.command1);
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
        }

        /// <summary>
        /// Obtiene el número de fila de un comando de pantalla.
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        private int GetScreenRowMapp(ScreenRow_Type rowNumber)
        {
            int ret = 0;
            ScreenRowMapping mappedRow = new ScreenRowMapping();
            mappedRow.NDCRowNumber = ScreenRow_Type.Item0;
            mappedRow.NewPositionInPixels = "0";
            try
            {
                mappedRow = this.ListOfScreenRowMapping.Find(x => x.NDCRowNumber == rowNumber);
                if (!int.TryParse(mappedRow.NewPositionInPixels, out ret))
                {
                    //TODO: show error
                }
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return ret;
        }

        /// <summary>
        /// Obtiene el número de columna de un comando de pantalla.
        /// </summary>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        private int GetScreenColumnMapp(ScreenColumn_Type columnNumber)
        {
            int ret = 0;
            ScreenColumnMapping mappColumn = new ScreenColumnMapping();
            mappColumn.NDCColumnNumber = ScreenColumn_Type.Item0;
            mappColumn.NewPositionInPixels = "0";
            try
            {
                mappColumn = this.ListOfScreenColumnMapping.Find(x => x.NDCColumnNumber == columnNumber);
                if (!int.TryParse(mappColumn.NewPositionInPixels, out ret))
                {
                    //TODO: show error
                }
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
            return ret;
        }

        /// <summary>
        /// Setea a default el color y parpadeo.
        /// </summary>
        private void ResetBlinkColorSettings()
        {
            this.isBlinkOn = false;
            this.isTransparentOn = true;
            this.CurForegroundColor = this.CurDefaultForegroundColor;
            this.CurBackgroundColor = this.CurDefaultBackgroundColor;
        }

        /// <summary>
        /// Setea en variables globales: Color frontal, color de fondo, parpadeo y transparencia.
        /// </summary>
        /// <param name="_ndcColorName"></param>
        private void SetHtmlColorCode(BlinkingCommand_Type _ndcColorName)
        {
            ScreenColorMapping screenColor = new ScreenColorMapping();
            screenColor.HtmlColorCode = "#000000";
            screenColor.NdcColorCode = "21";
            screenColor.NdcColorName = BlinkingCommand_Type.RedForeground_LowIntensity;
            try
            {
                screenColor = this.ListOfScreenColorMapping.Find(x => x.NdcColorName == _ndcColorName);
                switch (screenColor.NdcColorName)
                {
                    case BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff:
                        {
                            this.ResetBlinkColorSettings();
                            break;
                        }
                    case BlinkingCommand_Type.SetBlinkingOn:
                        {
                            this.isBlinkOn = true;
                            break;
                        }
                    case BlinkingCommand_Type.SetBlinkingOff:
                        {
                            this.isBlinkOn = false;
                            break;
                        }
                    case BlinkingCommand_Type.TransparentBackground:
                        {
                            this.isTransparentOn = true;
                            break;
                        }
                    case BlinkingCommand_Type.RedForeground_LowIntensity:
                    case BlinkingCommand_Type.RedForeground_HighIntensity:
                    case BlinkingCommand_Type.GreenForeground_LowIntensity:
                    case BlinkingCommand_Type.GreenForeground_HighIntensity:
                    case BlinkingCommand_Type.BlackForeground_LowIntensity:
                    case BlinkingCommand_Type.BlackForeground_HighIntensity:
                    case BlinkingCommand_Type.YellowForeground_LowIntensity:
                    case BlinkingCommand_Type.YellowForeground_HighIntensity:
                    case BlinkingCommand_Type.BlueForeground_LowIntensity:
                    case BlinkingCommand_Type.BlueForeground_HighIntensity:
                    case BlinkingCommand_Type.MagentaForeground_LowIntensity:
                    case BlinkingCommand_Type.MagentaForeground_HighIntensity:
                    case BlinkingCommand_Type.CyanForeground_LowIntensity:
                    case BlinkingCommand_Type.CyanForeground_HighIntensity:
                    case BlinkingCommand_Type.WhiteForeground_LowIntensity:
                    case BlinkingCommand_Type.WhiteForeground_HighIntensity:
                        {
                            this.CurForegroundColor = screenColor.HtmlColorCode;
                            break;
                        }
                    case BlinkingCommand_Type.RedBackground_LowIntensity:
                    case BlinkingCommand_Type.RedBackground_HighIntensity:
                    case BlinkingCommand_Type.GreenBackground_LowIntensity:
                    case BlinkingCommand_Type.GreenBackground_HighIntensity:
                    case BlinkingCommand_Type.BlackBackground_LowIntensity:
                    case BlinkingCommand_Type.BlackBackground_HighIntensity:
                    case BlinkingCommand_Type.YellowBackground_LowIntensity:
                    case BlinkingCommand_Type.YellowBackground_HighIntensity:
                    case BlinkingCommand_Type.BlueBackground_LowIntensity:
                    case BlinkingCommand_Type.BlueBackground_HighIntensity:
                    case BlinkingCommand_Type.MagentaBackground_LowIntensity:
                    case BlinkingCommand_Type.MagentaBackground_HighIntensity:
                    case BlinkingCommand_Type.CyanBackground_LowIntensity:
                    case BlinkingCommand_Type.CyanBackground_HighIntensity:
                    case BlinkingCommand_Type.WhiteBackground_LowIntensity:
                    case BlinkingCommand_Type.WhiteBackground_HighIntensity:
                        {
                            this.CurBackgroundColor = screenColor.HtmlColorCode;
                            break;
                        }
                    case BlinkingCommand_Type.RedDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.RedDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.GreenDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.GreenDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.BlackDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.BlackDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.YellowDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.YellowDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.BlueDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.BlueDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.MagentaDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.MagentaDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.CyanDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.CyanDefaultForeground_HighIntensity:
                    case BlinkingCommand_Type.WhiteDefaultForeground_LowIntensity:
                    case BlinkingCommand_Type.WhiteDefaultForeground_HighIntensity:
                        {
                            this.CurDefaultForegroundColor = screenColor.HtmlColorCode;
                            break;
                        }
                    case BlinkingCommand_Type.RedDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.RedDefaultBackground_HighIntensity:
                    case BlinkingCommand_Type.GreenDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.BlackDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.BlueDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.BlueDefaultBackground_HighIntensity:
                    case BlinkingCommand_Type.MagentaDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.MagentaDefaultBackground_HighIntensity:
                    case BlinkingCommand_Type.CyanDefaultBackground_LowIntensity:
                    case BlinkingCommand_Type.CyanDefaultBackground_HighIntensity:
                        {
                            this.CurDefaultBackgroundColor = screenColor.HtmlColorCode;
                            break;
                        }
                }
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }
        }

        private XDocument GetMessageErrorForWebBrowser(string message)
        {
            var xDocument = new XDocument();
            try
            {
                xDocument = new XDocument(
                new XDocumentType("html", null, null, null),
                new XElement("html",
                new XElement("head"),
                new XElement("body",
                    new XElement("div", new XAttribute("style", "position: absolute; left: 100px; top: 200px"),
                        new XElement("p", new XAttribute("id", "A"), new XAttribute("class", "NDCFontSingleSizeAlphanumeric1"), new XAttribute("style", "letter-spacing:5px"), new XElement("font", new XAttribute("color", "#A80000"), message)))

                )));
            }
            catch (Exception ex) { this.ProcessLogException(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), ex); }

            return xDocument;
        }

        /// <summary>
        /// Procesa el logeo de excepciones en archivo, pantalla y EventLog.
        /// </summary>
        /// <param name="ex">Excepción a logear</param>
        public void ProcessLogException(string functionName, Exception ex)
        {
            try
            {
                if (this.LogAppEnable)
                    this.LoggerField.LogException(ex, functionName);
            }
            catch (Exception exi) { System.IO.File.WriteAllText(string.Format(@"{0}\LogError.log", Business.Const.appPath), string.Format("LogException(): {0}", exi.Message)); }
        }

        /// <summary>
        /// Procesa el logeo de excepciones en archivo, pantalla y EventLog.
        /// </summary>
        /// <param name="ex">Excepción a logear</param>
        public void ProcessLog(string functionName, int iD, Utilities.Logger.LogType logLevel, string message)
        {
            try
            {
                if (this.LogAppEnable)
                {
                    this.LoggerField.LogMessage(functionName, iD, logLevel, message);
                }
            }
            catch (Exception ex) { System.IO.File.WriteAllText(string.Format(@"{0}\LogError.log", Business.Const.appPath), string.Format("LogAppMessage(): {0}", ex.Message)); }
        }

        internal string GetNdcSubfolder(string screenName)
        {
            switch (screenName.Length)
            {
                case 3:
                    if (Regex.IsMatch(screenName, "[0-9]{3}"))
                    {
                        return "00" + screenName.Substring(0, 1) + "00\\";
                    }
                    if (Regex.IsMatch(screenName, "[tT][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[aAhHiImMpPqQsS][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[cC][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[bB][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[lLgG][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[kK][0-9]{2}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (screenName.ToLower().Equals("itr"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    break;
                case 4:
                    if (Regex.IsMatch(screenName, "[1-9][0-9]{3}"))
                    {
                        return "0" + screenName.Substring(0, 2) + "00\\";
                    }
                    if (Regex.IsMatch(screenName, "_[iI][0-9]{2}"))
                    {
                        return screenName.Substring(0, 2) + "\\";
                    }
                    break;
                case 5:
                    if (Regex.IsMatch(screenName, "[uUvVxXyYzZ][0-9]{4}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[eE][0-9]{4}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    if (Regex.IsMatch(screenName, "[dD][0-9]{4}"))
                    {
                        return screenName.Substring(0, 1) + "\\";
                    }
                    break;
            }
            this.ProcessLog(string.Format("HtmlGenerator.{0}", new StackTrace().GetFrame(0).GetMethod().Name), 0, Utilities.Logger.LogType.Info, string.Format("Invalid screen name pattern: {0}", screenName));
            throw new ArgumentException("Invalid NDC screen name:" + screenName);
        }
        //private KeyMask_Type GetKeyMaskData(string text)
        //{
        //    KeyMask_Type keyMask = new KeyMask_Type();
        //    keyMask.FDKA = false;
        //    keyMask.FDKB = false;
        //    keyMask.FDKC = false;
        //    keyMask.FDKD = false;
        //    keyMask.FDKF = false;
        //    keyMask.FDKG = false;
        //    keyMask.FDKH = false;
        //    keyMask.FDKI = false;
        //    byte[] arrByte;
        //    int readCond;
        //    if (int.TryParse(text, out readCond))
        //    {
        //        if (Utilities.Utils.DecToBin(readCond, out arrByte))
        //        {
        //            if (arrByte[7] == 1)
        //                keyMask.FDKA = true;
        //            if (arrByte[6] == 1)
        //                keyMask.FDKB = true;
        //            if (arrByte[5] == 1)
        //                keyMask.FDKC = true;
        //            if (arrByte[4] == 1)
        //                keyMask.FDKD = true;
        //            if (arrByte[3] == 1)
        //                keyMask.FDKF = true;
        //            if (arrByte[2] == 1)
        //                keyMask.FDKG = true;
        //            if (arrByte[1] == 1)
        //                keyMask.FDKH = true;
        //            if (arrByte[0] == 1)
        //                keyMask.FDKI = true;
        //        }
        //    }
        //    return keyMask;
        //}
    }
}
