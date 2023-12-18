using Azure.Identity;
using Barunson.DbContext;
using Barunson.FamilyWeb.Models;
using Barunson.FamilyWeb.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    //운영 환경에서만 사용
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://barunsecret.vault.azure.net/"),
        new DefaultAzureCredential());
}
else
{
    //개발용 환경에서만 사용
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://dev-barunsecret.vault.azure.net/"),
        new DefaultAzureCredential());
}
// DB Context 
builder.Services.AddDbContext<BarunsonContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarunsonDBConn")));
builder.Services.AddDbContext<BarShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarShopDBConn")));

builder.Services.AddHttpClient();

builder.Services.AddScoped<INiceCPClientService, NiceCPClientService>();
builder.Services.AddScoped<IRouletteEventService, RouletteEventService>();
builder.Services.AddSingleton<List<SiteInfo>>(builder.Configuration.GetSection("SiteInfos").Get<List<SiteInfo>>());
builder.Services.AddSingleton<SiteConfig>(builder.Configuration.GetSection("SiteConfig").Get<SiteConfig>());

builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    //Application Gateway 사용으로 사용하지 않음.
    //AG 외부 서비스시 아래 주석 제거
    //app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

app.Run();
