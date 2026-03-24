using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookOasis.Models;

namespace BookOasis.Books
{
    public class BookOasisContext : DbContext
    {
        public BookOasisContext (DbContextOptions<BookOasisContext> options)
            : base(options)
        {
        }

        public DbSet<BookOasis.Models.BooksDisplayModel> BooksDisplayModel { get; set; } = default!;
    }
}
