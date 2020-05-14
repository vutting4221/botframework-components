using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using SkillServiceLibrary.Utilities;

namespace PointOfInterestSkill.FactoryStorage
{
    public class TurnContextAwareStorageFactory : ITurnContextAwareStorage
    {
        private readonly IStorage _storage;
        private readonly IRestStorage _restStorage;

        public TurnContextAwareStorageFactory(IStorage storage, IRestStorage restStorage)
        {
            _storage = storage;
            _restStorage = restStorage;
        }

        public async Task<IDictionary<string, object>> ReadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.IsSkill())
            {
                return await _restStorage.ReadAsync(turnContext, cancellationToken);
            }
            else
            {
                var storageKey = GetStorageKey(turnContext);
                var items = await _storage.ReadAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
                return items;
            }
        }

        public async Task WriteAsync(ITurnContext turnContext, IDictionary<string, object> dataDict, CancellationToken cancellationToken = default)
        {
            if (turnContext.IsSkill())
            {
                await _restStorage.WriteAsync(turnContext, dataDict, cancellationToken);
            }
            else
            {
                var key = GetStorageKey(turnContext);
                var changes = new Dictionary<string, object>
                {
                    { key, dataDict },
                };
                await _storage.WriteAsync(changes).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        private string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }
    }
}
