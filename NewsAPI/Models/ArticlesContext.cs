using Microsoft.EntityFrameworkCore;

namespace NewsAPI.Models
{
    public class ArticlesContext : DbContext
    {
        public DbSet<Article> NewsItems { get; set; }
        public ArticlesContext(DbContextOptions<ArticlesContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
