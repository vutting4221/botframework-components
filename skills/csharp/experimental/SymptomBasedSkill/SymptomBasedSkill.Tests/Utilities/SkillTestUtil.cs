// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Luis;
using Microsoft.Bot.Builder;
using SymptomBasedSkill.Tests.Mocks;
using SymptomBasedSkill.Tests.Utterances;

namespace SymptomBasedSkill.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, SymptomBasedSkillLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, SymptomBasedSkillLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static SymptomBasedSkillLuis CreateIntent(string userInput, SymptomBasedSkillLuis.Intent intent)
        {
            var result = new SymptomBasedSkillLuis
            {
                Text = userInput,
                Intents = new Dictionary<SymptomBasedSkillLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new SymptomBasedSkillLuis._Entities
            {
                _instance = new SymptomBasedSkillLuis._Entities._Instance()
            };

            return result;
        }
    }
}
