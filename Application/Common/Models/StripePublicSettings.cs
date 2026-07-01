namespace Application.Common.Models;


public sealed class StripePublicSettings
{
    public const string SectionName = "Stripe";
    public string PublishableKey { get; set; } = string.Empty;
}
