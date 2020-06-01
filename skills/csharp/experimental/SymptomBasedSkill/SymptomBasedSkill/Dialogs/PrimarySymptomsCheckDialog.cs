using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SymptomBasedSkill.Dialogs.AdaptiveCardJson;
using SymptomBasedSkill.Models;
using SymptomBasedSkill.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SymptomBasedSkill.Dialogs
{
    public class PrimarySymptomsCheckDialog : SkillDialogBase
    {
        public PrimarySymptomsCheckDialog(
           IServiceProvider serviceProvider)
           : base(nameof(PrimarySymptomsCheckDialog), serviceProvider)
        {
            var checkSymptoms = new WaterfallStep[]
            {
                GetPrimarySymptomsAsync
                //GetAuthTokenAsync,
                //AfterGetAuthTokenAsync
            };

            AddDialog(new WaterfallDialog("GetPrimarySymptopms", checkSymptoms));
        }

        protected async Task<DialogTurnResult> GetPrimarySymptomsAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);
            var reply = MessageFactory.Attachment(this.GetTaskModuleHeroCard());
            await sc.Context.SendActivityAsync(reply);
            return await sc.EndDialogAsync(await CreateActionResultAsync(sc.Context, true, cancellationToken), cancellationToken);
            //var adaptiveCard = AdaptiveCardHelper.GetCardFromJson("Dialogs/AdaptiveCardJson/PrimarySymptomChecker.json");
            //adaptiveCard.Id = "GetUserInput";

            //////var cardAttachment = new Attachment()
            //////{
            //////    ContentType = "application/vnd.microsoft.card.adaptive",
            //////    Content = JsonConvert.DeserializeObject(adaptiveCard),
            //////};
            //var reply2 = sc.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment>()
            //{
            //    new Microsoft.Bot.Schema.Attachment() { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard }
            //};

            ////var opts = new PromptOptions
            ////{
            ////    Prompt = new Activity
            ////    {
            ////        Type = ActivityTypes.Message,
            ////        Text = "waiting for user input...", // You can comment this out if you don't want to display any text. Still works.
            ////    }
            ////};

            ////return await sc.PromptAsync(nameof(TextPrompt), opts, cancellationToken);
            ////adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
            ////{
            ////    Title = "Submit",
            ////    Data = new AdaptiveCardValue<TaskModuleMetadata>()
            ////    {
            ////        Data = new TaskModuleMetadata()
            ////        {
            ////            TaskModuleFlowType = TeamsFlowType.CreateTicket_Form.ToString(),
            ////            Submit = true
            ////        }
            ////    }
            ////});            
            //adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
            //{
            //    Title = "Submit",
            //    //Data = new AdaptiveCardValue<TaskModuleMetadata>()
            //    //{
            //    //    Data = new TaskModuleMetadata()
            //    //    {
            //    //        TaskModuleFlowType = TeamsFlowType.CreateTicket_Form.ToString(),
            //    //        Submit = true
            //    //    }
            //    //}
            //});

            //var reply = sc.Context.Activity.CreateReply();
            //reply.Attachments = new List<Attachment>()
            //{
            //    new Microsoft.Bot.Schema.Attachment() { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard }
            //};

            //ResourceResponse resourceResponse = await sc.Context.SendActivityAsync(reply, cancellationToken);
            //return await sc.NextAsync(cancellationToken: cancellationToken);
        }

        protected async Task<DialogTurnResult> ProcessPrimarySymptomsAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);
            var activityValueObject = JObject.FromObject(sc.Context.Activity.Value);
            return await sc.EndDialogAsync(await CreateActionResultAsync(sc.Context, true, cancellationToken), cancellationToken);
        }

        private Attachment GetTaskModuleHeroCard()
        {
            return new HeroCard()
            {
                Title = "Primary Symptom Check Card",
                Subtitle = "Please fill out the invoked form",
                Buttons = new List<CardAction>()
                {
                    new TaskModuleAction("CheckSymptoms", new { data = "adaptivecard" }),
                },
            }.ToAttachment();
        }
    }
}
