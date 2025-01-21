using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class RestApiFetcher
{
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Fetches and processes data from a REST API with retry logic and error handling.
    /// </summary>
    /// <typeparam name="T">The type of data to deserialize from the API response.</typeparam>
    /// <param name="url">The URL of the REST API endpoint.</param>
    /// <param name="maxRetries">The maximum number of retries in case of failure.</param>
    /// <param name="delayBetweenRetries">The delay between retries, in milliseconds.</param>
    /// <returns>The deserialized data of type T, or default(T) if the request fails.</returns>
    public async Task<T> FetchDataWithRetriesAsync<T>(string url, int maxRetries = 3, int delayBetweenRetries = 1000)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                // Attempt to fetch data from the API
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                
                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Deserialize the response to the specified type
                T data = await response.Content.ReadFromJsonAsync<T>();

                // Return the processed data
                return data;
            }
            catch (HttpRequestException ex)
            {
                // Log the error (placeholder for actual logging)
                Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");

                // Increment the attempt counter
                attempt++;

                if (attempt < maxRetries)
                {
                    // Wait before retrying
                    await Task.Delay(delayBetweenRetries);
                }
                else
                {
                    // Max retries reached, rethrow or handle gracefully
                    Console.WriteLine("Max retries reached. Returning default value.");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                break; // Exit the loop for non-retriable errors
            }
        }

        // Return default value in case of failure
        return default;
    }
}
