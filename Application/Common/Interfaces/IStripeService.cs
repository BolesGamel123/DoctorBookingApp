using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IStripeService
{
  
    Task<(string ClientSecret, string PaymentIntentId)> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string bookingReference,
        CancellationToken ct = default);

    Task<bool> RefundPaymentAsync(
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken ct = default);
}
