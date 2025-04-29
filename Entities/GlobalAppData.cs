using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace Entities
{
    /// <summary>
    /// Esta clase es global para todo el proyecto
    /// </summary>
    public sealed class GlobalAppData
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private static readonly NLog.Logger EJ = NLog.LogManager.GetLogger("EJ");

        public string GlobalHtmlScreenData;

        public static GlobalAppData Instance { get; private set; }

        private static SimpleEncryption Encryptor;

        private GlobalAppData() { } //to prevent creation of another instance

        private static Dictionary<string, object> Scratchpad;

        static GlobalAppData()
        {
            Instance = new GlobalAppData();
            Scratchpad = new Dictionary<string, object>();
            Encryptor = new SimpleEncryption(Const.key);
        }

        public string EncryptString(string text)
        {
           return Encryptor.Encrypt(text);
        }

        public string DecryptString(string text)
        {
            return Encryptor.Decrypt(text);
        }

        public void WriteEJ(string data)
        {
            try
            {
                EJ.Trace(data);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void ClearScratchpad()
        {
            Scratchpad.Clear();
        }

        public bool GetScratchpad(string key, out object value)
        {
            value = string.Empty;
            bool ret = false;
            if (Scratchpad != null)
            {
                if (Scratchpad.TryGetValue(key, out value))
                    ret = true;
            }
            return ret;
        }

        public bool SetScratchpad(string key, object value)
        {
            bool ret = false;
            if (Scratchpad != null)
            {
                if (Scratchpad.ContainsKey(key))
                    Scratchpad.Remove(key);
                Scratchpad.Add(key, value);
                ret = true;
            }
            return ret;
        }

        public bool DeleteScratchpad(string key)
        {
            bool ret = false;
            if (Scratchpad != null)
            {
                if (Scratchpad.ContainsKey(key))
                {
                    Scratchpad.Remove(key);
                    ret = true;
                }
            }
            return ret;
        }

        public bool GetAlephObject<T>(string path, string key, out T output)
        {
            bool ret = false;
            output = default(T);
            try
            {
                string certPath = $"{Const.appPath}AlephCERT.pfx";
                if (File.Exists(certPath))
                {
                    if (File.Exists(path))
                    {
                        var cert = Utilities.Encryption.GetX509CertFromFile(certPath, Utils.HexToStr(key));
                        if (cert.PublicKey == null)
                            throw new Exception("Invalid public key");
                        if (Utilities.Encryption.ProDecryptObject<T>(path, out output, cert.SerialNumber))
                        {
                            Log.Info($"Get Aleph Object \"{Path.GetFileNameWithoutExtension(path)}\" OK");
                            ret = true;
                        }
                        else
                            Log.Error($"Get Aleph Object \"{Path.GetFileNameWithoutExtension(path)}\" ERROR");
                    }
                    else
                        Log.Error($"File \"{path}\" not found");
                }
                else
                    Log.Error($"File \"{certPath}\" not found");
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        public bool CreateAlephObject<T>(string path, string key, T xmlDoc)
        {
            bool ret = true;
            try
            {
                string certPath = $"{Const.appPath}AlephCERT.pfx";
                var cert = Utilities.Encryption.GetX509CertFromFile(certPath, Utils.HexToStr(key));
                if (cert.PublicKey == null)
                    throw new Exception("Invalid public key");
                ret = Utilities.Encryption.ProEncryptObject<T>(path, xmlDoc, cert.SerialNumber);
                Log.Info("ProEncryptObject success!");
            }
            catch (Exception ex) { Log.Fatal(ex); ret = false; }
            return ret;
        }
    }
}
