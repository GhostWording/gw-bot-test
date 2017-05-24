using BotGoodMorningEvening.Dialogs;
using BotGoodMorningEvening.Helpers;
using Facebook;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

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
                var timezone = findTimeZone(activity.From.Id);

                UserStorageManager.AddOrUpdateUser(activity.From.Id, activity.From.Name, activity.Recipient.Id,
                    activity.Recipient.Name, activity.ServiceUrl, timezone,
                    activity.ChannelId);

                await Conversation.SendAsync(activity, () => new RootDialog(activity.From.Id));
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private static int? findTimeZone(string userId)
        {
            int timezone;
            try
            {
                var pageAccessToken = ConfigurationManager.AppSettings["PageAccessToken"];
                var fbclient = new FacebookClient(pageAccessToken);
                dynamic result = fbclient.Get($"https://graph.facebook.com/v2.6/{userId}?fields=first_name,last_name,profile_pic,locale,timezone,gender&access_token={pageAccessToken}");
                var gmt = result.ContainsKey("timezone") ? result["timezone"] : null;

                if (gmt != null && gmt < int.MaxValue)
                    timezone = (int)gmt;
                else
                    return null;

                return timezone;
            }
            catch (Exception e)
            {
                return null;
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