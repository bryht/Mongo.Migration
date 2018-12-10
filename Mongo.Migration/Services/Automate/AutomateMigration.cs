using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Services.Automate
{
    internal class AutomateMigration : IAutomateMigration
    {
        private const int UPDATE_DELAY = 100;

        private readonly Dictionary<AutomateInformation, List<BsonDocument>> _cache =
            new Dictionary<AutomateInformation, List<BsonDocument>>();

        private readonly IMongoClient _client;

        private Timer _timer;

        public AutomateMigration(IMongoClient client)
        {
            _client = client;
            //_timer = new Timer(async _ => await Flush());
        }

        public async Task Flush()
        {
            foreach (var entry in _cache)
            {
                if (_client == null) throw new NoMongoClientRegisteredException();

                var information = entry.Key;
                var documents = entry.Value;

                var collection = _client.GetDatabase(information.DatabaseName)
                    .GetCollection<BsonDocument>(information.CollectionName);

                var bulkOperations = new List<WriteModel<BsonDocument>>();

                foreach (var document in documents)
                {
                    var filter = new BsonDocument {{"_id", document["_id"]}};
                    var replaceModel = new ReplaceOneModel<BsonDocument>(filter, document) {IsUpsert = true};
                    bulkOperations.Add(replaceModel);
                }

                await collection.BulkWriteAsync(bulkOperations)
                    .ContinueWith(t =>
                    {
                        Console.WriteLine("write: " + entry.Value.Count);
                        _cache.Remove(entry.Key);
                        _timer.Dispose();
                        _timer = null;
                    });
            }
        }

        public void Add(AutomateInformation information, BsonDocument document)
        {
            var collection = _client.GetDatabase(information.DatabaseName)
                .GetCollection<BsonDocument>(information.CollectionName);
            collection.ReplaceOne(new BsonDocument {{"_id", document["_id"]}}, document);
            /*ResetTimer();

            AddOrCreateEntry(information, document);*/
        }

//        private void ResetTimer()
//        {
//            _timer.Change(TimeSpan.FromMilliseconds(UPDATE_DELAY), TimeSpan.FromMilliseconds(UPDATE_DELAY));
//        }
//
//        private void AddOrCreateEntry(AutomateInformation information, BsonDocument document)
//        {
//            if (_cache.ContainsKey(information))
//            {
//                _cache[information].Add(document);
//                return;
//            }
//
//            _cache[information] = new List<BsonDocument> {document};
//        }
    }
}