using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SagaNetwork.Models;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;
using System.Linq;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Models;

namespace SagaNetwork.Controllers
{
    public class AccessKeysEditorController : BaseEditorController<AccessKey>
    {
        [HttpPost]
        public async Task<IActionResult> GetFreeKeys (int count)
        {
            if (count <= 0 || count > 100)
                return ErrorView("You may only add 1 to 100 access keys at a time.");

            var existingKeys = await new AccessKey().RetrieveAllAsync();
            var accessKeys = new List<AccessKey>(count);

            for (int i = 0; i < count; i++)
            {
                var existingFreeKey = existingKeys.Find(k => !k.IsActivated && string.IsNullOrEmpty(k.AssociatedEmail) && !accessKeys.Exists(ek => ek.Id == k.Id));
                if (existingFreeKey != null)
                {
                    accessKeys.Add(existingFreeKey);
                }
                else
                {
                    var newKey = await new AccessKey().AddAsync(existingKeys);
                    existingKeys.Add(newKey);
                    accessKeys.Add(newKey);
                }
            }

            return View("AccessKeyList", accessKeys);
        }

        [HttpPost]
        public async Task<IActionResult> SendKeysViaEmail (string listId)
        {
            var existingKeys = await new AccessKey().RetrieveAllAsync();

            var mailChimp = new MailChimpManager("3f4978dbd632a4471650eeeebe681ce3-us12");
            var mandrill = new MandrillApi("DQCr_ZMk5Ga5slGv9uWGPQ");

            List mailChimpList;
            try { mailChimpList = await mailChimp.Lists.GetAsync(listId); }
            catch (MailChimpException) { return ErrorView($"MailChimp list with ID {listId} doesn't exist."); }

            var resultsList = new List<string> { $"Scanned MailChimp list {mailChimpList.Name} and sent an access key for each member:" };

            var listMembers = await mailChimp.Members.GetAllAsync(listId);
            foreach (var listMember in listMembers)
            {
                var accessKey = await new AccessKey().AddAsync(existingKeys, listMember.EmailAddress);
                existingKeys.Add(accessKey);

                var emailMessage = new EmailMessage() { To = new List<EmailAddress> { new EmailAddress(listMember.EmailAddress) } };
                var accessKeyContent = new TemplateContent() { Name = "acces-key", Content = accessKey.Id };
                var sendRequest = new SendMessageTemplateRequest(emailMessage, "Access Key", new List<TemplateContent> { accessKeyContent });

                var sendResults = await mandrill.SendMessageTemplate(sendRequest);
                var result = sendResults.First();
                resultsList.Add($" - Sent key {accessKey.Id} to {result.Email} with status {result.Status}. " 
                    + (result.Status != EmailResultStatus.Sent ? $"Reject reason: {result.RejectReason}" : string.Empty));
            }

            return View("SendAccessKeysResult", resultsList);
        }
    }
}
