using Elasticsearch.Net;
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
    public sealed class LowLevelTests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ElasticLowLevelClient _client;
    
        public LowLevelTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var setting = new ConnectionConfiguration(
                                    new Uri("http://localhost:9200"))
                                .RequestTimeout(TimeSpan.FromSeconds(10));
            _client = new ElasticLowLevelClient(setting);
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task BasicTest()
        {
            string index = "test-index";
            string id = "test-id";
            var data = new Person("Bnaya","CTO");

            var respnse = await _client.IndexAsync<BytesResponse>(index, id, PostData.Serializable(data), new IndexRequestParameters { });
            byte[] body = respnse.Body;

            Assert.True(respnse.Success);

            var textJson = Encoding.UTF8.GetString(body);
            var json = JsonDocument.Parse(textJson);

            _outputHelper.WriteLine(json.ToJsonString());

            Assert.Equal(index, json.GetString("_index"));
            Assert.Equal("_doc", json.GetString("_type"));
            Assert.Equal(id, json.GetString("_id"));

            var person =  await _client.GetAsync<PersonResponse>(index, id, new GetRequestParameters { });
            Assert.True(person.Success);
            Assert.Equal("Bnaya", person.Body.Name);
            Assert.Equal("CTO", person.Body.Role);
        }

    }
}
