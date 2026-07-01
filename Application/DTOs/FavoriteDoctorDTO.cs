using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class FavoriteDoctorDTO
    {
        public long DoctorId { get; set; }

        public string DoctorName { get; set; }

        public string Specialization { get; set; }

        public int Rating { get; set; }
        public decimal Rating_AVG { get; set; }

        public string ImageUrl { get; set; }
    }
}
