namespace Application.Features.Payments.DTOs;
public sealed class PaymentIntentResponse
{
    public string  ClientSecret    { get; init; } = default!;
    public string  PublishableKey  { get; init; } = default!;
    public string  PaymentIntentId { get; init; } = default!;
    public decimal Amount          { get; init; }
    public string  Currency        { get; init; } = default!;
    public string  BookingReference{ get; init; } = default!;
}
