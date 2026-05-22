using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BettingApp.API.Models
{
    public class OddsApiEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("sport_title")]
        public string SportTitle { get; set; } = string.Empty;

        [JsonPropertyName("commence_time")]
        public DateTime CommenceTime { get; set; }

        [JsonPropertyName("home_team")]
        public string HomeTeam { get; set; } = string.Empty;

        [JsonPropertyName("away_team")]
        public string AwayTeam { get; set; } = string.Empty;

        [JsonPropertyName("bookmakers")]
        public List<OddsApiBookmaker> Bookmakers { get; set; } = new();
    }

    public class OddsApiBookmaker
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("markets")]
        public List<OddsApiMarket> Markets { get; set; } = new();
    }

    public class OddsApiMarket
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("outcomes")]
        public List<OddsApiOutcome> Outcomes { get; set; } = new();
    }

    public class OddsApiOutcome
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public double Price { get; set; }
    }

    public class ScoresApiEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }

        [JsonPropertyName("scores")]
        public List<ScoresApiScore>? Scores { get; set; }
    }

    public class ScoresApiScore
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public string Score { get; set; } = string.Empty;
    }
}
