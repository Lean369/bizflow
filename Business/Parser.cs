using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.VisualBasic;
using Entities;
using System.IO;

namespace Business
{
    public class Parser
    {
        public List<AnonymousStateTableData_Type> ExtensionStateTable;
        public List<string> PendingStateTable;

        public Parser()
        {
            this.ExtensionStateTable = new List<AnonymousStateTableData_Type>();
            this.PendingStateTable = new List<string>();
        }

        //1)- SCREENS PARSE
        #region "SCREENS DATA"
        public Dictionary<string, object> ScreenDataMessageParser(string msg)
        {
            Dictionary<string, object> dicOfObjectData = new Dictionary<string, object>();
            string screenName = string.Empty;
            string[] DataInPut;
            string temp = string.Empty;
            //bool isRscreen = false;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 3)
                {
                    for (int i = 4; i < DataInPut.Length; i++)
                    {
                        if (DataInPut[i].Length > 2)
                        {
                            screenName = DataInPut[i].Substring(0, 3);
                            if (screenName.Equals("R00"))
                            {
                                //TEST
                            }
                            if (dicOfObjectData.ContainsKey(screenName))
                            {
                                dicOfObjectData.Remove(screenName);
                            }
                            if (screenName.Equals("R00") || screenName.Equals("R01"))//Manejo de pantallas "R"
                            {
                                SimulatedPrePrintedReceiptScreen_Type simulatedPrePrintedReceiptScreen = new SimulatedPrePrintedReceiptScreen_Type();
                                simulatedPrePrintedReceiptScreen.ScreenNumber = screenName;
                                simulatedPrePrintedReceiptScreen.SimulatedPrePrintedReceiptData = this.GetPrinterData(DataInPut[i].Substring(3));
                                dicOfObjectData.Add(screenName, simulatedPrePrintedReceiptScreen);
                            }
                            else
                            {
                                string screenData = DataInPut[i].Substring(3);
                                if (screenData.Contains("<!DOCTYPE html []>"))//Manejo para pantallas HTML (Genera directamente un archivo *.htm)
                                {
                                    this.GenerateHTMLScreenfile(screenName, screenData);
                                }
                                else//Manejo para pantallas NDC
                                {
                                    string path = this.GetScreenFolder(screenName);
                                    string fileNameAndPath = string.Format(@"{0}\{1}.htm", path, screenName);
                                    if (File.Exists(fileNameAndPath))
                                        File.Delete(fileNameAndPath);
                                    dicOfObjectData.Add(screenName, this.GetScreenData(screenData));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw ex; }
            return dicOfObjectData;
        }

        public string GetScreenFolder(string screenName)
        {
            string path = string.Empty;
            int i = 0;
            if (int.TryParse(screenName, out i))//Carpeta numérica, se padea  a 3
                path = string.Format(@"{0}Screens\Screens\{1}", Const.appPath, screenName.Substring(0, 1).PadRight(3, '0'));
            else
                path = string.Format(@"{0}Screens\Screens\{1}", Const.appPath, screenName.Substring(0, 1));//Carpeta alfabética, no se padea
            return path;
        }

        private void GenerateHTMLScreenfile(string screenName, string screenData)
        {
            string path;
            try
            {
                path = this.GetScreenFolder(screenName);
                string fileNameAndPath = string.Format(@"{0}\{1}.htm", path, screenName);
                if (!Directory.Exists(string.Format(@"{0}", path)))
                    Directory.CreateDirectory(string.Format(@"{0}", path));
                Utilities.Utils.WriteUTF8FileStream(fileNameAndPath, screenData);
            }
            catch (Exception ex) { throw ex; }
        }

        private ScreenData_Type GetScreenData(string msg)
        {
            List<SingleScreenScreenData_Type> listOfScreenData = new List<SingleScreenScreenData_Type>();
            ScreenData_Type screenData = new ScreenData_Type();
            SingleScreenScreenData_Type singleScreenScreenData;
            StringBuilder sb;
            string aux = string.Empty, print = string.Empty;
            int k = 0;
            int pos = 0;
            bool addScreenCommand = false;
            bool addPrintScreenCommand = false;
            List<ScreenColorMapping> listScreenColorMapping;
            try
            {
                //char[] control = new char[] { (char)0x0C, (char)0x0E, (char)0x0F, (char)0x09, (char)0x11, (char)0x12, (char)0x1B, (char)0x0D, (char)0x0B };
                //string[] commands = msg.Split(control);
                sb = new StringBuilder(msg);
                if (!ScreenColorMapping.GetScreenColorMapping(out listScreenColorMapping))
                    throw new Exception(string.Format("ScreenColorMapping XML file error."));

                for (pos = 0; pos < msg.Length; pos++)
                {
                    singleScreenScreenData = new SingleScreenScreenData_Type();
                    //A)- Print command 
                    if (addPrintScreenCommand)
                    {
                        if (sb[pos] >= 0x20) //Si no es caracter de control, acumulo 
                        {
                            print = string.Format("{0}{1}", print, sb[pos]);
                            if (msg.Length == pos + 1)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.Print;
                                singleScreenScreenData.Item = print;
                                print = string.Empty;
                                listOfScreenData.Add(singleScreenScreenData);
                                singleScreenScreenData = new SingleScreenScreenData_Type();
                                addPrintScreenCommand = false;
                            }
                        }
                        else
                        {
                            if(!string.IsNullOrEmpty(print))//parche para que no agregue comandos de texto vacios
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.Print;
                                singleScreenScreenData.Item = print;
                                print = string.Empty;
                                listOfScreenData.Add(singleScreenScreenData);
                                singleScreenScreenData = new SingleScreenScreenData_Type();
                                addPrintScreenCommand = false;
                            }
                        }
                    }
                    //B)- Screen commands (busco caracteres de control específicos)
                    switch (sb[pos])
                    {
                        case (char)(0x0C): //FF (Clear the screen and positions the cursor in the top left hand corner of the screen)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.ClearScreen;
                                singleScreenScreenData.Item = new EmptyElement_Type();
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x0E): //SO (Inserts the screen called by the next 3, 5 or 6 characters.)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.InsertScreen;
                                singleScreenScreenData.Item = string.Format("{0}{1}{2}", sb[pos + 1], sb[pos + 2], sb[pos + 3]);
                                pos = pos + 3;
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x0F): //SI (Sets the cursor to the position indicated by the next two characters. Row selected first, column selected second.)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.SetCursor;
                                SetCursor_Type setCursor = new SetCursor_Type();
                                k = GetScreenCursor(sb[pos + 1].ToString());
                                setCursor.Row = (ScreenRow_Type)k;
                                k = GetScreenCursor(sb[pos + 2].ToString());
                                setCursor.Column = (ScreenColumn_Type)k;
                                singleScreenScreenData.Item = setCursor;
                                if (sb.Length > pos + 3)
                                {
                                    addPrintScreenCommand = sb[pos + 3] >= 0x20 ? true : false; //Si no es caracter de control, agrego texto
                                }
                                pos = pos + 2;
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x09): //HT (Causes the name encoded on Track 1 of the card to be displayed,)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.PrintCardTrack1Name;
                                singleScreenScreenData.Item = new EmptyElement_Type();
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x11): //DC1 (Enable video)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.EnableVideo;
                                singleScreenScreenData.Item = new EmptyElement_Type();
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x12): //DC2 (Disable video)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.DisableVideo;
                                singleScreenScreenData.Item = new EmptyElement_Type();
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x1B): //ESC (Control character)
                            {
                                switch (sb[pos + 1])
                                {
                                    case (char)(0x28): //( (Select Primary CharSet)
                                        {
                                            singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.SelectPrimaryCharSet;
                                            if (int.TryParse(sb[pos + 2].ToString(), out k))
                                            {
                                                pos = pos + 1;
                                                singleScreenScreenData.Item = (CharSet_Type)k;
                                            }
                                            addScreenCommand = true;
                                            break;
                                        }
                                    case (char)(0x29): //) (Select Secondary CharSet)
                                        {
                                            singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.SelectSecondaryCharSet;
                                            singleScreenScreenData.Item = (CharSet_Type)sb[pos + 1];
                                            pos = pos + 1;
                                            addScreenCommand = true;
                                            break;
                                        }
                                    case (char)(0x50): //P 
                                        {
                                            //pos = pos + 1;
                                            //string s = sb[pos + 2].ToString();
                                            switch (sb[pos + 2])
                                            {
                                                case (char)(0x30): //0 (Voice)
                                                    {
                                                        singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.Voice;
                                                        singleScreenScreenData.Item = new Voice_Type();
                                                        addScreenCommand = true;
                                                        break;
                                                    }
                                                case (char)(0x31): //1 (Logo control)
                                                    {
                                                        singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.DisplayLogo;
                                                        singleScreenScreenData.Item = string.Format("{0}{1}", sb[pos + 2], sb[pos + 3]);
                                                        pos = pos + 3;
                                                        addScreenCommand = true;
                                                        break;
                                                    }
                                                case (char)(0x32): //2 (Picture control)
                                                    {
                                                        singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.DrawPicture;
                                                        aux = string.Empty;
                                                        //Acumula caracteres distintos a 'ESC'
                                                        int l;
                                                        for (l = pos + 3; !sb[l].Equals((char)0x1B) & l < sb.Length; l++)
                                                        {
                                                            aux = string.Format("{0}{1}", aux, sb[l]);
                                                        }
                                                        pos = l + 1;//+1 es un Parche para que no agregue la barra como texto
                                                        singleScreenScreenData.Item = aux;
                                                        addScreenCommand = true;
                                                        break;
                                                    }
                                                case (char)(0x45): //E (Draw image / video file)
                                                    {
                                                        singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.DrawImageFile;
                                                        aux = string.Empty;
                                                        //Acumula caracteres distintos a 'ESC'
                                                        int l;
                                                        for (l = pos + 3; !sb[l].Equals((char)0x1B) & l < sb.Length; l++)
                                                        {
                                                            aux = string.Format("{0}{1}", aux, sb[l]);
                                                        }
                                                        pos = l;
                                                        singleScreenScreenData.Item = aux;
                                                        addScreenCommand = true;
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        addScreenCommand = false;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case (char)(0x5B): //[ (Screen blinking and colour control.)
                                        {
                                            aux = string.Empty;
                                            string[] controls;
                                            char lastCharacter = '0';
                                            //Acumula caracteres distintos a 'm' y 'z' y 'p'
                                            for (int l = pos + 2; (!sb[l].Equals((char)0x6D) & !sb[l].Equals((char)0x7A) & !sb[l].Equals((char)0x70)) & l < sb.Length; l++)
                                            {
                                                aux = string.Format("{0}{1}", aux, sb[l]);
                                                lastCharacter = sb[l + 1];
                                            }
                                            if (lastCharacter.Equals((char)0x6D) && !string.IsNullOrEmpty(aux)) //m
                                            {
                                                ScreenBlinkingColorControl_Type screenBlinkingColorControl = new ScreenBlinkingColorControl_Type();
                                                ScreenColorMapping screenColorMapping = new ScreenColorMapping();
                                                controls = aux.Split(';');
                                                switch (controls.Length)
                                                {
                                                    case 1:
                                                        {
                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[0], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command1 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[0], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command1 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[1], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command2 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            screenBlinkingColorControl.command2Specified = true;
                                                            break;
                                                        }
                                                    case 3:
                                                        {

                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[0], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command1 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[1], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command2 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            screenBlinkingColorControl.command2Specified = true;
                                                            screenColorMapping = listScreenColorMapping.FirstOrDefault(x => x.NdcColorCode.Equals(controls[2], StringComparison.Ordinal));
                                                            screenBlinkingColorControl.command3 = screenColorMapping != null ? screenColorMapping.NdcColorName : BlinkingCommand_Type.TransparentBackground;
                                                            screenBlinkingColorControl.command3Specified = true;
                                                            break;
                                                        }

                                                }
                                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.ScreenBlinkingColorControl;
                                                singleScreenScreenData.Item = screenBlinkingColorControl;
                                                addScreenCommand = true;
                                            }
                                            if (lastCharacter.Equals((char)0x7A) && !string.IsNullOrEmpty(aux)) //z (Changing Display In Idle)
                                            {
                                                //TODO:
                                                //For example, user‐defined screens 20 and 21 will be displayed
                                                //alternately for 10 and 15 seconds if the following idle screen is
                                                //defined:
                                                //S0 020 ESC [100z S0 021 ESC [150z
                                                //An idle screen delay sequence resets the following screen controls:
                                                //● Cursor position reset to ‘@@’
                                                //● Blink control off
                                                //● Character set – single size alpha
                                                //● Left margin set to left‐most column
                                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.ChangingDisplayInIdle;
                                                singleScreenScreenData.Item = aux;
                                                addScreenCommand = true;
                                            }
                                            if (lastCharacter.Equals((char)0x70) && !string.IsNullOrEmpty(aux)) //p (Left Margin Control)
                                            {
                                                //TODO:
                                                //The column position of the cursor following a CR control character
                                                //can be set by using the following control sequence. The default is
                                                //column 00.
                                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.LeftMargin;
                                                singleScreenScreenData.Item = aux;
                                                addScreenCommand = true;
                                            }
                                            break;
                                        }
                                    case (char)(0x58): //X PixelCoordinates
                                        {
                                            singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.PixelCoordinate;
                                            PixelCoordinate_Type pixelCoordinate = new PixelCoordinate_Type();
                                            int l = 0;
                                            if (sb.Length > pos + 8)
                                            {
                                                if (int.TryParse(string.Format("{0}{1}{2}{3}", sb[pos + 2], sb[pos + 3], sb[pos + 4], sb[pos + 5]), out l))
                                                    pixelCoordinate.Column = l;
                                                l = 0;
                                                if (int.TryParse(string.Format("{0}{1}{2}{3}", sb[pos + 6], sb[pos + 7], sb[pos + 8], sb[pos + 9]), out l))
                                                    pixelCoordinate.Row = l;
                                                pos = pos + 9;
                                                addPrintScreenCommand = sb[pos] >= 0x20 ? true : false; //Si no es caracter de control, agrego texto
                                                singleScreenScreenData.Item = pixelCoordinate;
                                                addScreenCommand = true;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            addScreenCommand = false;
                                            break;
                                        }
                                }
                                break;
                            }
                        case (char)(0x0D): //CR (Causes the cursor to be moved to the character position specified by the current left‐hand margin on the following line.)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.LeftMargin;
                                singleScreenScreenData.Item = string.Format("{0}{1}", sb[pos + 1], sb[pos + 2]);
                                pos = pos + 2;
                                addScreenCommand = true;
                                break;
                            }
                        case (char)(0x0B): //VT (Causes the next character to be displayed from the secondary character set.)
                            {
                                singleScreenScreenData.ItemElementName = ItemChoiceSingleScreenScreenData_Type.PrintUsingSecondaryCharSet;
                                singleScreenScreenData.Item = string.Format("{0}{1}", sb[pos + 1], sb[pos + 2]);
                                pos = pos + 2;
                                addScreenCommand = true;
                                break;
                            }
                        default:
                            {
                                addScreenCommand = false;
                                break;
                            }
                    }
                    //Solo agrega comandos procesados correctamente
                    if (addScreenCommand)
                        listOfScreenData.Add(singleScreenScreenData);
                }
                //Add to array
                screenData.Command = listOfScreenData.ToArray();
                return screenData;
            }
            catch (Exception ex) { throw new Exception(string.Format("GetScreenData(): POS {0} - {1}", pos, ex.Message)); }
        }

        private int GetScreenCursor(string dataIn)
        {
            int ret = 0;
            try
            {
                switch (dataIn)
                {
                    case "@": { ret = 0; break; }
                    case "A": { ret = 1; break; }
                    case "B": { ret = 2; break; }
                    case "C": { ret = 3; break; }
                    case "D": { ret = 4; break; }
                    case "E": { ret = 5; break; }
                    case "F": { ret = 6; break; }
                    case "G": { ret = 7; break; }
                    case "H": { ret = 8; break; }
                    case "I": { ret = 9; break; }
                    case "J": { ret = 10; break; }
                    case "K": { ret = 11; break; }
                    case "L": { ret = 12; break; }
                    case "M": { ret = 13; break; }
                    case "N": { ret = 14; break; }
                    case "O": { ret = 15; break; }
                    case "0": { ret = 16; break; }
                    case "1": { ret = 17; break; }
                    case "2": { ret = 18; break; }
                    case "3": { ret = 19; break; }
                    case "4": { ret = 20; break; }
                    case "5": { ret = 21; break; }
                    case "6": { ret = 22; break; }
                    case "7": { ret = 23; break; }
                    case "8": { ret = 24; break; }
                    case "9": { ret = 25; break; }
                    case ":": { ret = 26; break; }
                    case ";": { ret = 27; break; }
                    case "<": { ret = 28; break; }
                    case "=": { ret = 29; break; }
                    case ">": { ret = 30; break; }
                    case "?": { ret = 31; break; }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        #endregion "SCREENS"

        //2)- STATES PARSE
        #region "STATES TABLE"
        public List<StateTable_Type> StateTablesMessageParser(string msg)
        {
            StateTable_Type stateTable;
            List<StateTable_Type> stateTables = new List<StateTable_Type>();
            string[] DataInPut;
            string temp = string.Empty;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 3)
                {
                    for (int i = 4; i < DataInPut.Length; i++)
                    {
                        if (DataInPut[i].Length > 27)
                            if (this.GetStateTable(DataInPut[i], out stateTable))
                            {
                                stateTables.Add(stateTable);
                            }
                    }
                }
                //Try insert pending states tables
                if (this.PendingStateTable.Count != 0 && this.ExtensionStateTable.Count != 0)
                {
                    foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                    {
                        foreach (string aux in this.PendingStateTable)
                        {
                            if (aux.Substring(aux.Length - 3, 3).Equals(aSt.StateType, StringComparison.Ordinal))
                                temp = aux;
                        }
                    }
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (this.GetStateTable(temp, out stateTable))
                        {
                            stateTables.Add(stateTable);
                            this.PendingStateTable.Remove(temp);
                        }
                    }
                }
            }
            catch (Exception ex) { throw ex; }
            return stateTables;
        }

        private bool GetStateTable(string msg, out StateTable_Type stateTable)
        {
            int buffer = 0;
            bool ret = false;
            stateTable = new StateTable_Type();
            try
            {
                stateTable.StateNumber = msg.Substring(0, 3);
                //if (stateTable.StateNumber.Equals("488", StringComparison.Ordinal))
                //{
                //    string q;
                //}
                stateTable.Item = new object();
                stateTable.ItemElementName = (ItemChoiceStateTable_Type)this.GetItemChoiceStateTable(msg);
                switch (stateTable.ItemElementName)
                {
                    case ItemChoiceStateTable_Type.AnonymousExtensionStateTableData: //Z - Extension
                        {
                            AnonymousStateTableData_Type anonymousStateTableData = new AnonymousStateTableData_Type();
                            anonymousStateTableData.StateType = msg.Substring(0, 3);
                            anonymousStateTableData.UnparsedStateTableData = msg.Substring(4);
                            this.ExtensionStateTable.Add(anonymousStateTableData);
                            ret = false;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CardReadStateTableData: //A - Card read state
                        {
                            CardReadStateTableData_Type cardReadStateTableData = new CardReadStateTableData_Type();
                            cardReadStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            cardReadStateTableData.GoodReadNextState = msg.Substring(7, 3); //Table entry: 3
                            cardReadStateTableData.ErrorScreenNumber = msg.Substring(10, 3); //Table entry: 4
                            cardReadStateTableData.ReadCondition1 = this.GetReadCondition(msg.Substring(13, 3)); //Table entry: 5
                            cardReadStateTableData.ReadCondition2 = this.GetReadCondition(msg.Substring(16, 3)); //Table entry: 6
                            cardReadStateTableData.ReadCondition3 = this.GetReadCondition(msg.Substring(19, 3)); //Table entry: 7
                            if (msg.Substring(22, 3).Equals("001", StringComparison.Ordinal)) //Table entry: 8
                                cardReadStateTableData.CardReturnFlag = CardReturnFlag_Type.AsSpecifiedByTransactionReply;
                            else
                                cardReadStateTableData.CardReturnFlag = CardReturnFlag_Type.EjectCardImmediately;
                            cardReadStateTableData.NoFitMatchNextState = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = cardReadStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.PINEntryStateTableData: //B - Pin entry state
                        {
                            PINEntryStateTableData_Type pINEntryStateTableData = new PINEntryStateTableData_Type();
                            pINEntryStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            pINEntryStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            pINEntryStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            pINEntryStateTableData.LocalPINCheckGoodPINNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            pINEntryStateTableData.LocalPINCheckMaximumBadPINNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            pINEntryStateTableData.LocalPINCheckErrorScreenNumber = msg.Substring(19, 3); //Table entry: 7
                            pINEntryStateTableData.RemotePINCheckNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            int localPINCheckMaximumPINRetries = 0;
                            int.TryParse(msg.Substring(25, 3), out localPINCheckMaximumPINRetries);
                            pINEntryStateTableData.LocalPINCheckMaximumPINRetries = localPINCheckMaximumPINRetries; //Table entry: 9
                            stateTable.Item = pINEntryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.EnvelopeDispenserStateTableData: //C - Envelope dispenser state
                        {
                            EnvelopeDispenserStateTableData_Type envelopeDispenserStateTableData = new EnvelopeDispenserStateTableData_Type();
                            envelopeDispenserStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            envelopeDispenserStateTableData.NextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            envelopeDispenserStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            envelopeDispenserStateTableData.ErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            envelopeDispenserStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            buffer = 0;
                            int.TryParse(msg.Substring(19, 3), out buffer);
                            envelopeDispenserStateTableData.EnvelopeOperationMode = (EnvelopeOperationMode_Type) buffer; //Table entry: 7
                            envelopeDispenserStateTableData.Item = new EmptyElement_Type(); //Table entry: 8
                            envelopeDispenserStateTableData.Item1 = new EmptyElement_Type(); //Table entry: 9
                            stateTable.Item = envelopeDispenserStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.PreSetOperationCodeBufferStateTableData: //D - Pre set operation code buffer state
                        {
                            PreSetOperationCodeBufferStateTableData_Type preSetOperationCodeBufferStateTableData = new PreSetOperationCodeBufferStateTableData_Type();
                            preSetOperationCodeBufferStateTableData.NextStateNumber = msg.Substring(4, 3); //Table entry: 2
                            buffer = 0;
                            int.TryParse(msg.Substring(7, 3), out buffer);
                            preSetOperationCodeBufferStateTableData.BufferEntriesCleared = buffer; //Table entry: 3
                            buffer = 0;
                            int.TryParse(msg.Substring(10, 3), out buffer);
                            preSetOperationCodeBufferStateTableData.BufferEntriesSetToA = buffer; //Table entry: 4
                            buffer = 0;
                            int.TryParse(msg.Substring(13, 3), out buffer);
                            preSetOperationCodeBufferStateTableData.BufferEntriesSetToB = buffer; //Table entry: 5
                            buffer = 0;
                            int.TryParse(msg.Substring(16, 3), out buffer);
                            preSetOperationCodeBufferStateTableData.BufferEntriesSetToC = buffer; //Table entry: 6
                            buffer = 0;
                            int.TryParse(msg.Substring(19, 3), out buffer);
                            preSetOperationCodeBufferStateTableData.BufferEntriesSetToD = buffer; //Table entry: 7
                            preSetOperationCodeBufferStateTableData.Item = new EmptyElement_Type(); //Table entry: 8
                            string extensionStateNumber = msg.Substring(25, 3);//Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        PreSetOperationCodeBufferStateTableDataExtension_Type tableDataExtended = new PreSetOperationCodeBufferStateTableDataExtension_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(0, 3), out buffer);
                                        tableDataExtended.BufferEntriesSetToF = buffer; //Table entry: 2
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(3, 3), out buffer);
                                        tableDataExtended.BufferEntriesSetToG = buffer; //Table entry: 3
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(6, 3), out buffer);
                                        tableDataExtended.BufferEntriesSetToH = buffer; //Table entry: 4
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(9, 3), out buffer);
                                        tableDataExtended.BufferEntriesSetToI = buffer; //Table entry: 5
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(12, 3), out buffer);
                                        tableDataExtended.Item = new EmptyElement_Type(); //Table entry: 6
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(15, 3), out buffer);
                                        tableDataExtended.Item1 = new EmptyElement_Type(); //Table entry: 7
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(18, 3), out buffer);
                                        tableDataExtended.Item2 = new EmptyElement_Type(); //Table entry: 8
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(21, 3), out buffer);
                                        tableDataExtended.Item3 = new EmptyElement_Type(); //Table entry: 9
                                        preSetOperationCodeBufferStateTableData.Item1 = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = preSetOperationCodeBufferStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.FourFDKSelectionFunctionStateTableData: //E - Four FDK selection function state
                        {
                            FourFDKSelectionFunctionStateTableData_Type fourFDKSelectionFunctionStateTableData = new FourFDKSelectionFunctionStateTableData_Type();
                            fourFDKSelectionFunctionStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            fourFDKSelectionFunctionStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            fourFDKSelectionFunctionStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            fourFDKSelectionFunctionStateTableData.FDKAorINextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            fourFDKSelectionFunctionStateTableData.FDKBorHNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            fourFDKSelectionFunctionStateTableData.FDKCorGNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            fourFDKSelectionFunctionStateTableData.FDKDorFNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            buffer = 0;
                            int.TryParse(msg.Substring(25, 3), out buffer);
                            fourFDKSelectionFunctionStateTableData.OperationCodeBufferEntryNumber = (OperationCodeBufferEntryNumberChar3_Type)buffer; //Table entry: 9
                            stateTable.Item = fourFDKSelectionFunctionStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.AmountEntryStateTableData: //F - Amount Entry state
                        {
                            AmountEntryStateTableData_Type amountEntryStateTableData = new AmountEntryStateTableData_Type();
                            amountEntryStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            amountEntryStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            amountEntryStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            amountEntryStateTableData.FDKAorINextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            amountEntryStateTableData.FDKBorHNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            amountEntryStateTableData.FDKCorGNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            amountEntryStateTableData.FDKDorFNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            amountEntryStateTableData.AmountDisplayScreenNumber = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = amountEntryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.AmountCheckStateTableData: //G - Amount check state
                        {
                            AmountCheckStateTableData_Type amountCheckStateTableData = new AmountCheckStateTableData_Type();
                            amountCheckStateTableData.AmountCheckConditionTrueNextStateNumber = msg.Substring(4, 3); //Table entry: 2
                            amountCheckStateTableData.AmountCheckConditionFalseNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            buffer = 0;
                            int.TryParse(msg.Substring(10, 3), out buffer);
                            amountCheckStateTableData.BufferToBeChecked = (BufferToBeChecked_Type)buffer; //Table entry: 4
                            buffer = 0;
                            int.TryParse(msg.Substring(13, 3), out buffer);
                            amountCheckStateTableData.IntegerMultipleValue = buffer; //Table entry: 5
                            buffer = 0;
                            int.TryParse(msg.Substring(16, 3), out buffer);
                            amountCheckStateTableData.IntegerMultipleValue = buffer; //Table entry: 6
                            buffer = 0;
                            int.TryParse(msg.Substring(19, 3), out buffer);
                            amountCheckStateTableData.NumberOfDecimalPlaces = buffer; //Table entry: 7
                            amountCheckStateTableData.CurrencyType = msg.Substring(22, 3); //Table entry: 8
                            buffer = 0;
                            int.TryParse(msg.Substring(25, 3), out buffer);
                            amountCheckStateTableData.AmountCheckCondition = (AmountCheckCondition_Type)buffer; //Table entry: 9
                            stateTable.Item = amountCheckStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.InformationEntryStateTableData: //H - Information entry state
                        {
                            InformationEntryStateTableData_Type informationEntryStateTableData = new InformationEntryStateTableData_Type();
                            informationEntryStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            informationEntryStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            informationEntryStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            informationEntryStateTableData.FDKANextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            informationEntryStateTableData.FDKBNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            informationEntryStateTableData.FDKCNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            informationEntryStateTableData.FDKDNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            informationEntryStateTableData.EntryModeAndBufferConfiguration = new EntryModeAndBufferConfiguration_Type();
                            buffer = 0;
                            int.TryParse(msg.Substring(25, 2), out buffer);
                            informationEntryStateTableData.EntryModeAndBufferConfiguration.KeyboardEntryMode = (KeyboardEntryMode_Type)buffer; 
                            buffer = 0;
                            int.TryParse(msg.Substring(27, 1), out buffer);
                            informationEntryStateTableData.EntryModeAndBufferConfiguration.DisplayAndBufferParameters = (DisplayAndBufferParameters_Type)buffer; 
                            stateTable.Item = informationEntryStateTableData; //Table entry: 9
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.TransactionRequestStateTableData: //I - Transaction Request
                        {
                            TransactionRequestStateTableData_Type transactionRequestStateTableData = new TransactionRequestStateTableData_Type();
                            transactionRequestStateTableData.NextState = msg.Substring(4, 3); //Table entry: 2
                            transactionRequestStateTableData.CentralResponseTimeoutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            transactionRequestStateTableData.SendTrack2Data = msg.Substring(10, 3).Equals("001", StringComparison.Ordinal) ? 1 : 0; //Table entry: 4
                            //transactionRequestStateTableData.SendTrack1Or3Data = this.GetSendTrack1Or3(msg.Substring(13, 3)); //Table entry: 5
                            transactionRequestStateTableData.SendOperationCodeData = msg.Substring(16, 3).Equals("001", StringComparison.Ordinal) ? 1 : 0; //Table entry: 6
                            transactionRequestStateTableData.SendAmountData = msg.Substring(19, 3).Equals("001", StringComparison.Ordinal) ? 1 : 0; //Table entry: 7
                            transactionRequestStateTableData.SendPINBufferADataSelectExtendedFormat = this.GetSendPINBufferADataSelectExtendedFormat(msg.Substring(22, 3)); //Table entry: 8
                            string extensionStateNumber = msg.Substring(25, 3);//Table entry: 9
                            string[] SendBufferBOrC = { "000", "001", "002", "003", "004", "005", "006", "007" };
                            string result = SendBufferBOrC.FirstOrDefault(x => x.Equals(extensionStateNumber));
                            if (result == null)
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        TransactionRequestStateTableDataExtended_Type tableDataExtended = new TransactionRequestStateTableDataExtended_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.SendGeneralPurposeBuffersBAndOrC = this.GetSendGeneralPurposeBuffersBAndOrC(aSt.UnparsedStateTableData.Substring(0, 3)); //Table entry: 2
                                        tableDataExtended.SendOptionalDataFieldsAH = this.GetSendOptionalDataFieldsAH(aSt.UnparsedStateTableData.Substring(3, 3)); //Table entry: 3
                                        tableDataExtended.SendOptionalDataFieldsIL = this.GetSendOptionalDataFieldsIL(aSt.UnparsedStateTableData.Substring(6, 3)); //Table entry: 4
                                        tableDataExtended.SendOptionalDataFieldsQV = this.GetSendOptionalDataFieldsQV(aSt.UnparsedStateTableData.Substring(9, 3)); //Table entry: 5
                                        tableDataExtended.SendOptionalData = this.GetSendOptionalData(aSt.UnparsedStateTableData.Substring(12, 3)); //Table entry: 6
                                        tableDataExtended.Item = new EmptyElement_Type();
                                        tableDataExtended.EMVCAMProcessingFlag = this.GetEMVCAMProcessingFlag(aSt.UnparsedStateTableData.Substring(18, 3)); //Table entry: 8
                                        tableDataExtended.Item1 = new EmptyElement_Type(); //Table entry: 9
                                        transactionRequestStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            else
                            {
                                transactionRequestStateTableData.Item = extensionStateNumber;
                            }
                            stateTable.Item = transactionRequestStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CloseStateTableData: //J - Close
                        {
                            CloseStateTableData_Type closeStateTableData = new CloseStateTableData_Type();
                            closeStateTableData.ReceiptDeliveredScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            closeStateTableData.NextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            closeStateTableData.NoReceiptDeliveredScreenNumber = msg.Substring(10, 3); //Table entry: 4
                            closeStateTableData.CardRetainedScreenNumber = msg.Substring(13, 3); //Table entry: 5
                            closeStateTableData.StatementDeliveredScreenNumber = msg.Substring(16, 3); //Table entry: 6
                            closeStateTableData.Item = new EmptyElement_Type(); //Table entry: 7
                            closeStateTableData.BNANotesReturnedScreenNumber = msg.Substring(22, 3); //Table entry: 8
                            string extensionStateNumber = msg.Substring(25, 3);//Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        CloseStateTableDataExtension_Type tableDataExtended = new CloseStateTableDataExtension_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.CPMTakeDocumentScreenNumber = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(3, 3), out buffer);
                                        tableDataExtended.CPMDocumentRetainReturnFlag = (CPMDocumentRetainReturnFlag_Type)buffer; //Table entry: 3
                                        tableDataExtended.Item = new EmptyElement_Type(); //Table entry: 4
                                        tableDataExtended.Item1 = new object(); //Table entry: 5
                                        tableDataExtended.BCACoinsReturnedScreenNumber = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(15, 3), out buffer);
                                        tableDataExtended.BCACoinsReturnRetainFlag = (BNANotesReturnRetainFlag_Type)buffer; //Table entry: 7
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(18, 3), out buffer);
                                        tableDataExtended.BNANotesReturnRetainFlag = (BNANotesReturnRetainFlag_Type)buffer; //Table entry: 8
                                        tableDataExtended.Item2 = new EmptyElement_Type(); //Table entry: 9
                                        closeStateTableData.Item1 = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = closeStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.PrintStateTableData: //P ‐ PrintState
                        {
                            PrintStateTableData_Type printStateTableData = new PrintStateTableData_Type();
                            printStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            printStateTableData.GoodOperationNextState = msg.Substring(7, 3); //Table entry: 3
                            printStateTableData.HardwareFaultNextState = msg.Substring(10, 3); //Table entry: 4
                            buffer = 0;
                            int.TryParse(msg.Substring(13, 3), out buffer);
                            printStateTableData.UnitNumber = (PrinterFlag_Type)buffer; //Table entry: 5
                            printStateTableData.Operation = msg.Substring(16, 3); //Table entry: 6
                            buffer = 0;
                            int.TryParse(msg.Substring(19, 3), out buffer);
                            printStateTableData.ScreenTimer = buffer; //Table entry: 7
                            printStateTableData.FdkActiveMask = msg.Substring(22, 3); //Table entry: 8
                            printStateTableData.PrintBufferID = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = printStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.FITSwitchStateTableData:
                        {
                            FITSwitchStateTableData_Type FITSwitchStateTableData_Type = new FITSwitchStateTableData_Type();
                            stateTable.Item = FITSwitchStateTableData_Type;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ExpandedFITSwitchStateTableData:
                        {
                            ExpandedFITSwitchStateTableData_Type expandedFITSwitchStateTableData = new ExpandedFITSwitchStateTableData_Type();
                            stateTable.Item = expandedFITSwitchStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CardWriteStateTableData:
                        {
                            CardWriteStateTableData_Type cardWriteStateTableData = new CardWriteStateTableData_Type();
                            stateTable.Item = cardWriteStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.EnhancedPINEntryStateTableData:
                        {
                            EnhancedPINEntryStateTableData_Type enhancedPINEntryStateTableData = new EnhancedPINEntryStateTableData_Type();
                            stateTable.Item = enhancedPINEntryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CameraControlStateTableData:
                        {
                            CameraControlStateTableData_Type cameraControlStateTableData = new CameraControlStateTableData_Type();
                            stateTable.Item = cameraControlStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.EnhancedAmountEntryStateTableData: //R Enhanced amountEntry state
                        {
                            EnhancedAmountEntryStateTableData_Type enhancedAmountEntryStateTableData = new EnhancedAmountEntryStateTableData_Type();
                            enhancedAmountEntryStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            enhancedAmountEntryStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            enhancedAmountEntryStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            enhancedAmountEntryStateTableData.FDKANextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            enhancedAmountEntryStateTableData.FDKBNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            enhancedAmountEntryStateTableData.FDKCNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            enhancedAmountEntryStateTableData.FDKDNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            string extensionStateNumber = msg.Substring(25, 3);//Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        EnhancedAmountEntryStateTableDataExtension_Type tableDataExtended = new EnhancedAmountEntryStateTableDataExtension_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        buffer = 0;
                                        int.TryParse(aSt.UnparsedStateTableData.Substring(0, 3), out buffer);
                                        tableDataExtended.AmountBuffer = (AmountTargetBuffer_Type)buffer; //Table entry: 2
                                        tableDataExtended.AmountDisplayScreenNumber = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.StartCAVCommand = false; //Table entry: 4
                                        tableDataExtended.LanguageDependentScreenFlag = false; //Table entry: 5
                                        enhancedAmountEntryStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = enhancedAmountEntryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.LanguageCodeSwitchStateTableData:
                        {
                            LanguageCodeSwitchStateTableData_Type languageCodeSwitchStateTableData = new LanguageCodeSwitchStateTableData_Type();
                            stateTable.Item = languageCodeSwitchStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CardReadPINEntryInitiationStateTableData:
                        {
                            CardReadPINEntryInitiationStateTableData_Type cardReadPINEntryInitiationStateTableData = new CardReadPINEntryInitiationStateTableData_Type();
                            stateTable.Item = cardReadPINEntryInitiationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.LanguageSelectFromCardStateTableData:
                        {
                            LanguageSelectFromCardStateTableData_Type languageSelectFromCardStateTableData = new LanguageSelectFromCardStateTableData_Type();
                            stateTable.Item = languageSelectFromCardStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.FDKSwitchStateTableData: //W ‐ FDKSwitchState
                        {
                            FDKSwitchStateTableData_Type fDKSwitchStateTableData = new FDKSwitchStateTableData_Type();
                            fDKSwitchStateTableData.FDKANextStateNumber = msg.Substring(4, 3); //Table entry: 2
                            fDKSwitchStateTableData.FDKBNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            fDKSwitchStateTableData.FDKCNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            fDKSwitchStateTableData.FDKDNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            fDKSwitchStateTableData.FDKFNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            fDKSwitchStateTableData.FDKGNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            fDKSwitchStateTableData.FDKHNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            fDKSwitchStateTableData.FDKINextStateNumber = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = fDKSwitchStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.FDKInformationEntryStateTableData:
                        {
                            FDKInformationEntryStateTableData_Type fDKInformationEntryStateTableData = new FDKInformationEntryStateTableData_Type();
                            stateTable.Item = fDKInformationEntryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.EightFDKSelectionFunctionStateTableData: //Y - Eight FDK selection function state
                        {
                            EightFDKSelectionFunctionStateTableData_Type eightFDKSelectionFunctionStateTableData = new EightFDKSelectionFunctionStateTableData_Type();
                            eightFDKSelectionFunctionStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            eightFDKSelectionFunctionStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            eightFDKSelectionFunctionStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            eightFDKSelectionFunctionStateTableData.FDKNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            string extensionStateNumber = msg.Substring(16, 3); //Table entry: 6
                            eightFDKSelectionFunctionStateTableData.OperationCodeBufferPositions = msg.Substring(19, 3); //Table entry: 7
                            eightFDKSelectionFunctionStateTableData.ActiveFDKs = this.GetKeyMaskData(msg.Substring(22, 3)); //Table entry: 8
                            eightFDKSelectionFunctionStateTableData.Item1 = msg.Substring(25, 3); //Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        EightFDKSelectionFunctionStateTableDataExtension_Type tableDataExtended = new EightFDKSelectionFunctionStateTableDataExtension_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.CodeFDKA = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.CodeFDKB = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.CodeFDKC = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.CodeFDKD = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.CodeFDKF = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.CodeFDKG = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.CodeFDKH = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.CodeFDKI = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        eightFDKSelectionFunctionStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = eightFDKSelectionFunctionStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CustomerSelectablePINStateTableData:
                        {
                            CustomerSelectablePINStateTableData_Type customerSelectablePINStateTableData = new CustomerSelectablePINStateTableData_Type();
                            stateTable.Item = customerSelectablePINStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.SmartFITCheckStateTableData:
                        {
                            SmartFITCheckStateTableData_Type smartFITCheckStateTableData = new SmartFITCheckStateTableData_Type();
                            stateTable.Item = smartFITCheckStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.PINAndLanguageSelectStateTableData:
                        {
                            PINAndLanguageSelectState_Type pINAndLanguageSelectState = new PINAndLanguageSelectState_Type();
                            stateTable.Item = pINAndLanguageSelectState;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CashAcceptStateTableData: //> - Cash accept state
                        {
                            CashAcceptStateTableData_Type cashAcceptStateTableData = new CashAcceptStateTableData_Type();
                            cashAcceptStateTableData.CancelKeyMask = msg.Substring(4, 3); //Table entry: 2
                            cashAcceptStateTableData.DepositKeyMask = msg.Substring(7, 3); //Table entry: 3
                            cashAcceptStateTableData.AddMoreKeyMask = msg.Substring(10, 3); //Table entry: 4
                            cashAcceptStateTableData.RefundKeyMask = msg.Substring(13, 3); //Table entry: 5
                            string extensionStateNumber1 = msg.Substring(16, 3); //Table entry: 6
                            if (!extensionStateNumber1.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber1, StringComparison.Ordinal))
                                    {
                                        CashAcceptStateTableDataExtension1_Type tableDataExtended1 = new CashAcceptStateTableDataExtension1_Type();
                                        tableDataExtended1.StateNumber = extensionStateNumber1;
                                        tableDataExtended1.PleaseEnterNotesScreen = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended1.PleaseRemoveNotesScreen = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended1.ConfirmationScreen = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended1.HardwareErrorScreen = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended1.EscrowFullScreen = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended1.ProcessingNotesScreen = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended1.PleaseRemoveMoreThan90NotesScreen = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended1.PleaseWaitScreen = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        cashAcceptStateTableData.Item = tableDataExtended1;
                                    }
                                }
                            }
                            string extensionStateNumber2 = msg.Substring(19, 3); //Table entry: 7
                            if (!extensionStateNumber2.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber2, StringComparison.Ordinal))
                                    {
                                        CashAcceptStateTableDataExtension2_Type tableDataExtended2 = new CashAcceptStateTableDataExtension2_Type();
                                        tableDataExtended2.StateNumber = extensionStateNumber2;
                                        tableDataExtended2.GoodNextStateNumber = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended2.CancelNextStateNumber = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended2.DeviceErrorNextStateNumber = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended2.TimeOutNextStateNumber = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended2.DeclinedNextStateNumber = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended2.OperationMode = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended2.AutoDeposit = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended2.RetractingNotesScreen = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        cashAcceptStateTableData.Item1 = tableDataExtended2;
                                    }
                                }
                            }
                            string extensionStateNumber3 = msg.Substring(22, 3); //Table entry: 8
                            if (!extensionStateNumber3.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber3, StringComparison.Ordinal))
                                    {
                                        CashAcceptStateTableDataExtension3_Type tableDataExtended3 = new CashAcceptStateTableDataExtension3_Type();
                                        tableDataExtended3.StateNumber = extensionStateNumber3;
                                        tableDataExtended3.SetDenominations112 = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended3.SetDenominations1324 = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended3.SetDenominations2536 = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended3.SetDenominations3748 = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended3.SetDenominations4960 = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended3.SetDenominations6172 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended3.SetDenominations7384 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended3.SetDenominations8596 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        cashAcceptStateTableData.Item2 = tableDataExtended3;
                                    }
                                }
                            }
                            stateTable.Item = cashAcceptStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BunchChequeAcceptStateTableData:
                        {
                            BunchChequeAcceptStateTableData_Type bunchChequeAcceptStateTableData = new BunchChequeAcceptStateTableData_Type();
                            stateTable.Item = bunchChequeAcceptStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BunchChequeHandlingStateTableData:
                        {
                            BunchChequeHandlingStateTableData_Type bunchChequeHandlingStateTableData = new BunchChequeHandlingStateTableData_Type();
                            stateTable.Item = bunchChequeHandlingStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ChequeDetailDisplayStateTableData:
                        {
                            ChequeDetailDisplayStateTableData_Type chequeDetailDisplayStateTableData = new ChequeDetailDisplayStateTableData_Type();
                            stateTable.Item = chequeDetailDisplayStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.DisplayChequeSummaryStateTableData:
                        {
                            DisplayChequeSummaryStateTableData_Type displayChequeSummaryStateTableData = new DisplayChequeSummaryStateTableData_Type();
                            stateTable.Item = displayChequeSummaryStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BarcodeReadStateTableData:
                        {
                            BarcodeReadStateTableData_Type barcodeReadStateTableData = new BarcodeReadStateTableData_Type();
                            barcodeReadStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            barcodeReadStateTableData.GoodBarcodeReadStateNumber = msg.Substring(7, 3); //Table entry: 3
                            barcodeReadStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            barcodeReadStateTableData.ErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            barcodeReadStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            buffer = 0;
                            int.TryParse(msg.Substring(19, 3), out buffer);
                            barcodeReadStateTableData.BarcodeDataDestination = (BarcodeDataDestination_Type)buffer; //Table entry: 7
                            barcodeReadStateTableData.ActiveCancelFDKKeyMask = msg.Substring(22, 3); //Table entry: 8                            
                            string extensionStateNumber = msg.Substring(25, 3); //Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        BarcodeReadStateTableDataExtension1_Type tableDataExtended = new BarcodeReadStateTableDataExtension1_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.Item = new EmptyElement_Type(); //aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.Item1 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.Item2 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.Item3 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.Item4 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Item5 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Item6 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Item7 = new EmptyElement_Type(); // aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        barcodeReadStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = barcodeReadStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BeginICCInitialisationStateTableData:
                        {
                            BeginICCInitialisationStateTableData_Type beginICCInitialisationStateTableData = new BeginICCInitialisationStateTableData_Type();
                            stateTable.Item = beginICCInitialisationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BeginICCApplicationSelectionAndInitialisationStateTableData:
                        {
                            BeginICCApplicationSelectionAndInitialisationStateTableData_Type beginICCApplicationSelectionAndInitialisationStateTableData = new BeginICCApplicationSelectionAndInitialisationStateTableData_Type();
                            stateTable.Item = beginICCApplicationSelectionAndInitialisationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CompleteICCInitialisationStateTableData:
                        {
                            CompleteICCInitialisationStateTableData_Type completeICCInitialisationStateTableData = new CompleteICCInitialisationStateTableData_Type();
                            stateTable.Item = completeICCInitialisationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CompleteICCApplicationSelectionAndInitialisationStateTableData:
                        {
                            CompleteICCApplicationSelectionAndInitialisationStateTableData_Type completeICCApplicationSelectionAndInitialisationStateTableData = new CompleteICCApplicationSelectionAndInitialisationStateTableData_Type();
                            stateTable.Item = completeICCApplicationSelectionAndInitialisationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.SetICCTransactionDataStateTableData:
                        {
                            SetICCTransactionDataStateTableData_Type setICCTransactionDataStateTableData = new SetICCTransactionDataStateTableData_Type();
                            stateTable.Item = setICCTransactionDataStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ICCReinitialiseStateTableData:
                        {
                            ICCReinitialiseStateTableData_Type iCCReinitialiseStateTableData = new ICCReinitialiseStateTableData_Type();
                            stateTable.Item = iCCReinitialiseStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.AutomaticLanguageSelectionStateTableData:
                        {
                            AutomaticLanguageSelectionStateTableData_Type automaticLanguageSelectionStateTableData = new AutomaticLanguageSelectionStateTableData_Type();
                            stateTable.Item = automaticLanguageSelectionStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.EMVSmartDIPTakeCardStateTableData:
                        {
                            EMVSmartDIPTakeCardStateTableData_Type eMVSmartDIPTakeCardStateTableData = new EMVSmartDIPTakeCardStateTableData_Type();
                            stateTable.Item = eMVSmartDIPTakeCardStateTableData;
                            ret = true;
                            break;
                        }
                        case ItemChoiceStateTable_Type.AccountSelectorStateTableData://d Account Selector
                        {
                            AccountSelectorStateTableData_Type AccountSelectorStateTableData = new AccountSelectorStateTableData_Type();
                            AccountSelectorStateTableData.ScreenNumber = msg.Substring(4, 3); //Table entry: 2
                            AccountSelectorStateTableData.TimeOutNextStateNumber = msg.Substring(7, 3); //Table entry: 3
                            AccountSelectorStateTableData.CancelNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            buffer = 0;
                            if (int.TryParse(msg.Substring(13, 3), out buffer))
                                AccountSelectorStateTableData.OperationCodeData = (GetDataOperationMode_Type)buffer; //Table entry: 5
                            else
                                AccountSelectorStateTableData.OperationCodeData = GetDataOperationMode_Type.none;
                            AccountSelectorStateTableData.NextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            AccountSelectorStateTableData.BackNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            AccountSelectorStateTableData.Aux2 = msg.Substring(22, 3); //Table entry: 8
                            AccountSelectorStateTableData.Item = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = AccountSelectorStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.FingerPrintCaptureStateTableData: //z006 Finger Print Capture 
                        {
                            FingerPrintCaptureStateTableData_Type fingerPrintCaptureStateTableData = new FingerPrintCaptureStateTableData_Type();
                            fingerPrintCaptureStateTableData.PlaceFingerScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            fingerPrintCaptureStateTableData.TimeOutNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            fingerPrintCaptureStateTableData.CancelNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            fingerPrintCaptureStateTableData.FDKPressededNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            fingerPrintCaptureStateTableData.FDKActiveMask = msg.Substring(19, 3); //Table entry: 7
                            string extensionStateNumber1 = msg.Substring(22, 3); //Table entry: 8
                            if (!extensionStateNumber1.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber1, StringComparison.Ordinal))
                                    {
                                        FingerPrintCaptureStateTableExtension1_Type tableDataExtended = new FingerPrintCaptureStateTableExtension1_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber1;
                                        tableDataExtended.MinimumAcceptableWhitePercentage = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.MaximunAcceptableWhitePercentage = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.ImageCapturedNextStateNumber = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.ImageNotCapturedNextStateNumber = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.HardwareErrorOrDeviceNotPresentNextStateNumber = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Reserved1 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Reserved2 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Reserved3 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        fingerPrintCaptureStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            string extensionStateNumber2 = msg.Substring(25, 3); //Table entry: 9
                            if (!extensionStateNumber2.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber2, StringComparison.Ordinal))
                                    {
                                        FingerPrintCaptureStateTableExtension2_Type tableDataExtended = new FingerPrintCaptureStateTableExtension2_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber2;
                                        tableDataExtended.ReadingFingerScreenNumber = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.CheckFingerPositionScreenNumber = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.RemoveFingerScreenNumber = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.ImageLocationScreenNumber = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.PleaseWaitScreenNumber = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Reserved1 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Reserved2 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Reserved3 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        fingerPrintCaptureStateTableData.Item1 = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = fingerPrintCaptureStateTableData;
                            ret = true;
                            break;
                        } 
                    case ItemChoiceStateTable_Type.LoginStateTableData: //z008 Login State 
                        {
                            LoginStateTableData_Type loginStateTableData = new LoginStateTableData_Type();
                            loginStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            loginStateTableData.GoodANextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            loginStateTableData.GoodBNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            loginStateTableData.GoodCNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            loginStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            loginStateTableData.TimeOutNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            string extensionStateNumber = msg.Substring(25, 3); //Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        LoginStateTableExtension1_Type tableDataExtended = new LoginStateTableExtension1_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.Language1 = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.Language2 = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.Language3 = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.Language4 = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.Language5 = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Language6 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Language7 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Language8 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        loginStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }                          
                            stateTable.Item = loginStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ConfigurationStateTableData: //z015 Configuration State
                        {
                            ConfigurationStateTableData_Type configurationStateTableData = new ConfigurationStateTableData_Type();
                            configurationStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            configurationStateTableData.ExitNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            configurationStateTableData.TimeOutNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            configurationStateTableData.ErrorNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            configurationStateTableData.Item1 = msg.Substring(19, 3); //Table entry: 7
                            configurationStateTableData.Item2 = msg.Substring(22, 3); //Table entry: 8
                            configurationStateTableData.Item3 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = configurationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.BagDropDepositStateTableData: //z009 ‐ Bag drop deposit state
                        {
                            BagDropDepositStateTableData_Type bagDropDepositState = new BagDropDepositStateTableData_Type();
                            bagDropDepositState.Item2 = msg.Substring(7, 3); //Table entry: 3
                            bagDropDepositState.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            bagDropDepositState.CancelNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            bagDropDepositState.HardwareErrorNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            bagDropDepositState.TimeOutNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            buffer = 0;
                            if (int.TryParse(msg.Substring(22, 3), out buffer))
                                bagDropDepositState.OperationMode = (Enums.CimModel)buffer; //Table entry: 8
                            else
                                bagDropDepositState.OperationMode = Enums.CimModel.None;
                            string extensionStateNumber = msg.Substring(25, 3); //Table entry: 9
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        BagDropDepositStateTableExtension1_Type tableDataExtended = new BagDropDepositStateTableExtension1_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.ScreenMode = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.MainScreenNumber = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.ProcessScreenNumber = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.ConfirmationScreenNumber = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.DepositMaxQuantity = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Language6 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Language7 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Language8 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        bagDropDepositState.Item = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = bagDropDepositState;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.SettlementOperationStateTableData: //z010 Settlement Operation
                        {
                            SettlementOperationStateTableData_Type settlementOperationStateTableData = new SettlementOperationStateTableData_Type();
                            settlementOperationStateTableData.ScreenNumber1 = msg.Substring(7, 3); //Table entry: 2
                            settlementOperationStateTableData.ScreenNumber2 = msg.Substring(10, 3); //Table entry: 3
                            settlementOperationStateTableData.LoginMode = msg.Substring(13, 3); //Table entry: 4
                            settlementOperationStateTableData.CancelNextStateNumber = msg.Substring(16, 3); //Table entry: 5
                            settlementOperationStateTableData.EnterNextStateNumber = msg.Substring(19, 3); //Table entry: 6
                            settlementOperationStateTableData.EncriptPassword = msg.Substring(22, 3); //Table entry: 7
                            string extensionStateNumber = msg.Substring(25, 3); //Table entry: 8
                            if (!extensionStateNumber.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type aSt in this.ExtensionStateTable)
                                {
                                    if (aSt.StateType.Equals(extensionStateNumber, StringComparison.Ordinal))
                                    {
                                        SettlementOperationStateTableExtension1_Type tableDataExtended = new SettlementOperationStateTableExtension1_Type();
                                        tableDataExtended.StateNumber = extensionStateNumber;
                                        tableDataExtended.Language1 = aSt.UnparsedStateTableData.Substring(0, 3); //Table entry: 2
                                        tableDataExtended.Language2 = aSt.UnparsedStateTableData.Substring(3, 3); //Table entry: 3
                                        tableDataExtended.Language3 = aSt.UnparsedStateTableData.Substring(6, 3); //Table entry: 4
                                        tableDataExtended.Language4 = aSt.UnparsedStateTableData.Substring(9, 3); //Table entry: 5
                                        tableDataExtended.Language5 = aSt.UnparsedStateTableData.Substring(12, 3); //Table entry: 6
                                        tableDataExtended.Language6 = aSt.UnparsedStateTableData.Substring(15, 3); //Table entry: 7
                                        tableDataExtended.Language7 = aSt.UnparsedStateTableData.Substring(18, 3); //Table entry: 8
                                        tableDataExtended.Language8 = aSt.UnparsedStateTableData.Substring(21, 3); //Table entry: 9
                                        settlementOperationStateTableData.Item = tableDataExtended;
                                    }
                                }
                            }
                            stateTable.Item = settlementOperationStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.SetExtraDataStateTableData:
                        {
                            SetExtraDataStateTableData_Type setExtraDataStateTableData_Type = new SetExtraDataStateTableData_Type();
                            setExtraDataStateTableData_Type.ScreenNumber = msg.Substring(7, 3);
                            setExtraDataStateTableData_Type.SetGoodNextStateNumber = msg.Substring(10, 3);
                            setExtraDataStateTableData_Type.CancelNextStateNumber = msg.Substring(13, 3);
                            setExtraDataStateTableData_Type.HardwareErrorNextStateNumber = msg.Substring(16, 3);
                            setExtraDataStateTableData_Type.TimeOutNextStateNumber = msg.Substring(19, 3);
                            setExtraDataStateTableData_Type.Item1 = msg.Substring(22, 3);
                            string text2 = msg.Substring(25, 3);
                            if (!text2.Equals("000", StringComparison.Ordinal))
                            {
                                foreach (AnonymousStateTableData_Type item39 in ExtensionStateTable)
                                {
                                    if (item39.StateType.Equals(text2, StringComparison.Ordinal))
                                    {
                                        SetExtraDataStateTableExtension1_Type setExtraDataStateTableExtension1_Type = new SetExtraDataStateTableExtension1_Type();
                                        setExtraDataStateTableExtension1_Type.StateNumber = text2;
                                        setExtraDataStateTableExtension1_Type.SetChannelEnabled = item39.UnparsedStateTableData.Substring(0, 3);
                                        setExtraDataStateTableExtension1_Type.SetTransactionInfoEnabled = item39.UnparsedStateTableData.Substring(3, 3);
                                        setExtraDataStateTableExtension1_Type.SetTransactionRefEnabled = item39.UnparsedStateTableData.Substring(6, 3);
                                        setExtraDataStateTableExtension1_Type.SetShiftsEnabled = item39.UnparsedStateTableData.Substring(9, 3);
                                        setExtraDataStateTableExtension1_Type.SetCurrencyEnabled = item39.UnparsedStateTableData.Substring(12, 3);
                                        setExtraDataStateTableExtension1_Type.SetAmountLimitEnabled = item39.UnparsedStateTableData.Substring(15, 3);
                                        setExtraDataStateTableExtension1_Type.SetDenominations7384 = item39.UnparsedStateTableData.Substring(18, 3);
                                        setExtraDataStateTableExtension1_Type.SetDenominations8596 = item39.UnparsedStateTableData.Substring(21, 3);
                                        setExtraDataStateTableData_Type.Item = setExtraDataStateTableExtension1_Type;
                                    }
                                }
                            }
                            stateTable.Item = setExtraDataStateTableData_Type;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.MultiCashAcceptStateTableData: //z012 Multi Cash Accept State Table
                        {
                            MultiCashAcceptStateTableData_Type MultiCashAcceptStateTableData = new MultiCashAcceptStateTableData_Type();
                            MultiCashAcceptStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 2
                            MultiCashAcceptStateTableData.EnterNextStateNumber = msg.Substring(10, 3); //Table entry: 3
                            MultiCashAcceptStateTableData.TimeOutNextStateNumber = msg.Substring(13, 3); //Table entry: 4
                            MultiCashAcceptStateTableData.CancelNextStateNumber = msg.Substring(16, 3); //Table entry: 5
                            MultiCashAcceptStateTableData.MoreDepositNextStateNumber = msg.Substring(19, 3); //Table entry: 6
                            MultiCashAcceptStateTableData.PrdmActive = msg.Substring(22, 3); //Table entry: 7
                            MultiCashAcceptStateTableData.MaximumNumberOfDeposit = msg.Substring(25, 3); //Table entry: 8
                            stateTable.Item = MultiCashAcceptStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ShipoutStateTableData: //z013 Shipout State Table
                        {
                            ShipoutStateTableData_Type ShipoutStateTableData = new ShipoutStateTableData_Type();
                            ShipoutStateTableData.SendCollection = msg.Substring(7, 3); //Table entry: 2
                            ShipoutStateTableData.SendCollectionDeclared = msg.Substring(10, 3); //Table entry: 3
                            ShipoutStateTableData.SendContents = msg.Substring(13, 3); //Table entry: 4
                            ShipoutStateTableData.PrintTicket = msg.Substring(16, 3); //Table entry: 5
                            ShipoutStateTableData.UpdateTSN = msg.Substring(19, 3); //Table entry: 6
                            ShipoutStateTableData.ClearLogicalCounters= msg.Substring(22, 3); //Table entry: 7
                            ShipoutStateTableData.NextState = msg.Substring(25, 3); //Table entry: 8
                            stateTable.Item = ShipoutStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.SupervisorStateTableData: //z014 Supervisor State Table
                        {
                            SupervisorStateTableData_Type SupervisorStateTableData = new SupervisorStateTableData_Type();
                            SupervisorStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 2
                            SupervisorStateTableData.Reserved_1 = msg.Substring(10, 3); //Table entry: 3
                            SupervisorStateTableData.Reserved_2 = msg.Substring(13, 3); //Table entry: 4
                            SupervisorStateTableData.Reserved_3 = msg.Substring(16, 3); //Table entry: 5
                            SupervisorStateTableData.Reserved_4 = msg.Substring(19, 3); //Table entry: 6
                            SupervisorStateTableData.Reserved_5 = msg.Substring(22, 3); //Table entry: 7
                            SupervisorStateTableData.Reserved_6 = msg.Substring(25, 3); //Table entry: 8
                            stateTable.Item = SupervisorStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CashDispenseStateTableData: //z016 - CashDispenseState
                        {
                            CashDispenseStateTableData_Type cashDispenseStateTableData = new CashDispenseStateTableData_Type();
                            cashDispenseStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            cashDispenseStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            cashDispenseStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            cashDispenseStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            cashDispenseStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            cashDispenseStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            cashDispenseStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = cashDispenseStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ChoicesSelectorStateTableData: //z017 - ChoicesSelectorState
                        {
                            ChoicesSelectorStateTableData_Type choicesSelectorStateTableData = new ChoicesSelectorStateTableData_Type();
                            choicesSelectorStateTableData.NextStateNumberA = msg.Substring(7, 3); //Table entry: 3
                            choicesSelectorStateTableData.NextStateNumberB = msg.Substring(10, 3); //Table entry: 4
                            choicesSelectorStateTableData.NextStateNumberC = msg.Substring(13, 3); //Table entry: 5
                            choicesSelectorStateTableData.HardwareErrorNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            choicesSelectorStateTableData.TimeoutNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            choicesSelectorStateTableData.CancelNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            choicesSelectorStateTableData.BackNextStateNumber = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = choicesSelectorStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CheckDepositStateTableData: //z018 - CheckDepositState
                        {
                            CheckDepositStateTableData_Type checkDepositStateTableData = new CheckDepositStateTableData_Type();
                            checkDepositStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            checkDepositStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            checkDepositStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            checkDepositStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            checkDepositStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            checkDepositStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            checkDepositStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = checkDepositStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.VerifyNotesStateTableData: //z019 - VerifyNotesState
                        {
                            VerifyNotesStateTableData_Type verifyNotesStateTableData = new VerifyNotesStateTableData_Type();
                            verifyNotesStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            verifyNotesStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            verifyNotesStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            verifyNotesStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            verifyNotesStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            verifyNotesStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            verifyNotesStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = verifyNotesStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.PinPadPaymentStateTableData: //z020 - PinPadPaymentState
                        {
                            PinPadPaymentStateTableData_Type pinPadPaymentStateTableData = new PinPadPaymentStateTableData_Type();
                            pinPadPaymentStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            pinPadPaymentStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            pinPadPaymentStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            pinPadPaymentStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            pinPadPaymentStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            pinPadPaymentStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            pinPadPaymentStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = pinPadPaymentStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.TransactionMenuStateTableData: //z021 - TransactionMenuState
                        {
                            TransactionMenuStateTableData_Type transactionMenuStateTableData = new TransactionMenuStateTableData_Type();
                            transactionMenuStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            transactionMenuStateTableData.TimeOutNextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            transactionMenuStateTableData.CancelNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            transactionMenuStateTableData.NextStateNumberA = msg.Substring(16, 3); //Table entry: 6
                            transactionMenuStateTableData.NextStateNumberB = msg.Substring(19, 3); //Table entry: 7
                            transactionMenuStateTableData.DeviceErrorNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            transactionMenuStateTableData.ActiveFDKs = this.GetKeyMaskData(msg.Substring(25, 3)); //Table entry: 9
                            transactionMenuStateTableData.Item1 = msg.Substring(85, 3); //Table entry: 9
                            stateTable.Item = transactionMenuStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ShoppingCartStateTableData: //z022 - ShoppingCartState
                        {
                            ShoppingCartStateTableData_Type shoppingCartStateTableData = new ShoppingCartStateTableData_Type();
                            shoppingCartStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            shoppingCartStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            shoppingCartStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            shoppingCartStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            shoppingCartStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            shoppingCartStateTableData.BackNextStateNumber = msg.Substring(22, 3); //Table entry: 8
                            shoppingCartStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = shoppingCartStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.ChangeHandlerStateTableData: //z023 - ChangeHandlerState
                        {
                            ChangeHandlerStateTableData_Type changeHandlerStateTableData = new ChangeHandlerStateTableData_Type();
                            changeHandlerStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            changeHandlerStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            changeHandlerStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            changeHandlerStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            changeHandlerStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            changeHandlerStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            changeHandlerStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = changeHandlerStateTableData;
                            ret = true;
                            break;
                        }
                    case ItemChoiceStateTable_Type.CoinDispenserStateTableData: //z024 - CoinDispenserState
                        {
                            CoinDispenserStateTableData_Type coinDispenserStateTableData = new CoinDispenserStateTableData_Type();
                            coinDispenserStateTableData.ScreenNumber = msg.Substring(7, 3); //Table entry: 3
                            coinDispenserStateTableData.NextStateNumber = msg.Substring(10, 3); //Table entry: 4
                            coinDispenserStateTableData.HardwareErrorNextStateNumber = msg.Substring(13, 3); //Table entry: 5
                            coinDispenserStateTableData.TimeoutNextStateNumber = msg.Substring(16, 3); //Table entry: 6
                            coinDispenserStateTableData.CancelNextStateNumber = msg.Substring(19, 3); //Table entry: 7
                            coinDispenserStateTableData.Item1 = msg.Substring(22, 3); //Table entry: 8
                            coinDispenserStateTableData.Item2 = msg.Substring(25, 3); //Table entry: 9
                            stateTable.Item = coinDispenserStateTableData;
                            ret = true;
                            break;
                        }
                    default:
                        {
                            string aux = string.Format("Unknown state: {0}", stateTable.StateNumber);
                            break;
                        }
                }
                return ret;
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Obtiene el indice correspondiente al estado según su letra identificatoria.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private int GetItemChoiceStateTable(string msg)
        {
            int ret = 0;
            switch (msg.Substring(3, 1))
            {
                case "A": { ret = 14; break; } //CardReadStateTableData
                case "B": { ret = 36; break; } //PINEntryStateTableData
                case "C": { ret = 26; break; } //EnvelopeDispenserStateTableData
                case "D": { ret = 37; break; } //PreSetOperationCodeBufferStateTableData
                case "E": { ret = 31; break; } //FourFDKSelectionFunctionStateTableData
                case "F": { ret = 7; break; }  //AmountEntryStateTableData
                case "G": { ret = 6; break; }  //AmountCheckStateTableData 
                case "H": { ret = 33; break; } //InformationEntryStateTableData
                case "I": { ret = 40; break; } //TransactionRequestStateTableData
                case "J": { ret = 18; break; } //CloseStateTableData
                case "K": { ret = 30; break; } //FITSwitchStateTableData
                case "_": { ret = 27; break; } //ExpandedFITSwitchStateTableData
                case "L": { ret = 15; break; } //CardWriteStateTableData
                case "M": { ret = 25; break; } //EnhancedPINEntryStateTableData
                case "N": { ret = 12; break; } //CameraControlStateTableData
                case "P": { ret = 55; break; } //PrintStateTableData
                case "R": { ret = 24; break; } //EnhancedAmountEntryStateTableData
                case "S": { ret = 34; break; } //LanguageCodeSwitchStateTableData
                case "T": { ret = 13; break; } //CardReadPINEntryInitiationStateTableData
                case "V": { ret = 35; break; } //LanguageSelectFromCardStateTableData
                case "W": { ret = 29; break; } //FDKSwitchStateTableData
                case "X": { ret = 28; break; } //FDKInformationEntryStateTableData
                case "Y": { ret = 23; break; } //EightFDKSelectionFunctionStateTableData
                case "Z": { ret = 4; break; }  //AnonymousExtensionStateTableData
                case "b": { ret = 22; break; } //CustomerSelectablePINStateTableData
                case "d": { ret = 52; break; } //AccountSelectorStateTableData
                case "e": 
                case "f":
                case "g":
                    { ret = 99; break; }       //Exit states
                case "k": { ret = 39; break; } //SmartFITCheckStateTableData
                case "m": { ret = 43; break; } //PINAndLanguageSelectStateTableData
                case ">": { ret = 16; break; } //CashAcceptStateTableData
                case "&": { ret = 42; break; } //BarcodeReadStateTableData
                case "+": { ret = 11; break; } //BeginICCInitialisationStateTableData
                case ".": { ret = 10; break; } //BeginICCApplicationSelectionAndInitialisationStateTableData
                case ",": { ret = 20; break; } //CompleteICCInitialisationStateTableData
                case "/": { ret = 19; break; } //CompleteICCApplicationSelectionAndInitialisationStateTableData
                case "?": { ret = 38; break; } //SetICCTransactionDataStateTableData
                case ";": { ret = 32; break; } //ICCReinitialiseStateTableData
                case "-": { ret = 9; break; }  //AutomaticLanguageSelectionStateTableData
                case "z":
                    switch(msg.Substring(4, 3))
                    {
                        case "001": { ret = 45; break; } //BunchChequeAcceptStateTableData
                        case "002": { ret = 46; break; } //BunchChequeHandlingStateTableData
                        case "003": { ret = 47; break; } //ChequeDetailDisplayStateTableData
                        case "004": { ret = 48; break; } //DisplayChequeSummaryStateTableData
                        case "005": { ret = 53; break; } //FingerSelectionStateTableData
                        case "006": { ret = 54; break; } //FingerPrintCaptureStateTableData
                        case "007": { ret = 44; break; } //EMVSmartDIPTakeCardStateTableData
                        case "008": { ret = 56; break; } //LoginStateTableData
                        case "009": { ret = 57; break; } //BagDropDepositStateTableData
                        case "010": { ret = 58; break; } //SettlementOperationStateTableData
                        case "011": { ret = 59; break; } //SetChannelStateTableData
                        case "012": { ret = 60; break; } //MultiCashAcceptStateTableData
                        case "013": { ret = 61; break; } //ShipoutStateTableData
                        case "014": { ret = 62; break; } //SupervisorStateTableData
                        case "015": { ret = 63; break; } //ConfigurationStateTableData
                        case "016": { ret = 64; break; } //CashDispenseState
                        case "017": { ret = 65; break; } //ChoicesSelectorState
                        case "018": { ret = 66; break; } //CheckDepositState
                        case "019": { ret = 67; break; } //VerifyNotesState
                        case "020": { ret = 68; break; } //PinPadPaymentState
                        case "021": { ret = 69; break; } //TransactionMenuStateTableData 
                        case "022": { ret = 70; break; } //ShoppingCartStateTableData 
                        case "023": { ret = 71; break; } //ChangeHandlerStateTableData 
                        case "024": { ret = 72; break; } //CoinDispenserStateTableData
                    }
                    break;
                                                  //AudioControlStateTableData = 8,
                                                  //BufferValidationStateTableData = 50,
                                                  //ChequeAcceptStateTableData = 17,
                                                  //CoinAcceptStateTableData = 43,
                                                  //CourtesyAmountVerificationStateTableData = 21,
                                                  //IgnoredStateTableData = 5,
                                                  //InsertCardStateTableData = 49,
                                                  //PassbookStateTableData = 41,
            }
            return ret;
        }

        //BNANotesReturnRetainFlag_Type GetBNANotesReturnRetainFlag(string msg)
        //{
        //    BNANotesReturnRetainFlag_Type bNANotesReturnRetainFlag = new BNANotesReturnRetainFlag_Type();
        //    bNANotesReturnRetainFlag. = false;
        //    bNANotesReturnRetainFlag.SendBufferC = false;
        //    //msg|SendBufferB|SendBufferC|
        //    //000|     0     |      0    |
        //    //001|     1     |      0    |
        //    //002|     0     |      1    |
        //    //003|     1     |      1    |
        //    byte[] arrByte;
        //    int readCond;
        //    if (int.TryParse(msg, out readCond))
        //    {
        //        if (Business.Utilities.ByteArrayToBin(readCond, out arrByte))
        //        {
        //            if (arrByte[6] == 1)
        //                bNANotesReturnRetainFlag.SendBufferB = true;
        //            if (arrByte[7] == 1)
        //                bNANotesReturnRetainFlag.SendBufferC = true;
        //        }
        //    }
        //    return bNANotesReturnRetainFlag;
        //}

        EMVCAMProcessingFlag_Type GetEMVCAMProcessingFlag(string msg)
        {
            EMVCAMProcessingFlag_Type eMVCAMProcessingFlag = new EMVCAMProcessingFlag_Type();
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                readCond++; //Sumo 1 porque el valor inicial arranca en 0 en lugar de 1.
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        eMVCAMProcessingFlag.PerformEMVCAMProcessing = true;
                    if (arrByte[6] == 1)
                        eMVCAMProcessingFlag.PartialEMVProcessing = true;
                    if (arrByte[5] == 1)
                        eMVCAMProcessingFlag.Reserved1 = true;
                    if (arrByte[4] == 1)
                        eMVCAMProcessingFlag.Reserved2 = true;
                    if (arrByte[3] == 1)
                        eMVCAMProcessingFlag.Reserved3 = true;
                    if (arrByte[2] == 1)
                        eMVCAMProcessingFlag.Reserved4 = true;
                    if (arrByte[1] == 1)
                        eMVCAMProcessingFlag.Reserved5 = true;
                    if (arrByte[0] == 1)
                        eMVCAMProcessingFlag.Reserved6 = true;
                }
            }
            return eMVCAMProcessingFlag;
        }

        SendOptionalData_Type GetSendOptionalData(string msg)
        {
            SendOptionalData_Type sendOptionalData = new SendOptionalData_Type();
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                readCond++; //Sumo 1 porque el valor inicial arranca en 0 en lugar de 1.
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        sendOptionalData.SendUserDataFields = true;
                    if (arrByte[6] == 1)
                        sendOptionalData.SendFieldb = true;
                    if (arrByte[5] == 1)
                        sendOptionalData.SendFieldx = true;
                    if (arrByte[4] == 1)
                        sendOptionalData.SendReserved4 = true;
                    if (arrByte[3] == 1)
                        sendOptionalData.SendFielde = true;
                    if (arrByte[2] == 1)
                        sendOptionalData.SendFieldg = true;
                    if (arrByte[1] == 1)
                        sendOptionalData.SendReserved7 = true;
                    if (arrByte[0] == 1)
                        sendOptionalData.SendReserved8 = true;
                }
            }
            return sendOptionalData;
        }

        SendOptionalDataFieldsQV_Type GetSendOptionalDataFieldsQV(string msg)
        {
            SendOptionalDataFieldsQV_Type sendOptionalDataFieldsQV = new SendOptionalDataFieldsQV_Type();
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                readCond++; //Sumo 1 porque el valor inicial arranca en 0 en lugar de 1.
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        sendOptionalDataFieldsQV.SendFieldQ = true;
                    if (arrByte[6] == 1)
                        sendOptionalDataFieldsQV.SendFieldR = true;
                    if (arrByte[5] == 1)
                        sendOptionalDataFieldsQV.SendFieldS = true;
                    if (arrByte[4] == 1)
                        sendOptionalDataFieldsQV.SendFieldT = true;
                    if (arrByte[3] == 1)
                        sendOptionalDataFieldsQV.SendFieldU = true;
                    if (arrByte[2] == 1)
                        sendOptionalDataFieldsQV.SendFieldV = true;
                    if (arrByte[1] == 1)
                        sendOptionalDataFieldsQV.SendFieldw = true;
                    if (arrByte[0] == 1)
                        sendOptionalDataFieldsQV.SendFielda = true;
                }
            }
            return sendOptionalDataFieldsQV;
        }


        SendOptionalDataFieldsIL_Type GetSendOptionalDataFieldsIL(string msg)
        {
            SendOptionalDataFieldsIL_Type sendOptionalDataFieldsIL = new SendOptionalDataFieldsIL_Type();
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                readCond++; //Sumo 1 porque el valor inicial arranca en 0 en lugar de 1.
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        sendOptionalDataFieldsIL.SendFieldI = true;
                    if (arrByte[6] == 1)
                        sendOptionalDataFieldsIL.SendFieldJ = true;
                    if (arrByte[5] == 1)
                        sendOptionalDataFieldsIL.SendFieldK = true;
                    if (arrByte[4] == 1)
                        sendOptionalDataFieldsIL.SendFieldL = true;
                    if (arrByte[3] == 1)
                        sendOptionalDataFieldsIL.SendFieldM = true;
                    if (arrByte[2] == 1)
                        sendOptionalDataFieldsIL.SendFieldN = true;
                    if (arrByte[1] == 1)
                        sendOptionalDataFieldsIL.SendFieldO = true;
                    if (arrByte[0] == 1)
                        sendOptionalDataFieldsIL.SendFieldP = true;
                }
            }
            return sendOptionalDataFieldsIL;
        }

        SendOptionalDataFieldsAH_Type GetSendOptionalDataFieldsAH(string msg)
        {
            SendOptionalDataFieldsAH_Type sendOptionalDataFieldsAH = new SendOptionalDataFieldsAH_Type();
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                readCond++; //Sumo 1 porque el valor inicial arranca en 0 en lugar de 1.
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[7] == 1)
                        sendOptionalDataFieldsAH.SendFieldA = true;
                    if (arrByte[6] == 1)
                        sendOptionalDataFieldsAH.SendFieldB = true;
                    if (arrByte[5] == 1)
                        sendOptionalDataFieldsAH.SendFieldC = true;
                    if (arrByte[4] == 1)
                        sendOptionalDataFieldsAH.SendFieldD = true;
                    if (arrByte[3] == 1)
                        sendOptionalDataFieldsAH.SendFieldE = true;
                    if (arrByte[2] == 1)
                        sendOptionalDataFieldsAH.SendFieldF = true;
                    if (arrByte[1] == 1)
                        sendOptionalDataFieldsAH.SendFieldG = true;
                    if (arrByte[0] == 1)
                        sendOptionalDataFieldsAH.SendFieldH = true;
                }
            }
            return sendOptionalDataFieldsAH;
        }

        SendGeneralPurposeBuffersBAndOrC_Type GetSendGeneralPurposeBuffersBAndOrC(string msg)
        {
            SendGeneralPurposeBuffersBAndOrC_Type sendGeneralPurposeBuffersBAndOrC = new SendGeneralPurposeBuffersBAndOrC_Type();
            sendGeneralPurposeBuffersBAndOrC.SendBufferB = false;
            sendGeneralPurposeBuffersBAndOrC.SendBufferC = false;
            //msg|SendBufferB|SendBufferC|
            //000|     0     |      0    |
            //001|     1     |      0    |
            //002|     0     |      1    |
            //003|     1     |      1    |
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[6] == 1)
                        sendGeneralPurposeBuffersBAndOrC.SendBufferB = true;
                    if (arrByte[7] == 1)
                        sendGeneralPurposeBuffersBAndOrC.SendBufferC = true;
                }
            }
            return sendGeneralPurposeBuffersBAndOrC;
        }

        private SendPINBufferADataSelectExtendedFormat_Type GetSendPINBufferADataSelectExtendedFormat(string msg)
        {
            SendPINBufferADataSelectExtendedFormat_Type sendPINBufferADataSelectExtendedFormat = new SendPINBufferADataSelectExtendedFormat_Type();
            sendPINBufferADataSelectExtendedFormat.SendPINBufferA = false;
            sendPINBufferADataSelectExtendedFormat.SelectExtendedFormat = false;
            //msg|SendPINBufferA|SelectExtendedFormat|
            //000|       0      |         0          |
            //001|       1      |         0          |
            //128|       0      |         1          |
            //129|       1      |         1          |
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[0] == 1)
                        sendPINBufferADataSelectExtendedFormat.SelectExtendedFormat = true;
                    if (arrByte[7] == 1)
                        sendPINBufferADataSelectExtendedFormat.SendPINBufferA = true;
                }
            }
            return sendPINBufferADataSelectExtendedFormat;
        }

        private SendTrack1Or3Data_Type GetSendTrack1Or3(string msg)
        {
            SendTrack1Or3Data_Type sendTrack1Or3Data = new SendTrack1Or3Data_Type();
            sendTrack1Or3Data.SendTrack1Data = false;
            sendTrack1Or3Data.SendTrack3Data = false;
            sendTrack1Or3Data.SendCIM86VerifyCodeAndData = false;
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[5] == 1)
                        sendTrack1Or3Data.SendCIM86VerifyCodeAndData = true;
                    if (arrByte[6] == 1)
                        sendTrack1Or3Data.SendTrack1Data = true;
                    if (arrByte[7] == 1)
                        sendTrack1Or3Data.SendTrack3Data = true;
                }
            }
            return sendTrack1Or3Data;
        }

        private ReadCondition_Type GetReadCondition(string msg)
        {
            ReadCondition_Type readCondition = new ReadCondition_Type();
            readCondition.ReadTrack1 = false;
            readCondition.ReadTrack2 = false;
            readCondition.ReadTrack3 = false;
            readCondition.ChipConnectReadSmartData = false;
            byte[] arrByte;
            int readCond;
            if (int.TryParse(msg, out readCond))
            {
                if (Utilities.Utils.DecToBinArray(readCond, out arrByte))
                {
                    if (arrByte[4] == 1)
                        readCondition.ChipConnectReadSmartData = true;
                    if (arrByte[5] == 1)
                        readCondition.ReadTrack1 = true;
                    if (arrByte[6] == 1)
                        readCondition.ReadTrack2 = true;
                    if (arrByte[7] == 1)
                        readCondition.ReadTrack3 = true;
                }
            }
            return readCondition;
        }
        #endregion "STATES"

        //3)- ENHANCED PARAMETERS
        public EnhancedConfigurationParametersData_Type EnhancedParametersMessageParser(string msg)
        {
            EnhancedConfigurationParametersData_Type EnhancedConfigurationParameters = new EnhancedConfigurationParametersData_Type();
            List<Option_Type> ListOfOptions = new List<Option_Type>();
            List<Timer_Type> ListOfTimers = new List<Timer_Type>();
            string[] DataInPut;
            string op = string.Empty;
            string value = string.Empty;
            Option_Type option;
            int a = 0, i = 0;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 6)
                {
                    for (int j = 0; j <= DataInPut[5].Length - 4; j = j + 5)
                    {
                        a = j;
                        op = DataInPut[5].Substring(j, 2);
                        value = DataInPut[5].Substring(j + 2, 3);
                        option = new Option_Type();
                        switch (op)
                        {
                            case "01":
                                {
                                    CameraControlOptionChar3_Type CameraControlOption = new CameraControlOptionChar3_Type();
                                    CameraControlOption.AutomaticPictureTaking = value.Equals("001") ? true : false;
                                    option.ItemElementName = ItemChoiceOption_Type.CameraControlOption;
                                    option.Item = CameraControlOption;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "02":
                                {
                                    if (value.Equals("001"))
                                        option.Item = true;
                                    else if (value.Equals("002"))
                                        option.Item = false;
                                    option.ItemElementName = ItemChoiceOption_Type.AutoVoiceOn;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "03":
                                {
                                    i = 0;
                                    SupervisorJournalDateFormat_Type supervisorJournalDateFormat = new SupervisorJournalDateFormat_Type();
                                    if (int.TryParse(value, out i))
                                        supervisorJournalDateFormat = (SupervisorJournalDateFormat_Type)i;
                                    option.Item = supervisorJournalDateFormat;
                                    option.ItemElementName = ItemChoiceOption_Type.SupervisorJournalDateFormat;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "04":
                                {
                                    int rollWidth = 0;
                                    if (int.TryParse(value, out rollWidth))
                                        option.Item = rollWidth;
                                    option.ItemElementName = ItemChoiceOption_Type.RollWidth;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "05":
                                {
                                    int leftPrintColumn = 0;
                                    if (int.TryParse(value, out leftPrintColumn))
                                        option.Item = leftPrintColumn;
                                    option.ItemElementName = ItemChoiceOption_Type.LeftPrintColumn;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "07":
                                {
                                    i = 0;
                                    Track1Format_Type track1Format = new Track1Format_Type();
                                    if (int.TryParse(value, out i))
                                        track1Format = (Track1Format_Type)i;
                                    option.Item = track1Format;
                                    option.ItemElementName = ItemChoiceOption_Type.Track1Format;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "12":
                                {
                                    if (value.Equals("000"))
                                        option.Item = false;
                                    else if (value.Equals("001"))
                                        option.Item = true;
                                    option.ItemElementName = ItemChoiceOption_Type.TransmitSpecificCommandRejectStatus;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                            case "77":
                                {
                                    option.Item = value;
                                    option.ItemElementName = ItemChoiceOption_Type.AANDCNextStateNumber;
                                    ListOfOptions.Add(option);
                                    break;
                                }
                        }
                    }
                    EnhancedConfigurationParameters.OptionList = ListOfOptions.ToArray();
                    //Agrego los timers
                    string tr = "";
                    int t, item;
                    Timer_Type timer;
                    for (int j = 0; j <= DataInPut[6].Length - 4; j = j + 5)
                    {
                        a = j;
                        tr = DataInPut[6].Substring(j, 2);
                        int.TryParse(tr, out item);
                        value = DataInPut[6].Substring(j + 2, 3);
                        timer = new Timer_Type();
                        timer.ItemElementName = (ItemChoiceTimer_Type)item;
                        if (int.TryParse(value, out t))
                        {
                            timer.Item = t;
                            ListOfTimers.Add(timer);
                        }
                    }
                    EnhancedConfigurationParameters.TimerList = ListOfTimers.ToArray();
                }
                else { throw new Exception("EnhancedConfigurationParametersParser() error: insufficient data to parse unsolicitedStatusMessage"); }
            }
            catch (Exception ex) { throw new Exception(string.Format("EnhancedConfigurationParametersParser(): {0}", ex.Message)); }
            return EnhancedConfigurationParameters;
        }

        //4)- INTERACTIVE TRANSACTION RESPONSE PARSE
        public InteractiveTransactionResponse_Type ITRMessageParser(string msg)
        {
            InteractiveTransactionResponse_Type interactiveTransactionResponse = null;
            string[] DataInPut;
            int i = 0;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 5)//Hasta buffer C es mandatorio
                {
                    interactiveTransactionResponse = new InteractiveTransactionResponse_Type();
                    if (DataInPut[3].Length > 1)
                    {
                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                            interactiveTransactionResponse.DisplayFlag = (InteractiveTransactionResponseDisplayFlag_Type)i;
                        interactiveTransactionResponse.Activations = this.GetInteractiveTransactionResponseActivations(DataInPut[3].Substring(2));
                        interactiveTransactionResponse.ScreenTimerField = DataInPut[4];
                        interactiveTransactionResponse.ScreenData = this.GetScreenData(DataInPut[5]).Command;
                    }
                }
                return interactiveTransactionResponse;
            }
            catch (Exception ex) { throw new Exception(string.Format("TransactionRequestMessageParser(): {0}", ex.Message)); }
        }

        //5)- TRANSACTION REPLY PARSE
        public TransactionReplyCommand_Type TransactionReplyMessageParser(string msg)
        {
            TransactionReplyCommand_Type transactionReply = null;
            string[] DataInPut;
            int i = 0;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 5)//Hasta buffer C es mandatorio
                {
                    transactionReply = new TransactionReplyCommand_Type();
                    transactionReply.LogicalUnitNumber = DataInPut[1];
                    transactionReply.TimeVariantOrSequenceNumber = new TransactionReplyCommand_TypeTimeVariantOrSequenceNumber();
                    if (DataInPut[2].Length == 8)
                        transactionReply.TimeVariantOrSequenceNumber.ItemElementName = ItemChoiceTransactionReplyCommand_TypeTimeVariantOrSequenceNumber.TimeVariant;
                    else
                        transactionReply.TimeVariantOrSequenceNumber.ItemElementName = ItemChoiceTransactionReplyCommand_TypeTimeVariantOrSequenceNumber.SequenceNumber;
                    transactionReply.TimeVariantOrSequenceNumber.Item = DataInPut[2];
                    if (DataInPut[3].Length > 2)
                        transactionReply.NextStateId = DataInPut[3];
                    else
                        throw new Exception(string.Format("Field \"3\": {0} invalid.", DataInPut[3]));
                    if (DataInPut[4].Length > 7)//NotesToDispense
                    {
                        transactionReply.NotesToDispense = new TransactionReplyCommand_TypeNotesToDispense();
                        i = 0;
                        if (int.TryParse(DataInPut[4].Substring(6, 2), out i))
                        {
                            transactionReply.NotesToDispense.FromCassetteType4 = i;
                            transactionReply.NotesToDispense.FromCassetteType4Specified = i == 0 ? false : true;
                        }
                    }
                    if (DataInPut[4].Length > 5)//NotesToDispense
                    {
                        transactionReply.NotesToDispense = new TransactionReplyCommand_TypeNotesToDispense();
                        i = 0;
                        if (int.TryParse(DataInPut[4].Substring(4, 2), out i))
                        {
                            transactionReply.NotesToDispense.FromCassetteType3 = i;
                            transactionReply.NotesToDispense.FromCassetteType3Specified = i == 0 ? false : true;
                        }
                    }
                    if (DataInPut[4].Length > 3)//NotesToDispense
                    {
                        transactionReply.NotesToDispense = new TransactionReplyCommand_TypeNotesToDispense();
                        i = 0;
                        if (int.TryParse(DataInPut[4].Substring(2, 2), out i))
                        {
                            transactionReply.NotesToDispense.FromCassetteType2 = i;
                            transactionReply.NotesToDispense.FromCassetteType2Specified = i == 0 ? false : true;
                        }
                    }
                    if (DataInPut[4].Length > 1)//NotesToDispense
                    {
                        transactionReply.NotesToDispense = new TransactionReplyCommand_TypeNotesToDispense();
                        i = 0;
                        if (int.TryParse(DataInPut[4].Substring(0, 2), out i))
                        {
                            transactionReply.NotesToDispense.FromCassetteType1 = i;
                        }
                    }
                    if (DataInPut[5].Length > 7)//Update screen
                    {
                        transactionReply.TransactionSerialNumber = DataInPut[5].Substring(0, 4);
                        i = 0;
                        string hexaValue = Utilities.Utils.StrToHex(DataInPut[5].Substring(4, 1));
                        i = Utilities.Utils.HexToInt(hexaValue);
                        transactionReply.FunctionId = (FunctionId_Type)i;
                        //Screen display
                        transactionReply.ScreenDisplay = new ScreenDisplay_Type();
                        transactionReply.ScreenDisplay.ScreenNumberToDisplay = new ExtendedScreenNumber_Type();
                        transactionReply.ScreenDisplay.ScreenNumberToDisplay.ItemElementName = ItemChoiceExtendedScreenNumber_Type.ScreenNumber;
                        transactionReply.ScreenDisplay.ScreenNumberToDisplay.Item = DataInPut[5].Substring(5, 3);
                        //Screen update
                        Digits2GroupScreen_Type digits2GroupScreen = new Digits2GroupScreen_Type();
                        digits2GroupScreen.ScreenNumber = DataInPut[5].Substring(8, 3);
                        digits2GroupScreen.ScreenData = this.GetScreenData(DataInPut[5].Substring(11)).Command;
                        transactionReply.ScreenDisplay.ScreenUpdates = new ScreenUpdates_Type();
                        transactionReply.ScreenDisplay.ScreenUpdates.ScreenUpdate = new ScreenUpdate_Type();
                        transactionReply.ScreenDisplay.ScreenUpdates.ScreenUpdate.Item = digits2GroupScreen;
                    }
                    else
                        throw new Exception(string.Format("Field \"5\": {0} invalid.", DataInPut[5]));
                    if (DataInPut[6].Length > 2)
                    {
                        transactionReply.MessageCoOrdinationNumber = DataInPut[6].Substring(0, 1);
                        i = 0;
                        if (int.TryParse(DataInPut[6].Substring(1, 1), out i))
                            transactionReply.CardReturnRetainFlag = (CardReturnRetainFlag_Type)i;
                        string[] printingInstructions = DataInPut[6].Substring(2).Split(Entities.Const.GS);
                        if (printingInstructions.Length > 0)
                        {
                            for (int j = 0; j < printingInstructions.Length; j++)
                            {
                                if (j == 0)
                                {
                                    TransactionReplyCommand_TypePrintingInstruction1 printingInstruction1 = new TransactionReplyCommand_TypePrintingInstruction1();
                                    if (int.TryParse(printingInstructions[j].Substring(0, 1), out i))
                                    {
                                        transactionReply.PrintingInstruction1 = new TransactionReplyCommand_TypePrintingInstruction1();
                                        transactionReply.PrintingInstruction1.Printer = (PrinterFlag_Type)i;
                                        transactionReply.PrintingInstruction1.PrinterData = this.GetPrinterData(printingInstructions[j].Substring(1));
                                    }
                                }
                                if (j == 1)
                                {
                                    TransactionReplyCommand_TypePrintingInstruction2 printingInstruction2 = new TransactionReplyCommand_TypePrintingInstruction2();
                                    if (int.TryParse(printingInstructions[j].Substring(0, 1), out i))
                                    {
                                        transactionReply.PrintingInstruction2 = new TransactionReplyCommand_TypePrintingInstruction2();
                                        transactionReply.PrintingInstruction2.Printer = (PrinterFlag_Type)i;
                                        transactionReply.PrintingInstruction2.PrinterData = this.GetPrinterData(printingInstructions[j].Substring(1));
                                    }
                                }
                                if (j == 2)
                                {
                                    TransactionReplyCommand_TypePrintingInstruction3 printingInstruction3 = new TransactionReplyCommand_TypePrintingInstruction3();
                                    if (int.TryParse(printingInstructions[j].Substring(0, 1), out i))
                                    {
                                        transactionReply.PrintingInstruction3 = new TransactionReplyCommand_TypePrintingInstruction3();
                                        transactionReply.PrintingInstruction3.Printer = (PrinterFlag_Type)i;
                                        transactionReply.PrintingInstruction3.PrinterData = this.GetPrinterData(printingInstructions[j].Substring(1));
                                    }
                                }
                            }
                        }
                    }
                    else
                        throw new Exception(string.Format("Field \"6\": {0} invalid.", DataInPut[6]));

                }
                return transactionReply;
            }
            catch (Exception ex) { throw new Exception(string.Format("TransactionRequestMessageParser(): {0}", ex.Message)); }
        }

        //6)- TRANSACTION REQUEST PARSE
        public TransactionRequest_Type TransactionRequestMessageParser(string msg)
        {
            TransactionRequest_Type transactionRequest = null;
            string[] DataInPut;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 11)//Hasta buffer C es mandatorio
                {
                    transactionRequest = new TransactionRequest_Type();
                    transactionRequest.LogicalUnitNumber = DataInPut[1];
                    transactionRequest.TimeVariant = DataInPut[3];
                    transactionRequest.TopOfReceipt = DataInPut[4].Substring(0, 1).Equals("0", StringComparison.Ordinal) ? TopOfReceipt_Type.WithoutPrinting : TopOfReceipt_Type.WithPrinting;
                    transactionRequest.MessageCoordinatorNumber = DataInPut[4].Substring(1);
                    transactionRequest.Track2Data = DataInPut[5];
                    transactionRequest.Track3Data = DataInPut[6];
                    transactionRequest.OperationCodeData = DataInPut[7];
                    transactionRequest.AmountEntry = DataInPut[8];
                    transactionRequest.PinBufferA = DataInPut[9];
                    transactionRequest.GeneralPurposeBufferB = DataInPut[10];
                    transactionRequest.GeneralPurposeBufferC = DataInPut[11];
                    if (DataInPut.Length > 11)
                    {
                        for (int i = 12; i < DataInPut.Length; i++)
                        {
                            if (DataInPut[i].ToString().Length > 0)
                            {
                                string a = DataInPut[i].Substring(0, 1);
                                switch (DataInPut[i].Substring(0, 1))
                                {
                                    case "1":
                                        {
                                            transactionRequest.Track1Data = new Track1Data_Type();
                                            transactionRequest.Track1Data.Track1 = DataInPut[i];
                                            break;
                                        }
                                    case "2":
                                        {
                                            transactionRequest.LastTransactionStatusData = new LastTransactionStatusData_Type();
                                            transactionRequest.LastTransactionStatusData.LastTransaction = new LastTransaction_Type();
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionSerialNumber = "";
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastStatusIssued = LastStatusIssued_Type.NoneSent;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionNotesDispensedFromCassette1 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionNotesDispensedFromCassette2 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionNotesDispensedFromCassette3 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionNotesDispensedFromCassette4 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.AdditionalLastTransactionNotesDispensed = new AdditionalLastTransactionNotesDispensed_Type();
                                            transactionRequest.LastTransactionStatusData.LastTransaction.AdditionalLastTransactionNotesDispensed.LastTransactionNotesDispensedFromCassette5 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.AdditionalLastTransactionNotesDispensed.LastTransactionNotesDispensedFromCassette6 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.AdditionalLastTransactionNotesDispensed.LastTransactionNotesDispensedFromCassette7 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinageAmountDispensed = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinageAmountDispensedSpecified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette1 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette1Specified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette2 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette2Specified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette3 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette3Specified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette4 = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinsDispensedFromCassette4Specified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastCashDepositTransactionDirection = LastCashDepositTransactionDirection_Type.VaultDirection;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastCashDepositTransactionDirectionSpecified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastCoinDepositTransactionDirection = LastCoinDepositTransactionDirection_Type.NotBCADeposit;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastCoinDepositTransactionDirectionSpecified = false;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinDeposit = new LastTransactionCoinDeposit_Type();
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinDeposit.NumberOfCoinsRefunded = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinDeposit.NumberOfCoinsRejected = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinDeposit.NumberOfCoinsEncashed = 0;
                                            transactionRequest.LastTransactionStatusData.LastTransaction.LastTransactionCoinDeposit.NumberOfCoinsToEscrow = 0;
                                            break;
                                        }
                                    case "w":
                                        {
                                            string bufferData = DataInPut[i].Substring(1);
                                            int j = 0;
                                            //if (bufferData.Length % 4 == 0)
                                            //{
                                            //    transactionRequestMsg.BNAFields = new NoteType_Type[bufferData.Length / 4];
                                            //    char[] array = DataInPut[i].ToArray();
                                            //    for (int k = 0; k < bufferData.Length / 4; k++)
                                            //    {
                                            //        transactionRequestMsg.BNAFields[k] = new NoteType_Type();
                                            //        transactionRequestMsg.BNAFields[k].DenominationCode = bufferData.Substring(j, 2);
                                            //        transactionRequestMsg.BNAFields[k].Item = bufferData.Substring(j + 2, 2);
                                            //        j = j + 4;
                                            //    }
                                            //}
                                            if (bufferData.Length % 5 == 0)
                                            {
                                                transactionRequest.BNAFields = new NoteType_Type[bufferData.Length / 5];
                                                char[] array = DataInPut[i].ToArray();
                                                for (int k = 0; k < bufferData.Length / 5; k++)
                                                {
                                                    transactionRequest.BNAFields[k] = new NoteType_Type();
                                                    transactionRequest.BNAFields[k].DenominationCode = bufferData.Substring(j, 2);
                                                    transactionRequest.BNAFields[k].Item = bufferData.Substring(j + 2, 3);
                                                    j = j + 5;
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
                return transactionRequest;
            }
            catch (Exception ex) { throw new Exception(string.Format("TransactionRequestMessageParser(): {0}", ex.Message)); }
        }

        //7)- UNSOLICITED STATUS PARSE
        public UnsolicitedStatusMessage_Type UnSolicitedStatusMessageParser(string msg)
        {
            UnsolicitedStatusMessage_Type unsolicitedStatusMessage;
            string[] DataInPut;
            int i = 0;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 2)
                {
                    unsolicitedStatusMessage = new UnsolicitedStatusMessage_Type();
                    unsolicitedStatusMessage.LogicalUnitNumber = DataInPut[1];
                    unsolicitedStatusMessage.DeviceFaultStatusInformation = new DeviceFaultStatusInformation_Type();
                    string a = DataInPut[3].Substring(0, 1);
                    switch (DataInPut[3].Substring(0, 1))
                    {
                        case "B"://PowerFailureStatus                           
                            {
                                PowerFailure_Type powerFailureStatus = new PowerFailure_Type();
                                powerFailureStatus.DeviceStatus = DataInPut[3].Substring(1);
                                unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = powerFailureStatus;
                                break;
                            }
                        case "D"://Card Reader Writer Status
                            {
                                if (DataInPut.Length > 6)
                                {
                                    CardReaderWriterStatus_Type cardReaderWriterStatus = new CardReaderWriterStatus_Type();
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 1)
                                    {
                                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                                            cardReaderWriterStatus.DeviceStatus = (CardReaderWriterDeviceStatus_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse TX / Device status D"); }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[4], out i))
                                        {
                                            cardReaderWriterStatus.ErrorSeverity = new CardReaderWriterErrorSeverity_Type();
                                            cardReaderWriterStatus.ErrorSeverity.MCRWErrorSeverity = (ErrorSeverity_Type)i;
                                        }
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Error severity status D"); }
                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        cardReaderWriterStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        cardReaderWriterStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].Substring(0, 2);
                                        cardReaderWriterStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status D"); }
                                    //E)- Supplies status
                                    if (DataInPut[6].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[6], out i))
                                            cardReaderWriterStatus.CardCaptureBinStatus = (CardCaptureBinStatus_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Supplies status D"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = cardReaderWriterStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse CardReaderWriterStatus D"); }
                                break;
                            }
                        case "E"://CashHandlerStatus
                            {
                                if (DataInPut.Length > 6)
                                {
                                    CashHandlerStatus_Type cashHandlerStatus = new CashHandlerStatus_Type();
                                    cashHandlerStatus.DeviceStatus = new CashHandlerDeviceStatus_Type();
                                    cashHandlerStatus.DeviceStatus.NotesDispensed = new CashHandlerDeviceStatus_TypeNotesDispensed();
                                    cashHandlerStatus.DeviceStatus.DeviceOrTransactionStatus = new CashHandlerTransactionDeviceStatus_Type();
                                    cashHandlerStatus.DeviceIdentificationGraphic = DataInPut[3].ToString().Substring(0, 1);
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 9)
                                    {
                                        if (int.TryParse(DataInPut[3].ToString().Substring(1, 1), out i))
                                            cashHandlerStatus.DeviceStatus.DeviceOrTransactionStatus = (CashHandlerTransactionDeviceStatus_Type)i;
                                        if (int.TryParse(DataInPut[3].ToString().Substring(2, 2), out i))
                                            cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette1 = i;
                                        if (int.TryParse(DataInPut[3].ToString().Substring(4, 2), out i))
                                            cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette2 = i;
                                        if (int.TryParse(DataInPut[3].ToString().Substring(6, 2), out i))
                                            cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette3 = i;
                                        if (int.TryParse(DataInPut[3].ToString().Substring(8, 2), out i))
                                            cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette4 = i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Device status E"); }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 4)
                                    {
                                        cashHandlerStatus.DeviceErrorSeverity = new CashHandlerErrorSeverity_Type();
                                        if (int.TryParse(DataInPut[4].ToString().Substring(0, 1), out i))
                                            cashHandlerStatus.DeviceErrorSeverity.CompleteDevice = (ErrorSeverity_Type)i;
                                        if (int.TryParse(DataInPut[4].ToString().Substring(1, 1), out i))
                                            cashHandlerStatus.DeviceErrorSeverity.CassetteType1ErrorSeverity = (ErrorSeverity_Type)i;
                                        if (int.TryParse(DataInPut[4].ToString().Substring(2, 1), out i))
                                            cashHandlerStatus.DeviceErrorSeverity.CassetteType1ErrorSeverity = (ErrorSeverity_Type)i;
                                        if (int.TryParse(DataInPut[4].ToString().Substring(3, 1), out i))
                                            cashHandlerStatus.DeviceErrorSeverity.CassetteType1ErrorSeverity = (ErrorSeverity_Type)i;
                                        if (int.TryParse(DataInPut[4].ToString().Substring(4, 1), out i))
                                            cashHandlerStatus.DeviceErrorSeverity.CassetteType1ErrorSeverity = (ErrorSeverity_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Error severity E"); }

                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        cashHandlerStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        cashHandlerStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].ToString().Substring(0, 2);
                                        cashHandlerStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].ToString().Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status E"); }
                                    //E)- Supplies status
                                    if (DataInPut[6].ToString().Length > 4)
                                    {
                                        cashHandlerStatus.SuppliesStatus = new CashHandlerSuppliesStatus_Type();
                                        if (int.TryParse(DataInPut[6].ToString().Substring(0, 1), out i))
                                            cashHandlerStatus.SuppliesStatus.RejectBinState = (RejectBinState_Type)i;
                                        if (int.TryParse(DataInPut[6].ToString().Substring(1, 1), out i))
                                            cashHandlerStatus.SuppliesStatus.CassetteType1State = (CassetteTypeState_Type)i;
                                        if (int.TryParse(DataInPut[6].ToString().Substring(2, 1), out i))
                                            cashHandlerStatus.SuppliesStatus.CassetteType2State = (CassetteTypeState_Type)i;
                                        if (int.TryParse(DataInPut[6].ToString().Substring(3, 1), out i))
                                            cashHandlerStatus.SuppliesStatus.CassetteType3State = (CassetteTypeState_Type)i;
                                        if (int.TryParse(DataInPut[6].ToString().Substring(4, 1), out i))
                                            cashHandlerStatus.SuppliesStatus.CassetteType4State = (CassetteTypeState_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Supplies status E"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = cashHandlerStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse CashHandlerStatus E"); }
                                break;
                            }
                        case "P"://SensorsStatus
                            {
                                SensorsStatus_Type sensorsStatus = new SensorsStatus_Type();
                                switch (DataInPut[3].Substring(1, 1))
                                {
                                    case "1": //Sensors Status TI Sensor Change (exchage cassette)
                                        {
                                            TISensorAlarmStateChange_Type sensorsStatusTISensorChange = new TISensorAlarmStateChange_Type();
                                            if (DataInPut[3].Length > 12)
                                            {
                                                if (int.TryParse(DataInPut[3].Substring(2, 1), out i))
                                                    sensorsStatusTISensorChange.VibrationAndOrHeatSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(3, 1), out i))
                                                    sensorsStatusTISensorChange.DoorContactSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(4, 1), out i))
                                                    sensorsStatusTISensorChange.SilentSignalSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(5, 1), out i))
                                                    sensorsStatusTISensorChange.ElectronicsEnclosureSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(6, 1), out i))
                                                    sensorsStatusTISensorChange.DepositBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(7, 1), out i))
                                                    sensorsStatusTISensorChange.CardBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(8, 1), out i))
                                                    sensorsStatusTISensorChange.CurrencyRejectBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(9, 1), out i))
                                                    sensorsStatusTISensorChange.CurrencyCassettePosition1 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(10, 1), out i))
                                                    sensorsStatusTISensorChange.CurrencyCassettePosition2 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(11, 1), out i))
                                                    sensorsStatusTISensorChange.CurrencyCassettePosition3 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(12, 1), out i))
                                                    sensorsStatusTISensorChange.CurrencyCassettePosition4 = (BitCodedInOut_Type)i;
                                            }
                                            else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse sensorsStatusTISensorChange P"); }
                                            sensorsStatus.Item = sensorsStatusTISensorChange;
                                            sensorsStatus.ItemElementName = ItemChoiceSensorsStatus_Type.SensorsStatusTISensorChange;
                                            break;
                                        }
                                    case "2": //Sensors Status Mode Change (supervisor)
                                        {
                                            ModeChange_Type sensorsStatusModeChange = new ModeChange_Type();
                                            if (DataInPut[3].Length > 2)
                                            {
                                                if (DataInPut[3].Substring(2, 1).Equals("1", StringComparison.Ordinal))
                                                    sensorsStatusModeChange.CurrentMode = CurrentMode_Type.SupervisorModeEntry;
                                                if (DataInPut[3].Substring(2, 1).Equals("0", StringComparison.Ordinal))
                                                    sensorsStatusModeChange.CurrentMode = CurrentMode_Type.SupervisorModeExit;
                                            }
                                            else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse sensorsStatusModeChange P"); }
                                            sensorsStatus.Item = sensorsStatusModeChange;
                                            sensorsStatus.ItemElementName = ItemChoiceSensorsStatus_Type.SensorsStatusModeChange;
                                            break;
                                        }
                                    case "3": //Sensors Status Alarm State Change (doors)
                                        {
                                            TISensorAlarmStateChange_Type sensorsStatusAlarmStateChange = new TISensorAlarmStateChange_Type();
                                            if (DataInPut[3].Length > 13)
                                            {
                                                if (int.TryParse(DataInPut[3].Substring(3, 1), out i))
                                                    sensorsStatusAlarmStateChange.VibrationAndOrHeatSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(4, 1), out i))
                                                    sensorsStatusAlarmStateChange.DoorContactSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(5, 1), out i))
                                                    sensorsStatusAlarmStateChange.SilentSignalSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(6, 1), out i))
                                                    sensorsStatusAlarmStateChange.ElectronicsEnclosureSensor = (BitCodedActiveInactive_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(7, 1), out i))
                                                    sensorsStatusAlarmStateChange.DepositBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(8, 1), out i))
                                                    sensorsStatusAlarmStateChange.CardBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(9, 1), out i))
                                                    sensorsStatusAlarmStateChange.CurrencyRejectBin = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(10, 1), out i))
                                                    sensorsStatusAlarmStateChange.CurrencyCassettePosition1 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(11, 1), out i))
                                                    sensorsStatusAlarmStateChange.CurrencyCassettePosition2 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(12, 1), out i))
                                                    sensorsStatusAlarmStateChange.CurrencyCassettePosition3 = (BitCodedInOut_Type)i;
                                                if (int.TryParse(DataInPut[3].Substring(13, 1), out i))
                                                    sensorsStatusAlarmStateChange.CurrencyCassettePosition4 = (BitCodedInOut_Type)i;
                                            }
                                            else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse sensorsStatusAlarmStateChange P"); }
                                            sensorsStatus.Item = sensorsStatusAlarmStateChange;
                                            sensorsStatus.ItemElementName = ItemChoiceSensorsStatus_Type.SensorsStatusAlarmStateChange;
                                            break;
                                        }
                                    case "5": //Sensors Status Full TI Sensor Alarm State Change
                                        {
                                            FullTISensorAlarmStateChange_Type sensorsStatusFullTISensorAlarmStateChange = new FullTISensorAlarmStateChange_Type();
                                            //TODO: falta parseo
                                            sensorsStatus.Item = sensorsStatusFullTISensorAlarmStateChange;
                                            sensorsStatus.ItemElementName = ItemChoiceSensorsStatus_Type.SensorsStatusFullTISensorAlarmStateChange;
                                            break;
                                        }
                                    case "6": //Flexible TI and alarms change detected
                                        {
                                            ExtendedTamperIndicator_Type flexibleTIAndAlarmsChangeDetected = new ExtendedTamperIndicator_Type();
                                            //TODO: falta parseo
                                            sensorsStatus.Item = flexibleTIAndAlarmsChangeDetected;
                                            sensorsStatus.ItemElementName = ItemChoiceSensorsStatus_Type.FlexibleTIAndAlarmsChangeDetected;
                                            break;
                                        }
                                }
                                unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = sensorsStatus;
                                break;
                            }
                        case "w": //Bunch Note Acceptor Status
                            {
                                if (DataInPut.Length > 6)
                                {
                                    BunchNoteAcceptorStatus_Type bunchNoteAcceptorStatus = new BunchNoteAcceptorStatus_Type();
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 1)
                                    {
                                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                                        {
                                            bunchNoteAcceptorStatus.DeviceStatus = new BNADeviceStatus_Type();
                                            bunchNoteAcceptorStatus.DeviceStatus.Error = (BNADeviceStatusError_Type)i;
                                            bunchNoteAcceptorStatus.DeviceStatus.EscrowCounts = new DenominationCounts_Type();
                                            bunchNoteAcceptorStatus.DeviceStatus.EscrowCounts.Denomination1 = 1;
                                            //TODO: completar EscrowCounts
                                            bunchNoteAcceptorStatus.DeviceStatus.VaultedCounts = new DenominationCounts_Type();
                                            bunchNoteAcceptorStatus.DeviceStatus.VaultedCounts.Denomination1 = 1;
                                            //TODO: completar VaultedCounts
                                            bunchNoteAcceptorStatus.DeviceStatus.ReturnedCounts = new DenominationCounts_Type();
                                            bunchNoteAcceptorStatus.DeviceStatus.ReturnedCounts.Denomination1 = 1;
                                            //TODO: completar ReturnedCounts        
                                            bunchNoteAcceptorStatus.DeviceStatus.TotalNumberOfBillsReturned = 0;//TODO: completar
                                            bunchNoteAcceptorStatus.DeviceStatus.TotalNumberOfBillsInEscrow = 0;//TODO: completar
                                            bunchNoteAcceptorStatus.DeviceStatus.TotalNumberOfBillsJustVaulted = 0; //TODO: completar
                                        }
                                    }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[4], out i))
                                        {
                                            bunchNoteAcceptorStatus.ErrorSeverity = new BunchNoteAcceptorFitnessData_Type();
                                            bunchNoteAcceptorStatus.ErrorSeverity.CashAcceptorFitness = (ErrorSeverity_Type)i;
                                            //bunchNoteAcceptorStatus.ErrorSeverity.CassetteFitnessData = new CassetteFitnessData_Type();
                                        }
                                    }
                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        bunchNoteAcceptorStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        bunchNoteAcceptorStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].Substring(0, 2);
                                        bunchNoteAcceptorStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status W"); }
                                    //E)- Supplies status
                                    if (DataInPut[6].ToString().Length > 0)
                                    {
                                        if (DataInPut[6].ToString().Length > 4)
                                        {
                                            bunchNoteAcceptorStatus.SuppliesStatus = new BNASuppliesStatus_Type();
                                            if (int.TryParse(DataInPut[6].ToString().Substring(0, 1), out i))
                                                bunchNoteAcceptorStatus.SuppliesStatus.BNAState = (BNAState_Type)i;
                                            bunchNoteAcceptorStatus.SuppliesStatus.CassetteSupplyStatus = new CassetteSupplyStatus_Type[4];
                                            if (int.TryParse(DataInPut[6].ToString().Substring(1, 1), out i))
                                            {
                                                //TODO
                                                bunchNoteAcceptorStatus.SuppliesStatus.CassetteSupplyStatus[0] = new CassetteSupplyStatus_Type();
                                                bunchNoteAcceptorStatus.SuppliesStatus.CassetteSupplyStatus[0].CassetteSypplyState = (BNAState_Type)i;
                                                bunchNoteAcceptorStatus.SuppliesStatus.CassetteSupplyStatus[0].CassetteType = 1;
                                            }
                                        }
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Supplies status W"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = bunchNoteAcceptorStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse BunchNoteAcceptorStatus W"); }
                                break;
                            }
                        case "L"://Encryptor Status
                            {
                                if (DataInPut.Length > 5)
                                {
                                    EncryptorStatus_Type encryptorStatus = new EncryptorStatus_Type();
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 1)
                                    {
                                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                                            encryptorStatus.DeviceStatus = (EncryptorDeviceStatus_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse TX / Device status L"); }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[4], out i))
                                        {
                                            encryptorStatus.DeviceErrorSeverity = (ErrorSeverity_Type)i;
                                        }
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Error severity status L"); }
                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        encryptorStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        encryptorStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].Substring(0, 2);
                                        encryptorStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status L"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = encryptorStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse EncryptorStatus L"); }
                                break;
                            }
                        case "G"://Receipt Printer Status
                            {
                                if (DataInPut.Length > 6)
                                {
                                    ReceiptPrinterStatus_Type receiptPrinterStatus = new ReceiptPrinterStatus_Type();
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 1)
                                    {
                                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                                            receiptPrinterStatus.DeviceStatus = (ReceiptPrinterDeviceStatus_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse TX / Device status G"); }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[4], out i))
                                            receiptPrinterStatus.ErrorSeverity = (ErrorSeverity_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Error severity status G"); }
                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        receiptPrinterStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        receiptPrinterStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].Substring(0, 2);
                                        receiptPrinterStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status G"); }
                                    //E)- Supplies status
                                    if (DataInPut[6].ToString().Length > 3)
                                    {
                                        receiptPrinterStatus.SuppliesStatus = new PrinterSuppliesStatus_Type();
                                        if (int.TryParse(DataInPut[6].Substring(0, 1), out i))
                                        {
                                            receiptPrinterStatus.SuppliesStatus.PrinterPaperStatus = new PrinterPaperStatus_Type();
                                            receiptPrinterStatus.SuppliesStatus.PrinterPaperStatus = (PrinterPaperStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(1, 1), out i))
                                        {
                                            receiptPrinterStatus.SuppliesStatus.RibbonStatus = new RibbonStatus_Type();
                                            receiptPrinterStatus.SuppliesStatus.RibbonStatus = (RibbonStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(2, 1), out i))
                                        {
                                            receiptPrinterStatus.SuppliesStatus.PrintHeadStatus = new PrintHeadStatus_Type();
                                            receiptPrinterStatus.SuppliesStatus.PrintHeadStatus = (PrintHeadStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(3, 1), out i))
                                        {
                                            receiptPrinterStatus.SuppliesStatus.KnifeStatus = new KnifeStatus_Type();
                                            receiptPrinterStatus.SuppliesStatus.KnifeStatus = (KnifeStatus_Type)i;
                                        }
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Supplies status G"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = receiptPrinterStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse CardReaderWriterStatus G"); }
                                break;
                            }
                        case "H"://Journal Printer Status
                            {
                                if (DataInPut.Length > 6)
                                {
                                    JournalPrinterStatus_Type journalPrinterStatus = new JournalPrinterStatus_Type();
                                    //A)- Transaction / Device status
                                    if (DataInPut[3].ToString().Length > 1)
                                    {
                                        if (int.TryParse(DataInPut[3].Substring(1, 1), out i))
                                            journalPrinterStatus.DeviceStatus = (JournalPrinterDeviceStatus_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse TX / Device status H"); }
                                    //B)- Error severity
                                    if (DataInPut[4].ToString().Length > 0)
                                    {
                                        if (int.TryParse(DataInPut[4], out i))
                                            journalPrinterStatus.DeviceErrorSeverity = (ErrorSeverity_Type)i;
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Error severity status H"); }
                                    //C)- Diagnostic status
                                    if (DataInPut[5].ToString().Length > 1)
                                    {
                                        journalPrinterStatus.DiagnosticStatus = new DiagnosticStatus_Type();
                                        journalPrinterStatus.DiagnosticStatus.MaintenanceData = DataInPut[5].Substring(0, 2);
                                        journalPrinterStatus.DiagnosticStatus.MaintenanceStatus = DataInPut[5].Substring(2);
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Diagnostic status H"); }
                                    //E)- Supplies status
                                    if (DataInPut[6].ToString().Length > 3)
                                    {
                                        journalPrinterStatus.SuppliesStatus = new PrinterSuppliesStatus_Type();
                                        if (int.TryParse(DataInPut[6].Substring(0, 1), out i))
                                        {
                                            journalPrinterStatus.SuppliesStatus.PrinterPaperStatus = new PrinterPaperStatus_Type();
                                            journalPrinterStatus.SuppliesStatus.PrinterPaperStatus = (PrinterPaperStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(1, 1), out i))
                                        {
                                            journalPrinterStatus.SuppliesStatus.RibbonStatus = new RibbonStatus_Type();
                                            journalPrinterStatus.SuppliesStatus.RibbonStatus = (RibbonStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(2, 1), out i))
                                        {
                                            journalPrinterStatus.SuppliesStatus.PrintHeadStatus = new PrintHeadStatus_Type();
                                            journalPrinterStatus.SuppliesStatus.PrintHeadStatus = (PrintHeadStatus_Type)i;
                                        }
                                        if (int.TryParse(DataInPut[6].Substring(3, 1), out i))
                                        {
                                            journalPrinterStatus.SuppliesStatus.KnifeStatus = new KnifeStatus_Type();
                                            journalPrinterStatus.SuppliesStatus.KnifeStatus = (KnifeStatus_Type)i;
                                        }
                                    }
                                    else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse Supplies status H"); }
                                    //Cargo los datos parseados.
                                    unsolicitedStatusMessage.DeviceFaultStatusInformation.Item = journalPrinterStatus;
                                }
                                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse CardReaderWriterStatus H"); }
                                break;
                            }
                    }
                    return unsolicitedStatusMessage;
                }
                else { throw new Exception("UnSolicitedStatusMessageParser() error: insufficient data to parse unsolicitedStatusMessage"); }
            }
            catch (Exception ex) { throw new Exception(string.Format("UnSolicitedStatusMessageParser(): {0}", ex.Message)); }
        }

        //8)- SOLICITED STATUS PARSE
        public SolicitedStatusMessage_Type SolicitedStatusMessageParser(string msg)
        {
            SolicitedStatusMessage_Type solicitedStatusMessage;
            string[] DataInPut;
            int i = 0, j = 0;
            try
            {
                DataInPut = msg.Split(Entities.Const.FS);
                if (DataInPut.Length > 2)
                {
                    solicitedStatusMessage = new SolicitedStatusMessage_Type();
                    solicitedStatusMessage.MessageSubClass = DataInPut[0].Substring(0, 2);
                    solicitedStatusMessage.LogicalUnitNumber = DataInPut[1];
                    solicitedStatusMessage.TimeVariantNumber = new TimeVariantNumber_Type();
                    solicitedStatusMessage.TimeVariantNumber.TimeVariant = DataInPut[2];
                    solicitedStatusMessage.Status = new Status_Type();
                    switch (DataInPut[3])
                    {
                        case "A"://CommandReject
                            {
                                CommandReject_Type commandReject = new CommandReject_Type();
                                commandReject.StatusDescriptor = "A";
                                solicitedStatusMessage.Status.Item = commandReject;
                                break;
                            }
                        case "8"://DeviceFault_Type
                            {
                                DeviceFault_Type deviceFault = new DeviceFault_Type();
                                DeviceFaultStatusInformation_Type deviceFaultStatusInformation = new DeviceFaultStatusInformation_Type();
                                deviceFault.StatusDescriptor = "8";
                                deviceFault.DeviceFaultStatusInformation = new DeviceFaultStatusInformation_Type();
                                switch (DataInPut[4].Substring(0, 1))
                                {
                                    case "E": //CashHandlerStatus
                                        {
                                            CashHandlerStatus_Type cashHandlerStatus = new CashHandlerStatus_Type();
                                            cashHandlerStatus.DeviceIdentificationGraphic = "E";
                                            cashHandlerStatus.DeviceStatus = new CashHandlerDeviceStatus_Type();
                                            cashHandlerStatus.DeviceStatus.NotesDispensed = new CashHandlerDeviceStatus_TypeNotesDispensed();
                                            if (DataInPut[4].Length > 9)
                                            {
                                                if (int.TryParse(DataInPut[4].Substring(1, 1), out i))
                                                    cashHandlerStatus.DeviceStatus.DeviceOrTransactionStatus = (CashHandlerTransactionDeviceStatus_Type)i;
                                                if (int.TryParse(DataInPut[4].Substring(2, 2), out i))
                                                    cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette1 = i;
                                                if (int.TryParse(DataInPut[4].Substring(4, 2), out i))
                                                    cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette2 = i;
                                                if (int.TryParse(DataInPut[4].Substring(6, 2), out i))
                                                    cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette3 = i;
                                                if (int.TryParse(DataInPut[4].Substring(8, 2), out i))
                                                    cashHandlerStatus.DeviceStatus.NotesDispensed.NotesDispensedFromCassette4 = i;
                                            }
                                            else
                                            {
                                                throw new Exception("Ready _ Device E: length less to 10.");
                                            }
                                            deviceFaultStatusInformation.Item = cashHandlerStatus;
                                            break;
                                        }
                                }
                                deviceFault.DeviceFaultStatusInformation.Item = deviceFaultStatusInformation;
                                solicitedStatusMessage.Status.Item = deviceFault;
                                break;
                            }
                        case "9"://ReadyInstructionCompletedSuccessfully
                            {
                                ReadyInstructionCompletedSuccessfully_Type readyInstructionCompletedSuccessfully = new ReadyInstructionCompletedSuccessfully_Type();
                                readyInstructionCompletedSuccessfully.StatusDescriptor = "9";
                                solicitedStatusMessage.Status.Item = readyInstructionCompletedSuccessfully;
                                break;
                            }
                        case "B"://ReadyTransactionReplySuccessful 22000001003B001010100100322
                            {
                                ReadyTransactionReplySuccessful_Type readyTransactionReplySuccessful = new ReadyTransactionReplySuccessful_Type();
                                if (DataInPut.Length > 4)
                                {
                                    readyTransactionReplySuccessful.ReadyBStatusInformation = new ReadyBStatusInformation_Type();
                                    readyTransactionReplySuccessful.ReadyBStatusInformation.LastTransactionSerialNumber = DataInPut[4];
                                    readyTransactionReplySuccessful.ReadyBStatusInformation.AdditionalTransactionData = new AdditionalTransactionData_Type();
                                    CashDepositRecycleData_Type cashDepositRecycleData = new CashDepositRecycleData_Type();
                                    CashDispenseRecycleData_Type cashDispenseRecycleData = new CashDispenseRecycleData_Type();
                                    //TODO: hacer ajuste para mas de una gaveta recicladora      
                                    string[] bufferData = DataInPut[5].ToString().Split(Entities.Const.GS);
                                    if (bufferData.Length > 0)
                                    {
                                        if (DataInPut[5].Substring(0, 1).Equals("1", StringComparison.Ordinal)) //1)- Recycle Cassette Deposit Data
                                        {
                                            cashDepositRecycleData.DepositCassetteData = new DepositCassetteData_Type[bufferData.Length - 1];
                                            for (int l = 0; l < bufferData.Length - 1; l++)
                                            {
                                                if (bufferData[l].Length > 10)
                                                {
                                                    cashDepositRecycleData.DepositCassetteData[l] = new DepositCassetteData_Type();
                                                    if (int.TryParse(bufferData[l].Substring(3, 3), out i))
                                                        cashDepositRecycleData.DepositCassetteData[l].NDCCassetteType = i;
                                                    if (int.TryParse(bufferData[l].Substring(6, 3), out i))
                                                        cashDepositRecycleData.DepositCassetteData[l].NumberOfNotesStoredOrRetained = i;
                                                    if (int.TryParse(bufferData[l].Substring(bufferData[l].Length - 1, 1), out i))
                                                        cashDepositRecycleData.DepositCassetteData[l].CurrentSuppliesStatusOfCassette = (SuppliesStatusField_Type)i;
                                                    if (int.TryParse(bufferData[l].Substring(bufferData[l].Length - 2, 1), out i))
                                                        cashDepositRecycleData.DepositCassetteData[l].CurrentFitnessOfCassette = (ErrorSeverity_Type)i;
                                                }
                                            }
                                            readyTransactionReplySuccessful.ReadyBStatusInformation.AdditionalTransactionData.Item = cashDepositRecycleData;
                                        }
                                        else if (DataInPut[5].Substring(0, 1).Equals("2", StringComparison.Ordinal)) //B)- Recycle Cassette Dispense Data
                                        {
                                            cashDispenseRecycleData.DispenserCassetteData = new DispenserCassetteData_Type[bufferData.Length - 1];
                                            for (int l = 0; l < bufferData.Length - 1; l++)
                                            {
                                                if (bufferData[l].Length > 10)
                                                {
                                                    cashDispenseRecycleData.DispenserCassetteData[l] = new DispenserCassetteData_Type();
                                                    if (int.TryParse(bufferData[l].Substring(3, 3), out i))
                                                        cashDispenseRecycleData.DispenserCassetteData[l].NDCCassetteType = i;
                                                    if (int.TryParse(bufferData[l].Substring(6, 3), out i))
                                                        cashDispenseRecycleData.DispenserCassetteData[l].NumberOfNotesDispensed = i;
                                                    if (int.TryParse(bufferData[l].Substring(bufferData[l].Length - 1, 1), out i))
                                                        cashDispenseRecycleData.DispenserCassetteData[l].CurrentSuppliesStatusOfCassette = (BNAState_Type)i;
                                                    if (int.TryParse(bufferData[l].Substring(bufferData[l].Length - 2, 1), out i))
                                                        cashDispenseRecycleData.DispenserCassetteData[l].CurrentFitnessOfCassette = (ErrorSeverity_Type)i;
                                                }
                                            }
                                            readyTransactionReplySuccessful.ReadyBStatusInformation.AdditionalTransactionData.Item = cashDispenseRecycleData;
                                        }
                                    }
                                }
                                solicitedStatusMessage.Status.Item = readyTransactionReplySuccessful;
                                break;
                            }
                        case "C"://SpecificCommandReject
                            {
                                SpecificCommandRejectStatus_Type specificCommandRejectStatus = new SpecificCommandRejectStatus_Type();
                                specificCommandRejectStatus.StatusDescriptor = "C";
                                solicitedStatusMessage.Status.Item = specificCommandRejectStatus;
                                break;
                            }
                        case "F"://TerminalStateStatus
                            {
                                TerminalStateStatus_Type terminalStateStatus = new TerminalStateStatus_Type();
                                terminalStateStatus.StatusDescriptor = "F";
                                switch (DataInPut[4].Substring(0, 1))
                                {
                                    case "1": //ConfigurationInformation_Type (?)
                                        {
                                            ConfigurationInformation_Type configurationInformation = new ConfigurationInformation_Type();
                                            object item = new object();
                                            BasicHardwareConfiguration_Type item1 = new BasicHardwareConfiguration_Type();
                                            EnhancedOrBasicSuppliesStatus_Type suppliesStatus = new EnhancedOrBasicSuppliesStatus_Type();
                                            SensorStatus_Type sensorStatus = new SensorStatus_Type();
                                            SofwareIDAndReleaseNumberDataForConfiguration_Type sofwareIDAndReleaseNumberData = new SofwareIDAndReleaseNumberDataForConfiguration_Type();
                                            configurationInformation.MessageIdentifier = "1";
                                            configurationInformation.ConfigurationID = null;
                                            configurationInformation.Item = item;
                                            configurationInformation.Item1 = item1;
                                            configurationInformation.SuppliesStatus = suppliesStatus;
                                            configurationInformation.SensorStatus = sensorStatus;
                                            configurationInformation.SofwareIDAndReleaseNumberData = sofwareIDAndReleaseNumberData;
                                            terminalStateStatus.Item = configurationInformation;
                                            break;
                                        }
                                    case "2": //SupplyCounters_Type (1...4)
                                        {
                                            SupplyCounters_Type supplyCounters = new SupplyCounters_Type();
                                            terminalStateStatus.Item = supplyCounters;
                                            break;
                                        }
                                    case "5": //SendDateTime_Type (3...1C)
                                        {
                                            SendDateTime_Type sendDateTime = new SendDateTime_Type();
                                            terminalStateStatus.Item = sendDateTime;
                                            break;
                                        }
                                    case "6": //SendConfigurationID_Type (1...3)
                                        {
                                            SendConfigurationID_Type sendConfigurationID = new SendConfigurationID_Type();
                                            sendConfigurationID.MessageIdentifier = DataInPut[4].Substring(0, 1);
                                            sendConfigurationID.ConfigurationID = DataInPut[4].Substring(1);
                                            terminalStateStatus.Item = sendConfigurationID;
                                            break;
                                        }
                                    case "H":
                                        {
                                            #region "HardwareConfigurationData_Type (1...71)"
                                            HardwareConfigurationData_Type hardwareConfigurationData = new HardwareConfigurationData_Type();
                                            hardwareConfigurationData.MessageIdentifier = DataInPut[4].Substring(1, 1);
                                            hardwareConfigurationData.ConfigurationID = DataInPut[4].Substring(2);
                                            ProductClass_Type productClass = new ProductClass_Type();
                                            if (int.TryParse(DataInPut[5].Substring(1), out i))
                                                productClass = (ProductClass_Type)i;
                                            hardwareConfigurationData.ProductClass = productClass;
                                            string[] devices = DataInPut[6].Split((char)0x1D);
                                            hardwareConfigurationData.Devices = new Device_Type[devices.Length];
                                            j = 0;
                                            foreach (string dev in devices)
                                            {
                                                hardwareConfigurationData.Devices[j] = new Device_Type();
                                                hardwareConfigurationData.Devices[j].Item = new object();
                                                switch (dev.Substring(0, 1))
                                                {
                                                    case "C":
                                                        {
                                                            SystemDisk_Type systemDisk = new SystemDisk_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                systemDisk = (SystemDisk_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = systemDisk;
                                                            device.ItemElementName = ItemChoiceDevice_Type.SystemDisk;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "D":
                                                        {
                                                            MagneticCardReaderWriter_Type magneticCardReaderWriter = new MagneticCardReaderWriter_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                magneticCardReaderWriter = (MagneticCardReaderWriter_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = magneticCardReaderWriter;
                                                            device.ItemElementName = ItemChoiceDevice_Type.MagneticCardReaderWriter;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "E":
                                                        {
                                                            CashHandlerDevice_Type cashHandlerDevice = new CashHandlerDevice_Type();
                                                            cashHandlerDevice.CashHandler = new CashHandler_Type();
                                                            cashHandlerDevice.MaximumItemsDispense = 0;
                                                            cashHandlerDevice.MaximumItemsDispenseSpecified = false;
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                            {
                                                                cashHandlerDevice.CashHandler = (CashHandler_Type)i;
                                                            }
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = cashHandlerDevice;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CashHandlerDevice;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "F":
                                                        {
                                                            EnvelopeDepository_Type envelopeDepository = new EnvelopeDepository_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                envelopeDepository = (EnvelopeDepository_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = envelopeDepository;
                                                            device.ItemElementName = ItemChoiceDevice_Type.EnvelopeDepository;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "G":
                                                        {
                                                            ReceiptPrinterConfiguration_Type receiptPrinterConfiguration = new ReceiptPrinterConfiguration_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                receiptPrinterConfiguration = (ReceiptPrinterConfiguration_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = receiptPrinterConfiguration;
                                                            device.ItemElementName = ItemChoiceDevice_Type.ReceiptPrinter;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "H":
                                                        {
                                                            JournalPrinter_Type journalPrinter = new JournalPrinter_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                journalPrinter = (JournalPrinter_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = journalPrinter;
                                                            device.ItemElementName = ItemChoiceDevice_Type.JournalPrinter;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "K":
                                                        {
                                                            NightSafeDepository_Type nightSafeDepository = new NightSafeDepository_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                nightSafeDepository = (NightSafeDepository_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = nightSafeDepository;
                                                            device.ItemElementName = ItemChoiceDevice_Type.NightSafeDepository;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "L":
                                                        {
                                                            Encryptor_Type encryptor = new Encryptor_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                encryptor = (Encryptor_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = encryptor;
                                                            device.ItemElementName = ItemChoiceDevice_Type.Encryptor;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "M":
                                                        {
                                                            SecurityCamera_Type securityCamera = new SecurityCamera_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                securityCamera = (SecurityCamera_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = securityCamera;
                                                            device.ItemElementName = ItemChoiceDevice_Type.SecurityCamera;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "N":
                                                        {
                                                            HardwareConfigurationField_Type doorAccessSytem = new HardwareConfigurationField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                doorAccessSytem = (HardwareConfigurationField_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = doorAccessSytem;
                                                            device.ItemElementName = ItemChoiceDevice_Type.DoorAccessSytem;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "O":
                                                        {
                                                            FlexDisk_Type flexDisk = new FlexDisk_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                flexDisk = (FlexDisk_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = flexDisk;
                                                            device.ItemElementName = ItemChoiceDevice_Type.FlexDisk;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "P":
                                                        {
                                                            TamperIndicatingBins_Type tamperIndicatingBins = new TamperIndicatingBins_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                tamperIndicatingBins = (TamperIndicatingBins_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = tamperIndicatingBins;
                                                            device.ItemElementName = ItemChoiceDevice_Type.TamperIndicatingBins;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "Q":
                                                        {
                                                            CardholderKeyboard_Type cardholderKeyboard = new CardholderKeyboard_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                cardholderKeyboard = (CardholderKeyboard_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = cardholderKeyboard;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CardholderKeyboard;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "R":
                                                        {
                                                            OperatorKeyboard_Type operatorKeyboard = new OperatorKeyboard_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                operatorKeyboard = (OperatorKeyboard_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = operatorKeyboard;
                                                            device.ItemElementName = ItemChoiceDevice_Type.OperatorKeyboard;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "S":
                                                        {
                                                            CardholderDisplayVoice_Type cardholderDisplayVoice = new CardholderDisplayVoice_Type();
                                                            //TODO: completar
                                                            cardholderDisplayVoice.TouchScreen = false;
                                                            cardholderDisplayVoice.VGMTranslator = false;
                                                            cardholderDisplayVoice.VoiceSupported = false;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = cardholderDisplayVoice;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CardholderDisplayVoice;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "V":
                                                        {
                                                            StatementPrinter_Type statementPrinter = new StatementPrinter_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                statementPrinter = (StatementPrinter_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = statementPrinter;
                                                            device.ItemElementName = ItemChoiceDevice_Type.StatementPrinter;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "Y":
                                                        {
                                                            CoinDispenser_Type coinDispenser = new CoinDispenser_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                coinDispenser = (CoinDispenser_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = coinDispenser;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CoinDispenser;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "Z":
                                                        {
                                                            SystemDisplay_Type systemDisplay = new SystemDisplay_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                systemDisplay = (SystemDisplay_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = systemDisplay;
                                                            device.ItemElementName = ItemChoiceDevice_Type.SystemDisplay;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "[":
                                                        {
                                                            HardwareConfigurationField_Type mediaEntryExitIndicators = new HardwareConfigurationField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                mediaEntryExitIndicators = (HardwareConfigurationField_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = mediaEntryExitIndicators;
                                                            device.ItemElementName = ItemChoiceDevice_Type.MediaEntryExitIndicators;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case @"\":
                                                        {
                                                            EnvelopeDispenser_Type envelopeDispenser = new EnvelopeDispenser_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                envelopeDispenser = (EnvelopeDispenser_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = envelopeDispenser;
                                                            device.ItemElementName = ItemChoiceDevice_Type.EnvelopeDispenser;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "a":
                                                        {
                                                            VoiceGuidanceConfig_Type voiceGuidanceConfig = new VoiceGuidanceConfig_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                voiceGuidanceConfig = (VoiceGuidanceConfig_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = voiceGuidanceConfig;
                                                            device.ItemElementName = ItemChoiceDevice_Type.VoiceGuidance;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "d":
                                                        {
                                                            CashHandlerDevice_Type cashHandlerDevice = new CashHandlerDevice_Type();
                                                            cashHandlerDevice.CashHandler = new CashHandler_Type();
                                                            cashHandlerDevice.MaximumItemsDispense = 0;
                                                            cashHandlerDevice.MaximumItemsDispenseSpecified = false;
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                            {
                                                                cashHandlerDevice.CashHandler = (CashHandler_Type)i;
                                                            }
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = cashHandlerDevice;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CashHandlerDevice0;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "e":
                                                        {
                                                            CashHandlerDevice_Type cashHandlerDevice = new CashHandlerDevice_Type();
                                                            cashHandlerDevice.CashHandler = new CashHandler_Type();
                                                            cashHandlerDevice.MaximumItemsDispense = 0;
                                                            cashHandlerDevice.MaximumItemsDispenseSpecified = false;
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                            {
                                                                cashHandlerDevice.CashHandler = (CashHandler_Type)i;
                                                            }
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = cashHandlerDevice;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CashHandlerDevice1;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "f":
                                                        {
                                                            BarcodeConfigData_Type barcodeConfigData = new BarcodeConfigData_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                barcodeConfigData = (BarcodeConfigData_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = barcodeConfigData;
                                                            device.ItemElementName = ItemChoiceDevice_Type.BarcodeReader;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "q":
                                                        {
                                                            //TODO
                                                            CheckProcessingModuleConfigData_Type checkProcessingModuleConfigData = new CheckProcessingModuleConfigData_Type();
                                                            checkProcessingModuleConfigData.MICRReader = false;
                                                            checkProcessingModuleConfigData.RearPrinter = false;
                                                            checkProcessingModuleConfigData.ChequeStamper = false;
                                                            checkProcessingModuleConfigData.FrontScanner = false;
                                                            checkProcessingModuleConfigData.RearScanner = false;
                                                            checkProcessingModuleConfigData.BunchChequeAcceptor = false;
                                                            checkProcessingModuleConfigData.BunchChequeAcceptorThatAcceptsCashSeparately = false;
                                                            checkProcessingModuleConfigData.BunchChequeAcceptorThatAcceptsCashTogether = false;
                                                            checkProcessingModuleConfigData.OCRCodelineReader = false;
                                                            checkProcessingModuleConfigData.MICRSupportsE13B = false;
                                                            checkProcessingModuleConfigData.MICRSupportsCMC7 = false;
                                                            checkProcessingModuleConfigData.NumberOfBins = new BNABinNumber_Type();
                                                            //if (int.TryParse(dev.Substring(1), out i))
                                                            //    checkProcessingModuleConfigData = (CheckProcessingModuleConfigData_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = checkProcessingModuleConfigData;
                                                            device.ItemElementName = ItemChoiceDevice_Type.CheckProcessingModule;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                    case "w":
                                                        {
                                                            BunchNoteAcceptorConfigData_Type bunchNoteAcceptorConfigData = new BunchNoteAcceptorConfigData_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                bunchNoteAcceptorConfigData = (BunchNoteAcceptorConfigData_Type)i;
                                                            Device_Type device = new Device_Type();
                                                            device.Item = new object();
                                                            device.Item = bunchNoteAcceptorConfigData;
                                                            device.ItemElementName = ItemChoiceDevice_Type.BunchNoteAcceptor;
                                                            hardwareConfigurationData.Devices[j] = device;
                                                            break;
                                                        }
                                                }
                                                j++;
                                            }
                                            //IssuerScriptResultsAndCompletionData_Type issuerScriptResultsAndCompletionDataField;
                                            terminalStateStatus.Item = hardwareConfigurationData;
                                            break;
                                            #endregion
                                        }
                                    case "I": //SuppliesData_Type (1...72)
                                        {
                                            #region "SuppliesData_Type (1...72)"
                                            SuppliesData_Type suppliesData = new SuppliesData_Type();
                                            suppliesData.MessageIdentifier = DataInPut[4].Substring(0, 1);
                                            string[] devices = DataInPut[4].Split((char)0x1D);
                                            devices[0] = devices[0].Substring(2);
                                            suppliesData.SuppliesStatus = new DeviceSuppliesStatus_Type[devices.Length];
                                            j = 0;
                                            foreach (string dev in devices)
                                            {
                                                suppliesData.SuppliesStatus[j] = new DeviceSuppliesStatus_Type();
                                                suppliesData.SuppliesStatus[j].Item = new object();
                                                switch (dev.Substring(0, 1))
                                                {
                                                    case "D":
                                                        {
                                                            SuppliesStatusField_Type cardCaptureBinSuppliesStatus = new SuppliesStatusField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                cardCaptureBinSuppliesStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cardCaptureBinSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CardCaptureBinSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "E":
                                                        {
                                                            CashHandlerSupplies_Type cashHandlerSupplies = new CashHandlerSupplies_Type();
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandlerSupplies.RejectBinState = (SuppliesStatusField_Type)i;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandlerSupplies.CassetteType1State = (SuppliesStatusField_Type)i;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandlerSupplies.CassetteType2State = (SuppliesStatusField_Type)i;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandlerSupplies.CassetteType3State = (SuppliesStatusField_Type)i;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandlerSupplies.CassetteType4State = (SuppliesStatusField_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...72 _ Device E: length less to 6.");
                                                            }
                                                            //if (int.TryParse(dev.Substring(2, 1), out i))
                                                            //    cashHandlerSupplies.ExtendedCassetteTypeState = (ExtendedCassetteTypeSupplies_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cashHandlerSupplies;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CashHandlerSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "F":
                                                        {
                                                            SuppliesStatusField_Type envelopeDispenserSuppliesStatus = new SuppliesStatusField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                envelopeDispenserSuppliesStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = envelopeDispenserSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.EnvelopeDispenserSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "G":
                                                        {
                                                            PrinterSuppliesData_Type receiptPrinterSuppliesStatus = new PrinterSuppliesData_Type();
                                                            if (int.TryParse(dev.Substring(1, 1), out i))
                                                                receiptPrinterSuppliesStatus.PrinterPaperStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(2, 1), out i))
                                                                receiptPrinterSuppliesStatus.RibbonStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(3, 1), out i))
                                                                receiptPrinterSuppliesStatus.PrintHeadStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(4, 1), out i))
                                                                receiptPrinterSuppliesStatus.KnifeStatus = (SuppliesStatusField_Type)i;
                                                            //TODO: cargar estos datos
                                                            receiptPrinterSuppliesStatus.KnifeStatusSpecified = true;
                                                            receiptPrinterSuppliesStatus.CaptureBinStatus = new SuppliesStatusField_Type();
                                                            receiptPrinterSuppliesStatus.CaptureBinStatusSpecified = false;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = receiptPrinterSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.ReceiptPrinterSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "H":
                                                        {
                                                            PrinterSuppliesData_Type journalPrinterSuppliesStatus = new PrinterSuppliesData_Type();
                                                            if (int.TryParse(dev.Substring(1, 1), out i))
                                                                journalPrinterSuppliesStatus.PrinterPaperStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(2, 1), out i))
                                                                journalPrinterSuppliesStatus.RibbonStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(3, 1), out i))
                                                                journalPrinterSuppliesStatus.PrintHeadStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = journalPrinterSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.JournalPrinterSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "L":
                                                        {
                                                            SuppliesStatusField_Type encryptorSuppliesStatus = new SuppliesStatusField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                encryptorSuppliesStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = encryptorSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.EncryptorSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "M":
                                                        {
                                                            SuppliesStatusField_Type cameraSuppliesStatus = new SuppliesStatusField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                cameraSuppliesStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cameraSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CameraSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "V":
                                                        {
                                                            PrinterSuppliesData_Type statementPrinterSuppliesStatus = new PrinterSuppliesData_Type();
                                                            if (int.TryParse(dev.Substring(1, 1), out i))
                                                                statementPrinterSuppliesStatus.PrinterPaperStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(2, 1), out i))
                                                                statementPrinterSuppliesStatus.RibbonStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(3, 1), out i))
                                                                statementPrinterSuppliesStatus.PrintHeadStatus = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(4, 1), out i))
                                                                statementPrinterSuppliesStatus.KnifeStatus = (SuppliesStatusField_Type)i;
                                                            //TODO: cargar estos datos
                                                            statementPrinterSuppliesStatus.KnifeStatusSpecified = true;
                                                            statementPrinterSuppliesStatus.CaptureBinStatus = new SuppliesStatusField_Type();
                                                            statementPrinterSuppliesStatus.CaptureBinStatusSpecified = false;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatuS = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatuS.Item = new object();
                                                            deviceSuppliesStatuS.Item = statementPrinterSuppliesStatus;
                                                            deviceSuppliesStatuS.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.StatementPrinterSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatuS;
                                                            break;
                                                        }
                                                    case "Y":
                                                        {
                                                            CoinDispenserSuppliesData_Type coinDispenserSuppliesData = new CoinDispenserSuppliesData_Type();
                                                            if (int.TryParse(dev.Substring(1, 1), out i))
                                                                coinDispenserSuppliesData.HopperType1State = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(2, 1), out i))
                                                                coinDispenserSuppliesData.HopperType2State = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(3, 1), out i))
                                                                coinDispenserSuppliesData.HopperType3State = (SuppliesStatusField_Type)i;
                                                            if (int.TryParse(dev.Substring(4, 1), out i))
                                                                coinDispenserSuppliesData.HopperType4State = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = coinDispenserSuppliesData;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CoinDispenserSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case @"\":
                                                        {
                                                            SuppliesStatusField_Type envelopeDispenserSuppliesStatus = new SuppliesStatusField_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                envelopeDispenserSuppliesStatus = (SuppliesStatusField_Type)i;
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = envelopeDispenserSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.EnvelopeDispenserSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "d":
                                                        {
                                                            CashHandlerSuppliesData_Type cashHandler0SuppliesStatus = new CashHandlerSuppliesData_Type();
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandler0SuppliesStatus.RejectBin = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType1 = 0;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType1SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType2 = 0;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType2SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType3 = 0;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType3SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType4 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType4SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType5 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType5SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType6 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType6SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler0SuppliesStatus.CassetteType7 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler0SuppliesStatus.CassetteType7SuppliesStatus = (SuppliesStatusField_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...72 _ Device d: length less to 6.");
                                                            }
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cashHandler0SuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CashHandler0SuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "e":
                                                        {
                                                            CashHandlerSuppliesData_Type cashHandler1SuppliesStatus = new CashHandlerSuppliesData_Type();
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandler1SuppliesStatus.RejectBin = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType1 = 0;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType1SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType2 = 0;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType2SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType3 = 0;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType3SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType4 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType4SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType5 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType5SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType6 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType6SuppliesStatus = (SuppliesStatusField_Type)i;
                                                                cashHandler1SuppliesStatus.CassetteType7 = 0;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler1SuppliesStatus.CassetteType7SuppliesStatus = (SuppliesStatusField_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...72 _ Device e: length less to 6.");
                                                            }
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cashHandler1SuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.CashHandler1SuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "q":
                                                        {
                                                            CPMSuppliesStatus_Type cPMSuppliesStatus = new CPMSuppliesStatus_Type();
                                                            cPMSuppliesStatus.EndorsePrinter = new CPMSuppliesStatusField_Type();
                                                            cPMSuppliesStatus.Stamper = new CPMSuppliesStatusField_Type();
                                                            //cPMSuppliesStatus.BinSuppliesStatusList = new CPMSuppliesStatusField_Type(4);
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = cPMSuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.ChequeProcessingModuleSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                    case "w":
                                                        {
                                                            CassetteSupplyStatus_Type cassetteSupplyStatus = new CassetteSupplyStatus_Type();
                                                            BNASuppliesStatus_Type bNASuppliesStatus = new BNASuppliesStatus_Type();
                                                            bNASuppliesStatus.CassetteSupplyStatus = new CassetteSupplyStatus_Type[4];
                                                            if (dev.Length > 4)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                {
                                                                    bNASuppliesStatus.BNAState = (BNAState_Type)i;
                                                                }
                                                                if (int.TryParse(dev.Substring(2, 3), out i))
                                                                {
                                                                    cassetteSupplyStatus.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    {
                                                                        cassetteSupplyStatus.CassetteSypplyState = (BNAState_Type)i;
                                                                        bNASuppliesStatus.CassetteSupplyStatus[0] = cassetteSupplyStatus;
                                                                    }
                                                                }
                                                            }
                                                            if (dev.Length > 8)
                                                            {
                                                                if (int.TryParse(dev.Substring(6, 3), out i))
                                                                {
                                                                    cassetteSupplyStatus.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(9, 1), out i))
                                                                    {
                                                                        cassetteSupplyStatus.CassetteSypplyState = (BNAState_Type)i;
                                                                        bNASuppliesStatus.CassetteSupplyStatus[1] = cassetteSupplyStatus;
                                                                    }
                                                                }

                                                            }
                                                            if (dev.Length > 13)
                                                            {
                                                                if (int.TryParse(dev.Substring(10, 3), out i))
                                                                {
                                                                    cassetteSupplyStatus.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(13, 1), out i))
                                                                    {
                                                                        cassetteSupplyStatus.CassetteSypplyState = (BNAState_Type)i;
                                                                        bNASuppliesStatus.CassetteSupplyStatus[2] = cassetteSupplyStatus;
                                                                    }
                                                                }
                                                            }
                                                            if (dev.Length > 17)
                                                            {
                                                                if (int.TryParse(dev.Substring(14, 3), out i))
                                                                {
                                                                    cassetteSupplyStatus.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(17, 1), out i))
                                                                    {
                                                                        cassetteSupplyStatus.CassetteSypplyState = (BNAState_Type)i;
                                                                        bNASuppliesStatus.CassetteSupplyStatus[2] = cassetteSupplyStatus;
                                                                    }
                                                                }
                                                            }
                                                            DeviceSuppliesStatus_Type deviceSuppliesStatus = new DeviceSuppliesStatus_Type();
                                                            deviceSuppliesStatus.Item = new object();
                                                            deviceSuppliesStatus.Item = bNASuppliesStatus;
                                                            deviceSuppliesStatus.ItemElementName = ItemChoiceDeviceSuppliesStatus_Type.BunchNoteAcceptorSuppliesStatus;
                                                            suppliesData.SuppliesStatus[j] = deviceSuppliesStatus;
                                                            break;
                                                        }
                                                }
                                                j++;
                                            }
                                            terminalStateStatus.Item = suppliesData;
                                            break;
                                            #endregion
                                        }
                                    case "J":
                                        {
                                            #region "FitnessData_Type (1...73)"
                                            FitnessData_Type fitnessData = new FitnessData_Type();
                                            fitnessData.MessageIdentifier = DataInPut[4].Substring(0, 1);
                                            string[] devices = DataInPut[4].Split((char)0x1D);
                                            devices[0] = devices[0].Substring(2);
                                            fitnessData.HardwareFitnessData = new HardwareFitnessData_Type();
                                            fitnessData.HardwareFitnessData.Device = new DeviceFitnessData_Type[devices.Length];
                                            j = 0;
                                            foreach (string dev in devices)
                                            {
                                                fitnessData.HardwareFitnessData.Device[j] = new DeviceFitnessData_Type();
                                                fitnessData.HardwareFitnessData.Device[j].Item = new object();
                                                switch (dev.Substring(0, 1))
                                                {
                                                    case "A":
                                                        {
                                                            ErrorSeverity_Type timeofDayClockError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                timeofDayClockError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = timeofDayClockError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.TimeofDayClockError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "B":
                                                        {
                                                            ErrorSeverity_Type highOrderCommunicationsError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                highOrderCommunicationsError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = highOrderCommunicationsError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.HighOrderCommunicationsError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "C":
                                                        {
                                                            ErrorSeverity_Type systemDiskError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                systemDiskError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = systemDiskError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.SystemDiskError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "D":
                                                        {
                                                            ErrorSeverity_Type magneticCardReaderWriterError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                magneticCardReaderWriterError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = magneticCardReaderWriterError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.MagneticCardReaderWriterError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "E":
                                                        {
                                                            CashHandlerErrorSeverity_Type cashHandlerError = new CashHandlerErrorSeverity_Type();
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandlerError.CompleteDevice = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandlerError.CassetteType1ErrorSeverity = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandlerError.CassetteType2ErrorSeverity = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandlerError.CassetteType3ErrorSeverity = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandlerError.CassetteType4ErrorSeverity = (ErrorSeverity_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...73 _ Device E: length less to 6.");
                                                            }
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = cashHandlerError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CashHandlerError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "F":
                                                        {
                                                            ErrorSeverity_Type envelopeDepositoryError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                envelopeDepositoryError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = envelopeDepositoryError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.EnvelopeDepositoryError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "G":
                                                        {
                                                            ErrorSeverity_Type receiptPrinterError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                receiptPrinterError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = receiptPrinterError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.ReceiptPrinterError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "H":
                                                        {
                                                            ErrorSeverity_Type journalPrinterError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                journalPrinterError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = journalPrinterError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.JournalPrinterError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "K":
                                                        {
                                                            ErrorSeverity_Type nightSafeDepositoryError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                nightSafeDepositoryError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = nightSafeDepositoryError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.NightSafeDepositoryError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "L":
                                                        {
                                                            ErrorSeverity_Type encryptorError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(1), out i))
                                                                encryptorError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = encryptorError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.EncryptorError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "M":
                                                        {
                                                            ErrorSeverity_Type securityCameraError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                securityCameraError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = securityCameraError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.SecurityCameraError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "O":
                                                        {
                                                            ErrorSeverity_Type flexDiskError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                flexDiskError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = flexDiskError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.FlexDiskError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "P":
                                                        {
                                                            ErrorSeverity_Type tamperIndicatingBinsError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                tamperIndicatingBinsError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = tamperIndicatingBinsError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.TamperIndicatingBinsError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "Q":
                                                        {
                                                            ErrorSeverity_Type cardholderKeyboardError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                cardholderKeyboardError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = cardholderKeyboardError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CardholderKeyboardError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "R":
                                                        {
                                                            ErrorSeverity_Type operatorKeyboardError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                operatorKeyboardError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = operatorKeyboardError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.OperatorKeyboardError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "S":
                                                        {
                                                            ErrorSeverity_Type cardholderDisplayVoiceError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                cardholderDisplayVoiceError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = cardholderDisplayVoiceError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CardholderDisplayVoiceError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "V":
                                                        {
                                                            ErrorSeverity_Type statementPrinterError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                statementPrinterError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = statementPrinterError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.StatementPrinterError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "Y":
                                                        {
                                                            ErrorSeverity_Type coinDispenserError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                coinDispenserError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = coinDispenserError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CoinDispenserError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "Z":
                                                        {
                                                            ErrorSeverity_Type systemDisplayError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                systemDisplayError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = systemDisplayError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.SystemDisplayError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "[":
                                                        {
                                                            ErrorSeverity_Type mediaEntryExitIndicatorsError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                mediaEntryExitIndicatorsError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = mediaEntryExitIndicatorsError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.MediaEntryExitIndicatorsError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case @"\":
                                                        {
                                                            ErrorSeverity_Type envelopeDispenserError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                envelopeDispenserError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = envelopeDispenserError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.EnvelopeDispenserError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "d":
                                                        {
                                                            CashHandlerError_Type cashHandler0Error = new CashHandlerError_Type();
                                                            cashHandler0Error.CompleteDevice = 0;
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandler0Error.CompleteDevice = (ErrorSeverity_Type)i;
                                                                cashHandler0Error.CassetteType1 = 1;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandler0Error.CassetteType1FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler0Error.CassetteType2 = 2;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandler0Error.CassetteType2FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler0Error.CassetteType3 = 3;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandler0Error.CassetteType3FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler0Error.CassetteType4 = 4;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler0Error.CassetteType4FitnessStatus = (ErrorSeverity_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...73 _ Device d: length less to 6.");
                                                            }
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = cashHandler0Error;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CashHandler0Error;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "e":
                                                        {
                                                            CashHandlerError_Type cashHandler1Error = new CashHandlerError_Type();
                                                            cashHandler1Error.CompleteDevice = 0;
                                                            if (dev.Length > 5)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    cashHandler1Error.CompleteDevice = (ErrorSeverity_Type)i;
                                                                cashHandler1Error.CassetteType1 = 1;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    cashHandler1Error.CassetteType1FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler1Error.CassetteType2 = 2;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    cashHandler1Error.CassetteType2FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler1Error.CassetteType3 = 3;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    cashHandler1Error.CassetteType3FitnessStatus = (ErrorSeverity_Type)i;
                                                                cashHandler1Error.CassetteType4 = 4;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    cashHandler1Error.CassetteType4FitnessStatus = (ErrorSeverity_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...73 _ Device e: length less to 6.");
                                                            }
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = cashHandler1Error;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CashHandler1Error;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "f":
                                                        {
                                                            ErrorSeverity_Type barcodeReaderError = new ErrorSeverity_Type();
                                                            if (int.TryParse(dev.Substring(2), out i))
                                                                barcodeReaderError = (ErrorSeverity_Type)i;
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = barcodeReaderError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.BarcodeReaderError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "q":
                                                        {
                                                            CheckProcessingModuleFitnessData_Type checkProcessingModuleError = new CheckProcessingModuleFitnessData_Type();
                                                            checkProcessingModuleError.ExtendedMessageCPMFitnessData = new ExtendedMessageCPMFitnessData_Type();
                                                            if (dev.Length > 7)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                    checkProcessingModuleError.FitnessCPM = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(2, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.EndorsePrinter = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(3, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.Stamper = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(4, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.EscrowRebuncher = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.Reserved = string.Empty;
                                                                checkProcessingModuleError.ExtendedMessageCPMFitnessData.BinCPMFitnessDataList = new ErrorSeverity_Type[2];
                                                                if (int.TryParse(dev.Substring(6, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.BinCPMFitnessDataList[0] = (ErrorSeverity_Type)i;
                                                                if (int.TryParse(dev.Substring(7, 1), out i))
                                                                    checkProcessingModuleError.ExtendedMessageCPMFitnessData.BinCPMFitnessDataList[1] = (ErrorSeverity_Type)i;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("1...73 _ Device q: length less to 8.");
                                                            }
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = checkProcessingModuleError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.CheckProcessingModuleError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                    case "w":
                                                        {
                                                            BunchNoteAcceptorFitnessData_Type bunchNoteAcceptorError = new BunchNoteAcceptorFitnessData_Type();
                                                            bunchNoteAcceptorError.CassetteFitnessData = new CassetteFitnessData_Type[4];
                                                            if (dev.Length > 4)
                                                            {
                                                                if (int.TryParse(dev.Substring(1, 1), out i))
                                                                {
                                                                    bunchNoteAcceptorError.CashAcceptorFitness = (ErrorSeverity_Type)i;
                                                                }
                                                                //Fitness of Cassette Type 2
                                                                if (int.TryParse(dev.Substring(2, 3), out i))
                                                                {
                                                                    CassetteFitnessData_Type cassetteFitnessData = new CassetteFitnessData_Type();
                                                                    cassetteFitnessData.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(5, 1), out i))
                                                                    {
                                                                        cassetteFitnessData.CassetteFitness = (ErrorSeverity_Type)i;
                                                                        bunchNoteAcceptorError.CassetteFitnessData[0] = cassetteFitnessData;
                                                                    }
                                                                }
                                                            }
                                                            //Fitness of Cassette Type 2
                                                            if (dev.Length > 8)
                                                            {
                                                                if (int.TryParse(dev.Substring(6, 3), out i))
                                                                {
                                                                    CassetteFitnessData_Type cassetteFitnessData = new CassetteFitnessData_Type();
                                                                    cassetteFitnessData.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(9, 1), out i))
                                                                    {
                                                                        cassetteFitnessData.CassetteFitness = (ErrorSeverity_Type)i;
                                                                        bunchNoteAcceptorError.CassetteFitnessData[1] = cassetteFitnessData;
                                                                    }
                                                                }
                                                            }
                                                            //Fitness of Cassette Type 3
                                                            if (dev.Length > 13)
                                                            {
                                                                if (int.TryParse(dev.Substring(10, 3), out i))
                                                                {
                                                                    CassetteFitnessData_Type cassetteFitnessData = new CassetteFitnessData_Type();
                                                                    cassetteFitnessData.CassetteType = i;
                                                                    if (int.TryParse(dev.Substring(13, 1), out i))
                                                                    {
                                                                        cassetteFitnessData.CassetteFitness = (ErrorSeverity_Type)i;
                                                                        bunchNoteAcceptorError.CassetteFitnessData[2] = cassetteFitnessData;
                                                                    }
                                                                }
                                                            }
                                                            //Fitness of Cassette Type 4
                                                            if (dev.Length > 17)
                                                            {
                                                                if (int.TryParse(dev.Substring(14, 3), out i))
                                                                {
                                                                    CassetteFitnessData_Type cassetteFitnessData = new CassetteFitnessData_Type();
                                                                    cassetteFitnessData.CassetteType = i;

                                                                    if (int.TryParse(dev.Substring(17, 1), out i))
                                                                    {
                                                                        cassetteFitnessData.CassetteFitness = (ErrorSeverity_Type)i;
                                                                        bunchNoteAcceptorError.CassetteFitnessData[3] = cassetteFitnessData;
                                                                    }
                                                                }
                                                            }
                                                            DeviceFitnessData_Type deviceFitnessData = new DeviceFitnessData_Type();
                                                            deviceFitnessData.Item = new object();
                                                            deviceFitnessData.Item = bunchNoteAcceptorError;
                                                            deviceFitnessData.ItemElementName = ItemChoiceDeviceFitnessData_Type.BunchNoteAcceptorError;
                                                            fitnessData.HardwareFitnessData.Device[j] = deviceFitnessData;
                                                            break;
                                                        }
                                                }
                                                j++;
                                            }
                                            terminalStateStatus.Item = fitnessData;
                                            break;
                                            #endregion
                                        }
                                    case "K":
                                        {
                                            #region "TamperAndSensorStatusData_Type (1...74)"
                                            TamperAndSensorStatusData_Type tamperAndSensorStatusData = new TamperAndSensorStatusData_Type();
                                            SensorStatusBytes26_Type sensorStatus = new SensorStatusBytes26_Type();
                                            if (DataInPut[5].Substring(0, 1).Equals("1", StringComparison.Ordinal))
                                                sensorStatus.VibrationAndOrHeatSensor = BitCodedActiveInactive_Type.Active;
                                            else
                                                sensorStatus.VibrationAndOrHeatSensor = BitCodedActiveInactive_Type.Inactive;
                                            sensorStatus.DoorContactSensor = BitCodedActiveInactive_Type.Inactive;
                                            sensorStatus.SilentSignalSensor = BitCodedActiveInactive_Type.Inactive;
                                            sensorStatus.ElectronicsEnclosureSensor = BitCodedActiveInactive_Type.Inactive;
                                            tamperAndSensorStatusData.SensorStatus = sensorStatus;
                                            TamperStatus_Type tamperStatus = new TamperStatus_Type();
                                            tamperStatus.DepositBin = BitCodedInOut_Type.Out;
                                            tamperStatus.CardBin = BitCodedInOut_Type.Out;
                                            tamperStatus.CurrencyRejectBin = BitCodedInOut_Type.Out;
                                            tamperStatus.CurrencyCassettePosition1 = BitCodedInOut_Type.Out;
                                            tamperStatus.CurrencyCassettePosition2 = BitCodedInOut_Type.Out;
                                            tamperStatus.CurrencyCassettePosition3 = BitCodedInOut_Type.Out;
                                            tamperStatus.CurrencyCassettePosition4 = BitCodedInOut_Type.Out;
                                            tamperStatus.CoinDispenser = BitCodedInOut_Type.Out;
                                            tamperStatus.CoinDispenserHopper1 = BitCodedInOut_Type.Out; ;
                                            tamperStatus.CoinDispenserHopper2 = BitCodedInOut_Type.Out; ;
                                            tamperStatus.CoinDispenserHopper3 = BitCodedInOut_Type.Out;
                                            tamperStatus.CoinDispenserHopper4 = BitCodedInOut_Type.Out;
                                            tamperStatus.DPMPocket = BitCodedInOut_Type.Out;
                                            tamperAndSensorStatusData.TamperStatus = tamperStatus;
                                            ExtendedTamperIndicator_Type extendedTamperIndicator = new ExtendedTamperIndicator_Type();
                                            tamperAndSensorStatusData.ExtendedTamperIndicator = extendedTamperIndicator;
                                            Data_Type data = new Data_Type();
                                            tamperAndSensorStatusData.Data = data;
                                            terminalStateStatus.Item = tamperAndSensorStatusData;
                                            break;
                                            #endregion
                                        }
                                    case "L": //SoftwareIDAndReleaseNumberData_Type (1...75)
                                        {
                                            SoftwareIDAndReleaseNumberData_Type softwareIDAndReleaseNumberData = new SoftwareIDAndReleaseNumberData_Type();
                                            terminalStateStatus.Item = softwareIDAndReleaseNumberData;
                                            break;
                                        }
                                    case "M": // LocalConfigurationOptionDigits_Type(1...77)
                                        {
                                            LocalConfigurationOptionDigits_Type localConfigurationOptionDigits = new LocalConfigurationOptionDigits_Type();
                                            terminalStateStatus.Item = localConfigurationOptionDigits;
                                            break;
                                        }
                                    case "N": //NoteDefinitions_Type (1...78)
                                        {
                                            NoteDefinitions_Type noteDefinitions = new NoteDefinitions_Type();
                                            terminalStateStatus.Item = noteDefinitions;
                                            break;
                                        }

                                }

                                solicitedStatusMessage.Status.Item = terminalStateStatus;
                                break;
                            }
                            //default:
                            //    {
                            //        StatusInformation = DataInPut[4];
                            //        break;
                            //    }
                    }
                    return solicitedStatusMessage;
                }
                else { throw new Exception("SolicitedStatusMessageParser() error: Cantidad de separadores insuficientes para el parseo."); }
            }
            catch (Exception ex) { throw ex; }

        }

        //9)- MISCELANEOUS
        #region "MISCELANEOUS"
        private InteractiveTransactionResponseActivations_Type GetInteractiveTransactionResponseActivations(string activeKeys)
        {
            int[] arrByte = new int[activeKeys.Length];
            int i = 0;
            InteractiveTransactionResponseActivations_Type interactiveTransactionResponseActivations = new InteractiveTransactionResponseActivations_Type();
            for (int j = 0; j < activeKeys.Length; j++)
            {
                i = 0;
                if (int.TryParse(activeKeys.Substring(j, 1), out i))
                {
                    arrByte[j] = i;
                }
            }
            if (arrByte.Length > 0)
            {
                interactiveTransactionResponseActivations.NumericKeys = (Activation_Type)arrByte[0];
                interactiveTransactionResponseActivations.NumericKeysSpecified = true;
            }
            if (arrByte.Length > 1)
            {
                interactiveTransactionResponseActivations.FDKAtouchArea = (Activation_Type)arrByte[1];
                interactiveTransactionResponseActivations.FDKAtouchAreaSpecified = true;
            }
            if (arrByte.Length > 2)
            {
                interactiveTransactionResponseActivations.FDKBtouchArea = (Activation_Type)arrByte[2];
                interactiveTransactionResponseActivations.FDKBtouchAreaSpecified = true;
            }
            if (arrByte.Length > 3)
            {
                interactiveTransactionResponseActivations.FDKCtouchArea = (Activation_Type)arrByte[3];
                interactiveTransactionResponseActivations.FDKCtouchAreaSpecified = true;
            }
            if (arrByte.Length > 4)
            {
                interactiveTransactionResponseActivations.FDKDtouchArea = (Activation_Type)arrByte[4];
                interactiveTransactionResponseActivations.FDKDtouchAreaSpecified = true;
            }
            if (arrByte.Length > 5)
            {
                interactiveTransactionResponseActivations.CancelKeyE = (Activation_Type)arrByte[5];
                interactiveTransactionResponseActivations.CancelKeyESpecified = true;
            }
            if (arrByte.Length > 6)
            {
                interactiveTransactionResponseActivations.FDKFtouchArea = (Activation_Type)arrByte[6];
                interactiveTransactionResponseActivations.FDKFtouchAreaSpecified = true;
            }
            if (arrByte.Length > 7)
            {
                interactiveTransactionResponseActivations.FDKGtouchArea = (Activation_Type)arrByte[7];
                interactiveTransactionResponseActivations.FDKGtouchAreaSpecified = true;
            }
            if (arrByte.Length > 8)
            {
                interactiveTransactionResponseActivations.FDKHtouchArea = (Activation_Type)arrByte[8];
                interactiveTransactionResponseActivations.FDKHtouchAreaSpecified = true;
            }
            if (arrByte.Length > 9)
            {
                interactiveTransactionResponseActivations.FDKItouchArea = (Activation_Type)arrByte[9];
                interactiveTransactionResponseActivations.FDKItouchAreaSpecified = true;
            }
            return interactiveTransactionResponseActivations;
        }

        /// <summary>
        /// Obtiene la máscara de activación de FDKs a partir de un valor decimal.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public KeyMask_Type GetKeyMaskData(string text)
        {
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

        public Entities.Const.MsgType GetMessageType(string msg)
        {
            Entities.Const.MsgType ret = Entities.Const.MsgType.UnDefined;
            string[] DataInPut;
            DataInPut = msg.Split(Entities.Const.FS);
            try
            {
                if (DataInPut.Length > 2)
                {
                    //A)- Writte command data.
                    if (DataInPut[0].Equals("4"))
                        ret = Entities.Const.MsgType.TransactionReply;
                    //B)- Writte command data.
                    if (DataInPut[0].Equals("3"))
                        if (DataInPut[3].Length > 1)
                        {
                            if (DataInPut[3].Substring(0, 1).Equals("2"))//ITR
                            {
                                ret = Entities.Const.MsgType.Itr;
                            }
                            else
                                switch (DataInPut[3])
                                {
                                    case "12":
                                        {
                                            ret = Entities.Const.MsgType.State;
                                            break;
                                        }
                                    case "11":
                                        {
                                            ret = Entities.Const.MsgType.Screen;
                                            break;
                                        }
                                    case "1B":
                                        {
                                            ret = Entities.Const.MsgType.ConfParam;
                                            break;
                                        }
                                    case "15":
                                        {
                                            ret = Entities.Const.MsgType.Fit;
                                            break;
                                        }
                                    case "42":
                                        {
                                            ret = Entities.Const.MsgType.EncryptionKeyChange;
                                            break;
                                        }
                                    case "1A":
                                        {
                                            ret = Entities.Const.MsgType.EnhConfParam;
                                            break;
                                        }
                                    case "16":
                                        {
                                            ret = Entities.Const.MsgType.ConfIDload;
                                            break;
                                        }
                                    case "1C":
                                        {
                                            ret = Entities.Const.MsgType.DateAndTime;
                                            break;
                                        }
                                }
                        }
                        else
                            throw new Exception("Insufficient writte command data.");
                    //C)- Terminal Commands
                    if (DataInPut[0].Equals("1"))
                        if (DataInPut[3].Length > 0)
                        {
                            switch (DataInPut[3])
                            {
                                case "2":
                                    {
                                        ret = Entities.Const.MsgType.GoOutOfServiceTermCmd;
                                        break;
                                    }
                                case "1":
                                    {
                                        ret = Entities.Const.MsgType.GoInServiceTermCmd;
                                        break;
                                    }
                                case "3":
                                    {
                                        ret = Entities.Const.MsgType.SndConfIDTermCmd;
                                        break;
                                    }
                                case "4":
                                    {
                                        ret = Entities.Const.MsgType.SndSupplyCountersTermCmd;
                                        break;
                                    }
                                case "71":
                                    {
                                        ret = Entities.Const.MsgType.SndConfHwdTermCmd;
                                        break;
                                    }
                                case "72":
                                    {
                                        ret = Entities.Const.MsgType.SndSuppiesStatusTermCmd;
                                        break;
                                    }
                                case "73":
                                    {
                                        ret = Entities.Const.MsgType.SndHwdFitnessTermCmd;
                                        break;
                                    }
                                case "74":
                                    {
                                        ret = Entities.Const.MsgType.SndSensorStatusTermCmd;
                                        break;
                                    }
                                case "75":
                                    {
                                        ret = Entities.Const.MsgType.SndSoftIDTermCmd;
                                        break;
                                    }
                                case "76":
                                    {
                                        ret = Entities.Const.MsgType.SndEnhConfDataTermCmd;
                                        break;
                                    }
                                case "77":
                                    {
                                        ret = Entities.Const.MsgType.SndLocalConfTermCmd;
                                        break;
                                    }
                                case "78":
                                    {
                                        ret = Entities.Const.MsgType.SndNoteDefinitionsTermCmd;
                                        break;
                                    }
                            }
                        }
                        else
                            throw new Exception("Insufficient terminal data command.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;

        }


        #endregion ""

        //10)- Printer
        #region "Printer"
        internal PrinterDataCommand_Type[] GetPrinterData(string msg)
        {
            List<PrinterDataCommand_Type> listOfPrinterDataCommand = new List<PrinterDataCommand_Type>();
            PrinterDataCommand_Type printerDataCommand;
            StringBuilder sb;
            string print = string.Empty, aux = string.Empty;
            int i = 0;
            try
            {
                sb = new StringBuilder(msg);
                for (int pos = 0; pos < msg.Length; pos++)
                {
                    printerDataCommand = new PrinterDataCommand_Type();

                    //A)- Print command 
                    if (sb[pos] >= 0x20) //Si no es caracter de control, acumulo 
                    {
                        printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.PrintCommand;
                        printerDataCommand.Item = sb[pos].ToString();
                    }
                    else
                    {
                        //B)- Screen commands (busco caracteres de control específicos)
                        switch (sb[pos])
                        {
                            case (char)(0x0A): //LF Line Feed
                                {
                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.LineFeed;
                                    printerDataCommand.Item = new EmptyElement_Type();
                                    break;
                                }
                            case (char)(0x0B): //VT Bold font
                                {
                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SetBoldFont;
                                    printerDataCommand.Item = new EmptyElement_Type();
                                    break;
                                }
                            case (char)(0x0C): //FF Cut and deliver to customer
                                {
                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.FeedToBlackMarkCutAndDeliver;
                                    break;
                                }
                            case (char)(0x0E): //SO Completa un área con espacios
                                {
                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.FillWithSpaces;
                                    printerDataCommand.Item = this.GetFillWithSpaces(sb[pos + 1].ToString());
                                    pos = pos + 1;
                                    break;
                                }
                            case (char)(0x09): //HT
                                {
                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.NextTabColumn;
                                    break;
                                }
                            case (char)(0x1B): //ESC
                                {
                                    switch (sb[pos + 1])
                                    {
                                        case (char)(0x5B): // [ Set Left or Right Margin
                                            {
                                                aux = string.Empty;
                                                int l;
                                                for (l = pos + 2; !sb[l].Equals((char)0x70) & !sb[l].Equals((char)0x71) & l < sb.Length; l++)
                                                {
                                                    aux = string.Format("{0}{1}", aux, sb[l]);
                                                }
                                                pos = l;
                                                if (sb[l] == 0x70) // p Set Left Margin
                                                {
                                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SetLeftMargin;
                                                    SetLeftMargin_Type setLeftMargin = new SetLeftMargin_Type();
                                                    i = 0;
                                                    if (int.TryParse(aux, out i))
                                                        setLeftMargin.PositionOfLeftmostPrintColFromLeftPaperEdge = i;
                                                    printerDataCommand.Item = setLeftMargin;
                                                }
                                                else if (sb[l] == 0x71) // q Set Right Margin
                                                {
                                                    printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SetRightMargin;
                                                    SetRightMargin_Type setRightMargin = new SetRightMargin_Type();
                                                    i = 0;
                                                    if (int.TryParse(aux, out i))
                                                        setRightMargin.PositionOfRightmostPrintColFromCol2 = i;
                                                    printerDataCommand.Item = setRightMargin;
                                                }
                                                break;
                                            }
                                        case (char)(0x25): // % Select OS/2 Code Page
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SelectOS2CodePage;
                                                printerDataCommand.Item = string.Format("{0}{1}{2}", sb[pos + 2], sb[pos + 3], sb[pos + 4]);
                                                pos = pos + 4;
                                                break;
                                            }
                                        case (char)(0x32): // 2 Select International Character Sets
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SelectInternationalCharsets;
                                                printerDataCommand.Item = string.Format("{0}", sb[pos + 2]);
                                                pos = pos + 2;
                                                break;
                                            }
                                        case (char)(0x33): // 3 Select Arabic Character Sets
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SelectArabicCharsets;
                                                printerDataCommand.Item = string.Format("{0}", sb[pos + 2]);
                                                pos = pos + 2;
                                                break;
                                            }
                                        case (char)(0x2F): // / Print Downloadable Bit Image
                                            {
                                                aux = string.Empty;
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.PrintDownloadableBitImage;
                                                PrintDownloadableBitImage_Type printDownloadableBitImage = new PrintDownloadableBitImage_Type();
                                                printDownloadableBitImage.ImageFormat = new PrintBitImageFormat_Type();
                                                if (sb[pos + 2].Equals((char)(0x31)))
                                                    printDownloadableBitImage.ImageNumber = 1;
                                                if (sb[pos + 2].Equals((char)(0x32)))
                                                    printDownloadableBitImage.ImageNumber = 2;
                                                if (sb[pos + 3].Equals((char)(0x30)))
                                                {
                                                    printDownloadableBitImage.ImageFormat.DoubleHeight = false;
                                                    printDownloadableBitImage.ImageFormat.DoubleWidth = false;
                                                }
                                                if (sb[pos + 3].Equals((char)(0x31)))
                                                {
                                                    printDownloadableBitImage.ImageFormat.DoubleHeight = true;
                                                    printDownloadableBitImage.ImageFormat.DoubleWidth = false;
                                                }
                                                if (sb[pos + 3].Equals((char)(0x32)))
                                                {
                                                    printDownloadableBitImage.ImageFormat.DoubleHeight = false;
                                                    printDownloadableBitImage.ImageFormat.DoubleWidth = true;
                                                }
                                                if (sb[pos + 3].Equals((char)(0x33)))
                                                {
                                                    printDownloadableBitImage.ImageFormat.DoubleHeight = true;
                                                    printDownloadableBitImage.ImageFormat.DoubleWidth = true;
                                                }
                                                printerDataCommand.Item = printDownloadableBitImage;
                                                pos = pos + 3;
                                                break;
                                            }
                                        case (char)(0x6B): // k Print Barcode
                                            int num2 = int.Parse($"{sb[pos + 2]}{sb[pos + 3]}");
                                            printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.PrintBarcode;
                                            PrintBarcode_Type printBarcode_Type = new PrintBarcode_Type();
                                            printBarcode_Type.BarcodeType = BarcodeType_Type.CODE128;
                                            int m;
                                            for (m = pos + 4; m <= num2 + pos + 4; m++)
                                            {
                                                printBarcode_Type.ASCIIToBePrinter = $"{printBarcode_Type.ASCIIToBePrinter}{sb[m]}";
                                            }
                                            printerDataCommand.Item = printBarcode_Type;
                                            pos = m - 1;
                                            break;
                                        case (char)(0x6C): // l Print QR
                                            string num = $"{sb[pos + 2]}{sb[pos + 3]}{sb[pos + 4]}";
                                            int n = int.Parse(num);
                                            printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.PrintQR;
                                            PrintBarcode_Type printBarcode = new PrintBarcode_Type();
                                            printBarcode.BarcodeType = BarcodeType_Type.CODE128;
                                            int m1;
                                            for (m1 = pos + 5; m1 <= n + pos + 5; m1++)
                                            {
                                                printBarcode.ASCIIToBePrinter = $"{printBarcode.ASCIIToBePrinter}{sb[m1]}";
                                            }
                                            printerDataCommand.Item = printBarcode;
                                            pos = m1 - 1;
                                            break;
                                        case (char)(0x47): // G Print Graphics
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.PrintGraphics;
                                                PrintGraphics_Type printGraphics = new PrintGraphics_Type();
                                                //Acumula caracteres distintos a 'ESC'
                                                int l;
                                                for (l = pos + 2; !sb[l].Equals((char)0x1B) & l < sb.Length; l++)
                                                {
                                                    printGraphics.GraphicsFileName = string.Format("{0}{1}", printGraphics.GraphicsFileName, sb[l]);
                                                }
                                                printerDataCommand.Item = printGraphics;
                                                pos = l;
                                                break;
                                            }
                                        case (char)(0x2A): // * Define Downloadable Bit Image
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.DefineDownloadableBitImage;
                                                DefineDownloadableBitImage_Type defineDownloadableBitImage = new DefineDownloadableBitImage_Type();
                                                if (sb[pos + 2].Equals("1"))
                                                    defineDownloadableBitImage.BitImageNumber = 1;
                                                if (sb[pos + 2].Equals("2"))
                                                    defineDownloadableBitImage.BitImageNumber = 2;
                                                //Acumula caracteres distintos a 'ESC'
                                                int l;
                                                for (l = pos + 3; !sb[l].Equals((char)0x1B) & l < sb.Length; l++)
                                                {
                                                    defineDownloadableBitImage.FileName = string.Format("{0}{1}", defineDownloadableBitImage.FileName, sb[l]);
                                                }
                                                printerDataCommand.Item = defineDownloadableBitImage;
                                                pos = l;
                                                break;
                                            }
                                        case (char)(0x28): // ( (Select alternative charSet)
                                            {
                                                printerDataCommand.ItemElementName = ItemChoicePrinterDataCommand_Type.SelectAlternativeCharset;
                                                PrinterDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset sacs = new PrinterDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset();
                                                i = 0;
                                                if (int.TryParse(sb[pos + 2].ToString(), out i))
                                                    sacs.Charset = (PrinterCharSet_Type)i;
                                                printerDataCommand.Item = sacs;
                                                pos = pos + 2;
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                    }
                    listOfPrinterDataCommand.Add(printerDataCommand);
                    printerDataCommand = new PrinterDataCommand_Type();
                }
            }
            catch (Exception ex) { throw new Exception(string.Format("GetPrinterData(): {0}", ex.Message)); }
            return listOfPrinterDataCommand.ToArray();
        }

        private FillWithSpaces_Type GetFillWithSpaces(string character)
        {
            FillWithSpaces_Type fillWithSpaces = new FillWithSpaces_Type();
            switch (character)
            {
                case "1":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item1;
                    break;
                case "2":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item2;
                    break;
                case "3":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item3;
                    break;
                case "4":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item4;
                    break;
                case "5":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item5;
                    break;
                case "6":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item6;
                    break;
                case "7":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item7;
                    break;
                case "8":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item8;
                    break;
                case "9":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item9;
                    break;
                case ":":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item10;
                    break;
                case ";":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item11;
                    break;
                case "<":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item12;
                    break;
                case "=":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item13;
                    break;
                case ">":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item12;
                    break;
                case "?":
                    fillWithSpaces.NumberOfSpaces = NumberOfSpaces_Type.Item15;
                    break;
            }
            return fillWithSpaces;
        }
        #endregion "Printer"
    }
}

