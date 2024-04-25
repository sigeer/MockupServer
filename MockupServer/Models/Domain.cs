using FreeSql.DataAnnotations;

namespace MockupServer.Models
{
    [Table(Name = "domain")]
    public class Domain
    {
        protected Domain() { }
        public Domain(string urlPrefix, string target)
        {
            UrlPrefix = urlPrefix;
            Target = target;
        }

        [Column(IsPrimary = true)]
        public string UrlPrefix { get; set; } = "/";
        public string Target { get; set; } = "http://localhost";
    }
}
