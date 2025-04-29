using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Entities;

namespace Business
{
    public class HtmlGenerator
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private List<ScreenRowMapping> ListOfScreenRowMapping;
        private List<ScreenColumnMapping> ListOfScreenColumnMapping;
        private List<ScreenColorMapping> ListOfScreenColorMapping;
        private int ScreenRowPositionPx = 0;
        private int ScreenColumnPositionPx = 0;
        private string CurBackgroundColor = "#ffffff";//"#16777215";
        private string CurDefaultForegroundColor = "#000000";
        private string CurDefaultBackgroundColor = "#ffffff";
        private string CurForegroundColor = "#000000";
        private string PrimaryCharset = "SingleSizeAlphanumeric1";
        private string SecondaryCharset = "SingleSizeAlphanumeric2";
        private bool isTransparentOn = true;
        //private bool isBlinkOn = false;
        private Dictionary<string, ScreenData_Type> DiccScreenData;
        private string ScreensPath;
        public enum ScreenType { General, AmountEntry, InformationEntry, PinEntry, Message, TableSelector }
        Dictionary<string, string> DicOfPlaceholder = new Dictionary<string, string>();
        private bool addScreen = false;
        private Const.Resolution Resolution;
        private ScreenConfiguration ScreenConfiguration;
        private string BlinkId = string.Empty;

        public HtmlGenerator(Dictionary<string, ScreenData_Type> diccScreenData, List<ScreenRowMapping> listOfScreenRowMapping, List<ScreenColumnMapping> listOfScreenColumnMapping,
            List<ScreenColorMapping> listOfScreenColorMapping, string screensPath, ScreenConfiguration screenConfiguration, Const.Resolution resolution)
        {
            this.ListOfScreenRowMapping = listOfScreenRowMapping;
            this.ListOfScreenColumnMapping = listOfScreenColumnMapping;
            this.ListOfScreenColorMapping = listOfScreenColorMapping;
            this.DiccScreenData = diccScreenData;
            this.ScreensPath = screensPath;
            this.Resolution = resolution;
            this.ScreenConfiguration = screenConfiguration;
        }

        /// Genera un documento HTML a partir del número de pantalla y máscara de FDKs activas solicitados.
        public bool GetHtmlDocument(string screenNumber, KeyMask_Type keyMask, object optionalParameter, ScreenType screenType, string messageToShow, out string htmlToShow)
        {
            bool ret = true;
            StringReader sr;
            XDocument xDocumentMain = new XDocument(new XDocumentType("html", null, null, null));
            string insertScreen = string.Empty, templateFileName, templateName = "NDCTemplate";
            List<XElement> ListOfxElements1 = new List<XElement>();
            List<XElement> ListOfxElements2 = new List<XElement>();
            List<SingleScreenScreenData_Type> ListOfInsertScreens = new List<SingleScreenScreenData_Type>();
            List<XElement> ListOfXElements = new List<XElement>();
            List<SingleScreenScreenData_Type> ListOfSingleScreenScreenData = new List<SingleScreenScreenData_Type>();
            htmlToShow = this.GetMessageErrorForWebBrowser(string.Format("Error when trying to create screen: {0}", screenNumber)).ToString();
            try
            {
                Log.Debug("/--->");
                this.addScreen = true;
                //1)- Cargo los elementos HTMl estáticos desde el archivo NDCTemplate.htm
                if (!string.IsNullOrEmpty(screenNumber))
                {
                    templateName = this.GetScreenTemplateName(screenType);
                    screenNumber = screenNumber.ToUpper().PadLeft(3, '0').Replace(".NDC", "");
                    templateFileName = string.Format(@"{0}\{1}{2}.htm", this.ScreensPath, templateName, screenNumber);
                    if (File.Exists(templateFileName))//Template específico para la pantalla a mostrar.
                    {
                        xDocumentMain = XDocument.Load(templateFileName);
                    }
                    else //Template general
                    {
                        templateFileName = string.Format(@"{0}\{1}.htm", this.ScreensPath, templateName);
                        if (File.Exists(templateFileName))
                            xDocumentMain = XDocument.Load(templateFileName);
                        else
                        {
                            Log.Error("HTML Template file doesn´t exist: {0}", templateFileName);
                            ret = false;
                        }
                    }
                    ///////////////////////////////////////////////////
                    //2)- Traigo los comandos de la pantalla solicitada.
                    ///////////////////////////////////////////////////
                    if (this.DiccScreenData.ContainsKey(screenNumber))
                    {
                        ScreenData_Type mainScreenData = this.DiccScreenData[screenNumber];
                        if (mainScreenData.Command != null)
                        {
                            foreach (SingleScreenScreenData_Type singleScreenScreenData in mainScreenData.Command)
                            {
                                //2.A)- Agrego los comandos de "Insert Screen" a la lista de XElements
                                if (singleScreenScreenData.ItemElementName == ItemChoiceSingleScreenScreenData_Type.InsertScreen)
                                {
                                    if (this.DiccScreenData.ContainsKey(singleScreenScreenData.Item.ToString()))
                                    {
                                        ScreenData_Type insertScreenData = this.DiccScreenData[singleScreenScreenData.Item.ToString()];
                                        foreach (SingleScreenScreenData_Type insertSingleScreenScreenData in insertScreenData.Command)
                                        {
                                            ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(insertSingleScreenScreenData));
                                        }
                                    }
                                    else
                                    {
                                        Log.Error("Insert screen {0} doesn´t exist.", singleScreenScreenData.Item);
                                    }
                                }
                                else //2.B)- Agrego los comandos de "Main screen" a la lista de XElements
                                {
                                    ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(singleScreenScreenData));
                                }
                            }
                            if (!string.IsNullOrEmpty(messageToShow))
                            {
                                SingleScreenScreenData_Type singleScreenScreenMessage = new SingleScreenScreenData_Type();
                                singleScreenScreenMessage.itemElementNameField = ItemChoiceSingleScreenScreenData_Type.Print;
                                singleScreenScreenMessage.Item = messageToShow;
                                ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(singleScreenScreenMessage));
                            }
                        }
                        else
                        {
                            Log.Error("Commands of Screen {0} are null.", screenNumber);
                        }
                        //2.C)- Agrego los comandos INPUTBOX y TABLESELECTOR a la lista de XElements, si es que existen.
                        ListOfSingleScreenScreenData.AddRange(this.GetAdditionalCommands(screenType, optionalParameter));
                        foreach (SingleScreenScreenData_Type singleScreenScreenData in ListOfSingleScreenScreenData)
                        {
                            ListOfXElements.AddRange(this.ConvertNDCCommandToHTMLElements(singleScreenScreenData));
                        }
                        ///////////////////////////////////////////////////////
                        //3)- Cargo las pantallas de de memoria, si corresponde
                        ///////////////////////////////////////////////////////
                        if (addScreen && !string.IsNullOrEmpty(Entities.GlobalAppData.Instance.GlobalHtmlScreenData))
                        {
                            sr = new StringReader(Entities.GlobalAppData.Instance.GlobalHtmlScreenData);
                            XDocument xDocMemory = XDocument.Load(sr);
                            //3)- Agrego los elementos HTML desde la lista de XElements dentro del tag "body" y "head" del Doc XML leído desde el template.
                            foreach (XNode node in xDocumentMain.DescendantNodes())
                            {
                                if (node is XElement)
                                {
                                    //3.A)- Inserto elementos en el Head
                                    XElement element = (XElement)node;
                                    if (element.Name.LocalName.Equals("head"))
                                    {
                                        foreach (XNode child in xDocMemory.Root.Elements("head").DescendantNodes())
                                        {
                                            element.Add(child);
                                        }
                                    }
                                    //3.B)- Inserto elementos en el Body que esten contenidos en un DIV
                                    if (element.Name.LocalName.Equals("body"))
                                    {
                                        element.Add(xDocMemory.Descendants().Where(p => p.Name.LocalName.Equals("div")));
                                    }
                                }
                            }
                        }
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //4)- Agrego los elementos HTML desde la lista de XElements dentro del tag "body" y "head" del Doc XML leído desde el template.
                        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        foreach (XNode node in xDocumentMain.DescendantNodes())
                        {
                            if (node is XElement)
                            {
                                //4.A)- Inserto elementos en el Head
                                XElement element = (XElement)node;
                                if (element.Name.LocalName.Equals("head"))
                                {
                                    //Se carga desde archivo
                                    //element.AddFirst(new XElement("link", new XAttribute("rel", "stylesheet"), new XAttribute("type", "text/css"),
                                    //    new XAttribute("href", string.Format(@"{0}\style.css", this.ScreensPath))));//El archivo "Style.css" lo cargo aqui porque no lo toma desde el template.
                                }
                                //4.B)- Inserto elementos en el Body
                                if (element.Name.LocalName.Equals("body"))
                                {
                                    foreach (XElement xE in ListOfXElements)
                                    {
                                        element.Add(xE); //Agrego TODOS los comandos de pantalla
                                    }
                                    element.Add(this.BuildScreenAreasFDKs(keyMask)); //Agrego la activación de zonas de TS
                                }
                            }
                        }
                        //5)- Reemplazo los PLACEHOLDER
                        htmlToShow = xDocumentMain.ToString();
                        htmlToShow = this.ReplacePlaceHolder(htmlToShow);
                        //foreach (KeyValuePair<string, string> pair in this.dicOfPlaceholder)
                        //{
                        //    if (htmlToShow.Contains(pair.Key))
                        //        htmlToShow = htmlToShow.Replace(pair.Key, pair.Value);
                        //}
                        //6)- Guardo el XDocument en una variable Global.
                        Entities.GlobalAppData.Instance.GlobalHtmlScreenData = htmlToShow;
                    }
                    else
                    {
                        Log.Error("Screen: {0} not found.", screenNumber);
                        htmlToShow = this.GetMessageErrorForWebBrowser(string.Format("Screen {0} not found.", screenNumber)).ToString();
                        ret = false;
                    }
                    //7)- Adapto los comandos de espacio en blanco
                    //htmlToShow.Replace("&amp;nbsp;", "&nbsp;");
                }
                else
                {
                    ret = false;
                    Log.Error("Screen number is null.");
                }
            }
            catch (Exception ex)
            {
                ret = false;
                Log.Fatal(ex);
            }
            return ret;
        }

        /// <summary>
        /// Convierte los comandos de pantalla NDC a código HTML que luego se representará en el Web Browser.
        /// </summary>
        /// <param name="screenDataCommands"></param>
        /// <returns></returns>
        private List<XElement> ConvertNDCCommandToHTMLElements(SingleScreenScreenData_Type singleScreenScreenData)
        {
            //var xDocument = new XDocument(new XDocumentType("html", null, null, null));
            XElement xElement;
            List<XElement> ListOfxElements = new List<XElement>();
            int delayTimeDisplay = 0;
            try
            {
                //Log.Error("/-->Start of function<--| - Element type: {0}", singleScreenScreenData.ItemElementName));
                switch (singleScreenScreenData.ItemElementName)
                {
                    case ItemChoiceSingleScreenScreenData_Type.Voice: //Comando de VG
                        {
                            Voice_Type voice = (Voice_Type)singleScreenScreenData.Item;
                            //TODO
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.ClearScreen://Comando que realiza el borrado de pantalla
                        {
                            Entities.GlobalAppData.Instance.GlobalHtmlScreenData = string.Empty;
                            this.addScreen = false;
                            this.ScreenRowPositionPx = 0;
                            this.ScreenColumnPositionPx = 0;
                            this.ResetBlinkColorSettings();
                            this.PrimaryCharset = "SingleSizeAlphanumeric1";
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.SetCursor: //Comando que setea el posicionamiento del cursor de pantalla NDC
                        {
                            SetCursor_Type setCursor = (SetCursor_Type)singleScreenScreenData.Item;
                            this.ScreenRowPositionPx = this.GetScreenRowMapp(setCursor.Row);
                            this.ScreenColumnPositionPx = this.GetScreenColumnMapp(setCursor.Column);
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.ScreenBlinkingColorControl: //Comando para activar el parpadeo de texto
                        {
                            ScreenBlinkingColorControl_Type screenBlinkingColorControl = (ScreenBlinkingColorControl_Type)singleScreenScreenData.Item;
                            this.SetHtmlColorCode(screenBlinkingColorControl);
                            if (screenBlinkingColorControl.command1 == BlinkingCommand_Type.SetBlinkingOn || screenBlinkingColorControl.command2 == BlinkingCommand_Type.SetBlinkingOn || screenBlinkingColorControl.command3 == BlinkingCommand_Type.SetBlinkingOn)
                                this.BlinkId = "blink";
                            if (screenBlinkingColorControl.command1 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff || screenBlinkingColorControl.command2 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff || screenBlinkingColorControl.command3 == BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff)
                                ResetBlinkColorSettings();
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.SelectPrimaryCharSet: //Comando para setear el tipo de fuente primario
                        {
                            this.PrimaryCharset = singleScreenScreenData.Item.ToString();
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.SelectSecondaryCharSet: //Comando para setear el tipo de fuente secundario
                        {
                            this.SecondaryCharset = singleScreenScreenData.Item.ToString();
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.DrawPicture: //Comando para insertar una imágen JPG en pantalla
                        {
                            xElement = this.GetDivElementForText(0, 0);
                            string imagePath = this.GetIconFileName(singleScreenScreenData.Item.ToString(), this.Resolution.ToString().Substring(1));
                            //string resolutionFolder = this.Resolution.ToString().Substring(1);
                            //string screenPath = string.Format(@"{0}\Images\{1}\icon{2}.jpg", this.ScreensPath, resolutionFolder, singleScreenScreenData.Item);
                            xElement.Add(new XElement("p", new XElement("img", new XAttribute("border", "0"), new XAttribute("src", imagePath))));
                            ListOfxElements.Add(xElement);
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.Print: //Comando para insertar un texto en pantalla
                        {
                            xElement = this.GetDivElementForText(0, 0);
                            string text = singleScreenScreenData.Item.ToString().Replace(" ", "&nbsp;");
                            xElement.Add(new XElement("p", new XAttribute("id", this.BlinkId), new XAttribute("class", this.PrimaryCharset), new XAttribute("style", "letter-spacing:6px"), new XElement("font", new XAttribute("color", this.CurForegroundColor), text)));
                            ListOfxElements.Add(xElement);
                            this.ScreenColumnPositionPx = text.Length * 11; //Agrego 20 pixeles por cada caracter para evitar superposición por falta de cursor.
                            this.BlinkId = string.Empty;
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.ChangingDisplayInIdle:
                        {
                            if (int.TryParse(singleScreenScreenData.Item.ToString(), out delayTimeDisplay))
                            {
                                delayTimeDisplay = delayTimeDisplay * 100;
                            }
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.DrawImageFile: //Comando para insertar un video mpg en pantalla
                        {
                            string[] res = this.Resolution.ToString().Substring(1).Split('x');
                            string width = res[0];
                            string height = res[1];
                            xElement = this.GetDivElementForText(0, 0);
                            string videoPath = string.Format(@"{0}", singleScreenScreenData.Item);
                            xElement.Add(new XElement("p", new XElement("embed", new XAttribute("src", string.Format("{0}", singleScreenScreenData.Item)), new XAttribute("loop", "true"), new XAttribute("border", "0"), new XAttribute("ShowControls", "false"))));
                            ListOfxElements.Add(xElement);
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.PrintCardTrack1Name: //Comando para escribir el Track 1 por pantalla
                        {
                            xElement = this.GetDivElementForText(0, 0);
                            string text = "|_TRACK1_NAME_|";
                            xElement.Add(new XElement("p", new XAttribute("id", BlinkId), new XAttribute("class", this.PrimaryCharset), new XAttribute("style", "letter-spacing:6px"), new XElement("font", new XAttribute("color", this.CurForegroundColor), text)));
                            ListOfxElements.Add(xElement);
                            this.ScreenColumnPositionPx = text.Length * 20; //Agrego 20 pixeles por cada caracter para evitar superposición por falta de cursor.
                            this.BlinkId = string.Empty;
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.PixelCoordinate: //Comando que setea el posicionamiento del cursor de pantalla en pixeles
                        {
                            PixelCoordinate_Type pixelCoordinate = (PixelCoordinate_Type)singleScreenScreenData.Item;
                            this.ScreenRowPositionPx = pixelCoordinate.Row;
                            this.ScreenColumnPositionPx = pixelCoordinate.Column;
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
                            string value = mask.Replace("*", "").Replace("Z", "").Replace("T", "");
                            this.ScreenRowPositionPx += this.ScreenConfiguration.AddPxRowPosAmountInput;//Ajuste de altura
                            this.ScreenColumnPositionPx = this.ScreenColumnPositionPx + (290 - (sizeInputBox * 15));
                            //Agrego el inputBox
                            xElement = this.GetDivElementForInput("InputLayer", "NDCamountentry_box");
                            xElement.Add(new XElement("form", new XAttribute("name", "InputForm"), new XAttribute("ID", "Form1"), new XElement("INPUT", new XAttribute("id", "Text1"),
                                new XAttribute("value", string.Format("{0}", value)), new XAttribute("type", "text"), new XAttribute("size", sizeInputBox.ToString()), new XAttribute("name", "TextBox"),
                                new XAttribute("style", string.Format("color:{0}; letter-spacing:6px; TEXT-ALIGN:right; left: {1}px; top: {2}px", this.CurForegroundColor, this.ScreenColumnPositionPx, this.ScreenRowPositionPx)),
                                new XAttribute("class", this.PrimaryCharset), new XAttribute("onfocus", "blur()"))));
                            //Agrego las invocaciones a funciones Javascript
                            this.AddPlaceHolder("|_SET_FIELD_LENGTH_|", sizeMask.ToString());
                            this.AddPlaceHolder("|_SET_MONETARY_FIELD_|", MonetaryField);
                            this.AddPlaceHolder("|_SET_INFO_FIELD_MASK_|", mask);
                            ListOfxElements.Add(xElement);
                            //Agrego el teclado numérico en pantalla
                            if (this.ScreenConfiguration.KeyboardEntryMode == KeyboardEntryMode_Type.NumericKeyboardOnScreen)
                            {
                                XElement xElementKyb = this.GetNumericKeyboard();
                                if (xElementKyb != null)
                                    ListOfxElements.Add(xElementKyb);
                            }
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.InputInformationEntry: //Comando que inserta un InputBox para una pantalla de Information Entry
                        {
                            EntryModeAndBufferConfiguration_Type entryModeAndBufferConfiguration = (EntryModeAndBufferConfiguration_Type)singleScreenScreenData.Item;
                            string mask = string.Empty, type = string.Empty;
                            int sizeMask = 0, sizeInputBox = 0;
                            if (entryModeAndBufferConfiguration.DisplayAndBufferParameters == DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferB || 
                                entryModeAndBufferConfiguration.DisplayAndBufferParameters == DisplayAndBufferParameters_Type.DisplayDataKeyedIn_StoreDataInBufferC)
                            {
                                mask = "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
                                type = "text";
                                char[] ch = { '*', 'Z', 'T' };
                                sizeMask = mask.Split(ch).Length - 1;//El tamaño de la mascara lo determina la cantidad de patrones '*','Z'
                                sizeInputBox = mask.Length;
                                this.ScreenRowPositionPx += this.ScreenConfiguration.AddPxRowPosInfEntryInput;
                            }
                            else if (entryModeAndBufferConfiguration.DisplayAndBufferParameters == DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferB || 
                                entryModeAndBufferConfiguration.DisplayAndBufferParameters == DisplayAndBufferParameters_Type.DisplayX_StoreDataInBufferC)
                            {
                                mask = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
                                type = "password";
                                char[] ch = { '*', 'Z', 'T', 'X' };
                                sizeMask = mask.Split(ch).Length - 1;//El tamaño de la mascara lo determina la cantidad de patrones '*','Z'
                                sizeInputBox = mask.Length;
                                this.ScreenRowPositionPx += this.ScreenConfiguration.AddPxRowPosInfEntryInput;
                            }
                            else if (entryModeAndBufferConfiguration.DisplayAndBufferParameters == DisplayAndBufferParameters_Type.ITR)//Ajustes para el ITR
                            {
                                mask = "Z";
                                type = "text";
                                sizeMask = 1;
                                sizeInputBox = 32;
                                this.ScreenRowPositionPx += this.ScreenConfiguration.AddPxRowPosITRInput;//Ajuste de altura
                            }
                            string value = string.Empty;
                            //Agrego el inputBox
                            xElement = this.GetDivElementForInput("InputLayer", "NDCtext_box");
                            xElement.Add(new XElement("form", new XAttribute("name", "InputForm"), new XAttribute("ID", "Form1"), new XElement("INPUT", new XAttribute("id", "Text1"),
                                new XAttribute("value", string.Format("{0}", value)), new XAttribute("type", type), new XAttribute("size", sizeInputBox.ToString()), new XAttribute("name", "TextBox"),
                                new XAttribute("style", string.Format("color:{0}; TEXT-ALIGN:left; left: {1}px; top: {2}px", this.CurForegroundColor, this.ScreenColumnPositionPx, this.ScreenRowPositionPx)),
                                new XAttribute("class", "NDCFontSingleSizeAlphanumeric2_Entry_box"), new XAttribute("onfocus", "blur()")),
                                new XAttribute("dir", "RTL")));
                            //Agrego las invocaciones a funciones Javascript
                            this.AddPlaceHolder("|_SET_FIELD_LENGTH_|", sizeMask.ToString());//Cargo el PlaceHolder que se reemplazará por el path del archivo JavaScript
                            this.AddPlaceHolder("|_SET_INFO_FIELD_MASK_|", mask);//Cargo el PlaceHolder que se reemplazará por el path del archivo JavaScript
                            ListOfxElements.Add(xElement);
                            //Agrego el teclado numérico en pantalla
                            if (entryModeAndBufferConfiguration.KeyboardEntryMode == KeyboardEntryMode_Type.NumericKeyboardOnScreen ||
                                            this.ScreenConfiguration.KeyboardEntryMode == KeyboardEntryMode_Type.NumericKeyboardOnScreen)
                            {
                                XElement xElementKyb = this.GetNumericKeyboard();
                                if (xElementKyb != null)
                                    ListOfxElements.Add(xElementKyb);
                            }
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.InputPinEntry: //Comando que inserta un InputBox para una pantalla de Pin Entry
                        {
                            string mask = "****";
                            char[] ch = { '*', 'Z', 'T' };
                            int sizeMask = mask.Split(ch).Length - 1;//El tamaño de la mascara lo determina la cantidad de patrones '*','Z'
                            int sizeInputBox = mask.Length;
                            string value = string.Empty;
                            this.ScreenRowPositionPx += this.ScreenConfiguration.AddPxRowPosPinEntryInput;//Ajuste de altura
                            //Agrego el inputBox
                            xElement = this.GetDivElementForInput("PinInputLayer", "NDCpindisplay_box");
                            xElement.Add(new XElement("form", new XAttribute("name", "InputForm"), new XAttribute("ID", "Form1"), new XElement("INPUT", new XAttribute("id", "TextPin"),
                                new XAttribute("value", string.Format("{0}", value)), new XAttribute("type", "text"), new XAttribute("size", sizeInputBox.ToString()), new XAttribute("name", "TextBox"),
                                new XAttribute("style", string.Format("color:{0}; TEXT-ALIGN:left; left: {1}px; top: {2}px", this.CurForegroundColor, this.ScreenColumnPositionPx, this.ScreenRowPositionPx)),
                                new XAttribute("class", "NDCFontSingleSizeAlphanumeric2_Entry_box")),
                                new XAttribute("dir", "RTL")));
                            //Agrego las invocaciones a funciones Javascript
                            this.AddPlaceHolder("|_SET_FIELD_LENGTH_|", sizeMask.ToString());//Cargo el PlaceHolder que se reemplazará por el path del archivo JavaScript
                            this.AddPlaceHolder("|_SET_INFO_FIELD_MASK_|", mask);//Cargo el PlaceHolder que se reemplazará por el path del archivo JavaScript
                            ListOfxElements.Add(xElement);
                            //Agrego el teclado numérico en pantalla
                            if (this.ScreenConfiguration.KeyboardEntryMode == KeyboardEntryMode_Type.NumericKeyboardOnScreen)
                            {
                                XElement xElementKyb = this.GetNumericKeyboard();
                                if (xElementKyb != null)
                                    ListOfxElements.Add(xElementKyb);
                            }
                            break;
                        }
                    case ItemChoiceSingleScreenScreenData_Type.TableSelector: //Comando que inserta una Tabla de seleccción en pantalla.
                        {
                            List<TableItem> listOfTableItem = singleScreenScreenData.Item as List<TableItem>;
                            string buttonType = "btn-outline-secondary";
                            string buttonText = "Entregado";
                            xElement = new XElement("table", new XAttribute("id", "table"), new XAttribute("class", "table table-bordered"), new XAttribute("style", "width:100%"));
                            XElement xElementThead = new XElement("tr");
                            XElement xElementTbody = new XElement("tbody");
                            XElement xElementItems = new XElement("tr");
                            System.Reflection.PropertyInfo[] listaPropiedades = typeof(TableItem).GetProperties();
                            foreach (System.Reflection.PropertyInfo propiedad in listaPropiedades)
                            {
                                xElementThead.Add(new XElement("th", new XAttribute("scope", "col"), propiedad.Name));
                            }
                            foreach(TableItem item in listOfTableItem)
                            {
                                xElementItems.Add(new XElement("th", new XAttribute("scope", "row"), item.ID));
                                xElementItems.Add(new XElement("td", item.Nombre));
                                xElementItems.Add(new XElement("td", item.Documento));
                                switch(item.Estado)
                                {
                                    case Const.EnvelopeState.delivered:
                                        buttonType = "btn-outline-secondary";
                                        buttonText = "Entregado";
                                        break;
                                    case Const.EnvelopeState.Empty:
                                        buttonType = "btn-outline-info";
                                        buttonText = "Vacío";
                                        break;
                                    case Const.EnvelopeState.Loaded:
                                        buttonType = "btn-outline-primary";
                                        buttonText = "Cargado";
                                        break;
                                }
                                xElementItems.Add(new XElement("td", new XElement("button", new XAttribute("type", "button"), new XAttribute("onclick", string.Format("tableButtonPress({0})", item.ID)), new XAttribute("class", buttonType)), buttonText));
                                xElementItems.Add(new XElement("td", item.Fecha));
                                xElementItems.Add(new XElement("td", item.Accion));
                                xElementTbody.Add(xElementItems);
                                xElementItems = new XElement("tr");
                            }    
                            xElement.Add(new XElement("thead", new XElement(xElementThead)));
                            xElement.Add(xElementTbody);
                            ListOfxElements.Add(xElement);
                            break;
                        }
                    default:
                        {
                            Log.Error("Unknown screen element : {0}", singleScreenScreenData.ItemElementName);
                            break;
                        }
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
            return ListOfxElements;
        }

        private string GetIconFileName(string iconName, string resolutionFolder)
        {
            string fileName = string.Empty;
            string iconPath = string.Format(@"{0}\Images\{1}\icon{2}.png", this.ScreensPath, resolutionFolder, iconName);
            try
            {
                if (!File.Exists(iconPath))
                {
                    iconPath = string.Format(@"{0}\Images\{1}\icon{2}.jpg", this.ScreensPath, resolutionFolder, iconName);
                    if (!File.Exists(iconPath))
                    {
                        iconPath = string.Format(@"{0}\Images\{1}\icon{2}.bmp", this.ScreensPath, resolutionFolder, iconName);
                        if (!File.Exists(iconPath))
                        {
                            iconPath = string.Format(@"{0}\Images\{1}\icon{2}.gif", this.ScreensPath, resolutionFolder, iconName);
                            if (!File.Exists(iconPath))
                            {
                                iconPath = string.Format(@"{0}\Images\{1}\icon{2}.pcx", this.ScreensPath, resolutionFolder, iconName);
                            }
                        }
                    }
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
            return iconPath;
        }

        private XElement GetNumericKeyboard()
        {
            XElement xElement = null;
            string templateFileName;
            try
            {
                templateFileName = string.Format(@"{0}\NumericKeyboardTemplate.htm", this.ScreensPath);
                if (File.Exists(templateFileName))
                {
                    xElement = XElement.Load(templateFileName);
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
            return xElement;
        }

        private string GetScreenTemplateName(ScreenType screenType)
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
                case ScreenType.TableSelector:
                    {
                        templateFileName = "NDCTableSelectorTemplate";
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
        private List<SingleScreenScreenData_Type> GetAdditionalCommands(ScreenType screenType, object optionalParameter)
        {
            List<SingleScreenScreenData_Type> ListOfCommands = new List<SingleScreenScreenData_Type>();
            string entryMask = string.Empty;
            EntryModeAndBufferConfiguration_Type entryModeAndBufferConfiguration_Type;
            switch (screenType)
            {
                case ScreenType.AmountEntry:
                    {
                        entryMask = optionalParameter as string;
                        if (this.DiccScreenData.ContainsKey(entryMask))
                        {
                            ScreenData_Type screenTemplateCommands = this.DiccScreenData[entryMask];
                            foreach (SingleScreenScreenData_Type sSd in screenTemplateCommands.Command)
                            {
                                if (sSd.ItemElementName == ItemChoiceSingleScreenScreenData_Type.Print)
                                    sSd.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InputAmountEntry; //Cambio el comando para luego insertar un input data.
                                ListOfCommands.Add(sSd);
                            }
                        }
                        else
                            Log.Error("Entry mask not found: {0}", entryMask);
                        break;
                    }
                case ScreenType.InformationEntry:
                    {
                        entryModeAndBufferConfiguration_Type = (EntryModeAndBufferConfiguration_Type)optionalParameter;
                        SingleScreenScreenData_Type sSd = new SingleScreenScreenData_Type();
                        sSd.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InputInformationEntry;
                        sSd.Item = entryModeAndBufferConfiguration_Type;
                        ListOfCommands.Add(sSd);
                        break;
                    }
                case ScreenType.PinEntry:
                    {
                        string track1Name = optionalParameter as string;//TODO: aplicar conf.
                        SingleScreenScreenData_Type sSd1 = new SingleScreenScreenData_Type();
                        sSd1.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InputPinEntry;
                        this.AddPlaceHolder("|_TRACK1_NAME_|", track1Name);
                        ListOfCommands.Add(sSd1);
                        break;
                    }
                case ScreenType.TableSelector:
                    {
                        List<TableItem> listOfTableItem = optionalParameter as List<TableItem>;
                        SingleScreenScreenData_Type sSd1 = new SingleScreenScreenData_Type();
                        sSd1.ItemElementName = ItemChoiceSingleScreenScreenData_Type.TableSelector;
                        sSd1.Item = listOfTableItem;
                        ListOfCommands.Add(sSd1);
                        break;
                    }
            }
            return ListOfCommands;
        }

        public string ReplacePlaceHolder(string htmlToShow)
        {
            string ret = htmlToShow;
            foreach (KeyValuePair<string, string> pair in this.DicOfPlaceholder)
            {
                if (ret.Contains(pair.Key))
                    ret = ret.Replace(pair.Key, pair.Value);
            }
            return ret;
        }

        public void AddPlaceHolder(string placeHolder, string data)
        {
            if (this.DicOfPlaceholder.ContainsKey(placeHolder))
            {
                this.DicOfPlaceholder.Remove(placeHolder);
            }
            this.DicOfPlaceholder.Add(placeHolder, data);
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
                xElement = new XElement("div", new XAttribute("style", string.Format("position: absolute; left: {0}px; top: {1}px;", this.ScreenColumnPositionPx + addScreenColumnPosition, this.ScreenRowPositionPx + addScreenRowPosition)));
            else
                xElement = new XElement("div", new XAttribute("style", string.Format("position: absolute; left: {0}px; top: {1}px; background-color:{2}", this.ScreenColumnPositionPx, this.ScreenRowPositionPx, this.CurBackgroundColor)));
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
            string resolutionFolder = this.Resolution.ToString().Substring(1);
            XElement xTable = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", "ScreenTable"));
            if (keyMask != null)
            {
                if (keyMask.FDKF)//F
                {
                    XElement xTableF = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F1_{0}", resolutionFolder)), new XAttribute("ID", "F"));
                    xTableF.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableF);
                }
                if (keyMask.FDKG)//G
                {
                    XElement xTableG = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F2_{0}", resolutionFolder)), new XAttribute("ID", "G"));
                    xTableG.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableG);
                }
                if (keyMask.FDKH)//H 
                {
                    XElement xTableH = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F3_{0}", resolutionFolder)), new XAttribute("ID", "H"));
                    xTableH.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableH);
                }
                if (keyMask.FDKI)//I
                {
                    XElement xTableI = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F4_{0}", resolutionFolder)), new XAttribute("ID", "I"));
                    xTableI.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableI);
                }
                if (keyMask.FDKA)
                {
                    XElement xTableA = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F9_{0}", resolutionFolder)), new XAttribute("ID", "A"));
                    xTableA.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableA);
                }
                if (keyMask.FDKB)
                {
                    XElement xTableB = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F10_{0}", resolutionFolder)), new XAttribute("ID", "B"));
                    xTableB.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableB);
                }
                if (keyMask.FDKC)
                {
                    XElement xTableC = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F11_{0}", resolutionFolder)), new XAttribute("ID", "C"));
                    xTableC.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableC);
                }
                if (keyMask.FDKD)
                {
                    XElement xTableD = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("F12_{0}", resolutionFolder)), new XAttribute("ID", "D"));
                    xTableD.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                    xTable.Add(xTableD);
                }
                //if (keyMask.CANCEL)
                //{
                //    XElement xTableD = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("CANCEL_{0}", resolutionFolder)), new XAttribute("ID", "CANCEL"));
                //    xTableD.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                //    xTable.Add(xTableD);
                //}
                //if (keyMask.ENTER)
                //{
                //    XElement xTableD = new XElement("table", new XAttribute("width", "100%"), new XAttribute("class", string.Format("ENTER_{0}", resolutionFolder)), new XAttribute("ID", "ENTER"));
                //    xTableD.Add(new XElement("tr", new XElement("td", new XAttribute("width", "100%"), new XElement("div", new XAttribute("class", "button")))));
                //    xTable.Add(xTableD);
                //}
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
             catch (Exception ex) { Log.Fatal(ex); }
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
                if (mappedRow != null)
                {
                    if (!int.TryParse(mappedRow.NewPositionInPixels, out ret))
                    {
                        Log.Error("Row: {0} is not numeric.", rowNumber);
                    }
                }
                else
                {
                    Log.Error("Row: {0} doesn´t exist.", rowNumber);
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
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
                //if (!int.TryParse(mappColumn.NewPositionInPixels, out ret))
                //{
                //    //TODO: show error
                //}
                if (mappColumn != null)
                {
                    if (!int.TryParse(mappColumn.NewPositionInPixels, out ret))
                    {
                        Log.Error("Column: {0} is not numeric.", mappColumn);
                    }
                }
                else
                {
                    Log.Error("Column: {0} doesn´t exist.", mappColumn);
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        /// <summary>
        /// Setea a default el color y parpadeo.
        /// </summary>
        private void ResetBlinkColorSettings()
        {
            //this.isBlinkOn = false;
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
            screenColor.HtmlColorCode = "#C2BCBC";
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
                            //this.isBlinkOn = true;
                            break;
                        }
                    case BlinkingCommand_Type.SetBlinkingOff:
                        {
                            //this.isBlinkOn = false;
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
                    case BlinkingCommand_Type.WhiteDefaultBackground_HighIntensity:
                        {
                            this.CurDefaultBackgroundColor = screenColor.HtmlColorCode;
                            break;
                        }
                }
            }
             catch (Exception ex) { Log.Fatal(ex); }
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
             catch (Exception ex) { Log.Fatal(ex); }

            return xDocument;
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
            Log.Error("Invalid screen name pattern: {0}", screenName);
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
        //        if (Utilities.Utils.ByteArrayToBin(readCond, out arrByte))
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
