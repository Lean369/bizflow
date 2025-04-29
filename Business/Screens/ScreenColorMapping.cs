using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Collections;
using System.IO;
//using System.Xml.Serialization;
using Entities;

namespace Business
{
    public class ScreenColorMapping
    {
        private BlinkingCommand_Type NdcColorNameField;
        private string NdcColorCodeField;
        private string HtmlColorCodeField;
        //private static string AppPath = System.AppDomain.CurrentDomain.BaseDirectory;

        public ScreenColorMapping(BlinkingCommand_Type _ndcColorName, string _ndcColorCode, string _htmlColorCode)
        {
            this.NdcColorName = _ndcColorName;
            this.NdcColorCode = _ndcColorCode;
            this.HtmlColorCode = _htmlColorCode;
        }

        public ScreenColorMapping() { }

        public static bool GetScreenColorMapping(out List<ScreenColorMapping> listOfScreenColorMapping)
        {
            bool ret = false;
            string fileName = string.Format(@"{0}Config\ScreenColorMapping.xml", Entities.Const.appPath);
            listOfScreenColorMapping = new List<ScreenColorMapping>();
            try
            {
                if (!Directory.Exists(string.Format(@"{0}\Config", Entities.Const.appPath)))
                {
                    Directory.CreateDirectory(string.Format(@"{0}\Config", Entities.Const.appPath));
                }
                if (!File.Exists(fileName))
                {
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.ResetsColorsToDefaults_BlinkingOff, "0", "0"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.SetBlinkingOn, "10", "0"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.SetBlinkingOff, "11", "0"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.TransparentBackground, "80", "#ffffff"));

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackForeground_LowIntensity, "20", "#000000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackForeground_HighIntensity, "B0", "#640000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackBackground_LowIntensity, "30", "#000000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackBackground_HighIntensity, "C0", "#640000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackDefaultForeground_LowIntensity, "60", "#000000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackDefaultForeground_HighIntensity, "F0", "#640000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackDefaultBackground_LowIntensity, "70", "#000000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlackDefaultBackground_HighIntensity, "G0", "#640000")); //No pasa

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedForeground_LowIntensity, "21", "#A80000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedForeground_HighIntensity, "B1", "#ff0000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedBackground_LowIntensity, "31", "#A80000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedBackground_HighIntensity, "C1", "#ff0000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedDefaultForeground_LowIntensity, "61", "#A80000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedDefaultForeground_HighIntensity, "F1", "#ff0000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedDefaultBackground_LowIntensity, "71", "#A80000"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.RedDefaultBackground_HighIntensity, "G1", "#ff0000"));

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenForeground_LowIntensity, "B2", "#005f00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenForeground_HighIntensity, "22", "#00e300"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenBackground_LowIntensity, "C2", "#005f00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenBackground_HighIntensity, "32", "#00e300"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenDefaultForeground_LowIntensity, "F2", "#005f00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenDefaultForeground_HighIntensity, "62", "#00e300"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenDefaultBackground_LowIntensity, "G2", "#005f00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.GreenDefaultBackground_HighIntensity, "72", "#00e300")); //No pasa

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowForeground_LowIntensity, "B3", "#d6b800"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowForeground_HighIntensity, "23", "#ffff00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowBackground_LowIntensity, "C3", "#d6b800"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowBackground_HighIntensity, "33", "#ffff00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowDefaultForeground_LowIntensity, "F3", "#d6b800"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowDefaultForeground_HighIntensity, "63", "#ffff00"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowDefaultBackground_LowIntensity, "G3", "#d6b800")); //No pasa
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.YellowDefaultBackground_HighIntensity, "73", "#ffff00")); //No pasa

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueForeground_LowIntensity, "24", "#0000A8"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueForeground_HighIntensity, "B4", "#0000ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueBackground_LowIntensity, "34", "#0000A8"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueBackground_HighIntensity, "C4", "#0000ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueDefaultForeground_LowIntensity, "64", "#0000A8"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueDefaultForeground_HighIntensity, "F4", "#0000ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueDefaultBackground_LowIntensity, "74", "#0000A8"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.BlueDefaultBackground_HighIntensity, "G4", "#0000ff"));

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaForeground_LowIntensity, "25", "#ffa1ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaForeground_HighIntensity, "B5", "#ff00ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaBackground_LowIntensity, "35", "#ffa1ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaBackground_HighIntensity, "C5", "#ff00ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaDefaultForeground_LowIntensity, "65", "#ffa1ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaDefaultForeground_HighIntensity, "F5", "#ff00ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaDefaultBackground_LowIntensity, "75", "#ffa1ff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.MagentaDefaultBackground_HighIntensity, "G5", "#ff00ff"));

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanForeground_LowIntensity, "26", "#d6ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanForeground_HighIntensity, "B6", "#00ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanBackground_LowIntensity, "36", "#d6ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanBackground_HighIntensity, "C6", "#00ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanDefaultForeground_LowIntensity, "66", "#d6ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanDefaultForeground_HighIntensity, "F6", "#00ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanDefaultBackground_LowIntensity, "76", "#d6ffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.CyanDefaultBackground_HighIntensity, "G6", "#00ffff"));

                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteForeground_LowIntensity, "B7", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteForeground_HighIntensity, "27", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteBackground_LowIntensity, "C7", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteBackground_HighIntensity, "37", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteDefaultForeground_LowIntensity, "F7", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteDefaultForeground_HighIntensity, "67", "#ffffff"));
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteDefaultBackground_LowIntensity, "G7", "#ffffff")); //No pasa
                    listOfScreenColorMapping.Add(new ScreenColorMapping(BlinkingCommand_Type.WhiteDefaultBackground_HighIntensity, "77", "#ffffff")); 
                }
                listOfScreenColorMapping = Utilities.Utils.GetGenericXmlData<List<ScreenColorMapping>>(out ret, fileName, listOfScreenColorMapping);                
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        //////////////////////////////////////////////////////////////////
        ////////////////////////PROPERTIES////////////////////////////////
        //////////////////////////////////////////////////////////////////
        [System.Xml.Serialization.XmlElement("NdcColorName")]
        public BlinkingCommand_Type NdcColorName
        {
            get { return this.NdcColorNameField; }
            set { this.NdcColorNameField = value; }
        }

        [System.Xml.Serialization.XmlElement("NdcColorCode")]
        public string NdcColorCode
        {
            get { return this.NdcColorCodeField; }
            set { this.NdcColorCodeField = value; }
        }

        [System.Xml.Serialization.XmlElement("HtmlColorCode")]
        public string HtmlColorCode
        {
            get { return this.HtmlColorCodeField; }
            set { this.HtmlColorCodeField = value; }
        }
    }
}
