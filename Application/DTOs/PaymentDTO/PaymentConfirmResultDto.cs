using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class PaymentConfirmResultDto
    {
        public long PaymentId { get; set; }
        public long BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
