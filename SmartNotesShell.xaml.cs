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

        // Added "Type" to distinguish normal cards from the Review button
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
            // We add a special card at the top for the Study Session
            var cards = new List<StudyCard>
        {
            // The "Review" Action Card
            new StudyCard("Review Session", "All Cards", "Click here to study all your saved cards randomly.", "ReviewAction"),
            
            // Placeholders
            new StudyCard("Sample Card 1", "Algorithms", "Define Big-O notation in your own words."),
            new StudyCard("Sample Card 2", "Korean", "안녕하세요 - Polite hello, used in most situations.")
        };

            ActiveCardsItemsControl.ItemsSource = cards;
        }

        // ---------------------------------------------------------
        // IMPORTANT: You need to bind the Click/MouseUp event 
        // of the buttons in your ActiveCardsItemsControl XAML 
        // to this method for this to work!
        // ---------------------------------------------------------
        private void Card_Click(object sender, RoutedEventArgs e)
        {
            // Check what was clicked. 
            // Assuming the sender is the Button or element holding the DataContext
            if (sender is FrameworkElement element && element.DataContext is StudyCard card)
            {
                if (card.Type == "ReviewAction")
                {
                    // Open the new Study Window
                    var studyWin = new StudyWindow();
                    studyWin.Owner = this;
                    studyWin.ShowDialog();
                }
                else
                {
                    // Just a placeholder action for normal sample cards
                    // Or maybe open "View All Cards" focused on this one later
                    MessageBox.Show($"You clicked on {card.Title}. Functionality coming soon!", "SmartNotes");
                }
            }
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

            okButton.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
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

            var item = new ListBoxItem { Content = name };
            StudySetsListBox.Items.Add(item);
            StudySetsListBox.SelectedItem = item;
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
            MessageBox.Show($"Later, delete '{selected}' from Firebase here.",
                            "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show("Image upload + Vertex AI note creation will plug in here.",
                            "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
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
