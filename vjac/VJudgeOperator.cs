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
    public class VJudgeOperator
    {
        CookieContainer cookies = new CookieContainer();
        public string ContestID { get; set; }
        const string VJ_URL = "https://vjudge.net";
        const string VJ_DOMAIN = "vjudge.net";
        const string VJ_CONTEST_STATUS_URL = "https://vjudge.net/status/data/";
        const string VJ_STATUS_URL = "https://vjudge.net/solution/data/{0}";
        const string VJ_STATUS_FORM_URL = "draw=3&columns%5B0%5D%5Bdata%5D=0&columns%5B0%5D%5Bname%5D=&columns%5B0%5D%5Bsearchable%5D=true&columns%5B0%5D%5Borderable%5D=false&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B1%5D%5Bdata%5D=1&columns%5B1%5D%5Bname%5D=&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5Borderable%5D=false&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5Bdata%5D=2&columns%5B2%5D%5Bname%5D=&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=false&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=3&columns%5B3%5D%5Bname%5D=&columns%5B3%5D%5Bsearchable%5D=true&columns%5B3%5D%5Borderable%5D=false&columns%5B3%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=4&columns%5B4%5D%5Bname%5D=&columns%5B4%5D%5Bsearchable%5D=true&columns%5B4%5D%5Borderable%5D=false&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B4%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B5%5D%5Bdata%5D=5&columns%5B5%5D%5Bname%5D=&columns%5B5%5D%5Bsearchable%5D=true&columns%5B5%5D%5Borderable%5D=false&columns%5B5%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B5%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B6%5D%5Bdata%5D=6&columns%5B6%5D%5Bname%5D=&columns%5B6%5D%5Bsearchable%5D=true&columns%5B6%5D%5Borderable%5D=false&columns%5B6%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B6%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B7%5D%5Bdata%5D=7&columns%5B7%5D%5Bname%5D=&columns%5B7%5D%5Bsearchable%5D=true&columns%5B7%5D%5Borderable%5D=false&columns%5B7%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B7%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B8%5D%5Bdata%5D=8&columns%5B8%5D%5Bname%5D=&columns%5B8%5D%5Bsearchable%5D=true&columns%5B8%5D%5Borderable%5D=false&columns%5B8%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B8%5D%5Bsearch%5D%5Bregex%5D=false&start=0&length=20&search%5Bvalue%5D=&search%5Bregex%5D=false&un={0}&num=-&res=0&language=&inContest=true&contestId={1}";
        const string VJ_CONTEST_URL = "https://vjudge.net/contest/{0}";
        const string PROBLEM_ID_MATCHER = "<a href=\"/problem/[0-9]*/origin\"\n\t(.*?)\n\t(.+)";
        public void InitCookies(string JSSESSIONID, string ga, string JAXQ)
        {
            cookies = new CookieContainer();
            cookies.Add(new Uri(VJ_URL), new Cookie()
            {
                Name = "JSESSIONID",
                Domain = VJ_DOMAIN,
                Value = JSSESSIONID,
            });
            cookies.Add(new Uri(VJ_URL), new Cookie()
            {
                Name = "_ga",
                Domain = VJ_DOMAIN,
                Value = ga,
            });
            cookies.Add(new Uri(VJ_URL), new Cookie()
            {
                Name = "Jax.Q",
                Domain = VJ_DOMAIN,
                Value = JAXQ,
            });
        }

        public async Task<string> GetStatusStringAsync(string UserName)
        {
            AJAXCrawler crawler = new AJAXCrawler();
            crawler.Url = VJ_CONTEST_STATUS_URL;
            crawler.Content = string.Format(VJ_STATUS_FORM_URL, UserName, ContestID);
            crawler.Cookies = cookies;
            return await crawler.PostFormAsync();
        }

        public async Task<List<Status>> GetStatusAsync(string UserName)
        {
            string orig = await GetStatusStringAsync(UserName);
            JsonObject obj = JsonObject.Parse(orig) as JsonObject;
            List<Status> list = new List<Status>();
            foreach (JsonValue i in obj["data"])
            {
                var log = i as JsonObject;
                if (log["statusType"].ToString() == "0")
                {
                    var s = new Status()
                    {
                        RID = log["runId"].ToString(),
                        User = log["userName"].ToString(),
                        Problem = log["contestNum"].ToString(),
                    };
                    list.Add(s);
                }

            }
            return list;
        }

        public async Task<string> GetCodeAsync(string RID)
        {
            AJAXCrawler crawler = new AJAXCrawler();
            crawler.Url = string.Format(VJ_STATUS_URL, RID);
            crawler.Cookies = cookies;
            string orig = await crawler.PostAsync();
            JsonObject obj = JsonArray.Parse(orig) as JsonObject;
            return Regex.Unescape(obj["code"].ToString());
        }

        public async Task<List<string>> GetProblemListAsync()
        {
            AJAXCrawler crawler = new AJAXCrawler();
            crawler.Url = string.Format(VJ_CONTEST_URL, ContestID);
            crawler.Cookies = cookies;
            string page = await crawler.GetAsync();
            var parser = new HtmlParser();
            var document = await parser.ParseAsync(page);
            var list = new List<string>();
            foreach (var i in document.QuerySelectorAll("td.prob-origin > a"))
            {
                list.Add(i.InnerHtml.Trim());
            }
            return list;
        }
    }
}