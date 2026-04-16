using Size_Finder.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<ShopifyService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ShopifyService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Add this to allow iframe embedding
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Frame-Options");
    context.Response.Headers.Append("X-Frame-Options", "ALLOWALL");
    context.Response.Headers.Append("Content-Security-Policy",
        "frame-ancestors *");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SizeFinder}/{action=Index}/{id?}");

app.Run();