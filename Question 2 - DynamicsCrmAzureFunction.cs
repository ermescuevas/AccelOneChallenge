/*

Explanation:
The function is triggered via an HTTP POST request, which is invoked by the webhook in Dynamics CRM
The function reads the request body, extracts the needed properties like entityName and recordId in order to check if the data is correct or not, 
and logs the information for tracing and debug

For send the email uses the library of System.Net.Mail
The SMTP information for send the email is stored in the Environment Variables in the azure function application settings
The error handling return the status code and the logs the error

*/

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class DynamicsCrmAzureFunction
{
    [FunctionName("DynamicsCrmAzureFunction")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation("Dynamics CRM trigger add entity started.");

        // Parse the request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        // Extract entity details from the request body
        string entityName = data?.PrimaryEntityName;
        string recordId = data?.PrimaryEntityId;
        string subject = data?.Subject ?? "New CRM Record Created";

        if (string.IsNullOrEmpty(entityName) || string.IsNullOrEmpty(recordId))
        {
            return new BadRequestObjectResult("Invalid request data.");
        }

        log.LogInformation($"Entity: {entityName}, Record ID: {recordId}");

        // Send an email notification
        try
        {
            SendEmailNotification(subject, entityName, recordId);
            log.LogInformation("Email sent successfully.");
            return new OkObjectResult("Webhook processed successfully.");
        }
        catch (Exception ex)
        {
            log.LogError($"Error sending email: {ex.Message}");
            return new StatusCodeResult(500);
        }
    }

    private static void SendEmailNotification(string subject, string entityName, string recordId)
    {
        // Load SMTP configuration from environment variables
        string smtpHost = Environment.GetEnvironmentVariable("SmtpHost");
        string smtpPort = Environment.GetEnvironmentVariable("SmtpPort");
        string smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
        string smtpPassword = Environment.GetEnvironmentVariable("SmtpPassword");
        string fromEmail = Environment.GetEnvironmentVariable("FromEmail");
        string toEmail = Environment.GetEnvironmentVariable("ToEmail");

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
        {
            throw new InvalidOperationException("SMTP environment variables are not configured properly.");
        }

        int port = Convert.ToInt32(smtpPort);
        var smtpClient = new SmtpClient(smtpHost)
        {
            Port = port,
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = $"New Record in {entityName}",
            Body = $"A new record with ID {recordId} has been created in the {entityName} entity.",
            IsBodyHtml = false,
        };
        mailMessage.To.Add(toEmail);

        smtpClient.Send(mailMessage);
    }
}
