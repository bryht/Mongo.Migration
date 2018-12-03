using System;
using System.Collections.Generic;
using System.Linq;
using Mongo.Migration.Documents.Attributes;

namespace Mongo.Migration.Documents.Locators
{
    internal class AutomateLocator : AbstractLocator<AutomateInformation, Type>, IAutomateLocator
    {
        public override AutomateInformation? GetLocateOrNull(Type type)
        {
            if (!LocatesDictionary.ContainsKey(type))
                return null;

            LocatesDictionary.TryGetValue(type, out var value);
            return value;
        }

        public override void Locate()
        {
            var types =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(AutomateMigrationFor), true)
                where attributes != null && attributes.Length > 0
                select new {Type = t, Attributes = attributes.Cast<AutomateMigrationFor>()};

            var versions = new Dictionary<Type, AutomateInformation>();

            foreach (var type in types)
            {
                var information = type.Attributes.First().Information;
                versions.Add(type.Type, information);
            }

            LocatesDictionary = versions;
        }   
    }
}