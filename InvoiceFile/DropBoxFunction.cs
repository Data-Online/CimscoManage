using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;


namespace InvoiceFile
{
    public class DropBoxFunction
    {
        public static Metadata GetDropboxFilesList(string path = null)
        {
            var contentlist = RestApiCaller<Metadata>("https://api.dropboxapi.com/1/metadata/auto" + ConfigurationManager.AppSettings["DropBoxFolderPath"] + path);
            return contentlist;
        }

        public static byte[] GetDropboxFilesDownload(string filepath)
        {
            string ApiUrl = "https://content.dropboxapi.com/1/files/auto" + ConfigurationManager.AppSettings["DropBoxFolderPath"] + filepath;
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ConfigurationManager.AppSettings["DropBoxAccessToken"]);
                using (var response = client.GetAsync("").Result)
                {
                    return response.Content.ReadAsByteArrayAsync().Result;
                }
            }
        }

        public static string GetDropboxFilesDelete(string filepath)
        {
            string ApiUrl = "https://api.dropboxapi.com/1/fileops/delete";
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ConfigurationManager.AppSettings["DropBoxAccessToken"]);
                using (var response = client.PostAsync("?root=dropbox&path=" + ConfigurationManager.AppSettings["DropBoxFolderPath"] + filepath, null).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        return response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
        }
        public static T RestApiCaller<T>(string ApiUrl)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ConfigurationManager.AppSettings["DropBoxAccessToken"]);
                using (var response = client.PostAsync("", null).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string contentString = response.Content.ReadAsStringAsync().Result;
                        var jsonReturn = JsonConvert.DeserializeObject<T>(contentString);
                        return jsonReturn;
                    }
                    else
                    {
                        string contentString = response.Content.ReadAsStringAsync().Result;
                        var jsonReturn = JsonConvert.DeserializeObject<T>(contentString);
                        return jsonReturn;
                    }
                }
            }
        }


    }

    public class Metadata
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("is_dir")]
        public bool IsDirectory { get; set; }

        [JsonProperty("contents")]
        public List<Metadata> Contents { get; set; }
    }
}
