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


// "type": "text" used standard analyzer

#pragma warning disable AMNF0001 // Asynchronous method name is not ending with 'Async'

namespace ElasticSearchPlayground
{
    public sealed class RestTermSuggestionTests : RestSuggestionBase
    {
        private const string INDEX = "test-index-text-completion";

        #region Ctor

        public RestTermSuggestionTests(ITestOutputHelper outputHelper)
            : base(outputHelper, INDEX)
        {
        }

        #endregion Ctor

        #region Create_Index_Test // text use standard analyzer

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
            JsonDocument json = await SearchAsync(@"Json\Queries\term\search.json");

            JsonElement completer = json.GetElement("suggest", "spell-check-1");
            JsonElement found = completer[0];
            var items = found.EnumerateArray().ToArray();

            Assert.Single(items, e => e.Get<string>("text") == "dog");
            //Assert.Equal(2, items.Length);
        }

        #endregion Search_Test

        #region Analyze_Test 

        [Fact]
        public async Task Analyze_Test()
        {
            JsonDocument json = await AnalyzeAsync(@"Json\Queries\term\analyze.json");

            JsonElement tokensCollection = json.GetElement("tokens");
            var tokens = tokensCollection.EnumerateArray().ToArray();
            Assert.Equal(9, tokens.Length);
            Assert.All(tokens,
                item =>
                {
                    string token = item.Get<string>("token");
                    Assert.Equal(token.ToLower(), token);
                });
        }

        #endregion Analyze_Test

        #region Mapping_Test 

        [Fact]
        public async Task Mapping_Test()
        {
            JsonDocument json = await MappingAsync("title");

            string type = json.Get<string>(INDEX, "mappings", "title", "mapping", "title", "type");
            Assert.Equal("text", type);
        }

        #endregion Mapping_Test
    }
}
