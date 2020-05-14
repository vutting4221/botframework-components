using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using PointOfInterestSkill.FactoryStorage;
using SkillServiceLibrary.Utilities;

namespace PointOfInterestSkill
{
    public class ShortMemoryState : UserState
    {
        public const string ShortMemoryPropertyName = "ShortMemory";
        private readonly string _contextServiceKey;
        private readonly ITurnContextAwareStorage _storageFactory;
        private IRestStorage _restStorage;
        private IStorage _storage;

        //public ShortMemoryState(IStorage storage, IRestStorage restStorage)
        //{
        //    _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        //    _restStorage = restStorage ?? throw new ArgumentNullException(nameof(restStorage));
        //    _contextServiceKey = nameof(ShortMemoryState);
        //}

        public ShortMemoryState(ITurnContextAwareStorage storageFactory, IStorage storage)
            : base(storage)
        {
            _storageFactory = storageFactory;
            _contextServiceKey = nameof(ShortMemoryState);
        }

        /// <summary>
        /// Creates a named state property within the scope of a <see cref="BotState"/> and returns
        /// an accessor for the property.
        /// </summary>
        /// <typeparam name="T">The value type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>An accessor for the property.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        public IStatePropertyAccessor<T> CreateProperty<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new ShortMemoryStatePropertyAccessor<T>(this, name);
        }

        /// <summary>
        /// Populates the state cache for this <see cref="BotState"/> from the storage layer.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to overwrite any existing state cache;
        /// or <c>false</c> to load state from storage only if the cache doesn't already exist.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public override async Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);
            var storageKey = GetStorageKey(turnContext);
            if (force || cachedState == null || cachedState.State == null)
            {
                IDictionary<string, object> items;
                object val;

                val = await _storageFactory.ReadAsync(turnContext, cancellationToken);

                if (val is IDictionary<string, object> asDictionary)
                {
                    turnContext.TurnState[_contextServiceKey] = new CachedShortMemoryState(asDictionary);
                }
                else if (val is JObject asJobject)
                {
                    // If types are not used by storage serialization, and Newtonsoft is the serializer
                    // the item found will be a JObject.
                    turnContext.TurnState[_contextServiceKey] = new CachedShortMemoryState(asJobject.ToObject<IDictionary<string, object>>());
                }
                else if (val == null)
                {
                    // This is the case where the dictionary did not exist in the store.
                    turnContext.TurnState[_contextServiceKey] = new CachedShortMemoryState();
                }
                else
                {
                    // This should never happen
                    throw new InvalidOperationException("Data is not in the correct format for BotState.");
                }
            }
        }

        /// <summary>
        /// Writes the state cache for this <see cref="BotState"/> to the storage layer.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="force">Optional, <c>true</c> to save the state cache to storage;
        /// or <c>false</c> to save state to storage only if a property in the cache has changed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public override async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);
            if (cachedState != null && (force || cachedState.IsChanged()))
            {
                await _storageFactory.WriteAsync(turnContext, cachedState.State, cancellationToken);
                cachedState.Hash = cachedState.ComputeHash(cachedState.State);
                return;
            }
        }

        /// <summary>
        /// Clears the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This method clears the state cache in the turn context. Call
        /// <see cref="SaveChangesAsync(ITurnContext, bool, CancellationToken)"/> to persist this
        /// change in the storage layer.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public override Task ClearStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Explicitly setting the hash will mean IsChanged is always true. And that will force a Save.
            turnContext.TurnState[_contextServiceKey] = new CachedShortMemoryState { Hash = string.Empty };

            return Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a derived class, gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            var channelId = turnContext.Activity.ChannelId ?? throw new ArgumentNullException("invalid activity-missing channelId");
            var userId = turnContext.Activity.From?.Id ?? throw new ArgumentNullException("invalid activity-missing From.Id");
            return $"{channelId}/users/{userId}";
        }

        /// <summary>
        /// Gets the value of a property from the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <typeparam name="T">The value type of the property.</typeparam>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains the property value, otherwise it will be default(T).</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
        public new Task<T> GetPropertyValueAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);

            if (cachedState.State.TryGetValue(propertyName, out object result))
            {
                if (result is T t)
                {
                    return Task.FromResult(t);
                }

                if (result == null)
                {
                    return Task.FromResult(default(T));
                }

                // If types are not used by storage serialization, and Newtonsoft is the serializer,
                // use Newtonsoft to convert the object to the type expected.
                if (result is JObject jObj)
                {
                    return Task.FromResult(jObj.ToObject<T>());
                }

                if (result is JArray jarray)
                {
                    return Task.FromResult(jarray.ToObject<T>());
                }

                // attempt to convert result to T using json serializer.
                return Task.FromResult(JToken.FromObject(result).ToObject<T>());
            }

            if (typeof(T).IsValueType)
            {
                throw new KeyNotFoundException(propertyName);
            }

            return Task.FromResult(default(T));
        }

        /// <summary>
        /// Deletes a property from the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
        public new Task DeletePropertyValueAsync(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);
            cachedState.State.Remove(propertyName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the value of a property in the state cache for this <see cref="BotState"/>.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to set on the property.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> or
        /// <paramref name="propertyName"/> is <c>null</c>.</exception>
        public new Task SetPropertyValueAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);
            cachedState.State[propertyName] = value;
            return Task.CompletedTask;
        }
    }
}