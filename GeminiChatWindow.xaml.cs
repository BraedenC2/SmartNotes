using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;


namespace SmartNotes
{
    public partial class GeminiChatWindow : Window
    {
        public GeminiChatWindow()
        {
            InitializeComponent();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var englishWord = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(englishWord))
            {
                MessageBox.Show(
                    "Please enter a single word",
                    "SmartNotes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (englishWord.Contains(" "))
            {
                MessageBox.Show(
                    "Please enter just a single word (no spaces).",
                    "SmartNotes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            SendButton.IsEnabled = false;

            try
            {
                OutputTextBox.Text = "Creating your card...";

                var cardData = await GeminiService.GenerateSpanishCardAsync(englishWord);

                if (cardData == null)
                {
                    OutputTextBox.Text = "Error!";
                    return;
                }

                var card = new LanguageFlashcard
                {
                    Id = Guid.NewGuid(),
                    FrontLanguage = "en",
                    BackLanguage = "es",
                    FrontText = cardData.EnglishWord,
                    BackText =
                        $"{cardData.SpanishTranslation}\n\n" +
                        $"Example (ES): {cardData.SpanishExampleSentence}\n" +
                        $"Example (EN): {cardData.EnglishExampleSentence}\n\n" +
                        $"Usage: {cardData.UsageNotes}",
                    EnglishWord = cardData.EnglishWord,
                    SpanishWord = cardData.SpanishTranslation,
                    SpanishExampleSentence = cardData.SpanishExampleSentence,
                    EnglishExampleSentence = cardData.EnglishExampleSentence,
                    UsageNotes = cardData.UsageNotes,
                    CreatedUtc = DateTime.UtcNow
                };

                LocalCardStorage.SaveCard(card);

                OutputTextBox.Text =
                    "Card saved!\n\n" +
                    $"Front: {card.FrontText}\n\n" +
                    "Back:\n" +
                    card.BackText;
            }
            finally { SendButton.IsEnabled = true; }
        }
    }
}
