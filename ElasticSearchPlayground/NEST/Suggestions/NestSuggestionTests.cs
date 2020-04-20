using Elasticsearch.Net;
using Nest;
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

// basic NEST: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-queries.html

// suggestion: https://www.elastic.co/guide/en/elasticsearch/reference/current/search-suggesters.html#search-suggesters
// Fuzzy: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/fuzzy-query-usage.html
// check: https://discuss.elastic.co/t/autocomplete-across-multiple-fields-with-nest-client/120878/2

namespace ElasticSearchPlayground
{
    public sealed class NestSuggestionTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IElasticClient _client;
        private readonly ElasticClient _clientObject;
        private const string KEYWORDS_INDEX = "test-keywords";
        //private const string DEFAULT_INDEX = "test-default-index";

        #region Ctor

        public NestSuggestionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var setting = new ConnectionSettings(new Uri("http://localhost:9200"))
                                // see, default index: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index-name-inference.html#_default_index_name_on_connection_settings
                                //.DefaultIndex(DEFAULT_INDEX)
                                .DefaultMappingFor<Keywords>(m =>
                                        m.IndexName(KEYWORDS_INDEX)
                                         .IdProperty(p => p.Id));
            setting.ThrowExceptions(true);
            ElasticClient client = new ElasticClient(setting);

            ExistsResponse exists = client.Indices.Exists(KEYWORDS_INDEX);
            if (exists.Exists)
                client.Indices.Delete(KEYWORDS_INDEX);

            var createIndexResponse = client.Indices.Create(KEYWORDS_INDEX, c => c
                    .Map<Keywords>(m => // m.AutoMap()
                            m.Properties(ps =>
                                ps
                                    .Keyword(d => d.Name(d1 => d1.Values))
                                    .Text(d => d.Name(d1 => d1.Values))
                                    .SearchAsYouType(d => d.Name(d1 => d1.Values))
                            )));


            var descriptor = new BulkDescriptor();

            int len = Constant.Words.Length;
            foreach (var i in Enumerable.Range(0, 1000))
            {
                descriptor.Index<Keywords>(op => op
                    .Document(new Keywords { Id = 100 + i, Values = new [] {
                            Constant.Words[i % len],
                            Constant.Words[(i + 1) % len],
                            Constant.Words[(i + 2) % len],
                        }
                    })
                );
            }

            var result = client.Bulk(descriptor);
           

            _clientObject = client;
            _client = client;
        }

        #endregion Ctor

        [Fact]
        public async Task SuggestionTest()
        {
            ISearchResponse<Keywords> response = await _client.SearchAsync<Keywords>(s =>
                    s.Size(5)
                     .Query(q => 
                        q.Fuzzy(f => f.Value("substance")))
                       );

            _outputHelper.WriteLine($"Count: {response.Documents.Count}");
            foreach (Keywords doc in response.Documents)
            {
                _outputHelper.WriteLine(doc.ToString());

            }

            var hits = response.HitsMetadata.Hits.Select(h => (h.Score, h.Source));
            // var highlights = response.HitsMetadata.Hits.Select(h => h.Highlight);
            foreach (var hit in hits)
            {
                _outputHelper.WriteLine(hit.ToString());

            }

        }
    }
}