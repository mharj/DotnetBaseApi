using DotnetBaseApi.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetBaseApi.Services
{
    public class BookService : IBookService
    {
        private readonly IMongoCollection<Book> _books;
        private readonly ILogger<BookService> _logger;

        public BookService(IBookstoreDatabaseSettings settings, ILogger<BookService> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _books = database.GetCollection<Book>(settings.BooksCollectionName);
            
            // Fire-and-forget: Create indexes asynchronously without blocking constructor
            _ = CreateIndexAsync();
        }

        /// <summary>
        /// Creates database indexes asynchronously for improved query performance.
        /// This is a fire-and-forget operation; failures are logged but don't block initialization.
        /// </summary>
        public async Task CreateIndexAsync()
        {
            try
            {
                var nameIndex = Builders<Book>.IndexKeys.Ascending(indexKey => indexKey.BookName);
                var authorIndex = Builders<Book>.IndexKeys.Ascending(indexKey => indexKey.Author);
                
                await _books.Indexes.CreateOneAsync(new CreateIndexModel<Book>(nameIndex)).ConfigureAwait(false);
                await _books.Indexes.CreateOneAsync(new CreateIndexModel<Book>(authorIndex)).ConfigureAwait(false);
                
                _logger.LogInformation("Database indexes created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create database indexes. Queries may be slower.");
            }
        }

        public List<Book> Get() =>
            _books.Find(book => true).ToList();

        public Book Get(string id) =>
            _books.Find<Book>(book => book.Id == id).FirstOrDefault();

        public Book Create(Book book)
        {
            _books.InsertOne(book);
            return book;
        }

        public void Update(string id, Book bookIn) =>
            _books.ReplaceOne(book => book.Id == id, bookIn);

        public void Remove(Book bookIn) =>
            _books.DeleteOne(book => book.Id == bookIn.Id);

        public void Remove(string id) =>
            _books.DeleteOne(book => book.Id == id);
    }
}