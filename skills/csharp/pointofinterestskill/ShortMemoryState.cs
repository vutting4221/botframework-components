using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using SkillServiceLibrary.Utilities;

namespace PointOfInterestSkill
{
    public class ShortMemoryState
    {
        public const string ShortMemoryPropertyName = "ShortMemory";
        private readonly string _contextServiceKey;
        private IRestStorage _restStorage;
        private IStorage _storage;

        public ShortMemoryState(IStorage storage, IRestStorage restStorage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _restStorage = restStorage ?? throw new ArgumentNullException(nameof(restStorage));
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
        public async Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
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

                if (turnContext.IsSkill())
                {
                    val = await _restStorage.ReadAsync(turnContext, cancellationToken);
                }
                else
                {
                    items = await _storage.ReadAsync(new[] { storageKey }, cancellationToken).ConfigureAwait(false);
                    items.TryGetValue(storageKey, out val);
                }

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
        public async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var cachedState = turnContext.TurnState.Get<CachedShortMemoryState>(_contextServiceKey);
            if (cachedState != null && (force || cachedState.IsChanged()))
            {
                var key = GetStorageKey(turnContext);
                var changes = new Dictionary<string, object>
                {
                    { key, cachedState.State },
                };

                if (turnContext.IsSkill())
                {
                    await _restStorage.WriteAsync(turnContext, cachedState.State, cancellationToken);
                }
                else
                {
                    await _storage.WriteAsync(changes).ConfigureAwait(false);
                }

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
        public Task ClearStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
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
        /// Gets a copy of the raw cached data for this <see cref="BotState"/> from the turn context.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>A JSON representation of the cached state.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="turnContext"/> is <c>null</c>.</exception>
        public JToken Get(ITurnContext turnContext)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var stateKey = this.GetType().Name;
            var cachedState = turnContext.TurnState.Get<object>(stateKey);
            return JObject.FromObject(cachedState)["State"];
        }

        /// <summary>
        /// When overridden in a derived class, gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        public string GetStorageKey(ITurnContext turnContext)
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
        public Task<T> GetPropertyValueAsync<T>(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task DeletePropertyValueAsync(ITurnContext turnContext, string propertyName, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task SetPropertyValueAsync(ITurnContext turnContext, string propertyName, object value, CancellationToken cancellationToken = default(CancellationToken))
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