using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class RefundRequestDto
    {
        public long PaymentId { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
    }
}
