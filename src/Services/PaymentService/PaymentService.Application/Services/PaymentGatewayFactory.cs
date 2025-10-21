using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Application.Services;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string provider);
}

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGateway GetGateway(string provider)
    {
        var gateways = _serviceProvider.GetServices<IPaymentGateway>();
        
        var gateway = gateways.FirstOrDefault(g => 
            g.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));

        if (gateway == null)
        {
            throw new NotSupportedException($"Payment provider '{provider}' is not supported");
        }

        return gateway;
    }
}

