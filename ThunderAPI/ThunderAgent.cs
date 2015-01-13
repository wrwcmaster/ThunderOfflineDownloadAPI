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

namespace ThunderAPI
{
    public class ThunderAgent
    {
        private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.106 Safari/535.2";
        private const string REFER = "http://lixian.vip.xunlei.com/";
        private Dictionary<string, string> _cookieStore = new Dictionary<string,string>();
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
            string verifyCode = GetVerifyCode(userName);
            var md5 = MD5.Create();
            string hashPassword = md5.GetMd5Hash(md5.GetMd5Hash((md5.GetMd5Hash(password))) + verifyCode.ToUpper());
            var parameters = new KeyValuePairList<string, string>(){
                { "u", userName },
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
                    return null;
                }),
                null
                );
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
                    string responseStr = sr.ReadToEnd();
                    string json = responseStr.Trim('(', ')');
                    return (BtTaskCommitResponse)new DataContractJsonSerializer(typeof(BtTaskCommitResponse)).Deserialize(json);
                }
            }), null);
        }

        private string GenerateRandomValue()
        {
            return DateTime.Now.GetTimestamp().ToString() + (new Random().NextDouble() * (2000000 - 10) + 10).ToString();
        }
    }
}
