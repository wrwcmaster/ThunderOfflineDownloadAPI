using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class KuaiUrlAnalysisResponse
    {
        [DataMember(Name = "result")]
        public int ResultCode { get; set; }
        [DataMember(Name = "msg")]
        public ResultBody Result { get; set; }

        [DataContract]
        public class ResultBody
        {
            [DataMember(Name = "cid")]
            public string Cid { get; set; }
            [DataMember(Name = "filesize")]
            public long FileSize { get; set; }
            [DataMember(Name = "gcid")]
            public string Gcid { get; set; }
            [DataMember(Name = "filename")]
            public string FileName { get; set; }
            [DataMember(Name = "url")]
            public string Url { get; set; }
            [DataMember(Name = "section")]
            public string Section { get; set; }
        }
    }
}
