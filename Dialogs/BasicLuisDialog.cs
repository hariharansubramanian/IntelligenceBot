using LuisBot.Data;
using LuisBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        [LuisIntent("workflow")]
        public async Task InitiateIncidentWorkflow(IDialogContext context, LuisResult result)
        {
            await this.ManageWorkflowInitiation(context, result);
        }


        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            string resp = BingWebSearch(result.Query);
            await this.ShowBingResult(context, resp);
        }



        private async Task ShowBingResult(IDialogContext context, string respnse)
        {
            await context.PostAsync(respnse);
            context.Wait(MessageReceived);
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

        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowGreetingResult(context, result.Query);
        }

        private async Task ShowGreetingResult(IDialogContext context, string query)
        {
            await context.PostAsync($"Hello, I am Ivantilligence. How can I assist you today?");
            context.Wait(MessageReceived);
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
            else
            {
                if (FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault() == null)
                {
                    await context.PostAsync($"Unable to find this incident. ");
                    context.Wait(MessageReceived);
                }
                else
                {
                    string currentOwner = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault().owner;

                    FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).ToList().ForEach(s => s.owner = owner.Entity);
                    string newOwner = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault().owner;

                    string str = String.Format("Updated Incident owner from {0} to {1}. ", currentOwner, newOwner);
                    await context.PostAsync(str);
                    context.Wait(MessageReceived);
                }

            }




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
            string str = String.Format("Incident {0} has been deleted successfully! ", id.Entity);
            await context.PostAsync(str);
            context.Wait(MessageReceived);
        }
        private async Task ManageWorkflowInitiation(IDialogContext context, LuisResult result)
        {
            EntityRecommendation id;
            if (!result.TryFindEntity(ENTITY_INTCIDENT_ID, out id))
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }
            Incident incidentFromDb = FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault();
            if (incidentFromDb == null)
            {
                await context.PostAsync($"Unable to find this incident. ");
                context.Wait(MessageReceived);
            }
            else
            {
                FakeDb.incidents.Where(i => i.id.Equals(id.Entity)).FirstOrDefault().status = "Initiated";

                string str = String.Format("Workflow for Incident {0} is now being executed. Updated status to {1} ", id.Entity, incidentFromDb.status);
                //TODO: initiate an email
                sendEmail(incidentFromDb);
                await context.PostAsync(str);
                context.Wait(MessageReceived);
            }

        }



        private void sendEmail(Incident incidentFromDb)
        {
            string mailBody = String.Format("<html>\r\n\r\n<head></head>\r\n<body>Hi Hariharan, <br /> Your incident is now being processed. <br />\r\n <br /> Incident Details: <br /> Id: {0} <br /> Description: {1} <br /> Status: {2}  </body>\r\n</html>", incidentFromDb.id, incidentFromDb.summary, incidentFromDb.status);
            string mailSubject = String.Format("Incident {0} has been initiated", incidentFromDb.id);
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("tbd.hackathon@gmail.com");
            mail.To.Add("hariharan.subramanian@ivanti.com");
            mail.Subject = mailSubject;
            mail.Body = mailBody;
            mail.IsBodyHtml = true;

            SmtpServer.Port = 25;
            SmtpServer.Credentials = new System.Net.NetworkCredential("tbd.hackathon@gmail.com", "tbdtbdtbd");
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }

        private static string generateRandomId(Incident incident)
        {
            Random r = new Random();
            var x = r.Next(0, 1000000);
            return x.ToString("000000");
        }
        static string BingWebSearch(string searchQuery)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string accessKey = "526d877ff5d4401aafe42d6f8696e642";
            string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/search";

            // Construct the URI of the search request
            var uriQuery = uriBase + "?q=" + Uri.EscapeDataString(searchQuery) + "&answerCount=2&responseFilter=webpages";

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = accessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();


            // Console.WriteLine(json);

            BingResponse obj = JsonConvert.DeserializeObject<BingResponse>(json);

            return obj.webPages.value[0].snippet;
        }
    }
}