using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities.Devices
{
    public class CashAcceptor
    {
        public CashAcceptor() { }
        public T GetCommand<T>(string cmd)
        {
            return Utilities.Utils.StringToEnum<T>(cmd);
        }
        public enum Commands
        {
            CashInStart,
            CashIn,
            CashInEnd,
            CashInRollback,
            Retract,
            OpenShutter,
            CloseShutter,
            Reset,
            ConfigureNotetypes,
            ReadRawData
        }
    }
}
