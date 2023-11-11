using Comicsbox;
using Comicsbox.Cache;
using Comicsbox.FileBrowser;
using Comicsbox.Imaging;
using Comicsbox.Worker;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

var externalFileProvider = new PhysicalFileProvider(builder.Configuration["Settings:AbsoluteBasePath"]);
var compositeProvider = new CompositeFileProvider(externalFileProvider);
builder.Services.AddSingleton<IFileProvider>(compositeProvider);

builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IImageService, ImageService>();

builder.Services.AddTransient<IBookInfoService, BookInfoService>();
builder.Services.AddTransient<IFilePathFinder, FilePathFinder>();
builder.Services.AddSingleton<ThumbnailWorker, ThumbnailWorker>();

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
