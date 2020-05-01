using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace PointOfInterestSkill
{
    public class ShortMemoryRestStorage : IRestStorage
    {
        private const string ApiEndpoint = "state";
        private const string ShortMemoryPropertyName = "ShortMemory";
        private const string ConversationIdKeyName = "ConversationId";
        private const string PropertyNameKeyName = "PropertyName";
        private HttpClient _httpClient;

        public ShortMemoryRestStorage(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IDictionary<string, object>> ReadAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            using (var httpRequestMessage = new HttpRequestMessage())
            {
                var activity = turnContext.Activity;

                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.RequestUri = new Uri($"{activity.ServiceUrl}{ApiEndpoint}?{ConversationIdKeyName}={turnContext.Activity.Conversation.Id}&{PropertyNameKeyName}={ShortMemoryPropertyName}");

                var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<StateRestPayload>(responseString);

                return responseData.Data;
            }
        }

        public async Task WriteAsync(ITurnContext turnContext, IDictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            using (var httpRequestMessage = new HttpRequestMessage())
            {
                var activity = turnContext.Activity;

                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri($"{activity.ServiceUrl}{ApiEndpoint}");

                var payload = new StateRestPayload();
                payload.ConversationId = turnContext.Activity.Conversation.Id;
                payload.PropertyName = ShortMemoryPropertyName;
                payload.Data = data;

                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload));

                await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}