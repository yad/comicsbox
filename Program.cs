using Comicsbox;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ImageService, ImageService>();

builder.Services.AddSingleton<BookInfoService, BookInfoService>();
builder.Services.AddSingleton<ThumbnailProvider, ThumbnailProvider>();
builder.Services.AddSingleton<ThumbnailWorkerService, ThumbnailWorkerService>();

builder.Services.AddHostedService<ThumbnailWorkerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
