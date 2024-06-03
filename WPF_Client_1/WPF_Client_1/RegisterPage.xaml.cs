using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_Client_1
{
    public partial class RegisterPage : Window
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                RegisterMessage.Text = "Username and password cannot be empty";
                RegisterMessage.Visibility = Visibility.Visible;
                return;
            }

            var user = new { Username = username, Password = password };
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync("http://localhost:8080/register", content);
                    if (response.IsSuccessStatusCode)
                    {
                        RegisterMessage.Text = "Registration successful. Please log in.";
                        RegisterMessage.Visibility = Visibility.Visible;
                        await Task.Delay(2000);
                        this.Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        RegisterMessage.Text = "Username already exists.";
                        RegisterMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        var responseMessage = await response.Content.ReadAsStringAsync();
                        RegisterMessage.Text = $"Registration failed: {responseMessage}";
                        RegisterMessage.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    RegisterMessage.Text = $"Error: {ex.Message}";
                    RegisterMessage.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
