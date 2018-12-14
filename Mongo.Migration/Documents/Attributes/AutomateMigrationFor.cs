using System;
using System.ComponentModel.Design;

namespace Mongo.Migration.Documents.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutomateMigrationFor : Attribute
    {
        public AutomateMigrationFor(string databaseName, string collectionName, bool delayUpdate = false)
        {
            Information = new AutomateInformation(databaseName, collectionName, delayUpdate);
        }

        public AutomateInformation Information { get; }
    }
}