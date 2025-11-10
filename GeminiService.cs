using Google.GenAI;
using System;
using System.Threading.Tasks;

namespace SmartNotes
{
    internal static class GeminiService
    {
        // DONT COMMIT API KEY REMINDER!!!!!!!!!!!
        private const string ApiKey = "";

        private static readonly Client _client = new Client(apiKey: ApiKey);

        // Might change the model in the future depending on pricing. 
        private const string ModelName = "gemini-2.0-flash";

        public static async Task<string> SendAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return string.Empty;

            try
            {
                var response = await _client.Models.GenerateContentAsync(
                    model: ModelName,
                    contents: userMessage);

                var text = response.Candidates?[0]?.Content?.Parts?[0]?.Text;
                return text ?? "(Gemini returned no text.)";
            }
            catch (Exception ex)
            {
                return $"[Error calling Gemini: {ex.Message}]";
            }
        }
    }
}
