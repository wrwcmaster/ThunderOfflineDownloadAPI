using Gaia.Common;
using Gaia.Common.Text;
using Gaia.Common.Serialization;
using Gaia.Common.Net.Http;
using Gaia.Common.Net.Http.RequestModifier;
using Gaia.Common.Net.Http.ResponseParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Gaia.Common.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Gaia.Common.Collections;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Net;
using HtmlAgilityPack;

namespace ThunderAPI
{
    public class ThunderAgent
    {
        private string _userName;
        private string _password;
        public bool IsLogin
        {
            get { return _cookieStore.ContainsKey("sessionid"); }
        }

        private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.106 Safari/535.2";
        private const string REFER = "http://lixian.vip.xunlei.com/";
        private Dictionary<string, string> _cookieStore = new Dictionary<string,string>();
        private string _uid;

        private void ParseCookieInResponse(string setCookieHeader, IDictionary<string, string> cookieStore)
        {
            //TODO: move to commonlib, support path, domain, etc
            foreach (var cookieEntry in setCookieHeader.Split(new string[]{";,"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var cookieEntryParts = cookieEntry.Split(';');
                var mainPart = cookieEntryParts[0].Trim();
                var subParts = mainPart.Split('=');
                cookieStore[subParts[0]] = subParts[1];
            }
        }

        private string GenerateCookieHeaderForRequest(IDictionary<string, string> cookieStore)
        {
            StringBuilder sb = new StringBuilder();
            bool firstFlag = true;
            foreach (var pair in cookieStore)
            {
                if (firstFlag)
                {
                    firstFlag = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append(pair.Key + "=" + pair.Value);
            }
            return sb.ToString();
        }

        public string GetVerifyCode(string userName)
        {
            return HttpHelper.SendRequest<string>(new Uri("http://login.xunlei.com/check"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                    new HttpRequestSimpleUriModifier("u", userName),
                    new HttpRequestSimpleUriModifier("cachetime", DateTime.Now.GetTimestamp().ToString())
                },
                new HttpResponseCustomParser<string>((res, control) =>
                {
                    string setCookieHeader = res.Headers.Get("Set-Cookie");
                    ParseCookieInResponse(setCookieHeader, _cookieStore);
                    string checkResult =  _cookieStore["check_result"];
                    var parts = checkResult.Split(':');
                    if (parts[0] == "0") return parts[1];
                    else return null;
                }),
                null
                );
        }

        public void Login(string userName, string password)
        {
            Console.WriteLine("Login");
            _userName = userName;
            _password = password;
            string verifyCode = GetVerifyCode(_userName);
            var md5 = MD5.Create();
            string hashPassword = md5.GetHashedString(md5.GetHashedString((md5.GetHashedString(_password))) + verifyCode.ToUpper());
            var parameters = new KeyValuePairList<string, string>(){
                { "u", _userName },
                { "p", hashPassword },
                { "verifycode", verifyCode },
                { "login_enable", "1" },
                { "login_hour", "720" }
            };

            HttpHelper.SendRequest<string>(new Uri("http://login.xunlei.com/sec2login/"), HttpMethod.POST, new List<IHttpRequestModifier>(){
                    new HttpRequestUrlEncodedFormModifier(parameters),
                    new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore))
                },
                new HttpResponseCustomParser<string>((res, control) =>
                {
                    string setCookieHeader = res.Headers.Get("Set-Cookie");
                    ParseCookieInResponse(setCookieHeader, _cookieStore);
                    _uid = _cookieStore["userid"];
                    if (_cookieStore.ContainsKey("sessionid"))
                    {
                        Console.WriteLine("SessionID: " + _cookieStore["sessionid"]);
                    }
                    else
                    {
                        Console.WriteLine("Login failed, session not established.");
                    }
                    return null;
                }),
                null
                );
        }

        public void Logout()
        {
            Console.WriteLine("Logout");
            if (_cookieStore.ContainsKey("sessionid"))
            {
                string sessionId = _cookieStore["sessionid"];
                var request = HttpHelper.BuildRequest(new Uri("http://login.xunlei.com/unregister"), HttpMethod.GET, new List<IHttpRequestModifier>()
                {
                    new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                    new HttpRequestSimpleUriModifier("sessionid", sessionId)
                }, null);
                var response = request.GetResponse();
                _cookieStore.Clear();
            }
        }

        public UrlQueryResult QueryUrl(string url)
        {
            string response = HttpHelper.SendRequest<string>(new Uri("http://dynamic.cloud.vip.xunlei.com/interface/url_query?callback=queryUrl&interfrom=task"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestSimpleUriModifier("u", url),
                new HttpRequestSimpleUriModifier("random", GenerateRandomValue()),
                new HttpRequestSimpleUriModifier("tcache", DateTime.Now.GetTimestamp().ToString())
            }, new HttpResponseStringParser(), null);

            return (new UrlQueryResult.Parser()).Parse(response);
        }

        public TorrentUploadResponse UploadTorrent(string fileName, Stream fileStream)
        {
            var parameters = new HttpRequestMultipartFormModifier(new KeyValuePairList<string, string>(){
                { "random", GenerateRandomValue() }
            }, new KeyValuePairList<string, HttpRequestMultipartFormModifier.FileInfo>(){
                { "filepath", new HttpRequestMultipartFormModifier.FileInfo(fileName, fileStream) }
            });

            return HttpHelper.SendRequest<TorrentUploadResponse>(new Uri("http://dynamic.cloud.vip.xunlei.com/interface/torrent_upload"), HttpMethod.POST, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                parameters
            }, new HttpResponseCustomParser<TorrentUploadResponse>((res, control) =>
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string responseStr = sr.ReadToEnd();
                    string json = responseStr.Extract("var btResult =", ";");
                    return (TorrentUploadResponse)new DataContractJsonSerializer(typeof(TorrentUploadResponse)).Deserialize(json);
                }
            }), null);
        }

        public BtTaskCommitResponse CommitBtTask(string cid, IEnumerable<int> indexes)
        {
            
            string indexStr = String.Join("_", indexes) + "_";
            return HttpHelper.SendRequest<BtTaskCommitResponse>(new Uri("http://dynamic.cloud.vip.xunlei.com/interface/bt_task_commit"), HttpMethod.POST, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestUrlEncodedFormModifier(new KeyValuePairList<string, string>(){
                    { "cid", cid },
                    { "findex", indexStr}
                })
            }, new HttpResponseCustomParser<BtTaskCommitResponse>((res, control) =>
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    BtTaskCommitResponse commitResponse = null;
                    string responseStr = sr.ReadToEnd();
                    try
                    {
                        string json = responseStr.Trim('(', ')');
                        commitResponse = (BtTaskCommitResponse)new DataContractJsonSerializer(typeof(BtTaskCommitResponse)).Deserialize(json);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to commit bt task - " + responseStr + ".", ex);
                    }
                    if (commitResponse == null || commitResponse.TaskId == 0)
                    {
                        throw new InvalidOperationException("Failed to commit bt task - " + responseStr + ".");
                    }
                    return commitResponse;
                }
            }), null);
        }

        public BTDetailResponse QueryBTDetail(string cid, long taskId, int pageIndex)
        {
            return HttpHelper.SendRequest<BTDetailResponse>(new Uri("http://dynamic.cloud.vip.xunlei.com/interface/fill_bt_list?callback=fill_bt_list&g_net=1&interfrom=task"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestSimpleUriModifier("p", pageIndex.ToString()),
                new HttpRequestSimpleUriModifier("infoid", cid),
                new HttpRequestSimpleUriModifier("tid", taskId.ToString()),
                new HttpRequestSimpleUriModifier("uid", _uid)
            }, new HttpResponseCustomParser<BTDetailResponse>((res, control) =>
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string responseStr = sr.ReadToEnd();
                    try
                    {
                        string json = responseStr.Substring(23, responseStr.Length - 25);
                        return (BTDetailResponse)new DataContractJsonSerializer(typeof(BTDetailResponse)).Deserialize(json);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Failed to query BT detail: " + responseStr + ".", ex);
                    }
                }
            }), null);
        }

        public HttpWebRequest PrivateDownload(string privateUrl)
        {
            if (!_cookieStore.ContainsKey("gdriveid"))
            {
                QueryTasks(1, 1); //Retrieve gdriveid for cookie
            }
            return HttpHelper.BuildRequest(new Uri(privateUrl), HttpMethod.GET, new List<IHttpRequestModifier>
            {
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore))
            }, null);
        }

        public void PrivateDownload(string privateUrl, Stream outputStream)
        {
            var request = PrivateDownload(privateUrl);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            var inputSteam = response.GetResponseStream();
            inputSteam.CopyTo(outputStream);
        }

        public TaskQueryResponse QueryTasks(int pageIndex, int pageSize)
        {
            return HttpHelper.SendRequest<TaskQueryResponse>(new Uri("http://dynamic.cloud.vip.xunlei.com/interface/showtask_unfresh?type_id=4&p=1&interfrom=task"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestSimpleUriModifier("page", pageIndex.ToString()),
                new HttpRequestSimpleUriModifier("tasknum", pageSize.ToString()),
                new HttpRequestSimpleUriModifier("t", DateTime.Now.GetTimestamp().ToString())
            }, new HttpResponseCustomParser<TaskQueryResponse>((res, control) =>
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string responseStr = sr.ReadToEnd();
                    string json = responseStr.Substring(7).Trim('(', ')');
                    var rtn = (TaskQueryResponse)new DataContractJsonSerializer(typeof(TaskQueryResponse)).Deserialize(json);
                    if (rtn != null && rtn.Info != null && rtn.Info.User != null)
                    {
                        _cookieStore["gdriveid"] = rtn.Info.User.Cookie;
                    }
                    return rtn;
                }
            }), null);
        }

        public KuaiUrlAnalysisResponse KuaiAnalyzeUrl(string url)
        {
            return HttpHelper.SendRequest<KuaiUrlAnalysisResponse>(new Uri("http://kuai.xunlei.com/webfilemail_interface?action=webfilemail_url_analysis"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestSimpleUriModifier("url", url)
            }, /*new HttpResponseJSONObjectParser<KuaiUrlAnalysisResponse>()*/
             new HttpResponseCustomParser<KuaiUrlAnalysisResponse>((res, control) =>
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string responseStr = sr.ReadToEnd();
                    string json = responseStr;
                    return (KuaiUrlAnalysisResponse)new DataContractJsonSerializer(typeof(KuaiUrlAnalysisResponse)).Deserialize(json);
                }
            }), null);
        }

        public KuaiForwardResponse KuaiForwardOfflineDownloadTask(string cid, long fileSize, string gcid, string title, string url, string section)
        {
            var request = new KuaiForwardRequest()
            {
                Cid = cid,
                FileSize = fileSize,
                Gcid = gcid,
                Title = title + ".removethis",
                Url = url,
                Section = section
            };

            var requestStr = new DataContractJsonSerializer(typeof(List<KuaiForwardRequest>)).Serialize(new List<KuaiForwardRequest>(){ request });

            Func<KuaiForwardResponse> operation = () => {
                Console.WriteLine("KuaiForwardOfflineDownloadTask");
                return HttpHelper.SendRequest<KuaiForwardResponse>(new Uri("http://kuai.xunlei.com/interface.php?action=lixian_forward_upload"), HttpMethod.POST, new List<IHttpRequestModifier>(){
                    new HttpRequestSimpleUriModifier("cachetime", DateTime.Now.GetTimestamp().ToString()),
                    new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                    new HttpRequestUrlEncodedFormModifier(new KeyValuePairList<string, string>(){
                        { "data", requestStr }
                    })
                }, new HttpResponseJSONObjectParser<KuaiForwardResponse>(), null);
            };

            //handle session expiration
            //TODO: move to general logic
            var result = operation();
            if (result != null && result.ForwardTaskId == -1)
            {
                Console.WriteLine("Request Failed: " + result.Message);
                Logout();
                Login(_userName, _password);
                result = operation();
            }
            Console.WriteLine(result.Message);
            return result;
        }

        public KuaiShortUrlResponse KuaiGetShortUrl(long taskId)
        {
            return HttpHelper.SendRequest<KuaiShortUrlResponse>(new Uri("http://kuai.xunlei.com/webfilemail_interface?action=webfilemail_get_short_url"), HttpMethod.GET, new List<IHttpRequestModifier>(){
                new HttpRequestSimpleHeaderModifier("Cookie", GenerateCookieHeaderForRequest(_cookieStore)),
                new HttpRequestSimpleUriModifier("task_id", taskId.ToString()),
                new HttpRequestSimpleUriModifier("from_uid", _uid)
            }, new HttpResponseJSONObjectParser<KuaiShortUrlResponse>(), null);
        }

        public string KuaiGetActualUrl(string shortUrl)
        {
            WebClient client = new WebClient();
            string html = client.DownloadString(shortUrl);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var linkNode = doc.DocumentNode.SelectSingleNode("//div[@id='file-list']/ul/li/div[@class='file_tr']/span[@class='c_2']/a");
            return linkNode.Attributes["href"].Value;
        }

        private string GenerateRandomValue()
        {
            return DateTime.Now.GetTimestamp().ToString() + (new Random().NextDouble() * (2000000 - 10) + 10).ToString();
        }
    }
}
