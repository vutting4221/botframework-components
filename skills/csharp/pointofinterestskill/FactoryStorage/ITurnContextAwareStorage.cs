using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace PointOfInterestSkill.FactoryStorage
{
    public interface ITurnContextAwareStorage
    {
        Task<IDictionary<string, object>> ReadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default);

        Task WriteAsync(ITurnContext turnContext, IDictionary<string, object> data, CancellationToken cancellationToken = default);
    }
}
