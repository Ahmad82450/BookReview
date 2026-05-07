using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using BookOasis;
using BookOasis.Controllers;
using BookOasis.Data;
using BookOasis.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Xunit;

namespace BookOasis.Controllers.UnitTests
{
    public class ReviewsControllerTests
    {
        /// <summary>
        /// Tests that Edit returns NotFound when no review exists with the supplied id.
        /// Input: various id values (including int.MinValue, negative, zero, int.MaxValue) that are not present in the data set.
        /// Expected: NotFoundResult is returned for each provided id.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Edit_IdProvided_ReviewNotFound_ReturnsNotFoundResult(int id)
        {
            // Arrange
            // Create a data set that does NOT contain the tested ids (only contains ReviewID = 1)
            var data = new List<Reviews>
            {
                new Reviews { ReviewID = 1, BookID = 10, ReviewText = "R1", ReviewRating = 5, UserID = "user1" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);
            mockContext.SetupGet(c => c.Reviews).Returns(mockSet.Object);

            var controller = new ReviewsController(mockContext.Object);

            // Act
            IActionResult result = controller.Edit(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that Edit returns a ViewResult containing the found review when the id exists.
        /// Input: id values that are present in the data set (1 and int.MaxValue).
        /// Expected: ViewResult is returned and the Model is the same Reviews instance that matches the id.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Edit_IdProvided_ReviewFound_ReturnsViewWithModel(int id)
        {
            // Arrange
            var r1 = new Reviews { ReviewID = 1, BookID = 10, ReviewText = "R1", ReviewRating = 5, UserID = "user1" };
            var rMax = new Reviews { ReviewID = int.MaxValue, BookID = 20, ReviewText = "RMax", ReviewRating = 3, UserID = "userMax" };

            var data = new List<Reviews> { r1, rMax }.AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);
            mockContext.SetupGet(c => c.Reviews).Returns(mockSet.Object);

            var controller = new ReviewsController(mockContext.Object);

            // Act
            IActionResult result = controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Reviews>(viewResult.Model);

            // Ensure the returned model is the exact instance from the mocked data when matching id.
            if (id == r1.ReviewID)
            {
                Assert.Same(r1, model);
            }
            else
            {
                Assert.Same(rMax, model);
            }
        }

        /// <summary>
        /// Tests that Create returns a ViewResult when a real ApplicationDbContext is provided.
        /// Input: a newly constructed ApplicationDbContext (with default options).
        /// Expected: a ViewResult is returned and its Model is null.
        /// </summary>
        [Fact]
        public void Create_NoParameters_WithRealContext_ReturnsViewResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            using var context = new ApplicationDbContext(options);
            var controller = new ReviewsController(context);

            // Act
            IActionResult result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        /// <summary>
        /// Tests that Create returns a ViewResult when a mocked ApplicationDbContext is provided.
        /// Input: a Moq.Mock of ApplicationDbContext constructed with DbContextOptions.
        /// Expected: a ViewResult is returned and its Model is null.
        /// </summary>
        [Fact]
        public void Create_NoParameters_WithMockedContext_ReturnsViewResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);
            var controller = new ReviewsController(mockContext.Object);

            // Act
            IActionResult result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        /// <summary>
        /// Helper to create a mocked DbSet{T} from an IEnumerable{T}.
        /// This sets up the IQueryable members so LINQ methods such as ToList() work.
        /// </summary>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = (data ?? Enumerable.Empty<T>()).AsQueryable();

            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Tests that Index returns a ViewResult with a List{Reviews} model containing the expected number of items.
        /// Input: various counts of reviews provided by the mocked DbSet (0, 1, 3).
        /// Expected: ViewResult is returned and the Model is a List{Reviews} whose Count equals the provided number.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void Index_WithVariousReviewCounts_ReturnsViewWithExpectedCount(int itemCount)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);

            var reviewsData = Enumerable.Range(1, itemCount)
                                        .Select(i => new Reviews
                                        {
                                            ReviewID = i,
                                            BookID = i,
                                            ReviewText = $"Review {i}",
                                            ReviewTimeStamp = DateTime.UtcNow,
                                            ReviewRating = i % 5
                                        })
                                        .ToList();

            var mockSet = CreateMockDbSet(reviewsData);
            mockContext.Setup(c => c.Reviews).Returns(mockSet.Object);

            var controller = new ReviewsController(mockContext.Object);

            // Act
            IActionResult result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Reviews>>(viewResult.Model);
            Assert.Equal(itemCount, model.Count);
            // Ensure sequence content matches expectation for non-empty lists
            if (itemCount > 0)
            {
                for (int i = 0; i < itemCount; i++)
                {
                    Assert.Equal(reviewsData[i].ReviewID, model[i].ReviewID);
                    Assert.Equal(reviewsData[i].BookID, model[i].BookID);
                    Assert.Equal(reviewsData[i].ReviewText, model[i].ReviewText);
                }
            }
        }

/// <summary>
/// Tests that Index throws an ArgumentNullException when the context's Reviews DbSet is null.
/// Input: ApplicationDbContext.Reviews returns null.
/// Expected: ArgumentNullException is thrown because ToList() throws when its source is null.
/// </summary>
[Fact]
public void Index_WhenReviewsIsNull_ThrowsNullReferenceException()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
    var mockContext = new Mock<ApplicationDbContext>(options);

    // Simulate an uninitialized Reviews property.
    mockContext.Setup(c => c.Reviews).Returns((DbSet<Reviews>?)null);

    var controller = new ReviewsController(mockContext.Object);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => controller.Index());
}

        /// <summary>
        /// Tests that when the model state is valid the review timestamp is updated,
        /// the review is added to the context, SaveChangesAsync is called, and the result
        /// redirects to Books/Details with the review's BookID. Also verifies user id
        /// handling based on authentication and presence/value of the NameIdentifier claim.
        /// Conditions tested (parameterized):
        /// - isAuthenticated: whether User.Identity.IsAuthenticated is true.
        /// - includeClaim: whether a NameIdentifier claim is present.
        /// - claimValue: value of the claim when present.
        /// Expected:
        /// - If authenticated and claimValue is non-empty, review.UserID is set to claimValue.
        /// - Otherwise review.UserID remains unchanged.
        /// - ReviewTimeStamp is updated from its initial value.
        /// - DbSet.Add and SaveChangesAsync are invoked exactly once.
        /// </summary>
        [Theory]
        [InlineData(true, true, "user123", true)]
        [InlineData(true, true, "", false)]
        [InlineData(false, false, "", false)]
        public async Task Create_ModelStateValid_UserHandlingAndPersistenceBehavior(
            bool isAuthenticated,
            bool includeClaim,
            string claimValue,
            bool expectUserIdUpdated)
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);

            var mockSet = new Mock<DbSet<Reviews>>();

            // Ensure Add is tracked; return null is acceptable as controller ignores return value.
            mockSet
                .Setup(m => m.Add(It.IsAny<Reviews>()))
                .Returns((EntityEntry<Reviews>?)null)
                .Verifiable();

            mockContext
                .SetupGet(c => c.Reviews)
                .Returns(mockSet.Object);

            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new ReviewsController(mockContext.Object);

            // Prepare ClaimsPrincipal according to inputs.
            List<Claim> claims = new List<Claim>();
            if (includeClaim)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, claimValue));
            }

            // If authentication type is null -> IsAuthenticated == false; otherwise true.
            var identity = includeClaim
                ? new ClaimsIdentity(claims, isAuthenticated ? "TestAuth" : null)
                : new ClaimsIdentity(new Claim[] { }, isAuthenticated ? "TestAuth" : null);

            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Start with a non-default timestamp and specific UserID to observe changes.
            var review = new Reviews
            {
                BookID = 42,
                ReviewText = "text",
                ReviewRating = 5,
                ReviewTimeStamp = DateTime.UtcNow.AddDays(-1),
                UserID = "initialUser"
            };

            DateTime before = review.ReviewTimeStamp;

            // Act
            IActionResult result = await controller.Create(review);

            // Assert
            // Redirect result checks
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Books", redirect.ControllerName);

            Assert.True(redirect.RouteValues.ContainsKey("id"));
            Assert.Equal(review.BookID, Convert.ToInt32(redirect.RouteValues["id"]));

            // Timestamp must be updated for valid model state.
            Assert.True(review.ReviewTimeStamp > before, "ReviewTimeStamp was not updated on valid model.");

            // Verify persistence calls
            mockSet.Verify(m => m.Add(It.Is<Reviews>(r => object.ReferenceEquals(r, review))), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify user id update behavior
            if (expectUserIdUpdated)
            {
                Assert.Equal(claimValue, review.UserID);
            }
            else
            {
                Assert.Equal("initialUser", review.UserID);
            }
        }

        /// <summary>
        /// Tests that when the model state is invalid:
        /// - The controller does NOT call DbSet.Add or SaveChangesAsync.
        /// - The controller still redirects to Books/Details with the review's BookID.
        /// - The ReviewTimeStamp remains unchanged.
        /// Input: ModelState contains an error.
        /// Expected: No persistence operations; timestamp unchanged; redirect to Details.
        /// </summary>
        [Fact]
        public async Task Create_ModelStateInvalid_NoPersistenceAndRedirectStillOccurs()
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);

            var mockSet = new Mock<DbSet<Reviews>>();
            mockContext.SetupGet(c => c.Reviews).Returns(mockSet.Object);

            // Ensure SaveChangesAsync would fail if called - set up to detect invocation.
            mockContext
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1)
                .Verifiable();

            var controller = new ReviewsController(mockContext.Object);

            // Mark model as invalid
            controller.ModelState.AddModelError("key", "some error");

            var review = new Reviews
            {
                BookID = 99,
                ReviewText = "bad",
                ReviewRating = 1,
                ReviewTimeStamp = DateTime.UtcNow.AddHours(-5),
                UserID = "initial"
            };

            DateTime before = review.ReviewTimeStamp;

            // Act
            IActionResult result = await controller.Create(review);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Books", redirect.ControllerName);
            Assert.Equal(review.BookID, Convert.ToInt32(redirect.RouteValues["id"]));

            // Timestamp should remain unchanged since model state invalid branch doesn't update it.
            Assert.Equal(before, review.ReviewTimeStamp);

            // Verify that Add and SaveChangesAsync were NOT invoked.
            mockSet.Verify(m => m.Add(It.IsAny<Reviews>()), Times.Never);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that Details returns NotFound when there is no review with the provided id.
        /// Input: various id values where Reviews collection does not contain a matching ReviewID (int.MinValue, -1, 0, int.MaxValue).
        /// Expected: NotFoundResult is returned for each case.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Details_IdProvided_ReviewNotFound_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var data = new List<Reviews>()
            {
                // intentionally empty or containing items with different IDs to ensure no match
                new Reviews { ReviewID = 1, BookID = 10 },
                new Reviews { ReviewID = 2, BookID = 20 }
            }.Where(r => r.ReviewID != id).ToList(); // ensure no item equals tested id

            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Reviews = mockSet.Object
            };

            var controller = new ReviewsController(context);

            // Act
            IActionResult result = controller.Details(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that Details returns a ViewResult containing the found review when the id exists.
        /// Input: id values for which the mocked Reviews collection contains a Reviews instance (1 and int.MaxValue).
        /// Expected: ViewResult is returned and the Model is the same instance returned by the data set.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Details_IdProvided_ReviewFound_ReturnsViewWithModel(int id)
        {
            // Arrange
            var model = new Reviews
            {
                ReviewID = id,
                BookID = 42
            };

            var data = new List<Reviews> { model }.AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Reviews = mockSet.Object
            };

            var controller = new ReviewsController(context);

            // Act
            IActionResult result = controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
        }

        /// <summary>
        /// Tests that Edit returns NotFound when the supplied id does not match review.ReviewID.
        /// Inputs: various id values (int.MinValue, -1, 0, int.MaxValue) while review.ReviewID = 1.
        /// Expected: NotFoundResult is returned and no update/save is attempted.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Edit_IdDoesNotMatch_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);
            var controller = new ReviewsController(mockContext.Object);

            var review = new Reviews
            {
                ReviewID = 1,
                BookID = 5
            };

            // Act
            IActionResult result = controller.Edit(id, review);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            // Ensure Update and SaveChanges were not called
            mockContext.Verify(m => m.Update(It.IsAny<Reviews>()), Times.Never);
            mockContext.Verify(m => m.SaveChanges(), Times.Never);
        }

        /// <summary>
        /// Tests that Edit updates the context, saves changes and redirects to Books/Details when ModelState is valid.
        /// Inputs: matching id and review.ReviewID (tested for 1 and int.MaxValue).
        /// Expected: RedirectToActionResult to ("Details", "Books") with route id = review.BookID, and Update/SaveChanges invoked once.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Edit_ModelStateValid_UpdatesSavesAndRedirects(int id)
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);
            // Ensure SaveChanges has a benign return value
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ReviewsController(mockContext.Object);

            var review = new Reviews
            {
                ReviewID = id,
                BookID = 42
            };

            // Sanity: ModelState is valid by default (no errors added)

            // Act
            IActionResult actionResult = controller.Edit(id, review);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(actionResult);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Books", redirect.ControllerName);

            // RouteValues contains the id passed to Books/Details
            Assert.NotNull(redirect.RouteValues);
            Assert.True(redirect.RouteValues.ContainsKey("id"));
            Assert.Equal(review.BookID, redirect.RouteValues["id"]);

            mockContext.Verify(m => m.Update(It.Is<Reviews>(r => object.ReferenceEquals(r, review))), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Edit returns the View with the supplied review when ModelState is invalid.
        /// Input: id matches review.ReviewID but ModelState contains an error.
        /// Expected: ViewResult is returned with Model equal to the provided review and no DB update/save occurs.
        /// </summary>
        [Fact]
        public void Edit_ModelStateInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var options = new DbContextOptions<ApplicationDbContext>();
            var mockContext = new Mock<ApplicationDbContext>(options);
            var controller = new ReviewsController(mockContext.Object);

            var review = new Reviews
            {
                ReviewID = 10,
                BookID = 99
            };

            // Make ModelState invalid
            controller.ModelState.AddModelError("SomeKey", "Some error");

            // Act
            IActionResult result = controller.Edit(review.ReviewID, review);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(review, viewResult.Model);

            mockContext.Verify(m => m.Update(It.IsAny<Reviews>()), Times.Never);
            mockContext.Verify(m => m.SaveChanges(), Times.Never);
        }

        /// <summary>
        /// Tests that Delete returns NotFound when no review exists with the provided id.
        /// Input: various id values where the Reviews DbSet contains no elements (int.MinValue, -1, 0, int.MaxValue).
        /// Expected: NotFoundResult is returned for each case.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Delete_IdProvided_ReviewNotFound_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var emptyData = new List<Reviews>().AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(emptyData.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(emptyData.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(emptyData.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => emptyData.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Reviews = mockSet.Object
            };

            var controller = new ReviewsController(context);

            // Act
            IActionResult result = controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests that Delete returns a ViewResult containing the found review when the id exists.
        /// Input: id values for which the mocked Reviews DbSet contains a Reviews instance (1 and int.MaxValue).
        /// Expected: ViewResult is returned and the Model is the same instance present in the DbSet.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Delete_IdProvided_ReviewFound_ReturnsViewWithModel(int id)
        {
            // Arrange
            var review = new Reviews
            {
                ReviewID = id,
                UserID = "user",
                BookID = 42,
                ReviewText = "text",
                ReviewTimeStamp = DateTime.UtcNow,
                ReviewRating = 5
            };

            var data = new List<Reviews> { review }.AsQueryable();

            var mockSet = new Mock<DbSet<Reviews>>();
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Reviews>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var context = new ApplicationDbContext(options)
            {
                Reviews = mockSet.Object
            };

            var controller = new ReviewsController(context);

            // Act
            IActionResult result = controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(review, viewResult.Model);
        }

        /// <summary>
        /// Tests that when a non-null ApplicationDbContext is supplied to the ReviewsController constructor,
        /// subsequent calls that use the context (Index) return a ViewResult with the reviews coming from
        /// the supplied context's Reviews DbSet.
        /// Input: a mocked ApplicationDbContext with Reviews returning two Reviews instances.
        /// Expected: Index returns ViewResult and the Model is a collection containing the same two Reviews.
        /// </summary>
        [Fact]
        public void ReviewsController_WithValidContext_IndexUsesProvidedContext_ReturnsViewWithReviews()
        {
            // Arrange
            var sampleReviews = new List<Reviews>
            {
                new Reviews { ReviewID = 1, BookID = 10, ReviewText = "A", ReviewRating = 5 },
                new Reviews { ReviewID = 2, BookID = 20, ReviewText = "B", ReviewRating = 4 }
            };

            var mockSet = CreateMockDbSet(sampleReviews);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);
            mockContext.SetupGet(c => c.Reviews).Returns(mockSet.Object);

            var controller = new ReviewsController(mockContext.Object);

            // Act
            ActionResult result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);

            // The controller calls ToList(), which materializes a list with the same element instances.
            var modelAsEnumerable = Assert.IsAssignableFrom<IEnumerable<Reviews>>(viewResult.Model);
            Assert.Equal(sampleReviews.Count, modelAsEnumerable.Count());
            Assert.Collection(
                modelAsEnumerable,
                item => Assert.Same(sampleReviews[0], item),
                item => Assert.Same(sampleReviews[1], item)
            );
        }

        /// <summary>
        /// Tests that when null is supplied to the ReviewsController constructor, controller methods that
        /// use the context throw a NullReferenceException.
        /// Input: null ApplicationDbContext.
        /// Expected: Index invocation throws NullReferenceException because _context is null.
        /// </summary>
        [Fact]
        public void ReviewsController_WithNullContext_IndexThrowsNullReferenceException()
        {
            // Arrange
            ApplicationDbContext? nullContext = null;
            var controller = new ReviewsController(nullContext!);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => controller.Index());
        }

        /// <summary>
        /// Tests that Delete redirects to Index when no review with the supplied id exists.
        /// Input: various id values where _context.Reviews contains no matching review (int.MinValue, -1, 0, int.MaxValue).
        /// Expected: RedirectToActionResult to Index is returned and no Remove/SaveChanges calls occur.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Delete_IdNotFound_RedirectsToIndex(int id)
        {
            // Arrange
            var emptyData = new List<Reviews>().AsQueryable();
            var mockSet = CreateMockDbSet(emptyData);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
            var mockContext = new Mock<ApplicationDbContext>(options);

            mockContext
                .SetupGet(c => c.Reviews)
                .Returns(mockSet.Object);

            // Ensure SaveChanges would throw if called unexpectedly (safer than verifying later).
            mockContext
                .Setup(c => c.SaveChanges())
                .Throws(new InvalidOperationException("SaveChanges should not be called for not-found case"));

            var controller = new ReviewsController(mockContext.Object);

            // Act
            IActionResult result = controller.Delete(id, new Reviews { ReviewID = id, BookID = 1 });

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ReviewsController.Index), redirect.ActionName);
            // Ensure no attempt to redirect to Books controller
            Assert.NotEqual("Books", redirect.ControllerName);

            // Clean up: verify Remove never invoked
            mockSet.Verify(m => m.Remove(It.IsAny<Reviews>()), Times.Never);
            // And SaveChanges was not called (the setup would have thrown otherwise)
            mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        #region Helpers

        /// <summary>
        /// Creates a Mock&lt;DbSet&lt;T&gt;&gt; backed by the provided IQueryable data.
        /// This enables LINQ queries like FirstOrDefault to operate against the in-memory data.
        /// </summary>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            // Provide Add/Remove defaults so controller operations can be verified.
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(t =>
            {
                // If underlying data is a List<T>, attempt to add via reflection is unsafe; leave no-op.
            });

            return mockSet;
        }

        #endregion
    }
}