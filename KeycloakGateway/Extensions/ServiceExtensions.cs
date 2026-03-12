using KeycloakGateway.Infrastructure;

namespace KeycloakGateway.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApiServices(
      this IServiceCollection services,
      IConfiguration configuration)
        {
            // ✅ Infrastructure
            services.AddInfrastructure(configuration);

            // ✅ Controllers
            services.AddControllers();

            // ✅ Swagger
            services.AddEndpointsApiExplorer();

            return services;
        }
    }
}
