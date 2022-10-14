namespace MockupServer
{
    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class MockupObject
    {
        public string RequestUrl { get; set; }
        public object ResponseData { get; set; }
    }
}
