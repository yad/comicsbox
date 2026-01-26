using comicsbox.Models;
using comicsbox.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<PdfReaderService>();
builder.Services.AddHostedService<ThumbnailWorker>();

// ---------- ZipWorker ----------
builder.Services.AddSingleton<ZipWorker>();        // pour l’injection dans le controller
builder.Services.AddHostedService<ZipCleanupWorker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ZipWorker>()); // démarre le worker en arrière-plan

// --------------------------------

builder.Services.Configure<List<BookCategory>>(builder.Configuration.GetSection("BookCategories"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
