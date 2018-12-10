namespace Mongo.Migration.Documents.Attributes
{
    public struct AutomateInformation
    {
        public AutomateInformation(string databaseName, string collectionName, bool delayedUpdate = false)
        {
            DatabaseName = databaseName;
            CollectionName = collectionName;
            DelayedUpdate = delayedUpdate;
        }
        
        public string DatabaseName { get; }
        public string CollectionName { get; }
        public bool DelayedUpdate { get; }
    }
}