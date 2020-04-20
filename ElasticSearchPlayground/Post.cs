using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearchPlayground
{
    public class PostResponse : ElasticsearchResponse<Post>
    {
    }
    public class Post : IEquatable<Post>
    {
        public Post()
        {

        }

        public Post(string title, string body, DateTimeOffset? date = null)
        {
            Title = title;
            Body = body;
            Date = date;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTimeOffset? Date { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Post);
        }

        public bool Equals(Post other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   Title == other.Title &&
                   Body == other.Body &&
                   EqualityComparer<DateTimeOffset?>.Default.Equals(Date, other.Date);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Body, Date);
        }

        public static bool operator ==(Post left, Post right)
        {
            return EqualityComparer<Post>.Default.Equals(left, right);
        }

        public static bool operator !=(Post left, Post right)
        {
            return !(left == right);
        }
    }
}
