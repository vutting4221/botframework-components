using AdaptiveCards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SymptomBasedSkill.Utilities
{
    public class AdaptiveCardHelper
    {
        // TODO: Replace with Cards.Lg
        public static AdaptiveCard GetCardFromJson(string jsonFile)
        {
            string jsonCard = GetJson(jsonFile);

            return JsonConvert.DeserializeObject<AdaptiveCard>(jsonCard);
        }

        public static string GetJson(string jsonFile)
        {
            var dir = Path.GetDirectoryName(typeof(AdaptiveCardHelper).Assembly.Location);
            var filePath = Path.Combine(dir, $"{jsonFile}");
            return File.ReadAllText(filePath);
        }

        public static AdaptiveCard BuildFeverCard(string shortnessOfBreath, string fever, string cough)
        {
            var card = new AdaptiveCard("1.0")
            {
                Id = "FeverResponseCard",
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Stretch,
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = $"ShortNessOfBreat: {shortnessOfBreath}",
                                                Wrap = true,
                                                Color = (shortnessOfBreath == "Yes") ? AdaptiveTextColor.Warning : AdaptiveTextColor.Good,
                                                Spacing = AdaptiveSpacing.Small,
                                                Weight = AdaptiveTextWeight.Bolder
                                            },
                                            new AdaptiveTextBlock
                                            {
                                                Text = $"Cough: {cough}",
                                                Color = (cough == "Yes") ? AdaptiveTextColor.Warning : AdaptiveTextColor.Good,
                                                MaxLines = 1,
                                                Weight = AdaptiveTextWeight.Bolder,
                                                Size = AdaptiveTextSize.Large
                                            },
                                            new AdaptiveTextBlock
                                            {
                                                Text = $"Fever: {fever}",
                                                Wrap = true,
                                                Color = (fever == "Yes") ? AdaptiveTextColor.Warning : AdaptiveTextColor.Good,
                                                Spacing = AdaptiveSpacing.Small,
                                                Weight = AdaptiveTextWeight.Bolder
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return card;
        }
    }
}
