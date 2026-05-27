using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookOasis.Models;
using BookOasis.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookOasis.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booksDisplayModel = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (booksDisplayModel == null)
            {
                return NotFound();
            }

            var book = _context.Books
                .Include(b => b.Reviews)
                .FirstOrDefault(b => b.BookID == id);

            return View(booksDisplayModel);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("BookID,bookName,bookISBN,bookAuthor,bookDescription,bookReleaseDate")] BooksModel booksDisplayModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booksDisplayModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booksDisplayModel);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booksDisplayModel = await _context.Books.FindAsync(id);
            if (booksDisplayModel == null)
            {
                return NotFound();
            }
            return View(booksDisplayModel);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("BookID,bookName,bookISBN,bookAuthor,bookDescription,bookReleaseDate")] BooksModel booksDisplayModel)
        {
            if (id != booksDisplayModel.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booksDisplayModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BooksDisplayModelExists(booksDisplayModel.BookID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booksDisplayModel);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booksDisplayModel = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (booksDisplayModel == null)
            {
                return NotFound();
            }

            return View(booksDisplayModel);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booksDisplayModel = await _context.Books.FindAsync(id);
            if (booksDisplayModel != null)
            {
                _context.Books.Remove(booksDisplayModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BooksDisplayModelExists(int id)
        {
            return _context.Books.Any(e => e.BookID == id);
        }
    }
}
