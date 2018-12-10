using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Services.Automate
{
    internal class AutomateMigration : IAutomateMigration
    {
        private readonly IMongoClient _client;
        private readonly IMigrationQueue _queue;

        public AutomateMigration(IMongoClient client, IMigrationQueue queue)
        {
            _client = client;
            _queue = queue;
        }

        public void UpdateOrQueue(AutomateInformation information, BsonDocument document)
        {
            if (information.DelayedUpdate)
            {
                _queue.Add(information, document);
                return;
            }

            _client.GetDatabase(information.DatabaseName)
                .GetCollection<BsonDocument>(information.CollectionName)
                .ReplaceOne(
                    new BsonDocument {{"_id", document["_id"]}}, document
                );
        }
    }
}