using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;

namespace Business
{
    public interface Ihost
    {
        event DelegateFindUserByLogin EvtFindUserByLogin;
        AuthorizationResult CashDeposit(Enums.TransactionType transactionType, string TransactionNumber, Contents contents);
        AuthorizationResult SendContents();
        AuthorizationResult SendTicketData(string jsonData, int ticketID);
        AuthorizationResult SendDeviceError(List<Device> lstDevices);
        AuthorizationResult SendCollection(string secuence);
        AuthorizationResult SendCollectionDeclared(string secuence);
        void FindUserByLogin(string userNumber);
    }
}
