using LogMonitor.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<AscoltoRabbit>();

var host = builder.Build();
host.Run();