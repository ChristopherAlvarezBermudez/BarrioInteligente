using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Services;
using BarrioInteligenteWeb.Hubs;

// Crear carpeta Logs si no existe
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .WriteTo.File(
        path: Path.Combine(Directory.GetCurrentDirectory(), "Logs", "bug-report-.txt"),
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando Barrio Inteligente...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

<<<<<<< Updated upstream
=======
    // ── SQL Server: usa la cadena "DefaultConnection" de appsettings.json ──
>>>>>>> Stashed changes
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.Cookie.Name = "BarrioInteligente.Auth";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

    builder.Services.AddControllersWithViews();

    // Servicio de Emails
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Sistema de Reputación Ciudadana
    builder.Services.AddScoped<IReputacionService, ReputacionService>();

    // Filtro de Profanidades Híbrido
    builder.Services.AddScoped<IProfanityService, ProfanityService>();

    // HttpClient para API externa de moderación
    builder.Services.AddHttpClient("ModerationApi", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    // Soporte para proxy inverso
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // Aceptar tráfico de cualquier proxy/red (necesario para túneles locales)
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddSignalR();

    var app = builder.Build();

<<<<<<< Updated upstream
=======
    // ── Aplicar migraciones y seed de datos ──
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        DemoDataSeeder.Seed(context);
    }

>>>>>>> Stashed changes
    // Debe ir primero: transforma los headers X-Forwarded-* antes que el resto del pipeline
    app.UseForwardedHeaders();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHub<ReportesHub>("/reportesHub");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Reportes}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó de forma inesperada.");
}
finally
{
    Log.CloseAndFlush();
}