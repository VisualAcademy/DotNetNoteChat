using DotNetNote.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotNetNote.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Chat> Chats { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
