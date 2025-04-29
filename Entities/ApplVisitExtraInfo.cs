using System;
using System.Collections.Generic;
using static Entities.Enums;
//using System.Threading.Tasks;

namespace Entities
{
    public class ApplVisitExtraInfo
    {
        public AuthorizationStatus AuthorizationStatus { get; set; }
        public string PendingUserNotification { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountLimit { get; set; }
        public decimal AmountToDispenseInNotes { get; set; }
        public decimal AmountToDispenseInCoins { get; set; }
        public string Barcode { get; set; }
        public string BarcodeASCII { get; set; }
        public string QRdata { get; set; }
        public string ChannelA{ get; set; }
        public string FingerPrint { get; set; }
        public string NewPinBlock { get; set; }
        public string NewPinBlockConfirmation { get; set; }
        public string NewTrack1 { get; set; }
        public string NewTrack2 { get; set; }
        public string NewTrack3 { get; set; }
        public string OperationCodeData { get; set; }
        public string VirtualTrack2 { get; set; }
        public string BufferB { get; set; }
        public string BufferC { get; set; }
        public string AccountHolder { get; set; }
        public string DepositorAccount { get; set; }
        public bool ReceiptRequired { get; set; }
        public string ReceiptTicketType { get; set; }
        public List<StateEvent> LstPrintData { get; set; } //Guarda los datos de impresión
        public object TableItemSelected { get; set; }
        public NDCData ndcData { get; set; } //Datos almacenados en standard NDC

        //Guarda las credenciales de acceso del usuario en curso
        public UserProfile_Type UserProfileMain { get; set; } //Usuario principar para realizar transacciones

        public UserProfile_Type UserProfileAlt { get; set; } //Usuario que realiza operaciones en nombre de otro usuario

        ///// <summary>
        ///// Guarda los datos extra de host
        ///// </summary>
        public Dictionary<string, object> HostExtraData { get; set; }

        public List<AvTxn> AvailableTxns { get; set; }
        public AvTxn CurrentTxn { get; set; }
        public List<ExtraData> ExtraData { get; set; }

        public List<ExtraData> PaymentData { get; set; }

        public List<ExtraData> PaymentAmounts { get; set; }

        public ErrorData ErrorCode { get; set; }

        public List<ExtraDataConf> SetExtraDataFields { get; set; }

        public SetExtraDataInfo SetExtraDataInfo { get; set; }

        public string CollectionID { get; set; }

        //Guarda una lista con el detalle de los sobres depositados
        public BagDropInfo BagDropInfo { get; set; }

        //Guarda las cantidades de billetes acumuladas en cada lectura parcial (solo se utiliza para mostrar la info por pantalla)
        public CashInInfo CashInInfo
        {
            get;
            set;
        }

        //Guarda las cantidades de billetes de cada depósito (que hayan pasado al tesoro)
        public CashInMultiCashData CashInMultiCashData { get; set; }

        /// <summary>
        /// Se ejecuta en el InitVisit
        /// </summary>
        public ApplVisitExtraInfo()
        {
            this.ndcData = new NDCData();
            this.HostExtraData = new Dictionary<string, object>();
            this.CashInMultiCashData = new CashInMultiCashData(); //Datos relacionados con la operación de depósitos. Contiene todos los depósitos confirmados.
            this.OperationCodeData = "        ";
            this.AmountLimit = 0;
            this.AmountToDispenseInNotes = 0;
            this.AmountToDispenseInNotes = 0;
            this.LstPrintData = new List<StateEvent>();
        }

        //Utilizada en pago de servicios, para saber si debe dirigirse a pantalla de recarga.
        public bool IsMobileTopup { get; set; }

        public int ShoppingCartItems { get; set; }

        public decimal GetTotalAmount(string currency)
        {
            decimal total = 0;
            try 
            {
                foreach (Bills b in this.CashInInfo.Bills)
                {
                    if (currency.Equals(b.Currency))
                    {
                        total += b.Quantity * b.Value;
                    }
                }
                return total;
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
