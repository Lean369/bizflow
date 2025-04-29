using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace Business
{
    public class AuthorizationResult
    {
        internal AuthorizationStatus authorizationStatus;
        internal string tnum;
        public AuthorizationResult(AuthorizationStatus result, string transactionNumber)
        {
            authorizationStatus = result;
            tnum = transactionNumber;
        }
    }


    #region "Enums"
    public enum AuthorizationStatus
    {
        NotAuthorized = 0,
        HostTimeout = 1,
        CommsError = 2,
        Authorized = 3,
        IncorrectPIN = 4,
        InsufficientFunds = 5,
        InvalidAccount = 6,
        InvalidDestinationAccount = 7,
        InvalidAmount = 8,
        LimitExceeded = 9,
        Declined = 10,
        MultipleAccounts = 11,
        MultipleDestinationAccounts = 12,
        HostInfoRequest = 13,
        InvalidNewPIN = 14,
        InvalidPayeeInfo = 15,
        CardRestricted = 16,
        CardExpired = 17,
        CardInvalid = 18,
        DeclinedEndSession = 19
    }
    //
    // Resumen:
    //     Indicates the type of debit transaction being done: or where the funds debited
    //     from customer's account are going.
    public enum DebitType
    {
        CashWithdrawal = 0,
        MobileTopup = 1,
        EPurseTopup = 2,
        VoucherPurchase = 3
    }
    //
    // Resumen:
    //     Indicates the type of credit transaction being done, or where funds being credited
    //     to customer's account originate from.
    public enum CreditType
    {
        CashDeposit = 0,
        EnvelopeDeposit = 1,
        CheckDeposit = 2,
        EPurseDeposit = 3,
        BagDropDeposit = 4
    }
    //
    // Resumen:
    //     Indicates the type of transfer transaction being performed
    public enum TransferType
    {
        AccountTransfer = 0
    }
    //
    // Resumen:
    //     Indicates the type of payment transaction being performed, or how payment is
    //     being made.
    public enum PaymentType
    {
        PaymentFromAccount = 0,
        PaymentByCash = 1,
        PaymentByEnvelopeDeposit = 2,
        PaymentByCheckDeposit = 3,
        PaymentByEPurse = 4,
        PaymentForMobileTopup = 5,
        ExternalTransfer = 6
    }
    //
    // Resumen:
    //     Indicates the type of inquiry transaction being performed.
    public enum InquiryType
    {
        Media = 0,
        Account = 1,
        Balance = 2,
        Statement = 3,
        Surcharge = 4,
        General = 5,
        PayeeList = 6,
        CheckRequest = 7,
        PaymentAmount = 8,
        ProviderList = 9,
        BankList = 10,
        PaymentList = 11,
        StatementRequest = 12,
        PassbookRequest = 13,
        PinValidationData = 14,
        Admin = 15
    }

    #endregion "Enums"

}
