using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class FavoriteResponseDTO
    {
        public long id { get; set; }

        public long doctor_id { get; set; }

        public string DoctorName { get; set; }

        public DateTime created_at { get; set; }
    }
}
