// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Solutions.Proactive;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SymptomBasedSkill.Utilities;

namespace SymptomBasedSkill.Bots
{
    public class DefaultActivityHandler<T> : TeamsActivityHandler
        where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<ProactiveModel> _proactiveStateAccessor;
        private readonly ProactiveState _proactiveState;
        private readonly MicrosoftAppCredentials _appCredentials;

        private readonly LocaleTemplateManager _templateEngine;

        public DefaultActivityHandler(IServiceProvider serviceProvider, T dialog)
        {
            _dialog = dialog;
            _dialog.TelemetryClient = serviceProvider.GetService<IBotTelemetryClient>();
            _conversationState = serviceProvider.GetService<ConversationState>();
            _userState = serviceProvider.GetService<UserState>();
            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _templateEngine = serviceProvider.GetService<LocaleTemplateManager>();
            _proactiveState = serviceProvider.GetService<ProactiveState>();
            _proactiveStateAccessor = _proactiveState.CreateProperty<ProactiveModel>(nameof(ProactiveModel));
            _appCredentials = serviceProvider.GetService<MicrosoftAppCredentials>();
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(_templateEngine.GenerateActivityForLocale("IntroMessage"), cancellationToken);
            await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // directline speech occasionally sends empty message activities that should be ignored
            var activity = turnContext.Activity;
            if (activity.ChannelId == Channels.DirectlineSpeech && activity.Type == ActivityTypes.Message && string.IsNullOrEmpty(activity.Text))
            {
                return Task.CompletedTask;
            }

            return _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var ev = turnContext.Activity.AsEventActivity();
            var value = ev.Value?.ToString();

            switch (ev.Name)
            {
                case "Proactive":
                    {
                        var eventData = (turnContext.Activity.Value.ToString());

                        var proactiveModel = await _proactiveStateAccessor.GetAsync(turnContext, () => new ProactiveModel());
                        // TODO: Implement a proactive subscription manager for mapping Notification to ConversationReference
                        var conversationReference = new ConversationReference
                        {
                            User = new ChannelAccount {Id = "29:1IWq2aXvmoP7ClihX7mnyqYIsmFm_zQI9WelfQwYxV7UC2_RNVjruRj8EaffaFZzsHPS_JPgHMn1slbPyx-", Name = "Siddharth More", AadObjectId = "ec7032a8-f569-458c-a261-9dc82da03c67",
                            Role = "user"},
                            Bot = new ChannelAccount {Id = "28:9468be5c-2d38-4c6c-aa83-45f616757685", Name = "SymptomSkill-672iyuq" },
                            ActivityId = "1590783144332",
                            ServiceUrl = "https://smba.trafficmanager.net/amer/",
                            ChannelId = Microsoft.Bot.Connector.Channels.Msteams,
                            Conversation = new ConversationAccount {ConversationType = "channel", Id = "19:e6c2dac86ab44862a7742b9ccab7bd5a@thread.skype;messageid=1590783144332", Name = "Test" , TenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47" },
                        };

                        //    proactiveModel["29:1IWq2aXvmoP7ClihX7mnyqYIsmFm_zQI9WelfQwYxV7UC2_RNVjruRj8EaffaFZzsHPS_JPgHMn1slbPyx-DJRQ"].Conversation;
                        await turnContext.Adapter.ContinueConversationAsync("9468be5c-2d38-4c6c-aa83-45f616757685", conversationReference, ContinueConversationCallback(turnContext, eventData), cancellationToken);

                        break;
                    }

                default:
                    {
                        await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
                        break;
                    }
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //var reply = MessageFactory.Text("OnTeamsTaskModuleFetchAsync TaskModuleRequest: " + JsonConvert.SerializeObject(taskModuleRequest));
            //await turnContext.SendActivityAsync(reply);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(AdaptiveCardHelper.GetJson("Dialogs/AdaptiveCardJson/PrimarySymptomChecker.json")),
            };

            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Card = adaptiveCardAttachment,
                        Height = 500,
                        Width = 400,
                        Title = "Adaptive Card: Inputs",
                    },
                },
            };
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + JsonConvert.SerializeObject(taskModuleRequest));
            //await turnContext.SendActivityAsync(reply);

            var activityValueObject = JObject.FromObject(turnContext.Activity.Value);

            var isDataObject = activityValueObject.TryGetValue("data", StringComparison.InvariantCultureIgnoreCase, out JToken dataValue);
            JObject dataObject = null;
            if (isDataObject)
            {
                dataObject = dataValue as JObject;

                // Get Title
                var shortnessOfBreath = dataObject.GetValue("ShortnessOfBreathInput");

                // Get Description
                var coughInput = dataObject.GetValue("CoughInput");

                var feverInput = dataObject.GetValue("FeverInput"); 
                var card = AdaptiveCardHelper.BuildFeverCard(shortnessOfBreath.Value<string>(),
                    coughInput.Value<string>(), feverInput.Value<string>());
                var reply2 = MessageFactory.Text("Your Reported Symptoms are:");

                reply2.Attachments = new List<Attachment>()
                {
                    new Microsoft.Bot.Schema.Attachment() { ContentType = AdaptiveCard.ContentType, Content = card }
                };

                await turnContext.SendActivityAsync(reply2);
            }

            return new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse()
                {
                    Value = "Thanks!",
                },
            };
        }

        protected override Task OnEndOfConversationActivityAsync(ITurnContext<IEndOfConversationActivity> turnContext, CancellationToken cancellationToken)
        {
            return _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);
        }

        private BotCallbackHandler ContinueConversationCallback(ITurnContext context, string notification)
        {
            return async (turnContext, cancellationToken) =>
            {
                var activity = context.Activity.CreateReply();
                activity.Text = notification;

                await turnContext.SendActivityAsync(activity);
            };
        }
    }
}
