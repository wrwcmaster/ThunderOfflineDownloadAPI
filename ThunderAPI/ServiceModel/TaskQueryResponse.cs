using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class TaskQueryResponse
    {
        [DataMember(Name = "info")]
        public QueryInfo Info { get; set; }

        [DataContract]
        public class QueryInfo
        {
            [DataMember(Name = "total_num")]
            public int TotalCount { get; set; }
            [DataMember(Name = "tasks")]
            public List<TaskInfo> TaskList { get; set; }
            [DataMember(Name = "user")]
            public UserInfo User { get; set; }

            [DataContract]
            public class UserInfo
            {
                [DataMember(Name = "cookie")]
                public string Cookie { get; set; }
                //TODO: read other fields
            }

            [DataContract]
            public class TaskInfo
            {
                [DataMember(Name = "taskname")]
                public string TaskName { get; set; }
                [DataMember(Name = "cid")]
                public string Cid { get; set; }
                [DataMember(Name = "cookie")]
                public string Cookie { get; set; }
                [DataMember(Name = "download_status")]
                public int DownloadStatus { get; set; }
                [DataMember(Name = "progress")]
                public double Progress { get; set; }
                [DataMember(Name = "dt_committed")]
                public string CommittedDate { get; set; }
                [DataMember(Name = "ext")]
                public string Ext { get; set; }
                [DataMember(Name = "file_size")]
                public long FileSize { get; set; }
                [DataMember(Name = "file_type")]
                public string FileType { get; set; }
                [DataMember(Name = "finish_sum")]
                public int FinishSum { get; set; }
                [DataMember(Name = "gcid_real")]
                public string Gcid { get; set; }
                [DataMember(Name = "global_id")]
                public string GlobalId { get; set; }
                [DataMember(Name = "id")]
                public string Id { get; set; }
                [DataMember(Name = "is_blocked")]
                public int IsBlocked { get; set; }
                [DataMember(Name = "left_live_time")]
                public string LeftLiveTime { get; set; }
                [DataMember(Name = "list_sum")]
                public int ListSum { get; set; }
                [DataMember(Name = "lixian_url")]
                public string OfflineUrl { get; set; }
                [DataMember(Name = "url")]
                public string Url { get; set; }
                [DataMember(Name = "task_type")]
                public int TaskType { get; set; }
                [DataMember(Name = "verify")]
                public string Verify { get; set; }
                [DataMember(Name = "restype")]
                public int ResType { get; set; }
            }
        }
    }
}
