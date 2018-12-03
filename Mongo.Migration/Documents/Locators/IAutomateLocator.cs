using System;
using Mongo.Migration.Documents.Attributes;

namespace Mongo.Migration.Documents.Locators
{
    public interface IAutomateLocator : ILocator<AutomateInformation, Type>
    {
    }
}