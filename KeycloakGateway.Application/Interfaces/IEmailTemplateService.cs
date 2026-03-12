using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeycloakGateway.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        string RenderTemplate(string templateName, Dictionary<string, string> data);
    }
}
