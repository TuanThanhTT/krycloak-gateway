using Microsoft.Extensions.DependencyInjection;

namespace KeycloakGateway.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Đăng ký các service của Application layer
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // 🔹 Nếu sau này dùng MediatR thì mở dòng này
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // 🔹 Nếu sau này dùng FluentValidation
            // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // 🔹 Hiện tại Application chỉ chứa contracts nên chưa cần register gì thêm

            return services;
        }
    }
}
