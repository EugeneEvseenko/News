using Microsoft.AspNetCore.Mvc;
using NewsAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace NewsAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        ArticlesContext db;
        public SearchController(ArticlesContext articlesContext)
        {
            db = articlesContext;
        }
        // GET: api/posts?from={}&to={}
        [HttpGet]
        public ActionResult<string> Get(string text)
        {
            if (db.NewsItems.Any())
            {
                if (text == null)
                {
                    return BadRequest(new Response
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Неправильный запрос."
                    });
                }
                else
                {
                    if (text.Trim().Length == 0)
                    {
                        return BadRequest(new Response
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Message = "Неправильный запрос."
                        });
                    }
                    else
                    {
                        var result = db.NewsItems
                            .ToList()
                            .Select(s => s)
                            .Where(w => w.Text.ToLower().Contains(text.ToLower()) || w.Title.ToLower().Contains(text.ToLower()));
                        return new OkObjectResult(result);
                    }
                }
            }
            else
            {
                return NoContent();
            }
        }
    }
}
