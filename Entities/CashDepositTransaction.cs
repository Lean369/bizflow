using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable()]
    public class CashDepositTransaction
    {
        private List<CashInInfo> _ListCashInInfo;
        private string _Currency;
        private string _User;
        private string _UserName;
        private string _CollectionId;
        List<ExtraData> _ExtraData;
        private DateTime _TxDateTime;
        private int _tsn;
        private bool _depositHardwareError;
        public CashDepositTransaction() { }

        public CashDepositTransaction(List<CashInInfo> listCashInInfo, string currency, string user, string userName, string collectionId, List<ExtraData> extraData, DateTime txDateTime, int tsn, bool depositHwError)
        {
            _ListCashInInfo = listCashInInfo;
            _Currency = currency;
            _User = user;
            _UserName = userName;
            _CollectionId = collectionId;
            _ExtraData = extraData;
            _TxDateTime = txDateTime;
            _tsn = tsn;
            _depositHardwareError = depositHwError;
        }

        [XmlArray("ListCashInInfo"), XmlArrayItem(typeof(CashInInfo), ElementName = "CashInInfo")]
        public List<CashInInfo> ListCashInInfo { get => _ListCashInInfo; set => _ListCashInInfo = value; }

        [XmlElement("Currency")]
        public string Currency { get => _Currency; set => _Currency = value; }

        [XmlElement("User")]
        public string User { get => _User; set => _User = value; }

        [XmlElement("UserName")]
        public string UserName { get => _UserName; set => _UserName = value; }

        [XmlElement("CollectionId")]
        public string CollectionId { get => _CollectionId; set => _CollectionId = value; }

        [XmlElement("ExtraData")]
        public List<ExtraData> ExtraData { get => _ExtraData; set => _ExtraData = value; }

        [XmlElement("txDateTime")]
        public DateTime txDateTime { get => _TxDateTime; set => _TxDateTime = value; }

        [XmlElement("tsn")]
        public int tsn { get => _tsn; set => _tsn = value; }

        [XmlElement("depositHardwareError")]
        public bool depositHardwareError { get => _depositHardwareError; set => _depositHardwareError = value; }

    }
}
