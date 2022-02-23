using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace NewsAPI.Models
{
    public class Response
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
    public class DataResponse : Response
    {
        public int ItemsCount { get; set; }
        public List<Article> News { get; set; }
    }
}
