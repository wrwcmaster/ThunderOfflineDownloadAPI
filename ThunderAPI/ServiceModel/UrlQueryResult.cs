using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderAPI
{
    public class UrlQueryResult
    {
        public int Flag { get; set; }
        public string Cid { get; set; }
        public long Size { get; set; }
        public string Title { get; set; }
        public bool isFull { get; set; }
        public List<FileInfo> FileList { get; set; }
        public string Random { get; set; }

        public class FileInfo
        {
            public string FileName { get; set; }
            public long Size { get; set; }
            public int Index { get; set; }
        }

        public UrlQueryResult()
        {

        }

        public int[] GetIndexArray()
        {
            int[] rtn = new int[FileList.Count];
            for (int i = 0; i < FileList.Count; i++)
            {
                rtn[i] = FileList[i].Index;
            }
            return rtn;
        }

        public class Parser
        {
            private List<object> _valueList;
            private object _lastObj = null;
            private Stack<dynamic> _stack;
            private StringBuilder _tokenBuilder = null;
            private StringBuilder TokenBuilder
            {
                get
                {
                    if (_tokenBuilder == null)
                    {
                        _tokenBuilder = new StringBuilder(); 
                    }
                    return _tokenBuilder;
                }
            }
            
            private void ResetTokenBuilder()
            {
                _tokenBuilder = null;
            }

            public UrlQueryResult Parse(string rawString)
            {
                _stack = new Stack<dynamic>();
                char[] array = rawString.ToCharArray();
                for (int i = 0; i < array.Length; i++)
                {
                    char c = array[i];
                    ReadChar(c);
                }

                var result = new UrlQueryResult();
                result.Flag = Int32.Parse((string)_valueList[0]);
                result.Cid = (string)_valueList[1];
                result.Size = Int64.Parse((string)_valueList[2]);
                result.Title = (string)_valueList[3];
                result.isFull = ((string)_valueList[4]) == "1";
                List<object> titleList = (List<object>) _valueList[5];
                List<object> sizeList = (List<object>)_valueList[7];
                List<object> indexList = (List<object>)_valueList[10];
                result.FileList = new List<FileInfo>();
                for (int i = 0; i < titleList.Count; i++)
                {
                    result.FileList.Add(new FileInfo()
                    {
                        FileName = (string) titleList[i],
                        Size = Int64.Parse((string)sizeList[i]),
                        Index = Int32.Parse((string)indexList[i])
                    });
                }
                result.Random = (string)_valueList[12];
                return result;
            }

            private void ReadChar(char c)
            {
                if(!CheckToken(c)) TokenBuilder.Append(c);
            }

            private bool CheckToken(char c)
            {
                string token = TokenBuilder.ToString();
                dynamic current = null;
                if (_stack.Count > 0)
                {
                    current = _stack.Peek();
                }
                if (current == null || current.Name == "Array")
                {
                    if ((token == "queryUrl" || token == "newArray") && c == '(')
                    {
                        _stack.Push(new { Name = "Array", Value = new List<object>() });
                        ResetTokenBuilder();
                        return true;
                    }
                    if (c == ')')
                    {
                        _stack.Pop();
                        if (_lastObj != null) current.Value.Add(_lastObj);
                        else current.Value.Add(token);

                        if (_stack.Count == 0) _valueList = current.Value;
                        else _lastObj = current.Value;

                        ResetTokenBuilder();
                        return true;
                    }
                    if (c == '\'')
                    {
                        _stack.Push(new { Name = "Text" });
                        ResetTokenBuilder();
                        return true;
                    }
                    if (Char.IsWhiteSpace(c))
                    {
                        return true;
                    }
                    else if(c == ',')
                    {
                        if (_lastObj != null) current.Value.Add(_lastObj);
                        else current.Value.Add(token);
                        ResetTokenBuilder();
                        return true;
                    }
                }
                if (current != null && current.Name == "Text")
                {
                    if (c == '\'')
                    {
                        _stack.Pop();
                        _lastObj = token;
                        ResetTokenBuilder();
                        return true;
                    }
                }
                
                return false;
            }
        }
    }
}
