using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Json;
using System.Text.RegularExpressions;
using AngleSharp.Parser.Html;


namespace vjac
{
    public class SolutionCrawler
    {
        public string ProblemID { get; set; }
        const string BD_SEARCH = "https://www.baidu.com/s?ie=UTF-8&wd={0}&pn={1}";
        public async Task<List<string>> GetBaiduSearchResult(int MaxResult)
        {
            int pn = 0;
            int OKCount = 0;
            List<string> result = new List<string>();
            string pid = ProblemID.Split(' ')[1];
            System.Console.WriteLine($"[{DateTime.Now}]正在百度搜索题目{ProblemID}");
            do
            {
                OKCount = 0;
                AJAXCrawler crawler = new AJAXCrawler();
                crawler.Url = string.Format(BD_SEARCH, ProblemID.Replace(" ", "%20"), pn);
                string page = await crawler.GetAsync();
                var parser = new HtmlParser();
                var document = await parser.ParseAsync(page);
                foreach (var i in document.QuerySelectorAll("h3.t > a"))
                {
                    if (i.InnerHtml.Contains(pid.ToUpper()) || i.InnerHtml.Contains(pid.ToLower()))
                    {
                        result.Add(i.GetAttribute("href"));
                        OKCount++;
                    }
                }
                pn += 10;
                Console.WriteLine($"[{DateTime.Now}]前{pn}个结果已抓取完成.");
                Thread.Sleep(1000);
            } while (OKCount >= 8 && pn <= MaxResult);
            return result;
        }
        public async Task<(List<string> code,string src)> GetCodeAsync(string Url)
        {
            AJAXCrawler crawler = new AJAXCrawler();
            crawler.Url = Url;
            string page = await crawler.GetAsync();
            var parser = new HtmlParser();
            var document = await parser.ParseAsync(page);
            List<string> result = new List<string>();
            foreach (var i in document.QuerySelectorAll("pre"))
            {
                string code = i.InnerHtml;
                Regex removeLineNumber = new Regex("<span style=\"color: #008080\">(.+?)</span>");
                //处理诸如http://www.cnblogs.com/npugen/p/9658756.html的行号格式
                code = removeLineNumber.Replace(code, "");
                Regex reg = new Regex(@"<[^>]+>|</[^>]+>");
                code = reg.Replace(code, "");
                code = code.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&quot;","\"");
                if (code.Contains("#include"))
                {
                    result.Add(code);
                }
            }
            return (result,crawler.Url);
        }
    }
}