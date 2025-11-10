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

        private sealed record StudyCard(string Title, string Subtitle, string Preview);

        public SmartNotesShell(FirebaseAuthService.AuthResult auth)
        {
            InitializeComponent();
            _auth = auth;

            InitHeader();
            InitSampleCards();
            // TODO: Load study sets + cards from Firebase for this user (_auth.LocalId / _auth.Email)
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
            // Just placeholders right now!
            ActiveCardsItemsControl.ItemsSource = new[]
            {
                new StudyCard("Title", "Term/topic", "Defintion/example"),
                new StudyCard("Sample Card 1", "Algorithms", "Define Big-O notation in your own words."),
                new StudyCard("Sample Card 2", "Korean", "안녕하세요 - Polite hello, used in most situations.")
            };
        }

        private string? PromptForStudySetName()
        {
            var dialog = new Window
            {
                Title = "New Study Set",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                Background = (Brush)FindResource("BgCream"),
                ShowInTaskbar = false
            };

            var root = new Grid { Margin = new Thickness(16) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var title = new TextBlock
            {
                Text = "What would you like to call this study set?",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("Ink")
            };
            Grid.SetRow(title, 0);
            root.Children.Add(title);

            var textBox = new TextBox
            {
                Margin = new Thickness(0, 8, 0, 0),
                MinWidth = 260
            };
            Grid.SetRow(textBox, 1);
            root.Children.Add(textBox);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Margin = new Thickness(0, 0, 8, 0),
                IsCancel = true
            };

            var okButton = new Button
            {
                Content = "Create",
                Width = 80,
                IsDefault = true
            };

            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(okButton);
            Grid.SetRow(buttonsPanel, 2);
            root.Children.Add(buttonsPanel);

            // Might change to sender, e, but this lambda is new and cool to me:
            okButton.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // [redacted] for now
                    //MessageBox.Show(dialog,
                    //    "",
                    //    "SmartNotes",
                    //    MessageBoxButton.OK,
                    //    MessageBoxImage.Information);
                    return;
                }

                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.Content = root;

            var result = dialog.ShowDialog();
            if (result == true)
            {
                return textBox.Text.Trim();
            }

            return null;
        }


        // !===! UI handlers !===!

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Quick toggle. I might add an storyboard animation for this later! Small stretch goal I will say. 
            // rn it just simply expands/collapses instantly for the popup side, for the sets. 
            if (SideMenuColumn.Width.Value == 0)
            {
                SideMenuColumn.Width = new GridLength(260);
            }
            else
            {
                SideMenuColumn.Width = new GridLength(0);
            }
        }

        private void NewStudySetButton_Click(object sender, RoutedEventArgs e)
        {
            var name = PromptForStudySetName();
            if (string.IsNullOrWhiteSpace(name)) return;

            // Right now, it just adds it to the list visually, not functionally. 
            var item = new ListBoxItem { Content = name };
            StudySetsListBox.Items.Add(item);
            StudySetsListBox.SelectedItem = item;

            // TODO: save this new study set to the user's Firebase database
            //       and load them from Firebase on startup instead of hard-coded items.
        }


        private void DeleteStudySetButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudySetsListBox.SelectedItem is null)
            {
                MessageBox.Show("Select a study set to delete.",
                                "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selected = StudySetsListBox.SelectedItem;
            // TODO: confirm + delete from Firebase
            MessageBox.Show($"Later, delete '{selected}' from Firebase here.",
                            "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewCardButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, New Card opens the Gemini helper window.
            var window = new GeminiChatWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }


        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 1) Open file dialog
            //       2) Upload image to storage (e.g., Firebase Storage)
            //       3) Call Vertex AI to generate notes/cards
            MessageBox.Show("Image upload + Vertex AI note creation will plug in here.",
                            "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewAllCardsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: navigate to a full “all cards” view for the selected study set.
            MessageBox.Show("Full list of cards for the active study set will go here.",
                            "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // TODO: More handlers for card interactions (edit, delete, study mode, etc.) can go here.
        // ALSO: Actually implement studying lol. 
    }
}
