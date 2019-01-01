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
        const int SIMILARITY_LIMIT = 80;
        public Contest(string id)
        {
            ContestID = id;
            VJ.ContestID = id;
        }

        public void SetCookies(string a, string b, string c)
        {
            VJ.InitCookies(a, b, c);
        }

        public async Task LoadProblemListAsync()
        {
            Directory.CreateDirectory(ContestID);
            List<string> prob = await VJ.GetProblemListAsync();
            Console.WriteLine($"{prob.Count} problems found.");
            for (int i = 0; i < prob.Count; i++)
            {
                ProblemID.Add((char)('A' + i), prob[i]);
            }
        }

        public async void DownloadSolutionsAsync()
        {
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

        List<string> GetAllFileNames(string path)
        {
            DirectoryInfo dinf = new DirectoryInfo(path);
            var list = new List<string>();
            foreach (var i in dinf.GetFiles())
            {
                list.Add(i.Name);
            }
            return list;
        }

        public void RemoveSameSolution()
        {
            System.Console.WriteLine("正在检查重复文件");
            foreach (var i in ProblemID)
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "sim_c++";
                proc.StartInfo.WorkingDirectory = $"{ContestID}/{i.Key}";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //proc.StartInfo.Arguments = $"-p -t 100";

                StringBuilder args = new StringBuilder("-p -t 100 ");
                args.AppendJoin(' ', GetAllFileNames($"{ContestID}/{i.Key}"));
                proc.StartInfo.Arguments = args.ToString();

                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                proc.WaitForExit();
                string result = proc.StandardOutput.ReadToEnd();
                //Console.WriteLine(result);
                Regex reg = new Regex("([0-9]+).cpp consists for 100 % of ([0-9]+).cpp material");
                var match = reg.Matches(result);
                foreach (Match j in match)
                {
                    Console.WriteLine($"重复的文件:{j.Groups[1]},{j.Groups[2]}");
                    if (File.Exists($"{ContestID}/{i.Key}/{j.Groups[1]}.cpp") && File.Exists($"{ContestID}/{i.Key}/{j.Groups[2]}.cpp"))
                    {
                        File.Delete($"{ContestID}/{i.Key}/{j.Groups[1]}.cpp");
                    }
                }
            }
            System.Console.WriteLine("检查完毕");
        }

        public (bool, string) CheckCodeAsync(string ProblemID, string Code, string RID)
        {
            System.Console.WriteLine($"正在检查{RID}号程序");
            File.WriteAllText($"{ContestID}/{ProblemID}/{RID}.cpp", $"//{RID}\n" + Code);

            Process proc = new Process();
            proc.StartInfo.FileName = "sim_c++";
            proc.StartInfo.WorkingDirectory = $"{ContestID}/{ProblemID}";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //proc.StartInfo.Arguments = $"-p -t 100";

            StringBuilder args = new StringBuilder($"-p -t {SIMILARITY_LIMIT} ");
            args.AppendJoin(' ', GetAllFileNames($"{ContestID}/{ProblemID}"));
            proc.StartInfo.Arguments = args.ToString();

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();
            string result = proc.StandardOutput.ReadToEnd();
            //Console.WriteLine(result);
            Regex reg = new Regex("([0-9]+).cpp consists for ([0-9]+) % of ([0-9]+).cpp material");
            var match = reg.Matches(result);
            bool isCheat = false;
            string msg = "";
            foreach (Match j in match)
            {
                if (j.Groups[1].ToString() == RID)
                {
                    isCheat = true;
                    string source = File.ReadAllLines($"{ContestID}/{ProblemID}/{j.Groups[3]}.cpp")[0].Substring(2);
                    msg += $"与来自{source}的答案有{j.Groups[2]}的相似度\n";
                }
                if (j.Groups[3].ToString() == RID)
                {
                    isCheat = true;
                    string source = File.ReadAllLines($"{ContestID}/{ProblemID}/{j.Groups[1]}.cpp")[0].Substring(2);
                    msg += $"与来自{source}的答案有{j.Groups[2]}的相似度\n";
                }
            }
            return (isCheat, msg);
        }
    }
}