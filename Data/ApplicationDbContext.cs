using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pendlerapp.Models;

namespace Pendlerapp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Favoritt> Favoritter { get; set; }
        public DbSet<Reisehistorikk> Reisehistorikker { get; set; }
    }
}