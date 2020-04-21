//using Elasticsearch.Net;
//using Nest;
//using System;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Xunit;
//using Xunit.Abstractions;

//// Setup docker: docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk

//// check: https://discuss.elastic.co/t/autocomplete-across-multiple-fields-with-nest-client/120878/2

//namespace ElasticSearchPlayground
//{
//    public sealed class NestTests : IDisposable
//    {
//        private readonly ITestOutputHelper _outputHelper;
//        private readonly IElasticClient _client;
//        private readonly ElasticClient _clientObject;
//        private const string PEOPLE_INDEX = "test-people";
//        private const string DEFAULT_INDEX = "test-default-index";

//        public NestTests(ITestOutputHelper outputHelper)
//        {
//            _outputHelper = outputHelper;
//            var setting = new ConnectionSettings(new Uri("http://localhost:9200"))
//                                // see, default index: https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/index-name-inference.html#_default_index_name_on_connection_settings
//                                .DefaultIndex(DEFAULT_INDEX)
//                                .DefaultMappingFor<Person>(m =>
//                                        m.IndexName(PEOPLE_INDEX)
//                                         .IdProperty(p => p.Id));
//            setting.ThrowExceptions(true);
//            ElasticClient client = new ElasticClient(setting);

//            ExistsResponse exists = client.Indices.Exists(PEOPLE_INDEX);
//            if (exists.Exists)
//                client.Indices.Delete(PEOPLE_INDEX);

//            exists = client.Indices.Exists(DEFAULT_INDEX);
//            if (exists.Exists)
//                client.Indices.Delete(DEFAULT_INDEX);

//            var createIndexResponse = client.Indices.Create(PEOPLE_INDEX, c => c
//                    .Map<Person>(m => // m.AutoMap()
//                        m.Properties(ps => 
//                                ps
//                                  .Text(d => d.Name(d1 => d1.Name))
//                                  .Text(d => d.Name(d1 => d1.Role))
//                                  .DateRange(d => d.Name(d1 => d1.Date))
//                    )));
//            createIndexResponse = client.Indices.Create(DEFAULT_INDEX, c => c
//                    .Map<Post>(m => m.AutoMap()));

//            _clientObject = client;
//            _client = client;            

//        }

//        public void Dispose()
//        {
//        }

//        [Fact]
//        public async Task DefaultsTest()
//        {
//            // GET
//            var searchResponse = await _client.SearchAsync<object>(search =>
//             search.Size(4)
//             .Query(q =>
//                q.Match(m =>
//                    m.Field(f => f)
//                        .Query("Bnaya"))));
//            _outputHelper.WriteLine(searchResponse.DebugInformation);
//            _outputHelper.WriteLine("=====================================================");

//            Assert.Equal("http://localhost:9200/default-index/_search?typed_keys=true",
//                          searchResponse.ApiCall.Uri.ToString());

//            // Type fall-back
//            searchResponse = await _client.SearchAsync<Person>(search =>
//             search.Size(4)
//                 .Query(q =>
//                    q.Match(m =>
//                        m.Field(f => f.Name)
//                            .Query("Bnaya"))));
//            _outputHelper.WriteLine(searchResponse.DebugInformation);

//            Assert.Equal("http://localhost:9200/Person/_search?typed_keys=true",
//                          searchResponse.ApiCall.Uri.ToString());
//            _outputHelper.WriteLine("=====================================================");

//            // Request override
//            searchResponse = await _client.SearchAsync<Person>(search =>
//                 search.Index("override-index")
//                 .Size(4)
//                 .Query(q =>
//                    q.Match(m =>
//                        m.Field(f => f.Name)
//                            .Query("Bnaya"))));
//            _outputHelper.WriteLine(searchResponse.DebugInformation);

//            Assert.Equal("http://localhost:9200/override-index/_search?typed_keys=true",
//                          searchResponse.ApiCall.Uri.ToString());
//        }

//        [Fact]
//        public async Task SearchTest()
//        {
//            var input = new Person("Bnaya", "CTO");
//            IndexResponse response = await _client.IndexDocumentAsync(input, CancellationToken.None);

//            _outputHelper.WriteLine(response.Result.ToString());

//            await _clientObject.Indices.RefreshAsync(Indices.Index(PEOPLE_INDEX));

//            // GET
//            var searchResponse = await _client.SearchAsync<Person>(s => s
//                .From(0)
//                .Size(10)
//                .Query(q => q
//                     .Match(m => m
//                        .Field(f => f.Name)
//                        .Query("Bnaya")
//                     )
//                )
//            );
//            var people = searchResponse.Documents;
//            _outputHelper.WriteLine($"Count = {people.Count}");
//            Assert.Equal(1, people.Count);
//            Person result = people.Single();
//            Assert.Equal(input, result);
//        }

//        [Fact]
//        public async Task SearchDateRangeTest()
//        {
//            var inputs = new[]
//            {
//                new Person("Bnaya", "CTO", DateTime.Now.AddHours(-1)){ Id = Guid.NewGuid() },
//                new Person("Nadav", "Co-founder", DateTime.Now.AddDays(1)){ Id = Guid.NewGuid() }
//            };
//            BulkResponse response = await _client.IndexManyAsync(
//                                        inputs, 
//                                        cancellationToken: CancellationToken.None);

//            _outputHelper.WriteLine($"Errors: {response.Errors}");

//            await _clientObject.Indices.RefreshAsync(Indices.Index(PEOPLE_INDEX));


//            // GET
//            var searchResponse = await _client.SearchAsync<Person>(search =>
//             search.Size(4).
//             Query(q =>
//                q.DateRange(m =>
//                    m.Field(f => f.Date)
//                        .GreaterThan(DateTime.Today.AddDays(-2))
//                        .LessThanOrEquals(DateTime.Now.AddHours(1))
//    .Format("dd/MM/yyyy||yyyy||yyyy/MM/dd")
//    .TimeZone("+02:00")
//                        )));

//            var people = searchResponse.Documents;
//            var input = inputs[0];
//            _outputHelper.WriteLine($"Count = {people.Count}");
//            Assert.Equal(1, people.Count);
//            Person result = people.Single();
//            Assert.Equal(input, result);
//        }

//        [Fact]
//        public async Task SearchMatchAllTest()
//        {
//            throw new NotImplementedException();
//            // GET
//            //var searchResponse = await _client.SearchAsync<Person>(search =>
//            // search.Size(4).
//            // MatchAll(m => )));

//        }
//    }
//}