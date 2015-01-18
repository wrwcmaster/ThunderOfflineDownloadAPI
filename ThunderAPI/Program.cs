using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gaia.Common.Serialization;
using System.Runtime.Serialization.Json;
namespace ThunderAPI
{
    class Program
    {
        static void Main(string[] args)
        {

            ThunderAgent agent = new ThunderAgent();
            agent.Login("username", "password");
            var uploadResponse = agent.UploadTorrent("test.torrent", new FileStream("test.torrent", FileMode.Open));
            Console.WriteLine(uploadResponse.Title);
            var commitResponse = agent.CommitBtTask(uploadResponse.InfoId, new int[] { 0 });
            Console.WriteLine(commitResponse.Id);
            var queryResult = agent.QueryTasks(1, 30);
            var task = queryResult.Info.TaskList[0];
            Console.WriteLine(task.TaskName);
            var analysisResult = agent.KuaiAnalyzeUrl(task.OfflineUrl);
            Console.WriteLine(analysisResult.Result.Url);
            var forwardResponse = agent.KuaiForwardOfflineDownloadTask(analysisResult.Result.Cid, analysisResult.Result.FileSize, analysisResult.Result.Gcid, task.TaskName, analysisResult.Result.Url, analysisResult.Result.Section);
            Console.WriteLine(forwardResponse.ForwardTaskId);
            var shortUrlResponse = agent.KuaiGetShortUrl(forwardResponse.ForwardTaskId);
            Console.WriteLine(shortUrlResponse.Url);
            var url = agent.KuaiGetActualUrl(shortUrlResponse.Url);
            Console.WriteLine(url);
        }
    }
}
