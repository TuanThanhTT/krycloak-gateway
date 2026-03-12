using KeycloakGateway.Application.DTOs.Email;


namespace KeycloakGateway.Application.Interfaces
{
    public interface IEmailProducer
    {
        Task PublishAsync(EmailMessage message);
    }
}
