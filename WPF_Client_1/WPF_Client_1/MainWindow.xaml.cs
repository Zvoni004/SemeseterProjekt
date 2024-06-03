using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using System.Collections.Generic;

namespace WPF_Client_1
{
    public partial class MainWindow : Window
    {
        private ClientWebSocket _clientWebSocket;
        private string _username = null;
        private string _password = null;
        private bool _isConnected = false;
        private List<int> _sentMessageIds = new List<int>();

        public MainWindow()
        {
            InitializeComponent();
            ConnectWebSocket();
            StartPollingTasks();
        }

        private void StartPollingTasks()
        {
            _ = ReceiveMessages();
           // _ = ReceiveImages();
           // _ = ReceiveAudios();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            _username = UsernameInput.Text.Trim();
            _password = PasswordInput.Password;

            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
            {
                LoginMessage.Text = "Username and password cannot be empty";
                LoginMessage.Visibility = Visibility.Visible;
                return;
            }

            var user = new { Username = _username, Password = _password };
            var json = JsonSerializer.Serialize(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync("http://localhost:8080/login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        LoginPage.Visibility = Visibility.Collapsed;
                        ChatPage.Visibility = Visibility.Visible;

                        await ConnectWebSocket();
                        await SendMessageAsync(new { sender = _username, type = "JOIN" });
                    }
                    else
                    {
                        var responseMessage = await response.Content.ReadAsStringAsync();
                        LoginMessage.Text = $"Login failed: {responseMessage}";
                        LoginMessage.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    LoginMessage.Text = $"Error: {ex.Message}";
                    LoginMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private async Task ConnectWebSocket()
        {
            if (_clientWebSocket != null)
            {
                _clientWebSocket.Dispose();
            }

            _clientWebSocket = new ClientWebSocket();
            try
            {
                await _clientWebSocket.ConnectAsync(new Uri("ws://localhost:8080/chat"), CancellationToken.None);
                _isConnected = true;
                Console.WriteLine("WebSocket connected.");
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
        }

        private void OpenRegisterPage_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            registerPage.ShowDialog();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            string messageContent = MessageInput.Text.Trim();

            if (!string.IsNullOrEmpty(messageContent))
            {
                var chatMessage = new { message = messageContent, sender = _username };
                var json = JsonSerializer.Serialize(chatMessage);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var message = new { content = MessageInput.Text, sender = _username };


                using (var client = new HttpClient())
                {
                    try
                    {
                        var response = await client.PostAsync("http://localhost:8080/sendMessage", content);
                        var responseMessage = await response.Content.ReadAsStringAsync();
                        await SendMessageAsync(responseMessage);
                        MessageInput.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        //MessageArea.Items.Add($"Failed to send message: {ex.Message}");
                    }
                }
            }
        }

        private async void SendAudio_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var audioData = File.ReadAllBytes(filePath);
                var audioBase64 = Convert.ToBase64String(audioData);

                var audioMessage = new { audioBase64 = audioBase64, sender = _username };
                await SendAudioAsync(audioMessage);
            }
        }

        private async void SendImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var imageData = File.ReadAllBytes(filePath);
                var imageBase64 = Convert.ToBase64String(imageData);

                var imageMessage = new { imageBase64 = imageBase64, sender = _username };
                await SendImageAsync(imageMessage);
            }
        }

        private async Task SendMessageAsync(object message)
        {
            if (!_isConnected)
            {
                return;
            }

            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);

            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task SendAudioAsync(object audioMessage)
        {
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(audioMessage);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:8080/sendAudio", content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send audio: {responseMessage}");
                }
            }
        }

        private async Task SendImageAsync(object imageMessage)
        {
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(imageMessage);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:8080/sendImage", content);
                if (!response.IsSuccessStatusCode)
                {
                    var responseMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send image: {responseMessage}");
                }
            }
        }

        private async Task ReceiveMessages()
        {
            using (var client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await client.GetAsync("http://localhost:8080/topic/messages");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Received JSON: {json}");

                            var message = JsonSerializer.Deserialize<Message>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (message != null && !_sentMessageIds.Contains(message.Id))
                            {
                                _sentMessageIds.Add(message.Id);

                                Console.WriteLine($"Deserialized message: {message.Sender}, {message.MessageContent}");

                                Dispatcher.Invoke(() =>
                                {
                                    Console.WriteLine($"Processing message from {message.Sender}: {message.MessageContent}");

                                    var textBlock = new TextBlock
                                    {
                                        Text = $"{message.Sender}: {message.MessageContent}",
                                        Margin = new Thickness(5)
                                    };
                                    MessagesPanel.Children.Add(textBlock);
                                });
                            }
                            else
                            {
                                Console.WriteLine("Message has already been sent");
                            }
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error response: {error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving messages: {ex.Message}");
                    }

                    await Task.Delay(3000);
                }
            }
        }






        private async Task ReceiveImages()
        {
            using (var client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await client.GetAsync("http://localhost:8080/topic/images");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var images = JsonSerializer.Deserialize<List<ImageData>>(json);

                            Dispatcher.Invoke(() =>
                            {
                                foreach (var image in images)
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = new MemoryStream(Convert.FromBase64String(image.Data));
                                    bitmap.EndInit();

                                    var imageControl = new Image
                                    {
                                        Source = bitmap,
                                        Margin = new Thickness(5),
                                        Height = 100,
                                        Width = 100
                                    };
                                    MessagesPanel.Children.Add(imageControl);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving images: {ex.Message}");
                    }

                    await Task.Delay(3000);
                }
            }
        }


        private async Task ReceiveAudios()
        {
            using (var client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await client.GetAsync("http://localhost:8080/topic/audios");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var audios = JsonSerializer.Deserialize<List<AudioData>>(json);

                            Dispatcher.Invoke(() =>
                            {
                                foreach (var audio in audios)
                                {
                                    var button = new Button
                                    {
                                        Content = $"{audio.Sender}'s audio",
                                        Tag = audio.Data,
                                        Margin = new Thickness(5)
                                    };
                                    button.Click += PlayAudio_Click;
                                    MessagesPanel.Children.Add(button);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving audios: {ex.Message}");
                    }

                    await Task.Delay(3000);
                }
            }
        }


        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var audioData = Convert.FromBase64String(button.Tag.ToString());

            var tempFilePath = Path.GetTempFileName();
            File.WriteAllBytes(tempFilePath, audioData);

            var mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(tempFilePath));
            mediaPlayer.Play();
        }

    }
}
