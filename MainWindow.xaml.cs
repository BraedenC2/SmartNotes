using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SmartNotes
{
    public partial class MainWindow : Window
    {
        // TODO: REMOVE THIS KEY BEFORE PUSHING TO GITHUB DEPLOYMENT!!!
        private const string FirebaseWebApiKey = "";

        // Where we store the refresh token if "Remember me" is checked
        private readonly string TokenFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartNotes", "firebase_session.json");
        //C:\Users\USER\AppData\Local\SmartNotes\firebase_session.json
        // For future reference, delete this file to sign out the auto-signed-in user.

        public MainWindow()
        {
            InitializeComponent();
            TryAutoSignIn();
        }

        // Makes the 'Password' text disappear and reappear depending if the user has typed anything
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
            => PwdWatermark.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Collapsed;

        // Handles "Register here" / "Forgot Password" links 
        private async void Link_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;
            switch (tb.Tag as string)
            {
                // Switches to registering 
                case "toggle":
                    ModeToggle.IsChecked = !ModeToggle.IsChecked;
                    StatusText.Text = "";
                    break;
                // Sends password reset email
                case "forgot":
                    await SendPasswordReset();
                    break;
            }
        }

        // Login OR Register (depending on ModeToggle)
        // It is called LoginButton, but it handles both login and registration
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            DisableUi(true);
            try
            {
                var email = EmailBox.Text?.Trim();
                var pass = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
                {
                    StatusText.Text = "Please enter email and password.";
                    return;
                }

                FirebaseAuthService.AuthResult result;
                if (ModeToggle.IsChecked == true)
                {
                    // Register
                    result = await FirebaseAuthService.SignUpWithEmailPasswordAsync(email, pass, FirebaseWebApiKey);
                    StatusText.Text = "Account created. You have been signed in.";
                }
                else
                {
                    // Login
                    result = await FirebaseAuthService.SignInWithEmailPasswordAsync(email, pass, FirebaseWebApiKey);
                    StatusText.Text = "Welcome back! :)";
                }

                await HandleSuccessfulSignIn(result);
            }
            catch (FirebaseAuthService.AuthException ex)
            {
                StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                DisableUi(false);
            }
        }

        // This exists because I was going to have a guest mode separate from login/register button for testing purposes,
        // anonymous signing in is STILL possible and is active on my API key and project, however,
        // I decided to not implement this feature since it could complicate things and confuse me in the future.
        // This code can be deleted since the UI element currently isn't there to activate it, however,
        // I wanted to keep it to show that anonymous sign-in is possible with Firebase Auth REST API in my app (technically possible).
        private async void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            DisableUi(true);
            try
            {
                var result = await FirebaseAuthService.SignInAnonymouslyAsync(FirebaseWebApiKey);
                StatusText.Text = "Continuing as guest.";
                await HandleSuccessfulSignIn(result);
            }
            catch (FirebaseAuthService.AuthException ex)
            {
                StatusText.Text = ex.Message;
            }
            finally
            {
                DisableUi(false);
            }
        }

        // WORKS DO NOT TOUCH!!!
        // For professor:
        // You may have to check your spam/junk folder to see the password reset email if you do try this feature. 
        private async Task SendPasswordReset()
        {
            var email = EmailBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                StatusText.Text = "Enter your email first.";
                return;
            }

            DisableUi(true);
            try
            {
                await FirebaseAuthService.SendPasswordResetEmailAsync(email, FirebaseWebApiKey);
                StatusText.Text = "Password reset email sent (check your inbox/spam).";
            }
            catch (FirebaseAuthService.AuthException ex)
            {
                StatusText.Text = ex.Message;
            }
            finally
            {
                DisableUi(false);
            }
        }

        // TODO: Fix email not returned on refresh token exchange
        private async void TryAutoSignIn()
        {
            try
            {
                // Check for saved refresh token
                if (!File.Exists(TokenFile)) return;

                // Read all of the saved token information, deserialize, and return if the saved data is invalid
                var json = await File.ReadAllTextAsync(TokenFile);
                var saved = JsonSerializer.Deserialize<SavedSession>(json);
                if (saved?.RefreshToken is null) return;

                // if valid, it will disable the ui to make a network call
                DisableUi(true);
                var refreshed = await FirebaseAuthService.ExchangeRefreshTokenAsync(saved.RefreshToken, FirebaseWebApiKey);
                await HandleSuccessfulSignIn(refreshed, silent: true);
                //TODO: Email is not returned!!!!!!!!!!!!!
                StatusText.Text = $"Welcome back{(string.IsNullOrEmpty(refreshed.Email) ? "" : $", {refreshed.Email}")}.";
            }
            catch
            {
                // ignore; user will sign in manually
            }
            finally
            {
                DisableUi(false);
            }
        }

        private async Task HandleSuccessfulSignIn(FirebaseAuthService.AuthResult result, bool silent = false)
        {

            
            if (RememberCheck.IsChecked == true)
            {
                // Makes SURE the directory exists before creating a file in a folder that may not exist yet
                Directory.CreateDirectory(Path.GetDirectoryName(TokenFile)!);
                await File.WriteAllTextAsync(TokenFile,
                    JsonSerializer.Serialize(new SavedSession { RefreshToken = result.RefreshToken }));
            }

            // TODO: Proceed to the main app window
            if (!silent)
                MessageBox.Show("Signed in successfully.", "SmartNotes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisableUi(bool busy)
        {
            // Does not allow the user to interact with the UI while an auth operation is in progress
            EmailBox.IsEnabled = !busy;
            PasswordBox.IsEnabled = !busy;
            LoginButton.IsEnabled = !busy;
        }

        // Helps deserialize
        private record SavedSession { public string? RefreshToken { get; set; } }
    }

    // ____________________ UI HELP ____________________
    // Right now this doesnt do anything, but I would like to keep it and return to it for future improvements!
    public class EmptyToVisibilityConverter : IValueConverter
    {
        // TODO MAYBE: Change the culture info to be more inclusive to other people in the future as a stretch goal. Right now, it's just blank/null only for the us
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // This changes the tittle label (not the logo, the logo is an png image). 
    public sealed class BoolToTitleConverter : IValueConverter
    {
        // false = sign in | true = create account
        public static readonly BoolToTitleConverter Instance = new();
        public object Convert(object v, Type t, object p, CultureInfo c)
            => (v as bool? == true) ? "Create an Account" : "Sign In";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }
    // This changes the button text that allows you to login originally, but then can switch to create account when toggled.
    public sealed class BoolToCtaConverter : IValueConverter
    {
        public static readonly BoolToCtaConverter Instance = new();
        public object Convert(object v, Type t, object p, CultureInfo c)
            => (v as bool? == true) ? "CREATE ACCOUNT" : "LOGIN";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    // ____________________ Firebase REST STUFF ___________________
    // (no third-party SDKs needed btw)

    // Most of this code was adapted, since I never used Firebase before:
    internal static class FirebaseAuthService
    {
        private static readonly HttpClient http = new();

        public sealed class AuthException : Exception
        {
            public AuthException(string message) : base(message) { }
        }

        public sealed record AuthResult(
            string IdToken,
            string RefreshToken,
            string LocalId,
            string Email,
            int ExpiresInSeconds);

        private static readonly JsonSerializerOptions jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static async Task<AuthResult> SignUpWithEmailPasswordAsync(string email, string password, string apiKey)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
            var payload = new { email, password, returnSecureToken = true };
            return await SendAuthRequest(url, payload);
        }

        public static async Task<AuthResult> SignInWithEmailPasswordAsync(string email, string password, string apiKey)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
            var payload = new { email, password, returnSecureToken = true };
            return await SendAuthRequest(url, payload);
        }

        // As you can see, we can sign in anonymously too! But the UI does not have a way to trigger this anymore like I said previously. 
        public static async Task<AuthResult> SignInAnonymouslyAsync(string apiKey)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";
            var payload = new { returnSecureToken = true };
            return await SendAuthRequest(url, payload);
        }

        public static async Task SendPasswordResetEmailAsync(string email, string apiKey)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";
            var payload = new { requestType = "PASSWORD_RESET", email };
            using var content = new StringContent(JsonSerializer.Serialize(payload, jsonOpts), Encoding.UTF8, "application/json");
            var res = await http.PostAsync(url, content);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new AuthException(ParseFirebaseError(msg));
            }
        }

        public static async Task<AuthResult> ExchangeRefreshTokenAsync(string refreshToken, string apiKey)
        {
            var url = $"https://securetoken.googleapis.com/v1/token?key={apiKey}";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","refresh_token"),
                new KeyValuePair<string,string>("refresh_token", refreshToken)
            });
            var res = await http.PostAsync(url, form);
            var body = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) throw new AuthException(ParseFirebaseError(body));

            using var doc = JsonDocument.Parse(body);
            var rt = doc.RootElement.GetProperty("refresh_token").GetString()!;
            var id = doc.RootElement.GetProperty("id_token").GetString()!;
            var uid = doc.RootElement.GetProperty("user_id").GetString()!;
            var expires = int.Parse(doc.RootElement.GetProperty("expires_in").GetString()!);

            return new AuthResult(id, rt, uid, Email: "", ExpiresInSeconds: expires);
        }

        private static async Task<AuthResult> SendAuthRequest(string url, object payload)
        {
            using var content = new StringContent(JsonSerializer.Serialize(payload, jsonOpts), Encoding.UTF8, "application/json");
            var res = await http.PostAsync(url, content);
            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode) throw new AuthException(ParseFirebaseError(body));

            using var doc = JsonDocument.Parse(body);
            var idToken = doc.RootElement.GetProperty("idToken").GetString()!;
            var refreshToken = doc.RootElement.GetProperty("refreshToken").GetString()!;
            var localId = doc.RootElement.GetProperty("localId").GetString()!;
            var email = doc.RootElement.TryGetProperty("email", out var e) ? e.GetString() ?? "" : "";
            var expiresIn = int.Parse(doc.RootElement.GetProperty("expiresIn").GetString()!);

            return new AuthResult(idToken, refreshToken, localId, email, expiresIn);
        }

        private static string ParseFirebaseError(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var msg = doc.RootElement.GetProperty("error").GetProperty("message").GetString();
                return msg switch
                {
                    "EMAIL_EXISTS" => "Email is already registered.",
                    "OPERATION_NOT_ALLOWED" => "This sign-in method is disabled in your project.",
                    "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many attempts. Try again later.",
                    "EMAIL_NOT_FOUND" => "No account found for that email.",
                    "INVALID_PASSWORD" => "Incorrect password.",
                    "USER_DISABLED" => "This account has been disabled.",
                    _ => $"Auth error: {msg}"
                };
            }
            catch
            {
                return $"Auth error: {responseBody}";
            }
        }
    }
}
