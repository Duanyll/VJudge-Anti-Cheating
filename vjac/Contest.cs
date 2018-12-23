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
    public class Contest
    {
        VJudgeOperator VJ = new VJudgeOperator();
        Dictionary<char, string> ProblemID = new Dictionary<char, string>();
        public readonly string ContestID;
        const int MAX_SOLUTION_COUNT = 100;
        public Contest(string id)
        {
            ContestID = id;
            VJ.ContestID = id;
        }

        public void SetCookies(string a, string b, string c)
        {
            VJ.InitCookies(a, b, c);
        }

        public async void LoadProblems()
        {
            Directory.CreateDirectory(ContestID);
            List<string> prob = await VJ.GetProblemListAsync();
            Console.WriteLine($"{prob.Count} problems found.");
            for (int i = 0; i < prob.Count; i++)
            {
                ProblemID.Add((char)('A' + i), prob[i]);
            }
            foreach (var i in ProblemID)
            {
                Directory.CreateDirectory($"{ContestID}/{i.Key}");
                SolutionCrawler crawler = new SolutionCrawler();
                crawler.ProblemID = i.Value;
                List<string> urls = await crawler.GetBaiduSearchResult(MAX_SOLUTION_COUNT);
                int solID = 1;
                foreach (var j in urls)
                {
                    if (File.Exists($"{ContestID}/{i.Key}/{solID}.cpp"))
                    {
                        solID++;
                        continue;
                    }
                    List<string> code = await crawler.GetCodeAsync(j);
                    foreach (var k in code)
                    {
                        File.WriteAllText($"{ContestID}/{i.Key}/{solID++}.cpp", $"//{j}\n" + k);
                    }
                    Console.WriteLine($"Code from ${j} saved.");
                    Thread.Sleep(500);
                }
            }
        }
    }
}