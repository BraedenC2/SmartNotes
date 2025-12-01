using Google.GenAI;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartNotes
{
    internal static class GeminiService
    {
        // DONT COMMIT API KEY REMINDER!!!!!!!!!!!
        private const string ApiKey = "";

        private static readonly Client _client = new Client(apiKey: ApiKey);
        private const string ModelName = "gemini-2.0-flash";

        public sealed class SpanishCardResponse
        {
            [JsonPropertyName("english_word")]
            public string EnglishWord { get; set; } = string.Empty;

            [JsonPropertyName("spanish_translation")]
            public string SpanishTranslation { get; set; } = string.Empty;

            [JsonPropertyName("spanish_example_sentence")]
            public string SpanishExampleSentence { get; set; } = string.Empty;

            [JsonPropertyName("english_example_sentence")]
            public string EnglishExampleSentence { get; set; } = string.Empty;

            [JsonPropertyName("usage_notes")]
            public string UsageNotes { get; set; } = string.Empty;
        }

        public static async Task<SpanishCardResponse?> GenerateSpanishCardAsync(string englishWord)
        {
            if (string.IsNullOrWhiteSpace(englishWord))
                return null;

            var trimmed = englishWord.Trim();

            var prompt = $@"
                You are a helpful Spanish tutor.

                For the single English word ""{trimmed}"", create information for a language flashcard.

                Return ONLY a single JSON object with this exact shape, no extra text, no comments, no code fences:

                {{
                    ""english_word"": ""the original English word"",
                    ""spanish_translation"": ""the main Spanish translation"",
                    ""spanish_example_sentence"": ""a natural Spanish example sentence using the Spanish word"",
                    ""english_example_sentence"": ""an English translation of that example sentence"",
                    ""usage_notes"": ""a short explanation of how this word is used (grammar, register, common patterns)""
                }}";

            try
            {
                var response = await _client.Models.GenerateContentAsync(
                    model: ModelName,
                    contents: prompt);

                var text = response.Candidates?[0]?.Content?.Parts?[0]?.Text;

                if (string.IsNullOrWhiteSpace(text))
                    return null;

                text = text.Trim();

                if (text.StartsWith("```"))
                {
                    var firstNewLine = text.IndexOf('\n');
                    var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
                    if (firstNewLine >= 0 && lastFence > firstNewLine)
                    {
                        text = text.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
                    }
                }

                var card = JsonSerializer.Deserialize<SpanishCardResponse>(
                    text,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return card;
            }
            catch
            {
                return null;
            }
        }


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
                return text ?? "(Gemini returned no text)";
            }
            catch (Exception ex)
            {
                return $"[Error calling Gemini: {ex.Message}]";
            }
        }
    }
}
