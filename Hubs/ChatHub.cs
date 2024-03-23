using Microsoft.AspNetCore.SignalR;
using Kheti.Data;
using Kheti.Models;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Kheti.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }

        public async Task SendMessage(string user, string message, int queryFormId)
        {            
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isExpert = IsUserExpert(userId); 

            var query = _db.QueryForms
                .Include(q => q.QueryComments)
                .Include(q => q.User)
                .FirstOrDefault(q => q.Id == queryFormId);               

            var newComment = new QueryComment
            {
                CommentText = message,
                DateCreated = DateTime.Now,
                QueryFormId = queryFormId,
                UserId = userId,
                IsExpert = isExpert
            };

            query.QueryComments.Add(newComment);
            await _db.SaveChangesAsync();
            
            await Clients.All.SendAsync("ReceiveMessage", user, message, isExpert);
        }
        private bool IsUserExpert(string userId)
        {            
            return _db.ExpertProfiles.Any(ep => ep.UserId == userId);
        }

        //forBookingRequest
        public async Task SendMessageForBooking(string user, string message, Guid bookingId, string userRole)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isSeller = userRole == "Seller";


            var booking = _db.Bookings
                .Include(q => q.BookingComments)
                .Include(q => q.User)
                .FirstOrDefault(q => q.BookingId == bookingId);

            var newComment = new BookingComments
            {
                CommentText = message,
                DateCreated = DateTime.Now,
                BookingId = bookingId,
                UserId = userId,
                IsSeller = isSeller,
            };

            booking.BookingComments.Add(newComment);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user, message, isSeller);
        }
    }
}
