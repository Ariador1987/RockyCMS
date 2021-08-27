using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailjetSettings _mailJetSettings;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailjetSettings>();

            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
             new JObject {
              {
               "From",
               new JObject {
                {"Email", "bozidar.bralic@gmail.com"},
                {"Name", "Bozidar"}
               }
              }, {
               "To",
               new JArray {
                new JObject {
                 {
                  "Email",
                  email
                 }, {
                  "Name",
                  "Bozidar"
                 }
                }
               }
              }, {
               "Subject",
               subject
              }, {
               "TextPart",
               "My first Mailjet email"
              }, {
               "HTMLPart",
               body
              }, {
               "CustomID",
               "AppGettingStartedTest"
              }
             }
                     });
            await client.PostAsync(request);
        }
    }
}
