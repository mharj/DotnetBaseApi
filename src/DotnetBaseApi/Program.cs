using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using DotnetBaseApi.Models;
using DotnetBaseApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Adds authentication and authorization services to validate bearer tokens.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
builder.Services.AddAuthorization();

// Configure MongoDB settings
builder.Services.Configure<BookstoreDatabaseSettings>(
    builder.Configuration.GetSection(nameof(BookstoreDatabaseSettings)));

builder.Services.AddSingleton<IBookstoreDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<BookstoreDatabaseSettings>>().Value);

builder.Services.AddSingleton<IBookService, BookService>();

// Register controllers with JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

// Configure WebSocket support
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

app.UseRouting();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
