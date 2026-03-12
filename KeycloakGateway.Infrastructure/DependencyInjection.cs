using KeycloakGateway.Application.Interfaces;
using KeycloakGateway.Infrastructure.Excel;
using KeycloakGateway.Infrastructure.Kafka;
using KeycloakGateway.Infrastructure.Keycloak;
using KeycloakGateway.Infrastructure.Keycloak.Services;
using KeycloakGateway.Infrastructure.PasswordReset;
using KeycloakGateway.Infrastructure.Redis;
using KeycloakGateway.Infrastructure.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace KeycloakGateway.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            // ✅ bind options từ appsettings
            services.Configure<KeycloakOptions>(
                configuration.GetSection("Keycloak"));

            // ✅ HttpClient
            services.AddHttpClient<IKeycloakAuthService, KeycloakAuthService>();
            services.AddHttpClient<IKeycloakUserService, KeycloakUserService>();

            services.AddHttpContextAccessor();

            // ✅ Admin token service
            services.AddScoped<IKeycloakAdminTokenService, KeycloakAdminTokenService>();
            services.AddScoped<IExcelImportService, ExcelImportService>();
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IEmailProducer, KafkaEmailProducer>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<AuthorizationCodeService>();

            return services;
        }
    }
}
