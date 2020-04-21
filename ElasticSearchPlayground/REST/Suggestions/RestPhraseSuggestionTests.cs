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
    public sealed class RestPhraseSuggestionTests : RestSuggestionBase
    {
        private const string INDEX = "test-index-phrase-suggest";

        #region Ctor

        public RestPhraseSuggestionTests(ITestOutputHelper outputHelper)
            :base(outputHelper, INDEX)
        {
        }

        #endregion Ctor

        #region Create_Index_Test

        [Fact]
        public Task Create_Index_Test() =>
                CreateIndexAsync(true);

        #endregion Create_Index_Test

        #region Create_Docs_Test

        [Fact]
        public Task Create_Docs_Test() => CreateDocsAsync(true);

        #endregion Create_Docs_Test

        #region Search_Test

        [Fact]
        public async Task Search_Test()
        {
            JsonDocument json = await SearchAsync(@"Json\Queries\phrase\search.json");

            JsonElement found = json.GetElement("suggest", "phrase-lookup");
            var items = found.EnumerateArray().ToArray();
            
            Assert.Single(items);

            var options = items[0].GetElement("options").EnumerateArray().ToArray();
            Assert.Single(options, e => e.Get<string>("text") == "quick brown fox");
        }

        #endregion Search_Test

        #region Search_Confidence_Test

        [Fact]
        public async Task Search_Confidence_Test()
        {
            JsonDocument json = await SearchAsync(@"Json\Queries\phrase\search-confidence.json");

            JsonElement found = json.GetElement("suggest", "phrase-lookup");
            var items = found.EnumerateArray().ToArray();
            
            Assert.Single(items);

            var options = items[0].GetElement("options").EnumerateArray().ToArray();
            Assert.Single(options, e => e.Get<string>("text") == "quick brown fox jump");
        }

        #endregion Search_Confidence_Test

        #region Analyze_Test 

        [Fact]
        public async Task Analyze_Test()
        {
            JsonDocument json = await AnalyzeAsync(@"Json\Queries\phrase\analyze.json");

            JsonElement tokensCollection = json.GetElement("tokens");
            var tokens = tokensCollection.EnumerateArray().Select(m => m.Get<string>("token")).ToArray();
            Assert.Equal(22, tokens.Length);
            Assert.Single(tokens, token => token == "quick");
            Assert.Single(tokens, token => token == "quick brown");
            Assert.Single(tokens, token => token == "quick brown fox");
            Assert.DoesNotContain(tokens, token => token == "quick brown fox jumps");
            Assert.DoesNotContain(tokens, token => token == "the");
        }

        #endregion Analyze_Test

        #region Mapping_Test 

        [Fact]
        public async Task Mapping_Test()
        {
            JsonDocument json = await MappingAsync("title");

            string typebase = json.Get<string>(INDEX, "mappings", "title", "mapping", "title", "type");
            Assert.Equal("keyword", typebase);
            string type = json.Get<string>(INDEX, "mappings", "title", "mapping", "title", "fields", "phrasey", "type");
            Assert.Equal("text", type);
            string analyzer = json.Get<string>(INDEX, "mappings", "title", "mapping", "title", "fields", "phrasey", "analyzer");
            Assert.Equal("phrase-analyzer", analyzer);
        }

        #endregion Mapping_Test
    }
}
