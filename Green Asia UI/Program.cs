using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromHours(1);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.Configure<FormOptions>(x =>
{
	x.BufferBody = true;
	x.ValueLengthLimit = 1073741822; // 32 MiB
	x.ValueCountLimit = 64048;// 1024
	x.MultipartHeadersCountLimit = 256; // 16
	x.MultipartHeadersLengthLimit = 1024768; // 16384
	x.MultipartBoundaryLengthLimit = 1024; // 128
	x.MultipartBodyLengthLimit = 134217728; // 128 MiB
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

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomePage}/{id?}");

app.Run();
