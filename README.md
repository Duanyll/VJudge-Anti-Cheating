# VJudge-Anti-Cheating

基于SIM的vjudge比赛反抄袭工具，使用C#+dotnet core 2.1编写

原理和行为：使用百度搜索并下载标称，使用SIM工具比较选手程序与标称

## 编译
1. [下载](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)并安装.NET Core SDK(Windows,Linux,macOS)
2. Windows用户可以[下载](https://dickgrune.com/Programs/similarity_tester/)SIM工具并添加到Path，Ubuntu用户可以`sudo apt install similarity_tester`,其他发行版用户可以自行编译SIM
3. 
```sh
git clone https://github.com/Duanyll/VJudge-Anti-Cheating
cd VJudge-Anti-Cheating/vjac
dotnet build
```
## 运行
命令行参数选项：

|参数(\*可选)|                   作用                        |
|-----------|-----------------------------------------------|
|contest    |指定比赛编号|
|scan_timeout\*|两次扫描新提交记录之间间隔多少毫秒，建议不少于30000|
|action\*     |start（默认选项搜索标称并开始检查）或者clean（清除比赛数据）|
|similarity_limit*|应该将与标称相似度高于百分之多少视为作弊，默认60|
|ga\*    |VJudge的一项cookie，默认是`GA1.2.905820731.1539859305`,可不指定|
|jaxq    |VJudge的一项cookie，若不在命令行中指定可以保存在`vjac/cookie`文件中，该文件优先级更高|
|sessionid|VJudge的一项cookie，可不指定|

使用时必须使用VJudge比赛创建者账号，通过`Jax.Q`cookie登录。检查结果保存在`比赛编号/result.md`中

命令行参数示例：

```sh
dotnet run contest=123456 scan_timeout=60000 jaxq=admin|XXXXXXXX
```

## 已知问题

1. 小概率重复的检查结果
2. 小概率VJudge无法一次性登陆成功，显示`发现0道题目`
