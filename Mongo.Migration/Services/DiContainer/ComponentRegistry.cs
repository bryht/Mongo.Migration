﻿using LightInject;
using Mongo.Migration.Documents.Locators;
using Mongo.Migration.Documents.Serializers;
using Mongo.Migration.Migrations;
using Mongo.Migration.Migrations.Locators;
using Mongo.Migration.Services.Automate;
using Mongo.Migration.Services.Interceptors;
using Mongo.Migration.Services.MongoDB;
using MongoDB.Driver;

namespace Mongo.Migration.Services.DiContainer
{
    internal class ComponentRegistry : ICompoentRegistry
    {
        private readonly ServiceContainer _container;

        public ComponentRegistry()
        {
            _container = new ServiceContainer();
        }

        public void RegisterComponents()
        {
            _container.Register<DocumentVersionSerializer, DocumentVersionSerializer>();
            _container.Register<MigrationInterceptorProvider, MigrationInterceptorProvider>();
            _container.Register<IMigrationLocator, TypeMigrationLocator>(new PerContainerLifetime());
            _container.Register<IVersionLocator, VersionLocator>(new PerContainerLifetime());
            _container.Register<IAutomateLocator, AutomateLocator>(new PerContainerLifetime());

            _container.Register<IMigrationRunner, MigrationRunner>();
            _container.Register<IMigrationInterceptorFactory, MigrationInterceptorFactory>();
            _container.Register<IMongoRegistrator, MongoRegistrator>();
            _container.Register<IApplication, Application>();
        }

        public void SetMongoClient(MongoClient implementation)
        {
            _container.RegisterInstance(typeof(IMongoClient), implementation);
            _container.Register<IAutomateMigration, AutomateMigration>(new PerContainerLifetime());
            _container.Register<IMigrationQueue, MigrationQueue>(new PerContainerLifetime());
        }

        public TComponent Get<TComponent>() where TComponent : class
        {
            return _container.GetInstance<TComponent>();
        }
    }
}