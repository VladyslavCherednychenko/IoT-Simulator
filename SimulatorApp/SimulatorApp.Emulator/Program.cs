using SimulatorApp.Emulator;
using SimulatorApp.Emulator.Mqtt;
using SimulatorApp.Emulator.Simulation;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<DeviceSimulator>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
