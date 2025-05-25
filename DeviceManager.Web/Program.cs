using DeviceManager.Web;
using DeviceManager.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:44317/"),
    DefaultRequestHeaders = { { "Accept", "application/json" } }
});

builder.Services.AddSingleton<DeviceService>();

builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
});

await builder.Build().RunAsync();

