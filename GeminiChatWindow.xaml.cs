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
    public partial class GeminiChatWindow : Window
    {
        public GeminiChatWindow()
        {
            InitializeComponent();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var userText = UserInputTextBox.Text;

            if (string.IsNullOrWhiteSpace(userText))
            {
                MessageBox.Show(
                    "Type something before sending.",
                    "SmartNotes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            SendButton.IsEnabled = false;

            try
            {
                OutputTextBox.Text = "Thinking...";
                var reply = await GeminiService.SendAsync(userText);
                OutputTextBox.Text = reply;
            }
            finally
            {
                SendButton.IsEnabled = true;
            }
        }
    }
}
