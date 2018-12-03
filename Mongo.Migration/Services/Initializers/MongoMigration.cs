using System;
using Mongo.Migration.Exceptions;
using Mongo.Migration.Services.DiContainer;
using MongoDB.Driver;

namespace Mongo.Migration.Services.Initializers
{
    public static class MongoMigration 
    {
        private static bool _isInitialized;

        private static ICompoentRegistry _components;
       
        public static void Initialize()
        {
            if (_isInitialized) throw new AlreadyInitializedException();

            RegisterComponents();
            
            var app = _components.Get<IApplication>();
            app.Run();

            _isInitialized = true;
        }

        public static void Initialize(MongoClient client)
        {
            if (_isInitialized) throw new AlreadyInitializedException();
            
            RegisterComponents();
            
            _components.SetInstance<IMongoClient, MongoClient>(client);

            Initialize();
        }

        private static void RegisterComponents()
        {
            if (_components != null)
            {
                return;
            }
            
            _components = new ComponentRegistry();
            _components.RegisterComponents();
        }

        public static void Reset()
        {
            _isInitialized = false;
            _components = null;
        }
    }
}