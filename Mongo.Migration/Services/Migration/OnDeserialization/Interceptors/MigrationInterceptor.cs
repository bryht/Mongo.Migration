﻿using Mongo.Migration.Documents;
using Mongo.Migration.Migrations;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mongo.Migration.Services.Migration.OnDeserialization.Interceptors
{
    internal class MigrationInterceptor<TDocument> : BsonClassMapSerializer<TDocument> where TDocument : class, IDocument
    {
        private readonly IMigrationRunner _migrationRunner;

        public MigrationInterceptor(IMigrationRunner migrationRunner)
            : base(BsonClassMap.LookupClassMap(typeof(TDocument)))
        {
            _migrationRunner = migrationRunner;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TDocument value)
        {
            _migrationRunner.CheckVersion(value);

            base.Serialize(context, args, value);
        }

        public override TDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // TODO: Performance? LatestVersion, dont do anything
            var document = BsonDocumentSerializer.Instance.Deserialize(context);
            
            _migrationRunner.Run(typeof(TDocument), document);
            
            var migratedContext =
                BsonDeserializationContext.CreateRoot(new BsonDocumentReader(document));

            return base.Deserialize(migratedContext, args);
        }
    }
}