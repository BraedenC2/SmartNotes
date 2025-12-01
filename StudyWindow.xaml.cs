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

namespace SmartNotes
{
    public partial class StudyWindow : Window
    {
        private List<LanguageFlashcard> _cards = new();
        private int _currentIndex = 0;
        private bool _isShowingFront = true;
        public StudyWindow()
        {
            InitializeComponent();
            LoadAndShuffleCards();
        }

        private void LoadAndShuffleCards()
        {
            var allCards = LocalCardStorage.LoadAllCards().ToList();

            if (allCards.Count == 0)
            {
                MessageBox.Show("You have no saved cards to study!", "Empty Deck", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            var rng = new Random();
            _cards = allCards.OrderBy(a => rng.Next()).ToList();

            _currentIndex = 0;
            ShowCurrentCard();
        }

        private void ShowCurrentCard()
        {
            if (_currentIndex >= _cards.Count)
            {
                MessageBox.Show("You have reviewed all cards!", "Session Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            var card = _cards[_currentIndex];


            if (NextBtn.Template.FindName("CardContentText", NextBtn) is TextBlock)
            {

            }

            UpdateCardTextDisplay(card);
            ProgressText.Text = $"Card {_currentIndex + 1} of {_cards.Count}";
        }

        private void UpdateCardTextDisplay(LanguageFlashcard card)
        {
            var btn = this.Content as Grid;
            var textBlock = FindChild<TextBlock>(this, "CardContentText");

            if (textBlock != null)
            {
                textBlock.Text = _isShowingFront ? card.FrontText : card.BackText;
            }
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            _isShowingFront = !_isShowingFront;
            ShowCurrentCard();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentIndex++;
            _isShowingFront = true;
            ShowCurrentCard();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && (child as FrameworkElement)?.Name == childName)
                {
                    return typedChild;
                }

                var result = FindChild<T>(child, childName);
                if (result != null) return result;
            }
            return null;
        }
    }
}

