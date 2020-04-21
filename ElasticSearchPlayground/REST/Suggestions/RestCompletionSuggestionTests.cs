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
    public sealed class RestCompletionSuggestionTests : RestSuggestionBase
    {
        private const string INDEX = "test-index-keyword-completion";

        #region Ctor

        public RestCompletionSuggestionTests(ITestOutputHelper outputHelper)
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
            JsonDocument json = await SearchAsync(@"Json\Queries\completer\search.json");

            JsonElement completer = json.GetElement("suggest", "completer");
            JsonElement found = completer[0].GetElement("options");
            var items = found.EnumerateArray().ToArray();

            Assert.Single(items, e => e.Get<string>("text") == "Animal");
            Assert.Single(items, e => e.Get<string>("text") == "animal");
            Assert.Equal(2, items.Length);
        }

        #endregion Search_Test

        #region Search_Fuzzy_Test

        [Fact]
        public async Task Search_Fuzzy_Test()
        {
            JsonDocument json = await SearchAsync(@"Json\Queries\completer\search-fuzzy.json");

            JsonElement completer = json.GetElement("suggest", "completer");
            JsonElement found = completer[0].GetElement("options");
            var items = found.EnumerateArray().ToArray();

            Assert.Single(items, e => e.Get<string>("text") == "Animal");
            Assert.Single(items, e => e.Get<string>("text") == "animal");
            Assert.Equal(2, items.Length);
        }

        #endregion Search_Fuzzy_Test

        #region Analyze_Test 

        [Fact]
        public async Task Analyze_Test()
        {
            JsonDocument json = await AnalyzeAsync(@"Json\Queries\completer\analyze.json");

            JsonElement tokensCollection = json.GetElement("tokens");
            var tokens = tokensCollection.EnumerateArray().ToArray();
            Assert.Single(tokens);
            string token = tokens[0].GetRawText("token");
            Assert.Equal(@"""the\u001Fquick\u001Fbrown\u001Ffox\u001Fjumps\u001Fover\u001Fthe\u001Flazy\u001Fdog""",
                token);
        }

        #endregion Analyze_Test

        #region Mapping_Test 

        [Fact]
        public async Task Mapping_Test()
        {
            JsonDocument json = await MappingAsync("related");

            string type = json.Get<string>(INDEX, "mappings", "related", "mapping", "related", "type");
            Assert.Equal("completion", type);
        }

        #endregion Mapping_Test
    }
}
