using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// Setup docker: docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk
// Suggesters: Beyond Autocomplete: https://www.youtube.com/watch?v=PQGlhbf7o7c

#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

namespace ElasticSearchPlayground
{
    public sealed class RestSuggestionTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _outputHelper;
        private const string INDEX_1 = "test-index-1";
        private static readonly TimeSpan DELAY_WAIT_FOR_WRITE = TimeSpan.FromSeconds(1);

        #region Ctor

        public RestSuggestionTests(ITestOutputHelper outputHelper)
        {
            _client = new HttpClient();
            _outputHelper = outputHelper;

            _client.BaseAddress = new Uri("http://localhost:9200");
            _client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    //.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.DefaultRequestHeaders
            //        .Add("Accept-Encoding", "gzip, deflate, br");
        }

        #endregion Ctor

        #region Dispose

        public void Dispose()
        {
            _client.Dispose();
        }

        #endregion Dispose

        #region Create_Index1_Test

        [Fact]
        public Task Create_Index1_Test() => Create_Index1(true);
        private async Task Create_Index1(bool withWrite)
        {
            string data = File.ReadAllText($@"Json\{INDEX_1}.json");
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string baseUrl = $"/{INDEX_1}";
            string url = $"{baseUrl}?timeout=10s";
            string urlExists = $"{baseUrl}/_search?timeout=10s";
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync(url);

                response = await _client.PutAsync(url, content);
                Stream stream = await response.Content.ReadAsStreamAsync();
                var json = await JsonDocument.ParseAsync(stream);
                if (withWrite)
                    _outputHelper.WriteLine(json.ToJsonString());

                Assert.True(json.GetBool("acknowledged"));
                Assert.True(json.GetBool("shards_acknowledged"));
                Assert.Equal(INDEX_1, json.GetString("index"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Create_Index1_Test

        #region Create_Index1_Docs_Test

        [Fact]
        public Task Create_Index1_Docs_Test() => Create_Index1_Docs(true);
        private async Task Create_Index1_Docs(bool withWrite)
        {
            await Create_Index1(false);
            string baseUrl = $"/{INDEX_1}/_doc";
            string url = $"{baseUrl}?timeout=10s";
            foreach (var file in Directory.GetFiles($@"Json\Docs"))
            {
                string data = File.ReadAllText(file);
                var requestJson = JsonDocument.Parse(data);
                HttpContent content = data.AsJsonContent();


                HttpResponseMessage response = await _client.PostAsync(url, content);
                Stream stream = await response.Content.ReadAsStreamAsync();
                var json = await JsonDocument.ParseAsync(stream);

                if (withWrite)
                    _outputHelper.WriteLine(json.ToJsonString());

                Assert.Equal(INDEX_1, json.GetString("_index"));
                Assert.Equal("_doc", json.GetString("_type"));
                Assert.Equal("created", json.GetString("result"));
                Assert.NotEmpty(json.GetString("_id"));
            }
        }

        #endregion Create_Index1_Docs_Test

        #region Search_Index1_1_Test

        [Fact]
        public async Task Search_Index1_1_Test()
        {
            await Create_Index1_Docs(false);

            await Task.Delay(DELAY_WAIT_FOR_WRITE);

            string data = File.ReadAllText(@"Json\Queries\Suggest1.json");
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            //string baseUrl = "/test-postman-1/_search";
            string baseUrl = $"/{INDEX_1}/_search";
            string url = $"{baseUrl}?timeout=10s";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            JsonDocument json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            JsonElement completer = json.GetElement("suggest", "completer");
            JsonElement found = completer[0].GetElement("options");
            var items = found.EnumerateArray().ToArray();

            Assert.Single(items, e => e.Get<string>("text") == "Animal");
            Assert.Single(items, e => e.Get<string>("text") == "animal");
            Assert.Equal(2, items.Length);
        }

        #endregion Search_Index1_1_Test

        #region Search_Fuzzy_Index1_1_Test

        [Fact]
        public async Task Search_Fuzzy_Index1_1_Test()
        {
            await Create_Index1_Docs(false);

            await Task.Delay(DELAY_WAIT_FOR_WRITE);

            string data = File.ReadAllText(@"Json\Queries\Suggest1Fuzzy.json");
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            //string baseUrl = "/test-postman-1/_search";
            string baseUrl = $"/{INDEX_1}/_search";
            string url = $"{baseUrl}?timeout=10s";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            JsonElement completer = json.GetElement("suggest", "completer");
            JsonElement found = completer[0].GetElement("options");
            var items = found.EnumerateArray().ToArray();

            Assert.Single(items, e => e.Get<string>("text") == "Animal");
            Assert.Single(items, e => e.Get<string>("text") == "animal");
            Assert.Equal(2, items.Length);
        }

        #endregion Search_Fuzzy_Index1_1_Test

    }
}
