using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Application.DTOs
{
    public class DoctorSearchDTO
    {
        public string? Name { get; set; }

        public int? SpecializationId { get; set; }
    }
}
