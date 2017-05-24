using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotGoodMorningEvening.Helpers;
using BotGoodMorningEvening.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace BotGoodMorningEvening.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private Dictionary<string, Tuple<int, int, List<Card>>> cardListReference; // (intentionId, (hashcode, index, cards))

        private readonly string startIntentionId;

        public RootDialog(string userId, string intentionId = null)
        {
            startIntentionId = intentionId;
            UserId = userId;
        }

        public string UserId { get; }

        public async Task StartAsync(IDialogContext context)
        {
            if (startIntentionId == null)
                context.Wait(MessageReceivedAsync);
            else
                await GetAndSendCard(context, startIntentionId);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            try
            {
                // Retrieve conversation info
                var message = await argument;

                //var resumptionCookie = JsonConvert.SerializeObject(new ConversationReference(context.Activity.Id));
                var resumptionCookie = new ResumptionCookie(message).GZipSerialize();
                var currentuser = UserStorageManager.GetUser(this.UserId);
                if (currentuser != null)
                    currentuser.ResumptionCookie = resumptionCookie;

                UserStorageManager.UpdateUser(currentuser);

                var text = message.Text ?? "Good Morning";
                // Find a card
                await FindAndSendCard(context, text);
            }
            catch (Exception)
            {
                await context.PostAsync("MessageReceived error");
            }
        }

        private async Task State1(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            try
            {
                var message = await argument;

                if (message.ChannelId.Equals("facebook", StringComparison.InvariantCultureIgnoreCase))
                {
                    var quickReplyResponse = message.ChannelData.message.quick_reply;
                    if (quickReplyResponse != null)
                    {
                        var payload = quickReplyResponse.payload.ToString() as string;
                        if (payload != null)
                        {
                            if (payload.StartsWith("SHOW_ANOTHER_"))
                            {
                                await GetAndSendCard(context, payload.Substring(13));
                            }
                            else if (payload == "CHANGE_TOPIC")
                            {
                                await GetAndSendTopics(context);
                            }
                            else
                            {
                                await context.PostAsync("Something is going wrong...");
                                context.Wait(MessageReceivedAsync);
                            }
                        }
                        else
                        {
                            await context.PostAsync("Something is going wrong...");
                            context.Wait(MessageReceivedAsync);
                        }
                    }
                    else
                    {
                        await FindAndSendCard(context, message.Text);
                    }
                }
                else
                {
                    if (message.Text.ToLower().Contains("another"))
                        await GetAndSendCard(context, CardHelper.GoodMorningIntentionId);
                    else
                        await FindAndSendCard(context, message.Text);
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Sorry, but I have an issue during state 1 action");
            }
        }

        private async Task State2(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            try
            {
                var message = await argument;

                if (message.ChannelId.Equals("facebook", StringComparison.InvariantCultureIgnoreCase))
                {
                    var quickReplyResponse = message.ChannelData.message.quick_reply;
                    if (quickReplyResponse != null)
                    {
                        var payload = quickReplyResponse.payload.ToString() as string;
                        if (payload != null)
                        {
                            if (payload.StartsWith("CHANGE_TOPIC_"))
                            {
                                await GetAndSendCard(context, payload.Substring(13));
                            }
                            else
                            {
                                await context.PostAsync("Something is going wrong...");
                                context.Wait(MessageReceivedAsync);
                            }
                        }
                        else
                        {
                            await context.PostAsync("Something is going wrong...");
                            context.Wait(MessageReceivedAsync);
                        }
                    }
                    else
                    {
                        await context.PostAsync(
                            "It seems you didn't click on a quick reply and you just typed a response.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                else
                {
                    await context.PostAsync("Sorry, but I talk only with messenger");
                    context.Wait(MessageReceivedAsync);
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Sorry, but I have an issue during state 2 action");
            }
        }

        private async Task FindAndSendCard(IDialogContext context, string text)
        {
            // Find the card
            var card = CardHelper.FindCard(text, new List<string>());

            // Send the card
            await SendCard(context, card);

            // Wait for response
            context.Wait(State1);
        }

        private async Task GetAndSendTopics(IDialogContext context)
        {
            // Get intentions
            var intentions = CardHelper.GetIntentions();

            // Send topics
            await SendTopics(context, intentions);

            // Wait for a response
            context.Wait(State2);
        }

        private async Task GetAndSendCard(IDialogContext context, string intentionId)
        {
            Card newCard;

            // Get the cache
            var user = UserStorageManager.GetUser(this.UserId);

            cardListReference = !string.IsNullOrEmpty(user?.CardsCache) ? JsonConvert.DeserializeObject<Dictionary<string, Tuple<int, int, List<Card>>>>(user.CardsCache) : new Dictionary<string, Tuple<int, int, List<Card>>>();

            // Get another card
            if (cardListReference.ContainsKey(intentionId))
            {
                var entry = cardListReference[intentionId];
                var newIndex = entry.Item2 + 1;
                if (newIndex < entry.Item3.Count)
                {
                    newCard = entry.Item3[newIndex];
                    cardListReference[intentionId] =
                        new Tuple<int, int, List<Card>>(entry.Item1, newIndex, entry.Item3);
                }
                else
                {
                    var (hashcode, cards) = CardHelper.GetIntentionCards(intentionId, entry.Item1);
                    cardListReference[intentionId] = new Tuple<int, int, List<Card>>(hashcode, 0, cards);
                    newCard = cards[0];
                }
            }
            else
            {
                var (hashcode, cards) = CardHelper.GetIntentionCards(intentionId, 0);
                cardListReference.Add(intentionId, new Tuple<int, int, List<Card>>(hashcode, 0, cards));
                newCard = cards[0];
            }

            // Send the card
            await SendCard(context, newCard);

            // Save the cache
            if (user != null)
            {
                user.CardsCache = JsonConvert.SerializeObject(cardListReference);
                UserStorageManager.UpdateUser(user);
            }

            // Wait for a response
            context.Wait(State1);
        }

        private static async Task SendTopics(IDialogContext context, List<Intention> intentions)
        {
            var reply = context.MakeMessage();
            if (reply.ChannelId.Equals("facebook", StringComparison.InvariantCultureIgnoreCase))
            {
                reply.Text = "Which topic inspires you?";
                var channelData = new FacebookChannelData
                {
                    QuickReplies = intentions
                        .Select(i => new FacebookTextQuickReply(i.Label, "CHANGE_TOPIC_" + i.IntentionId)).ToArray()
                };
                reply.ChannelData = channelData;
                await context.PostAsync(reply);
            }
        }

        private static async Task SendCard(IDialogContext context, Card card)
        {
            try
            {
                // Title
                var title = CardHelper.GetTitle(card.IntentionId);
                await context.PostAsync(title);

                // Image
                var imageMessage = context.MakeMessage();
                imageMessage.Attachments.Add(new Attachment { ContentType = "image/jpg", ContentUrl = card.ImageLink });
                await context.PostAsync(imageMessage);

                // Buttons
                var reply = context.MakeMessage();

                if (reply.ChannelId == null)
                    reply.ChannelId = "facebook";

                if (reply.ChannelId.Equals("facebook", StringComparison.InvariantCultureIgnoreCase))
                {
                    reply.Text = card.Content.Replace("\n", "<br/>");
                    var channelData = new FacebookChannelData
                    {
                        QuickReplies = new[]
                        {
                            new FacebookTextQuickReply("Show another", $"SHOW_ANOTHER_{card.IntentionId}"),
                            new FacebookTextQuickReply("Change topic", "CHANGE_TOPIC")
                        }
                    };
                    reply.ChannelData = channelData;
                    await context.PostAsync(reply);
                }
            }
            catch (Exception)
            {
                await context.PostAsync("Sorry, my send card action have an issue");
            }
        }
    }
}