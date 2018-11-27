using System;
using System.Collections.Generic;
using System.Linq;
using Mongo.Migration.Documents.Attributes;

namespace Mongo.Migration.Documents.Locators
{
    internal class VersionLocator : AbstractLocator<DocumentVersion, Type>, IVersionLocator
    {
        public override DocumentVersion? GetLocateOrNull(Type type)
        {
            if (!LocatesDictionary.ContainsKey(type))
                return null;

            DocumentVersion value;
            LocatesDictionary.TryGetValue(type, out value);
            return value;
        }

        public override void Locate()
        {
            var types =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(CurrentVersion), true)
                where attributes != null && attributes.Length > 0
                select new {Type = t, Attributes = attributes.Cast<CurrentVersion>()};

            var versions = new Dictionary<Type, DocumentVersion>();

            foreach (var type in types)
            {
                var version = type.Attributes.First().Version;
                versions.Add(type.Type, version);
            }

            LocatesDictionary = versions;
        }
    }
}