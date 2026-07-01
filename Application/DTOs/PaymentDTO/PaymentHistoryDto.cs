using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PaymentDTO
{
    public class PaymentHistoryDto
    {
        public long PaymentId { get; set; }
        public long BookingId { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal DoctorEarnings { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
