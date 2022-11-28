using System.ComponentModel.DataAnnotations.Schema;

namespace MockupServer.Models
{
    [Table("mockupobject")]
    public class MockupObject
    {
        public string RequestUrl { get; set; } = null!;
        public string ResponseData { get; set; } = null!;
    }
}
