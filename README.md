# ChatApp

## Inhaltsverzeichnis
1. [Softwaredesign (Architektur)](#softwaredesign-architektur)
2. [Beschreibung der Software](#beschreibung-der-software)
3. [API-Beschreibung](#api-beschreibung)
4. [Verwendung der API](#verwendung-der-api)
5. [Diagramme](#diagramme)
6. [Diskussion der Ergebnisse](#diskussion-der-ergebnisse)

## Softwar-Architektur
Die ChatApp basiert auf einem Spring Boot-Server mit einer MongoDB-Datenbank zur Speicherung von Nachrichten, Benutzern und Mediendaten (Images, Audios). Die Anwendung verwendet RESTful APIs für die Kommunikation und ermöglicht WebSocket-Verbindungen für Echtzeit-Messaging. Der Client ist eine WPF-Anwendung, welche die Benutzeroberfläche für die Interaktion mit dem Server bereitstellt.

```mermaid
graph TD;
    subgraph Server
        A[ChatAppServerApplication]
        A -->|Verarbeitet| B[ChatService]
        A -->|Verarbeitet| C[UserService]
        A -->|Verarbeitet| D[MessageServer]
        
        B -->|Verwendet| E[ChatMessageRepository]
        B -->|Verwendet| F[ChatMessage]
        
        C -->|Verwendet| G[UserRepository]
        C -->|Verwendet| H[User]
        
        D -->|Verarbeitet| I[AudioDataRepository]
        D -->|Verarbeitet| J[ImageDataRepository]
        D -->|Verarbeitet| K[SimpMessagingTemplate]
        D -->|Verarbeitet| L[ChatService]
        D -->|Verarbeitet| M[UserService]
        
        I -->|Speichert| N[AudioData]
        J -->|Speichert| O[ImageData]
    end

    subgraph MongoDB
        P[(MongoDB-Datenbank)]
    end

    subgraph WPF_Client
        Q[MainWindow]
        Q -->|Verarbeitet| R[ClientWebSocket]
        Q -->|Verarbeitet| S[HttpClient]
        Q -->|Verarbeitet| T[Message]
        Q -->|Verarbeitet| U[ImageData]
        Q -->|Verarbeitet| V[AudioData]
        Q -->|Verarbeitet| W[RegisterPage]
    end

    A -->|Speichert/Abruft Daten von| P
    Q -->|Kommuniziert mit| A

```

## Beschreibung der Software
Diese ChatApp Anwendung ermöglicht Benutzern Nachrichten in Echtzeit zu senden und zu empfangen. Die Hauptfunktionen umfassen das Senden von Textnachrichten, Bildern und Audiodateien sowie die Benutzerregistrierung und -anmeldung. Die Anwendung unterstützt Echtzeit-Kommunikation über WebSockets und speichert alle Nachrichten und Mediendaten in einer MongoDB-Datenbank. Ich habe mir vorgenommen, dass man auch Audios und Images empfangen kann nur konnte ich die entsprechenden Funktionen noch nicht fertig stellen aber das senden dieser Nachrichten Typen an den Server und die Speicherung in der MongoDB funktioniert.

Der WPF-Client bietet eine benutzerfreundliche Oberfläche für die Interaktion mit dem Server, einschließlich der Anzeige von Nachrichten, dem Senden von Textnachrichten, Bildern und Audiodateien sowie der Benutzerverwaltung. Wenn man den Client startet sieht man zuerst das Login Fenster mit der möglichkeit sich einzuloggen oder den Button für die Registrierung anzuklicken, erst wenn man sich angemeldet hat gelangt man in den eigentlichen Chat wo man die schon vorher genannten funktionen verwenden kann.

## API-Beschreibung
### Nachrichten
* **GET /api/messages**: Abrufen aller Nachrichten (alle "/api/..." wurden durch POSTMAN getestet)
* **POST /api/messages**: Erstellen einer neuen Nachricht
* **GET /api/messages/{id}**: Abrufen einer Nachricht nach ID
* **PUT /api/messages/{id}**: Aktualisieren einer Nachricht nach ID
* **DELETE /api/messages/{id}**: Löschen einer Nachricht nach ID
* **POST /sendMessage**: Senden einer Nachricht über WebSocket
* **GET /topic/messages**: Abrufen der letzten Nachricht über das Websocket Thema

### Bilder
* **POST /sendImage**: Senden eines Bildes

### Audio
* **POST /sendAudio**: Senden einer Audiodatei

### Benutzer
* **POST /register**: Benutzerregistrierung
* **POST /login**: Benutzeranmeldung

## Verwendung der API
### Nachrichten abrufen
```java
@GetMapping
    public List<ChatMessage> getAllMessages() {
        return chatService.getAllMessages();
    }
```
### Nachricht erstellen
```java
@PostMapping
    public ChatMessage createMessage(@RequestBody ChatMessage chatMessage) {
        return chatService.saveMessage(chatMessage);
    }
```
### Nachricht nach ID abrufen
```java
@GetMapping("/{id}")
    public Optional<ChatMessage> getMessageById(@PathVariable String id) {
        return chatService.getMessageById(Integer.parseInt(id));
    }
```
### Nachricht aktualisieren
```java
@PutMapping("/{id}")
    public ChatMessage updateMessage(@PathVariable String id, @RequestBody ChatMessage chatMessage) {
        return chatService.updateMessage(Integer.parseInt(id), chatMessage);
    }
```
### Nachricht löschen
```java
@DeleteMapping("/{id}")
    public boolean deleteMessage(@PathVariable String id) {
        return chatService.deleteMessage(Integer.parseInt(id));
    }
```
### Nachricht über WebSocket senden
```java
@PostMapping("/sendMessage")
    @SendTo("/topic/messages")
    public ResponseEntity<String> receiveMessage(@RequestBody ChatMessage message) {
        try {
            // Debugging logs
            System.out.println("Received message content: " + message.getMessage());
            System.out.println("Sender: " + message.getSender());
            // Set the new ID for the message
            ChatMessage lastMessage = chatService.getLastMessage();
            if (lastMessage != null) {
                message.setId(lastMessage.getId() + 1);
            } else {
                message.setId(1);
            }

            message.setTimestamp(new Date()); 


            chatService.saveMessage(message);
            messagingTemplate.convertAndSend("/topic/messages", message);
            return ResponseEntity.ok("Message received successfully!");
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body("Failed to send message: " + e.getMessage());
        }
    }
```
### Abrufen der letzten Nachricht über WebSocket-Thema
```java
@GetMapping("/topic/messages")
    public ResponseEntity<ChatMessage> getMessages() {
        try {
            ChatMessage message = chatService.getLastMessage();
            return ResponseEntity.ok(message);
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(null);
        }
    }
```

## Kommunikation des WPF-Client mit dem Server
### Beispielcode zum Senden einer Nachricht (C#)
```csharp
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
```

### Beispielcode zum Empfangen von Nachrichten (C#)
```csharp
private async Task ConnectWebSocketAsync()
{
    using (var client = new ClientWebSocket())
    {
        await client.ConnectAsync(new Uri("ws://localhost:8080/topic/messages"), CancellationToken.None);
        
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (result.MessageType != WebSocketMessageType.Close)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            // Process the message
            result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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

            await Task.Delay(3000);// test
        }
    }
}
```

## Diagramme
### Aktivitätsdiagramm WPF-Client
```mermaid
graph TD;
    Start[Start] --> LoginPage[LoginPage anzeigen];
    LoginPage --> UserInput[Eingabe von Benutzername und Passwort];
    UserInput --> LoginButton[Klicken auf den Anmeldebutton];
    LoginButton --> ValidateInputs[Benutzereingaben überprüfen];
    ValidateInputs --> |Fehler| ShowErrorMessage[Fehlermeldung anzeigen];
    ValidateInputs --> |Erfolg| HideLoginPage[LoginPage ausblenden];
    HideLoginPage --> ShowChatPage[Chatseite anzeigen];
    ShowChatPage --> ConnectWebSocket[WebSocket-Verbindung herstellen];
    ConnectWebSocket --> SendMessageAsync[Anfangsnachricht senden];
    SendMessageAsync --> ReceiveMessages[Nachrichten empfangen];
    SendMessageAsync --> ReceiveImages[Bilder empfangen];
    SendMessageAsync --> ReceiveAudios[Audios empfangen];
    ShowChatPage --> SendButton[Klicken auf den Senden-Button];
    SendButton --> SendMessageAsync[Nachricht senden];
    ShowChatPage --> SendImageButton[Klicken auf den Bilder senden-Button];
    SendImageButton --> SelectImage[Auswahl eines Bildes];
    SelectImage --> SendImageAsync[Bild senden];
    ShowChatPage --> SendAudioButton[Klicken auf den Audios senden-Button];
    SendAudioButton --> SelectAudio[Auswahl einer Audiodatei];
    SelectAudio --> SendAudioAsync[Audio senden];
    ShowChatPage --> CloseWindow[Schließen des Fensters];
    CloseWindow --> Terminate[Terminieren];
```
### Aktivitätsdiagramm Server
```mermaid
graph TD;
    Start((Start)) --> NachrichtEmpfangen[Nachricht empfangen];
    NachrichtEmpfangen --> NachrichtSpeichern[Nachricht speichern];
    NachrichtSpeichern --> NachrichtSenden[Nachricht senden];
    Start --> AudioEmpfangen[Audio empfangen];
    AudioEmpfangen --> AudioSpeichern[Audio speichern];
    AudioSpeichern --> AudioSenden[Audio senden];
    Start --> BildEmpfangen[Bild empfangen];
    BildEmpfangen --> BildSpeichern[Bild speichern];
    BildSpeichern --> BildSenden[Bild senden];
    Start --> BenutzerRegistrieren[Benutzer registrieren];
    BenutzerRegistrieren --> BenutzerSpeichern[Benutzer speichern];
    Start --> BenutzerAnmelden[Benutzer anmelden];
    BenutzerAnmelden --> BenutzerValidieren[Benutzer validieren];
    NachrichtSenden --> Ende((Ende));
    AudioSenden --> Ende;
    BildSenden --> Ende;
    BenutzerSpeichern --> Ende;
    BenutzerValidieren --> Ende;

```
### Use-Case-Diagramm
```mermaid
graph TD
    WPFClient -->|sendMessage| ChatAppServer
    WPFClient -->|sendImage| ChatAppServer
    WPFClient -->|sendAudio| ChatAppServer
    WPFClient -->|register| ChatAppServer
    WPFClient -->|login| ChatAppServer
```

### Klassen-Diagramm Server
```mermaid
classDiagram
    class AudioData {
        +String id
        +byte[] data
        +String sender
        +Date timestamp
        +getId()
        +setId(String id)
        +getData()
        +setData(byte[] data)
        +getSender()
        +setSender(String sender)
        +getTimestamp()
        +setTimestamp(Date timestamp)
    }

    class ChatMessage {
        +int id
        +String message
        +String sender
        +Date timestamp
        +getId()
        +setId(int id)
        +getMessage()
        +setMessage(String message)
        +getSender()
        +setSender(String sender)
        +getTimestamp()
        +setTimestamp(Date timestamp)
    }

    class ImageData {
        +String id
        +byte[] data
        +String sender
        +Date timestamp
        +getId()
        +setId(String id)
        +getData()
        +setData(byte[] data)
        +getSender()
        +setSender(String sender)
        +getTimestamp()
        +setTimestamp(Date timestamp)
    }

    class User {
        +String id
        +String username
        +String password
        +getId()
        +setId(String id)
        +getUsername()
        +setUsername(String username)
        +getPassword()
        +setPassword(String password)
    }

    class AudioDataRepository {
        <<interface>>
        +save(AudioData audioData)
        +findById(String id)
        +findAll()
        +deleteById(String id)
    }

    class ChatMessageRepository {
        <<interface>>
        +save(ChatMessage chatMessage)
        +findById(int id)
        +findAll()
        +deleteById(int id)
    }

    class ImageDataRepository {
        <<interface>>
        +save(ImageData imageData)
        +findById(String id)
        +findAll()
        +deleteById(String id)
    }

    class UserRepository {
        <<interface>>
        +save(User user)
        +findById(String id)
        +findAll()
        +deleteById(String id)
        +findByUsername(String username)
    }

    class ChatService {
        -ChatMessageRepository chatMessageRepository
        +getAllMessages()
        +saveMessage(ChatMessage chatMessage)
        +getMessageById(int id)
        +updateMessage(int id, ChatMessage chatMessage)
        +deleteMessage(int id)
        +getLastMessage()
    }

    class UserService {
        -UserRepository userRepository
        +saveUser(User user)
        +getUserByUsername(String username)
        +validateUser(String username, String password)
    }

    class ChatMessageController {
        -ChatService chatService
        +getAllMessages()
        +createMessage(ChatMessage chatMessage)
        +getMessageById(String id)
        +updateMessage(String id, ChatMessage chatMessage)
        +deleteMessage(String id)
    }

    class MessageServer {
        -ChatMessageRepository chatMessageRepository
        -AudioDataRepository audioDataRepository
        -ImageDataRepository imageDataRepository
        -UserRepository userRepository
        -UserService userService
        -ChatService chatService
        -SimpMessagingTemplate messagingTemplate
        +receiveMessage(ChatMessage message)
        +getMessages()
        +receiveAudio(AudioDataRequest audioDataRequest)
        +receiveImage(ImageDataRequest imageDataRequest)
        +registerUser(User user)
        +loginUser(Map<String, String> user)
    }

    AudioDataRepository <|-- AudioData
    ChatMessageRepository <|-- ChatMessage
    ImageDataRepository <|-- ImageData
    UserRepository <|-- User

    ChatService --> ChatMessageRepository
    UserService --> UserRepository

    ChatMessageController --> ChatService
    MessageServer --> ChatMessageRepository
    MessageServer --> AudioDataRepository
    MessageServer --> ImageDataRepository
    MessageServer --> UserRepository
    MessageServer --> UserService
    MessageServer --> ChatService
    MessageServer --> SimpMessagingTemplate

```
### Klassen-Diagramm WPF-Client
```mermaid
classDiagram
    class MainWindow {
        -ClientWebSocket _clientWebSocket
        -string _username
        -string _password
        -bool _isConnected
        -List<int> _sentMessageIds
        +MainWindow()
        +void Connect_Click(object sender, RoutedEventArgs e)
        +void OpenRegisterPage_Click(object sender, RoutedEventArgs e)
        +void Send_Click(object sender, RoutedEventArgs e)
        +void SendAudio_Click(object sender, RoutedEventArgs e)
        +void SendImage_Click(object sender, RoutedEventArgs e)
        +void PlayAudio_Click(object sender, RoutedEventArgs e)
        ~void StartPollingTasks()
        ~Task ConnectWebSocket()
        ~Task SendMessageAsync(object message)
        ~Task SendAudioAsync(object audioMessage)
        ~Task SendImageAsync(object imageMessage)
        ~Task ReceiveMessages()
        ~Task ReceiveImages()
        ~Task ReceiveAudios()
    }

    class RegisterPage {
        +RegisterPage()
        +void Register_Click(object sender, RoutedEventArgs e)
    }

    class Message {
        +string MessageContent
        +string Sender
        +DateTime Timestamp
        +int Id
    }

    class ImageData {
        +string Data
        +string Sender
    }

    class AudioData {
        +string Data
        +string Sender
    }

    MainWindow "1" --> "1" RegisterPage : Uses
    MainWindow "1" --> "1..*" Message : Manages
    MainWindow "1" --> "1..*" ImageData : Manages
    MainWindow "1" --> "1..*" AudioData : Manages

```
## Diskussion der Ergebnisse
### Zusammenfassung
Der ChatAppServer bietet eine robuste Plattform für Echtzeit-Kommunikation durch die Verwendung von Spring Boot und MongoDB. Die Integration von WebSockets ermöglicht eine schnelle und effiziente Nachrichtenübertragung, während die RESTful APIs eine einfache Interaktion mit dem Backend gewährleisten. Der WPF-Client bietet eine benutzerfreundliche Oberfläche für die Interaktion mit dem Server.


### Zukunfts-Ausblick
Zukünftige Arbeiten könnten die Fertigstellung der im Projekt-Antrag gesetzten Anforderungen beinhalten, darunter die Erstellung des WEB Clients und die Fertigstellung der Empfangsfunktionen für Images und Audios.
