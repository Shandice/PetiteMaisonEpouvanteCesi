using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PetiteMaisonEpouvante.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// when deployed in Kubernetes the UI and API will share the same host via ingress, so
// a relative base address simplifies configuration (requests go to /api).
// For local non-Kubernetes development the environment still works thanks to
// the Docker-Desktop /etc/hosts entries (api.localhost).
var apiBase = builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });

await builder.Build().RunAsync();