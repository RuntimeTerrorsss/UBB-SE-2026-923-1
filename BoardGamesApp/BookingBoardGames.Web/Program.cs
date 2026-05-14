using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Mapper;
using BookingBoardGames.Sharing.Repositories;
using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
string apiBaseUrl = "https://localhost:7027/";

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
builder.Services.AddHttpClient<IConversationRepository, ConversationAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<InterfaceGamesRepository, GamesAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IPaymentRepository, PaymentAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IRentalRepository, RentalAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IRepositoryPayment, RepositoryPaymentAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IUserRepository, UserAPIProxy>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// B. Register your Business Logic Services from your Shared Library
// TODO: Replace these with your actual interface and implementation names
builder.Services.AddScoped<InterfaceBookingService, BookingService>();
builder.Services.AddScoped<ICardPaymentService, CardPaymentService>();
builder.Services.AddScoped<ICashPaymentService, CashPaymentService>();
builder.Services.AddScoped<IConversationNotifier, ConversationNotifier>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<InterfaceGeographicalService, GeographicalService>();
builder.Services.AddScoped<IMapService, MapService>();
//builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IRentalService,  RentalService>();
builder.Services.AddScoped<InterfaceSearchAndFilterService, SearchAndFilterService>();
builder.Services.AddScoped<IServicePayment, ServicePayment>();
builder.Services.AddScoped<ICashPaymentMapper, CashPaymentMapper>();
builder.Services.AddScoped<IUserService, UserService>();


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