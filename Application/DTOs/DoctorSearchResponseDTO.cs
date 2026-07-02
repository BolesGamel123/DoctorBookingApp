using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class DoctorSearchResponseDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Specialization { get; set; }

        public decimal Rating { get; set; }
    }
}
