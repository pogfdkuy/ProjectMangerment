using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Web.Components;
using ProjectManagement.Web.Components.Account;
using ProjectManagement.Web.Data;
using ProjectManagement.Web.Models;
using ProjectManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 開發環境下打開詳細例外訊息：預設 Blazor 只會在瀏覽器 Console 顯示「There was an
// unhandled exception on the current circuit...」這種通用訊息，真正的例外型別/訊息/
// 堆疊追蹤要開這個才看得到。只在 Development 開，正式環境不會外洩例外細節。
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
    {
        options.DetailedErrors = true;
    });
}

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 注意：這兩個註冊的「順序」很重要。
// EF Core 內部用 TryAdd 來註冊 DbContextOptions<ApplicationDbContext>，也就是「先註冊的那個生命週期會贏」，
// 後面重複註冊會被忽略。AddDbContextFactory 需要的是 Singleton 生命週期的 Options；
// 如果讓 AddDbContext（Scoped）先註冊，會導致 Singleton 的 IDbContextFactory 依賴到 Scoped 的 Options，
// 在啟動時的服務驗證(ValidateOnBuild)就會直接炸掉「Cannot consume scoped service ... from singleton ...」。
// 所以一定要先呼叫 AddDbContextFactory，再呼叫 AddDbContext。

// Blazor Server 同一個連線(circuit)底下，畫面上的元件（例如版面配置裡的通知鈴鐺）
// 可能會跟目前這個頁面同時（並行）存取資料庫。EF Core 的 DbContext 不是執行緒安全的，
// 兩個並行的操作共用同一個 Scoped DbContext 實例就會丟出
// "A second operation was started on this context instance..." 這個例外。
// 所以我們自己的服務改用 IDbContextFactory，每次操作都建立一個獨立、短生命週期的 DbContext，
// 就不會跟其他元件共用同一個實例。
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity 需要一個可直接注入的 Scoped ApplicationDbContext。
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.Admin));
    options.AddPolicy("ManagerOrAdmin", p => p.RequireRole(RoleNames.Admin, RoleNames.Manager));
});

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// 業務邏輯服務
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IItemNoteService, ItemNoteService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// 啟動時灌入角色／預設管理員／示範用單位與系統類別（資料表結構仍須先用 `dotnet ef database update` 建立）。
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// 附件下載端點：需登入才能下載，附件本身存放在 wwwroot 之外，不會被直接用網址存取。
// 這個端點會帶 fileDownloadName，瀏覽器會強制觸發「下載」而不是直接開啟預覽。
app.MapGet("/attachments/{id:int}/download", async (int id, IAttachmentService attachmentService) =>
{
    var attachment = await attachmentService.GetByIdAsync(id);
    if (attachment is null) return Results.NotFound();

    var path = attachmentService.GetPhysicalPath(attachment);
    if (!File.Exists(path)) return Results.NotFound();

    return Results.File(path, attachment.ContentType, attachment.FileName);
}).RequireAuthorization();

// 附件檢視端點：不帶 fileDownloadName，就不會有 Content-Disposition: attachment，
// 瀏覽器對圖片、PDF 這類支援的格式會直接在新分頁預覽，而不是強制下載。
app.MapGet("/attachments/{id:int}/view", async (int id, IAttachmentService attachmentService) =>
{
    var attachment = await attachmentService.GetByIdAsync(id);
    if (attachment is null) return Results.NotFound();

    var path = attachmentService.GetPhysicalPath(attachment);
    if (!File.Exists(path)) return Results.NotFound();

    return Results.File(path, attachment.ContentType);
}).RequireAuthorization();

app.Run();
