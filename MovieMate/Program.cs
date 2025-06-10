using Microsoft.AspNetCore.Authentication.Cookies;
using MovieMate.DAL.Interfaces;
using MovieMate.DAL.Repositories;
using MovieMate.BLL.Interfaces;
using MovieMate.BLL.Services;
using MovieMate.BLL.Interfaces.MovieMate.BLL.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IRecommendationFeedbackRepository, RecommendationFeedbackRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IRecommendationFeedbackService, RecommendationFeedbackService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// --- Authentication and Authorization Setup ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";          // Path to your login page
        options.LogoutPath = "/Auth/Logout";         // Path to your logout action
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Path if authorization fails
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // How long the cookie is valid
        options.SlidingExpiration = true; // Resets expiry time on activity
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser() // Must be logged in
              .RequireRole("Admin"));     // Must have the "Admin" role claim

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser()); // Simple policy for any logged-in user
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
