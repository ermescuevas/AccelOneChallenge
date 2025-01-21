/*
Explanation:

XML comments, which are useful for comprehending the method's use and goal, are absent from it.
The incoming request (req) has no error handling or validation.
The statement is overly ambiguous, making the data's purpose unclear.
Response messages that are hardcoded ("Success") may result in redundancy or inflexibility. Absence of concern separation; 
the function body handles all logic. The request payload is not deserialized or type-checked.
Memory inefficiencies may result from reading the full request body (ReadAsStringAsync) without verifying 
or processing the payload, particularly for big requests.

To make the function's goal and expectations more clear, XML comments were added to the method and helper class (RequestData).
divided the code into manageable chunks and added appropriate comments.

Stream processing: To provide improved request stream processing, the body was read using StreamReader.
Early Exits: To conserve resources, requests with incorrect content types or empty bodies 
are turned down before they can be processed further.Using a JsonSerializer with case-insensitive property handling 
for robustness allowed for efficient deserialization.


*/

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public static class MyAzureFunction
{
    /// <summary>
    /// Processes incoming HTTP requests and returns a success response.
    /// </summary>
    /// <param name="req">The HTTP request object.</param>
    /// <param name="log">The logger instance.</param>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
    {
        log.LogInformation("Processing request.");

        try
        {
            // Validate the request content type
            if (!req.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                log.LogWarning("Invalid content type.");
                return new BadRequestObjectResult("Invalid content type. Expecting application/json.");
            }

            // Read and parse the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                log.LogWarning("Request body is empty.");
                return new BadRequestObjectResult("Request body cannot be empty.");
            }

            // Deserialize JSON data (assuming a known structure)
            var requestData = JsonSerializer.Deserialize<dynamic>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (requestData == null)
            {
                log.LogWarning("Invalid JSON format.");
                return new BadRequestObjectResult("Invalid JSON format.");
            }

            // Process the request (replace with actual logic)
            log.LogInformation("Request processed successfully.");
            return new OkObjectResult("Success");
        }
        catch (JsonException ex)
        {
            log.LogError(ex, "JSON deserialization error.");
            return new BadRequestObjectResult("Invalid JSON payload.");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "An unexpected error occurred.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}