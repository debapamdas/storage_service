using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Nokia.Storage.IntergrationTest
{
    public static class TestHelpers
    {
        public static MultipartFormDataContent GetMultipartFormDataContent(TestFile testfile)
        {
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            var bytes = Encoding.UTF8.GetBytes(testfile.Content);
            var content = new MemoryStream(bytes);
            testfile.Length = bytes.Length;

            HttpContent httpContent = new StreamContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(testfile.ContentType);

            multipartFormDataContent.Add(httpContent, "tarFile", testfile.FileName);
            return multipartFormDataContent;
        }

        public static async Task<T> DeserializeBody<T>(this HttpResponseMessage httpResponseMessage) where T : class
        {
            try
            {
                httpResponseMessage.EnsureSuccessStatusCode();
                //var resultStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                //return await JsonSerializer.DeserializeAsync<T>(resultStream);
                string resultString = await httpResponseMessage.Content.ReadAsStringAsync();
                var x = System.Text.Json.JsonSerializer.Deserialize<T>(resultString);
                return JsonConvert.DeserializeObject<T>(resultString);
            }
            finally
            {
                httpResponseMessage.Dispose();
            }
        }
    }
}
