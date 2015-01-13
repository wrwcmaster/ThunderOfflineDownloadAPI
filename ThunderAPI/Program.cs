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
            var responseA = agent.UploadTorrent("test.torrent", new FileStream("test.torrent", FileMode.Open));
            var responseB = agent.CommitBtTask(responseA.InfoId, new int[] { 0 });
        }


    }
}
