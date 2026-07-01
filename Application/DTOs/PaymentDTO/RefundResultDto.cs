using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class RefundResultDto
    {
        public long PaymentId { get; set; }
        public decimal RefundedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RefundedAt { get; set; }
    }
}
