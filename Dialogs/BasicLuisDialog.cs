using LuisBot.Data;
using LuisBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        private const string ENTITY_INTCIDENT_ID = "incidentid";
        private const string ENTITY_INCIDENT_OWNER = "incidentowner";

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {

        }

        [LuisIntent("create")]
        public async Task CreateIntent(IDialogContext context, LuisResult result)
        {
            await this.manageCreateIncident(context, result);
        }

        [LuisIntent("retrieve")]
        public async Task RetrieveIncident(IDialogContext context, LuisResult result)
        {
            try
            {
                await this.manageRetreiveIncident(context, result);
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                await context.PostAsync($"Unable to find this incident :( ");
                context.Wait(MessageReceived);

            }

        }

        [LuisIntent("update")]
        public async Task UpdateIncident(IDialogContext context, LuisResult result)
        {
            await this.ManageUpdateIncident(context, result);
        }

        [LuisIntent("delete")]
        public async Task DeleteIncident(IDialogContext context, LuisResult result)
        {
            await this.ManageDeleteIncident(context, result);
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
            FakeDb.incidents.Add(incident);
            await context.PostAsync(str);
            context.Wait(MessageReceived);
        }

        private async Task manageRetreiveIncident(IDialogContext context, LuisResult result)
        {
            EntityRecommendation id;
            if (!result.TryFindEntity(ENTITY_INTCIDENT_ID, out id))
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }
            else
            {
                var retreivedIncident = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault();
                if (retreivedIncident == null)
                {

                    await context.PostAsync($"Unable to find this incident. ");
                    context.Wait(MessageReceived);
                }
                else
                {
                    string str = String.Format("Incident Retreived\nIncident owner: \"{0}\"\nIncident summary: {1}\nIncident status {2}\nIncident ID: {3}",
              retreivedIncident.owner,
             retreivedIncident.summary,
             retreivedIncident.status,
             retreivedIncident.id);
                    await context.PostAsync(str);
                    context.Wait(MessageReceived);
                }

            }

        }

        private async Task ManageUpdateIncident(IDialogContext context, LuisResult result)
        {
            EntityRecommendation id;
            if (!result.TryFindEntity(ENTITY_INTCIDENT_ID, out id))
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }

            EntityRecommendation owner;
            if (!result.TryFindEntity(ENTITY_INCIDENT_OWNER, out owner))
            {
                await context.PostAsync($"Please provide an authorized owner to update this incident with.");
                context.Wait(MessageReceived);
            }

            if (FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault() == null)
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }
            string currentOwner = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault().owner;

            FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).ToList().ForEach(s => s.owner = owner.Entity);
            string newOwner = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault().owner;

            string str = String.Format("Updated Incident owner from {0} to {1}. ", currentOwner, newOwner);
            await context.PostAsync(str);
            context.Wait(MessageReceived);

        }

        private async Task ManageDeleteIncident(IDialogContext context, LuisResult result)
        {
            EntityRecommendation id;
            if (!result.TryFindEntity(ENTITY_INTCIDENT_ID, out id))
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }

            FakeDb.incidents.RemoveWhere(x => x.id.Equals(id.Entity));
            // FakeDb.incidents.ToList().RemoveAll((x) => x.id.Equals(id.Entity));
            string str = String.Format("Incident {0} has been deleted successfully! ", id.Entity);
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