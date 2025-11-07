using AspNetCoreHero.ToastNotification;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(cfg => {
    cfg.Cookie.Name = "QLBHT";          // Tên session trong cookie
    cfg.IdleTimeout = new TimeSpan(0, 30, 0); // Thời gian hết hạn session = 30 phút
});




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

app.UseAuthorization();
//là để thiết lập tuyến đường (route) cho Areas trong ứng dụng MVC.
app.MapControllerRoute(
            name:"areas",
            pattern:"{area:exists}/{controller=Home}/{action=Index}/{id?}"

    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



