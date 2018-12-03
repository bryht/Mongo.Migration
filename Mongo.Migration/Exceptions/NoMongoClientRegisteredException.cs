using System;

namespace Mongo.Migration.Exceptions
{
    public class NoMongoClientRegisteredException : Exception
    {
        public NoMongoClientRegisteredException()
            : base(string.Format(ErrorTexts.NoMongoClientRegistered))
        {
        }
    }
}