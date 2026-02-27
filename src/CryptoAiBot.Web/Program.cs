using CryptoAiBot.Infrastructure;
using CryptoAiBot.Web.Data;
using CryptoAiBot.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cryptoaibot.db"));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SignalFeedService>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

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
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    var hasMigrations = (await db.Database.GetMigrationsAsync()).Any();
    if (hasMigrations)
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        var hasAnyTables = await DatabaseHasAnyTablesAsync(db);
        var hasRolesTable = await DatabaseHasTableAsync(db, "AspNetRoles");

        if (hasAnyTables && !hasRolesTable)
        {
            logger.LogWarning(
                "Existing SQLite database schema is incomplete (missing AspNetRoles). Recreating the database because no EF migrations were found.");
            await db.Database.EnsureDeletedAsync();
        }

        await db.Database.EnsureCreatedAsync();
    }

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
}

app.Run();

static async Task<bool> DatabaseHasAnyTablesAsync(AppDbContext db)
{
    await using var connection = db.Database.GetDbConnection();
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND rootpage IS NOT NULL;";
    var count = (long)(await command.ExecuteScalarAsync() ?? 0L);
    return count > 0;
}

static async Task<bool> DatabaseHasTableAsync(AppDbContext db, string tableName)
{
    await using var connection = db.Database.GetDbConnection();
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";

    var parameter = command.CreateParameter();
    parameter.ParameterName = "$tableName";
    parameter.Value = tableName;
    command.Parameters.Add(parameter);

    var count = (long)(await command.ExecuteScalarAsync() ?? 0L);
    return count > 0;
}
