using System;
using System.ComponentModel.DataAnnotations;
namespace NewsAPI.Models
{
    public class Article
    {
        [Required]
        public int Id { get; set; }
        [Required, Display(Name = "Заголовок")]
        public string Title { get; set; }
        [Required, Display(Name = "Содержание")]
        public string Text { get; set; }
        [Required, Display(Name = "Дата")]
        public DateTime Date { get; set; }
        [Required, Display(Name = "Ссылка")]
        public string Link { get; set; }
    }
}
