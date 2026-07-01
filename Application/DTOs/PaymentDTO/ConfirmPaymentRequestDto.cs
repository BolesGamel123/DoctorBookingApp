using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class ConfirmPaymentRequestDto
    {
        public long BookingId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string GatewayTransactionId { get; set; } = string.Empty;
        public string GatewayResponse { get; set; } = string.Empty;
    }
}
