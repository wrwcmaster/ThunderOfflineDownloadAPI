using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class KuaiForwardRequest
    {
        [DataMember(Name = "cid_0")]
        public string Cid { get; set; }
        [DataMember(Name = "file_size_0")]
        public long FileSize { get; set; }
        [DataMember(Name = "gcid_0")]
        public string Gcid { get; set; }
        [DataMember(Name = "title_0")]
        public string Title { get; set; }
        [DataMember(Name = "url_0")]
        public string Url { get; set; }
        [DataMember(Name = "section_0")]
        public string Section { get; set; }
    }
}
