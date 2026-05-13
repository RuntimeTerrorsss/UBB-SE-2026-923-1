using Microsoft.AspNetCore.Authentication.Cookies;
// TODO: Add the using statements for your shared project here
// using BookingBoardGames.Shared.Services;
// using BookingBoardGames.Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);


// ADD AUTHENTICATION (Requirement: Guard from unauthorized users)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // If an unauthenticated user tries to access a guarded page, 
        // they will be redirected here. You'll need to create an AccountController later.
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();


// DEPENDENCY INJECTION (Requirement: Use a DI framework)
builder.Services.AddHttpClient<IYourProxyRepository, YourProxyRepository>(client =>
{
    // TODO: Replace with the actual URL and port your API runs on
    client.BaseAddress = new Uri("https://localhost:5001/");
});

// B. Register your Business Logic Services from your Shared Library
// TODO: Replace these with your actual interface and implementation names
builder.Services.AddScoped<IYourBusinessService, YourBusinessService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// MIDDLEWARE PIPELINE 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();