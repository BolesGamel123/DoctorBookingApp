using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class PaymentHistoryQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
    }
}
