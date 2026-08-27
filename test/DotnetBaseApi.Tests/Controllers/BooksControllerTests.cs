using System.Collections.Generic;
using Xunit;
using Moq;
using DotnetBaseApi.Controllers;
using DotnetBaseApi.Models;
using DotnetBaseApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBaseApi.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
        }

        [Fact]
        public void Get_Returns_List_Of_Books()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = "1", BookName = "Book 1", Author = "Author 1", Price = 10m, Category = "Fiction" },
                new Book { Id = "2", BookName = "Book 2", Author = "Author 2", Price = 15m, Category = "Non-Fiction" }
            };
            _mockBookService.Setup(s => s.Get()).Returns(books);

            // Act
            var result = _controller.Get();

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<Book>>>(result);
            var returnValue = Assert.IsType<List<Book>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            _mockBookService.Verify(s => s.Get(), Times.Once);
        }

        [Fact]
        public void Get_By_Id_Returns_Book_When_Found()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            var book = new Book { Id = bookId, BookName = "Test Book", Author = "Test Author", Price = 10m, Category = "Fiction" };
            _mockBookService.Setup(s => s.Get(bookId)).Returns(book);

            // Act
            var result = _controller.Get(bookId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Book>>(result);
            var returnValue = Assert.IsType<Book>(actionResult.Value);
            Assert.Equal(bookId, returnValue.Id);
            _mockBookService.Verify(s => s.Get(bookId), Times.Once);
        }

        [Fact]
        public void Get_By_Id_Returns_NotFound_When_Book_Not_Exist()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            _mockBookService.Setup(s => s.Get(bookId)).Returns((Book)null);

            // Act
            var result = _controller.Get(bookId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Book>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
            _mockBookService.Verify(s => s.Get(bookId), Times.Once);
        }

        [Fact]
        public void Create_Returns_CreatedAtRoute_With_New_Book()
        {
            // Arrange
            var newBook = new Book { BookName = "New Book", Author = "New Author", Price = 20m, Category = "Fiction" };
            _mockBookService.Setup(s => s.Create(It.IsAny<Book>()))
                .Callback((Book b) => b.Id = "507f1f77bcf86cd799439011");

            // Act
            var result = _controller.Create(newBook);

            // Assert
            var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
            Assert.Equal("GetBook", createdResult.RouteName);
            _mockBookService.Verify(s => s.Create(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public void Update_Returns_Updated_Book_When_Book_Exists()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            var existingBook = new Book { Id = bookId, BookName = "Old Title", Author = "Old Author", Price = 10m, Category = "Fiction" };
            var updatedBook = new Book { Id = bookId, BookName = "New Title", Author = "New Author", Price = 15m, Category = "Non-Fiction" };
            var returnBook = new Book { Id = bookId, BookName = "New Title", Author = "New Author", Price = 15m, Category = "Non-Fiction" };

            _mockBookService.Setup(s => s.Get(bookId)).Returns(existingBook);
            _mockBookService.Setup(s => s.Update(bookId, updatedBook));
            _mockBookService.Setup(s => s.Get(bookId)).Returns(returnBook);

            // Act
            var result = _controller.Update(bookId, updatedBook);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Book>>(result);
            var returnValue = Assert.IsType<Book>(actionResult.Value);
            Assert.Equal("New Title", returnValue.BookName);
            _mockBookService.Verify(s => s.Update(bookId, updatedBook), Times.Once);
        }

        [Fact]
        public void Update_Returns_NotFound_When_Book_Not_Exist()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            var updatedBook = new Book { BookName = "New Title", Author = "New Author", Price = 15m, Category = "Non-Fiction" };
            _mockBookService.Setup(s => s.Get(bookId)).Returns((Book)null);

            // Act
            var result = _controller.Update(bookId, updatedBook);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Book>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
            _mockBookService.Verify(s => s.Update(It.IsAny<string>(), It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public void Delete_Returns_NoContent_When_Book_Exists()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            var book = new Book { Id = bookId, BookName = "Test Book", Author = "Test Author", Price = 10m, Category = "Fiction" };
            _mockBookService.Setup(s => s.Get(bookId)).Returns(book);
            _mockBookService.Setup(s => s.Remove(bookId));

            // Act
            var result = _controller.Delete(bookId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookService.Verify(s => s.Remove(bookId), Times.Once);
        }

        [Fact]
        public void Delete_Returns_NotFound_When_Book_Not_Exist()
        {
            // Arrange
            var bookId = "507f1f77bcf86cd799439011";
            _mockBookService.Setup(s => s.Get(bookId)).Returns((Book)null);

            // Act
            var result = _controller.Delete(bookId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockBookService.Verify(s => s.Remove(It.IsAny<string>()), Times.Never);
        }
    }
}
