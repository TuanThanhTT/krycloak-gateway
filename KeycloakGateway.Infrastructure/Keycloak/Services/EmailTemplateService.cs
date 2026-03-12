using KeycloakGateway.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KeycloakGateway.Infrastructure.Keycloak.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IConfiguration _config;

        public EmailTemplateService(IConfiguration config)
        {
            _config = config;
        }

        public string RenderTemplate(string templateName, Dictionary<string, string> data)
        {
            var path = _config[$"EmailTemplates:{templateName}"];

            if (string.IsNullOrEmpty(path))
                throw new Exception($"Template config not found: {templateName}");

            var fullPath = Path.Combine(AppContext.BaseDirectory, path);

            if (!File.Exists(fullPath))
                throw new Exception($"Template file not found: {fullPath}");

            var html = File.ReadAllText(fullPath);

            foreach (var item in data)
            {
                html = html.Replace($"{{{item.Key}}}", item.Value);
            }

            return html;
        }

    }
}
