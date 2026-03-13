
namespace KeycloakGateway.Application.Interfaces
{
    public interface IClientService
    {
        Task<string?> GetRedirectUriAsync(string clientId);
    }
}
