using System;
using System.Collections.Generic;

namespace Application.DTOs.PaymentDTO;

public class CreateCheckoutSessionRequestDto
{
    public long BookingId { get; set; }
    public bool UseStripe { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}









