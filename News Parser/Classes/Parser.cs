using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace News_Parser
{
    public class Parser
    {
        public string _url;
        public HttpWebRequest _request;
        public string Accept { get; set; }
        public string Host { get; set; }
        public string Referer { get; set; }
        public string Useragent { get; set; }
        public string Response { get; set; }
        /// <summary>
        /// Объект парсера
        /// </summary>
        /// <param name="url">Ссылка на страницу для парсинга.</param>
        public Parser (string url)
        {
            _url = url;
        }
        public void Parse(CookieContainer cookieContainer)
        {
            _request = (HttpWebRequest)WebRequest.Create(_url);
            _request.Method = "Get";
            _request.CookieContainer = cookieContainer;
            _request.Accept = Accept;
            _request.Host = Host;
            _request.Referer = Referer;
            _request.UserAgent = Useragent;

            _request.Headers.Add("sec-ch-ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Yandex\";v=\"22\"");
            _request.Headers.Add("sec-ch-ua-mobile", "?0");
            _request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            _request.Headers.Add("sec-fetch-dest", "document");
            _request.Headers.Add("sec-fetch-mode", "navigate");
            _request.Headers.Add("sec-fetch-site", "same-origin");
            _request.Headers.Add("sec-fetch-user", "?1");
            _request.Headers.Add("upgrade-insecure-requests", "1");

            try
            {
                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null) Response = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
