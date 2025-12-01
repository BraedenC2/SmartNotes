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

        public string FrontLanguage { get; set; } = "en";
        public string BackLanguage { get; set; } = "es";

        public string FrontText { get; set; } = string.Empty;
        public string BackText { get; set; } = string.Empty;

        public string EnglishWord { get; set; } = string.Empty;
        public string SpanishWord { get; set; } = string.Empty;

        public string SpanishExampleSentence { get; set; } = string.Empty;
        public string EnglishExampleSentence { get; set; } = string.Empty;
        public string UsageNotes { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }
    }
}