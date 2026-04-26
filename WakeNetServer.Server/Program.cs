using WakeNetServer.Server.Repository.Sqlite;
using WakeNetServer.Server.Utils;
using WakeNetServer.Server.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

DotEnv.LoadNearest(
    builder.Environment.ContentRootPath,
    Directory.GetCurrentDirectory()
);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "WakeNet.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // Nếu FE gọi cross-site (vd: localhost:3000 -> localhost:5000/5001) thì cần SameSite=None
    options.Cookie.SameSite = SameSiteMode.None;
    // Cookie SameSite=None bắt buộc Secure trên HTTPS; trên HTTP sẽ không gửi cookie (nên dùng Vite proxy khi dev).
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.IdleTimeout = TimeSpan.FromDays(7);
});

var corsOrigins = (Environment.GetEnvironmentVariable("WAKENET_CORS_ORIGINS") ?? string.Empty)
    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(o => o.Trim())
    .Where(o => o.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WakeNetCors", policy =>
    {
        if (corsOrigins.Length == 0)
        {
            // Không cấu hình thì mặc định chỉ cho same-origin (không set policy)
            return;
        }

        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSqliteRepositories();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WakeNetServer.Server.Repository.Sqlite.WakeNetDbContext>();
    db.Database.EnsureCreated();
}

app.UseGlobalExceptionHandling();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Unspecified
});

if (corsOrigins.Length > 0)
{
    app.UseCors("WakeNetCors");
}

app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
