using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson;

namespace Mongo.Migration.Services.Automate
{
    public interface IMigrationQueue
    {
        void Add(AutomateInformation information, BsonDocument document);
    }
}