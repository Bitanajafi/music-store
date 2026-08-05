using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Enum
{
    public enum OrderStatus
    {
        Pending = 0,       // سفارش ثبت شده، منتظر پرداخت
        Paid = 1,          // پرداخت موفق
        Processing = 2,    // در حال آماده سازی
        Shipped = 3,       // ارسال شده
        Delivered = 4,     // تحویل داده شده
        Cancelled = 5      // لغو شده
    }
}
