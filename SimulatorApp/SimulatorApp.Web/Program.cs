using Microsoft.EntityFrameworkCore;
using SimulatorApp.Core.Enums;
using SimulatorApp.Core.Models;
using SimulatorApp.Infrastructure.Data;
using SimulatorApp.Web.Components;
using SimulatorApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
});

builder.Services.AddSingleton<DashboardStateService>();
builder.Services.AddSingleton<MqttListenerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttListenerService>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Devices.AnyAsync())
    {
        var dev0 = new Device { MAC = "F3D24C8D0EA5", Model = "Climate Sensor x1402abc", Location = "livingroom", IsOnline = true, LastSeen = DateTime.UtcNow };
        var dev1 = new Device { MAC = "56699D64012D", Model = "Motion Sensor xyz123", Location = "backyard", IsOnline = true, LastSeen = DateTime.UtcNow };
        var dev2 = new Device { MAC = "3A2CE01FE222", Model = "Unusual Sensor t900", Location = "secret room", IsOnline = true, LastSeen = DateTime.UtcNow };

        await db.Devices.AddRangeAsync(
            dev0,
            dev1,
            dev2
        );

        await db.Sensors.AddRangeAsync(
            new Sensor { Device = dev0, SensorType = SensorType.Temperature, Name = "Temperature Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev0, SensorType = SensorType.Humidity, Name = "Humidity Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev0, SensorType = SensorType.SoC, Name = "Battery Level Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev1, SensorType = SensorType.Motion, Name = "Motion Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev2, SensorType = SensorType.Resistance, Name = "Resistance Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev2, SensorType = SensorType.Pressure, Name = "Pressure Sensor", IsOnline = true, LastSeen = DateTime.UtcNow },
            new Sensor { Device = dev2, SensorType = SensorType.CO2, Name = "CO2 Sensor", IsOnline = true, LastSeen = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
