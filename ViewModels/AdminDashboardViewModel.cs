using Kheti.Models;

namespace Kheti.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalQueries { get; set; }
        public int TotalProducts { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<QueryForm> RecentQueries { get; set; }  
        public List<Category> PopularCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DateTime> RevenueDates { get; set; }
        public List<decimal> RevenueAmounts { get; set; }
        public List<OrderItem> LastSoldProducts { get; set; }

        // Total number of customers, sellers, and experts
        public int TotalCustomers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalExperts { get; set; }

        // Total number of crops, fertilizers, and machineries
        public int TotalCropProduct { get; set; }
        public int TotalFertilizer { get; set; }
        public int TotalMachinery { get; set; }

        public int TotalBookings { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalRentalRevenue { get; set; }

    }
}
