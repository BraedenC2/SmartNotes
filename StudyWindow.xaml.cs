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
    /// <summary>
    /// Interaction logic for StudyWindow.xaml
    /// </summary>
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

            // Randomize the order (Shuffle)
            var rng = new Random();
            _cards = allCards.OrderBy(a => rng.Next()).ToList();

            _currentIndex = 0;
            ShowCurrentCard();
        }

        private void ShowCurrentCard()
        {
            if (_currentIndex >= _cards.Count)
            {
                // End of stack
                MessageBox.Show("You have reviewed all cards!", "Session Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            var card = _cards[_currentIndex];

            // Update content text (Template FindName is tricky in code-behind, but since we used a Button template in XAML, 
            // we need to access the textblock via visual tree or simple binding. 
            // To keep it simple and robust, let's find the TextBlock inside the button template:

            if (NextBtn.Template.FindName("CardContentText", NextBtn) is TextBlock)
            {
                // This is just a fallback, usually we traverse the visual tree, 
                // but to keep code simple, let's assume the simple XAML structure.
            }

            // Direct access helper since 'CardContentText' is inside a ControlTemplate, 
            // we need to be careful. 
            // actually, x:Name inside a template isn't directly accessible in code-behind easily without loading.
            // FIX: Let's bind the TextBlock in XAML or just find it. 
            // SIMPLE FIX: Use the button's Content, but we set a Template.
            // BETTER FIX: Let's manually find the element or use DataBinding.
            // For this snippet, I will traverse the visual tree of the button sender in the click, 
            // BUT to set it initially, we need access.

            // Let's just update the Content of a simplified structure for the user:
            // I will update the XAML logic above to be simpler, but assuming the XAML provided:

            // Let's update the text logic:
            UpdateCardTextDisplay(card);
            ProgressText.Text = $"Card {_currentIndex + 1} of {_cards.Count}";
        }

        private void UpdateCardTextDisplay(LanguageFlashcard card)
        {
            // Find the TextBlock. Since x:Name in a Template isn't directly accessible:
            // We will rely on the visual tree helper or just simpler XAML.
            // To make this code copy-paste friendly without complex helpers, 
            // I'm going to reference the element by walking the tree or just use the Button's Content property if it wasn't templated.

            // Actually, in the XAML above, I used a ControlTemplate. 
            // Let's simplify the XAML access: 
            // We will use the Loaded event of the TextBlock to grab a reference, OR
            // we can just change the logic to not use a complex template for the button.

            // (Simpler Logic for this specific method):
            var btn = this.Content as Grid;
            // ... actually, let's use a simple trick. 
            // The TextBlock "CardContentText" might not be accessible.
            // Let's just traverse:
            var textBlock = FindChild<TextBlock>(this, "CardContentText");

            if (textBlock != null)
            {
                textBlock.Text = _isShowingFront ? card.FrontText : card.BackText;
            }
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            _isShowingFront = !_isShowingFront;
            ShowCurrentCard(); // Refresh text
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentIndex++;
            _isShowingFront = true; // Always start next card on front
            ShowCurrentCard();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Helper to find the TextBlock inside the Template
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

