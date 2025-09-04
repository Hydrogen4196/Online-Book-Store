using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DummyProject.DataStorage.Data;
using DummyProject.DataStorage.Repository.IRepository;
using DummyProject.DataStorage.Repository;
using DummyProject.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("conStr2") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();


builder.Services.AddScoped<IEmailSender, EmailSender>();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "3548284232140866";
    options.AppSecret = "4cfdff1951ad25b31a2ee3a299dc881c";
});

builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "1097365962498-qnj2oerks34j0p48fdnrme8anhok1mbq.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("StripeSetting"));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));//class name;-<EmailSettings> secction name :-EmailSettings(appsetting.json)

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

//StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSetting")["ScretKey"];
StripeConfiguration.ApiKey = builder.Configuration.GetSection("StripeSetting")["SecretKey"];


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();