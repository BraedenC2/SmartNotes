using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNotes
{
    internal class LanguageFlashcard
    {
        public Guid Id { get; set; }

        // For future multi-language stuff. I want to allow the user to pick any two languages, but for now it's just English and Spanish since its simple.
        public string FrontLanguage { get; set; } = "en";
        public string BackLanguage { get; set; } = "es";

        // What the user actually sees on the card.
        public string FrontText { get; set; } = string.Empty;
        public string BackText { get; set; } = string.Empty;

        // Structured fields:
        public string EnglishWord { get; set; } = string.Empty;
        public string SpanishWord { get; set; } = string.Empty;

        public string SpanishExampleSentence { get; set; } = string.Empty;
        public string EnglishExampleSentence { get; set; } = string.Empty;
        public string UsageNotes { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }
    }
}