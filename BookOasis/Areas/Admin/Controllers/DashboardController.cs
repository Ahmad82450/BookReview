using BookOasis.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookOasis.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["TotalBooks"] = await _context.Books.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalReviews"] = await _context.Reviews.CountAsync();
            return View();
        }
    }
}