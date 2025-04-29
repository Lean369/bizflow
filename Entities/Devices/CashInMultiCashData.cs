using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class CashInMultiCashData
    {
        [DataMember]
        public List<CashInInfo> ListCashInInfo;//Guarda el detalle de todos los depósitos parciales discriminados por denominaciones

        [DataMember]
        public RecognizedAmount TotalizedDeposit;//Guarda el monto totalizado de todos los depósitos parciales para cada moneda

        [DataMember]
        public RecognizedAmount ListPartialDeposit;//Guarda el monto total de cada uno de los depósitos parciales

        [DataMember]
        public bool DepositHardwareError;

        [DataMember]
        public bool DepositEscrowFull;

        public CashInMultiCashData()
        {
            this.ListCashInInfo = new List<CashInInfo>();
            this.TotalizedDeposit = new RecognizedAmount();
            this.ListPartialDeposit = new RecognizedAmount();
        }

        //public decimal GetTotalAmountDeposited(string currency)
        //{
        //    decimal ret = 0;
        //    if(this.TotalizedDeposit.total.Count > 0)
        //    {
        //        var a  = this.GetTotalizedAmount(this.ListCashInInfo);
        //        Values item = this.TotalizedDeposit.total.Find(e => e.currency.Equals(currency));
        //        ret = Utilities.Utils.GetDecimalAmount(item.amount);
        //    }
        //    return ret;
        //}

        public void UpdateMultiCashData(CashInInfo cashInInfo)
        {
            this.ListCashInInfo.Add(cashInInfo);
            this.ListPartialDeposit.total.AddRange(this.GetPartialAmount(cashInInfo));
            this.TotalizedDeposit = this.GetTotalizedAmount(this.ListCashInInfo);
        }

        private List<Values> GetPartialAmount(CashInInfo cashInInfo)
        {
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            decimal total = 0;
            List<Values> lstValues = new List<Values>();
            try
            {
                foreach (Bills b in cashInInfo.Bills)
                {
                    currencies.Add(b.Currency);//Cargo todos los currencies
                }
                distinctCurrencies = currencies.Distinct().ToArray();
                for (int k = 0; k < distinctCurrencies.Length; k++)
                {
                    foreach (Bills b in cashInInfo.Bills)
                    {
                        if (distinctCurrencies[k].Equals(b.Currency))
                        {
                            total += b.Quantity * b.Value;
                        }
                    }
                    //lstValues.Add(new Values(distinctCurrencies[k], Utilities.Utils.FormatMonetaryAmount("", total.ToString(), 12, false)));
                    string currency = distinctCurrencies[k];
                    lstValues.Add(new Values(currency, total.ToString()));
                    total = 0;
                }
                return lstValues;
            }
            catch (Exception ex) { throw ex; }
        }

        private RecognizedAmount GetTotalizedAmount(List<CashInInfo> listCashInInfo)
        {
            List<RecognizedAmount> lstRecognizedAmount = new List<RecognizedAmount>();
            RecognizedAmount recognizedAmount = new RecognizedAmount();
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            decimal partial = 0;

            try
            {
                foreach (CashInInfo ci in listCashInInfo)
                {
                    foreach (Bills b in ci.Bills)
                    {
                        currencies.Add(b.Currency);//Cargo todos los currencies
                    }
                }
                distinctCurrencies = currencies.Distinct().ToArray();
                for (int k = 0; k < distinctCurrencies.Length; k++)//Guardo un total por cada currency
                {
                    foreach (CashInInfo ci in listCashInInfo)
                    {
                        foreach (Bills b in ci.Bills)
                        {
                            if (distinctCurrencies[k].Equals(b.Currency))
                            {
                                partial += b.Quantity * b.Value;
                            }
                        }
                    }
                    string currency = distinctCurrencies[k];
                    recognizedAmount.total.Add(new Values(currency, Utilities.Utils.FormatCurrency(partial, currency, 12)));
                    partial = 0;
                }
                return recognizedAmount;
            }
            catch (Exception ex) { throw ex; }
        }

        public RecognizedAmount GetRecognizedAmount(CashInInfo cashInInfo)
        {
            List<RecognizedAmount> lstRecognizedAmount = new List<RecognizedAmount>();
            RecognizedAmount recognizedAmount = new RecognizedAmount();
            List<string> currencies = new List<string>();
            string[] distinctCurrencies;
            decimal partial = 0;
            try
            {
                if(cashInInfo != null)
                {
                    foreach (Bills b in cashInInfo.Bills)
                    {
                        currencies.Add(b.Currency);//Cargo todos los currencies
                    }
                }
                foreach (Values v in this.TotalizedDeposit.total)
                {
                    currencies.Add(v.currency);//Cargo todos los currencies
                }
                distinctCurrencies = currencies.Distinct().ToArray();
                for (int k = 0; k < distinctCurrencies.Length; k++)//Guardo un total por cada currency
                {
                    //Sumo los valores reconocidos
                    if (cashInInfo != null)
                    {
                        foreach (Bills b in cashInInfo.Bills)
                        {
                            if (distinctCurrencies[k].Equals(b.Currency))
                            {
                                partial += b.Quantity * b.Value;
                            }
                        }
                    }
                    //Agrego los valores ya depositados
                    foreach (Values v in this.TotalizedDeposit.total)
                    {
                        if (distinctCurrencies[k].Equals(v.currency))
                        {
                            partial += Utilities.Utils.GetDecimalAmount(v.amount) / 100;
                        }
                    }
                    string currency = distinctCurrencies[k];
                    recognizedAmount.total.Add(new Values(currency, partial.ToString()));
                    partial = 0;
                }
                return recognizedAmount;
            }
            catch (Exception ex) { throw ex; }
        }

    }
}

