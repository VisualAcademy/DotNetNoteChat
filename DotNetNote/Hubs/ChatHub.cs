using DotNetNote.Data;
using DotNetNote.Models;
using DotNetNote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DotNetNote.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private IEmailManager _emailManager;
        private ApplicationDbContext _dbContext;
        private string _adminEamil;

        public ChatHub(IEmailManager emailManager, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _emailManager = emailManager;
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _adminEamil = configuration.GetSection("AdminEmail").Value;
        }

        public override async Task OnConnectedAsync()
        {
            // 관리자에게 메일 전송
            await _emailManager.SendEmailCodeAsync(_adminEamil, $"{Context.User.Identity.Name}님이 접속했습니다.", $"{Context.User.Identity.Name}님이 접속했습니다.<br />접속 시간: {DateTimeOffset.Now}");

            // DB 저장
            var chat = new Chat { Name = Context.User.Identity.Name, Message = "joined", Created = DateTimeOffset.Now };
            _dbContext.Chats.Add(chat);
            _dbContext.SaveChanges(); 

            // 메시지 전체 전송
            await Clients.All.SendAsync("SendAction", Context.User.Identity.Name, "joined");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // DB 저장
            var chat = new Chat { Name = Context.User.Identity.Name, Message = "left", Created = DateTimeOffset.Now };
            _dbContext.Chats.Add(chat);
            _dbContext.SaveChanges();

            await Clients.All.SendAsync("SendAction", Context.User.Identity.Name, "left");
        }

        public async Task Send(string message)
        {
            // DB 저장
            var chat = new Chat { Name = Context.User.Identity.Name, Message = message, Created = DateTimeOffset.Now };
            _dbContext.Chats.Add(chat);
            _dbContext.SaveChanges();

            await Clients.All.SendAsync("SendMessage", Context.User.Identity.Name, message);
        }
    }
}
