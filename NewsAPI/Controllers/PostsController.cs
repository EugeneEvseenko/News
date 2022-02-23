using Microsoft.AspNetCore.Mvc;
using NewsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewsAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        ArticlesContext db;
        public PostsController(ArticlesContext articlesContext)
        {
            db = articlesContext;
        }
        // GET: api/posts?from={}&to={}
        [HttpGet]
        public ActionResult<string> Get(string from, string to)
        {
            if (db.NewsItems.Any())
            {
                if (from == null || to == null)
                {
                    return BadRequest(new Response
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Неправильный запрос."
                    });
                }
                DateTime fromTime = DateTime.Parse(from);
                DateTime toTime = DateTime.Parse(to);
                if (fromTime > toTime)
                    return BadRequest(new Response
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Возможно были перепутаны параметры? Параметр [from] не может быть старше [to]."
                    });
                var outList = new List<Article>(db.NewsItems.Where(a => a.Date >= fromTime && a.Date <= toTime));
                return new OkObjectResult(
                        new DataResponse
                        {
                            Status = StatusCodes.Status200OK,
                            ItemsCount = outList.Count,
                            News = outList
                        }
                    );
            }
            else
            {
                return NoContent();
            }
        }
    }
}
