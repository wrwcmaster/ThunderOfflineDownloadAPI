using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class BTDetailResponse
    {
        [DataMember(Name = "Tid")]
        public long TaskId { get; set; }
        [DataMember(Name = "Infoid")]
        public string InfoId { get; set; }
        [DataMember(Name="btnum")]
        public int FileCount { get; set; }
        [DataMember(Name = "btpernum")]
        public int OriginalFileCount { get; set; }
        [DataMember(Name = "now_page")]
        public int Page { get; set; }
        [DataMember(Name = "Record")]
        public List<FileInfo> FileList { get; set; }

        [DataContract]
        public class FileInfo
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }
            [DataMember(Name = "title")]
            public string FileName { get; set; }
            [DataMember(Name = "download_status")]
            public int DownloadStatus { get; set; }
            [DataMember(Name = "cid")]
            public string Cid { get; set; }
            [DataMember(Name = "percent")]
            public double Percent { get; set; }
            [DataMember(Name = "taskid")]
            public string TaskId { get; set; }
            [DataMember(Name = "downurl")]
            public string DownloadUrl { get; set; }
            [DataMember(Name = "filesize")]
            public long Size { get; set; }
            [DataMember(Name = "verify")]
            public string Verify { get; set; }
            [DataMember(Name = "url")]
            public string Url { get; set; }
            [DataMember(Name = "ext")]
            public string Ext { get; set; }
            [DataMember(Name = "dirtitle")]
            public string DirTitle { get; set; }
            [DataMember(Name = "is_blocked")]
            public bool IsBlocked { get; set; }
        }
    }
}
