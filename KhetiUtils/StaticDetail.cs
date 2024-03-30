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
        public const string OrderStatusShipped = "Shipped";  
        
        //booking status
        public const string BookingStatusPending = "Pending";        
        public const string BookingStatusApproved = "Approved";        
        public const string BookingStatusConfirmed = "Confirmed";
        public const string BookingStatusCompleted = "Completed";

        //rent status
        public const string RentStatusPending = "Pending";
        public const string RentStatusInProcess = "In Process";
        public const string RentStatusReturned = "Returned";
        public const string RentStatusCompleted = "Completed";


        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusCompleted = "Completed";

        public const string PaymentStatusPartialPaid = "Partial Payment Completed";
       
        //Query Status
        public const string QueryStatusPending = "Pending";
        public const string QueryStatusInProcess = "In Process";
        public const string QueryStatusSolved = "Solved";      
    }
}
