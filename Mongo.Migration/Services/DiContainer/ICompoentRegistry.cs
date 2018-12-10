using MongoDB.Driver;

namespace Mongo.Migration.Services.DiContainer
{
    internal interface ICompoentRegistry
    {
        void RegisterComponents();

        TComponent Get<TComponent>() where TComponent : class;

        void SetMongoClient(MongoClient implementation);
    }
}