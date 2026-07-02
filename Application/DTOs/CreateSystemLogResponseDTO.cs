using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class CreateSystemLogResponseDTO
    {
        public long id { get; set; }

        public string actor_type { get; set; }

        public long? actor_id { get; set; }

        public string action { get; set; }

        public string? target_type { get; set; }

        public long? target_id { get; set; }

        public string? description { get; set; }

        public string? metadata { get; set; }

        public string ip_address { get; set; }

        public DateTime created_at { get; set; }
    }
}
