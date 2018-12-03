using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using Mongo.Migration.Documents.Locators;
using Mongo.Migration.Exceptions;
using Mongo.Migration.Migrations.Locators;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Migrations
{
    internal class MigrationRunner : IMigrationRunner
    {
        private const string VERSION_FIELD = "Version";

        private readonly IAutomateLocator _automateLocator;
        private readonly IMongoClient _client;

        private readonly IMigrationLocator _migrationLocator;

        private readonly IVersionLocator _versionLocator;

        public MigrationRunner(IMigrationLocator migrationLocator, IVersionLocator versionLocator,
            IAutomateLocator automateLocator, IMongoClient client)
        : this(migrationLocator, versionLocator, automateLocator)
        {
            _client = client;
        }
        
        public MigrationRunner(IMigrationLocator migrationLocator, IVersionLocator versionLocator,
            IAutomateLocator automateLocator)
        {
            _migrationLocator = migrationLocator;
            _versionLocator = versionLocator;
            _automateLocator = automateLocator;
        }

        public void CheckVersion<TClass>(TClass instance) where TClass : class, IDocument
        {
            var type = typeof(TClass);
            var documentVersion = instance.Version.ToString();
            var latestVersion = _migrationLocator.GetLatestVersion(type);
            var currentVersion = _versionLocator.GetLocateOrNull(type) ?? latestVersion;

            if (documentVersion == currentVersion)
                return;

            if (latestVersion == documentVersion)
                return;

            if (DocumentVersion.Default() == documentVersion)
            {
                DetermineCurrentVersion(instance, currentVersion, latestVersion);
                return;
            }

            throw new VersionViolationException(currentVersion.ToString(), documentVersion, latestVersion);
        }

        public void Run(Type type, BsonDocument document)
        {
            var documentVersion = GetVersionOrDefault(document);
            // Zeitkritisch
            var latestVersion = _migrationLocator.GetLatestVersion(type);
            var currentVersion = _versionLocator.GetLocateOrNull(type) ?? latestVersion;
            var automateInformation = _automateLocator.GetLocateOrNull(type);

            if (documentVersion == currentVersion)
                return;

            if (documentVersion > currentVersion)
            {
                MigrateDown(type, currentVersion.ToString(), document);
                return;
            }

            MigrateUp(type, documentVersion, document);

            if (automateInformation != null)
            {
                UpdateMigrationOnDatabase((AutomateInformation) automateInformation, document);
            }
        }
        
        private void UpdateMigrationOnDatabase(AutomateInformation information, BsonDocument document)
        {
            if (_client == null)
            {
                throw new NoMongoClientRegisteredException();
            }
            
            var collection = _client.GetDatabase(information.DatabaseName).GetCollection<BsonDocument>(information.CollectionName);
            var filter = new BsonDocument(){{"_id", document["_id"]}};
            collection.ReplaceOne(filter, document);
        } 

        private static void DetermineCurrentVersion<TClass>(
            TClass instance,
            DocumentVersion? currentVersion,
            DocumentVersion latestVersion) where TClass : class, IDocument
        {
            if (currentVersion < latestVersion)
            {
                instance.Version = currentVersion.ToString();
                return;
            }

            instance.Version = latestVersion;
        }

        private void MigrateUp(Type type, DocumentVersion version, BsonDocument document)
        {
            var migrations = _migrationLocator.GetMigrationsGt(type, version).ToList();

            foreach (var migration in migrations)
            {
                migration.Up(document);
                SetVersion(document, migration.Version);
            }
        }

        private void MigrateDown(Type type, DocumentVersion version, BsonDocument document)
        {
            var migrations = _migrationLocator
                .GetMigrationsGtEq(type, version)
                .OrderByDescending(m => m.Version)
                .ToList();

            for (var m = 0; m < migrations.Count; m++)
            {
                if (version == migrations[m].Version)
                    break;

                migrations[m].Down(document);

                var docVersion = DetermineDocumentVersion(version, migrations, m);
                SetVersion(document, docVersion);
            }
        }

        private static DocumentVersion DetermineDocumentVersion(
            DocumentVersion version,
            List<IMigration> migrations,
            int m)
        {
            if (migrations.Last() != migrations[m])
                return migrations[m + 1].Version;
            return version;
        }

        private static string GetVersionOrDefault(BsonDocument document)
        {
            BsonValue value;
            document.TryGetValue(VERSION_FIELD, out value);

            if (value != null)
                return value.AsString;

            return DocumentVersion.Default();
        }

        private static void SetVersion(BsonDocument document, string version)
        {
            document[VERSION_FIELD] = version;
        }
    }
}