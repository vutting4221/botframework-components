using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace PointOfInterestSkill
{
    /// <summary>
    /// Implements an <see cref="IStatePropertyAccessor{T}"/> for a property container.
    /// Note the semantics of this accessor are intended to be lazy, this means the Get, Set and Delete
    /// methods will first call LoadAsync. This will be a no-op if the data is already loaded.
    /// The implication is you can just use this accessor in the application code directly without first calling LoadAsync
    /// this approach works with the AutoSaveStateMiddleware which will save as needed at the end of a turn.
    /// </summary>
    /// <typeparam name="T">type of value the propertyAccessor accesses.</typeparam>
    public class ShortMemoryStatePropertyAccessor<T> : IStatePropertyAccessor<T>
    {
        private ShortMemoryState _botState;

        public ShortMemoryStatePropertyAccessor(ShortMemoryState botState, string name)
        {
            _botState = botState;
            Name = name;
        }

        public string Name { get; private set; }

        /// <summary>
        /// Delete the property. The semantics are intended to be lazy, note the use of LoadAsync at the start.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await _botState.LoadAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await _botState.DeletePropertyValueAsync(turnContext, Name, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the property value. The semantics are intended to be lazy, note the use of LoadAsync at the start.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="defaultValueFactory">Defines the default value. Invoked when no value been set for the requested state property.  If defaultValueFactory is defined as null, the MissingMemberException will be thrown if the underlying property is not set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<T> GetAsync(ITurnContext turnContext, Func<T> defaultValueFactory, CancellationToken cancellationToken)
        {
            T result = default(T);

            await _botState.LoadAsync(turnContext, true, cancellationToken).ConfigureAwait(false);

            try
            {
                // if T is a value type, lookup up will throw key not found if not found, but as perf
                // optimization it will return null if not found for types which are not value types (string and object).
                result = await _botState.GetPropertyValueAsync<T>(turnContext, Name, cancellationToken).ConfigureAwait(false);

                if (result == null && defaultValueFactory != null)
                {
                    // use default Value Factory and save default value for any further calls
                    result = defaultValueFactory();
                    await SetAsync(turnContext, result, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (KeyNotFoundException)
            {
                if (defaultValueFactory != null)
                {
                    // use default Value Factory and save default value for any further calls
                    result = defaultValueFactory();
                    await SetAsync(turnContext, result, cancellationToken).ConfigureAwait(false);
                }
            }

            return result;
        }

        /// <summary>
        /// Set the property value. The semantics are intended to be lazy, note the use of LoadAsync at the start.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="value">value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetAsync(ITurnContext turnContext, T value, CancellationToken cancellationToken)
        {
            await _botState.LoadAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await _botState.SetPropertyValueAsync(turnContext, Name, value, cancellationToken).ConfigureAwait(false);
        }
    }
}
