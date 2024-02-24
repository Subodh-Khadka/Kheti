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
        private readonly ApplicationDbContext _dbContext;

        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessage(string user, string message, int queryFormId)
        {            
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isExpert = IsUserExpert(userId); 

            var query = _dbContext.QueryForms
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
            await _dbContext.SaveChangesAsync();
            
            await Clients.All.SendAsync("ReceiveMessage", user, message, isExpert);
        }
        private bool IsUserExpert(string userId)
        {            
            return _dbContext.ExpertProfiles.Any(ep => ep.UserId == userId);
        }
    }
}
