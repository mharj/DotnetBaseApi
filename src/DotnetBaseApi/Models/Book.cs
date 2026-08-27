using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DotnetBaseApi.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [Required]
        public string BookName { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string Author { get; set; }
    }
}