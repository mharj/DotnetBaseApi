using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DotnetBaseApi.Models;
using DotnetBaseApi.Services;

public class BooksApiETagTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private const string BooksEndpoint = "/api/books";

    public BooksApiETagTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Extracts and validates the ETag from response headers.
    /// ETag format: "[hex-hash]" (MD5 hex = 32 characters)
    /// </summary>
    private string GetETagFromResponse(HttpResponseMessage response)
    {
        Assert.True(response.Headers.Contains("ETag"), "Response must contain ETag header");
        var etag = response.Headers.GetValues("ETag").FirstOrDefault();
        Assert.NotNull(etag);
        // Validate ETag format: quoted MD5 hash (32 hex characters)
        Assert.Matches(@"^""[A-F0-9]{32}""$", etag);
        return etag;
    }

    [Fact]
    public async Task GetBooks_ReturnsETag_InResponseHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(BooksEndpoint);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var etag = GetETagFromResponse(response);
        Assert.NotEmpty(etag);
    }

    [Fact]
    public async Task GetBooks_WithMatchingETag_Returns304NotModified()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // First request to get the ETag
        var firstResponse = await client.GetAsync(BooksEndpoint);
        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        var etag = GetETagFromResponse(firstResponse);

        // Act - Second request with If-None-Match header
        var secondRequest = new HttpRequestMessage(HttpMethod.Get, BooksEndpoint);
        secondRequest.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var secondResponse = await client.SendAsync(secondRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotModified, secondResponse.StatusCode);
        var content = await secondResponse.Content.ReadAsStringAsync();
        Assert.Empty(content);
    }

    [Fact]
    public async Task GetBooks_MultipleRequests_SameETag()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response1 = await client.GetAsync(BooksEndpoint);
        Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);
        var etag1 = GetETagFromResponse(response1);

        var response2 = await client.GetAsync(BooksEndpoint);
        Assert.Equal(System.Net.HttpStatusCode.OK, response2.StatusCode);
        var etag2 = GetETagFromResponse(response2);

        // Assert - ETags should be identical for the same data
        Assert.Equal(etag1, etag2);
    }

    [Fact]
    public async Task GetBooks_WithDifferentETag_Returns200Ok()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - First request
        var firstResponse = await client.GetAsync(BooksEndpoint);
        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        GetETagFromResponse(firstResponse); // Validate first response has valid ETag

        // Second request with a different (invalid) ETag
        var secondRequest = new HttpRequestMessage(HttpMethod.Get, BooksEndpoint);
        secondRequest.Headers.TryAddWithoutValidation("If-None-Match", "\"INVALID-ETAG-VALUE\"");
        var secondResponse = await client.SendAsync(secondRequest);

        // Assert - Should get full response with 200 and valid ETag
        Assert.Equal(System.Net.HttpStatusCode.OK, secondResponse.StatusCode);
        var etag = GetETagFromResponse(secondResponse);
        var content = await secondResponse.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }
}

/// <summary>
/// Custom factory that removes authentication requirements for testing and mocks the BookService
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the real BookService implementation
            var bookServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBookService));
            if (bookServiceDescriptor != null)
            {
                services.Remove(bookServiceDescriptor);
            }

            // Add a mocked BookService
            var mockBookService = new Mock<IBookService>();
            
            // Create some test books
            var testBooks = new List<Book>
            {
                new Book 
                { 
                    Id = "1", 
                    BookName = "Test Book 1", 
                    Price = 10.99m, 
                    Category = "Fiction", 
                    Author = "Author One" 
                },
                new Book 
                { 
                    Id = "2", 
                    BookName = "Test Book 2", 
                    Price = 15.99m, 
                    Category = "NonFiction", 
                    Author = "Author Two" 
                }
            };

            // Setup mock to return test books
            mockBookService.Setup(s => s.Get()).Returns(testBooks);
            mockBookService.Setup(s => s.Get("1")).Returns(testBooks.FirstOrDefault(b => b.Id == "1"));
            mockBookService.Setup(s => s.Get("2")).Returns(testBooks.FirstOrDefault(b => b.Id == "2"));
            mockBookService.Setup(s => s.Get(It.IsAny<string>())).Returns((string id) => 
                testBooks.FirstOrDefault(b => b.Id == id));

            services.AddSingleton(mockBookService.Object);

            // Remove the authorization middleware for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", null);

            // Replace the authorization policy to allow everything in tests
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }
}

/// <summary>
/// Test authentication handler that automatically authenticates all requests
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "TestUser"),
            new(ClaimTypes.Name, "Test User"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}