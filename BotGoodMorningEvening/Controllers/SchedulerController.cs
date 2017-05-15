using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using BotGoodMorningEvening.Dialogs;
using BotGoodMorningEvening.Helpers;
using BotGoodMorningEvening.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace BotGoodMorningEvening.Controllers
{
    public class SchedulerController : ApiController
    {
        [HttpPost]
        public async Task WakeUpConversation()
        {
            using (var db = new UserContext())
            {
                // Send good morning
                var morningUsers = from u in db.UserResgistereds
                                   where DateTime.UtcNow.Hour + u.Gmtplus >= 8 && DateTime.UtcNow.Hour + u.Gmtplus < 12
                                   select u;

                var intentionId = CardHelper.GoodMorningIntentionId;
                await SendGood(morningUsers.ToList(), intentionId);

                // Send good evening
                var eveningUsers = from u in db.UserResgistereds
                                   where DateTime.UtcNow.Hour + u.Gmtplus >= 14 && DateTime.UtcNow.Hour + u.Gmtplus < 22
                                   select u;

                intentionId = CardHelper.GoodEveningIntentionId;
                await SendGood(eveningUsers.ToList(), intentionId);
            }
        }

        private static async Task SendGood(List<UserResgistered> results, string intentionId)
        {
            foreach (var item in results)
            {
                if (!string.IsNullOrEmpty(item.ResumptionCookie))
                {
                    try
                    {

                        var message = ResumptionCookie.GZipDeserialize(item.ResumptionCookie).GetMessage();

                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                        {
                            var botData = scope.Resolve<IBotData>();
                            await botData.LoadAsync(CancellationToken.None);

                            //This is our dialog stack
                            var task = scope.Resolve<IDialogTask>();

                            //interrupt the stack. This means that we're stopping whatever conversation that is currently happening with the user
                            //Then adding this stack to run and once it's finished, we will be back to the original conversation
                            Guid userId;
                            Guid.TryParse(item.UserId, out userId);
                            var dialog = new RootDialog(userId, intentionId);
                            task.Call(dialog.Void<object, IMessageActivity>(), null);

                            await task.PollAsync(CancellationToken.None);

                            //flush dialog stack
                            await botData.FlushAsync(CancellationToken.None);
                        }
                    }
                    catch (Exception)
                    {
                        // Mute errors
                    }

                }
            }
        }
    }
}