using Entities;
using System;
using System.Xml.Serialization;

namespace Business
{
    public class PrintConverter
    {
        //private string source = "NDC.Print.Conv.NDCConvertCharacters";
        //private XmlSerializer prePrintedSerializer;
        private PrinterDataCommand_Type[] stillToPrint;
        private bool enableVoice;
        private SpokenResponseItem_Type[] toSpeech;
        private PrintingInstruction_Type[] toBePrintedAfterSpeech;

        public bool EnableVoice
        {
            get
            {
                return this.enableVoice;
            }
            set
            {
                this.enableVoice = value;
            }
        }

        public SpokenResponseItem_Type[] ToSpeech
        {
            get
            {
                return this.toSpeech;
            }
            set
            {
                this.toSpeech = value;
            }
        }

        public PrintingInstruction_Type[] ToBePrintedAfterSpeech
        {
            get
            {
                return this.toBePrintedAfterSpeech;
            }
            set
            {
                this.toBePrintedAfterSpeech = value;
            }
        }


        public string GetCommandsStr(PrintingInstruction_Type[] printingInstr, bool isStatement)
        {
            string text = "";
            for (int i = 0; i < printingInstr.Length; i++)
            {
                if (printingInstr[i] != null)
                {
                    text += this.GetCommands(printingInstr[i].PrinterData, false, isStatement);
                    if (this.toSpeech != null && this.enableVoice)
                    {
                        int num = printingInstr.Length - i - 1;
                        if (this.stillToPrint != null)
                        {
                            num++;
                        }
                        if (num > 0)
                        {
                            this.toBePrintedAfterSpeech = new PrintingInstruction_Type[num];
                            int num2 = 0;
                            if (this.stillToPrint != null)
                            {
                                PrintingInstruction_Type printingInstruction_Type = new PrintingInstruction_Type();
                                printingInstruction_Type.PrinterData = this.stillToPrint;
                                this.toBePrintedAfterSpeech[0] = printingInstruction_Type;
                                num2++;
                            }
                            else
                            {
                                i++;
                            }
                            for (int j = num2; j < num; j++)
                            {
                                this.toBePrintedAfterSpeech[j] = printingInstr[j + i];
                            }
                        }
                        return text;
                    }
                }
            }
            return text;
        }


        public string GetCommands(PrinterDataCommand_Type[] arrayOfCommands, bool isPrePrintedData, bool isStatement)
        {
            if (arrayOfCommands == null && isPrePrintedData)
            {
                return null;
            }
            string text = "";
            for (int i = 0; i < arrayOfCommands.Length; i++)
            {
                switch (arrayOfCommands[i].ItemElementName)
                {
                    case ItemChoicePrinterDataCommand_Type.LineFeed:
                        text += "\n";
                        break;
                    case ItemChoicePrinterDataCommand_Type.SelectAlternativeCharset:
                        {
                            //PrinterDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset printerDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset = new PrinterDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset();
                            //printerDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset = (PrinterDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset)arrayOfCommands[i].Item;
                            //string charsetDesignator = this.GetCharsetDesignator(printerDataCommand_TypeAbstractPrinterDataCommandSelectAlternativeCharset.Charset);
                            //if (charsetDesignator != null)
                            //{
                            //    text = text + "\u001bs" + charsetDesignator;
                            //}
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.FeedToBlackMarkCutAndDeliver:
                        //text += "\f";
                        break;
                    case ItemChoicePrinterDataCommand_Type.NextTabColumn:
                        //text += "\t";
                        break;
                    case ItemChoicePrinterDataCommand_Type.FillWithSpaces:
                        {
                            FillWithSpaces_Type fillWithSpaces_Type = new FillWithSpaces_Type();
                            fillWithSpaces_Type = (FillWithSpaces_Type)arrayOfCommands[i].Item;
                            string text2 = fillWithSpaces_Type.NumberOfSpaces.ToString();
                            text2 = text2.Substring(4);
                            text += this.GenerateBlanks(text2);
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.SetLeftMargin:
                        {
                            //SetLeftMargin_Type setLeftMargin_Type = new SetLeftMargin_Type();
                            //setLeftMargin_Type = (SetLeftMargin_Type)arrayOfCommands[i].Item;
                            //string str = setLeftMargin_Type.PositionOfLeftmostPrintColFromLeftPaperEdge.ToString();
                            //text = text + "\u001b[" + str + "p";
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.SetBoldFont:
                        {
                            text = text + "\u001b[30;1m";
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.SetRightMargin:
                        {
                            //SetRightMargin_Type setRightMargin_Type = new SetRightMargin_Type();
                            //setRightMargin_Type = (SetRightMargin_Type)arrayOfCommands[i].Item;
                            //string str2 = setRightMargin_Type.PositionOfRightmostPrintColFromCol2.ToString();
                            //text = text + "\u001b[" + str2 + "q";
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.PrintDownloadableBitImage:
                        if (!isStatement)
                        {
                            PrintDownloadableBitImage_Type printDownloadableBitImage_Type = new PrintDownloadableBitImage_Type();
                            printDownloadableBitImage_Type = (PrintDownloadableBitImage_Type)arrayOfCommands[i].Item;
                            string str3 = printDownloadableBitImage_Type.ImageNumber.ToString();
                            PrintBitImageFormat_Type imageFormat = printDownloadableBitImage_Type.ImageFormat;
                            string str4;
                            if (!imageFormat.DoubleWidth)
                            {
                                if (!imageFormat.DoubleHeight)
                                {
                                    str4 = "0";
                                }
                                else
                                {
                                    str4 = "1";
                                }
                            }
                            else if (!imageFormat.DoubleHeight)
                            {
                                str4 = "2";
                            }
                            else
                            {
                                str4 = "3";
                            }
                            text = text + "\u001b/" + str3 + str4;
                        }
                        break;
                    case ItemChoicePrinterDataCommand_Type.PrintGraphics:
                        {
                            //PrintGraphics_Type printGraphics_Type = new PrintGraphics_Type();
                            //printGraphics_Type = (PrintGraphics_Type)arrayOfCommands[i].Item;
                            //text = text + "\u001bG" + printGraphics_Type.GraphicsFileName + "\u001b\\";
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.DefineDownloadableBitImage:
                        if (!isStatement)
                        {
                            DefineDownloadableBitImage_Type defineDownloadableBitImage_Type = new DefineDownloadableBitImage_Type();
                            defineDownloadableBitImage_Type = (DefineDownloadableBitImage_Type)arrayOfCommands[i].Item;
                            object obj = text;
                            text = string.Concat(new object[]
                            {
                            obj,
                            "\u001b*",
                            defineDownloadableBitImage_Type.BitImageNumber,
                            defineDownloadableBitImage_Type.FileName,
                            "\u001b\\"
                            });
                        }
                        break;
                    case ItemChoicePrinterDataCommand_Type.DownloadLogo:
                        if (isStatement)
                        {
                            DownloadLogo_Type downloadLogo_Type = new DownloadLogo_Type();
                            downloadLogo_Type = (DownloadLogo_Type)arrayOfCommands[i].Item;
                            string text3 = text;
                            text = string.Concat(new string[]
                            {
                            text3,
                            "\u001b*",
                            downloadLogo_Type.ThermalStatementPrinterMemoryBufferNumber.ToString(),
                            downloadLogo_Type.Filename,
                            "\u001b\\"
                            });
                        }
                        break;
                    case ItemChoicePrinterDataCommand_Type.PrintLogo:
                        if (isStatement)
                        {
                            PrintLogo_Type printLogo_Type = new PrintLogo_Type();
                            printLogo_Type = (PrintLogo_Type)arrayOfCommands[i].Item;
                            text = text + "\u001b/" + printLogo_Type.ThermalStatementPrinterMemoryBufferNumber.ToString() + "\u001b\\";
                        }
                        break;
                    case ItemChoicePrinterDataCommand_Type.PrintBarcode:
                        {
                            PrintBarcode_Type printBarcode_Type = (PrintBarcode_Type)arrayOfCommands[i].Item;
                            text = text + "\u001b/" + printBarcode_Type.ASCIIToBePrinter + "\u001b\\";
                            break;
                        }
                    case ItemChoicePrinterDataCommand_Type.PrintQR:
                        {
                            PrintBarcode_Type printBarcode_Type = (PrintBarcode_Type)arrayOfCommands[i].Item;
                            text = text + "\u001b-" + printBarcode_Type.ASCIIToBePrinter;
                            break;
                        }
                    //case ItemChoicePrinterDataCommand_Type.SpokenResponse:
                    //    if (this.enableVoice)
                    //    {
                    //        //Log.WriteLine(this.source, "The print operation will be executed with the commands added to the string of commands before this spoken command. After this, the message or file indicated by this spoken command will be spoken. Finally, the remaining commands will be printed.", 8, 1);
                    //        PrinterDataCommand_TypeAbstractPrinterDataCommandSpokenResponse printerDataCommand_TypeAbstractPrinterDataCommandSpokenResponse = new PrinterDataCommand_TypeAbstractPrinterDataCommandSpokenResponse();
                    //        printerDataCommand_TypeAbstractPrinterDataCommandSpokenResponse = (PrinterDataCommand_TypeAbstractPrinterDataCommandSpokenResponse)arrayOfCommands[i].Item;
                    //        this.toSpeech = printerDataCommand_TypeAbstractPrinterDataCommandSpokenResponse.SpokenResponseItem;
                    //        int num = arrayOfCommands.Length - i - 1;
                    //        if (num > 0)
                    //        {
                    //            this.stillToPrint = new PrinterDataCommand_Type[num];
                    //            for (int j = 0; j < this.stillToPrint.Length; j++)
                    //            {
                    //                this.stillToPrint[j] = arrayOfCommands[i + j + 1];
                    //            }
                    //        }
                    //        else
                    //        {
                    //            this.stillToPrint = null;
                    //        }
                    //        return text;
                    //    }
                    //    break;
                    case ItemChoicePrinterDataCommand_Type.PrintCommand:
                        text += arrayOfCommands[i].Item;
                        break;
                }
            }
            return text;
        }

        private string GetCharsetDesignator(PrinterCharSet_Type charset)
        {
            switch (charset)
            {
                case PrinterCharSet_Type.SingleSizeAlphanumeric1:
                    return "1";
                case PrinterCharSet_Type.SingleSizeAlphanumeric2:
                    return "2";
                case PrinterCharSet_Type.SingleSizeAlphanumeric3:
                    return "7";
                case PrinterCharSet_Type.DoubleSizeAlphanumeric1:
                    return ">";
                case PrinterCharSet_Type.DoubleSizeAlphanumeric2:
                    return "?";
                case PrinterCharSet_Type.DoubleSizeAlphanumeric3:
                    return "B";
                case PrinterCharSet_Type.CondensedAlphanumericSet1:
                    return "C";
                case PrinterCharSet_Type.CondensedAlphanumericSet2:
                    return "D";
                case PrinterCharSet_Type.CondensedAlphanumericSet3:
                    return "E";
                case PrinterCharSet_Type._12cpiSizeAlphanumeric1:
                    return "F";
                case PrinterCharSet_Type._12cpiSizeAlphanumeric2:
                    return "G";
                case PrinterCharSet_Type._12cpiSizeAlphanumeric3:
                    return "H";
                case PrinterCharSet_Type.SingleSizeAlphanumeric4:
                    return "I";
                case PrinterCharSet_Type.SingleSizeAlphanumeric5:
                    return "J";
                case PrinterCharSet_Type.DoubleSizeAlphanumeric4:
                    return "K";
                case PrinterCharSet_Type.DoubleSizeAlphanumeric5:
                    return "L";
                case PrinterCharSet_Type.CondensedAlphanumericSet4:
                    return "M";
                case PrinterCharSet_Type.CondensedAlphanumericSet5:
                    return "N";
                case PrinterCharSet_Type._12cpiSizeAlphanumeric4:
                    return "O";
                case PrinterCharSet_Type._12cpiSizeAlphanumeric5:
                    return "P";
                default:
                    return null;
            }
        }

        private string GenerateBlanks(string size)
        {
            string text = "";
            try
            {
                int num = Convert.ToInt32(size);
                for (int i = 0; i < num; i++)
                {
                    text += " ";
                }
            }
            catch (Exception)
            {
                //Log.WriteLine(this.source, "Exception when try to converting a string to an integer: " + ex.Message, 4, 1);
            }
            return text;
        }

        public string RemoveInvalidChars(string dataToBePrinted)
        {
            string text = dataToBePrinted.Replace("\\u0000", "");
            text = text.Replace("\0", "");
            text = text.Replace("\\u0009", "\t");
            //Log.WriteLine(this.source, "Remove any non display character from the data to be printed.", 16, 1);
            return text.Replace("\u0011", "");
        }


    }
}
