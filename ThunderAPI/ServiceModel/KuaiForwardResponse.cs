using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class KuaiForwardResponse
    {
        [DataMember(Name = "result")]
        public int ResultCode { get; set; }
        [DataMember(Name = "msg")]
        public long ForwardTaskId { get; set; }
    }
}
