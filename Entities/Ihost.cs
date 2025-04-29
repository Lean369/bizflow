using Entities.PaymentService;
using System.Collections.Generic;

namespace Entities
{
    public delegate void DelegateFindUserByLogin(UserProfile_Type userProfileData);

    public delegate void DelegateGetHostData(string msg);
    public interface Ihost
    {
        event DelegateFindUserByLogin EvtFindUserByLogin;

        event DelegateGetHostData EvtGetHostData;
        AuthorizationResult CashDeposit(int batchNumber, Enums.TransactionType transactionType, string transactionNumber, Contents contents, Dictionary<string, object> hostExtraData);
        AuthorizationResult CashDispense(int batchNumber, Enums.TransactionType transactionType, string transactionNumber, Contents contents, decimal amount, Dictionary<string, object> hostExtraData);
        AuthorizationResult SendContents(Contents contents);
        AuthorizationResult SendGetContents(Contents contents);
        AuthorizationResult SendTicketData(string jsonData, int ticketID);
        AuthorizationResult SendDeviceError(List<Device> lstDevices);
        AuthorizationResult SendCollection(string secuence, Contents contents, int batchNumber, Dictionary<string, object> hostExtraData);
        AuthorizationResult SendCollectionDeclared(string secuence, Contents contents, int batchNumber, Dictionary<string, object> hostExtraData);
        AuthorizationResult GetAccounts(string pan);
        AuthorizationResult GetInvoices(string clientNumber);
        void FindUserByLogin(string userNumber);

        AuthorizationResult PaymentFindService(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentInit(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentProcess(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentAddToCart(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentGetCart(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentRemoveFromCart(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentEmptyCart();
        AuthorizationResult PaymentsExecute(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentSummary(Dictionary<PaymentService.PropKey, object> requestData);
        AuthorizationResult PaymentGetReceipt(Dictionary<PropKey, object> requestData);
        AuthorizationResult SendShipin(Contents contents, int batchNumber, Dictionary<string, object> hostExtraData);
        AuthorizationResult SendCashin(Contents contents, int batchNumber, Dictionary<string, object> hostExtraData);
        AuthorizationResult SendCashout(Contents contents, int batchNumber, Dictionary<string, object> hostExtraData);
    }
}
