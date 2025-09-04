using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Utility
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee User";
        public const string Role_Company = "Company User";
        public const string Role_Indvidual = "Indvidual User";
        // Order Status---------------
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgsess = "Progcessing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCncelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";
        // Payment Status-------------
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayed = "PaymentStatusDelas";
        public const string PaymentStatusRejected = "Rejected";
        //Session---------------------
        public const string Ss_CartSessionCount = "Cart Count Session";
        //
        public static double GetPriceBasedOnQuantity(double quantity,double price,double price50,double price100)
        {
            if (quantity <50)
                return price;
            if (quantity <100) return price50;
            else return price100;
        }
    }
}
