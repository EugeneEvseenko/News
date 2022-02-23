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
    public class ToptenController : ControllerBase
    {
        ArticlesContext db;
        public ToptenController(ArticlesContext articlesContext)
        {
            db = articlesContext;
        }
        // GET: api/topten
        [HttpGet]
        public ActionResult<string> Get()
        {
            if (db.NewsItems.Any())
            {
                var result = db.NewsItems
                .ToList()
                .Select(x => x.Text.Split())
                .SelectMany(s => s)
                .Select(prep => Regex.Replace(prep, "[-.?!)(,:]", ""))
                .Where(word => word.Length > 3)
                .GroupBy(g => g)
                .Where(w => w.Count() > 1)
                .OrderByDescending(o => o.Count())
                .Take(10)
                .Select(ss => new { item = ss.Distinct().ToList()[0], count = ss.Count() });
                return new OkObjectResult(
                        result
                    );
            }
            else
            {
                return NoContent();
            }
        }
    }
}
