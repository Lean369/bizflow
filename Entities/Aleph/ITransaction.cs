using System;

namespace Entities
{
    public interface ITransaction
    {
        long MACHINE_ID { get; set; }
        string USER_ID { get; set; }
        string USER_NAME { get; set; }
        DateTime CREATED { get; set; }
    }
}
