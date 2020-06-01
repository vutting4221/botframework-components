using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SymptomBasedSkill.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SymptomBasedSkill.ServiceBusHandler
{
    public class ProcessData : IProcessData
    {
        private readonly IBot bot;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly BotServices botServices;

        public ProcessData(IBotFrameworkHttpAdapter httpAdapter, IBot bot, BotServices botServices)
        {
            this.bot = bot;
            this.botServices = botServices;
            this._adapter = httpAdapter;
        }

        public async Task<ServiceResponse> Process(string myPayload)
        {
            var activity = new Activity
            {
                Type = ActivityTypes.Event,
                ChannelId = "FaceMaskDetectorNotification",
                Conversation = new ConversationAccount(id: $"{Guid.NewGuid()}"),
                From = new ChannelAccount(id: $"Notification.FaceMaskDetector", name: $"Notification.SymptomBasedSkill"),
                Recipient = new ChannelAccount(id: $"Notification.FaceMaskDetector", name: $"Notification.SymptomBasedSkill"),
                Name = "Proactive",
                Value = JsonConvert.SerializeObject(myPayload)
            };

            await bot.OnTurnAsync(new TurnContext((BotAdapter)_adapter, activity), CancellationToken.None);

            return new ServiceResponse(HttpStatusCode.NoContent, string.Empty);
            Console.WriteLine(myPayload);
        }

        public class ServiceResponse
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceResponse"/> class.</summary>
            /// <param name="code">indicates the overall status of the operation.</param>
            /// <param name="message">provides details about the status of the operation.</param>
            public ServiceResponse(
                HttpStatusCode code,
                string message)
            {
                this.Message = message;
                this.Code = code;
            }

            /// <summary>Gets a code indicating the overall status of the operation.</summary>
            public HttpStatusCode Code { get; }

            /// <summary>Gets a message that provides details about the status of the operation.</summary>
            public string Message { get; }
        }
    }
}
