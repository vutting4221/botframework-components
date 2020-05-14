using Microsoft.Bot.Builder;

namespace PointOfInterestSkill.FactoryStorage
{
    //public abstract class StorageTypes
    //{
    //    public abstract IStorage GetIStorage(ITurnContext turnContext);
    //}

    public interface IStorageTypes
    {
       public IStorage GetIStorage(ITurnContext turnContext);
    }
}
