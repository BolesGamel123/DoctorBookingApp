using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class PaginatedResult<T>
    {
        //. static property meaning that the property will should be called using the name of the class 
        public bool IsSuccess { get; set; } = true;
        public string? message { get; set; }
        public List<T> Data { get; set; }
        public PaginationMetadata pagination { get; set; }
        public int? TotalCount { get; set; }
    }
}
