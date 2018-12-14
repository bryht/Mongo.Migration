using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Services.Automate
{
    public class MigrationQueue : IMigrationQueue
    {
        private const int UPDATE_DELAY = 250;

        private readonly IMongoClient _client;

        private readonly Dictionary<AutomateInformation, Stack<BsonDocument>> _multipleQueue =
            new Dictionary<AutomateInformation, Stack<BsonDocument>>();

        private Timer _timer;

        public MigrationQueue(IMongoClient client)
        {
            _client = client;
        }

        public void Add(AutomateInformation information, BsonDocument document)
        {
            if (!_multipleQueue.ContainsKey(information)) _multipleQueue[information] = new Stack<BsonDocument>();

            _multipleQueue[information].Push(document);

            if (_timer == null) StartTimer();
        }

        private void ScheduleWork(object obj)
        {
            if (_client == null) throw new NoMongoClientRegisteredException();

            StopTimerIfEmpty(_multipleQueue);

            foreach (var queue in _multipleQueue.ToList())
            {
                var thread = new Thread(() => { ProcessEntry(queue); });
                thread.Start();
            }
        }

        private void StartTimer()
        {
            Console.WriteLine("timer stared");
            _timer = new Timer(ScheduleWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(UPDATE_DELAY));
        }

        private void StopTimerIfEmpty(Dictionary<AutomateInformation, Stack<BsonDocument>> multipleQueue)
        {
            var stopTimer = true;
            foreach (var queue in multipleQueue)
                if (queue.Value.Count > 0)
                    stopTimer = false;

            if (!stopTimer) return;

            Console.WriteLine("timer stoped");
            _timer.Dispose();
            _timer = null;
        }

        private void ProcessEntry(KeyValuePair<AutomateInformation, Stack<BsonDocument>> queue)
        {
            var information = queue.Key;
            var stack = queue.Value;

            Console.WriteLine("entry:" + queue.Value.Count + "\n");

            var collection = _client.GetDatabase(information.DatabaseName)
                .GetCollection<BsonDocument>(information.CollectionName);

            var bulkOperations = new List<WriteModel<BsonDocument>>();

            for (var i = 0; i < stack.Count; i++)
            {
                var document = stack.Pop();
                if (document == null)
                {
                    continue;
                }
                
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", document["_id"]) & builder.Not(builder.Eq("Version", document["Version"])); 
                
                var replaceModel = new ReplaceOneModel<BsonDocument>(filter, document) {IsUpsert = true};
                bulkOperations.Add(replaceModel);
            }

            Console.WriteLine("bulk:" + bulkOperations.Count + "\n");

            if (bulkOperations.Count == 0) return;

            collection.BulkWrite(bulkOperations);
        }
    }
}