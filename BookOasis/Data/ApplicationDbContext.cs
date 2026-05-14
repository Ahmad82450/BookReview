using BookOasis.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookOasis.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {

        }
            
        public virtual DbSet<BookOasis.Models.BooksModel> Books { get; set; }

        public virtual DbSet<BookOasis.Models.Reviews> Reviews { get; set; }
    }
}
