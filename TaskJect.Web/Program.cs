using TaskJect.Web.Common;
using TaskJect.Web.DbContexts;
using TaskJect.Web.DomainEvent;
using TaskJect.Web.Hubs;
using TaskJect.Web.Middleware;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using TaskJect.Web.Services.BackgroundServices;
using Data;
using Domain.Database;
using Domain.DomainEvents;
using Domain.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//db connection
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(defaultConnection));

//IMemoryCache Explicit dependency connection
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();

if (builder.Configuration.GetValue<bool>("GitHub:Enabled"))
{
    builder.Services.AddHttpClient<GitHubAppService>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(builder.Configuration["GitHub:UserAgent"]);
        client.Timeout = TimeSpan.FromSeconds(60);
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddErrorDescriber<LocalizedIdentityErrorDescriber>()
.AddEntityFrameworkStores<ApplicationDbContext>() 
.AddDefaultTokenProviders();

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ByteBustersCookies" + builder.Environment.EnvironmentName;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

#region Localization

builder.Services.AddLocalization(options => options.ResourcesPath = "");

var supportedCultures = new[] { "en", "uk" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;

    options.RequestCultureProviders = new[] {
        new CookieRequestCultureProvider()
    };
});

#endregion

//mapper
builder.Services.AddAutoMapper(
    typeof(Data.Mapper.DataMappingProfile).Assembly,
    typeof(TaskJect.Web.Mapping.WebMappingProfile).Assembly
);


builder.Services.AddSignalR();

builder.Services.AddApplicationInsightsTelemetry();

//ï³äêëþ÷åííÿ ñåðâ³ñ³â ç Data
builder.Services.AddData(builder.Configuration);

builder.Services.Configure<GmailOptions>(
    builder.Configuration.GetSection("Gmail"));

builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection("Telegram"));

builder.Services.Configure<EncryptionOptions>(
    builder.Configuration.GetSection("Encryption"));

builder.Services.Configure<GoogleAnalytics>(
    builder.Configuration.GetSection("GoogleAnalytics"));

if (builder.Configuration.GetValue<bool>("Telegram:Enabled"))
{
    builder.Services.AddScoped<ITelegramService, TelegramService>();
    builder.Services.AddHostedService<TelegramTicketMaintenanceService>();
    builder.Services.AddHostedService<TelegramBackgroundWorker>();
}
else
{
    builder.Services.AddScoped<ITelegramService, FakeTelegramService>();
}
builder.Services.AddSingleton<ITelegramLinkBuilder, TelegramLinkBuilder>();
builder.Services.AddSingleton<TelegramMessageQueue>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailSender, ZohoEmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ITariffPlansInitializer, TariffPlansInitializer>();
builder.Services.AddScoped<IProjectUserPermissionInitializer, ProjectUserPermissionInitializer>();
builder.Services.AddScoped<IAvailableProjectPermissionChecker, AvailableProjectPermissionChecker>();
builder.Services.AddScoped<IEmailRequestSender, EmailRequestSender>();
builder.Services.AddScoped<IUserCreator, UserCreator>();
//builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory>();
builder.Services.AddScoped<ITemplateEmailBody, TemplateEmailBody>();
builder.Services.AddScoped<IRegistarionOrganization, RegistarionOrganization>();
builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddScoped<AesEncryptionHelper>();
builder.Services.AddScoped<IGumroadLinkProvider, GumroadLinkProvider>();

builder.Services.AddScoped<IWayforpayServices, WayforpayServices>();
builder.Services.AddScoped<IWayforpayWebhookService, WayforpayWebhookService>();

builder.Services.AddScoped<IGitHubService, GitHubService>();
if (builder.Configuration.GetValue<bool>("GitHub:Enabled"))
{
    builder.Services.AddScoped<IGitHubAppService, GitHubAppService>();
    builder.Services.AddScoped<IGitHubWebhookService, GitHubWebhookService>();
}
else
{
    builder.Services.AddScoped<IGitHubAppService, FakeGitHubAppService>();
    builder.Services.AddScoped<IGitHubWebhookService, FakeGitHubWebhookService>();
}

builder.Services.AddSingleton<ILocalizationProvider, LocalizationProvider>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<BaseNotificationQueue>();
builder.Services.AddScoped<IDomainEventHandler<TaskStatusChangedDomainEvent>, TaskStatusChangedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskDeadlineChangedDomainEvent>, TaskDeadlineChangedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskAssigneeChangedDomainEvent>, TaskAssigneeChangedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<OrganizationAppealSendsDomainEvent>, OrganizationAppealSendsDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskCreatedDomainEvent>, TaskCreatedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<TaskUpdatedDomainEvent>, TaskUpdatedDomainEventHandler>();

builder.Services.AddScoped<IDomainEventHandler<SubscriptionCongratulationsDomainEvent>, SubscriptionCongratulationsDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<SubscriptionPaymentFailedDomainEvent>, SubscriptionPaymentFailedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<SubscriptionPaymentRefundedDomainEvent>, SubscriptionPaymentRefundedDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<SubscriptionExpirationInWeekDomainEvent>, SubscriptionExpirationInWeekDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<SubscriptionExpirationIn3DaysDomainEvent>, SubscriptionExpirationIn3DaysDomainEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<SubscriptionExpiredDomainEvent>, SubscriptionExpiredDomainEventHandler>();

#region background services

builder.Services.AddHostedService<NotificationBackgroundWorker>();
#endregion


var app = builder.Build();
app.UseRequestLocalization();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        string originalPath = ctx.Request.Path.Value;
        ctx.Items["originalPath"] = originalPath;
        ctx.Request.HttpContext.Response.Redirect("/error/404");
        //await next();
    }
});
//var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<OrgCodeValidationMiddleware>();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Get IDbInitializer within the request scope
using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;

    //БД ініціалізовано вже, в поточному ініціалізаторі немає потреби + дані не валідні
    //var dbInitializer = scopedServices.GetRequiredService<IDbInitializer>();
    //await dbInitializer.InitializeAsync();
    var tariffPlanInitializer = scopedServices.GetRequiredService<ITariffPlansInitializer>();
    var projectUserPermissionInitializer = scopedServices.GetRequiredService<IProjectUserPermissionInitializer>();

    await tariffPlanInitializer.InitializeAsync();
    await projectUserPermissionInitializer.InitializeAsync();

    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
    await telegramService.RegisterWebhookAsync();
}

app.Run();
