using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using SkillServiceLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PointOfInterestSkill.FactoryStorage
{
    public class StorageFactory : IStorageTypes
    {
        private readonly IStorage _storage;
        private readonly HttpClient _httpClient;
        private readonly IStorageExtended _turnContextAwareStorage;

        public StorageFactory(IStorage storage, IStorageExtended turnContextAwareStorage, HttpClient httpClient)
        {
            _storage = storage;
            _turnContextAwareStorage = turnContextAwareStorage;
            _httpClient = httpClient;
        }

        public IStorage GetIStorage(ITurnContext turnContext)
        {
            if (turnContext.IsSkill())
            {
                return _turnContextAwareStorage;
            }
            else
            {
                return _storage;
            }
        }

    }
}
