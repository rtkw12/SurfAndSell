using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Util.Logging;

internal class LogStorage
{
    public LogStorage()
    {

    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Message { get; set; }
    public int? ErrorCode { get; set; }
}