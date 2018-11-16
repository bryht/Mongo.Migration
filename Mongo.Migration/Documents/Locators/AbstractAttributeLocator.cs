/*using System;
using System.Collections.Generic;
using System.Linq;
using Mongo.Migration.Documents.Attributes;

namespace Mongo.Migration.Documents.Locators
{
    public class AbstractAttributeLocator<TAttributeType, TReturnType> where TAttributeType : Attribute where TReturnType : class
    {
        private IDictionary<Type, TReturnType> _typeInformations;

        private IDictionary<Type, TReturnType> Versions
        {
            get
            {
                if (_typeInformations == null)
                    LoadVersions();
                return _typeInformations;
            }
        }

        public TReturnType? GetCurrentVersion(Type type)
        {
            if (!Versions.ContainsKey(type))
                return null;

            TReturnType value;
            Versions.TryGetValue(type, out value);
            return value;
        }

        public void LoadVersions()
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
                var version = type.Attributes.First(a => a.GetType() is CurrentVersion).Version;
                versions.Add(type.Type, version);
            }

            _typeInformations = versions;
        }
    }
}*/