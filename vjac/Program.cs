using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace vjac
{
    partial class Program
    {
        static IConfigurationRoot Config;
        const string HelpText = @"VJudge反作弊系统使用方法:

        ";

        static Contest con;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine(HelpText);
                return;
            }
            Config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>(){
                    {"scan_timeout","30000"},
                    {"action","start"},
                    {"contest",""},
                    {"sessionid",""},
                    {"jaxq",""},
                    {"ga","GA1.2.905820731.1539859305"},
                    {"similarity_limit","60"}
                })
                .AddCommandLine(args)
                .Build();
            if (Config["contest"] == "")
            {
                System.Console.WriteLine(HelpText);
                return;
            }
            con = new Contest(Config["contest"]);
            con.SimilarityLimit = int.Parse(Config["similarity_limit"]);
            con.SetCookies("", "", Config["jaxq"]);
            switch (Config["action"])
            {
                case "clear":
                    DeleteFolder($"{Config["contest"]}");
                    Directory.Delete($"{Config["contest"]}");
                    break;
                case "start":
                    if (File.Exists($"cookie"))
                    {
                        var cookie = File.ReadAllLines("cookie");
                        con.SetCookies("","",cookie[0]);
                    }
                    if (!con.ValidateCookie())
                    {
                        System.Console.WriteLine("提供的Cookie可能已经失效");
                        //return;
                    }
                    if (!Directory.Exists($"{Config["contest"]}"))
                    {
                        System.Console.WriteLine($"[{DateTime.Now}]正在下载题目与标程");
                        con.LoadProblemList();
                        con.DownloadSolutionsAsync().Wait();
                        con.RemoveSameSolution();
                        System.Console.WriteLine($"[{DateTime.Now}]下载结束");
                    }
                    System.Console.WriteLine("==========正在实时检查,按Ctrl+C结束===========");
                    Console.CancelKeyPress += (s, e) =>
                    {
                        System.Console.WriteLine($"[{DateTime.Now}]已停止程序");
                        e.Cancel = false;
                    };
                    con.LoadCheckedRID();
                    if (!File.Exists($"{con.ContestID}/result.md"))
                    {
                        File.WriteAllText($"{con.ContestID}/result.md",
$@"
# 比赛{con.ContestID}的作弊情况
|用户名|runID|题目编号|可能的来源|相似度|
|-|-|-|-|-|
");
                    }
                    var timer = new System.Timers.Timer();
                    timer.Interval = int.Parse(Config["scan_timeout"]);
                    timer.Elapsed += (s, arg) =>
                    {
                        var result = con.CheckNewStatus();
                        var sw = File.AppendText($"{con.ContestID}/result.md");
                        foreach (var i in result)
                        {
                            foreach (var j in i.PossibleSources)
                            {
                                sw.WriteLine($"|{i.Status.User}|{i.Status.RID}|{i.Status.Problem}|{j.Source}|{j.Similarity}|");
                            }
                            Console.ForegroundColor = ConsoleColor.Red;
                            System.Console.WriteLine($"[{DateTime.Now}]发现可能作弊:{i.Status.RID}");
                            Console.ResetColor();
                        }
                        sw.Close();
                    };
                    GC.KeepAlive(timer);
                    timer.Enabled = true;
                    timer.Start();
                    while (true)
                    {
                        Console.ReadKey();
                    }
                case "removesame":
                    if (File.Exists($"cookie"))
                    {
                        var cookie = File.ReadAllLines("cookie");
                        con.SetCookies("", "", cookie[0]);
                    }
                    if (!con.ValidateCookie())
                    {
                        System.Console.WriteLine("提供的Cookie可能已经失效");
                        //return;
                    }
                    if (Directory.Exists($"{Config["contest"]}"))
                    {
                        con.LoadProblemList();
                        con.RemoveSameSolution();
                    }
                    break;
                default:
                    System.Console.WriteLine("未知操作");
                    break;
            }
        }
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }
    }
}
