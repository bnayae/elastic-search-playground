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

// docker run -it --rm --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:7.6.2

// Suggesters: Beyond Autocomplete: https://www.youtube.com/watch?v=PQGlhbf7o7c
// Suggester: https://www.elastic.co/guide/en/elasticsearch/reference/7.x/search-suggesters.html


// TODO: check mapping (test): http://localhost:9200/test-index-text-completion/_mapping/field/title

#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

namespace ElasticSearchPlayground
{
    public abstract class RestSuggestionBase : IDisposable
    {
        protected readonly HttpClient _client;
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _index;
        protected static readonly TimeSpan DELAY_WAIT_FOR_WRITE = TimeSpan.FromSeconds(1);

        #region Ctor

        public RestSuggestionBase(ITestOutputHelper outputHelper, string index)
        {
            _client = new HttpClient();
            _outputHelper = outputHelper;
            _index = index;
            _client.BaseAddress = new Uri("http://localhost:9200");
            _client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            //.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //_client.DefaultRequestHeaders
            //        .Add("Accept-Encoding", "gzip, deflate, br");
        }

        #endregion Ctor

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposed) 
        {
            _client.Dispose();
        }

        ~RestSuggestionBase() 
        {
            Dispose(false);
        }

        #endregion Dispose

        #region CreateIndexAsync

        protected async Task CreateIndexAsync(bool withWrite)
        {
            string data = File.ReadAllText($@"Json\{_index}.json");
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string baseUrl = $"/{_index}";
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
                Assert.Equal(_index, json.GetString("index"));
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion CreateIndexAsync

        #region CreateDocsAsync

        protected async Task CreateDocsAsync(bool withWrite)
        {
            await CreateIndexAsync(false);
            string baseUrl = $"/{_index}/_doc";
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

                Assert.Equal(_index, json.GetString("_index"));
                Assert.Equal("_doc", json.GetString("_type"));
                Assert.Equal("created", json.GetString("result"));
                Assert.NotEmpty(json.GetString("_id"));
            }
        }

        #endregion CreateDocsAsync

        #region SearchAsync

        protected async Task<JsonDocument> SearchAsync(string queryPath)
        {
            await CreateDocsAsync(false);

            await Task.Delay(DELAY_WAIT_FOR_WRITE);

            string data = File.ReadAllText(queryPath);
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string baseUrl = $"/{_index}/_search";
            string url = $"{baseUrl}?timeout=10s";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            JsonDocument json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            return json;
        }

        #endregion SearchAsync

        #region AnalyzeAsync

        protected async Task<JsonDocument> AnalyzeAsync(string queryPath)
        {
            await CreateIndexAsync(false);

            string data = File.ReadAllText(queryPath);
            var requestJson = JsonDocument.Parse(data);
            HttpContent content = data.AsJsonContent();

            string url = $"/{_index}/_analyze";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            Stream stream = await response.Content.ReadAsStreamAsync();
            JsonDocument json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            return json;
        }

        #endregion AnalyzeAsync

        #region MappingAsync

        protected async Task<JsonDocument> MappingAsync(string field)
        {
            await CreateIndexAsync(false);
            
            string url = $"/{_index}/_mapping/field/{field}";
            HttpResponseMessage response = await _client.GetAsync(url);
            Stream stream = await response.Content.ReadAsStreamAsync();
            JsonDocument json = await JsonDocument.ParseAsync(stream);
            _outputHelper.WriteLine(json.ToJsonString());

            return json;
        }

        #endregion MappingAsync
    }
}
