using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Entities.PaymentService
{
    public static class PSConst
    {
        public const string PAYMENT_SERVICE_DATA = "PaymentServiceData";
    }

    public enum PropKey
    {
        ServiceID,
        ServiceName,
        Barcode,
        BillID,
        CurrentOrder,
        LastOrder,
        CartID,
        CartStatus,
        CartDate,
        FieldDetailList,
        PaymentList,
        PaymentMethodID,
        CurrencyID,
        Status,
        InvoiceSummary,
        SearchQuery,
        SearchCategory,
        Bills,
        Totals,
        TipoDoc,
        NroDoc,
        TipoCta,
        NroCta,
        Moneda,
        TipoDocOp,
        NroDocOp,
        FPago,
        Importe,
        Codeline,
        IdentificadorSobre,
        CantidadEnSobre,
        CollectionId,
    }

    public enum PaymentStatus
    {
        Pending,
        Ready,
        Paid,
        TaxedMovement,
        Confirmed,
        Failed,
        Other
    }

    public enum CartStatus
    {
        Construction,
        Pending,
        Execution,
        Confirmed,
        Failed,
        Canceled,
        Other,
    }

    public enum PaymentMethodType
    {
        Cash = 404,
        Card = 407
    }

    public enum ServiceStatus
    {
        Active,
        Inactive
    }

    public enum ServiceMode
    {
        Payment,
        Collection
    }

    public class ResponseBody
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public object Data { get; set; }
    }

    public class Field
    {
        public enum FieldType { Text, Numeric, Date, Boolean, Select, Multiselect, Description, Currency, Expiration, Amount, Option, Info, Document, Unknown }
        public long ID { get; set; }
        public string Name { get; set; }
        public string DisplayText { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public FieldType Type { get; set; }
        public string Value { get; set; }
    }
    public class FieldDetail : Field
    {
        public int Length { get; set; }
        public string HelpText { get; set; }
        public bool Editable { get; set; }
        public bool Mandatory { get; set; }
        public List<Option> Options { get; set; }
        public long[] SelectedOptionsID { get; set; }
    }

    public class Option
    {
        public long OptionID { get; set; }
        public List<Field> Fields { get; set; }
    }

    public class Bill
    {
        public long BillID { get; set; }
        public Service Service { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentStatus Status { get; set; }
        public string Reference { get; set; }
        public DateTime? Expiration { get; set; }
        public double Amount { get; set; }
        public long CurrencyID { get; set; }
        public short StepCurrent { get; set; }
        public int StepLast { get; set; }
        public List<FieldDetail> InputFieldDetails { get; set; }
        public List<FieldDetail> OutputFieldDetails { get; set; }
        public List<Field> CustomInfo { get; set; }
        public Dictionary<PropKey, object> CustomProps { get; set; }

    }

    public class Service
    {
        [JsonProperty(PropertyName = "id")]
        public long? ServiceID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public ServiceMode ServiceMode { get; set; }
             
        //public ServiceStatus? Status { get; set; }   
        public List<Field> CustomInfo { get; set; }

        [JsonProperty(PropertyName = "isEnabled")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "inactivityReason")]
        public string InactivityReason { get; set; }
    }

    public class Cart
    {
        public long CartID { get; set; }
        public List<Bill> Bills { get; set; }
        public DateTime? Date { get; set; }
        public List<Field> CustomInfo { get; set; }
        public List<Total> Total { get; set; }
        public bool SingleCartItem { get; set; }
    }

    public class Total
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
    //public class Currency
    //{
    //    public long CurrencyID { get; set;}
    //    public string CurrencyName { get; set; }
    //    public string Symbol { get; set; }
    //}

    public class PaymentMethod
    {
        public long PaymentMethodID { get; set; }
        public PaymentMethodType Type { get; set; }
    }


    /*to request a payment*/
    public class Payments : List<Payment> { }
    public class Payment
    {
        public long? CartID { get; set; } //not in use yet
        public long? PaymentMethodID { get; set; }
        public List<long> BillsIDs { get; set; }

        public List<Bill> BillList { get; set; }
        public Total Total { get; set; }
    }


    public class PaymentReceipt
    {
        public string Summary { get; set; }

        public string ElectronicInvoice { get; set; }

        public List<string> Details { get; set; }
    }
    
}
