using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public bool HasNextPage => CurrentPage < TotalPages; //. true --> has next page, false --> has not next page 
        public bool HasPreviousPage => CurrentPage > 1; //. true --> the current page is greater than 1 and has a previous page 
    }
}
