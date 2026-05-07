using BookOasis.Data;
using BookOasis.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace BookOasis.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reviews
        public ActionResult Index()
        {
            var reviews = _context.Reviews.ToList();
            return View(reviews);
        }

        // GET: Reviews/Details/5
        public ActionResult Details(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null)
                return NotFound();

            return View(review);
        }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reviews review)
        {

            if (ModelState.IsValid)
            {
                review.ReviewTimeStamp = DateTime.Now;

                if (User.Identity.IsAuthenticated)
                {
                    ClaimsPrincipal currentUser = this.User;
                    var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        review.UserID = userId;
                    }
                }

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

            }

            else
            {
                Console.WriteLine("Model state is invalid. Errors:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
            }

            return RedirectToAction("Details", "Books", new { id = review.BookID });
        }

        // GET: Reviews/Edit/5
        public IActionResult Edit(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null)
                return NotFound();

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Reviews review)
        {
            if (id != review.ReviewID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(review);
                _context.SaveChanges();

                return RedirectToAction("Details", "Books", new { id = review.BookID });
            }

            return View(review);
        }

        // GET: Reviews/Delete/5
        public ActionResult Delete(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null)
                return NotFound();

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Reviews review)
        {
            var existingReview = _context.Reviews.FirstOrDefault(r => r.ReviewID == id);
            if (existingReview != null)
            {
                int bookId = existingReview.BookID;

                _context.Reviews.Remove(existingReview);
                _context.SaveChanges();

                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}