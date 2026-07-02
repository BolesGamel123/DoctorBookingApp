using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class CreateSystemLogRequestDTO
    {
            public string actor_type { get; set; }

            public long? actor_id { get; set; }

            public string action { get; set; }

            public string? target_type { get; set; }

            public long? target_id { get; set; }

            public string? description { get; set; }

            public string? metadata { get; set; }
    }
}
