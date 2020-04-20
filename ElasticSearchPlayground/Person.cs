using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearchPlayground
{
    public class PersonResponse : ElasticsearchResponse<Person>
    {
    }
    public class Person : IEquatable<Person>
    {
        public Person()
        {

        }

        public Person(string name, string role, DateTime? date = null)
        {
            Name = name;
            Role = role;
            Date = date ?? DateTime.UtcNow;
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public string Role { get; set; }
        public DateTime Date { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   Name == other.Name &&
                   Role == other.Role &&
                   Date.Equals(other.Date);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Role, Date);
        }

        public static bool operator ==(Person left, Person right)
        {
            return EqualityComparer<Person>.Default.Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !(left == right);
        }
    }
}
