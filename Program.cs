using Comicsbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<BookInfoService, BookInfoService>();
builder.Services.AddSingleton<FileMapService, FileMapService>();
builder.Services.AddSingleton<ImageService, ImageService>();
builder.Services.AddTransient<PdfReaderService, PdfReaderService>();
builder.Services.AddSingleton<ThumbnailProvider, ThumbnailProvider>();

builder.Services.AddSingleton<PreCacheWorkerService, PreCacheWorkerService>();
builder.Services.AddSingleton<ReaderCleanerWorkerService, ReaderCleanerWorkerService>();
builder.Services.AddSingleton<ThumbnailWorkerService, ThumbnailWorkerService>();
builder.Services.AddSingleton<ZipCleanerWorkerService, ZipCleanerWorkerService>();

builder.Services.AddHostedService<PreCacheWorkerService>();
builder.Services.AddHostedService<ReaderCleanerWorkerService>();
builder.Services.AddHostedService<ThumbnailWorkerService>();
builder.Services.AddHostedService<ZipCleanerWorkerService>();

var app = builder.Build();

app.Services.GetService<FileMapService>()!.Init();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        string path = ctx.File.PhysicalPath!;
        string ext = Path.GetExtension(path);
        if (ext == ".jpg")
        {
            TimeSpan maxAge = new TimeSpan(7, 0, 0, 0);
            ctx.Context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
        }
    }
});
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
