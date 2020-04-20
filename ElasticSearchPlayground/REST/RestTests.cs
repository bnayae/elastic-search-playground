using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// Setup docker: docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk

namespace ElasticSearchPlayground
{
    public sealed class RestTests: IDisposable
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly ITestOutputHelper _outputHelper;
    
        public RestTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _client.BaseAddress = new Uri("http://localhost:9200");
            _client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task InfoTest()
        {
            HttpResponseMessage response = await _client.GetAsync("/");
            string result = await response.Content.ReadAsStringAsync();
            _outputHelper.WriteLine(result);
        }

        [Fact]
        public async Task HealthTest()
        {
            HttpResponseMessage response = await _client.GetAsync("/_cluster/health");
            Stream stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());
        }


        [Fact]
        public async Task PostTest()
        {
            string index = "test-index";
            string type = "test-type";
            string data = @"{""name"":""bnaya"", ""role"":""CTO""}";
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string url = $"/{index}/{type}?timeout=10s";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            Assert.Equal(index, json.GetString("_index"));
            Assert.Equal(type, json.GetString("_type"));
            Assert.NotEmpty(json.GetString("_id"));

            // GET

            string id = json.GetString("_id");
            url = $"/{index}/{type}/{id}";
            response = await _client.GetAsync(url);
            stream = await response.Content.ReadAsStreamAsync();
            json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            Assert.Equal(index, json.GetString("_index"));
            Assert.Equal(type, json.GetString("_type"));

            Assert.Equal(requestJson.GetString("name"), json.GetSourceString("name"));
            Assert.Equal(requestJson.GetString("role"), json.GetSourceString("role"));
        }

        [Fact]
        public async Task PutTest()
        {
            string index = "test-index";
            string type = "test-type";
            string id = "test-id";
            string data = @"{""name"":""bnaya"", ""role"":""CTO""}";
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string urlGet = $"/{index}/{type}/{id}";
            string url = $"{urlGet}?timeout=10s";
            HttpResponseMessage response = await _client.PutAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            Assert.Equal(index, json.GetString("_index"));
            Assert.Equal(type, json.GetString("_type"));
            Assert.Equal(id, json.GetString("_id"));

            // GET

            response = await _client.GetAsync(urlGet);
            stream = await response.Content.ReadAsStreamAsync();
            json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            Assert.Equal(index, json.GetString("_index"));
            Assert.Equal(type, json.GetString("_type"));
            Assert.Equal(id, json.GetString("_id"));

            Assert.Equal(requestJson.GetString("name"), json.GetSourceString("name"));
            Assert.Equal(requestJson.GetString("role"), json.GetSourceString("role"));
        }
    }
}
