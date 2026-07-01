using System.Threading.Tasks;
using Application.DTOs.PaymentDTO;

namespace Application.Interfaces;

public interface IPaymentService
{
    Task<CheckoutSessionResultDto> CreateCheckoutSessionAsync(CreateCheckoutSessionRequestDto request, long currentUserId);
    Task<PaymentConfirmResultDto> ConfirmPaymentAsync(ConfirmPaymentRequestDto request, long currentUserId);
    Task<PaymentConfirmResultDto> ProcessStripeWebhookAsync(string jsonPayload, string stripeSignatureHeader);
    Task<RefundResultDto> RefundPaymentAsync(RefundRequestDto request, long currentUserId, string currentUserRole);
    Task<PagedResultDto<PaymentHistoryDto>> GetPaymentHistoryAsync(PaymentHistoryQueryDto query, long currentUserId, string currentUserRole);
}
