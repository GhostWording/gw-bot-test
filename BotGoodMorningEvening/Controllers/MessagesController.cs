using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using BotGoodMorningEvening.Dialogs;
using BotGoodMorningEvening.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotGoodMorningEvening.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        ///     POST: api/Messages
        ///     Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var userId = AddOrUpdateUser(activity.From.Id, activity.From.Name, activity.Recipient.Id,
                    activity.Recipient.Name, activity.ServiceUrl, activity.LocalTimestamp?.Offset.Hours,
                    activity.ChannelId);
                await Conversation.SendAsync(activity, () => new RootDialog(userId));
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private static Guid AddOrUpdateUser(string userId, string userName, string botId, string botName,
            string serviceUrl, int? gmtPlus, string channelId)
        {
            using (var db = new UserContext())
            {
                var result = db.UserResgistereds.FirstOrDefault(x => x.UserId == userId && x.UserName == userName);

                // Update
                if (result != null)
                {
                    result.BotId = botId;
                    result.BotName = botName;
                    result.ServiceURL = serviceUrl;
                    result.Gmtplus = gmtPlus ?? 0;
                    result.ChannelId = channelId;
                }
                else
                {
                    // Or add
                    result = new UserResgistered
                    {
                        ID = Guid.NewGuid(),
                        UserId = userId,
                        UserName = userName,
                        BotId = botId,
                        BotName = botName,
                        ServiceURL = serviceUrl,
                        Gmtplus = gmtPlus,
                        ChannelId = channelId
                    };
                    db.UserResgistereds.Add(result);
                }
                db.SaveChanges();

                return result.ID;
            }
        }

        private static Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}