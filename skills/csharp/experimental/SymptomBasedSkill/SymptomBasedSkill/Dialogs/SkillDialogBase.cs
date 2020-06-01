// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Authentication;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Util;
using Microsoft.Extensions.DependencyInjection;
using SymptomBasedSkill.Models;
using SymptomBasedSkill.Services;
using SymptomBasedSkill.Utilities;

namespace SymptomBasedSkill.Dialogs
{
    public class SkillDialogBase : ComponentDialog
    {
        public SkillDialogBase(
             string dialogId,
             IServiceProvider serviceProvider)
             : base(dialogId)
        {
            Settings = serviceProvider.GetService<BotSettings>();
            Services = serviceProvider.GetService<BotServices>();
            TemplateEngine = serviceProvider.GetService<LocaleTemplateManager>();

            // Initialize skill state
            var conversationState = serviceProvider.GetService<ConversationState>();
            StateAccessor = conversationState.CreateProperty<SkillState>(nameof(SkillState));

            // NOTE: Uncomment the following if your skill requires authentication
            if (!Settings.OAuthConnections.Any())
            {
                throw new Exception("You must configure an authentication connection before using this component.");
            }

            //AppCredentials oauthCredentials = null;
            //if (Settings.OAuthCredentials != null &&
            //    !string.IsNullOrWhiteSpace(Settings.OAuthCredentials.MicrosoftAppId) &&
            //    !string.IsNullOrWhiteSpace(Settings.OAuthCredentials.MicrosoftAppPassword))
            //{
            //    oauthCredentials = new MicrosoftAppCredentials(Settings.OAuthCredentials.MicrosoftAppId, Settings.OAuthCredentials.MicrosoftAppPassword);
            //}

            //var baseAuth = new WaterfallStep[]
            //{
            //    GetAuthTokenAsync,
            //    AfterGetAuthTokenAsync
            ////};
            //var setTitle = new WaterfallStep[]
            //{
            //                CheckFeverAsync,
            //                CheckCoughAsync
            //};
            //AddDialog(new TextPrompt("CheckCoughMessage"));
            //AddDialog(new TextPrompt("CheckFeverMessage"));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            //AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


            //AddDialog(new MultiProviderAuthDialog(Settings.OAuthConnections, null, oauthCredentials));
            //AddDialog(new WaterfallDialog("BaseAuth", baseAuth));
            //base.InitialDialogId = "BaseAuth";
        }

        protected BotSettings Settings { get; }

        protected BotServices Services { get; }

        protected IStatePropertyAccessor<SkillState> StateAccessor { get; }

        protected LocaleTemplateManager TemplateEngine { get; }

        //protected async Task<DialogTurnResult> BeginInitialDialogAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    return await sc.ReplaceDialogAsync(InitialDialogId, sc.Options, cancellationToken: cancellationToken);
        //}

        protected async Task<DialogTurnResult> BeginDialogAsync(DialogContext sc, CancellationToken cancellationToken)
        {
            return await base.OnBeginDialogAsync(sc, cancellationToken);
        }
        protected async Task<DialogTurnResult> GetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            try
            {
                return await sc.PromptAsync(nameof(MultiProviderAuthDialog), new PromptOptions(), cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        protected async Task<DialogTurnResult> AfterGetAuthTokenAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default)
        {
            try
            {
                // When the user authenticates interactively we pass on the tokens/Response event which surfaces as a JObject
                // When the token is cached we get a TokenResponse object.
                if (sc.Result is ProviderTokenResponse providerTokenResponse)
                {
                    var state = await StateAccessor.GetAsync(sc.Context, cancellationToken: cancellationToken);
                    state.Token = providerTokenResponse.TokenResponse.Token;
                }

                return await sc.NextAsync(cancellationToken: cancellationToken);
            }
            catch (SkillException ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
            catch (Exception ex)
            {
                await HandleDialogExceptionsAsync(sc, ex, cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Cancelled, CommonUtil.DialogTurnResultCancelAllDialogs);
            }
        }

        // Validators
        protected Task<bool> TokenResponseValidatorAsync(PromptValidatorContext<Activity> pc, CancellationToken cancellationToken)
        {
            var activity = pc.Recognized.Value;
            if (activity != null && activity.Type == ActivityTypes.Event)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        protected Task<bool> AuthPromptValidatorAsync(PromptValidatorContext<TokenResponse> promptContext, CancellationToken cancellationToken)
        {
            var token = promptContext.Recognized.Value;
            if (token != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        // This method is called by any waterfall step that throws an exception to ensure consistency
        protected async Task HandleDialogExceptionsAsync(WaterfallStepContext sc, Exception ex, CancellationToken cancellationToken)
        {
            // send trace back to emulator
            var trace = new Activity(type: ActivityTypes.Trace, text: $"DialogException: {ex.Message}, StackTrace: {ex.StackTrace}");
            await sc.Context.SendActivityAsync(trace, cancellationToken);

            // log exception
            TelemetryClient.TrackException(ex, new Dictionary<string, string> { { nameof(sc.ActiveDialog), sc.ActiveDialog?.Id } });

            // send error message to bot user
            await sc.Context.SendActivityAsync(TemplateEngine.GenerateActivityForLocale("ErrorMessage"), cancellationToken);

            // clear state
            var state = await StateAccessor.GetAsync(sc.Context, cancellationToken: cancellationToken);
            state.Clear();
        }

        protected async Task<ActionResult> CreateActionResultAsync(ITurnContext context, bool success, CancellationToken cancellationToken)
        {
            var state = await StateAccessor.GetAsync(context, () => new SkillState(), cancellationToken);
            if (success && state.IsAction)
            {
                return new ActionResult(success);
            }
            else
            {
                return null;
            }
        }

        protected async Task<DialogTurnResult> CheckFeverAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);

            if (!string.IsNullOrEmpty(state.Fever))
            {
                return await sc.NextAsync(state.Fever, cancellationToken);
            }

            return await sc.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = TemplateEngine.GenerateActivity("ConfirmFever"),
            }, cancellationToken);
        }

        protected async Task<DialogTurnResult> CheckCoughAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);

            if (!string.IsNullOrEmpty(state.Cough))
            {
                return await sc.NextAsync(state.Cough, cancellationToken);
            }

            return await sc.PromptAsync(nameof(TextPrompt), new PromptOptions()
            {
                Prompt = TemplateEngine.GenerateActivity("ConfirmCough"),
            }, cancellationToken);
        }
    }
}