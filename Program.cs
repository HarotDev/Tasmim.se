var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Bind "Email" sektionen till EmailOptions
builder.Services.Configure<RaqmiWeb.Services.EmailOptions>(
    builder.Configuration.GetSection("Email"));

// Registrera MailKit-sändaren
builder.Services.AddSingleton<RaqmiWeb.Services.IAppEmailSender, RaqmiWeb.Services.SmtpEmailSender>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
