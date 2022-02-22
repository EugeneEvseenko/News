using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Xml.XPath;

namespace News_Parser
{
    internal class Program
    {
        static SqlConnection con = Helper.ConnectToDB(true);
        static void Main(string[] args)
        {
            string menu = "================================\n" +
                        "1 - Парсинг\n" +
                        "2 - Очиста таблицы в БД\n" +
                        "0 - Выход\n" +
                        "================================";
            Print("Подключение к SQLEXPRESS...", ConsoleColor.Yellow);
            CheckConnection(con);
            con.InfoMessage += Con_InfoMessage;
            SqlCommand sqlCom = new("IF db_id('NewsDB') IS NULL " +
                                "PRINT 'db not exist' " +
                                "ELSE " +
                                "PRINT 'db exist';", con);

            sqlCom.ExecuteNonQuery();
            sqlCom = new("IF OBJECT_ID(N'NewsItems','U') IS NULL " +
                "PRINT 'table not exist' " +
                "ELSE " +
                "PRINT 'table exist';", con);
            sqlCom.ExecuteNonQuery();
            
            
            while (true)
            {
                Console.WriteLine(menu);
                switch (Console.ReadLine())
                {
                    case "1":
                        {
                            GoParse();
                        }
                        break;
                    case "2":
                        {
                            if (CheckConnection(con, true))
                            {
                                SqlCommand sqlCommand = new("DELETE FROM NewsItems;", con);
                                int CountDelete = sqlCommand.ExecuteNonQuery();
                                if(CountDelete > 0)
                                    Print($"Удалено элементов: {CountDelete}", ConsoleColor.Green);
                                else
                                    Print($"Таблица пуста.", ConsoleColor.Red);
                            }
                        }
                        break;
                    case "0": return;
                    default:
                        Print("Неизвестная команда.", ConsoleColor.Red);
                        break;
                }
            }
        }
        /// <summary>
        /// Обработчик InfoMessage для SqlConnection
        /// </summary>
        private static void Con_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            switch (e.Message)
            {
                case "db not exist":
                    {
                        SqlCommand sqlComm = new("CREATE DATABASE NewsDB;", con);
                        sqlComm.ExecuteNonQuery();
                        con.Close();
                        con = Helper.ConnectToDB();
                        con.InfoMessage += Con_InfoMessage;
                        Print("База данных не существует, но я её уже создал.", ConsoleColor.Red);
                    }
                    break;
                case "db exist":
                    {
                        Print("База уже существует, сейчас подключусь к ней.", ConsoleColor.Green);
                        con.Close();
                        con = Helper.ConnectToDB();
                        con.InfoMessage += Con_InfoMessage;
                    }
                    break;
                case "table not exist":
                    {
                        SqlCommand sql = new("CREATE TABLE NewsItems (" +
                            "[id][int] IDENTITY(1, 1) NOT NULL, " +
                            "[Title][varchar](200) NOT NULL, " +
                            "[Link][varchar](max) NOT NULL, " +
                            "[Text][text] NOT NULL, " +
                            "[Date][datetime] NOT NULL, " +
                            "PRIMARY KEY(id))", con);
                        sql.ExecuteNonQuery();
                        Print("Таблица не существует, но она уже создана.", ConsoleColor.Red);
                    }break;
                case "table exist": Print("Таблица существует, можно приступать.", ConsoleColor.Green);break;
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
        /// Проверка соединения с базой данных.
        /// </summary>
        /// <param name="con">SqlConnection, представляющее соединение с Sql Server.</param>
        /// <param name="silence">Флаг отвечающий за вывод информации о подключении к терминал.</param>
        /// <returns>Возвращает флаг состояния подключения к БД.</returns>
        public static bool CheckConnection(SqlConnection con, bool silence = false)
        {
            if (con.State != ConnectionState.Open)
            {
                if (!silence)
                    Print("Произошла ошибка подключения к базе данных.", ConsoleColor.Red);
                return false;
            }
            else
            {
                if (!silence)
                    Print("Соединение с БД установлено.", ConsoleColor.Green);
                return true;
            }
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
                    if (CheckConnection(con))
                    {
                        foreach (var article in ArticlesList)
                        {
                            string command = "INSERT INTO NewsItems (Title,Link,Text,Date) VALUES (@Title,@Link,@Text,@Date)";
                            using (SqlCommand queryCommand = new(command, con))
                            {
                                queryCommand.Parameters.Add("@Title", SqlDbType.VarChar, 200).Value = article.Title;
                                queryCommand.Parameters.Add("@Link", SqlDbType.VarChar, int.MaxValue).Value = article.Link;
                                queryCommand.Parameters.Add("@Text", SqlDbType.Text).Value = article.Text;
                                queryCommand.Parameters.Add("@Date", SqlDbType.DateTime).Value = article.Date;
                                queryCommand.ExecuteNonQuery();
                            }
                            Console.WriteLine(article.ToString());
                        }
                    }
                } else {
                    Print($"Статьи на странице {parser.Referer} не найдены", ConsoleColor.Red);
                }
            }
        }
    }
}
