// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Luis;
using Microsoft.Bot.Builder;
using SymptomChecker.Tests.Mocks;
using SymptomChecker.Tests.Utterances;

namespace SymptomChecker.Tests.Utilities
{
    public class SkillTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, SymptomCheckerLuis.Intent.Sample) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, SymptomCheckerLuis.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static SymptomCheckerLuis CreateIntent(string userInput, SymptomCheckerLuis.Intent intent)
        {
            var result = new SymptomCheckerLuis
            {
                Text = userInput,
                Intents = new Dictionary<SymptomCheckerLuis.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new SymptomCheckerLuis._Entities
            {
                _instance = new SymptomCheckerLuis._Entities._Instance()
            };

            return result;
        }
    }
}
