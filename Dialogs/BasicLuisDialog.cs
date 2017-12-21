using LuisBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        HashSet<Incident> incidents;
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            if (incidents == null)
            {
                incidents = new HashSet<Incident>();
            }
        }

        [LuisIntent("create")]
        public async Task CreateIntent(IDialogContext context, LuisResult result)
        {
            await this.manageCreateIncident(context, result);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Gretting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }

        private async Task manageCreateIncident(IDialogContext context, LuisResult result)
        {
            Incident incident = new Incident();
            incident.id = generateRandomId(incident);
            incident.owner = "Hari";
            incident.summary = result.Query;
            incident.status = "New";
            string str = String.Format("Incident \"{0}\" created by {1} successfuly! Incident ID: {2}", incident.summary, incident.owner, incident.id);
            incidents.Add(incident);
            await context.PostAsync(str);
            context.Wait(MessageReceived);
        }

        private static string generateRandomId(Incident incident)
        {
            Random r = new Random();
            var x = r.Next(0, 1000000);
            return x.ToString("000000");
        }
    }
}