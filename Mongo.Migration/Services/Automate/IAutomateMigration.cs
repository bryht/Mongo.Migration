using System.Threading.Tasks;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson;

namespace Mongo.Migration.Services.Automate
{
    interface IAutomateMigration
    {
        void Add(AutomateInformation information, BsonDocument document);

        Task Flush();
    }
}