using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Parser
{
    public class Article
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public override string ToString()
        {
            return $"{Title} - {Date}\n[https://www.zakon.kz/{Link}]\n";
        }
    }
}
