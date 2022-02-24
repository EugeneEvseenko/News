using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using News_Parser.Classes;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace News_Parser
{
    public class Program
    {
        static ApplicationContext db = new();
        public static void Main(string[] args)
        {
            string menu = "================================\n" +
                        "1 - Парсинг\n" +
                        "2 - Вывод элементов из БД\n" +
                        "3 - Очиста таблицы в БД\n" +
                        "0 - Выход\n" +
                        "================================";
            while (true)
            {
                Console.WriteLine(menu);
                switch (Console.ReadLine())
                {
                    case "1":
                        GoParse(); break;
                    case "2":
                        if (db.NewsItems.Any())
                        {
                            Print($"Найдено элементов в БД: {db.NewsItems.Count()}", ConsoleColor.Cyan);
                            foreach (var item in db.NewsItems)
                            {
                                Print(item.ToString(), ConsoleColor.Yellow);
                            }
                        }
                        else
                            Print($"Таблица пуста.", ConsoleColor.Red);
                        break;
                    case "3":
                        if (db.NewsItems.Any())
                        {
                            int CountDelete = db.NewsItems.Count();
                            if (CountDelete > 0)
                            {
                                db.Database.ExecuteSqlRaw("TRUNCATE TABLE NewsItems;");
                                Print($"Удалено элементов: {CountDelete}", ConsoleColor.Green);
                            }
                        }else
                            Print($"Таблица пуста.", ConsoleColor.Red);
                        break;
                    case "0": return;
                    default:
                        Print("Неизвестная команда.", ConsoleColor.Red);
                        break;
                }
            }
        }

        /// <summary>
        /// Упрощенный вывод строки в терминал с выбором цвета
        /// </summary>
        /// <param name="line">Строка для вывода.</param>
        /// <param name="color">Цвет строки.</param>
        /// <param name="lineBreak">Перенос строки. По умолчанию включен.</param>
        public static void Print(string line, ConsoleColor color, bool lineBreak = true)
        {
            Console.ForegroundColor = color;
            if (lineBreak)
                Console.WriteLine(line);
            else
                Console.Write(line);
            Console.ForegroundColor = ConsoleColor.White;
        }
        /// <summary>
        /// Функция парсинга новостной страницы.
        /// </summary>
        public static void GoParse()
        {
            string[] errorsArr = { 
                "Это точно число?", 
                "Всё фигня Миша, давай по новой!",
                "Надо попробовать ещё разок.",
                "Буквы вводить нельзя.",
                "Нужно просто число."
            };
            int countCard;
            while (true)
            {
                Console.Write("Какое количество статей необходимо спарсить? - ");
                try
                {
                    countCard = int.Parse(Console.ReadLine());
                    if (countCard > 0)
                        break;
                    else Print("Число должно быть больше нуля.", ConsoleColor.Red);
                }
                catch
                {
                    Print(errorsArr[new Random().Next(0, errorsArr.Length)], ConsoleColor.Red);
                }
            }
            var htmlDoc = new HtmlDocument();
            var cookieContainer = new CookieContainer();
            var parser = new Parser("https://www.zakon.kz/")
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                Useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.174 YaBrowser/22.1.3.848 Yowser/2.5 Safari/537.36",
                Referer = "https://www.zakon.kz/",
                Host = "www.zakon.kz"
            };
            parser.Parse(cookieContainer);
            if (parser.Response != null)
            {
                htmlDoc.LoadHtml(parser.Response);
                var articles = htmlDoc.DocumentNode.SelectNodes($"(//div[contains(@class,'zmainCard_item')])[position() < {countCard + 1}]");
                Print($"Найдено статей на странице: {articles.Count}", ConsoleColor.Yellow);
                List<Article> ArticlesList = new();
                foreach (var article in articles)
                {
                    try
                    {
                        string articleURL = article.SelectSingleNode(".//a").Attributes["href"].Value;
                        Print($"[{articles.IndexOf(article) + 1}] Парсинг {articleURL}", ConsoleColor.Yellow);
                        parser._url = "https://www.zakon.kz/" + articleURL;
                        parser.Parse(cookieContainer);
                        if (parser.Response != null)
                        {
                            var articleHTML = new HtmlDocument();
                            articleHTML.LoadHtml(parser.Response);
                            var fullArticle = articleHTML.DocumentNode.SelectSingleNode("//section[@id='article']//div[@class='articleBlock']");
                            string date = fullArticle.SelectSingleNode(".//div[@class='date']").InnerText.Trim();
                            string text = fullArticle.SelectSingleNode(".//div[@class='description']").InnerText.Trim();
                            text += "\n" + fullArticle.SelectSingleNode(".//div[@class='content']").InnerText.Trim();
                            if (text.Contains("Новость по теме")) text = text.Remove(text.LastIndexOf("Новость по теме"));
                            ArticlesList.Add(new Article
                            {
                                Title = article.SelectSingleNode(".//div[contains(@class,'title')]").InnerText.Trim(),
                                Link = article.SelectSingleNode(".//a").Attributes["href"].Value,
                                Date = DateTime.Parse(date),
                                Text = text
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Print(ex.Message, ConsoleColor.Red);
                    }
                }
                if(ArticlesList.Count > 0)
                {
                    foreach (var article in ArticlesList)
                    {
                        db.NewsItems.Add(article);
                        Console.WriteLine(article.ToString());
                    }
                    db.SaveChanges();
                } else {
                    Print($"Статьи на странице {parser.Referer} не найдены", ConsoleColor.Red);
                }
            }
        }
    }
}
