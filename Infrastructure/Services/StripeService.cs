using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Services;

public sealed class StripeService : IStripeService
{
    public StripeService(IOptions<StripeSettings> options)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public async Task<(string ClientSecret, string PaymentIntentId)> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingReference,
        CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount             = (long)(amount * 100), 
            Currency           = currency.ToLowerInvariant(),
            PaymentMethodTypes = new List<string> { "card" },
            Metadata           = new Dictionary<string, string>
            {
                { "booking_reference", bookingReference }
            }
        };

        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options, cancellationToken: ct);

        return (intent.ClientSecret, intent.Id);
    }

    public async Task<bool> RefundPaymentAsync(
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken ct = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount        = (long)(amount * 100),
            Reason        = reason
        };

        var service = new RefundService();
        var refund  = await service.CreateAsync(options, cancellationToken: ct);

        return refund.Status == "succeeded";
    }
}
