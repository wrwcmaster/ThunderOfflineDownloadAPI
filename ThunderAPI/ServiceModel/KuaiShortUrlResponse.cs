using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class KuaiShortUrlResponse
    {
        [DataMember(Name = "result")]
        public int ResultCode { get; set; }
        [DataMember(Name = "msg")]
        public string Url { get; set; }
    }
}
