
using EmailWorker.Consumers;
using KeycloakGateway.EmailWorker.Models;
using KeycloakGateway.EmailWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<List<SmtpAccountOptions>>(
    builder.Configuration.GetSection("SmtpAccounts"));
builder.Services.Configure<EmailBrandingOptions>(
    builder.Configuration.GetSection("EmailBranding"));
builder.Services.AddSingleton<EmailAccountPool>();
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddHostedService<KafkaEmailConsumer>();

var app = builder.Build();
app.Run();