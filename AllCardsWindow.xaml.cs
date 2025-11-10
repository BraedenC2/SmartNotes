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
    public partial class AllCardsWindow : Window
    {
        private List<LanguageFlashcard> _cards = new();

        public AllCardsWindow()
        {
            InitializeComponent();
            LoadCards();
        }

        private void LoadCards()
        {
            var cards = LocalCardStorage.LoadAllCards();

            _cards = cards
                .OrderBy(c => c.FrontText, StringComparer.OrdinalIgnoreCase)
                .ToList();

            CardsListBox.ItemsSource = _cards;
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            if (button.DataContext is not LanguageFlashcard card) return;

            var isShowingFront = (string?)button.Tag != "Back";

            if (button.Content is not TextBlock tb) return;

            if (isShowingFront)
            {
                tb.Text = card.BackText;
                button.Tag = "Back";
            }
            else
            {
                tb.Text = card.FrontText;
                button.Tag = "Front";
            }
        }



        // Old
        //private void CardTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (sender is not TextBlock textBlock)
        //        return;

        //    if (textBlock.DataContext is not LanguageFlashcard card)
        //        return;

        //    var isShowingFront = (string?)textBlock.Tag != "Back";

        //    if (isShowingFront)
        //    {
        //        textBlock.Text = card.BackText;
        //        textBlock.Tag = "Back";
        //    }
        //    else
        //    {
        //        textBlock.Text = card.FrontText;
        //        textBlock.Tag = "Front";
        //    }
    }
}


