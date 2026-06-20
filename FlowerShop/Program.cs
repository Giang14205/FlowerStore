using AspNetCoreHero.ToastNotification;
using FlowerShop.Models;
using FlowerShop.Services.Vnpay;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<QlbhtContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Add services to the container.builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();


// Đăng kí vnpay nè 

builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        // Đường dẫn mà hệ thống sẽ chuyển hướng đến nếu người dùng chưa đăng nhập
        options.LoginPath = "/Login/DetailLogin";

        // Đường dẫn khi truy cập bị từ chối (bị Authorize nhưng không đủ quyền)
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Dịch vụ Authorization (ủy quyền)
builder.Services.AddAuthorization();


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

// 1. Đăng ký dịch vụ Session (Chèn TRƯỚC builder.Build())
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // 1. Đọc thông tin người dùng từ cookie/session
app.UseAuthorization();

// 2. Kích hoạt Session (Chèn TRƯỚC app.MapControllerRoute)
app.UseSession();
//là để thiết lập tuyến đường (route) cho Areas trong ứng dụng MVC.
app.MapControllerRoute(
            name:"areas",
            pattern:"{area:exists}/{controller=Home}/{action=Index}/{id?}"

    );
app.MapControllerRoute(
    name: "Trang-chu",
    pattern: "Trang-chu",
    defaults: new { controller = "Home1", action = "DetailHome1" }
);
app.MapControllerRoute(
    name: "Cua-hang",
    pattern: "Cua-hang",
    defaults: new { controller = "ShopFullScreen", action = "IDetailShopFullScreen" }
// Lưu ý: controller="Product" hay "Products" tùy tên file của bạn
);
app.MapControllerRoute(
    name: "Chung-toi",
    pattern: "Chung-toi",
    defaults: new { controller = "Home1", action = "DetailHome1" }
);
app.MapControllerRoute(
    name: "Bai-viet",
    pattern: "Bai-viet",
    defaults: new { controller = "Blog", action = "DetailBlog" }
);
app.MapControllerRoute(
    name: "Lien-he",
    pattern: "Lien-he",
    defaults: new { controller = "Contact", action = "DetailContact" }
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



