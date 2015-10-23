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
        public string Message { get; set; }

        public long ForwardTaskId {
            get
            {
                long output = -1;
                long.TryParse(Message, out output);
                return output;
            }
        }
    }
}
