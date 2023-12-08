using Azure.Identity;
using Barunson.DbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    //� ȯ�濡���� ���
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://barunsecret.vault.azure.net/"),
        new DefaultAzureCredential());
}
else
{
    //���߿� ȯ�濡���� ���
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://dev-barunsecret.vault.azure.net/"),
        new DefaultAzureCredential());
}
// DB Context 
builder.Services.AddDbContext<BarunsonContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarunsonDBConn")));
builder.Services.AddDbContext<BarShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarShopDBConn")));

builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/health");

app.Run();
