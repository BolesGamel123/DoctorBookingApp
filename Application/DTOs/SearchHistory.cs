using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class SearcHistoryDTO
    {
        public long id { get; set; }

        public string SearchText { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
