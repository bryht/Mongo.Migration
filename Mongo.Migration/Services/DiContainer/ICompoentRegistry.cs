namespace Mongo.Migration.Services.DiContainer
{
    internal interface ICompoentRegistry
    {
        void RegisterComponents();

        TComponent Get<TComponent>() where TComponent : class;

        void SetInstance<TInterface, TInstance>(TInstance implementation)
            where TInterface : class where TInstance : class;
    }
}