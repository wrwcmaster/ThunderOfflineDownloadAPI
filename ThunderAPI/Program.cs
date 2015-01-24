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

            var urlResult = agent.QueryUrl("magnet:?xt=urn:btih:O3XUJ6PORHEDBUIL6R4AYYDO6TNR3TOS&dn=&tr=http%3A%2F%2F208.67.16.113%3A8000%2Fannounce&tr=udp%3A%2F%2F208.67.16.113%3A8000%2Fannounce&tr=http%3A%2F%2Ftracker.openbittorrent.com%3A80%2Fannounce&tr=http%3A%2F%2Ftracker.publicbt.com%3A80%2Fannounce&tr=http%3A%2F%2Ftracker.prq.to%2Fannounce&tr=http%3A%2F%2Ftracker.ktxp.com%3A6868%2Fannounce&tr=http%3A%2F%2Ftracker.ktxp.com%3A7070%2Fannounce&tr=http%3A%2F%2Ftracker.dmhy.org%3A8000%2Fannounce&tr=http%3A%2F%2Fbt.wiiyi.com%3A6969%2Fannounce&tr=http%3A%2F%2Fbt.dmzg.net%3A6969%2Fannounce&tr=http%3A%2F%2Ftracker.dmguo.com%3A2710%2Fannounce&tr=http%3A%2F%2Ftracker.moeing.org%3A7070%2Fannounce&tr=http%3A%2F%2Ftk.greedland.net%2Fannounce&tr=http%3A%2F%2Fshare.xdmhy.net%3A8000%2Fannounce&tr=http%3A%2F%2Ft2.popgo.org%3A7456%2Fannonce&tr=http%3A%2F%2Ftracker.kaicn.com%3A2710%2Fannounce&tr=http%3A%2F%2Ftracker.ipv6.scau.edu.cn%2Fannounce.php%3Fpasskey%3D9a7d1ca3f987a9e440df4c946f2fafc9");
            Console.WriteLine(urlResult.Title);
            var commitResponse = agent.CommitBtTask(urlResult.Cid, urlResult.GetIndexArray());
            if (commitResponse.TaskId == 0)
            {
                Console.WriteLine("Failed to commit task.");
                return;
            }
            Console.WriteLine(commitResponse.TaskId);

            var detail = agent.QueryBTDetail(urlResult.Cid, commitResponse.TaskId, 1);
            
            foreach (var fileInfo in detail.FileList)
            {
                Console.WriteLine(fileInfo.FileName + " - " + fileInfo.Percent.ToString() + "%");
                if (fileInfo.Percent == 100)
                {
                    var analysisResult = agent.KuaiAnalyzeUrl(fileInfo.DownloadUrl);
                    Console.WriteLine(analysisResult.Result.Url);
                    var forwardResponse = agent.KuaiForwardOfflineDownloadTask(analysisResult.Result.Cid, analysisResult.Result.FileSize, analysisResult.Result.Gcid, fileInfo.FileName, analysisResult.Result.Url, analysisResult.Result.Section);
                    Console.WriteLine(forwardResponse.ForwardTaskId);
                    var shortUrlResponse = agent.KuaiGetShortUrl(forwardResponse.ForwardTaskId);
                    Console.WriteLine(shortUrlResponse.Url);
                    var url = agent.KuaiGetActualUrl(shortUrlResponse.Url);
                    Console.WriteLine(url);
                }
            }
        }
    }
}
