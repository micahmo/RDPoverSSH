using LiteDB;

namespace RDPoverSSH.Models
{
    public class ConnectionModel
    {
        [BsonId]
        public int ObjectId { get; set; }

        public string Name { get; set; }
    }
}
