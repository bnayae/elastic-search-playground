// Setup docker: docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/elk

// suggestion: https://www.elastic.co/guide/en/elasticsearch/reference/current/search-suggesters.html#search-suggesters

// check: https://discuss.elastic.co/t/autocomplete-across-multiple-fields-with-nest-client/120878/2

namespace ElasticSearchPlayground
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string[] Related { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Id}): [{string.Join(", ", Related)}]";
        }
    }
}