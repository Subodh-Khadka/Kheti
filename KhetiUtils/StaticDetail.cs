namespace Kheti.KhetiUtils
{
    public class StaticDetail
    {
        //Role status
        public const string CustomerRole = "Customer";
        public const string SellerRole = "Seller";
        public const string AdminRole = "Admin";
        public const string ExpertRole = "Expert";

        //OrderStatus
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProcess = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefund = "Refunded";

        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";

        //Query Status
        public const string QueryStatusPending = "Pending";
        public const string QueryStatusInProcess = "In Process";
        public const string QueryStatusSolved = "Solved";      
    }
}
