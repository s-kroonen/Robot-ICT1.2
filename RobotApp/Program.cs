using Microsoft.EntityFrameworkCore;
using RobotApp.Components;
using RobotApp.Data;
using RobotApp.Repositories;
using RobotApp.Services.Mqtt;

var builder = WebApplication.CreateBuilder(args);



// Configure a MQTT Message Processing Service (that runs continuously in the background)
// builder.Services.AddHostedService<MqttMessageProcessingService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// Register the Simple MQTT client as an object in the dependency injection container
builder.Services.AddSingleton(SimpleMqttClient.CreateSimpleMqttClientForHiveMQ("webapp_storm"));

builder.Services.AddSingleton<RobotStateService>();
builder.Services.AddSingleton<MqttService>();
builder.Services.AddSingleton<RobotCommandService>();

builder.Services.AddScoped<IRobotRepository, RobotRepository>();
builder.Services.AddScoped<IMeasurementRepository, MeasurementRepository>();

builder.Services.AddDbContext<RobotDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("Default")
        )));

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var mqtt = scope.ServiceProvider.GetRequiredService<MqttService>();
    await mqtt.StartAsync();
}
app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
