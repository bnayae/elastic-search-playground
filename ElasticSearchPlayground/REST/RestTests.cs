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
    }
}
