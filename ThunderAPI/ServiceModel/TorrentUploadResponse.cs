using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    [DataContract]
    public class TorrentUploadResponse
    {
        [DataMember(Name = "ret_value")]
        public int ReturnValue { get; set; }
        [DataMember(Name = "infoid")]
        public string Cid { get; set; }
        [DataMember(Name = "ftitle")]
        public string Title { get; set; }
        [DataMember(Name = "btsize")]
        public long Size { get; set; }
        [DataMember(Name = "is_full")]
        public int IsFull { get; set; }
        [DataMember(Name = "filelist")]
        public List<FileInfo> FileList { get; set; }
        [DataMember(Name = "random")]
        public string Random { get; set; }

        public int[] GetIndexArray()
        {
            int[] rtn = new int[FileList.Count];
            for (int i = 0; i < FileList.Count; i++)
            {
                rtn[i] = FileList[i].Index;
            }
            return rtn;
        }

        [DataContract]
        public class FileInfo
        {
            [DataMember(Name = "id")]
            public long Id { get; set; }
            [DataMember(Name = "subsize")]
            public long Size { get; set; }
            [DataMember(Name = "subformatsize")]
            public string FormatSize { get; set; }
            [DataMember(Name = "valid")]
            public bool IsValid { get; set; }
            [DataMember(Name = "findex")]
            public int Index { get; set; }
            [DataMember(Name = "subtitle")]
            public string Title { get; set; }
            [DataMember(Name = "ext")]
            public string Ext { get; set; }
            [DataMember(Name = "is_blocked")]
            public int IsBlocked { get; set; }
        }
    }
}
