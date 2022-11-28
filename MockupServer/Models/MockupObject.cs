using System.ComponentModel.DataAnnotations.Schema;

namespace MockupServer.Models
{
    [Table("mockupobject")]
    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class MockupObject
    {
        public string RequestUrl { get; set; } = null!;
        public string ResponseData { get; set; } = null!;
    }
}
