using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class SystemLogsRequestDTO
    {
        public string? ActorType { get; set; } //. Doctor/ Admin/ Patient 

        public long? ActorId { get; set; }

        public string? Action { get; set; }

        public string? TargetType { get; set; }

        public long? TargetId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; }
    }
}
