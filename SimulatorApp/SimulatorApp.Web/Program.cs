using Microsoft.EntityFrameworkCore;
using SimulatorApp.Core.Models;
using SimulatorApp.Infrastructure.Data;
using SimulatorApp.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Devices.AnyAsync())
    {
        var dev0 = new Device { Model = "Climate Sensor x1402abc", Location = "livingroom", IsOnline = true };
        var dev1 = new Device { Model = "Motion Sensor xyz123", Location = "backyard", IsOnline = true };
        var dev2 = new Device { Model = "Unusual Sensor t900", Location = "secret room", IsOnline = true };

        await db.Devices.AddRangeAsync(
            dev0,
            dev1,
            dev2
        );

        await db.Sensors.AddRangeAsync(
            new Sensor { Device = dev0, SensorType = SensorType.Temperature, Name = "Temperature Sensor", IsOnline = true },
            new Sensor { Device = dev0, SensorType = SensorType.Humidity, Name = "Humidity Sensor", IsOnline = true },
            new Sensor { Device = dev0, SensorType = SensorType.SoC, Name = "Battery Level Sensor", IsOnline = true },
            new Sensor { Device = dev1, SensorType = SensorType.Motion, Name = "Motion Sensor", IsOnline = true },
            new Sensor { Device = dev2, SensorType = SensorType.Resistance, Name = "Resistance Sensor", IsOnline = true },
            new Sensor { Device = dev2, SensorType = SensorType.Pressure, Name = "Pressure Sensor", IsOnline = true },
            new Sensor { Device = dev2, SensorType = SensorType.CO2, Name = "CO2 Sensor", IsOnline = true }
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
