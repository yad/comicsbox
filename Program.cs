using Comicsbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ImageService, ImageService>();
builder.Services.AddTransient<PdfReaderService, PdfReaderService>();

builder.Services.AddSingleton<BookInfoService, BookInfoService>();
builder.Services.AddSingleton<ThumbnailProvider, ThumbnailProvider>();
builder.Services.AddSingleton<ThumbnailWorkerService, ThumbnailWorkerService>();

builder.Services.AddHostedService<ThumbnailWorkerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
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
