using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Entities
{

    [XmlRoot("TransactionList")]
    public class TransactionList
    {
        public TransactionList()
        {
        }

        public static bool GetMapping(out TransactionList mapping)
        {
            string fileName = string.Format(@"{0}Config\TransactionObjectList.xml", Entities.Const.appPath);
            mapping = new TransactionList();
            bool ret;
            try
            {
                if (!File.Exists(fileName))
                {
                    mapping.DisableBagStatusBar = true;
                    mapping.DisableExitButton = false;
                    mapping.Transactions = new List<TransactionObject>
                    {
                        new TransactionObject {
                            MenuItem = new TransactionMenuItem("paymentServicesTx", Const.Colors.Primary, Const.Icons.Barcode, 'A'),
                            Dependencies = new List<Enums.Devices> { Enums.Devices.CashAcceptor, Enums.Devices.CashDispenser, Enums.Devices.CoinDispenser, Enums.Devices.IOBoard }
                        },
                        new TransactionObject {
                            MenuItem = new TransactionMenuItem("mobileTopupTx", Const.Colors.Primary, Const.Icons.Mobile, 'B'),
                            Dependencies = new List<Enums.Devices> { Enums.Devices.CashAcceptor, Enums.Devices.CashDispenser, Enums.Devices.CoinDispenser, Enums.Devices.IOBoard }
                        },
                        new TransactionObject {
                            MenuItem = new TransactionMenuItem("cashDepositTx", Const.Colors.Primary, Const.Icons.Bank, 'C', false),
                            Dependencies = new List<Enums.Devices> { Enums.Devices.CashAcceptor, Enums.Devices.CashDispenser, Enums.Devices.CoinDispenser, Enums.Devices.IOBoard }
                        },
                        new TransactionObject {
                            MenuItem = new TransactionMenuItem("cashWithdrawalTx", Const.Colors.Primary, Const.Icons.Bank, 'D', false),
                            Dependencies = new List<Enums.Devices> {Enums.Devices.CashDispenser, Enums.Devices.CoinDispenser, Enums.Devices.IOBoard }
                        }
                    };
                }
                mapping = Utilities.Utils.GetGenericXmlData<TransactionList>(out ret, fileName, mapping);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in {0} file. {1}", fileName, ex.InnerException));
            }
            return ret;
        }

        [XmlElement("TransactionObject", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TransactionObject> Transactions { get; set; }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool DisableBagStatusBar { get; set; }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool DisableExitButton { get; set; }
    }

    [Serializable]
    public class TransactionObject
    {
        public TransactionObject()
        {
        }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public TransactionMenuItem MenuItem { get; set; }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Enums.Devices> Dependencies { get; set; }
    }

    /*[Serializable]
    public enum TransactionDependency
    {
        [XmlEnum(Name = "CashAcceptor")]
        CashAcceptor,

        [XmlEnum(Name = "CashDispenser")]
        CashDispenser,

        [XmlEnum(Name = "CoinDispenser")]
        CoinDispenser,

        [XmlEnum(Name = "Printer")]
        Printer,
    }*/

    public class UnitDependency
    {
        public enum DepStatus { OK, InProgress, Error }
        public string ExternalCode { get; set; }
        public DepStatus Status { get; set; }
        public Enums.Devices Dependency { get; set; }
    }
}
