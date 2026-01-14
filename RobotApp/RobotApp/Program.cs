using RobotApp.Components;
using RobotApp.Components.Layout.Classes;

var builder = WebApplication.CreateBuilder(args);



// Register the Simple MQTT client as an object in the dependency injection container
builder.Services.AddSingleton(SimpleMqttClient.CreateSimpleMqttClientForHiveMQ("webapp_storm")); 

// Configure a MQTT Message Processing Service (that runs continuously in the background)
// builder.Services.AddHostedService<MqttMessageProcessingService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddTransient<MagicNumberService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
