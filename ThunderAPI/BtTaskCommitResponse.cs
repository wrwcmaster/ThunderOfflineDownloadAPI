using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class BtTaskCommitResponse
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }
        [DataMember(Name = "avail_space")]
        public long AvailableSpace { get; set; }
        [DataMember(Name = "time")]
        public double Time { get; set; }
        [DataMember(Name = "progress")]
        public double Progress { get; set; }
    }
}
