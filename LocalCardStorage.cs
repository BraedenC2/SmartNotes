using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartNotes
{
    internal static class LocalCardStorage
    {
        private static readonly string CardsFolderPath;

        static LocalCardStorage()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SmartNotes",
                "Cards");

            CardsFolderPath = root;
            Directory.CreateDirectory(CardsFolderPath);
        }

        public static void SaveCard(LanguageFlashcard card)
        {
            if (card.Id == Guid.Empty)
            {
                card.Id = Guid.NewGuid();
            }

            var path = Path.Combine(CardsFolderPath, $"{card.Id}.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(card, options);
            File.WriteAllText(path, json);
        }

        // Will be used and ready for ViewAllCards feature and the actual studying feature. 
        public static IEnumerable<LanguageFlashcard> LoadAllCards()
        {
            var list = new List<LanguageFlashcard>();

            if (!Directory.Exists(CardsFolderPath)) return list;

            foreach (var file in Directory.EnumerateFiles(CardsFolderPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var card = JsonSerializer.Deserialize<LanguageFlashcard>(json);
                    if (card != null){list.Add(card);}
                }
                catch
                {}
            }

            return list;
        }
    }
}
