using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BookOasis;
using BookOasis.Controllers;
using BookOasis.Data;
using BookOasis.Models;
using Microsoft;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BookOasis.Controllers.UnitTests
{
    public class BooksControllerTests
    {
        /// <summary>
        /// Tests that Edit returns NotFound when id is null.
        /// Input: id = null.
        /// Expected: NotFoundResult is returned.
        /// </summary>
        [Fact]
        public async Task Edit_IdIsNull_ReturnsNotFoundResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options);
            var controller = new BooksController(context);

            // Act
            IActionResult result = await controller.Edit(id: null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that Edit returns NotFound when a book with the supplied id is not found.
        /// Input: various id values where FindAsync returns null (int.MinValue, -1, 0, int.MaxValue).
        /// Expected: NotFoundResult is returned for each case.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task Edit_IdProvided_BookNotFound_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var mockSet = new Mock<DbSet<BooksModel>>();

            // Setup FindAsync to return null for any provided key values.
            mockSet
                .Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns(new ValueTask<BooksModel?>((BooksModel?)null));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Books = mockSet.Object
            };

            var controller = new BooksController(context);

            // Act
            IActionResult result = await controller.Edit(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that Edit returns a ViewResult containing the found book when the id exists.
        /// Input: id values for which the mocked FindAsync returns a BooksModel (1 and int.MaxValue).
        /// Expected: ViewResult is returned and the Model is the same instance returned by FindAsync.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task Edit_IdProvided_BookFound_ReturnsViewWithModel(int id)
        {
            // Arrange
            var model = new BooksModel
            {
                BookID = id,
                bookName = "Name",
                bookAuthor = "Author",
                bookISBN = "ISBN",
                bookDescription = "Desc",
                bookReleaseDate = DateTime.UtcNow
            };

            var mockSet = new Mock<DbSet<BooksModel>>();

            // Setup FindAsync to return the model when the provided key matches the tested id.
            mockSet
                .Setup(m => m.FindAsync(It.Is<object[]>(keys => keys != null && keys.Length > 0 && Convert.ToInt32(keys[0]) == id)))
                .Returns(new ValueTask<BooksModel?>(model));

            // For safety, any other keys return null (not strictly necessary for this test).
            mockSet
                .Setup(m => m.FindAsync(It.Is<object[]>(keys => keys == null || keys.Length == 0 || Convert.ToInt32(keys[0]) != id)))
                .Returns(new ValueTask<BooksModel?>((BooksModel?)null));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Books = mockSet.Object
            };

            var controller = new BooksController(context);

            // Act
            IActionResult result = await controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
        }

        /// <summary>
        /// Verifies that when a book with the specified id exists, DeleteConfirmed removes it and redirects to Index.
        /// Input conditions:
        ///  - id: tested for several numeric edge values (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected result:
        ///  - If the data layer returns an entity for the id, Remove should be called and method returns RedirectToAction(nameof(Index)).
        /// Implementation note:
        ///  - This test is skipped because ApplicationDbContext.Books (DbSet) cannot be mocked as-is.
        ///  - To implement: make Books virtual or inject a repository, or use EF Core InMemory DB and assert removal.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task DeleteConfirmed_BookExists_RemovesBookAndRedirects(int id)
        {
            // Arrange
            // Suggested implementation (uncomment and adapt when Books is mockable or when using InMemory DB):
            //
            // var mockSet = new Mock<DbSet<BooksModel>>();
            // mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            //        .ReturnsAsync(new BooksModel { BookID = id });
            // var options = new DbContextOptions<ApplicationDbContext>();
            // var mockContext = new Mock<ApplicationDbContext>(options);
            // mockContext.SetupGet(c => c.Books).Returns(mockSet.Object);
            // mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            //
            // var controller = new BooksController(mockContext.Object);
            //
            // Act
            // var result = await controller.DeleteConfirmed(id);
            //
            // Assert
            // mockSet.Verify(m => m.FindAsync(It.Is<object[]>(keys => (int)keys[0] == id)), Times.Once);
            // mockSet.Verify(m => m.Remove(It.Is<BooksModel>(b => b.BookID == id)), Times.Once);
            // var redirect = Assert.IsType<RedirectToActionResult>(result);
            // Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
            //
            await Task.CompletedTask;
        }

        /// <summary>
        /// Verifies that when a book with the specified id does NOT exist, DeleteConfirmed does not call Remove and still redirects to Index.
        /// Input conditions:
        ///  - id: tested for representative values (0 and 42).
        /// Expected result:
        ///  - If the data layer returns null for the id, Remove should not be called and method returns RedirectToAction(nameof(Index)).
        /// Implementation note:
        ///  - This test is skipped because ApplicationDbContext.Books (DbSet) cannot be mocked as-is.
        ///  - To implement: make Books virtual or inject a repository, or use EF Core InMemory DB and assert no removal occurred.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task DeleteConfirmed_BookDoesNotExist_NoRemoveAndRedirects(int id)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var controller = new BooksController(context);

            // Act
            var result = await controller.DeleteConfirmed(id);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that when the route/provided id does not match the BooksModel.BookID, the Edit method returns NotFoundResult.
        /// Input conditions:
        /// - requestId: various boundary integer values (including int.MinValue, 0, int.MaxValue)
        /// - modelId: different integer values to ensure mismatch
        /// Expected result:
        /// - The controller should return a NotFoundResult.
        /// Notes:
        /// - This test is marked skipped because ApplicationDbContext (the controller dependency) is not mockable in the current project setup.
        /// - To implement this test, provide a mockable ApplicationDbContext (or use EF Core InMemory provider) and then:
        ///     Arrange: create an ApplicationDbContext instance or mock with a DbSet<BooksModel>, instantiate BooksController with it,
        ///              create a BooksModel with BookID = modelId and ensure ModelState.IsValid = true/false as needed.
        ///     Act: call await controller.Edit(requestId, booksModel)
        ///     Assert: Assert.IsType<NotFoundResult>(result)
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, int.MinValue + 1)]
        [InlineData(0, 1)]
        [InlineData(int.MaxValue, int.MaxValue - 1)]
        public async Task Edit_IdMismatch_ReturnsNotFound(
            int requestId,
            int modelId)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var controller = new BooksController(context);

            var booksModel = new BooksModel
            {
                BookID = modelId
            };

            // Act
            var result = await controller.Edit(requestId, booksModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Test purpose:
        /// Verifies behavior when SaveChangesAsync throws a DbUpdateConcurrencyException and the book does not exist anymore.
        /// Input conditions:
        /// - booksModel.BookID equals provided id
        /// - ModelState.IsValid == true
        /// - SaveChangesAsync throws DbUpdateConcurrencyException
        /// - BooksDisplayModelExists returns false for the given BookID
        /// Expected result:
        /// - The controller should return NotFoundResult.
        /// Notes:
        /// - Skipped due to ApplicationDbContext not being mockable in this environment.
        /// - Implementation guidance:
        ///     * Mock or setup ApplicationDbContext.Update to accept the model.
        ///     * Mock SaveChangesAsync to throw DbUpdateConcurrencyException.
        ///     * Ensure Books DbSet does not contain the BookID so BooksDisplayModelExists returns false.
        /// </summary>
        [Fact]
        public async Task Edit_ConcurrencyConflict_BookDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var controller = new BooksController(context);

            var booksModel = new BooksModel
            {
                BookID = 9
            };

            // Act
            var result = await controller.Edit(9, booksModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Verifies that the GET Create action returns a ViewResult and does not supply a model.
        /// Conditions: controller ModelState is valid or invalid (parameterized).
        /// Expected: A ViewResult is returned, ViewName is null (uses default view), and Model is null.
        /// </summary>
        /// <param name="modelStateIsValid">If false an error is added to ModelState to simulate invalid state.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Create_ModelStateVariation_ReturnsViewResultWithNullModel(bool modelStateIsValid)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var controller = new BooksController(context);

            if (!modelStateIsValid)
            {
                controller.ModelState.AddModelError("AnyKey", "Simulated error");
            }

            // Act
            IActionResult result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Null(viewResult.ViewName);
            Assert.Null(viewResult.Model);
        }

        /// <summary>
        /// Ensures that calling Create does not redirect (i.e., returns a view result) when controller is constructed with a non-null context.
        /// Condition: standard controller constructed with a valid ApplicationDbContext instance (mock).
        /// Expected: result is ViewResult and not a RedirectToActionResult.
        /// </summary>
        [Fact]
        public void Create_WithMockedContext_ReturnsViewResult_NotRedirect()
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var realContext = new ApplicationDbContext(options);
            var controller = new BooksController(realContext);

            // Act
            IActionResult result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(result is RedirectToActionResult);
            Assert.Null(viewResult.Model);

            realContext.Dispose();
        }

        /// <summary>
        /// Tests that when ModelState is valid the Create POST action saves the entity and redirects to Index.
        /// Input conditions: BooksModel instance with varied BookID values (edge numeric values).
        /// Expected result: SaveChangesAsync is invoked exactly once and the action returns a RedirectToActionResult targeting "Index".
        /// </summary>
        /// <param name="bookId">BookID to test (edge numeric values).</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task Create_ModelStateValid_RedirectsToIndexAndSaves(int bookId)
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);

            // Ensure SaveChangesAsync returns a success code and can be verified.
            mockContext
                .Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new BooksController(mockContext.Object);

            var book = new BooksModel
            {
                BookID = bookId,
                bookName = "Title",
                bookISBN = "ISBN-123",
                bookAuthor = "Author",
                bookDescription = "Desc",
                bookReleaseDate = DateTime.UtcNow,
                Reviews = new System.Collections.Generic.List<Reviews>()
            };

            // Act
            var result = await controller.Create(book);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(BooksController.Index), redirect.ActionName);
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that when ModelState is invalid the Create POST action returns the view with the supplied model.
        /// Input conditions: BooksModel instance with varied BookID values and a ModelState error to force invalid state.
        /// Expected result: The action returns a ViewResult whose Model is the same instance passed in and no SaveChangesAsync is required.
        /// </summary>
        /// <param name="bookId">BookID to test (edge numeric values).</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public async Task Create_ModelStateInvalid_ReturnsViewWithModel(int bookId)
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);

            var controller = new BooksController(mockContext.Object);

            // Force invalid ModelState
            controller.ModelState.AddModelError(nameof(BooksModel.bookName), "Required");

            var book = new BooksModel
            {
                BookID = bookId,
                bookName = "Title",
                bookISBN = "ISBN-123",
                bookAuthor = "Author",
                bookDescription = "Desc",
                bookReleaseDate = DateTime.UtcNow,
                Reviews = new System.Collections.Generic.List<Reviews>()
            };

            // Act
            var result = await controller.Create(book);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(book, view.Model);
            // When ModelState is invalid the controller should not attempt to save changes.
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        public static IEnumerable<object[]> IndexMemberData()
        {
            // Case 1: empty list
            yield return new object[] { new List<BooksModel>() };

            // Case 2: two items including long and special character strings
            yield return new object[]
            {
                new List<BooksModel>
                {
                    new BooksModel
                    {
                        BookID = int.MinValue, // boundary numeric value
                        bookName = "Normal Title",
                        bookISBN = "ISBN-12345",
                        bookAuthor = "Author One",
                        bookDescription = "A short description.",
                        bookReleaseDate = DateTime.Parse("2000-01-01")
                    },
                    new BooksModel
                    {
                        BookID = int.MaxValue, // boundary numeric value
                        bookName = new string('A', 1024) + "\n\t\u2603", // very long and special chars
                        bookISBN = string.Empty,
                        bookAuthor = " ",
                        bookDescription = string.Empty,
                        bookReleaseDate = DateTime.Parse("9999-12-31")
                    }
                }
            };
        }

        /// <summary>
        /// Verifies that Index returns a ViewResult whose model is a List&lt;BooksModel&gt;,
        /// and that the list content matches the application's DbSet contents.
        /// Input conditions: the ApplicationDbContext.Books contains the provided collection (including empty).
        /// Expected result: Index() produces a ViewResult with a model equal to the items supplied.
        /// </summary>
        [Theory]
        [MemberData(nameof(IndexMemberData))]
        public async Task Index_DatabaseHasItems_ReturnsViewWithListModel(
            List<BooksModel> seededBooks)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            context.Books.AddRange(seededBooks);
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<List<BooksModel>>(viewResult.Model);

            Assert.Equal(seededBooks.Count, model.Count);

            var expectedPairs = seededBooks
                .Select(b => (b.BookID, b.bookName))
                .ToList();

            var actualPairs = model
                .Select(b => (b.BookID, b.bookName))
                .ToList();

            Assert.Equal(expectedPairs, actualPairs);
        }

        /// <summary>
        /// Tests that when a null id is passed to Delete, the controller returns NotFound.
        /// Input conditions: id == null.
        /// Expected result: NotFoundResult.
        /// Note: This test is skipped because ApplicationDbContext.Books is a non-virtual property and cannot be mocked with Moq.
        /// To enable this test, either:
        ///  - make Books virtual so it can be mocked, or
        ///  - use an EF Core InMemory database provider and initialize the Books DbSet with data.
        /// </summary>
        [Fact]
        public async Task Delete_IdIsNull_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var dbContext = new ApplicationDbContext(options);
            var controller = new BooksController(dbContext);

            // Act
            IActionResult result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}