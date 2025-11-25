using Harmic.Models;
using Microsoft.EntityFrameworkCore;
using Harmic.Services;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HarmicContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("HarmicContext"));
});

// VNPAY config (unchanged)
builder.Services.AddSingleton<IVnpay>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Vnpay");
    var tmnCode = cfg["TmnCode"];
    var hashSecret = cfg["HashSecret"];
    var baseUrl = cfg["BaseUrl"];
    var callbackUrl = cfg["CallbackUrl"];

    if (string.IsNullOrWhiteSpace(tmnCode) ||
        string.IsNullOrWhiteSpace(hashSecret) ||
        string.IsNullOrWhiteSpace(baseUrl) ||
        string.IsNullOrWhiteSpace(callbackUrl))
    {
        throw new InvalidOperationException("Vnpay config missing required values.");
    }

    var vnpay = new Vnpay();
    vnpay.Initialize(tmnCode, hashSecret, baseUrl, callbackUrl);
    return vnpay;
});

// Gemini
var apiKey = builder.Configuration["Gemini:ApiKey"]
             ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
             ?? string.Empty;
builder.Services.AddSingleton(new GeminiService(apiKey));

builder.Services.AddScoped<ChatRetrievalService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
