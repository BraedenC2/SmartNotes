using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;


namespace SmartNotes
{
    public partial class SmartNotesShell : Window
    {
        private readonly FirebaseAuthService.AuthResult _auth;

        private sealed record StudyCard(string Title, string Subtitle, string Preview, string Type = "Normal");

        public SmartNotesShell(FirebaseAuthService.AuthResult auth)
        {
            InitializeComponent();
            _auth = auth;

            InitHeader();
            InitSampleCards();
        }

        private void InitHeader()
        {
            if (!string.IsNullOrEmpty(_auth.Email))
            {
                var name = _auth.Email.Split('@')[0];
                WelcomeText.Text = $"Signed in as {name}";
            }
            else
            {
                WelcomeText.Text = "Signed in";
            }
        }

        private void InitSampleCards()
        {
            var cards = new List<StudyCard>
        {
            new StudyCard("Review Session", "All Cards", "Click here to study all your saved cards randomly.", "ReviewAction"),
            
            //new StudyCard("Sample Card 1", "Algorithms", "Define Big-O notation in your own words."),
            //new StudyCard("Sample Card 2", "Korean", "안녕하세요 - Polite hello, used in most situations.")
        };

            ActiveCardsItemsControl.ItemsSource = cards;
        }


        private void Card_Click(object sender, RoutedEventArgs e)
        {

            if (sender is FrameworkElement element && element.DataContext is StudyCard card)
            {
                if (card.Type == "ReviewAction")
                {
                    var studyWin = new StudyWindow();
                    studyWin.Owner = this;
                    studyWin.ShowDialog();
                }
                else
                {
                }
            }
        }


        // !===! UI handlers !===!

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewStudySetButton_Click(object sender, RoutedEventArgs e)
        {
        }


        private void DeleteStudySetButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NewCardButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new GeminiChatWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }


        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ViewAllCardsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AllCardsWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }
    }
}
