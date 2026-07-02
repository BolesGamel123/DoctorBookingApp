using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class Result
    {
        public static GeneicResult<T> Success<T>(T data)
        {
            return new GeneicResult<T>
            {
                IsSuccess = true,
                Message = "Process completed successfully.",
                Data = data
            };
        }

        public static GeneicResult<T> Success<T>(string? message)
        {
            return new GeneicResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = default
            };
        }

        public static GeneicResult<T> Failure<T>(string message)
        {
            return new GeneicResult<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }
    }
}
