using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using devexBlazor.Data;
using DevExpress.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<FileUrlStorageService>();
//builder.Services.AddHttpClient("MyApi", config => config.BaseAddress = new Uri("http://localhost:5184/"));
builder.Services.AddHttpClient("MyApi", config => config.BaseAddress = new Uri("https://localhost:63069/"));
builder.Services.AddControllers();
builder.Services.AddDevExpressBlazor(configure => configure.BootstrapVersion = BootstrapVersion.v5);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
