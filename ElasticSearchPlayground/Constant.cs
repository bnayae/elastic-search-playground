// Setup docker: docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk

// suggestion: https://www.elastic.co/guide/en/elasticsearch/reference/current/search-suggesters.html#search-suggesters

// check: https://discuss.elastic.co/t/autocomplete-across-multiple-fields-with-nest-client/120878/2

namespace ElasticSearchPlayground
{
    public static class Constant
    {
        public static string[] Words =
            {
            "Him",
            "peace ",
            "go",
            "matter",
            "dome",
            "hOly",
            "brow",
            "substance",
            "rake",
            "reverie",
            "raw material", 
            "drugs",
            "might",
            "went",
            "material"
        };
    }
}