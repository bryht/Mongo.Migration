namespace Mongo.Migration.Documents.Attributes
{
    public struct AutomateInformation
    {
        public AutomateInformation(string databaseName, string collectionName)
        {
            DatabaseName = databaseName;
            CollectionName = collectionName;
        }
        
        public string DatabaseName { get; }
        public string CollectionName { get; }
    }
}