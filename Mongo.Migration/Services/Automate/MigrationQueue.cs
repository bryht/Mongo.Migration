using System;
using System.Collections.Generic;
using System.Linq;
using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Services.Automate
{
    public class MigrationQueue : IMigrationQueue
    {
        private readonly IMongoClient _client;
        private const int UPDATE_DELAY = 1000;

        private readonly Dictionary<AutomateInformation, Stack<BsonDocument>> _multipleQueue =
            new Dictionary<AutomateInformation, Stack<BsonDocument>>();

        public MigrationQueue(IMongoClient client)
        {
            _client = client;
        }
        
        private void Flush()
        {
            foreach (var entry in _multipleQueue)
            {
                if (_client == null) throw new NoMongoClientRegisteredException();

                var information = entry.Key;
                var stack = entry.Value;

                var collection = _client.GetDatabase(information.DatabaseName)
                    .GetCollection<BsonDocument>(information.CollectionName);

                var bulkOperations = new List<WriteModel<BsonDocument>>();
                
                foreach (var element in stack)
                {
                    var document = stack.Pop();
                    var filter = new BsonDocument {{"_id", document["_id"]}};
                    var replaceModel = new ReplaceOneModel<BsonDocument>(filter, document) {IsUpsert = true};
                    bulkOperations.Add(replaceModel);
                }

                collection.BulkWrite(bulkOperations);
            }
        }
        
        public void Add(AutomateInformation information, BsonDocument document)
        {
            if (!_multipleQueue.ContainsKey(information))
            {
                _multipleQueue[information] = new Stack<BsonDocument>();
            }
            
            _multipleQueue[information].Push(document);
        }
    }
}