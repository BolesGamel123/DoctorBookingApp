using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class GeneicResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
