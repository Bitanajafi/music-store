using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Enum
{
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Cancelled = 3,
        Refunded = 4
    }
}
