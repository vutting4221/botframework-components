// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace SymptomBasedSkill.Models
{
    public class SkillState
    {
        public string Token { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public bool IsAction { get; set; }

        public string Temperature { get; set; }

        public string Fever { get; set; }

        public string Cough { get; set; }

        public string ShortnessOfBreath { get; set; }

        public bool Chills { get; set; }

        public bool MusclePain { get; set; }

        public bool SoreThroat { get; set; }

        public bool LossOfSmell { get; set; }

        public bool LossOfTaste { get; set; }

        public bool Vomitting { get; set; }

        public bool Diarrhea { get; set; }

        public bool NoSymptoms { get; set; }

        public bool Symptomatic { get; set; }

        public bool SymptomFree { get; set; }

        public void Clear()
        {
        }
    }
}
