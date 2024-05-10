using FreeSql.DataAnnotations;

namespace MockupServer.Models
{
    [Table(Name = "global")]
    public class GlobalCache
    {
        public int ListenerPort { get; set; } = 9550;
    }
}
