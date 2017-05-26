using System;
using BotGoodMorningEvening.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace BotGoodMorningEvening.Helpers
{
    public class CardHelper
    {
        private static List<Intention> intentions;
        private static Dictionary<string, Tuple<int, int, List<Card>>> intentionCardsCache; // (intentionId (hashcode, version, cards))

        public static string GoodMorningIntentionId { get; } = "030FD0";

        public static string GoodEveningIntentionId { get; } = "D392C1";

        public static string GetTitle(string intentionId)
        {
            return intentionId == null ? null : GetIntentions()?.FirstOrDefault(i => i.IntentionId == intentionId)?.Label ?? "Good morning";
        }

        public static Card FindCard(string text, List<string> lastTextIds)
        {
            var card = ProxyHelper.SendRequest<List<Card>>("bot/TestBot", "talktobot", HttpMethod.Post, new Command { text = text, top = 1 });
            return card.LastOrDefault();
        }

        private static Card GetNewCard(List<string> lastTextIds, string intentionId)
        {
            var card = ProxyHelper.SendRequest<List<Card>>("popular/TestBot", "IdeasOfTheDay", HttpMethod.Get, "forRecipient", "BotFriends", "andIntention", intentionId, new KeyValuePair<string, string>("nbcards", "1"));
            return card.LastOrDefault();
        }

        private static List<Card> GetCards(int version, string intentionId)
        {
            var parameters = new List<object> { "forRecipient", "BotFriends", "andIntention", intentionId, new KeyValuePair<string, string>("nbcards", "6")};
            if (version != 0) parameters.Add(new KeyValuePair<string, string>("version", version.ToString()));
            var cards = ProxyHelper.SendRequest<List<Card>>("popular/TestBot", "IdeasOfTheDay", HttpMethod.Get, parameters.ToArray());
            return cards;
        }

        public static List<Intention> GetIntentions()
        {
            if (intentions == null || intentions.Count == 0)
                intentions = ProxyHelper.SendRequest<List<Intention>>("HuggyMessages", "Intentions", HttpMethod.Get).ToList();
            return intentions;
        }

        public static (int, List<Card>) GetIntentionCards(string intentionId, int actualHash)
        {
            var version = 0;

            // Check if the cache is empty
            if (intentionCardsCache == null) intentionCardsCache = new Dictionary<string, Tuple<int, int, List<Card>>>();

            
            // Check if a card list is available (intention exists and it's another hash)
            if (intentionCardsCache.ContainsKey(intentionId))
            {     
                var entry = intentionCardsCache.FirstOrDefault(c => c.Key == intentionId);

                // If the cards exists and the caller haven't yet used it we return it
                if (entry.Value.Item1 != actualHash)
                {
                    return (entry.Value.Item1, entry.Value.Item3);
                }
                // If the cards exists but the caller already have it, we change the version
                version = intentionCardsCache[intentionId].Item2 + 1;
                version = version % 10;
            }

            // And get a new card list
            var cards = GetCards(version, intentionId);

            // And add to the cache
            var hashCode = string.Join("", cards.Select(c => c.IntentionId)).GetHashCode();
            intentionCardsCache.Remove(intentionId);
            intentionCardsCache.Add(intentionId, new Tuple<int, int, List<Card>>(hashCode, version, cards));

            // And return to the user
            return (hashCode, cards);            
        }
    }
}